using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("/api/users/[action]")]
    public class RegistrationCcontroller : ControllerBase
    {
        private readonly IRegisterService _registerService;

        public RegistrationCcontroller(IRegisterService registerService)
        {
            _registerService = registerService;
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
