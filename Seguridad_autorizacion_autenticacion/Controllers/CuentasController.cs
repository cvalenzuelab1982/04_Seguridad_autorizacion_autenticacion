using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Seguridad_autorizacion_autenticacion.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using System.Threading;
using Seguridad_autorizacion_autenticacion.Servicios;

namespace Seguridad_autorizacion_autenticacion.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly HashService _hashService;
        private readonly IDataProtector _dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager, IDataProtectionProvider dataProtectionProvider, HashService hashService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _hashService = hashService;
            //Crearemos parte de la llave
            _dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_secreto");
        }

        //obtener el Hash
        [HttpGet("hash/{textoPlano}")]
        public ActionResult RealizarHash(string textoPlano)
        {
            var resultado1 = _hashService.Hash(textoPlano);
            var resultado2 = _hashService.Hash(textoPlano);
            return Ok(new
            {
                textoPlano = textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2
            });
        }

        //encriptaremos un dato
        [HttpGet("encriptar")]
        public ActionResult Encriptar()
        {
            var textoPlano = "Carlos Valenzuela";
            var textoCifrado = _dataProtector.Protect(textoPlano);
            var textoDesencriptado = _dataProtector.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }

        //encriptaremos un dato por tiempo
        [HttpGet("encriptarPorTiempo")]
        public ActionResult EncriptarPorTiempo()
        {

            var protectorLimitadoPortiempo = _dataProtector.ToTimeLimitedDataProtector();

            var textoPlano = "Carlos Valenzuela";
            var textoCifrado = protectorLimitadoPortiempo.Protect(textoPlano, lifetime:TimeSpan.FromSeconds(5));
            var textoDesencriptado = protectorLimitadoPortiempo.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };

            var resultado = await _userManager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            } 
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            var resultado = await _signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Login incorrecto");
            }
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            //obtenemos el email del usuario
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            //Los Claim permite agregar algun datos referenciales del usuario esta sera acoplada al token
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
                new Claim("lo que yo quiera", "cualquier otro valor")
            };

            //obtener el  correo del usuario
            var usuario = await _userManager.FindByEmailAsync(credencialesUsuario.Email);
            //obtener todos los claims del usurio de la db
            var claimsDB = await _userManager.GetClaimsAsync(usuario);
            //fusionar el "claimsDB" con "claims" para construir el nuevos claims del token generado
            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1); //el token caduca en 1 año

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        //para darle rol de admin
        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAminDTO editarAminDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(editarAminDTO.Email);

            //Aqui se creara un Claim que se guardara en la tabla "AspNetUserClaims"
            await _userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        //para quitar el rol de admin
        [HttpPost("RemoverAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAminDTO editarAminDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(editarAminDTO.Email);

            await _userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }
    }
}
