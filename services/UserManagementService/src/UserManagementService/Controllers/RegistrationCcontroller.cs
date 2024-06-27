using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationCcontroller : ControllerBase
    {
        private readonly IUserService _userService;

        public RegistrationCcontroller(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserDTO registerData)
        {
            var result = await _userService.RegisterUser(registerData);

            var respone = new APIResponse<UserDTO>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                respone.SetStatus(BadRequest());
                return BadRequest(respone);
            }

            respone.SetStatus(Ok());
            return Ok(respone);
        }
    }
}
