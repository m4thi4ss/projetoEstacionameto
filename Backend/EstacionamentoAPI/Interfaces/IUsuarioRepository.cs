using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario> CreateAsync(Usuario usuario);
        Task<Usuario?> UpdateAsync(int id, Usuario usuario);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
        Task<bool> AlterarSenhaAsync(int userId, string senhaAtual, string novaSenha);
    }
}
