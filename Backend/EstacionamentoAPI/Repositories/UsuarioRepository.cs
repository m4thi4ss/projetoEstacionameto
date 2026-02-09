using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;
using BCrypt.Net;

namespace EstacionamentoAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EstacionamentoContext _context;

        public UsuarioRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            usuario.Email = usuario.Email.ToLower().Trim();
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);
            
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> UpdateAsync(int id, Usuario usuario)
        {
            var existingUsuario = await _context.Usuarios.FindAsync(id);
            if (existingUsuario == null)
                return null;

            existingUsuario.Nome = usuario.Nome;
            existingUsuario.Perfil = usuario.Perfil;
            existingUsuario.Ativo = usuario.Ativo;

            await _context.SaveChangesAsync();
            return existingUsuario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
        {
            var usuario = await GetByEmailAsync(email);
            
            if (usuario == null || !usuario.Ativo)
                return null;

            bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash);
            
            return senhaValida ? usuario : null;
        }

        public async Task<bool> AlterarSenhaAsync(int userId, string senhaAtual, string novaSenha)
        {
            var usuario = await GetByIdAsync(userId);
            if (usuario == null)
                return false;

            bool senhaAtualValida = BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.SenhaHash);
            if (!senhaAtualValida)
                return false;

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
