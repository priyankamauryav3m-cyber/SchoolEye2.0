using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using SchoolEye.ApplicationInterface.Admin;


namespace SchoolEye.ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentMiscellaneousController : ControllerBase
    {
        private readonly IStudentMiscellaneousRepository _service;

        public StudentMiscellaneousController(IStudentMiscellaneousRepository service)
        {
            _service = service;
        }

        [HttpPost("GetStudentMiscellaneousData")]
        public async Task<IActionResult> GetStudentMiscellaneous([FromBody] MiscellaneousRequest request)
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
                var data = await _service.GetStudentMiscellaneous(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<MiscellaneousResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<MiscellaneousResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<MiscellaneousResponse>>
                {
                    Success = true,
                    Message = "Students retrieved successfully.",
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
                    Message = "An error occurred while fetching the students.",
                    Code = -1
                });
            }
        }

        [HttpPost("GenerateMiscellaneousChallan")]
        public async Task<IActionResult> GenerateMiscellaneousChallan([FromBody] GenerateMiscChallanRequest request)
        {
            if (request == null || request.Students == null || request.Students.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select at least one student.",
                    Code = 0
                });
            }
            if (request.FeeHeads == null || request.FeeHeads.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please add at least one fee head.",
                    Code = 0
                });
            }
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Narration is required.",
                    Code = 0
                });
            }
            if (request.FeeHeads.Any(x => x.Amount <= 0))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fee head amount must be greater than 0.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GenerateMiscellaneousChallan(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<GenerateMiscChallanResponse>>
                    {
                        Success = false,
                        Message = "Challan not generated.",
                        Code = 0,
                        Data = Enumerable.Empty<GenerateMiscChallanResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<GenerateMiscChallanResponse>>
                {
                    Success = true,
                    Message = "Challan generated successfully.",
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
                    Message = "An error occurred while generating the challan.",
                    Code = -1
                });
            }
        }

        //[HttpPost("GetFeeHeadForCombo")]
        //public async Task<IActionResult> GetFeeHeadForCombo([FromBody] FeeHeadForComboRequest request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Data not found.",
        //            Code = 0
        //        });
        //    }

        //    try
        //    {
        //        var data = await _service.GetFeeHeadForCombo(request);

        //        if (data == null || !data.Any())
        //        {
        //            return Ok(new ApiResponse<IEnumerable<FeeHeadForComboResponse>>
        //            {
        //                Success = true,
        //                Message = "No records found.",
        //                Code = 0,
        //                Data = Enumerable.Empty<FeeHeadForComboResponse>()
        //            });
        //        }

        //        return Ok(new ApiResponse<IEnumerable<FeeHeadForComboResponse>>
        //        {
        //            Success = true,
        //            Message = "Fee heads retrieved successfully.",
        //            Code = 1,
        //            Data = data
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "An error occurred while fetching the fee heads.",
        //            Code = -1
        //        });
        //    }
        //}
    }
}
