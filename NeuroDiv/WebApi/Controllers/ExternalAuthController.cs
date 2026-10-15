using Application.DTOs.Account;
using Application.DTOs.Organizations;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalAuthService _externalAuthService;

        public ExternalAuthController(IExternalAuthService externalAuthService)
        {
            _externalAuthService = externalAuthService;
        }

        /// <summary>
        /// Step 1 — Redirect to Google login page.
        /// Frontend navigates to this URL directly (not an AJAX call).</summary>
        /// <returns></returns>
        [HttpGet("google/login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "ExternalAuth",
                null, Request.Scheme);
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Step 2 — Google redirects here after authentication.
        /// Returns ExternalAuthResponse — frontend checks HasAccount</summary>
        /// to decide which screen to show next.<returns></returns>
        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await _externalAuthService
                .HandleExternalLoginCallbackAsync("Google");
            return Ok(result);
        }

        /// <summary>
        /// Step 1 — Redirect to Microsoft login page.
        /// </summary>
        /// <returns></returns>
        [HttpGet("microsoft/login")]
        [AllowAnonymous]
        public IActionResult MicrosoftLogin()
        {
            var redirectUrl = Url.Action(nameof(MicrosoftCallback), "ExternalAuth", null, Request.Scheme);
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, "Microsoft");
        }

        /// <summary>
        /// Step 2 — Microsoft redirects here after authentication.
        /// </summary>
        /// <returns></returns>
        [HttpGet("microsoft/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> MicrosoftCallback()
        {
            var result = await _externalAuthService.HandleExternalLoginCallbackAsync("Microsoft");
            return Ok(result);
        }

        /// <summary>
        /// Step 3a — New user chose Individual.
        /// Completes registration and returns JWT.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("complete/individual")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteIndividualRegistration(CompleteExternalIndividualRegistrationRequest request)
        {
            var result = await _externalAuthService.CompleteExternalIndividualRegistrationAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Step 3b — New user chose Organization.
        /// Creates account only — org is created via /organization/create.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("complete/organization")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteOrgRegistration(CompleteExternalOrgRegistrationRequest request)
        {
            var result = await _externalAuthService.CompleteExternalOrgRegistrationAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Step 4 (org users only) — Creates the organization
        /// for an already-authenticated user.
        /// Also available standalone for any authenticated user
        /// who needs to create an org.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("organization/create")]
        [Authorize]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationForExistingUserRequest request)
        {
            var result = await _externalAuthService.CreateOrganizationForExistingUserAsync(request);
            return Ok(result);
        }
    }
}
