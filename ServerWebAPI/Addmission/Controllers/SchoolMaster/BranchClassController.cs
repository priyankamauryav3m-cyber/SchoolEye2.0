using ApplicationInterface.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BranchClassController : ControllerBase
    {
        private readonly IBranchClassRepository _service;

        public BranchClassController(IBranchClassRepository service)
        {
            _service = service;
        }
        [HttpGet("GetClassCode")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetBranchesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching class codes.",
                    error = ex.Message
                });
            }
        }
    }
}
