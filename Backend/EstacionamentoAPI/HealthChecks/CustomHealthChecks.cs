using Microsoft.Extensions.Diagnostics.HealthChecks;
using EstacionamentoAPI.Data;

namespace EstacionamentoAPI.HealthChecks
{
    /// <summary>
    /// Healthcheck customizado para verificar operações críticas do sistema
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly EstacionamentoContext _context;

        public DatabaseHealthCheck(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Tenta executar uma query simples
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy(
                        "Não foi possível conectar ao banco de dados SQLite"
                    );
                }

                // Verifica se há dados básicos
                var veiculosCount = await Task.Run(() => _context.Veiculos.Count(), cancellationToken);
                var configCount = await Task.Run(() => _context.Configuracoes.Count(), cancellationToken);

                var data = new Dictionary<string, object>
                {
                    { "database", "SQLite" },
                    { "veiculos_cadastrados", veiculosCount },
                    { "configuracoes_ativas", configCount },
                    { "timestamp", DateTime.UtcNow }
                };

                return HealthCheckResult.Healthy(
                    "Banco de dados operacional",
                    data
                );
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Erro ao verificar banco de dados",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Healthcheck para verificar configurações críticas
    /// </summary>
    public class ConfigurationHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                var issues = new List<string>();

                if (string.IsNullOrEmpty(jwtKey))
                    issues.Add("JWT Key não configurada");

                if (string.IsNullOrEmpty(connectionString))
                    issues.Add("Connection String não configurada");

                if (issues.Any())
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        "Configurações com problemas: " + string.Join(", ", issues)
                    ));
                }

                var data = new Dictionary<string, object>
                {
                    { "jwt_configured", !string.IsNullOrEmpty(jwtKey) },
                    { "database_configured", !string.IsNullOrEmpty(connectionString) }
                };

                return Task.FromResult(HealthCheckResult.Healthy(
                    "Todas as configurações críticas estão presentes",
                    data
                ));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Erro ao verificar configurações",
                    ex
                ));
            }
        }
    }
}
