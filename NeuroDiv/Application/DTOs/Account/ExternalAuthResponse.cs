namespace Application.DTOs.Account
{
    /// <summary>
    /// Returned after OAuth completes.
    /// HasAccount = false → frontend shows account type choice screen.
    /// HasAccount = true  → frontend receives JWT and logs user in.
    /// </summary>
    public class ExternalAuthResponse
    {
        /// <summary>True if the user already has an account - JWT is populated.</summary>
        public bool HasAccount { get; set; }

        /// <summary>Populated when HasAccount = true.</summary>
        public AuthenticationResponse? AuthData { get; set; }

        /// <summary>
        /// Populated when HasAccount = false.
        /// Passed back to the complete-registration endpoint so we
        /// don't have to re-authenticate with the provider.
        /// </summary>
        public string? ProviderEmail { get; set; }
        public string? ProviderFirstName { get; set; }
        public string? ProviderLastName { get; set; }
        public string? ProviderPictureUrl { get; set; }
        public string? Provider { get; set; }

        /// <summary>
        /// Short-lived token that proves this OAuth flow completed successfully.
        /// Frontend passes it back when completing registration.
        /// Prevents someone from calling the complete-registration endpoint
        /// without going through OAuth first.
        /// </summary>
        public string? OAuthCompletionToken { get; set; }
    }
}
