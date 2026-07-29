using ApplicationInterface.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectNameController : Controller
    {
        private readonly IStudentNameRepository _service;

        public SubjectNameController(IStudentNameRepository service)
        {
            _service = service;
        }

        [HttpGet("GetCompulsorySubjects")]
        public async Task<IActionResult> GetCompulsorySubjects(string groupCode, string branchCode, string streamCode)
        {
            try
            {
                var result = await _service.GetCompulsorySubjects(groupCode, branchCode, streamCode);

                if (result == null || result.Count == 0)
                    return NotFound("No Subjects Found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("ElectiveSubjects")]
        public async Task<IActionResult> GetElectiveSubjects(string groupCode,string branchCode,string streamCode,string groupId,string firstElement)
        {
            try
            {
                var result = await _service.ElectiveSubjectsData(
                    groupCode, branchCode, streamCode, groupId, firstElement);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
