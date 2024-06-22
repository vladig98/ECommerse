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

            if (!result.Succeeded)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = new ErrorDetails
                    {
                        Code = "400",
                        Message = result.Error.ErrorMessage,
                        Details = new List<string> { result.Error.ErrorMessage }
                    }
                };
                return BadRequest(errorResponse);
            }

            var successResponse = new APIResponse<UserDTO>
            {
                Status = "success",
                Data = result.Data,
                Message = "User registered successfully"
            };

            return Ok(successResponse);
        }
    }
}
