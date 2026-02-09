using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.ViewModels;

namespace EstacionamentoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Protege todos os endpoints - requer autenticação
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioRepository _repository;

        public RelatoriosController(IRelatorioRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("faturamento-diario")]
        public async Task<ActionResult<IEnumerable<FaturamentoDiarioViewModel>>> GetFaturamentoDiario([FromQuery] int dias = 7)
        {
            try
            {
                if (dias <= 0 || dias > 365)
                    return BadRequest(new { message = "O número de dias deve estar entre 1 e 365" });

                var faturamento = await _repository.GetFaturamentoDiarioAsync(dias);
                return Ok(faturamento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar relatório de faturamento", error = ex.Message });
            }
        }

        [HttpGet("top-veiculos-tempo")]
        public async Task<ActionResult<IEnumerable<VeiculoTempoEstacionadoViewModel>>> GetTopVeiculosPorTempo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            try
            {
                if (dataInicio > dataFim)
                    return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });

                var veiculos = await _repository.GetTop10VeiculosPorTempoAsync(dataInicio, dataFim);
                return Ok(veiculos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar relatório de top veículos", error = ex.Message });
            }
        }

        [HttpGet("ocupacao-por-hora")]
        public async Task<ActionResult<IEnumerable<OcupacaoPorHoraViewModel>>> GetOcupacaoPorHora(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            try
            {
                if (dataInicio > dataFim)
                    return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });

                var ocupacao = await _repository.GetOcupacaoPorHoraAsync(dataInicio, dataFim);
                return Ok(ocupacao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar relatório de ocupação", error = ex.Message });
            }
        }
    }
}
