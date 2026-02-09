namespace EstacionamentoAPI.ViewModels
{
    public class FaturamentoDiarioViewModel
    {
        public DateTime Data { get; set; }
        public decimal ValorTotal { get; set; }
        public int QuantidadeSaidas { get; set; }
    }

    public class VeiculoTempoEstacionadoViewModel
    {
        public string Placa { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public int QuantidadeSessoes { get; set; }
        public double TotalHorasEstacionadas { get; set; }
        public string TempoFormatado { get; set; } = string.Empty;
    }

    public class OcupacaoPorHoraViewModel
    {
        public DateTime DataHora { get; set; }
        public int QuantidadeVeiculos { get; set; }
    }
}
