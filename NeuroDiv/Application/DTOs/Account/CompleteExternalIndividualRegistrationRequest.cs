namespace Application.DTOs.Account
{
    /// <summary>
    /// Called after OAuth when the user is new and chose "Individual".
    /// </summary>
    public class CompleteExternalIndividualRegistrationRequest
    {
        /// <summary>Proves the OAuth flow completed — from ExternalAuthResponse.</summary>
        public string OAuthCompletionToken { get; set; }
        public string Provider { get; set; }
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Called after OAuth when the user is new and chose "Organization".
    /// Creates the user account only — org is created separately.
    /// </summary>
    public class CompleteExternalOrgRegistrationRequest
    {
        public string OAuthCompletionToken { get; set; }
        public string Provider { get; set; }
        public string PhoneNumber { get; set; }
    }
}
