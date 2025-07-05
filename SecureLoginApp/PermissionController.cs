using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetPermissions()
        {
            var allPermissions = _permissionService.GetAllPermissionDescriptions();
            return Ok(allPermissions);
        }

        [HttpGet("all-grouped")]
        public async Task<IActionResult> GetGroupedPermissionsFromDb()
        {
            var result = await _permissionService.GetPermissionsFromDbAsync();
            if (result.Succeeded) // Fix: Changed 'IsSuccess' to 'Succeeded' based on the ApiResult<T> type signature  
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
