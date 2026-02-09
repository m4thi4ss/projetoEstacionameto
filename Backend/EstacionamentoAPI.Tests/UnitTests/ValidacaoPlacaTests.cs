using Xunit;
using FluentAssertions;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace EstacionamentoAPI.Tests.UnitTests
{
    /// <summary>
    /// Testes unitários para validação de placa única
    /// Regra: Não pode haver 2 veículos com a mesma placa
    /// </summary>
    public class ValidacaoPlacaTests : IDisposable
    {
        private readonly EstacionamentoContext _context;
        private readonly VeiculoRepository _repository;

        public ValidacaoPlacaTests()
        {
            var options = new DbContextOptionsBuilder<EstacionamentoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EstacionamentoContext(options);
            _repository = new VeiculoRepository(_context);
        }

        [Fact]
        public async Task PlacaExists_PlacaNaoCadastrada_DeveRetornarFalse()
        {
            // Arrange
            var placa = "XYZ-9999";

            // Act
            var exists = await _repository.PlacaExistsAsync(placa);

            // Assert
            exists.Should().BeFalse("placa não cadastrada deve retornar false");
        }

        [Fact]
        public async Task PlacaExists_PlacaJaCadastrada_DeveRetornarTrue()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Placa = "ABC-1234",
                Modelo = "Honda Civic",
                Tipo = TipoVeiculo.Carro
            };
            await _repository.CreateAsync(veiculo);

            // Act
            var exists = await _repository.PlacaExistsAsync("ABC-1234");

            // Assert
            exists.Should().BeTrue("placa já cadastrada deve retornar true");
        }

        [Fact]
        public async Task PlacaExists_PlacaComCaseDiferente_DeveRetornarTrue()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Placa = "ABC-1234",
                Modelo = "Honda Civic",
                Tipo = TipoVeiculo.Carro
            };
            await _repository.CreateAsync(veiculo);

            // Act
            var existsLower = await _repository.PlacaExistsAsync("abc-1234");
            var existsUpper = await _repository.PlacaExistsAsync("ABC-1234");
            var existsMixed = await _repository.PlacaExistsAsync("AbC-1234");

            // Assert
            existsLower.Should().BeTrue("busca deve ser case-insensitive");
            existsUpper.Should().BeTrue("busca deve ser case-insensitive");
            existsMixed.Should().BeTrue("busca deve ser case-insensitive");
        }

        [Fact]
        public async Task CreateVeiculo_PlacaComEspacos_DeveNormalizarParaMaiusculaSemEspacos()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Placa = " abc-1234 ", // Com espaços e minúsculas
                Modelo = "Honda Civic",
                Tipo = TipoVeiculo.Carro
            };

            // Act
            var created = await _repository.CreateAsync(veiculo);

            // Assert
            created.Placa.Should().Be("ABC-1234", "placa deve ser normalizada (trim + upper)");
        }

        [Fact]
        public async Task GetByPlaca_PlacaExistente_DeveRetornarVeiculo()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Placa = "DEF-5678",
                Modelo = "Honda CG",
                Tipo = TipoVeiculo.Moto
            };
            await _repository.CreateAsync(veiculo);

            // Act
            var result = await _repository.GetByPlacaAsync("def-5678"); // Busca em minúscula

            // Assert
            result.Should().NotBeNull();
            result!.Placa.Should().Be("DEF-5678");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
