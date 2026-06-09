using System;

namespace DBC.Integrations.Fullscript.Services
{
    public sealed class InMemoryFullscriptOAuthTokenProvider : IFullscriptOAuthTokenProvider
    {
        private readonly object _syncRoot = new object();
        private FullscriptOAuthTokenResult _currentToken;

        public FullscriptOAuthTokenResult GetCurrentToken()
        {
            lock (_syncRoot)
            {
                return CloneToken(_currentToken);
            }
        }

        public void StoreToken(FullscriptOAuthTokenResult tokenResult)
        {
            if (tokenResult == null)
            {
                throw new ArgumentNullException("tokenResult");
            }

            if (!tokenResult.Success || !tokenResult.HasAccessToken())
            {
                throw new ArgumentException("A successful Fullscript OAuth token with an access token is required.", "tokenResult");
            }

            lock (_syncRoot)
            {
                _currentToken = CloneToken(tokenResult);
            }
        }

        public void ClearToken()
        {
            lock (_syncRoot)
            {
                _currentToken = null;
            }
        }

        private static FullscriptOAuthTokenResult CloneToken(FullscriptOAuthTokenResult tokenResult)
        {
            if (tokenResult == null)
            {
                return null;
            }

            return new FullscriptOAuthTokenResult
            {
                Success = tokenResult.Success,
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                TokenType = tokenResult.TokenType,
                ExpiresIn = tokenResult.ExpiresIn,
                Scope = tokenResult.Scope,
                CreatedAt = tokenResult.CreatedAt,
                ResourceOwner = tokenResult.ResourceOwner,
                ErrorMessage = tokenResult.ErrorMessage,
                StatusCode = tokenResult.StatusCode,
                ReceivedAtUtc = tokenResult.ReceivedAtUtc
            };
        }
    }
}