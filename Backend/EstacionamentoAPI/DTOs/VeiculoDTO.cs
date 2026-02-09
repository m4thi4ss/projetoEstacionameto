using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.DTOs
{
    public class VeiculoDTO
    {
        public string Placa { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public TipoVeiculo Tipo { get; set; }
    }

    public class VeiculoUpdateDTO
    {
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public TipoVeiculo? Tipo { get; set; }
    }
}
