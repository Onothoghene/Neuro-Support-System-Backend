using Application.DTOs.Account;
using Application.DTOs.Email;
using Application.DTOs.OrganizationUsers;
using Application.DTOs.OrganizationUsersInvite;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Wrappers;
using Domain.Seeds;
using Domain.Entities;
using Domain.Settings;
using Infrastructure.Identity.Context;
using Infrastructure.Identity.Helpers;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly IdentityContext _context;
        private readonly IUserProfileRepositoryAsync _userProfile;
        private readonly ApplicationUrl _applicationUrlSettings;
        private readonly IOrganizationUsersInviteRepositoryAsync _inviteRepository;
        private readonly IOrganizationUsersRepositoryAsync _orgUserRepository;
        private readonly IAuthenticatedUserService _authenticatedUser;
        private readonly IOrganizationsRepositoryAsync _organizationsRepository;
        private readonly IOrganizationRolesRepositoryAsync _organizationRolesRepository;
        private readonly IOrganizationUserRolesRepositoryAsync _organizationUserRolesRepository;
        private readonly IEmailChangeRequestRepositoryAsync _emailChangeRequestRepository;

        public AccountService(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IOptions<JWTSettings> jwtSettings,
                              IDateTimeService dateTimeService,
                              SignInManager<ApplicationUser> signInManager,
                              IEmailService emailService,
                              IdentityContext context,
                              IUserProfileRepositoryAsync userProfile,
                              IOptionsSnapshot<ApplicationUrl> applicationUrlSettings,
                              IOrganizationUsersInviteRepositoryAsync inviteRepository,
                              IOrganizationUsersRepositoryAsync orgUserRepository,
                              IAuthenticatedUserService authenticatedUser,
                              IOrganizationsRepositoryAsync organizationsRepository,
                              IOrganizationRolesRepositoryAsync organizationRolesRepository,
                              IOrganizationUserRolesRepositoryAsync organizationUserRolesRepository,
                              IEmailChangeRequestRepositoryAsync emailChangeRequestRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
            _userProfile = userProfile;
            _applicationUrlSettings = applicationUrlSettings.Value;
            _inviteRepository = inviteRepository;
            _orgUserRepository = orgUserRepository;
            _authenticatedUser = authenticatedUser;
            _organizationsRepository = organizationsRepository;
            _organizationRolesRepository = organizationRolesRepository;
            _organizationUserRolesRepository = organizationUserRolesRepository;
            _emailChangeRequestRepository = emailChangeRequestRepository;
        }

        // ── Registration Method for freelanncers/independent users
        public async Task<Response<string>> RegisterAsync(RegisterRequest request)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.Email);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Email '{request.Email}' is already taken.");
            }
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);

            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    // Assign system-level Identity role
                    await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

                    var otp = GenerateOTP();

                    //use the request to create a new user profile
                    var userProfile = new UserProfile
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        OtherName = request.OtherName,
                        Email = request.Email,
                        VerificationCode = otp,
                        PhoneNumber = request.PhoneNumber,
                    };

                    var profileResult = await _userProfile.AddAsync(userProfile);

                    if (profileResult.Id != Guid.Empty)
                    {
                        var templatePath = "EmailTemplate/ConfirmEmail.cshtml";

                        await _emailService.SendFluentEmailTemplate(new EmailRequest()
                        {
                            To = user.Email,
                            Body = $"",
                            Subject = "Confirm Registration",
                            Otp = otp,
                            FirstName = $"{request.FirstName}",
                            LastName = $"{request.LastName}",
                            Url = $"{_applicationUrlSettings.ConfirmEmailUrl}{request.Email}?code={otp}",
                        }, templatePath);

                        return new Response<string>("Check your mail for your OTP to verify your email",
                            message: $"User registered successfully.");
                    }
                    else
                    {
                        await _userManager.DeleteAsync(user);
                        throw new ApiException("Something went wrong while profiling user");
                    }
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    throw new ApiException($"{result.Errors}");
                }
            }
            else
            {
                throw new ApiException($"Email {request.Email} is already registered.");
            }
        }

        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, bool isOffline = false)
        {
            var user = await _userManager.FindByEmailAsync(request.Email) ??
                             throw new ApiException($"No Registered Account with {request.Email}.");

            if (!user.EmailConfirmed)
                throw new ApiException($"Account Not Confirmed for '{request.Email}'.");

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");
            

            var userProfile = await _userProfile.GetUserByEmailAsync(user.Email) ?? 
                              throw new ApiException($"User profile not found for '{request.Email}'.");

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user, isOffline);
            AuthenticationResponse response = new AuthenticationResponse
            {
                // IdentityId = user.Id,
                UserId = userProfile.Id.ToString(),
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            // fetch all org memberships for this user
            var orgMemberships = await _orgUserRepository.GetAllActiveByUserIdAsync(userProfile.Id);

            response.Organizations = orgMemberships.Select(m => new UserOrganizationVM
            {
                OrganizationId = m.OrganizationId,
                OrganizationName = m.Organizations.Name,
                Role = m.User.OrganizationUserRoles.FirstOrDefault(r => r.OrganizationId == m.OrganizationId)?
                                                   .OrganizationRoles?.Name ?? "No Role Assigned",
                IsActive = m.IsActive
            }).ToList();


            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            response.TokenExpires = jwtSecurityToken.ValidTo;

            if (user.RefreshTokens != null && user.RefreshTokens.Any(a => a.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                response.RefreshToken = activeRefreshToken.Token;
                response.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                response.RefreshToken = refreshToken.Token;
                response.RefreshTokenExpiration = refreshToken.Expires;
                user.RefreshTokens.Add(refreshToken);
                _context.Update(user);
                _context.SaveChanges();
            }

            //Update Last Login Date from the user profile table
            userProfile.IsLoggedIn = true;
            userProfile.LastDateLoggedIn = DateTime.Now;

            await _userProfile.UpdateAsync(userProfile);

            return new Response<AuthenticationResponse>(response, $"User {user.UserName} Authenticated");
        }

        // ── Logout from ALL devices
        public async Task<Response<bool>> LogOutAsync()
        {
            var userProfileId = Guid.Parse(_authenticatedUser.UserId);
            if (userProfileId == Guid.Empty)
                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(userProfileId);
            if (userProfile == null)
                throw new ApiException("User profile could not be found.");

            var user = await _userManager.FindByEmailAsync(userProfile.Email);
            if (user == null)
                throw new ApiException("No registered account found.");

            // Clear all refresh tokens — logs out from every device
            if (user.RefreshTokens != null)
                user.RefreshTokens.Clear();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ApiException("An error occurred during logout.");

            // Update IsLoggedIn on UserProfile — not Identity
            userProfile.IsLoggedIn = false;
            await _userProfile.UpdateAsync(userProfile);

            return new Response<bool>(true, "Successfully logged out from all devices.");
        }

        // ── Logout from current device only 
        public async Task<Response<bool>> LogOutAsync(string refreshToken)
        {
            var userProfileId = Guid.Parse(_authenticatedUser.UserId);
            if (userProfileId == Guid.Empty)
                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(userProfileId);
            if (userProfile == null)
                throw new ApiException("User profile could not be found.");

            var user = await _userManager.FindByEmailAsync(userProfile.Email);
            if (user == null)
                throw new ApiException("No registered account found.");

            // Find and remove only the token tied to this device
            var tokenRecord = user.RefreshTokens?.FirstOrDefault(x => x.Token == refreshToken);
            if (tokenRecord == null)
                throw new ApiException("Refresh token not found or already revoked.");

            user.RefreshTokens.Remove(tokenRecord);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ApiException("An error occurred during logout.");

            // Only set IsLoggedIn to false if no other active sessions remain
            // — user may still be logged in on another device
            var hasOtherActiveSessions = user.RefreshTokens.Any(t => t.IsActive);
            if (!hasOtherActiveSessions)
            {
                userProfile.IsLoggedIn = false;
                await _userProfile.UpdateAsync(userProfile);
            }

            return new Response<bool>(true, "Successfully logged out from this device.");
        }

        public async Task<Response<ValidateInviteTokenVM>> ValidateInviteTokenAsync(string token)
        {
            var invite = await _inviteRepository.GetByTokenAsync(token);

            if (invite == null)
                throw new ApiException("This invitation link is invalid.");

            if (invite.IsAccepted)
                throw new ApiException("This invitation has already been accepted.");

            if (invite.ExpiryDate < DateTime.UtcNow)
                throw new ApiException("This invitation link has expired. Please contact your organization admin for a new one.");

            // Check if the invited email already has an account
            var existingUser = await _userManager.FindByEmailAsync(invite.Email);

            var result = new ValidateInviteTokenVM
            {
                Token = invite.Token,
                FirstName = invite.FirstName,
                LastName = invite.LastName,
                Email = invite.Email,
                OrganizationId = invite.OrganizationId,
                HasExistingAccount = existingUser != null
            };

            return new Response<ValidateInviteTokenVM>(result,
                message: existingUser != null
                    ? "Account found. Please log in to accept the invitation."
                    : "Valid invitation. Please complete your registration.");
        }

        public async Task<Response<string>> RegisterViaInviteAsync(RegisterViaInviteRequest request)
        {
            if (request.Password != request.ConfirmPassword)
                throw new ApiException("Passwords do not match.");

            // Validate the invite token first
            var invite = await _inviteRepository.GetByTokenAsync(request.Token) ?? throw new ApiException("This invitation link is invalid.");

            if (invite.IsAccepted)
                throw new ApiException("This invitation has already been accepted.");

            if (invite.ExpiryDate < DateTime.Now)
                throw new ApiException("This invitation link has expired. Please contact your organization admin.");

            // Make sure no account already exists for this email
            // (they should have been routed to AcceptInviteExistingUserAsync instead)
            var existingUser = await _userManager.FindByEmailAsync(invite.Email);
            if (existingUser != null)
                throw new ApiException("An account already exists for this email. Please log in to accept the invitation.");

            // Create the Identity user — email comes from invite, NOT from the request
            var user = new ApplicationUser
            {
                Email = invite.Email,
                UserName = invite.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                // AccountType = AccountType.Organization,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (createResult.Succeeded)
            {
                try
                {
                    // Assign org therapist role
                    await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

                    // Create UserProfile
                    var otp = GenerateOTP();
                    var userProfile = new UserProfile
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = invite.Email,
                        PhoneNumber = request.PhoneNumber,
                        VerificationCode = otp,
                    };

                    var profileResult = await _userProfile.AddAsync(userProfile);

                    if (profileResult.Id == Guid.Empty)
                    {
                        await _userManager.DeleteAsync(user);
                        throw new ApiException("Something went wrong while creating the user profile.");
                    }

                    // Create OrganizationUser record — linking user to the org
                    var orgUser = new OrganizationUsers
                    {
                        UserId = profileResult.Id,
                        OrganizationId = invite.OrganizationId,
                        IsActive = true,
                        JoinedAt = DateTime.UtcNow,
                        CreatedBy = profileResult.Id,
                    };

                    await _orgUserRepository.AddAsync(orgUser);

                    var orgUserRole = new OrganizationUserRoles
                    {
                        UserId = profileResult.Id,
                        OrganizationId = invite.OrganizationId,
                        OrganizationRoleId = invite.OrganizationRoleId,
                        CreatedBy = profileResult.Id,
                    };

                    await _organizationUserRolesRepository.AddAsync(orgUserRole);

                    // ── Handle Clinic Owner transfer if that role was invited ─────────
                    var invitedRole = await _organizationRolesRepository.GetByIdAsync(invite.OrganizationRoleId);

                    if (invitedRole?.Name == DefaultOrganizationRoles.ClinicOwner)
                    {
                        // Find current Clinic Owner and demote them to Clinic Admin
                        var currentOwnerUserRole = await _organizationUserRolesRepository.GetCurrentClinicOwnerAsync(invite.OrganizationId);

                        if (currentOwnerUserRole != null)
                        {
                            var clinicAdminRole = await _organizationRolesRepository.GetByNameAndOrgAsync(DefaultOrganizationRoles.ClinicAdmin, invite.OrganizationId);

                            if (clinicAdminRole == null)
                                throw new ApiException("Clinic Admin role could not be found for this organization.");

                            currentOwnerUserRole.OrganizationRoleId = clinicAdminRole.Id;
                            currentOwnerUserRole.LastModified = DateTime.UtcNow;
                            currentOwnerUserRole.LastModifiedBy = profileResult.Id;

                            await _organizationUserRolesRepository.UpdateAsync(currentOwnerUserRole);
                        }
                        // Mark the invite as accepted
                        invite.IsAccepted = true;
                        //invite.AcceptedAt = DateTime.UtcNow;
                        //invite.AcceptedByUserId = profileResult.Id;
                        invite.LastModified = DateTime.Now;
                        //invite.LastModifiedBy = profileResult.Id.ToString();

                        await _inviteRepository.UpdateAsync(invite);

                        // Send email confirmation — same flow as normal registration
                        var templatePath = "EmailTemplate/ConfirmEmail.cshtml";
                        await _emailService.SendFluentEmailTemplate(new EmailRequest
                        {
                            To = user.Email,
                            Body = string.Empty,
                            Subject = "Confirm your Neuro-Support account",
                            Otp = otp,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            Url = $"{_applicationUrlSettings.ConfirmEmailUrl}{invite.Email}?code={otp}",
                        }, templatePath);

                    }


                    return new Response<string>(
        "Check your email for your OTP to verify your account.",
        message: "Registration successful. Please verify your email to continue.");
                }
                catch
                {
                    // If anything after user creation fails, roll back the Identity user
                    await _userManager.DeleteAsync(user);
                    throw;
                }
            }
            else
            {
                throw new ApiException(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        public async Task<Response<bool>> AcceptInviteExistingUserAsync(string token)
        {
            var invite = await _inviteRepository.GetByTokenAsync(token) ??
                         throw new ApiException("This invitation link is invalid.");

            if (invite.IsAccepted)
                throw new ApiException("This invitation has already been accepted.");

            if (invite.ExpiryDate < DateTime.UtcNow)
                throw new ApiException("This invitation link has expired. Please contact your organization admin.");

            // Get the currently authenticated user
            var authenticatedUserId = Guid.Parse(_authenticatedUser.UserId);

            if (authenticatedUserId == Guid.Empty)
                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(authenticatedUserId) ??
                                    throw new ApiException("User profile could not be found.");

            // Ensure the logged-in user's email matches the invite
            // Handles the personal email scenario — if they don't match,
            // the wrong person is trying to accept this invite
            if (!userProfile.Email.Equals(invite.Email, StringComparison.OrdinalIgnoreCase))
                throw new ApiException(
                    "The email address on this invitation does not match your account. " +
                    "Please log in with the email address this invitation was sent to.");

            // Make sure they're not already in an org
            // var existingMembership = await _orgUserRepository.GetActiveByUserIdAsync(authenticatedUserId);
            //if (existingMembership != null)
            //    throw new ApiException("You are already a member of an organization.");

            // a user shouldn't have duplicate membership in the same org
            var existingMembership = await _orgUserRepository.GetByUserIdAndOrgIdAsync(authenticatedUserId, invite.OrganizationId);

            if (existingMembership != null && existingMembership.IsActive)
                throw new ApiException("You are already a member of this organization.");

            // Create the OrganizationUser record
            var orgUser = new OrganizationUsers
            {
                UserId = authenticatedUserId,
                OrganizationId = invite.OrganizationId,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = authenticatedUserId,
            };

            await _orgUserRepository.AddAsync(orgUser);

            // Assign org role from invite via OrganizationUserRoles
            var orgUserRole = new OrganizationUserRoles
            {
                UserId = authenticatedUserId,
                OrganizationId = invite.OrganizationId,
                OrganizationRoleId = invite.OrganizationRoleId,
                CreatedBy = authenticatedUserId,
                Created = DateTime.UtcNow,
            };

            await _organizationUserRolesRepository.AddAsync(orgUserRole);

            // ── Handle Clinic Owner transfer if that role was invited ─────────────
            var invitedRole = await _organizationRolesRepository
                .GetByIdAsync(invite.OrganizationRoleId);

            if (invitedRole?.Name == DefaultOrganizationRoles.ClinicOwner)
            {
                var currentOwnerUserRole = await _organizationUserRolesRepository
                    .GetCurrentClinicOwnerAsync(invite.OrganizationId);

                if (currentOwnerUserRole != null)
                {
                    var clinicAdminRole = await _organizationRolesRepository
                        .GetByNameAndOrgAsync(
                            DefaultOrganizationRoles.ClinicAdmin,
                            invite.OrganizationId);

                    if (clinicAdminRole == null)
                        throw new ApiException("Clinic Admin role could not be found for this organization.");

                    currentOwnerUserRole.OrganizationRoleId = clinicAdminRole.Id;
                    currentOwnerUserRole.LastModified = DateTime.UtcNow;
                    currentOwnerUserRole.LastModifiedBy = authenticatedUserId;

                    await _organizationUserRolesRepository.UpdateAsync(currentOwnerUserRole);
                }
            }

            // Mark invite as accepted
            invite.IsAccepted = true;
            invite.AcceptedAt = DateTime.UtcNow;
            invite.LastModified = DateTime.UtcNow;
            invite.AcceptedByUserId = authenticatedUserId;
            invite.LastModifiedBy = authenticatedUserId;

            await _inviteRepository.UpdateAsync(invite);

            return new Response<bool>(true, "You have successfully joined the organization.");
        }

        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                throw new ApiException($"Token did not match any users.");

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                throw new ApiException($"Token Not Active.");

            var userProfile = await _userProfile.GetUserByEmailAsync(user.Email);
            if (userProfile == null)
                throw new ApiException("User profile could not be found.");

            //Revoke Current Refresh Token
            refreshToken.Revoked = DateTime.UtcNow;

            //Generate new Refresh Token and save to Database
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            _context.SaveChanges();

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            AuthenticationResponse response = new AuthenticationResponse
            {
                // IdentityId = user.Id,
                UserId = userProfile.Id.ToString(),
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                UserName = user.UserName
            };
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = [.. rolesList];
            response.IsVerified = user.EmailConfirmed;
            response.TokenExpires = jwtSecurityToken.ValidTo;
            response.RefreshToken = newRefreshToken.Token;
            response.RefreshTokenExpiration = newRefreshToken.Expires;

            return new Response<AuthenticationResponse>(response, $"Token refreshed for {user.UserName}.");
        }

        // ── Verify OTP (email confirmation)
        public async Task<Response<bool>> VerifyUser(int otp)
        {
            var userProfile = await _userProfile.GetUserByOtpAsync(otp);
            if (userProfile == null)
                throw new ApiException("Invalid OTP.");

            if (userProfile.VerificationCode == 0)
                throw new ApiException("This OTP has already been used or has expired.");

            // Mark OTP as used
            userProfile.VerificationCode = 0;
            await _userProfile.UpdateAsync(userProfile);

            // Confirm email on the Identity side
            var aspUser = await _userManager.FindByEmailAsync(userProfile.Email);
            if (aspUser == null)
                throw new ApiException("Associated account could not be found.");

            aspUser.EmailConfirmed = true;
            await _userManager.UpdateAsync(aspUser);

            return new Response<bool>(true, "Email verified successfully. You can now log in.");
        }

        // ── Resend Verification Email
        public async Task<Response<string>> ResendVerificationMailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new ApiException($"No registered account found with '{email}'.");

            if (user.EmailConfirmed)
                throw new ApiException($"'{email}' has already been confirmed. You can log in.");

            var userProfile = await _userProfile.GetUserByEmailAsync(email);
            if (userProfile == null)
                throw new ApiException($"User profile not found for '{email}'.");

            // Generate fresh OTP and update profile
            var otp = GenerateOTP();
            userProfile.VerificationCode = otp;
            await _userProfile.UpdateAsync(userProfile);

            await _emailService.SendFluentEmailTemplate(new EmailRequest
            {
                To = userProfile.Email,
                Body = string.Empty,
                Subject = "Confirm Your Neuro-Support Account",
                Otp = otp,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Url = $"{_applicationUrlSettings.ConfirmEmailUrl}{userProfile.Email}?code={otp}",
            }, "EmailTemplate/ConfirmEmail.cshtml");

            return new Response<string>(
                $"A new verification email has been sent to '{email}'. Please check your inbox.",
                message: "Verification email resent successfully.");
        }

        // ── Forgot Password
        public async Task<Response<bool>> ForgotPassword(ForgotPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ??
                             throw new ApiException($"No Accounts Registered with {model.Email}.");

            var userProfile = await _userProfile.GetUserByEmailAsync(user.Email);
            if (userProfile == null)
                throw new ApiException($"User profile not found for '{model.Email}'.");

            var role = await _userManager.GetRolesAsync(user);
            var url = await GenerateForgotPasswordUrl(user, role.FirstOrDefault());

            await _emailService.SendFluentEmailTemplate(new EmailRequest
            {
                To = user.Email,
                // To = "marioonosnorbert@gmail.com",
                Body = string.Empty,
                Subject = "Reset Your Neuro-Support Password",
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Url = url,
            }, "EmailTemplate/ResetPassword.cshtml");

            return new Response<bool>(true, "Password reset email sent successfully.");
        }

        // ── Reset Password
        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ??
                            throw new ApiException($"No registered account found with '{model.Email}'.");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);

            if (result.Succeeded)
            {
                // Invalidate all active refresh tokens on password reset —
                // forces re-login on all devices for security
                if (user.RefreshTokens != null)
                    user.RefreshTokens.Clear();

                _context.Update(user);
                _context.SaveChanges();

                return new Response<string>(model.Email, message: "Password reset successfully. Please log in with your new password.");
            }
            else
            {
                throw new ApiException(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        // ── Change Password 
        public async Task<Response<string>> ChangePassword(ChangePasswordRequest model)
        {
            var userProfileId = Guid.Parse(_authenticatedUser.UserId);
            if (userProfileId == Guid.Empty)
                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(userProfileId) ??
                              throw new ApiException("User profile could not be found.");

            var user = await _userManager.FindByEmailAsync(userProfile.Email) ??
                       throw new ApiException("Associated account could not be found.");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Invalidate all refresh tokens on password change —
                // forces re-login on all devices for security
                if (user.RefreshTokens != null)
                    user.RefreshTokens.Clear();

                _context.Update(user);
                _context.SaveChanges();

                return new Response<string>(
                    userProfile.Email,
                    succeeded: true,
                    message: "Password changed successfully. Please log in again.");
            }
            else
            {
                throw new ApiException(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        //Not in use right now 
        public async Task<Response<string>> ConfirmEmailAsync(Guid userId, string code)
        {
            var userProfile = await _userProfile.GetUserByIdAsync(userId);
            if (userProfile == null)
                throw new ApiException("User profile could not be found.");

            // Use the email from UserProfile to find the Identity user
            var user = await _userManager.FindByEmailAsync(userProfile.Email);
            if (user == null)
                throw new ApiException("Associated account could not be found.");

            if (user.EmailConfirmed)
                throw new ApiException($"'{user.Email}' has already been confirmed. You can log in.");

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

            if (result.Succeeded)
            {
                // Mark OTP as used on UserProfile side as well — keeps both in sync
                userProfile.VerificationCode = 0;
                await _userProfile.UpdateAsync(userProfile);

                return new Response<string>(
                    userId.ToString(),
                    message: $"Account confirmed for '{user.Email}'. You can now log in.");
            }
            else
            {
                throw new ApiException(
                    $"An error occurred while confirming '{user.Email}': " +
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        // ── Registration for organizations
        public async Task<Response<string>> RegisterOrganizationAsync(RegisterOrganizationRequest request)
        {
            if (request.Password != request.ConfirmPassword)
                throw new ApiException("Passwords do not match.");

            // ── Check email isn't already taken ───────────────────────────────────
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ApiException($"Email '{request.Email}' is already registered.");

            // ── Check verified domain isn't already claimed
            // Unlike org names, two orgs cannot own the same email domain
            if (!string.IsNullOrWhiteSpace(request.VerifiedDomain))
            {
                var domainTaken = await _organizationsRepository.GetByDomainAsync(request.VerifiedDomain);
                if (domainTaken != null)
                    throw new ApiException($"The domain '{request.VerifiedDomain}' is already associated with another organization.");
            }

            // ── Step 1: Create Identity user ──────────────────────────────────────
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                //AccountType = AccountType.Organization,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                throw new ApiException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            try
            {
                // Assign system-level Identity role
                await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

                // ── Step 3: Create UserProfile ────────────────────────────────────
                var otp = GenerateOTP();
                var userProfile = new UserProfile
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    OtherName = request.OtherName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    VerificationCode = otp,
                };

                var profileResult = await _userProfile.AddAsync(userProfile);
                if (profileResult.Id == Guid.Empty)
                {
                    await _userManager.DeleteAsync(user);
                    throw new ApiException("Something went wrong while creating the user profile.");
                }

                // ── Step 4: Create Organization ───────────────────────────────────
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
                    //Status = OrganizationStatus.Active,
                };

                var orgResult = await _organizationsRepository.AddAsync(organization);
                if (orgResult.Id == Guid.Empty)
                    throw new ApiException("Something went wrong while creating the organization.");

                // Seed default org roles for this organization ──────────────────
                var roleEntities = DefaultOrganizationRoles.GetDefaults().Select(r => new OrganizationRoles
                {
                    OrganizationId = orgResult.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsDefault = true,
                }).ToList();

                var seededRoles = await _organizationRolesRepository.AddRangeAsync(roleEntities);

                // ── Assign Clinic Owner role to the registering admin ─────────────
                var clinicOwnerRole = seededRoles.First(r => r.Name == DefaultOrganizationRoles.ClinicOwner);

                // ── Step 5: Create OrganizationUser — link admin to org ───────────
                var orgUser = new OrganizationUsers
                {
                    UserId = profileResult.Id,
                    OrganizationId = orgResult.Id,
                    //OrganizationRoleId = clinicOwnerRole.Id,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    CreatedBy = profileResult.Id,
                    Created = DateTime.UtcNow,
                };

                await _orgUserRepository.AddAsync(orgUser);

                //Create OrganizationUserRole to link the user to the role
                var orgUserRole = new OrganizationUserRoles
                {
                    UserId = profileResult.Id,
                    OrganizationId = orgResult.Id,
                    OrganizationRoleId = clinicOwnerRole.Id,
                    CreatedBy = profileResult.Id,
                };

                await _organizationUserRolesRepository.AddAsync(orgUserRole);

                // ── Step 6: Send confirmation email ──────────────────────────────
                await _emailService.SendFluentEmailTemplate(new EmailRequest
                {
                    To = request.Email,
                    Body = string.Empty,
                    Subject = "Confirm Your Neuro-Support Account",
                    Otp = otp,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Url = $"{_applicationUrlSettings.ConfirmEmailUrl}{request.Email}?code={otp}",
                }, "EmailTemplate/ConfirmEmail.cshtml");

                return new Response<string>(
                    "Check your email for your OTP to verify your account.",
                    message: "Organization registered successfully. Please verify your email to continue.");
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }
        }

        // ── Request Email Change
        public async Task<Response<string>> RequestEmailChangeAsync(RequestEmailChangeRequest request)
        {
            var userProfileId = _authenticatedUser.UserId ?? throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(Guid.Parse(userProfileId)) 
                              ?? throw new ApiException("User profile could not be found.");

            // Check new email isn't the same as current
            if (userProfile.Email.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase))
                throw new ApiException("The new email cannot be the same as your current email.");

            // Check new email isn't already taken
            var existingUser = await _userManager.FindByEmailAsync(request.NewEmail) 
                               ?? throw new ApiException($"'{request.NewEmail}' is already registered to another account.");

            // Validate password
            var identityUser = await _userManager.FindByEmailAsync(userProfile.Email) ?? 
                               throw new ApiException("Associated account could not be found.");

            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
            if (!passwordValid)
                throw new ApiException("Incorrect password. Please try again.");

            // Check no active pending request already exists
            var existingRequest = await _emailChangeRequestRepository.GetPendingByUserProfileIdAsync(Guid.Parse(userProfileId)) ?? 
                                  throw new ApiException("A pending email change request already exists. Please check your inbox or wait for it to expire before requesting again.");

            // Create new email change request record
            var otp = GenerateOTP();
            var emailChangeRequest = new EmailChangeRequest
            {
                CurrentEmail = userProfile.Email,
                NewEmail = request.NewEmail,
                OtpCode = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false,
            };

            await _emailChangeRequestRepository.AddAsync(emailChangeRequest);

            // Send OTP to the NEW email
            await _emailService.SendFluentEmailTemplate(new EmailRequest
            {
                To = request.NewEmail,
                Body = string.Empty,
                Subject = "Confirm Your New Email Address",
                Otp = otp,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Url = $"{_applicationUrlSettings.ConfirmEmailUrl}{request.NewEmail}?code={otp}",
            }, "EmailTemplate/ConfirmEmail.cshtml");

            return new Response<string>($"A verification code has been sent to '{request.NewEmail}'.",
                                        message: "Email change requested successfully.");
        }

        // ── Confirm Email Change 
        public async Task<Response<string>> ConfirmEmailChangeAsync(ConfirmEmailChangeRequest request)
        {
            var userProfileId = _authenticatedUser.UserId ?? 
                                throw new ApiException("Authenticated user could not be found.");

            var userProfile = await _userProfile.GetUserByIdAsync(Guid.Parse(userProfileId)) ??
                              throw new ApiException("User profile could not be found.");

            // Get the pending request from the dedicated table
            var emailChangeRequest = await _emailChangeRequestRepository.GetPendingByUserProfileIdAsync(Guid.Parse(userProfileId)) ??
                                     throw new ApiException("No pending email change found. Please request an email change first.");

            // Validate OTP
            if (emailChangeRequest.OtpCode != request.Otp)
                throw new ApiException("Invalid verification code. Please try again.");

            // Double-check new email still isn't taken
            var existingUser = await _userManager.FindByEmailAsync(emailChangeRequest.NewEmail) ?? 
                               throw new ApiException($"'{emailChangeRequest.NewEmail}' has been registered by another account. " +
                                                      "Please request a new email change with a different address.");

            // Update Identity user
            var identityUser = await _userManager.FindByEmailAsync(userProfile.Email) ?? 
                               throw new ApiException("Associated account could not be found.");

            identityUser.Email = emailChangeRequest.NewEmail;
            identityUser.UserName = emailChangeRequest.NewEmail;
            identityUser.NormalizedEmail = emailChangeRequest.NewEmail.ToUpper();
            identityUser.NormalizedUserName = emailChangeRequest.NewEmail.ToUpper();
            identityUser.EmailConfirmed = true;

            // Clear all refresh tokens — forces re-login with new email
            if (identityUser.RefreshTokens != null)
                identityUser.RefreshTokens.Clear();

            var updateResult = await _userManager.UpdateAsync(identityUser);
            if (!updateResult.Succeeded)
                throw new ApiException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

            // Update UserProfile email
            userProfile.Email = emailChangeRequest.NewEmail;

            await _userProfile.UpdateAsync(userProfile);

            // Mark the request as used — prevents OTP replay
            emailChangeRequest.IsUsed = true;

            await _emailChangeRequestRepository.UpdateAsync(emailChangeRequest);

            return new Response<string>(emailChangeRequest.NewEmail, 
                                        message: "Email updated successfully. Please log in again with your new email.");
        }

        public List<Guid> GetUserIdsByRoleAsync(string role)
        {
            var aspUsersEmail = _userManager.GetUsersInRoleAsync(role).Result.Select(x => x.Email).ToList();
            var userIds = _userProfile.GetUserIdsByEmail(aspUsersEmail).Result.ToList();

            return userIds;
        }

        public async Task<string> GetUserRoleByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            string rolename = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return rolename;
        }

        public async Task<string> GetUserRoleById(Guid userId)
        {
            var userprofile = await _userProfile.GetByIdAsync(userId);
            var user = await _userManager.FindByEmailAsync(userprofile.Email);
            string rolename = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return rolename;
        }

        public Response<AuthenticationResponse> PeriodicAuthentication(AuthenticationRequest request)
        {
            var user = _userManager.FindByEmailAsync(request.Email).Result;

            if (user == null)
            {
                throw new ApiException($"No Accounts Registered with {request.Email}.");
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var resp = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            //var result = _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false).Result;

            if (resp == PasswordVerificationResult.Failed)
            {
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");
            }

            var response = new AuthenticationResponse { UserName = user.UserName };

            return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }

        private RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                //CreatedByIp = ipAddress
            };
        }

        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user, bool isOffline = false)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userProfile = await _userProfile.GetUserByEmailAsync(user.Email);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", userProfile.Id.ToString()),
                new Claim("rol", roles.FirstOrDefault()),
                //new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            //var jwtSecurityToken = new JwtSecurityToken(
            //   issuer: _jwtSettings.Issuer,
            //   audience: _jwtSettings.Audience,
            //   claims: claims,
            //   expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            //   signingCredentials: signingCredentials);

            if (isOffline == true)
            {
                var jwtSecurityToken = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddDays(3650), //offline token would last for 10 years 😂 i.e it won't expire!
               signingCredentials: signingCredentials);

                return jwtSecurityToken;
            }
            else
            {
                var jwtSecurityToken = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
               signingCredentials: signingCredentials);

                return jwtSecurityToken;
            }

            //return jwtSecurityToken;
        }

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private int GenerateOTP()
        {
            int min = 1000;
            int max = 9999;
            var random = new Random();
            var otp = random.Next(min, max);

            return otp;
        }

        private async Task<string> GenerateForgotPasswordUrl(ApplicationUser user, string role)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var _enpointUri = new Uri(_applicationUrlSettings.ForgetPasswordUrl);
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "email", user.Email);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "role", role);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            return verificationUri;
        }

        //To Create Supervisors
        public async Task<Response<string>> CreateAdmin(RegisterRequest request)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.Email);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Email '{request.Email}' is already taken.");
            }
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);

            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, SystemRoles.SuperAdmin.ToString());

                    //use the request to create a new user profile
                    var userProfile = new UserProfile
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        VerificationCode = 0
                    };

                    var resp = await _userProfile.AddAsync(userProfile);

                    if (resp.Id != null)
                    {
                        return new Response<string>("User registered successfully.");
                    }
                    else
                    {
                        await _userManager.DeleteAsync(user);
                        throw new ApiException("Something went wrong while profiling user");
                    }
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    throw new ApiException($"{result.Errors}");
                }
            }
            else
            {
                throw new ApiException($"Email {request.Email} is already registered.");
            }
        }

        //To Get All Users
        public List<Guid> GetUsersAsync()
        {
            var aspUsersEmail = _userManager.GetUsersInRoleAsync(SystemRoles.User.ToString()).Result.Select(x => x.Email).ToList();
            var userIds = _userProfile.GetUserIdsByEmail(aspUsersEmail).Result.ToList();

            return userIds;
        }

        //To Get All Admimn
        public List<Guid> GetAdminsAsync()
        {
            var aspUsersEmail = _userManager.GetUsersInRoleAsync(SystemRoles.SuperAdmin.ToString()).Result.Select(x => x.Email).ToList();
            var userIds = _userProfile.GetUserIdsByEmail(aspUsersEmail).Result.ToList();

            return userIds;
        }

    }
}
