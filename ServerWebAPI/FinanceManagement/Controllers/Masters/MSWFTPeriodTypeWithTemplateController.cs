using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
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
    public class MSWFTPeriodTypeWithTemplateController : ControllerBase
    {

        private readonly IMSWFTPeriodTypeWithTemplateRepository _service;
        public MSWFTPeriodTypeWithTemplateController(IMSWFTPeriodTypeWithTemplateRepository service)
        {
            _service = service;
        }

        [HttpPost("GetIMSWFTPeriodType")]
        public async Task<IActionResult> GetIMSWFTPeriodTypeData([FromBody] IMSWFTPeriodType model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid request.");
                var result = await _service.GetIMSWFTPeriodTypeData(model);
                if (result == null || result.Count == 0)
                    return NotFound("No record found.");
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



      
        [HttpPost("GetClassSection")]
        public async Task<IActionResult> GetClassSection([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Invalid request data."
                    });
                }

                var result = await _service.GetClassSection(model);

                return Ok(new ApiResponse<IEnumerable<SectionModel>>
                {
                    Success = true,
                    Message = "Section list fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("MapFeePeriodWithStudent")]
        public async Task<IActionResult> MapFeePeriodWithStudent([FromBody] MapFeePeriodWithStudentModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request data."
                    });
                }
                var result = await _service.MapFeePeriodWithStudent(model);
                if (result)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Fee period mapped successfully.",
                        Code=1
                        
                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to map fee period."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("MapFeeTemplateWithStudent")]
        public async Task<IActionResult> MapFeeTemplateWithStudent([FromBody] MapFeePeriodWithStudentModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request data."
                    });
                }

                bool result = await _service.MapFeeTemplateWithStudent(model);

                if (result)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Template mapped successfully.",
                        Code = 1

                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to map fee period."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}



