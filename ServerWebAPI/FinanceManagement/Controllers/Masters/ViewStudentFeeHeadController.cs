using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ViewStudentFeeHeadController : ControllerBase
    {
        private readonly ViewStudentFeeHeadRepository _service;
        public ViewStudentFeeHeadController(ViewStudentFeeHeadRepository service)
        {
            _service = service;
        }
        [HttpPost("GetStudentMappedWithFeeHead")]
        public async Task<IActionResult> GetStudentMappedWithFeeHead([FromBody] MapwithFeehead model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid request.");
                var result = await _service.GetStudentMappedWithFeeHead(model);
                if (result == null || result.Count == 0)
                    return NotFound("No record found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching data.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("GetSearchedStudent")]
        public async Task<IActionResult> GetSearchedStudentData([FromBody] MapwithFeehead model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid request.");
                var result = await _service.GetSearchedStudentData(model);
                if (result == null || result.Count == 0)
                    return NotFound("No record found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching data.",
                    Error = ex.Message
                });
            }
        }


        [HttpPost("UnMapFeeHeadWithStudent")]
        public async Task<IActionResult> UnMapFeeHeadWithStudent([FromBody] UnMapFeeHead model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid request data.");
                var result = await _service.UnMapFeeHeadWithStudent(model);
                if (result.Status)
                    return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }


        [HttpPost]
        [Route("StudentCopyHead")]
        public async Task<IActionResult> StudentCopyHeadData([FromBody] StudenmapheadModal stu)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _service.StudentCopyHeadData(stu);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Copy(Head  already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Copy(Head inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Copy(Head updated successfully)",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }
    }
}

