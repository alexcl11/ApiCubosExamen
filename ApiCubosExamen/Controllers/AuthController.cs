using ApiCubosExamen.Helpers;
using ApiCubosExamen.Models;
using ApiCubosExamen.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiCubosExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryCubos repo;
        private HelperActionOAuthService helper;

        public AuthController(RepositoryCubos repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> LogIn(LoginModel model)
        {
            Usuario usuario = await this.repo.LoginAsync(model.Email, model.Password);
            if (usuario == null)
            {
                return Unauthorized();
            }
            else
            {
                string jsonUsuario = JsonConvert.SerializeObject(usuario);
                string jsonCifrado = HelperCifrado.CifrarString(jsonUsuario);
                Claim[] claims = new[]
                {
                    new Claim("UserData", jsonCifrado)
                };

                SigningCredentials credentials = new SigningCredentials
                    (this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: this.helper.Issuer,
                    audience: this.helper.Audience,
                    signingCredentials: credentials,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(20),
                    notBefore: DateTime.UtcNow
                );
                return Ok(new
                {
                    response = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }
    }
}
