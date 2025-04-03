using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Servicios
{
    [TestClass]
    public class ServiciosUsuariosPruebas
    {
        private UserManager<Usuario> userManager = null!;
        private IHttpContextAccessor contextAccesor = null!;
        private ServiciosUsuarios servicioUsuarios = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);

            contextAccesor = Substitute.For<IHttpContextAccessor>();
            servicioUsuarios = new ServiciosUsuarios(userManager, contextAccesor);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarNulo_CuandoNoHayClaimEmail()
        {
            //Preparacion
            var httpContext = new DefaultHttpContext();
            contextAccesor.HttpContext.Returns(httpContext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarUsuario_CuandoHayClaimEmail()
        {
            //Preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(usuarioEsperado));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims};
            contextAccesor.HttpContext.Returns(httpContext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNotNull(usuario);
            Assert.AreEqual(expected: email, usuario.Email);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarNulo_CuandoUsuarioNoExiste()
        {
            //Preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<Usuario>(null!));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
            contextAccesor.HttpContext.Returns(httpContext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNull(usuario);
        }
    }
}
