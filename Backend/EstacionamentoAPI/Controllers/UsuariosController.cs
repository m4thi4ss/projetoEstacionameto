using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;
using System.Security.Claims;

namespace EstacionamentoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;

        public UsuariosController(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetAll()
        {
            try
            {
                var usuarios = await _repository.GetAllAsync();
                var usuariosDto = usuarios.Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = u.Perfil.ToString(),
                    Ativo = u.Ativo
                });

                return Ok(usuariosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar usuários", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UsuarioDTO>> Create([FromBody] CriarUsuarioDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _repository.EmailExistsAsync(dto.Email))
                    return Conflict(new { message = "Já existe um usuário com este email" });

                var usuario = new Usuario
                {
                    Nome = dto.Nome,
                    Email = dto.Email,
                    SenhaHash = dto.Senha, // Será hasheada no repository
                    Perfil = dto.Perfil,
                    Ativo = true
                };

                var created = await _repository.CreateAsync(usuario);

                var usuarioDto = new UsuarioDTO
                {
                    Id = created.Id,
                    Nome = created.Nome,
                    Email = created.Email,
                    Perfil = created.Perfil.ToString(),
                    Ativo = created.Ativo
                };

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, usuarioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar usuário", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UsuarioDTO>> GetById(int id)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { message = "Usuário não encontrado" });

                var usuarioDto = new UsuarioDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Perfil = usuario.Perfil.ToString(),
                    Ativo = usuario.Ativo
                };

                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar usuário", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UsuarioDTO>> Update(int id, [FromBody] UsuarioDTO dto)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { message = "Usuário não encontrado" });

                usuario.Nome = dto.Nome;
                usuario.Perfil = Enum.Parse<Role>(dto.Perfil);
                usuario.Ativo = dto.Ativo;

                var updated = await _repository.UpdateAsync(id, usuario);

                var usuarioDto = new UsuarioDTO
                {
                    Id = updated!.Id,
                    Nome = updated.Nome,
                    Email = updated.Email,
                    Perfil = updated.Perfil.ToString(),
                    Ativo = updated.Ativo
                };

                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar usuário", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Usuário não encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir usuário", error = ex.Message });
            }
        }

        [HttpPost("alterar-senha")]
        public async Task<ActionResult> AlterarSenha([FromBody] AlterarSenhaDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized();

                var sucesso = await _repository.AlterarSenhaAsync(userId, dto.SenhaAtual, dto.NovaSenha);

                if (!sucesso)
                    return BadRequest(new { message = "Senha atual incorreta" });

                return Ok(new { message = "Senha alterada com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao alterar senha", error = ex.Message });
            }
        }
    }
}
