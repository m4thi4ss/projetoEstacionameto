using Xunit;
using FluentAssertions;
using Moq;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace EstacionamentoAPI.Tests.UnitTests
{
    /// <summary>
    /// Testes unitários para o cálculo de valores de estacionamento
    /// Regra: Primeira hora R$ X, demais horas R$ Y (arredondamento para cima)
    /// </summary>
    public class CalculoValorTests
    {
        private readonly Mock<IConfiguracaoRepository> _mockConfiguracaoRepo;
        private readonly SessaoRepository _sessaoRepository;
        private readonly EstacionamentoContext _context;

        public CalculoValorTests()
        {
            // Configurar banco em memória
            var options = new DbContextOptionsBuilder<EstacionamentoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EstacionamentoContext(options);

            // Mock do repositório de configuração
            _mockConfiguracaoRepo = new Mock<IConfiguracaoRepository>();
            _mockConfiguracaoRepo.Setup(x => x.GetPrecoPrimeiraHoraAsync())
                .ReturnsAsync(5.00m); // R$ 5,00 primeira hora
            _mockConfiguracaoRepo.Setup(x => x.GetPrecoDemaisHorasAsync())
                .ReturnsAsync(3.00m); // R$ 3,00 demais horas

            _sessaoRepository = new SessaoRepository(_context, _mockConfiguracaoRepo.Object);
        }

        [Fact]
        public async Task CalcularValor_MenosDe1Hora_DeveRetornarPrecoPrimeiraHora()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 10, 45, 0); // 45 minutos

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            valor.Should().Be(5.00m, "menos de 1 hora deve cobrar apenas a primeira hora");
        }

        [Fact]
        public async Task CalcularValor_Exatamente1Hora_DeveRetornarPrecoPrimeiraHora()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 11, 0, 0); // Exatamente 1 hora

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            valor.Should().Be(5.00m, "exatamente 1 hora deve cobrar apenas a primeira hora");
        }

        [Fact]
        public async Task CalcularValor_1HoraE1Minuto_DeveArredondarPara2Horas()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 11, 1, 0); // 1h 1min

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            // 1ª hora: R$ 5,00 + 1 hora adicional: R$ 3,00 = R$ 8,00
            valor.Should().Be(8.00m, "qualquer fração além de 1h deve arredondar para hora completa");
        }

        [Fact]
        public async Task CalcularValor_2HorasExatas_DeveRetornarValorCorreto()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 12, 0, 0); // Exatamente 2 horas

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            // 1ª hora: R$ 5,00 + 1 hora adicional: R$ 3,00 = R$ 8,00
            valor.Should().Be(8.00m, "2 horas exatas = primeira hora + 1 hora adicional");
        }

        [Fact]
        public async Task CalcularValor_3HorasE30Minutos_DeveArredondarPara4Horas()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 13, 30, 0); // 3h 30min

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            // 1ª hora: R$ 5,00 + 3 horas adicionais: R$ 9,00 = R$ 14,00
            valor.Should().Be(14.00m, "3h 30min deve arredondar para 4 horas");
        }

        [Fact]
        public async Task CalcularValor_5HorasExatas_DeveRetornarValorCorreto()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 8, 0, 0);
            var saida = new DateTime(2026, 2, 7, 13, 0, 0); // Exatamente 5 horas

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            // 1ª hora: R$ 5,00 + 4 horas adicionais: R$ 12,00 = R$ 17,00
            valor.Should().Be(17.00m, "5 horas = primeira hora + 4 horas adicionais");
        }

        [Fact]
        public async Task CalcularValor_1Segundo_DeveArredondarPara1Hora()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 7, 10, 0, 1); // 1 segundo

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            valor.Should().Be(5.00m, "qualquer tempo, mesmo 1 segundo, cobra a primeira hora");
        }

        [Fact]
        public async Task CalcularValor_24Horas_DeveCalcularCorretamente()
        {
            // Arrange
            var entrada = new DateTime(2026, 2, 7, 10, 0, 0);
            var saida = new DateTime(2026, 2, 8, 10, 0, 0); // 24 horas exatas

            // Act
            var valor = await _sessaoRepository.CalcularValorAsync(entrada, saida);

            // Assert
            // 1ª hora: R$ 5,00 + 23 horas adicionais: R$ 69,00 = R$ 74,00
            valor.Should().Be(74.00m, "24 horas = primeira hora + 23 horas adicionais");
        }
    }
}
