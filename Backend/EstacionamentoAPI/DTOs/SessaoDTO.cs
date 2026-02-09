namespace EstacionamentoAPI.DTOs
{
    public class SessaoEntradaDTO
    {
        public string Placa { get; set; } = string.Empty;
    }

    public class SessaoSaidaDTO
    {
        public int SessaoId { get; set; }
    }
}
