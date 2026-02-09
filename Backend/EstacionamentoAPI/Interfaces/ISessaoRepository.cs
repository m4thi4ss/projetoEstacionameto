using EstacionamentoAPI.Models;
using EstacionamentoAPI.ViewModels;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Interfaces
{
    public interface ISessaoRepository
    {
        Task<IEnumerable<Sessao>> GetAllAsync();
        Task<PagedResult<Sessao>> GetPagedAsync(SessaoFiltros filtros);
        Task<Sessao?> GetByIdAsync(int id);
        Task<IEnumerable<Sessao>> GetSessoesAbertasAsync();
        Task<Sessao?> GetSessaoAbertaByVeiculoIdAsync(int veiculoId);
        Task<Sessao> CreateAsync(Sessao sessao);
        Task<Sessao?> RegistrarSaidaAsync(int sessaoId);
        Task<decimal> CalcularValorAsync(DateTime dataHoraEntrada, DateTime dataHoraSaida);
        Task<IEnumerable<VeiculoPatioViewModel>> GetVeiculosNoPatioAsync(string? placaFiltro = null);
    }
}
