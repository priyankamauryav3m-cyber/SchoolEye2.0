using ApplicationInterface.FinanceMNGT.FeeMNGT;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using static DomainModel.FinanceMNGT.FeeCollectionDuesNew;

namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentFeeController : ControllerBase
    {
        private readonly IStudentFeeRepository _studentFeeRepository;

        public StudentFeeController(IStudentFeeRepository studentFeeRepository)
        {
            _studentFeeRepository = studentFeeRepository;
        }

        [HttpPost("GetStudentDetailsForFee")]
        public async Task<IActionResult> GetStudentDetailsForFee([FromBody] StudentDetailsForFeeRequest request)
        {
         
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<List<StudentDetailsForFee>>
                    {
                        Code = 0,
                        Message = "Invalid request.",
                        Data = null
                    });
                }

                var response = await _studentFeeRepository.GetStudentDetailsForFeeAsync(request);

                if (response == null)

                    return NotFound(ApiResponse<string>.Fail("No student List  found"));
                return Ok(new ApiResponse<IEnumerable<StudentDetailsForFee>>
                {
                    Success = true,
                    Data = response
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }

        [HttpPost("GetStudentFeeCollectionDues")]
        public async Task<IActionResult> GetStudentDetailsForFeeCollection([FromBody] GetStudentFeeHeadDuesForAdjustmentRequest request)
        {

            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<List<FeeCollectionDuesNew>>
                    {
                        Code = 0,
                        Message = "Invalid request.",
                        Data = null
                    });
                }

                var response = await _studentFeeRepository.GetStudentDetailsForFeeDue(request);

                if (response == null)

                    return NotFound(ApiResponse<string>.Fail("No  List  found"));
                return Ok(new ApiResponse<IEnumerable<FeeCollectionDuesNew>>
                {
                    Success = true,
                    Data = response
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }
        [HttpPost("AdjustStudentFeeCollection")]
        public async Task<IActionResult> AdjustStudentFeeHeadWiseData([FromBody] AdjustStudentFeeHeadWiseRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }

                var result = await _studentFeeRepository.AdjustStudentFeeHeadWise(request);

                if (result == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fee adjustment failed."
                    });
                }

                if (string.IsNullOrWhiteSpace(result.StudentReceiptNo))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Receipt could not be generated."
                    });
                }

                return Ok(new ApiResponse<AdjustStudentFeeHeadWiseResponse>
                {
                    Success = true,
                    Message = "Fee adjusted successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("StudentFeeHeadCollectionRecept")]
        public async Task<IActionResult> AdjustStudentFeeHeadWise([FromBody] FeeReceiptRequest request)
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
                var result = await _studentFeeRepository.AdjustStudentFeeHeadWise(request);
                return Ok(new ApiResponse<ReceiptResponse>
                {
                    Success = true,
                    Message = "Fee adjustment receipt generated successfully.",
                    Code = 1,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating the fee adjustment receipt.",
                    Code = -1
                });
            }
        }

    }
}
