using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

            var respone = new APIResponse<RegisterDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                respone.SetStatus(BadRequest());
                return BadRequest(respone);
            }

            var status = Created("/users/1", respone);

            respone.SetStatus(status);
            return status;
        }
    }
}
