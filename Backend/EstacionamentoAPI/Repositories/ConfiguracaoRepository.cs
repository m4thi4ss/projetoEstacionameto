using Microsoft.EntityFrameworkCore;
using System.Globalization;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories
{
    public class ConfiguracaoRepository : IConfiguracaoRepository
    {
        private readonly EstacionamentoContext _context;

        public ConfiguracaoRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetPrecoPrimeiraHoraAsync()
        {
            var config = await GetByChaveAsync("PrecoPrimeiraHora");
            return config != null ? decimal.Parse(config.Valor, CultureInfo.InvariantCulture) : 5.00m;
        }

        public async Task<decimal> GetPrecoDemaisHorasAsync()
        {
            var config = await GetByChaveAsync("PrecoDemaisHoras");
            return config != null ? decimal.Parse(config.Valor, CultureInfo.InvariantCulture) : 3.00m;
        }

        public async Task<Configuracao?> GetByChaveAsync(string chave)
        {
            return await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == chave);
        }

        public async Task UpdateAsync(Configuracao configuracao)
        {
            _context.Configuracoes.Update(configuracao);
            await _context.SaveChangesAsync();
        }
    }
}
