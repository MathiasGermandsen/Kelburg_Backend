using Microsoft.AspNetCore.Mvc;
using KelBurgAPI.Models;

namespace KelBurgAPI.Controllers;


[Route("api/[controller]")]
[ApiController]

public class BogusController : ControllerBase
{

    [HttpPost("GenUsers")]
    public ActionResult<List<UserCreateDTO>> GenUsers(int count = 10)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }
        
        var users = KelBurgAPI.BogusGenerators.BogusUsers.GenerateUsers(count);
        return Ok(users);
    }
}
