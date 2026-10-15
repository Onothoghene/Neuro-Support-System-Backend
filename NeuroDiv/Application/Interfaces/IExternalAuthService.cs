using Application.DTOs.Account;
using Application.DTOs.Organizations;
using Application.Wrappers;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IExternalAuthService
    {
        /// <summary>
        /// Handles the OAuth callback after Google/Microsoft authentication.
        /// Returns existing user JWT if account found,
        /// or a completion token for new users to finish registration.
        /// </summary>
        Task<Response<ExternalAuthResponse>> HandleExternalLoginCallbackAsync(string provider);

        /// <summary>
        /// Completes registration for a new individual therapist who signed up via OAuth.
        /// </summary>
        Task<Response<AuthenticationResponse>> CompleteExternalIndividualRegistrationAsync(
                                               CompleteExternalIndividualRegistrationRequest request);

        /// <summary>
        /// Completes registration for a new org user who signed up via OAuth.
        /// Creates the user account only — org is created in a separate call.
        /// </summary>
        Task<Response<AuthenticationResponse>> CompleteExternalOrgRegistrationAsync(
                                              CompleteExternalOrgRegistrationRequest request);

        /// <summary>
        /// Creates an organization for an already-authenticated user.
        /// Used by both OAuth org users AND as a standalone endpoint
        /// for any authenticated user who needs to create an org.
        /// </summary>
        Task<Response<bool>> CreateOrganizationForExistingUserAsync(
                             CreateOrganizationForExistingUserRequest request);

    }
}
