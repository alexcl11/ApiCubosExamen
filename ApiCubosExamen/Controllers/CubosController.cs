using ApiCubosExamen.Models;
using ApiCubosExamen.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCubosExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CubosController : ControllerBase
    {
        private RepositoryCubos repo;
        private ServiceStorageBlobs service;
        public CubosController(RepositoryCubos repo, ServiceStorageBlobs service)
        {
            this.repo = repo;
            this.service = service;
        }


        [HttpGet]
        public async Task<ActionResult<List<Cubo>>> Get()
        {
            string urlImagen = this.service.GetContainerUrl("containercubos");
            List<Cubo> cubosSinUrl = await this.repo.GetCubosAsync();
            List<Cubo> cubosConFoto = cubosSinUrl.Select(cubo => new Cubo
            {
                IdCubo = cubo.IdCubo,
                Nombre = cubo.Nombre,
                Marca = cubo.Marca,
                Imagen = urlImagen + "/"+cubo.Imagen,
                Precio = cubo.Precio
            }).ToList();
            return cubosConFoto;
        }

        [HttpGet("{marca}")]
        [Route("[action]/{marca}")]
        public async Task<ActionResult<List<Cubo>>> CubosMarca(string marca)
        {
            string urlImagen = this.service.GetContainerUrl("containercubos");
            List<Cubo> cubosSinUrl = await this.repo.GetCubosMarcaAsync(marca);
            List<Cubo> cubosConFoto = cubosSinUrl.Select(cubo => new Cubo
            {
                IdCubo = cubo.IdCubo,
                Nombre = cubo.Nombre,
                Marca = cubo.Marca,
                Imagen = urlImagen + "/" + cubo.Imagen,
                Precio = cubo.Precio
            }).ToList();
            return cubosConFoto;
        }
    }
}
