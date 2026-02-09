using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly EstacionamentoContext _context;

        public VeiculoRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Veiculo>> GetAllAsync()
        {
            return await _context.Veiculos
                .OrderBy(v => v.Placa)
                .ToListAsync();
        }

        public async Task<PagedResult<Veiculo>> GetPagedAsync(VeiculoFiltros filtros)
        {
            var query = _context.Veiculos.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtros.Placa))
            {
                query = query.Where(v => v.Placa.ToUpper().Contains(filtros.Placa.ToUpper()));
            }

            if (filtros.Tipo.HasValue)
            {
                query = query.Where(v => (int)v.Tipo == filtros.Tipo.Value);
            }

            // Aplicar ordenação
            query = filtros.OrderBy?.ToLower() switch
            {
                "modelo" => filtros.Descending 
                    ? query.OrderByDescending(v => v.Modelo) 
                    : query.OrderBy(v => v.Modelo),
                "tipo" => filtros.Descending 
                    ? query.OrderByDescending(v => v.Tipo) 
                    : query.OrderBy(v => v.Tipo),
                "data" => filtros.Descending 
                    ? query.OrderByDescending(v => v.DataCadastro) 
                    : query.OrderBy(v => v.DataCadastro),
                _ => filtros.Descending 
                    ? query.OrderByDescending(v => v.Placa) 
                    : query.OrderBy(v => v.Placa)
            };

            // Contar total
            var totalCount = await query.CountAsync();

            // Aplicar paginação
            var items = await query
                .Skip((filtros.PageNumber - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return new PagedResult<Veiculo>
            {
                Items = items,
                PageNumber = filtros.PageNumber,
                PageSize = filtros.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Veiculo?> GetByIdAsync(int id)
        {
            return await _context.Veiculos
                .Include(v => v.Sessoes)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Veiculo?> GetByPlacaAsync(string placa)
        {
            return await _context.Veiculos
                .Include(v => v.Sessoes)
                .FirstOrDefaultAsync(v => v.Placa.ToUpper() == placa.ToUpper());
        }

        public async Task<Veiculo> CreateAsync(Veiculo veiculo)
        {
            veiculo.Placa = veiculo.Placa.ToUpper().Trim();
            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();
            return veiculo;
        }

        public async Task<Veiculo?> UpdateAsync(int id, Veiculo veiculo)
        {
            var existingVeiculo = await _context.Veiculos.FindAsync(id);
            if (existingVeiculo == null)
                return null;

            existingVeiculo.Modelo = veiculo.Modelo;
            existingVeiculo.Cor = veiculo.Cor;
            existingVeiculo.Tipo = veiculo.Tipo;

            await _context.SaveChangesAsync();
            return existingVeiculo;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return false;

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PlacaExistsAsync(string placa)
        {
            return await _context.Veiculos
                .AnyAsync(v => v.Placa.ToUpper() == placa.ToUpper());
        }
    }
}
