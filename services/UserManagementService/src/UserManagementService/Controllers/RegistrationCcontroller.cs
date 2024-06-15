using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationCcontroller : ControllerBase
    {
        [HttpPost]
        public IActionResult Register(string usernmae, string password)
        {
            return Ok();
        }
    }
}
