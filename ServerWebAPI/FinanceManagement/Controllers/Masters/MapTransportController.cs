using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using System.Net.Sockets;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MapTransportController : ControllerBase
    {
        private readonly IMapTransportRepository _repository;

        public MapTransportController(IMapTransportRepository repository)
        {
            _repository = repository;
        }
        [HttpPost("GetAllStudentData")]
        public async Task<IActionResult> GetStudentData(TransportSearchModel transportSearch)
        {
            try
            {
                var data = await _repository.GetTransportStudentDataAsync(transportSearch);
                return Ok(new ApiResponse<IEnumerable<TransportStudentDataModel>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                   // Message = _localizer["Error"]
                });
            }
           
        }
        [HttpPost("GetStudentMappedTransport")]
        public async Task<IActionResult> GetStudentTransport(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var data = await _repository.GetStudentTransportData(searchAnyRequest);
                return Ok(new ApiResponse<IEnumerable<StudentTransportMappedModel>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                 
                });
            }
         
        }
        [HttpPost("GetStudentTransportRoute")]
        public async Task<IActionResult> GetStudentRouteData(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var data = await _repository.GetStudentTransporRoutetData(searchAnyRequest);
                return Ok(new ApiResponse<IEnumerable<TransportRoute>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,

                });
            }

        }
        [HttpPost("GetStudentBoardingPoint")]
        public async Task<IActionResult> GetStudentPointData(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var data = await _repository.GetBoardingPoints(searchAnyRequest);
                return Ok(new ApiResponse<IEnumerable<TransportRoutePoint>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,

                });
            }

        }
      


        [HttpPost("AddOrUpdateTransportMapMonthConfig")]
        public async Task<IActionResult> AddOrUpdateTransportMapMonthData([FromBody] TransportRequestModel transport)
        {
            try
            {
                if (transport == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data."
                    });
                }
                var result =
                    await _repository.AddOrUpdateTransportMapMonthData(transport);

                if (result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Transport configuration saved successfully.",
                        Code = 1
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to save transport configuration.",
                    Code = 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }

    }
}
