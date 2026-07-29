using ApplicationInterface.Admin;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Infrastructure.Admin;
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
    public class AdminDashboardController : ControllerBase
    {

        private readonly IAdminDashboard _service;

        public AdminDashboardController(IAdminDashboard service)
        {
            _service = service;
        }

        [HttpPost("GetAdminDashboard")]
        public async Task<IActionResult> GetAdminDashboardData([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                var data = await _service.GetAdminDashboardData(model);

                return Ok(new ApiResponse<AdminDashboardModal>
                {
                    Success = true,
                    Message = "Dashboard data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message,
                        Data = null
                    });
            }
        }

        [HttpPost("GetHeadCollectionDashboard")]
        public async Task<IActionResult> GetFeeHeadDashboardData([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                var data = await _service.GetFeeHeadCollectionSummary(model);

                return Ok(new ApiResponse<IEnumerable<FeeHeadCollectionDto>>
                {
                    Success = true,
                    Message = "Dashboard Fee Head data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message,
                        Data = null
                    }
                );
            }
        }
        [HttpPost("GetAdmissionDas")]
        public async Task<IActionResult> GetAdmissionData([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                var data = await _service.GetAdmissionData(model);
                return Ok(new ApiResponse<AdmissionDashboardModel>
                {
                    Success = true,
                    Message = "Dashboard data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = false,
                    Message = "An error occurred while fetching dashboard data.",
                    Error = ex.Message,
                    Data = (object)null
                });
            }
        }

    }
}
