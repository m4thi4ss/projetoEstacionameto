using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.ViewModels;

namespace EstacionamentoAPI.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly EstacionamentoContext _context;

        public RelatorioRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FaturamentoDiarioViewModel>> GetFaturamentoDiarioAsync(int dias)
        {
            var dataInicio = DateTime.Now.Date.AddDays(-dias);

            // Buscar dados do banco primeiro
            var sessoes = await _context.Sessoes
                .Where(s => !s.SessaoAberta && s.DataHoraSaida != null && s.DataHoraSaida >= dataInicio)
                .ToListAsync();

            // Processar agrupamento em memória
            var faturamento = sessoes
                .GroupBy(s => s.DataHoraSaida!.Value.Date)
                .Select(g => new FaturamentoDiarioViewModel
                {
                    Data = g.Key,
                    ValorTotal = g.Sum(s => s.ValorCobrado ?? 0),
                    QuantidadeSaidas = g.Count()
                })
                .OrderBy(f => f.Data)
                .ToList();

            return faturamento;
        }

        public async Task<IEnumerable<VeiculoTempoEstacionadoViewModel>> GetTop10VeiculosPorTempoAsync(DateTime dataInicio, DateTime dataFim)
        {
            // Buscar sessões do banco de dados
            var sessoes = await _context.Sessoes
                .Include(s => s.Veiculo)
                .Where(s => !s.SessaoAberta && 
                           s.DataHoraSaida != null && 
                           s.DataHoraEntrada >= dataInicio && 
                           s.DataHoraSaida <= dataFim)
                .ToListAsync();

            // Processar em memória (compatível com SQLite)
            var veiculos = sessoes
                .GroupBy(s => new { s.VeiculoId, s.Veiculo.Placa, s.Veiculo.Modelo })
                .Select(g => new VeiculoTempoEstacionadoViewModel
                {
                    Placa = g.Key.Placa,
                    Modelo = g.Key.Modelo,
                    QuantidadeSessoes = g.Count(),
                    TotalHorasEstacionadas = g.Sum(s => 
                        (s.DataHoraSaida!.Value - s.DataHoraEntrada).TotalHours),
                    TempoFormatado = ""
                })
                .OrderByDescending(v => v.TotalHorasEstacionadas)
                .Take(10)
                .ToList();

            // Formatar tempo
            foreach (var veiculo in veiculos)
            {
                veiculo.TempoFormatado = FormatarHoras(veiculo.TotalHorasEstacionadas);
            }

            return veiculos;
        }

        public async Task<IEnumerable<OcupacaoPorHoraViewModel>> GetOcupacaoPorHoraAsync(DateTime dataInicio, DateTime dataFim)
        {
            // Não ajustar as datas - usar exatamente como vem do frontend
            // O frontend já envia 00:00:00 e 23:59:59
            
            var sessoes = await _context.Sessoes
                .Where(s => (s.DataHoraEntrada >= dataInicio && s.DataHoraEntrada <= dataFim) ||
                           (s.DataHoraSaida >= dataInicio && s.DataHoraSaida <= dataFim) ||
                           (s.DataHoraEntrada <= dataInicio && (s.DataHoraSaida >= dataFim || s.DataHoraSaida == null)))
                .ToListAsync();

            var ocupacaoPorHora = new List<OcupacaoPorHoraViewModel>();
            
            // Calcular dias entre as datas
            var diasDiferenca = (dataFim.Date - dataInicio.Date).Days + 1;
            var dataAtual = dataInicio.Date;

            for (int dia = 0; dia < diasDiferenca; dia++)
            {
                for (int hora = 0; hora < 24; hora++)
                {
                    var inicioHora = dataAtual.AddHours(hora);
                    var fimHora = inicioHora.AddHours(1);

                    // Conta veículos que estavam no pátio durante esta hora
                    var quantidade = sessoes.Count(s =>
                        s.DataHoraEntrada < fimHora &&
                        (s.DataHoraSaida == null || s.DataHoraSaida >= inicioHora)
                    );

                    ocupacaoPorHora.Add(new OcupacaoPorHoraViewModel
                    {
                        DataHora = inicioHora,
                        QuantidadeVeiculos = quantidade
                    });
                }
                dataAtual = dataAtual.AddDays(1);
            }

            return ocupacaoPorHora.OrderBy(o => o.DataHora);
        }

        private string FormatarHoras(double horas)
        {
            var dias = (int)(horas / 24);
            var horasRestantes = (int)(horas % 24);
            var minutos = (int)((horas - Math.Floor(horas)) * 60);

            if (dias > 0)
                return $"{dias}d {horasRestantes}h {minutos}m";
            else if (horasRestantes > 0)
                return $"{horasRestantes}h {minutos}m";
            else
                return $"{minutos}m";
        }
    }
}
