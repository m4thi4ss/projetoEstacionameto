using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.ViewModels
{
    public class VeiculoViewModel
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
    }

    public class VeiculoPatioViewModel
    {
        public int VeiculoId { get; set; }
        public int SessaoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataHoraEntrada { get; set; }
        public string TempoEstacionado { get; set; } = string.Empty;
        public decimal ValorAtual { get; set; }
    }
}
