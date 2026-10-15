using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;

namespace Infrastructure.Integrations.Services
{
    public class OAuthCompletionTokenService : IOAuthCompletionTokenService
    {
        private readonly IMemoryCache _cache;
        private const int ExpiryMinutes = 10;

        public OAuthCompletionTokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string Generate(string email, string provider)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "-").Replace("/", "_").Replace("=", "");

            _cache.Set(CacheKey(token), (email, provider), TimeSpan.FromMinutes(ExpiryMinutes));

            return token;
        }

        public (string email, string provider)? Validate(string token)
        {
            if (_cache.TryGetValue(CacheKey(token), out (string email, string provider) value))
                return value;

            return null;
        }

        public void Consume(string token) => _cache.Remove(CacheKey(token));

        private static string CacheKey(string token) => $"oauth_completion:{token}";
    }
}
