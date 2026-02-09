using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.ViewModels;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Tests.IntegrationTests
{
    /// <summary>
    /// Testes de integração para endpoints de Sessões
    /// Testa entrada, saída, cálculo de valor e regras de negócio
    /// </summary>
    public class SessoesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private string? _token;

        public SessoesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> ObterTokenAsync()
        {
            if (_token != null) return _token;

            var loginDto = new LoginDTO { Email = "admin@estacionamento.com", Senha = "admin123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                _token = result!.Token;
            }

            return _token!;
        }

        private async Task<HttpClient> ObterClienteAutenticadoAsync()
        {
            var token = await ObterTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return _client;
        }

        private async Task<VeiculoViewModel> CriarVeiculoTesteAsync()
        {
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = new VeiculoDTO
            {
                Placa = $"TST-{new Random().Next(1000, 9999)}",
                Modelo = "Veiculo Teste",
                Tipo = TipoVeiculo.Carro
            };

            var response = await client.PostAsJsonAsync("/api/veiculos", veiculo);
            return (await response.Content.ReadFromJsonAsync<VeiculoViewModel>())!;
        }

        [Fact]
        public async Task RegistrarEntrada_VeiculoValido_DeveRetornar201()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = await CriarVeiculoTesteAsync();

            var entrada = new SessaoEntradaDTO { Placa = veiculo.Placa };

            // Act
            var response = await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created, 
                "entrada válida deve retornar 201 Created");
            
            var sessao = await response.Content.ReadFromJsonAsync<SessaoViewModel>();
            sessao.Should().NotBeNull();
            sessao!.SessaoAberta.Should().BeTrue();
            sessao.PlacaVeiculo.Should().Be(veiculo.Placa);
        }

        [Fact]
        public async Task RegistrarEntrada_VeiculoJaNoPatrio_DeveRetornar400()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = await CriarVeiculoTesteAsync();

            // Primeira entrada
            var entrada = new SessaoEntradaDTO { Placa = veiculo.Placa };
            await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);

            // Act - Tentar entrar novamente
            var response = await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
                "veículo já no pátio não pode entrar novamente");
        }

        [Fact]
        public async Task RegistrarEntrada_VeiculoInexistente_DeveRetornar404()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var entrada = new SessaoEntradaDTO { Placa = "INEXISTENTE-9999" };

            // Act
            var response = await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, 
                "veículo não cadastrado deve retornar 404");
        }

        [Fact]
        public async Task CalcularSaida_SessaoAberta_DeveRetornarValorCalculado()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = await CriarVeiculoTesteAsync();

            // Registrar entrada
            var entrada = new SessaoEntradaDTO { Placa = veiculo.Placa };
            var entradaResponse = await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);
            var sessao = await entradaResponse.Content.ReadFromJsonAsync<SessaoViewModel>();

            var calcular = new SessaoSaidaDTO { SessaoId = sessao!.Id };

            // Act
            var response = await client.PostAsJsonAsync("/api/sessoes/saida/calcular", calcular);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var resultado = await response.Content.ReadFromJsonAsync<SessaoSaidaViewModel>();
            resultado.Should().NotBeNull();
            resultado!.ValorCobrado.Should().BeGreaterThan(0, "valor deve ser calculado");
            resultado.TempoEstacionado.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RegistrarSaida_SessaoAberta_DeveRetornar200EFecharSessao()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = await CriarVeiculoTesteAsync();

            // Registrar entrada
            var entrada = new SessaoEntradaDTO { Placa = veiculo.Placa };
            var entradaResponse = await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);
            var sessaoEntrada = await entradaResponse.Content.ReadFromJsonAsync<SessaoViewModel>();

            var saida = new SessaoSaidaDTO { SessaoId = sessaoEntrada!.Id };

            // Act
            var response = await client.PostAsJsonAsync("/api/sessoes/saida", saida);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var sessaoSaida = await response.Content.ReadFromJsonAsync<SessaoViewModel>();
            sessaoSaida.Should().NotBeNull();
            sessaoSaida!.SessaoAberta.Should().BeFalse("sessão deve ser fechada");
            sessaoSaida.DataHoraSaida.Should().NotBeNull();
            sessaoSaida.ValorCobrado.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetPatioAgora_ComVeiculosNoPatio_DeveRetornarLista()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = await CriarVeiculoTesteAsync();

            // Registrar entrada
            var entrada = new SessaoEntradaDTO { Placa = veiculo.Placa };
            await client.PostAsJsonAsync("/api/sessoes/entrada", entrada);

            // Act
            var response = await client.GetAsync("/api/sessoes/patio");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var patio = await response.Content.ReadFromJsonAsync<List<VeiculoPatioViewModel>>();
            patio.Should().NotBeNull();
            patio.Should().ContainSingle(v => v.Placa == veiculo.Placa, 
                "veículo recém-entrado deve estar no pátio");
        }

        [Fact]
        public async Task GetSessoesPaged_ComFiltros_DeveRetornarPaginado()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();

            // Act
            var response = await client.GetAsync(
                "/api/sessoes/paged?PageNumber=1&PageSize=10&SessaoAberta=true&OrderBy=dataEntrada&Descending=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResultDTO<SessaoViewModel>>();
            result.Should().NotBeNull();
            result!.Items.Should().AllSatisfy(s => s.SessaoAberta.Should().BeTrue(), 
                "filtro por sessão aberta deve retornar apenas sessões abertas");
        }

        // Helper DTO
        private class PagedResultDTO<T>
        {
            public List<T> Items { get; set; } = new();
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
        }
    }
}
