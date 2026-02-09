using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Interfaces
{
    public interface IConfiguracaoRepository
    {
        Task<decimal> GetPrecoPrimeiraHoraAsync();
        Task<decimal> GetPrecoDemaisHorasAsync();
        Task<Configuracao?> GetByChaveAsync(string chave);
        Task UpdateAsync(Configuracao configuracao);
    }
}
