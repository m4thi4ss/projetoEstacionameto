namespace EstacionamentoAPI.ViewModels
{
    public class SessaoViewModel
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public string PlacaVeiculo { get; set; } = string.Empty;
        public DateTime DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public decimal? ValorCobrado { get; set; }
        public bool SessaoAberta { get; set; }
        public string TempoEstacionado { get; set; } = string.Empty;
    }

    public class SessaoSaidaViewModel
    {
        public int SessaoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public DateTime DataHoraEntrada { get; set; }
        public DateTime DataHoraSaida { get; set; }
        public string TempoEstacionado { get; set; } = string.Empty;
        public decimal ValorCobrado { get; set; }
    }
}
