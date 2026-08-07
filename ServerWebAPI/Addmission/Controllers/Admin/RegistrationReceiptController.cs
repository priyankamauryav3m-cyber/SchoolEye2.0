using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
     [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationReceiptController : ControllerBase
    {
        private readonly IRegistrationReceiptRepository _repository;

        public RegistrationReceiptController(IRegistrationReceiptRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("GetRegistrationReceipt")]
        public async Task<IActionResult> GetRegistrationReceipt([FromBody] RegistrationReceiptRequest request)
        {
            try
            {
                var data = await _repository.GetRegistrationReceiptAsync(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<RegistrationReceiptResponse>>
                    {
                        Success = false,
                        Message = "No Record Found.",
                        Data = Enumerable.Empty<RegistrationReceiptResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<RegistrationReceiptResponse>>
                {
                    Success = true,
                    Message = "Data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<RegistrationReceiptResponse>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
