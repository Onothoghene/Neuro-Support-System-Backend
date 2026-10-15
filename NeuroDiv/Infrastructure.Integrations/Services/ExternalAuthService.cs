using Application.DTOs.Account;
using Application.DTOs.Organizations;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Seeds;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Infrastructure.Integrations.Services
{
    public class ExternalAuthService(UserManager<ApplicationUser> userManager,
                                     SignInManager<ApplicationUser> signInManager,
                                     IUserProfileRepositoryAsync userProfile,
                                     IOrganizationsRepositoryAsync organizationsRepository,
                                     IOrganizationUsersRepositoryAsync orgUserRepository,
                                     IOrganizationRolesRepositoryAsync orgRolesRepository,
                                     IOrganizationUserRolesRepositoryAsync orgUserRolesRepository,
                                     IAuthenticatedUserService authenticatedUser,
                                     IOAuthCompletionTokenService completionTokenService,
                                     IAccountService accountService)
         : IExternalAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IUserProfileRepositoryAsync _userProfile = userProfile;
        private readonly IOrganizationsRepositoryAsync _organizationsRepository = organizationsRepository;
        private readonly IOrganizationUsersRepositoryAsync _orgUserRepository = orgUserRepository;
        private readonly IOrganizationRolesRepositoryAsync _orgRolesRepository = orgRolesRepository;
        private readonly IOrganizationUserRolesRepositoryAsync _orgUserRolesRepository = orgUserRolesRepository;
        private readonly IAuthenticatedUserService _authenticatedUser = authenticatedUser;
        private readonly IOAuthCompletionTokenService _completionTokenService = completionTokenService;
        private readonly IAccountService _accountService = accountService;

        public async Task<Response<ExternalAuthResponse>> HandleExternalLoginCallbackAsync(string provider)
        {
            // Get the login info from the OAuth provider
            var info = await _signInManager.GetExternalLoginInfoAsync() ??
                 throw new ApiException("External login information could not be retrieved. Please try again.");

            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ??
                throw new ApiException($"{provider} did not return an email address. " +
                                       $"Please ensure your account has a verified email.");

            var firstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;

            var lastName = info.Principal.FindFirst(
                System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty;

            var picture = info.Principal.FindFirst("picture")?.Value;

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                // ── Existing user — log them in ───────────────────────────
                // Ensure the external login is linked to their account
                var logins = await _userManager.GetLoginsAsync(existingUser);
                var alreadyLinked = logins.Any(l =>
                    l.LoginProvider == provider &&
                    l.ProviderKey == info.ProviderKey);

                if (!alreadyLinked)
                    await _userManager.AddLoginAsync(existingUser, info);

                // Use the existing AccountService to generate JWT
                var authResult = await _accountService.AuthenticateAsync(
                    new AuthenticationRequest
                    {
                        Email = email,
                        Password = string.Empty, // bypassed — OAuth verified identity
                    },
                    isExternal: true);  // flag to skip password check

                return new Response<ExternalAuthResponse>(new ExternalAuthResponse
                {
                    HasAccount = true,
                    AuthData = authResult.Data,
                }, "Login successful.");
            }
            else
            {
                // ── New user — generate completion token ──────────────────
                var completionToken = _completionTokenService.Generate(email, provider);

                return new Response<ExternalAuthResponse>(new ExternalAuthResponse
                {
                    HasAccount = false,
                    ProviderEmail = email,
                    ProviderFirstName = firstName,
                    ProviderLastName = lastName,
                    ProviderPictureUrl = picture,
                    Provider = provider,
                    OAuthCompletionToken = completionToken,
                }, "New user — please complete registration.");
            }
        }

        public async Task<Response<AuthenticationResponse>> CompleteExternalIndividualRegistrationAsync(CompleteExternalIndividualRegistrationRequest request)
        {
            // Validate the completion token
            var tokenData = _completionTokenService.Validate(request.OAuthCompletionToken) ??
                throw new ApiException($"This registration session has expired. Please sign in with {request.Provider} again to continue.");

            var (email, provider) = tokenData;

            using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Get the external login info again to link it
            var info = await _signInManager.GetExternalLoginInfoAsync() ??
                throw new ApiException("External login session expired. Please try again.");

            // Create Identity user — no password needed for OAuth users
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty,
                EmailConfirmed = true, // OAuth provider already verified the email
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new ApiException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            try
            {
                // Link the external login to the identity user
                await _userManager.AddLoginAsync(user, info);

                // Assign system role
                await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

                // Create UserProfile — no OTP needed, email already confirmed
                var userProfile = new UserProfile
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = email,
                    PhoneNumber = request.PhoneNumber,
                    // VerificationCode = 0 — no OTP needed
                };

                var profileResult = await _userProfile.AddAsync(userProfile);
                if (profileResult.Id == Guid.Empty)
                {
                    await _userManager.DeleteAsync(user);
                    throw new ApiException("Something went wrong while creating the user profile.");
                }

                // Consume the token — single use
                _completionTokenService.Consume(request.OAuthCompletionToken);

                ts.Complete();

                // Log them in and return JWT
                var authResult = await _accountService.AuthenticateAsync(
                    new AuthenticationRequest { Email = email, Password = string.Empty },
                    isExternal: true);

                return new Response<AuthenticationResponse>(authResult.Data, "Registration successful.");
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }
        }

        public async Task<Response<AuthenticationResponse>> CompleteExternalOrgRegistrationAsync(CompleteExternalOrgRegistrationRequest request)
        {
            // Validate the completion token
            var tokenData = _completionTokenService.Validate(request.OAuthCompletionToken) ??
                throw new ApiException($"This registration session has expired. Please sign in with {request.Provider} again to continue.");

            var (email, provider) = tokenData;

            using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var info = await _signInManager.GetExternalLoginInfoAsync() ??
                       throw new ApiException("External login session expired. Please try again.");

            // Create Identity user
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty,
                EmailConfirmed = true,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new ApiException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            try
            {
                await _userManager.AddLoginAsync(user, info);
                await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

                var userProfile = new UserProfile
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = email,
                    PhoneNumber = request.PhoneNumber,
                };

                var profileResult = await _userProfile.AddAsync(userProfile);
                if (profileResult.Id == Guid.Empty)
                {
                    await _userManager.DeleteAsync(user);
                    throw new ApiException("Something went wrong while creating the user profile.");
                }

                _completionTokenService.Consume(request.OAuthCompletionToken);

                ts.Complete();

                // Log them in — they'll hit CreateOrganizationForExistingUserAsync next
                var authResult = await _accountService.AuthenticateAsync(
                    new AuthenticationRequest { Email = email, Password = string.Empty },
                    isExternal: true);

                return new Response<AuthenticationResponse>(
                    authResult.Data,
                    "Account created. Please create your organization to continue.");
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }
        }

        public async Task<Response<bool>> CreateOrganizationForExistingUserAsync(CreateOrganizationForExistingUserRequest request)
        {
            using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var userProfileId = Guid.Parse(_authenticatedUser.UserId);

            if (userProfileId == Guid.Empty)
                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetByIdAsync(userProfileId) ??
                              throw new ApiException("User profile could not be found.");

            // Check they don't already own an org
            var existingMembership = await _orgUserRepository.GetAllActiveByUserIdAsync(userProfileId);

            var alreadyOwnsOrg = false;
            foreach (var membership in existingMembership)
            {
                var role = await _orgUserRolesRepository.GetUserRoleInOrgAsync(userProfileId, membership.OrganizationId);

                if (role?.OrganizationRoles?.Name == DefaultOrganizationRoles.ClinicOwner)
                {
                    alreadyOwnsOrg = true;
                    break;
                }
            }

            if (alreadyOwnsOrg)
                throw new ApiException("You already own an organization. A user can only be the Clinic Owner of one organization.");

            // Check domain isn't already claimed
            //if (!string.IsNullOrWhiteSpace(request.VerifiedDomain))
            //{
            //    var domainTaken = await _organizationsRepository
            //        .GetByDomainAsync(request.VerifiedDomain);

            //    if (domainTaken != null)
            //        throw new ApiException(
            //            $"The domain '{request.VerifiedDomain}' is already " +
            //            "associated with another organization.");
            //}

            // Create the organization
            var organization = new Organizations
            {
                Name = request.OrgName,
                Description = request.OrgDescription,
                PhoneNumber = request.OrgPhoneNumber,
                Address = request.OrgAddress,
                City = request.OrgCity,
                Country = request.OrgCountry,
                Website = request.OrgWebsite,
                Domain = request.VerifiedDomain,
            };

            var orgResult = await _organizationsRepository.AddAsync(organization);
            if (orgResult.Id == Guid.Empty)
                throw new ApiException("Something went wrong while creating the organization.");

            // Seed default org roles
            var roleEntities = DefaultOrganizationRoles.GetDefaults()
                .Select(r => new OrganizationRoles
                {
                    OrganizationId = orgResult.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsDefault = true,
                    CreatedBy = Guid.Empty,
                }).ToList();

            var seededRoles = await _orgRolesRepository.AddRangeAsync(roleEntities);

            // Assign Clinic Owner role to this user
            var clinicOwnerRole = seededRoles.First(r => r.Name == DefaultOrganizationRoles.ClinicOwner);

            // Create OrganizationUsers record
            var orgUser = new OrganizationUsers
            {
                UserId = userProfileId,
                OrganizationId = orgResult.Id,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
            };

            await _orgUserRepository.AddAsync(orgUser);

            // Assign the Clinic Owner org role
            var orgUserRole = new OrganizationUserRoles
            {
                UserId = userProfileId,
                OrganizationId = orgResult.Id,
                OrganizationRoleId = clinicOwnerRole.Id,
            };

            await _orgUserRolesRepository.AddAsync(orgUserRole);

            ts.Complete();

            return new Response<bool>(true, "Organization created successfully.");
        }
    }
}
