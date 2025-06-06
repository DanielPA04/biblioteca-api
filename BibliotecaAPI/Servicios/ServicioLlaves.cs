﻿using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Servicios
{
    public class ServicioLlaves : IServicioLlaves
    {
        private readonly ApplicationDbContext context;

        public ServicioLlaves(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<LlaveAPI> CrearLlave(string usuarioId, TipoLlave tipoLLave)
        {
            var llave = GenerarLlave();
            var llaveAPI = new LlaveAPI
            {
                Activa = true,
                Llave = llave,
                TipoLLave = tipoLLave,
                UsuarioId = usuarioId
            };

            context.Add(llaveAPI);

            await context.SaveChangesAsync();

            return llaveAPI;

        }

        public string GenerarLlave() => Guid.NewGuid().ToString().Replace("-", "");
    }
}
