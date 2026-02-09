using Xunit;
using FluentAssertions;
using Moq;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstacionamentoAPI.Tests.UnitTests
{
    /// <summary>
    /// Testes unitários para validações de entrada/saída
    /// Regras:
    /// - Veículo não pode entrar se já estiver no pátio (sessão aberta)
    /// - Veículo não pode sair se não tiver sessão aberta
    /// </summary>
    public class ValidacaoSessaoTests : IDisposable
    {
        private readonly EstacionamentoContext _context;
        private readonly SessaoRepository _sessaoRepository;
        private readonly VeiculoRepository _veiculoRepository;
        private readonly Mock<IConfiguracaoRepository> _mockConfiguracaoRepo;

        public ValidacaoSessaoTests()
        {
            var options = new DbContextOptionsBuilder<EstacionamentoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EstacionamentoContext(options);

            _mockConfiguracaoRepo = new Mock<IConfiguracaoRepository>();
            _mockConfiguracaoRepo.Setup(x => x.GetPrecoPrimeiraHoraAsync()).ReturnsAsync(5.00m);
            _mockConfiguracaoRepo.Setup(x => x.GetPrecoDemaisHorasAsync()).ReturnsAsync(3.00m);

            _sessaoRepository = new SessaoRepository(_context, _mockConfiguracaoRepo.Object);
            _veiculoRepository = new VeiculoRepository(_context);
        }

        [Fact]
        public async Task GetSessaoAbertaByVeiculoId_VeiculoSemSessaoAberta_DeveRetornarNull()
        {
            // Arrange
            var veiculo = new Veiculo { Placa = "ABC-1234", Tipo = TipoVeiculo.Carro };
            var created = await _veiculoRepository.CreateAsync(veiculo);

            // Act
            var sessao = await _sessaoRepository.GetSessaoAbertaByVeiculoIdAsync(created.Id);

            // Assert
            sessao.Should().BeNull("veículo sem sessão aberta deve retornar null");
        }

        [Fact]
        public async Task GetSessaoAbertaByVeiculoId_VeiculoComSessaoAberta_DeveRetornarSessao()
        {
            // Arrange
            var veiculo = new Veiculo { Placa = "DEF-5678", Tipo = TipoVeiculo.Moto };
            var veiculoCreated = await _veiculoRepository.CreateAsync(veiculo);

            var sessao = new Sessao
            {
                VeiculoId = veiculoCreated.Id,
                DataHoraEntrada = DateTime.Now,
                SessaoAberta = true
            };
            await _sessaoRepository.CreateAsync(sessao);

            // Act
            var result = await _sessaoRepository.GetSessaoAbertaByVeiculoIdAsync(veiculoCreated.Id);

            // Assert
            result.Should().NotBeNull("veículo com sessão aberta deve retornar a sessão");
            result!.SessaoAberta.Should().BeTrue();
            result.VeiculoId.Should().Be(veiculoCreated.Id);
        }

        [Fact]
        public async Task GetSessaoAbertaByVeiculoId_VeiculoComSessaoFechada_DeveRetornarNull()
        {
            // Arrange
            var veiculo = new Veiculo { Placa = "GHI-9012", Tipo = TipoVeiculo.Caminhonete };
            var veiculoCreated = await _veiculoRepository.CreateAsync(veiculo);

            var sessao = new Sessao
            {
                VeiculoId = veiculoCreated.Id,
                DataHoraEntrada = DateTime.Now.AddHours(-2),
                DataHoraSaida = DateTime.Now,
                ValorCobrado = 8.00m,
                SessaoAberta = false // Sessão fechada
            };
            await _sessaoRepository.CreateAsync(sessao);

            // Act
            var result = await _sessaoRepository.GetSessaoAbertaByVeiculoIdAsync(veiculoCreated.Id);

            // Assert
            result.Should().BeNull("veículo com apenas sessão fechada deve retornar null");
        }

        [Fact]
        public async Task RegistrarSaida_SessaoAberta_DeveFecharSessaoECalcularValor()
        {
            // Arrange
            var veiculo = new Veiculo { Placa = "JKL-3456", Tipo = TipoVeiculo.Van };
            var veiculoCreated = await _veiculoRepository.CreateAsync(veiculo);

            var sessao = new Sessao
            {
                VeiculoId = veiculoCreated.Id,
                DataHoraEntrada = DateTime.Now.AddHours(-2), // 2 horas atrás
                SessaoAberta = true
            };
            var sessaoCreated = await _sessaoRepository.CreateAsync(sessao);

            // Act
            var result = await _sessaoRepository.RegistrarSaidaAsync(sessaoCreated.Id);

            // Assert
            result.Should().NotBeNull();
            result!.SessaoAberta.Should().BeFalse("sessão deve ser fechada");
            result.DataHoraSaida.Should().NotBeNull("data de saída deve ser preenchida");
            result.ValorCobrado.Should().BeGreaterThan(0, "valor deve ser calculado");
        }

        [Fact]
        public async Task RegistrarSaida_SessaoJaFechada_DeveRetornarNull()
        {
            // Arrange
            var veiculo = new Veiculo { Placa = "MNO-7890", Tipo = TipoVeiculo.Carro };
            var veiculoCreated = await _veiculoRepository.CreateAsync(veiculo);

            var sessao = new Sessao
            {
                VeiculoId = veiculoCreated.Id,
                DataHoraEntrada = DateTime.Now.AddHours(-3),
                DataHoraSaida = DateTime.Now.AddHours(-1),
                ValorCobrado = 8.00m,
                SessaoAberta = false // Já fechada
            };
            var sessaoCreated = await _sessaoRepository.CreateAsync(sessao);

            // Act
            var result = await _sessaoRepository.RegistrarSaidaAsync(sessaoCreated.Id);

            // Assert
            result.Should().BeNull("não pode registrar saída de sessão já fechada");
        }

        [Fact]
        public async Task RegistrarSaida_SessaoInexistente_DeveRetornarNull()
        {
            // Act
            var result = await _sessaoRepository.RegistrarSaidaAsync(99999);

            // Assert
            result.Should().BeNull("sessão inexistente deve retornar null");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
