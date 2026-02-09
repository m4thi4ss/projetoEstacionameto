using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.ViewModels;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Repositories
{
    public class SessaoRepository : ISessaoRepository
    {
        private readonly EstacionamentoContext _context;
        private readonly IConfiguracaoRepository _configuracaoRepository;

        public SessaoRepository(EstacionamentoContext context, IConfiguracaoRepository configuracaoRepository)
        {
            _context = context;
            _configuracaoRepository = configuracaoRepository;
        }

        public async Task<IEnumerable<Sessao>> GetAllAsync()
        {
            return await _context.Sessoes
                .Include(s => s.Veiculo)
                .OrderByDescending(s => s.DataHoraEntrada)
                .ToListAsync();
        }

        public async Task<PagedResult<Sessao>> GetPagedAsync(SessaoFiltros filtros)
        {
            var query = _context.Sessoes
                .Include(s => s.Veiculo)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtros.Placa))
            {
                query = query.Where(s => s.Veiculo.Placa.ToUpper().Contains(filtros.Placa.ToUpper()));
            }

            if (filtros.SessaoAberta.HasValue)
            {
                query = query.Where(s => s.SessaoAberta == filtros.SessaoAberta.Value);
            }

            if (filtros.DataInicio.HasValue)
            {
                query = query.Where(s => s.DataHoraEntrada >= filtros.DataInicio.Value);
            }

            if (filtros.DataFim.HasValue)
            {
                var dataFimFinal = filtros.DataFim.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(s => s.DataHoraEntrada <= dataFimFinal);
            }

            // Aplicar ordenação
            switch (filtros.OrderBy?.ToLower())
            {
                case "datasaida":
                    query = filtros.Descending
                        ? query.OrderByDescending(s => s.DataHoraSaida)
                        : query.OrderBy(s => s.DataHoraSaida);
                    break;
                case "valor":
                    // Para valor, não aplicamos ordenação aqui - faremos em memória após ToListAsync
                    break;
                case "placa":
                    query = filtros.Descending
                        ? query.OrderByDescending(s => s.Veiculo.Placa)
                        : query.OrderBy(s => s.Veiculo.Placa);
                    break;
                default:
                    query = filtros.Descending
                        ? query.OrderByDescending(s => s.DataHoraEntrada)
                        : query.OrderBy(s => s.DataHoraEntrada);
                    break;
            }

            // Contar total
            var totalCount = await query.CountAsync();

            // Aplicar paginação
            var items = await query
                .Skip((filtros.PageNumber - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            // Se ordenação é por valor, fazer em memória após carregar os dados
            if (filtros.OrderBy?.ToLower() == "valor")
            {
                items = filtros.Descending
                    ? items.OrderByDescending(s => s.ValorCobrado ?? 0).ToList()
                    : items.OrderBy(s => s.ValorCobrado ?? 0).ToList();
            }

            return new PagedResult<Sessao>
            {
                Items = items,
                PageNumber = filtros.PageNumber,
                PageSize = filtros.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Sessao?> GetByIdAsync(int id)
        {
            return await _context.Sessoes
                .Include(s => s.Veiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sessao>> GetSessoesAbertasAsync()
        {
            return await _context.Sessoes
                .Include(s => s.Veiculo)
                .Where(s => s.SessaoAberta)
                .OrderByDescending(s => s.DataHoraEntrada)
                .ToListAsync();
        }

        public async Task<Sessao?> GetSessaoAbertaByVeiculoIdAsync(int veiculoId)
        {
            return await _context.Sessoes
                .Include(s => s.Veiculo)
                .FirstOrDefaultAsync(s => s.VeiculoId == veiculoId && s.SessaoAberta);
        }

        public async Task<Sessao> CreateAsync(Sessao sessao)
        {
            _context.Sessoes.Add(sessao);
            await _context.SaveChangesAsync();
            return sessao;
        }

        public async Task<Sessao?> RegistrarSaidaAsync(int sessaoId)
        {
            var sessao = await GetByIdAsync(sessaoId);
            if (sessao == null || !sessao.SessaoAberta)
                return null;

            sessao.DataHoraSaida = DateTime.Now;
            sessao.ValorCobrado = await CalcularValorAsync(sessao.DataHoraEntrada, sessao.DataHoraSaida.Value);
            sessao.SessaoAberta = false;

            await _context.SaveChangesAsync();
            return sessao;
        }

        public async Task<decimal> CalcularValorAsync(DateTime dataHoraEntrada, DateTime dataHoraSaida)
        {
            var precoPrimeiraHora = await _configuracaoRepository.GetPrecoPrimeiraHoraAsync();
            var precoDemaisHoras = await _configuracaoRepository.GetPrecoDemaisHorasAsync();

            var tempoEstacionado = dataHoraSaida - dataHoraEntrada;
            
            // Arredondamento: qualquer fração de hora é cobrada como hora completa
            var totalHoras = Math.Ceiling(tempoEstacionado.TotalHours);

            if (totalHoras <= 1)
            {
                return precoPrimeiraHora;
            }
            else
            {
                var horasAdicionais = totalHoras - 1;
                return precoPrimeiraHora + (precoDemaisHoras * (decimal)horasAdicionais);
            }
        }

        public async Task<IEnumerable<VeiculoPatioViewModel>> GetVeiculosNoPatioAsync(string? placaFiltro = null)
        {
            var query = _context.Sessoes
                .Include(s => s.Veiculo)
                .Where(s => s.SessaoAberta);

            if (!string.IsNullOrWhiteSpace(placaFiltro))
            {
                query = query.Where(s => s.Veiculo.Placa.ToUpper().Contains(placaFiltro.ToUpper()));
            }

            var sessoes = await query.ToListAsync();
            var result = new List<VeiculoPatioViewModel>();

            foreach (var sessao in sessoes)
            {
                var tempoEstacionado = DateTime.Now - sessao.DataHoraEntrada;
                var valorAtual = await CalcularValorAsync(sessao.DataHoraEntrada, DateTime.Now);

                result.Add(new VeiculoPatioViewModel
                {
                    VeiculoId = sessao.VeiculoId,
                    SessaoId = sessao.Id,
                    Placa = sessao.Veiculo.Placa,
                    Modelo = sessao.Veiculo.Modelo,
                    Cor = sessao.Veiculo.Cor,
                    Tipo = sessao.Veiculo.Tipo.ToString(),
                    DataHoraEntrada = sessao.DataHoraEntrada,
                    TempoEstacionado = FormatarTempo(tempoEstacionado),
                    ValorAtual = valorAtual
                });
            }

            return result.OrderBy(v => v.Placa);
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
