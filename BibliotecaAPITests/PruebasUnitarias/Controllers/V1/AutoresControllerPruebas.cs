﻿using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;


namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AutoresController controller = null!;

        [TestInitialize]
        public void SetUp()
        {
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            almacenadorArchivos = Substitute.For<IAlmacenadorArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AutoresController(context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);
        }


        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            // Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            var libro1 = new Libro { Titulo = "Libro1" };
            var libro2 = new Libro { Titulo = "Libro2" };

            var autor = new Autor()
            {
                Nombres = "Felipe",
                Apellidos = "Gabilan",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro{Libro = libro1},
                    new AutorLibro{Libro = libro2}

                }
            };

            context.Add(autor);
            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Libros.Count);

        }

        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor() { Nombres = "Felipe", Apellidos = "Gavilán" });
            context.Autores.Add(new Autor() { Nombres = "Claudia", Apellidos = "Rodrígez" });
            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

        [TestMethod]
        public async Task Get_DebeLLamarGetDelServicioAutores()
        {
            // Preparacion           
            var paginacionDTO = new PaginacionDTO(2, 3);

            // Prueba
            await controller.Get(paginacionDTO);

            // Verificacion
            await servicioAutores.Received(1).Get(paginacionDTO);

        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            //Preparacion
            var context = ConstruirContext(nombreBD);
            var nuevoAutor = new AutorCreacionDTO() { Nombres = "Felipe", Apellidos = "Gavilán" };

            //Prueba
            var respuesta = await controller.Post(nuevoAutor);

            //Verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);


        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

        }

        private const string cache = "autores-obtener";
        private const string contenedor = "autores";

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorSinFoto()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor { Nombres = "Felipe", Apellidos = "Gavilan", Identificacion = "Id" });

            await context.SaveChangesAsync();

            var autorCreacionDTO = new AutorCreacionDTOConFoto { Nombres = "Felipe2", Apellidos = "Gavilan2", Identificacion = "Id2" };

            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBD);
            var autorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "Felipe2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Gavilan2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default, default!, default!);



        }

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorConFoto()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Editar(default, default!, default!).ReturnsForAnyArgs(urlNueva);

            context.Autores.Add(new Autor { Nombres = "Felipe", Apellidos = "Gavilan", Identificacion = "Id", Foto = urlAnterior });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();

            var autorCreacionDTO = new AutorCreacionDTOConFoto { Nombres = "Felipe2", Apellidos = "Gavilan2", Identificacion = "Id2", Foto = formFile };

            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBD);
            var autorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "Felipe2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Gavilan2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: autorActualizado.Foto);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Editar(urlAnterior, contenedor, formFile);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            //Prueba
            var respuesta = await controller.Patch(1, patchDoc: null!);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(400, resultado!.StatusCode);

        }

        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            // Preparacion
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Felipe",
                Apellidos = "Gavilan",
                Identificacion = "123"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeDeError = "mensaje de error";
            controller.ModelState.AddModelError("", mensajeDeError);

            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeDeError, actual: problemDetails.Errors.Values.First().First());

        }

        [TestMethod]
        public async Task Patch_ActualizaUnCampo_CuandoSeLeEnviaUnaOperacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Felipe",
                Apellidos = "Gavilan",
                Identificacion = "123",
                Foto = "URL-1"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

           
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AutorPatchDTO>("replace","/nombres",null,"Felipe2"));

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ConstruirContext(nombreBD);
            var autorDB = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Felipe2", autorDB.Nombres);
            Assert.AreEqual(expected: "Gavilan", autorDB.Apellidos);
            Assert.AreEqual(expected: "123", autorDB.Identificacion);
            Assert.AreEqual(expected: "URL-1", autorDB.Foto);

        }

        [TestMethod]
        public async Task Delete_Retorna404_CuandoAutorNoExiste()
        {
            //Prueba
            var respuesta = await controller.Delete(1);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_BorraAutor_CuandoAutorExiste()
        {
            // Preparacion
            var urlFoto = "URL-1";

            var context = ConstruirContext(nombreBD);

            context.Autores.Add(new Autor { Nombres = "Autor1", Apellidos = "Autor1", Foto = urlFoto});
            context.Autores.Add(new Autor { Nombres = "Autor2", Apellidos = "Autor2" });

            await context.SaveChangesAsync();
            //Prueba
            var respuesta = await controller.Delete(1);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            var context2 = ConstruirContext(nombreBD);
            var cantidadAutores = await context2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadAutores);

            var autor2Existe = await context2.Autores.AnyAsync(x => x.Nombres == "Autor2");
            Assert.IsTrue(autor2Existe);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Borrar(urlFoto, contenedor);
        }

    }
}
