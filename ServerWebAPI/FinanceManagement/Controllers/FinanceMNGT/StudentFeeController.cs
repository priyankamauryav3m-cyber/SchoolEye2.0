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
    [Authorize]
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
    }
}
