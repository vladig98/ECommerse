using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("/api/users/[action]")]
    public class RegistrationCcontroller : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;

        public RegistrationCcontroller(IRegisterService registerService, ILoginService loginService)
        {
            _registerService = registerService;
            _loginService = loginService;
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
