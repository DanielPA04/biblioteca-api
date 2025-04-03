using BibliotecaAPI.DTOs;
using BibliotecaAPITests.Utilidades;
using System.Net;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas:BasePruebas
    {
        private static readonly string url = "/api/v1/libros";
        private string nombreDB = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Devuelve400_CuandoAutoresIdsEsVacio()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB);
            var cliente = factory.CreateClient();
            var libroCreacionDTO = new LibroCreacionDTO()
            {
                Titulo = "Titulo"
            };
            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, libroCreacionDTO);

            //verificacion
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);

        }

    }
}
