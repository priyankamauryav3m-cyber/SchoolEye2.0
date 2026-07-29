using ApplicationInterface.FinanceMNGT.FeeMNGT;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PromoteConStudentController : ControllerBase
    {
        private readonly IPromoteConStRepository _service;
        public PromoteConStudentController(IPromoteConStRepository service)

        {

            _service = service;

        }

        [HttpPost]
        [Route("PromotionConcessionStudent")]
        public async Task<IActionResult> GetPromotionConcessionStudent(PromoteStudent requestModel)
        {

            try
            {

                var resultlist = await _service.GetPromotionConcessionStudent(requestModel);

                if (resultlist == null || !resultlist.Any())

                    return NotFound(ApiResponse<string>.Fail("No Pramote List  found"));
                return Ok(new ApiResponse<IEnumerable<StudentResponse>>
                {
                    Success = true,
                    Data = resultlist
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }
        [HttpPost]
        [Route("PromoteStudentConcession")]
        public async Task<IActionResult> PromoteStudentConcessionData(PromoteConcessionRequest request)
        {

            var result = await _service.PromoteStudentConcession(request);

            if (result.ResultCode == 1)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Student concession promoted successfully.",
                    Code = 1
                });
            }
            else if (result.ResultCode == 3)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Some students are not available in the next session.",
                    Code = 3,
                    Data = result.MissingStudents
                });
            }

            else
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unable to promote concession.",
                    Code = 0
                });
            }
        }
    }
}
