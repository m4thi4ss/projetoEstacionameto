using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Services;

namespace EstacionamentoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;

        public AuthController(IUsuarioRepository usuarioRepository, ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Senha))
                    return BadRequest(new { message = "Email e senha são obrigatórios" });

                var usuario = await _usuarioRepository.ValidarCredenciaisAsync(loginDto.Email, loginDto.Senha);

                if (usuario == null)
                    return Unauthorized(new { message = "Email ou senha inválidos" });

                var token = _tokenService.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString());

                var response = new LoginResponseDTO
                {
                    Token = token,
                    Usuario = new UsuarioDTO
                    {
                        Id = usuario.Id,
                        Nome = usuario.Nome,
                        Email = usuario.Email,
                        Perfil = usuario.Perfil.ToString(),
                        Ativo = usuario.Ativo
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao realizar login", error = ex.Message });
            }
        }
    }
}
