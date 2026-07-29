using ApplicationInterface.FinanceMNGT;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using DomainModel.Resources.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.VisualBasic;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConcessionController : ControllerBase
    {
        private readonly IConcessionRepository _concessionRepository;
        private readonly IStringLocalizer<Resource> _localizer;
        public ConcessionController(IConcessionRepository concessionRepository, IStringLocalizer<Resource> localizer)
        {
            _concessionRepository = concessionRepository;
            _localizer = localizer;
        }
        [HttpPost]
        [Route("AddOrUpdateConcession")]
        public async Task<IActionResult> AddUpdateconcessionAsync([FromBody] ConcessionModel concession)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
            var result = await _concessionRepository.AddUpdateConcession(concession);
            return result switch
            {
                "0" => Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = " Concession(name already exists)",
                    Code = 0
                }),
                "1" => Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = " Concession( inserted successfully)",
                    Code = 1
                }),
                "2" => Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = " Concession( updated successfully)",
                    Code = 2
                }),

                "-3" => Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Database error occurred."
                }),
                _ => Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unknown operation result"
                })
            };
        }


        [HttpPost]
        [Route("DeleteConcession")]
        public async Task<IActionResult> DeleteconcessionAsync([FromBody] int Cid)
        {
            try
            {
                if (Cid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _concessionRepository.DeleteConcessionData(Cid);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet]
        [Route("GetListConcession")]
        public async Task<IActionResult> ListconcessionAsync()
        {
            try
            {
                var concessionList = await _concessionRepository.GetConcessionData();
                if (concessionList == null || !concessionList.Any())
                    return NotFound(ApiResponse<string>.Fail("No concession List found"));
                return Ok(concessionList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("GetStudentMapConcession")]
        public async Task<IActionResult> GetStudentTransportDetailsData([FromBody] StudentConcessionFilterDto model)
        {

            try
            {
                var result = await _concessionRepository.GetStudentWithConcessionAsync(model);
                return Ok(new ApiResponse<IEnumerable<StudentWithConcessionDto>>
                {
                    Success = true,
                    Data = result
                });

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
        [HttpPost("GetStudentData")]
        public async Task<IActionResult> GetStudentAllStudent([FromBody] SearchAnyRequestModel searchAny)
        {

            try
            {
                var result = await _concessionRepository.GetSearchStudent(searchAny);
                return Ok(new ApiResponse<IEnumerable<StudentWithConcessionDto>>
                {
                    Success = true,
                    Data = result
                });

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
        [Route("add-or-update-feehead-concession")]
        public async Task<IActionResult> AddOrUpdateFeeheadConcessionData([FromBody] List<ConcessionFeehead> model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request"));
            var result = await _concessionRepository.AddOrUpdateFeeheadConcessionData(model);
            var results = result.Split(',').Select(x => x.Trim()).ToList();
            if (results.Any(x => x == "0"))
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Some records failed to save",
                    Code = -1
                });
            }
            int inserted = results.Count(x => x == "1");
            int updated = results.Count(x => x == "2");
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Data mapped successfully",
                Code = 1
            });
        }
        [HttpPost("AddStudentConcession")]
        public async Task<IActionResult> SaveStudentConcessionData([FromBody] StudentConcessionDto list)
        {
            if (!ModelState.IsValid || list == null)
                return BadRequest(ApiResponse<string>.Fail("Invalid request"));

            try
            {
                var result = await _concessionRepository.SaveStudentConcession(list);

                return result switch
                {
                    "1" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Concession inserted successfully",
                        Code = 1
                    }),
                    "-2" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Concession con't accept 100%",
                        Code = -2
                    }),
                    _ => Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = result ?? "Unknown operation result",
                        Code = -1
                    })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(ex.Message));
            }
        }
        [HttpPost]
        [Route("UpdateStudentConcessionRemark")]
        public async Task<IActionResult> UpdateStudentConcessionRemarks([FromBody] StudentConcessionRemarks concession)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request"));
            var result = await _concessionRepository.UpdateStudentConcessionRemarksData(concession);
            if (result == 1)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Student Concession updated successfully",
                    Code = 1
                });
            }
            else if (result == -1)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error occurred while updating",
                    Code = -1
                });
            }
            else
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No record found",
                    Code = 0
                });
            }
        }

        [HttpPost("manage-concession-approval")]
        public async Task<IActionResult> ManageConcessionApproval([FromBody] ConcessionManageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request"));

            var result = await _concessionRepository.ManageConcessionAsync(request);

            if (result == 2)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Concession approved successfully",
                    Code = 2
                });
            }
            else if (result == -1)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error occurred while processing",
                    Code = -1
                });
            }
            else
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No record found",
                    Code = 0
                });
            }
        }
        [HttpPost("GetStudentMappedConcession")]
        public async Task<IActionResult> GetStudentMapConStudent([FromBody] SearchAnyRequestModel searchAny)
        {

            try
            {
                var result = await _concessionRepository.GetStudentMappedConcession(searchAny);
                return Ok(new ApiResponse<IEnumerable<StudentMappedConcessionDto>>
                {
                    Success = true,
                    Data = result
                });

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
        [HttpPost("UnMapConcession")]
        public async Task<IActionResult> UnMapConcession([FromBody] UnMapConcessionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        Status = false,
                        Message = "Invalid Request"
                    });
                }
                var result = await _concessionRepository.UnMapConcessionWithStudentAsync(request);
                if (result ==2)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Concession UnMapped Successfully,",
                        Code = 2
                    });
              
                }
                return Ok(new
                {
                    Status = false,
                    Message = "Failed To UnMap Concession"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

    }
}

