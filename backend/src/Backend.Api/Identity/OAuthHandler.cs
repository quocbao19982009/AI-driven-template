namespace Backend.Identity;

// TODO: Implement OAuth provider integrations (Google, GitHub, etc.)
public class OAuthHandler
{
    private readonly IConfiguration _configuration;

    public OAuthHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }
}
