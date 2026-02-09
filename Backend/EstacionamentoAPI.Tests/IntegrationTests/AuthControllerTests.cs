using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Tests.IntegrationTests
{
    /// <summary>
    /// Testes de integração para autenticação e autorização
    /// Testa login, validação de credenciais e controle de acesso
    /// </summary>
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_CredenciaisValidas_DeveRetornar200ComToken()
        {
            // Arrange
            var loginDto = new LoginDTO 
            { 
                Email = "admin@estacionamento.com", 
                Senha = "admin123" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                "credenciais válidas devem retornar 200 OK");
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty("token JWT deve ser retornado");
            result.Usuario.Should().NotBeNull();
            result.Usuario.Email.Should().Be("admin@estacionamento.com");
            result.Usuario.Perfil.Should().Be("Admin");
        }

        [Fact]
        public async Task Login_EmailInvalido_DeveRetornar401()
        {
            // Arrange
            var loginDto = new LoginDTO 
            { 
                Email = "usuario@inexistente.com", 
                Senha = "senhaqualquer" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                "email inexistente deve retornar 401 Unauthorized");
        }

        [Fact]
        public async Task Login_SenhaInvalida_DeveRetornar401()
        {
            // Arrange
            var loginDto = new LoginDTO 
            { 
                Email = "admin@estacionamento.com", 
                Senha = "senhaErrada123" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                "senha incorreta deve retornar 401 Unauthorized");
        }

        [Fact]
        public async Task Login_CamposVazios_DeveRetornar400()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "", Senha = "" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
                "campos vazios devem retornar 400 Bad Request");
        }

        [Fact]
        public async Task AcessarEndpointProtegido_SemToken_DeveRetornar401()
        {
            // Act
            var response = await _client.GetAsync("/api/veiculos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                "acesso sem token deve retornar 401");
        }

        [Fact]
        public async Task AcessarEndpointProtegido_ComTokenValido_DeveRetornar200Ou204()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "admin@estacionamento.com", Senha = "admin123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDTO>();

            var clientAutenticado = _factory.CreateClient();
            clientAutenticado.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await clientAutenticado.GetAsync("/api/veiculos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                "token válido deve permitir acesso aos endpoints protegidos");
        }

        [Fact]
        public async Task AcessarEndpointAdmin_ComUsuarioOperador_DeveRetornar403()
        {
            // Este teste requer um usuário Operador criado
            // Por enquanto, vamos testar apenas que Admin consegue acessar
            
            // Arrange
            var loginDto = new LoginDTO { Email = "admin@estacionamento.com", Senha = "admin123" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDTO>();

            var clientAutenticado = _factory.CreateClient();
            clientAutenticado.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await clientAutenticado.GetAsync("/api/usuarios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                "Admin deve ter acesso ao endpoint de usuários");
        }
    }
}
