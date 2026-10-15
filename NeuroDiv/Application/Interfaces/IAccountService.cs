using Application.DTOs.Account;
using Application.DTOs.OrganizationUsersInvite;
using Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<string>> RegisterAsync(RegisterRequest request);
        Task<Response<bool>> VerifyUser(int otp);
        Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, bool isOffline = false, bool isExternal = false);
        Task<Response<string>> ConfirmEmailAsync(Guid userId, string code);
        Task<Response<bool>> ForgotPassword(ForgotPasswordRequest model);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<string>> ChangePassword(ChangePasswordRequest model);
        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token);
        List<Guid> GetUserIdsByRoleAsync(string role);
        Task<Response<string>> ResendVerificationMailAsync(string email);
        Task<string> GetUserRoleByEmail(string email);
        Task<string> GetUserRoleById(Guid userId);
        Response<AuthenticationResponse> PeriodicAuthentication(AuthenticationRequest request);
        Task<Response<string>> CreateAdmin(RegisterRequest request);
        List<Guid> GetUsersAsync();
        List<Guid> GetAdminsAsync();
        Task<Response<bool>> LogOutAsync();
        Task<Response<bool>> LogOutAsync(string refreshToken);
        Task<Response<ValidateInviteTokenVM>> ValidateInviteTokenAsync(string token);
        Task<Response<string>> RegisterViaInviteAsync(RegisterViaInviteRequest request);
        Task<Response<bool>> AcceptInviteExistingUserAsync(string token);
        Task<Response<string>> RegisterOrganizationAsync(RegisterOrganizationRequest request);
        Task<Response<string>> RequestEmailChangeAsync(RequestEmailChangeRequest request);
        Task<Response<string>> ConfirmEmailChangeAsync(ConfirmEmailChangeRequest request);
    }
}
