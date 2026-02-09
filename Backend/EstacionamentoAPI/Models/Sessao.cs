using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Sessao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; } = null!;

        [Required]
        public DateTime DataHoraEntrada { get; set; } = DateTime.Now;

        public DateTime? DataHoraSaida { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorCobrado { get; set; }

        public bool SessaoAberta { get; set; } = true;

        // Propriedade calculada para tempo estacionado
        [NotMapped]
        public TimeSpan TempoEstacionado
        {
            get
            {
                var dataFim = DataHoraSaida ?? DateTime.Now;
                return dataFim - DataHoraEntrada;
            }
        }
    }
}
