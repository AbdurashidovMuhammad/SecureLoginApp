
using Microsoft.AspNetCore.Mvc;
using SecureLoginApp.API.Attributes;
using SecureLoginApp.Application.Security.AuthEnums;
using SecureLoginApp.Application.Services;

namespace SecureLoginApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetPermissions()
        {
            var allPermissions = _permissionService.GetAllPermissionDescriptions();
            return Ok(allPermissions);
        }

        [HttpGet("all-grouped")]
        [Authorize(ApplicationPermissionCode.GetPermissions)]
        public async Task<IActionResult> GetGroupedPermissionsFromDb()
        {
            var result = await _permissionService.GetPermissionsFromDbAsync();
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
