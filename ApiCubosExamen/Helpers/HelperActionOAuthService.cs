using ApiCubosExamen.Models;
using ApiCubosExamen.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace ApiCubosExamen.Helpers
{
    public class HelperActionOAuthService
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        private HttpContextAccessor contextAccessor;
        public HelperActionOAuthService(IConfiguration configuration, SecretClient secretClient)
        {
            this.Issuer = configuration.GetValue<string>
            ("ApiOAuthToken:Issuer");
            this.Audience = configuration.GetValue<string>
            ("ApiOAuthToken:Audience");
            KeyVaultSecret secretToken = secretClient.GetSecret("secret-token-key");
            this.SecretKey = secretToken.Value;
            this.contextAccessor = new HttpContextAccessor();
        }

        //NECESITAMOS UN METODO PARA GENERAR EL TOKEN A PARTIR 
        //DE NUESTRO SECRET KEY 
        public SymmetricSecurityKey GetKeyToken()
        {
            //CONVERTIMOS A BYTES NUESTRO SECRET KEY 
            byte[] data = Encoding.UTF8.GetBytes(this.SecretKey);
            return new SymmetricSecurityKey(data);
        }

        //UTILIZAMOS CLASES ACTION PARA SEPARAR LA CAPA  
        //DE LOS SERVICES DE AUTORIZACION DEL PROGRAM 
        public Action<JwtBearerOptions> GetJWtBearerOptions()
        {
            Action<JwtBearerOptions> options =
            new Action<JwtBearerOptions>(options =>
            {
                //INDICAMOS LO QUE SE VA A VALIDAR DENTRO DEL  
                //TOKEN PARA PERMITIR EL ACCESO 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = this.Issuer,
                    ValidAudience = this.Audience,
                    IssuerSigningKey = this.GetKeyToken()
                };
            });
            return options;
        }

        //EL ESQUEMA DE NUESTRA VALIDACION JwtBearerDefaults 
        public Action<AuthenticationOptions> GetAuthenticationSchema()
        {
            Action<AuthenticationOptions> options = new Action<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            return options;
        }

        public Usuario GetUsuario()
        {
            // 1. Buscamos el Claim "UserData" dentro del Token que nos ha enviado el MVC
            Claim claim = this.contextAccessor.HttpContext.User.FindFirst(z => z.Type == "UserData");

            string jsonCifrado = claim.Value;

            // 2. ¡AQUÍ SE DESENCRIPTA! Usando la clave AES que sacamos del Key Vault en Program.cs
            string jsonDescifrado = HelperCifrado.DescifrarString(jsonCifrado);

            // 3. Convertimos el JSON de texto plano a nuestro objeto C#
            Usuario model = JsonConvert.DeserializeObject<Usuario>(jsonDescifrado);

            return model;
        }

    }
}