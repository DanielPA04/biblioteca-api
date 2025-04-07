using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [Route("api/v1/llavesapi")]
    [Authorize]
    [ApiController]
    [DeshabilitarLimitarPeticiones]
    public class LlavesAPIController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicioLlaves servicioLlaves;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public LlavesAPIController(ApplicationDbContext context, IMapper mapper, IServicioLlaves servicioLlaves, IServiciosUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioLlaves = servicioLlaves;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]
        public async Task<IEnumerable<LlaveDTO>> Get()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var llaves = await context.LlavesAPI
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .Where(x => x.UsuarioId == usuarioId)
                .ToListAsync();
            return mapper.Map<IEnumerable<LlaveDTO>>(llaves);
        }

        [HttpGet("{id:int}", Name = "ObtenerLlaveV1")]
        public async Task<ActionResult<LlaveDTO>> Get(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return NotFound();
            }

            var llave = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if (llave is null)
            {
                return NotFound();
            }

            if (llave.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            return mapper.Map<LlaveDTO>(llave);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LlaveCreacionDTO llaveCreacionDTO)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId()!;

            if (llaveCreacionDTO.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                var elUsuarioYaTieneLlaveGratuita = await context.LlavesAPI
                    .AnyAsync(x => x.UsuarioId == usuarioId && x.TipoLLave == Entidades.TipoLlave.Gratuita);

                if (elUsuarioYaTieneLlaveGratuita)
                {
                    ModelState.AddModelError(nameof(llaveCreacionDTO.TipoLlave), "El usuario ya tiene una llave gratuita");
                    return ValidationProblem();
                }
            }

            var llaveAPI = await servicioLlaves.CrearLlave(usuarioId, llaveCreacionDTO.TipoLlave);
            var llaveDTO = mapper.Map<LlaveDTO>(llaveAPI);
            return CreatedAtRoute("ObtenerLlaveV1", new { id = llaveAPI.Id }, llaveDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LlaveActualizacionDTO llaveActualizacionDTO)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if (llaveDB is null)
            {
                return NotFound();
            }

            if (llaveDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            if (llaveActualizacionDTO.ActualizarLlave)
            {
                llaveDB.Llave = servicioLlaves.GenerarLlave();
            }

            llaveDB.Activa = llaveActualizacionDTO.Activa;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if (llaveDB is null)
            {
                return NotFound();
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (llaveDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            if (llaveDB.TipoLLave == Entidades.TipoLlave.Gratuita)
            {
                ModelState.AddModelError("", "No se puede eliminar una llave gratuita");
                return ValidationProblem();
            }

            context.Remove(llaveDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
