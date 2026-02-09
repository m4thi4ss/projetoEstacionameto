using EstacionamentoAPI.Models;
using EstacionamentoAPI.ViewModels;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<IEnumerable<Veiculo>> GetAllAsync();
        Task<PagedResult<Veiculo>> GetPagedAsync(VeiculoFiltros filtros);
        Task<Veiculo?> GetByIdAsync(int id);
        Task<Veiculo?> GetByPlacaAsync(string placa);
        Task<Veiculo> CreateAsync(Veiculo veiculo);
        Task<Veiculo?> UpdateAsync(int id, Veiculo veiculo);
        Task<bool> DeleteAsync(int id);
        Task<bool> PlacaExistsAsync(string placa);
    }
}
