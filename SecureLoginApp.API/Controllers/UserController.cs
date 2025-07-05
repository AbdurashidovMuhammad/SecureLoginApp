using Microsoft.AspNetCore.Mvc;
using SecureLoginApp.API.Attributes;
using SecureLoginApp.Application.Models;
using SecureLoginApp.Application.Models.Users;
using SecureLoginApp.Application.Services;

namespace SecureLoginApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]

        public async Task<ApiResult<string>> RegisterAsync([FromBody] RegisterUserModel model)
        {
            var result = await _userService.RegisterAsync(model.Fullname, model.Email, model.Password, model.isAdminSite);
            return result;
        }

        [HttpPost("login")]
        public async Task<ApiResult<LoginResponseModel>> LoginAsync([FromBody] LoginUserModel model)
        {
            var result = await _userService.LoginAsync(model);
            return result;
        }

        [HttpPost("verify-otp")]
        public async Task<ApiResult<string>> VerifyOtpAsync([FromBody] OtpVerificationModel model)
        {
            var result = await _userService.VerifyOtpAsync(model);
            return result;
        }

        [Authorize]
        [HttpGet("get-user-auth")]
        public async Task<IActionResult> GetUserAuth()
        {
            var result = await _userService.GetUserAuth();
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
