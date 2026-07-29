using ApplicationInterface.FinanceMNGT;
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
    public class ViewTemplateFeeHeadController : ControllerBase
    {
        private readonly IViewTemplateFeeHeadRepository _service;
        public ViewTemplateFeeHeadController(IViewTemplateFeeHeadRepository service)
        {
            _service = service;
        }

        [HttpPost("GetFeeHeadsMappedWithTemplateList")]
        public async Task<IActionResult> GetFeeHeadsMappedWithTemplateList([FromBody] FeeHeadTemplateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data.");
                var result = await _service.GetFeeHeadsMappedWithTemplateList(request);
                if (result == null || !result.Any())
                    return NotFound("No records found.");
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
        [HttpPost]
        [Route("DeleteMapFeeHead")]
        public async Task<IActionResult> DeleteFeeHead([FromBody] int FeeHeadId)
        {
            try
            {
                if (FeeHeadId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _service.DeleteFeeMapTemplateData(FeeHeadId);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("SaveFeeTemplateFeeHead")]
        public async Task<IActionResult> SaveFeeTemplateFeeHead([FromBody] ClassFeeHeadsModel request)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.SaveFeeTemplateFeeHeads(request);
                return returnValue switch
                {
                    "-1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate inserted successfully)",
                        Code = 1
                    }),
                    "-2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(duplicate fee head template )",
                        Code = -2
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate updated successfully)",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating record."
                    });
            }
        }
        [HttpPost("GetFeeHeadTemplatesList")]

        public async Task<IActionResult> GetFeeHeadTemplatesList([FromBody] FeeHeadTemplateRequest request)
        {

            if (request == null)

                return BadRequest("Invalid request.");

            try

            {

                var result = await _service.GetFeeHeadTemplatesList(request);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new

                {

                    Message = "An error occurred while fetching fee head templates list.",

                    Error = ex.Message

                });

            }

        }

    }
}
