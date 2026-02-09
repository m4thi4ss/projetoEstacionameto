using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória")]
        [StringLength(10, ErrorMessage = "A placa deve ter no máximo 10 caracteres")]
        public string Placa { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres")]
        public string? Modelo { get; set; }

        [StringLength(50, ErrorMessage = "A cor deve ter no máximo 50 caracteres")]
        public string? Cor { get; set; }

        [Required(ErrorMessage = "O tipo é obrigatório")]
        public TipoVeiculo Tipo { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamento com Sessões
        public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }

    public enum TipoVeiculo
    {
        Carro = 1,
        Moto = 2,
        Caminhonete = 3,
        Van = 4
    }
}
