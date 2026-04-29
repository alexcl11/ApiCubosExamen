using ApiCubosExamen.Data;
using ApiCubosExamen.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;

namespace ApiCubosExamen.Repositories
{
    public class RepositoryCubos
    {
        private ContextCubos context;
        public RepositoryCubos(ContextCubos context)
        {
            this.context = context;
        }
        public async Task<List<Cubo>> GetCubosAsync()
        {
            return await this.context.Cubos.ToListAsync();
        }
        public async Task<List<Cubo>> GetCubosMarcaAsync(string marca)
        {
            return await this.context.Cubos.Where(c => c.Marca == marca).ToListAsync();
        }
        public async Task CreateUserAsync(string nombre, string email, string password, string imagen)
        {
            int id = await this.context.Usuarios.MaxAsync(x => x.IdUsuario) + 1;
            Usuario user = new Usuario
            {
                IdUsuario = id,
                Nombre = nombre,
                Email = email,
                Password = password,
                Imagen = imagen
            };
            await this.context.Usuarios.AddAsync(user);
            await this.context.SaveChangesAsync();
        }
        public async Task<Usuario> FindUsuarioAsync(int id)
        {
            return await this.context.Usuarios.Where(x => x.IdUsuario == id).FirstOrDefaultAsync();
        }
        public async Task<List<CompraCubo>> GetPedidosUserAsync(int idUsuario)
        {
            return await this.context.Compras.Where(x => x.IdUsuario == idUsuario).ToListAsync();
        }
        public async Task RealizarCompraAsync(int idUsuario, int idCubo)
        {
            int idpedido = await this.context.Compras.MaxAsync(x => x.IdPedido) + 1;
            CompraCubo compra = new CompraCubo
            {
                IdPedido = idpedido,
                IdUsuario = idUsuario,
                IdCubo = idCubo,
                FechaPedido = DateTime.Now
            };
            await this.context.Compras.AddAsync(compra);
            await this.context.SaveChangesAsync();
        }
        public async Task<Usuario> LoginAsync(string email, string password)
        {
            return await this.context.Usuarios.Where(x => x.Email == email && x.Password == password).FirstOrDefaultAsync();
        }
    }
}
