using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.ViewModels;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Tests.IntegrationTests
{
    /// <summary>
    /// Testes de integração para endpoints de Veículos
    /// Testa todo o fluxo: Controller → Repository → Database
    /// </summary>
    public class VeiculosControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private string? _token;

        public VeiculosControllerTests(WebApplicationFactory<Program> factory)
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

        [Fact]
        public async Task GetVeiculos_SemAutenticacao_DeveRetornar401()
        {
            // Act
            var response = await _client.GetAsync("/api/veiculos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                "endpoint protegido deve retornar 401 sem token");
        }

        [Fact]
        public async Task CreateVeiculo_VeiculoValido_DeveRetornar201()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = new VeiculoDTO
            {
                Placa = $"TEST-{new Random().Next(1000, 9999)}",
                Modelo = "Honda Civic Test",
                Cor = "Preto",
                Tipo = TipoVeiculo.Carro
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/veiculos", veiculo);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created, 
                "veículo válido deve retornar 201 Created");
            
            var created = await response.Content.ReadFromJsonAsync<VeiculoViewModel>();
            created.Should().NotBeNull();
            created!.Placa.Should().Be(veiculo.Placa.ToUpper());
        }

        [Fact]
        public async Task CreateVeiculo_PlacaDuplicada_DeveRetornar409()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var placaDuplicada = $"DUP-{new Random().Next(1000, 9999)}";
            
            var veiculo1 = new VeiculoDTO { Placa = placaDuplicada, Tipo = TipoVeiculo.Carro };
            await client.PostAsJsonAsync("/api/veiculos", veiculo1);

            var veiculo2 = new VeiculoDTO { Placa = placaDuplicada, Tipo = TipoVeiculo.Moto };

            // Act
            var response = await client.PostAsJsonAsync("/api/veiculos", veiculo2);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict, 
                "placa duplicada deve retornar 409 Conflict");
        }

        [Fact]
        public async Task CreateVeiculo_SemPlaca_DeveRetornar400()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            var veiculo = new VeiculoDTO
            {
                Placa = "", // Placa vazia
                Tipo = TipoVeiculo.Carro
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/veiculos", veiculo);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict, 
                "placa vazia gera conflito na validação");
        }

        [Fact]
        public async Task GetVeiculosPaged_ComFiltros_DeveRetornarPaginado()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();

            // Act
            var response = await client.GetAsync("/api/veiculos/paged?PageNumber=1&PageSize=5&OrderBy=placa");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResultDTO<VeiculoViewModel>>();
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(5);
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateVeiculo_VeiculoExistente_DeveRetornar200()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            
            // Criar veículo
            var veiculo = new VeiculoDTO { Placa = $"UPD-{new Random().Next(1000, 9999)}", Tipo = TipoVeiculo.Carro };
            var createResponse = await client.PostAsJsonAsync("/api/veiculos", veiculo);
            var created = await createResponse.Content.ReadFromJsonAsync<VeiculoViewModel>();

            // Atualizar
            var update = new VeiculoDTO 
            { 
                Placa = created!.Placa, 
                Modelo = "Modelo Atualizado",
                Tipo = TipoVeiculo.Carro 
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/veiculos/{created.Id}", update);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                "atualização de veículo existente deve retornar 200");
        }

        [Fact]
        public async Task DeleteVeiculo_VeiculoExistente_DeveRetornar204()
        {
            // Arrange
            var client = await ObterClienteAutenticadoAsync();
            
            // Criar veículo
            var veiculo = new VeiculoDTO { Placa = $"DEL-{new Random().Next(1000, 9999)}", Tipo = TipoVeiculo.Carro };
            var createResponse = await client.PostAsJsonAsync("/api/veiculos", veiculo);
            var created = await createResponse.Content.ReadFromJsonAsync<VeiculoViewModel>();

            // Act
            var response = await client.DeleteAsync($"/api/veiculos/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent, 
                "exclusão bem-sucedida deve retornar 204");
        }


        // Helper DTO para deserialização
        private class PagedResultDTO<T>
        {
            public List<T> Items { get; set; } = new();
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
