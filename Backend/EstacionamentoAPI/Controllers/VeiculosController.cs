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
    public class VeiculosController : ControllerBase
    {
        private readonly IVeiculoRepository _repository;
        private readonly ILogger<VeiculosController> _logger;

        public VeiculosController(IVeiculoRepository repository, ILogger<VeiculosController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VeiculoViewModel>>> GetAll()
        {
            _logger.LogInformation("🚗 Listando todos os veículos");
            var veiculos = await _repository.GetAllAsync();
            var viewModels = veiculos.Select(v => new VeiculoViewModel
            {
                Id = v.Id,
                Placa = v.Placa,
                Modelo = v.Modelo,
                Cor = v.Cor,
                Tipo = v.Tipo.ToString(),
                DataCadastro = v.DataCadastro
            });

            return Ok(viewModels);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<VeiculoViewModel>>> GetPaged([FromQuery] VeiculoFiltros filtros)
        {
            try
            {
                _logger.LogInformation("📄 Buscando veículos paginados: Página={Page}, Tamanho={Size}", filtros.PageNumber, filtros.PageSize);
                
                var result = await _repository.GetPagedAsync(filtros);
                
                var viewModels = new PagedResult<VeiculoViewModel>
                {
                    Items = result.Items.Select(v => new VeiculoViewModel
                    {
                        Id = v.Id,
                        Placa = v.Placa,
                        Modelo = v.Modelo,
                        Cor = v.Cor,
                        Tipo = v.Tipo.ToString(),
                        DataCadastro = v.DataCadastro
                    }).ToList(),
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar veículos paginados");
                return StatusCode(500, new { message = "Erro ao buscar veículos", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VeiculoViewModel>> GetById(int id)
        {
            _logger.LogInformation("🔍 Buscando veículo por ID: {Id}", id);
            
            var veiculo = await _repository.GetByIdAsync(id);
            if (veiculo == null)
            {
                _logger.LogWarning("⚠️ Veículo não encontrado: ID={Id}", id);
                return NotFound(new { message = "Veículo não encontrado" });
            }

            var viewModel = new VeiculoViewModel
            {
                Id = veiculo.Id,
                Placa = veiculo.Placa,
                Modelo = veiculo.Modelo,
                Cor = veiculo.Cor,
                Tipo = veiculo.Tipo.ToString(),
                DataCadastro = veiculo.DataCadastro
            };

            return Ok(viewModel);
        }

        [HttpGet("placa/{placa}")]
        public async Task<ActionResult<VeiculoViewModel>> GetByPlaca(string placa)
        {
            _logger.LogInformation("🔍 Buscando veículo por placa: {Placa}", placa);
            
            var veiculo = await _repository.GetByPlacaAsync(placa);
            if (veiculo == null)
            {
                _logger.LogWarning("⚠️ Veículo não encontrado: Placa={Placa}", placa);
                return NotFound(new { message = "Veículo não encontrado" });
            }

            var viewModel = new VeiculoViewModel
            {
                Id = veiculo.Id,
                Placa = veiculo.Placa,
                Modelo = veiculo.Modelo,
                Cor = veiculo.Cor,
                Tipo = veiculo.Tipo.ToString(),
                DataCadastro = veiculo.DataCadastro
            };

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult<VeiculoViewModel>> Create([FromBody] VeiculoDTO dto)
        {
            try
            {
                _logger.LogInformation("📝 Criando novo veículo: {Placa}", dto.Placa);
                
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validar se placa já existe
                if (await _repository.PlacaExistsAsync(dto.Placa))
                {
                    _logger.LogWarning("⚠️ Tentativa de criar veículo com placa duplicada: {Placa}", dto.Placa);
                    return Conflict(new { message = "Já existe um veículo cadastrado com esta placa" });
                }

                var veiculo = new Veiculo
                {
                    Placa = dto.Placa.ToUpper().Trim(),
                    Modelo = dto.Modelo,
                    Cor = dto.Cor,
                    Tipo = dto.Tipo
                };

                var created = await _repository.CreateAsync(veiculo);

                _logger.LogInformation("✅ Veículo criado com sucesso: ID={Id}, Placa={Placa}", created.Id, created.Placa);

                var viewModel = new VeiculoViewModel
                {
                    Id = created.Id,
                    Placa = created.Placa,
                    Modelo = created.Modelo,
                    Cor = created.Cor,
                    Tipo = created.Tipo.ToString(),
                    DataCadastro = created.DataCadastro
                };

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao criar veículo: {Placa}", dto?.Placa ?? "N/A");
                return StatusCode(500, new { message = "Erro ao criar veículo", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VeiculoViewModel>> Update(int id, [FromBody] VeiculoUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("🔄 Atualizando veículo: ID={Id}", id);
                
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var veiculo = await _repository.GetByIdAsync(id);
                if (veiculo == null)
                {
                    _logger.LogWarning("⚠️ Tentativa de atualizar veículo inexistente: ID={Id}", id);
                    return NotFound(new { message = "Veículo não encontrado" });
                }

                if (dto.Modelo != null) veiculo.Modelo = dto.Modelo;
                if (dto.Cor != null) veiculo.Cor = dto.Cor;
                if (dto.Tipo.HasValue) veiculo.Tipo = dto.Tipo.Value;

                var updated = await _repository.UpdateAsync(id, veiculo);

                _logger.LogInformation("✅ Veículo atualizado: ID={Id}, Placa={Placa}", id, updated!.Placa);

                var viewModel = new VeiculoViewModel
                {
                    Id = updated!.Id,
                    Placa = updated.Placa,
                    Modelo = updated.Modelo,
                    Cor = updated.Cor,
                    Tipo = updated.Tipo.ToString(),
                    DataCadastro = updated.DataCadastro
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao atualizar veículo: ID={Id}", id);
                return StatusCode(500, new { message = "Erro ao atualizar veículo", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("🗑️ Excluindo veículo: ID={Id}", id);
                
                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("⚠️ Tentativa de excluir veículo inexistente: ID={Id}", id);
                    return NotFound(new { message = "Veículo não encontrado" });
                }

                _logger.LogInformation("✅ Veículo excluído: ID={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao excluir veículo: ID={Id}", id);
                return StatusCode(500, new { message = "Erro ao excluir veículo", error = ex.Message });
            }
        }
    }
}
