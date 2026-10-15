namespace Application.DTOs.Organizations
{
    /// <summary>
    /// Creates an organization for an already-existing user.
    /// Used by users who registered via OAuth and chose "Organization".
    /// </summary>
    public class CreateOrganizationForExistingUserRequest
    {
        public string OrgName { get; set; }
        public string? OrgDescription { get; set; }
        public string? OrgPhoneNumber { get; set; }
        public string? OrgAddress { get; set; }
        public string? OrgCity { get; set; }
        public string? OrgCountry { get; set; }
        public string? OrgWebsite { get; set; }
        public string? VerifiedDomain { get; set; }
    }

}
