using ASPNET_Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET_Ecommerce.Controllers.Api;

[ApiController]
[Route("api/storefront")]
public class StorefrontController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;

    public StorefrontController(ISystemSettingService systemSettingService)
    {
        _systemSettingService = systemSettingService;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _systemSettingService.GetPublicAsync(cancellationToken);
        return Ok(settings);
    }
}