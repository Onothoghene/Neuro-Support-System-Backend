using Application.Interfaces;
using Infrastructure.Integrations.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            // In-memory cache for OAuth completion tokens
            services.AddMemoryCache();
            services.AddSingleton<IOAuthCompletionTokenService, OAuthCompletionTokenService>();

            services.AddScoped<IExternalAuthService, ExternalAuthService>();

            //Google OAuth
            var googleClientId = configuration["Integrations:Google:ClientId"];
            var googleClientSecret = configuration["Integrations:Google:ClientSecret"];

            if (!string.IsNullOrWhiteSpace(googleClientId) &&
                !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.SaveTokens = true;

                    // Request profile + email scopes
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    // Map picture claim
                    options.ClaimActions.MapJsonKey("picture", "picture");
                });
            }

            //  Microsoft / Outlook OAuth 
            var msClientId = configuration["Integrations:Microsoft:ClientId"];
            var msClientSecret = configuration["Integrations:Microsoft:ClientSecret"];

            if (!string.IsNullOrWhiteSpace(msClientId) &&
                !string.IsNullOrWhiteSpace(msClientSecret))
            {
                services.AddAuthentication().AddMicrosoftAccount(options =>
                {
                    options.ClientId = msClientId;
                    options.ClientSecret = msClientSecret;
                    options.SaveTokens = true;
                });
            }
        }
    }
}
