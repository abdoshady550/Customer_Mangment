using Customer_Mangment.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Customer_Mangment.IdentityServer.Controllers
{

    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { user.Id, user.Email });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Email, user.DisplayName });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }
    }

    public record RegisterModel(string Email, string Password, string DisplayName);
    public record ChangePasswordModel(string CurrentPassword, string NewPassword);
}
