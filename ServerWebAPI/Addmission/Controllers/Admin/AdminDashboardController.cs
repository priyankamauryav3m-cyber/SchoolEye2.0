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
       // [Authorize]
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
        [HttpPost("GetEnquiryDashboardData")]
        public async Task<IActionResult> GetDashboard(EnquiryDashboardSearchRequest searchAnyRequest)
        {
            var result = await _service.GetDashboardAsync(searchAnyRequest);

            return Ok(result);
        }
        [HttpPost("GetFeeManagementDashboard")]
        public async Task<ApiResponse<FeeManagementDashboardModel>> GetFeeManagementDashboard([FromBody] AdmissionDashboardRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode) || request.SessionId <= 0)
            {
                return new ApiResponse<FeeManagementDashboardModel>
                {
                    Success = false,
                    Code = 0,
                    Message = "Please select a valid session."
                };
            }

            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate.Value.Date > request.ToDate.Value.Date)
            {
                return new ApiResponse<FeeManagementDashboardModel>
                {
                    Success = false,
                    Code = 0,
                    Message = "From date cannot be after To date."
                };
            }

            try
            {
                var data = await _service.GetFeeManagementDashboard(request);
                return new ApiResponse<FeeManagementDashboardModel>
                {
                    Success = true,
                    Code = 1,
                    Message = "Fee dashboard data fetched successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetFeeManagementDashboard failed: {ex.Message}");
                return new ApiResponse<FeeManagementDashboardModel>
                {
                    Success = false,
                    Code = 0,
                    Message = "Unable to load the fee dashboard right now. Please try again."
                };
            }
        }

        [HttpPost("GetAdmissionDashboardData")]
        public async Task<ApiResponse<AdmissionDashboardModel>> GetAdmissionDas([FromBody] AdmissionDashboardRequest request)
        {
            try
            {
                var data = await _service.GetAdmissionHeadDashboard(request);
                return new ApiResponse<AdmissionDashboardModel>
                {
                    Success = true,
                    Code = 1,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AdmissionDashboardModel>
                {
                    Success = false,
                    Code = 0,
                    Message = ex.Message
                };
            }
        }
    }
}
