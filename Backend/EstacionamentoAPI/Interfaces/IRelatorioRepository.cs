using EstacionamentoAPI.ViewModels;

namespace EstacionamentoAPI.Interfaces
{
    public interface IRelatorioRepository
    {
        Task<IEnumerable<FaturamentoDiarioViewModel>> GetFaturamentoDiarioAsync(int dias);
        Task<IEnumerable<VeiculoTempoEstacionadoViewModel>> GetTop10VeiculosPorTempoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<OcupacaoPorHoraViewModel>> GetOcupacaoPorHoraAsync(DateTime dataInicio, DateTime dataFim);
    }
}
