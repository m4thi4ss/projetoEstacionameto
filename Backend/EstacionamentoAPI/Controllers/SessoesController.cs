using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.ViewModels;

namespace EstacionamentoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessoesController : ControllerBase
    {
        private readonly ISessaoRepository _sessaoRepository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly ILogger<SessoesController> _logger;

        public SessoesController(
            ISessaoRepository sessaoRepository, 
            IVeiculoRepository veiculoRepository,
            ILogger<SessoesController> logger)
        {
            _sessaoRepository = sessaoRepository;
            _veiculoRepository = veiculoRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessaoViewModel>>> GetAll()
        {
            var sessoes = await _sessaoRepository.GetAllAsync();
            var viewModels = sessoes.Select(s => MapToViewModel(s));
            return Ok(viewModels);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<SessaoViewModel>>> GetPaged([FromQuery] SessaoFiltros filtros)
        {
            try
            {
                _logger.LogInformation("📄 Buscando sessões paginadas: Página={Page}, Tamanho={Size}", filtros.PageNumber, filtros.PageSize);
                
                var result = await _sessaoRepository.GetPagedAsync(filtros);
                
                var viewModels = new PagedResult<SessaoViewModel>
                {
                    Items = result.Items.Select(s => MapToViewModel(s)).ToList(),
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar sessões paginadas");
                return StatusCode(500, new { message = "Erro ao buscar sessões", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SessaoViewModel>> GetById(int id)
        {
            var sessao = await _sessaoRepository.GetByIdAsync(id);
            if (sessao == null)
                return NotFound(new { message = "Sessão não encontrada" });

            return Ok(MapToViewModel(sessao));
        }

        [HttpGet("patio")]
        public async Task<ActionResult<IEnumerable<VeiculoPatioViewModel>>> GetPatioAgora([FromQuery] string? placa = null)
        {
            _logger.LogInformation("🏢 Consultando pátio: {Filtro}", placa ?? "todos");
            var veiculos = await _sessaoRepository.GetVeiculosNoPatioAsync(placa);
            _logger.LogInformation("✅ {Count} veículo(s) no pátio", veiculos.Count());
            return Ok(veiculos);
        }

        [HttpPost("entrada")]
        public async Task<ActionResult<SessaoViewModel>> RegistrarEntrada([FromBody] SessaoEntradaDTO dto)
        {
            try
            {
                _logger.LogInformation("🚪 Registrando entrada: {Placa}", dto.Placa);
                
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Buscar veículo pela placa
                var veiculo = await _veiculoRepository.GetByPlacaAsync(dto.Placa);
                if (veiculo == null)
                {
                    _logger.LogWarning("⚠️ Entrada rejeitada - veículo não cadastrado: {Placa}", dto.Placa);
                    return NotFound(new { message = "Veículo não encontrado. Cadastre o veículo antes de registrar a entrada." });
                }

                // Verificar se já existe sessão aberta para este veículo
                var sessaoAberta = await _sessaoRepository.GetSessaoAbertaByVeiculoIdAsync(veiculo.Id);
                if (sessaoAberta != null)
                {
                    _logger.LogWarning("⚠️ Entrada rejeitada - veículo já está no pátio: {Placa}", dto.Placa);
                    return BadRequest(new { message = "Este veículo já está no pátio (possui sessão aberta)" });
                }

                var sessao = new Sessao
                {
                    VeiculoId = veiculo.Id,
                    DataHoraEntrada = DateTime.Now,
                    SessaoAberta = true
                };

                var created = await _sessaoRepository.CreateAsync(sessao);

                _logger.LogInformation("✅ Entrada registrada: Sessão={SessaoId}, Veículo={Placa}", created.Id, veiculo.Placa);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToViewModel(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao registrar entrada: {Placa}", dto?.Placa ?? "N/A");
                return StatusCode(500, new { message = "Erro ao registrar entrada", error = ex.Message });
            }
        }

        [HttpPost("saida/calcular")]
        public async Task<ActionResult<SessaoSaidaViewModel>> CalcularSaida([FromBody] SessaoSaidaDTO dto)
        {
            var sessao = await _sessaoRepository.GetByIdAsync(dto.SessaoId);
            if (sessao == null)
                return NotFound(new { message = "Sessão não encontrada" });

            if (!sessao.SessaoAberta)
                return BadRequest(new { message = "Esta sessão já foi encerrada" });

            var dataHoraSaida = DateTime.Now;
            var tempoEstacionado = dataHoraSaida - sessao.DataHoraEntrada;
            var valorCobrado = await _sessaoRepository.CalcularValorAsync(sessao.DataHoraEntrada, dataHoraSaida);

            var viewModel = new SessaoSaidaViewModel
            {
                SessaoId = sessao.Id,
                Placa = sessao.Veiculo.Placa,
                DataHoraEntrada = sessao.DataHoraEntrada,
                DataHoraSaida = dataHoraSaida,
                TempoEstacionado = FormatarTempo(tempoEstacionado),
                ValorCobrado = valorCobrado
            };

            return Ok(viewModel);
        }

        [HttpPost("saida")]
        public async Task<ActionResult<SessaoViewModel>> RegistrarSaida([FromBody] SessaoSaidaDTO dto)
        {
            try
            {
                _logger.LogInformation("🚪 Registrando saída: SessaoId={SessaoId}", dto.SessaoId);
                
                var sessao = await _sessaoRepository.GetByIdAsync(dto.SessaoId);
                if (sessao == null)
                {
                    _logger.LogWarning("⚠️ Saída rejeitada - sessão não encontrada: {SessaoId}", dto.SessaoId);
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                if (!sessao.SessaoAberta)
                {
                    _logger.LogWarning("⚠️ Saída rejeitada - sessão já encerrada: {SessaoId}", dto.SessaoId);
                    return BadRequest(new { message = "Esta sessão já foi encerrada" });
                }

                var updated = await _sessaoRepository.RegistrarSaidaAsync(dto.SessaoId);
                
                _logger.LogInformation("✅ Saída registrada: Sessão={SessaoId}, Placa={Placa}, Valor={Valor:C}", 
                    updated!.Id, updated.Veiculo.Placa, updated.ValorCobrado);
                
                return Ok(MapToViewModel(updated!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao registrar saída: SessaoId={SessaoId}", dto?.SessaoId ?? 0);
                return StatusCode(500, new { message = "Erro ao registrar saída", error = ex.Message });
            }
        }

        private SessaoViewModel MapToViewModel(Sessao sessao)
        {
            var tempoEstacionado = sessao.TempoEstacionado;

            return new SessaoViewModel
            {
                Id = sessao.Id,
                VeiculoId = sessao.VeiculoId,
                PlacaVeiculo = sessao.Veiculo?.Placa ?? "",
                DataHoraEntrada = sessao.DataHoraEntrada,
                DataHoraSaida = sessao.DataHoraSaida,
                ValorCobrado = sessao.ValorCobrado,
                SessaoAberta = sessao.SessaoAberta,
                TempoEstacionado = FormatarTempo(tempoEstacionado)
            };
        }

        private string FormatarTempo(TimeSpan tempo)
        {
            if (tempo.TotalDays >= 1)
                return $"{(int)tempo.TotalDays}d {tempo.Hours}h {tempo.Minutes}m";
            else if (tempo.TotalHours >= 1)
                return $"{(int)tempo.TotalHours}h {tempo.Minutes}m";
            else
                return $"{tempo.Minutes}m {tempo.Seconds}s";
        }
    }
}
