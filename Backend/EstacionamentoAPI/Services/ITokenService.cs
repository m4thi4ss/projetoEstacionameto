namespace EstacionamentoAPI.Services
{
    public interface ITokenService
    {
        string GerarToken(int userId, string email, string perfil);
    }
}
