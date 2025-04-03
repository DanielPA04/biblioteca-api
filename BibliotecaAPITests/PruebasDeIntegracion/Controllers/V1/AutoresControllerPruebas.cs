using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        private static readonly string url = "/api/v1/autores";
        private string nombreDB = Guid.NewGuid().ToString();


        [TestMethod]
        public async Task Get_Devuelve404_CuandoAutorNoExiste()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB);

            var cliente = factory.CreateClient();
            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //verificacion
            var statusCode = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: respuesta.StatusCode);

        }

        [TestMethod]
        public async Task Get_DevuelveAutor_CuandoAutorExiste()
        {
            //Preparacion
            var context = ConstruirContext(nombreDB);
            context.Autores.Add(new Autor() { Nombres = "Felipe", Apellidos = "Gavilán" });
            context.Autores.Add(new Autor() { Nombres = "Claudia", Apellidos = "Rodrígez" }); var factory = ConstruirWebApplicationFactory(nombreDB);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //verificacion
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions)!;

            Assert.AreEqual(expected: 1, autor.Id);

        }


        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);

            var cliente = factory.CreateClient();
            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };


            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve403_CuandoUsuarioNoEsAdmin()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);
            var token = await CrearUsuario(nombreDB, factory);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };


            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve201_CuandoUsuarioEsAdmin()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);

            var claims = new List<Claim> { adminClaim };

            var token = await CrearUsuario(nombreDB, factory, claims);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };


            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            respuesta.EnsureSuccessStatusCode();

            Assert.AreEqual(expected: HttpStatusCode.Created, actual: respuesta.StatusCode);
        }

    }
}
