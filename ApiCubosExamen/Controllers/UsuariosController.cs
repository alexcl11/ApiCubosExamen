using ApiCubosExamen.Helpers;
using ApiCubosExamen.Models;
using ApiCubosExamen.Repositories;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiCubosExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController:ControllerBase
    {
        private RepositoryCubos repo;
        private HelperActionOAuthService helper;
        private ServiceStorageBlobs service;
        public UsuariosController(RepositoryCubos repo, HelperActionOAuthService helper, ServiceStorageBlobs service)
        {
            this.repo = repo;
            this.helper = helper;
            this.service = service;
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<Usuario>> Perfil()
        {
            Usuario user = this.helper.GetUsuario();
            Usuario perfil = await this.repo.FindUsuarioAsync(user.IdUsuario);

            // Sobrescribimos la propiedad Imagen con la URL temporal segura
            perfil.Imagen = this.service.GetBlobSasUrl("containerusuarios", perfil.Imagen);

            return perfil;
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<CompraCubo>>> ComprasUsuario()
        {
            Usuario user = this.helper.GetUsuario();

            return await this.repo.GetPedidosUserAsync(user.IdUsuario);
        }
        [Authorize]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> RealizarPedido(int idCubo)
        {
            Usuario user = this.helper.GetUsuario();
            await this.repo.RealizarCompraAsync(user.IdUsuario, idCubo);
            return Ok();
        }

        public class CreateUserModel
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public IFormFile Imagen { get; set; }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> CrearUsuario([FromForm] CreateUserModel model)
        {
            string nombreImagen = model.Imagen.FileName;
            using (Stream stream = model.Imagen.OpenReadStream())
            {
                await this.service.UploadBlobAsync
                ("containerusuarios", nombreImagen, stream);
            }

            await this.repo.CreateUserAsync(model.Nombre, model.Email, model.Password, nombreImagen);
            return Ok();
        }
    }
}
