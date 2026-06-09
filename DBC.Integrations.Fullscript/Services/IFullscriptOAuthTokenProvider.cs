namespace DBC.Integrations.Fullscript.Services
{
    public interface IFullscriptOAuthTokenProvider
    {
        FullscriptOAuthTokenResult GetCurrentToken();

        void StoreToken(FullscriptOAuthTokenResult tokenResult);

        void ClearToken();
    }
}