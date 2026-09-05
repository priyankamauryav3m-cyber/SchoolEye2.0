using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    /// <summary>
    /// Demo/POC controller exercising the standard Controller -> Service -> Repository -> Dapper -> SP flow.
    /// Deliberately named "RegistrationDemo" (not "Registration") to avoid colliding with the existing
    /// production RegistrationController / RegistrationService.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationDemoController : ControllerBase
    {
        private readonly IRegistrationDemoRepository _service;

        public RegistrationDemoController(IRegistrationDemoRepository service)
        {
            _service = service;
        }

        [HttpPost("AddRegistrationDemo")]
        public async Task<IActionResult> AddRegistrationDemo([FromBody] RegistrationDemoModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode)
                || string.IsNullOrWhiteSpace(model.StudentName))
            {
                return BadRequest(ApiResponse<object>.Fail("GroupCode, BranchCode and StudentName are required."));
            }

            try
            {
                var regiNo = await _service.AddRegistrationDemo(model);
                if (regiNo <= 0)
                {
                    return Ok(ApiResponse<object>.Ok(null, "Registration demo could not be saved.", code: 0));
                }

                return StatusCode(StatusCodes.Status201Created,
                    ApiResponse<object>.Ok(new { RegiNo = regiNo }, "Registration demo saved successfully.", code: 1));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while saving the registration demo."));
            }
        }

        [HttpGet("GetRegistrationDemo")]
        public async Task<IActionResult> GetRegistrationDemo(
            [FromQuery] string groupCode,
            [FromQuery] string branchCode,
            [FromQuery] long sessionId,
            [FromQuery] int regiNo)
        {
            if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(branchCode) || regiNo <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("GroupCode, BranchCode and RegiNo are required."));
            }

            try
            {
                var data = await _service.GetRegistrationDemoByRegiNo(groupCode, branchCode, sessionId, regiNo);
                if (data == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Registration demo record was not found."));
                }
                return Ok(ApiResponse<RegistrationDemoModel>.Ok(data, "Registration demo fetched successfully."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while fetching the registration demo."));
            }
        }

        [HttpGet("GetRegistrationDemoList")]
        public async Task<IActionResult> GetRegistrationDemoList(
            [FromQuery] string groupCode,
            [FromQuery] string branchCode,
            [FromQuery] long sessionId)
        {
            if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(branchCode))
            {
                return BadRequest(ApiResponse<object>.Fail("GroupCode and BranchCode are required."));
            }

            try
            {
                var data = await _service.GetRegistrationDemoList(groupCode, branchCode, sessionId);
                return Ok(ApiResponse<IEnumerable<RegistrationDemoModel>>.Ok(data, "Registration demo list fetched successfully."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while fetching the registration demo list."));
            }
        }
    }
}
