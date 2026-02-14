using Microsoft.AspNetCore.Mvc;

namespace ExternalIdentity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : Controller
{
    private readonly JsonTokenService _tokenService;
    
    public IdentityController(JsonTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Create([FromBody] CreateTokenRequest tokenRequest)
    {
        string token = _tokenService.GenerateToken(tokenRequest);

        return Ok(token);
    }
}