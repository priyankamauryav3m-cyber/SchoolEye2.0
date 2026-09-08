using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class SiblingController : ControllerBase
    {
        private readonly ISiblingRepository _service;

        public SiblingController(ISiblingRepository service)
        {
            _service = service;
        }

        [HttpPost("GetSiblingListData")]
        public async Task<IActionResult> GetSiblingList([FromBody] SiblingListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetSiblingList(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SiblingListResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<SiblingListResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<SiblingListResponse>>
                {
                    Success = true,
                    Message = "Sibling list retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the sibling list.",
                    Code = -1
                });
            }
        }
        [HttpPost("SiblingsData")]
        public async Task<IActionResult> GetSiblings([FromBody] SiblingListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetSiblings(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SiblingListResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<SiblingListResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<SiblingListResponse>>
                {
                    Success = true,
                    Message = "Siblings retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the siblings.",
                    Code = -1
                });
            }
        }

    }
}
