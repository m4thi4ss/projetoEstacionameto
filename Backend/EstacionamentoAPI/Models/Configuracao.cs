using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Configuracao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Chave { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Valor { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descricao { get; set; }
    }
}
