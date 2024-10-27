using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class UsersController(IRegisterService registerService, ILoginService loginService, IProfileService profileService) : ControllerBase
    {
        private readonly IRegisterService _registerService = registerService;
        private readonly ILoginService _loginService = loginService;
        private readonly IProfileService _profileService = profileService;

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Profile(string id)
        {
            APIResponse<UserDTO> result = await _profileService.GetUser(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Profile(string id, EditUserDto editData)
        {
            APIResponse<UserDTO> result = await _profileService.UpdateUser(id, editData);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginData)
        {
            if (IsAlreadyLoggedIn())
            {
                return BadRequest();
            }

            APIResponse<TokenDto> result = await _loginService.LoginUser(loginData);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserDTO registerData)
        {
            if (IsAlreadyLoggedIn())
            {
                return BadRequest();
            }

            APIResponse<RegisterDto> result = await _registerService.RegisterUserAsync(registerData);

            return CreatedAtAction(nameof(Profile), new { id = result.Data.UserData.Id }, result);
        }

        private bool IsAlreadyLoggedIn()
        {
            return !Request.Headers.Authorization.IsNullOrEmpty();
        }
    }
}