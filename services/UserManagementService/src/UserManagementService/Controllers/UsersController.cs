using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IProfileService _profileService;

        public UsersController(IRegisterService registerService, ILoginService loginService, IProfileService profileService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _profileService = profileService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Profile(string id)
        {
            var result = await _profileService.GetUser(id);

            var response = new APIResponse<UserDTO>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Profile(string id, EditUserDto editData)
        {
            var result = await _profileService.UpdateUser(id, editData);

            var response = new APIResponse<UserDTO>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginData)
        {
            var result = await _loginService.LoginUser(loginData);

            var response = new APIResponse<TokenDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserDTO registerData)
        {
            var result = await _registerService.RegisterUser(registerData);

            var response = new APIResponse<RegisterDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            var status = CreatedAtAction(nameof(Register), new { id = result.Data.UserData.Id }, response);
            response.SetStatus(status);

            return status;
        }
    }
}