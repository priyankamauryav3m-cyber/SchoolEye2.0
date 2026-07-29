using ApplicationInterface.SuperAdmin;
using DomainModel.Admin;
using DomainModel.Resources;
using DomainModel.Resources.Resource;
using Infrastructure.SchoolMaster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using MyApp.Common;
using ServerWebAPI.Authorization;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRepository _service;
        private readonly IStringLocalizer<Resource> _localizer;
        public RegistrationController(IRegistrationRepository service, IStringLocalizer<Resource> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        [HttpPost("SearchRegistration")]
        public async Task<IActionResult> SearchRegistrationData([FromBody] RegistrationSearchDto search)
        {
            try
            {   
                var data = await _service.SearchAsync(search);
                return Ok(new ApiResponse<IEnumerable<RegistrationDto>>
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
                    Message = _localizer["Error"]
                });
            }   
        }

        [HttpPost("AddRegistration")]
        public async Task<IActionResult> StudentRegistration([FromBody] RegistrationModal res)
        {
            if (res == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["InvalidData", "registration"]
                });
            }
            try
            {
                var registrationNo = await _service.AddStudentRegistration(res);
                if (string.IsNullOrWhiteSpace(registrationNo))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Registration failed.",
                        Code = 0
                    });
                }
                return StatusCode(StatusCodes.Status201Created, new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["Submit"],
                    Code = 1,
                    Data = registrationNo

                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while processing registration."
                    });
            }
        }
        [HttpPost("registrationchildupdate")]
        public async Task<IActionResult> RegistrationChildDetails([FromBody] ChildDetails res)
        {
            if (res == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["InvalidData", "Child"]
                });
            }

            try
            {
                string result = await _service.RegistrationChildDetails(res);

                if (result == "No Record Updated")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = result,
                        Code = 0
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["Updated"],
                    Code = 1
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = _localizer["Error"]
                    });
            }
        }

        [HttpGet("get-registration-childParent")]
        public async Task<IActionResult> GetRegistrationParentsChildData([FromQuery] string groupCode, [FromQuery] string branchCode, [FromQuery] long sessionId, [FromQuery] long registrationId)
        {
            try
            {
                if (registrationId==0)
                    return BadRequest(new { message = _localizer["RequiredField", "RegistrationNo"] });

                var data = await _service.GetRegistrationParentsChildById(groupCode, branchCode, sessionId,  registrationId);
                if (data == null)
                    return NotFound(new { message = _localizer["NoData"] });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = _localizer["Error"] });
            }
        }

        [HttpPost("registrationInformation")]
        public async Task<IActionResult> RegistrationAdditionalInformation([FromBody] PointsCriteria Cre)
        {
            if (Cre == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["InvalidData", "information"]
                });
            }
            try
            {
                string result = await _service.RegistrationAdditionalInformation(Cre);

                if (result == "No Record Updated")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = _localizer["Updated"],
                        Code = 0
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result,
                    Code = 1
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = _localizer["Error"]
                    });
            }
        }

        [HttpGet("get-registration-otherInformation")]
        public async Task<IActionResult> GetRegistrationChildotherInformation([FromQuery] string groupCode, [FromQuery] string branchCode, [FromQuery] long sessionId, [FromQuery] long registrationId)
        {
            try
            {
                if (registrationId==0)
                    return BadRequest(new { message = _localizer["RequiredField", "RegistrationNo"] });
                var data = await _service.GetRegistrationChildotherInformation(groupCode, branchCode, sessionId, registrationId);
                if (data == null)
                    return NotFound(new
                    {
                        message = _localizer["RecordNotFound", "Record"]
                    });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = _localizer["Error"] });
            }
        }

        [HttpPost("registrationAddressInfo")]
        public async Task<IActionResult> RegistrationAddressInformation([FromBody] AddressDetails Add)
        {
            if (Add == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["InvalidData", "information"]
                });
            }
            try
            {
                string result = await _service.RegistrationAddressInformation(Add);

                if (result == "No Record Updated")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = result,
                        Code = 0
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["Updated"],
                    Code = 1
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = _localizer["Error"]
                    });
            }
        }

        [HttpGet("get-registration-AddressInformation")]
        public async Task<IActionResult> GetRegistrationChildAddressInformation([FromQuery] string groupCode, [FromQuery] string branchCode, [FromQuery] long sessionId, [FromQuery] long registrationId, [FromQuery] string AddressType)
        {
            try
            {
                if (registrationId==0)
                    return BadRequest(new { message = _localizer["RequiredField", "RegistrationNo"] });
                var data = await _service.GetRegistrationChildAddressInformation(groupCode, branchCode, sessionId, registrationId, AddressType);
                if (data == null)
                {
                    return NotFound(new ApiResponse<AddressDetails>
                    {
                        Success = true,
                        Message = _localizer["RecordNotFound", "Record"],
                        Data = null
                    });
                }
                return Ok(new ApiResponse<AddressDetails>
                {
                    Success = true,
                    Message = "Registration  get Success",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = _localizer["Error"]
                });
            }
        }

        [HttpGet("get-registration-child")]
        public async Task<IActionResult> GetRegistrationChildData([FromQuery] string groupCode, [FromQuery] string branchCode, [FromQuery] long sessionId, [FromQuery] long registrationId)
        {
            try
            {
                if (registrationId==0)
                    return BadRequest(new { message = _localizer["RequiredField", "RegistrationNo"] });
                var data = await _service.GetRegistrationChildData(groupCode, branchCode, sessionId, registrationId);
                if (data == null)
                    return NotFound(new { message = _localizer["RecordNotFound", "Record"] });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = _localizer["Error"] });
            }
        }

        [HttpPost("InsertFamilyDetails")]
        public async Task<IActionResult> SubmitFamilyInfoDetails([FromBody] FamilyDetails res)
        {

            if (res == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["InvalidData", "registration"]

                });
            }
            try
            {
                var registrationNo = await _service.SubmitFamilyInfoDetails(res);
                if (string.IsNullOrWhiteSpace(registrationNo))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "failed.",
                        Code = 0
                    });
                }
                return StatusCode(StatusCodes.Status201Created, new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["Inserted"],
                    Code = 1,
                    Data = registrationNo
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while processing registration."
                    });
            }
        }
        [HttpPost("registration-parent-detail")]
        public async Task<IActionResult> RegistrationParentDetails([FromBody] ParentsDetails pd)
        {
            try
            {
                if (pd == null)
                    return BadRequest("Invalid request body.");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                string result = await _service.RegistrationParentDetails(pd);
                if (result == "No Record Updated")
                    return NotFound(result);
                return Ok(new { message = _localizer["Updated"], });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = _localizer["Error"] });
            }
        }
        [HttpGet("get-registration-FamilyDetails")]
        public async Task<IActionResult> GetRegistrationFamilyDetails([FromQuery] string groupCode, [FromQuery] string branchCode, [FromQuery] long sessionId, [FromQuery] long registrationId)
        {
            try
            {
                if (registrationId==0)
                    return BadRequest(new { message = _localizer["RequiredField", "RegistrationNo"] });
                var data = await _service.GetRegistrationFamilyDetails(groupCode, branchCode, sessionId, registrationId);
                if (data == null)
                    return NotFound(new { message = _localizer["RecordNotFound", "Record"] });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = _localizer["Error"] });
            }
        }

        [HttpPost("StudentDirectAdmission")]
        public async Task<IActionResult> StudentDirectAdmission([FromBody] StudentDirectAdmissionModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid Data");
                var result = await _service.StudentDirectAdmissionData(model);
                if (result == "-1")
                    return BadRequest("Admission Failed");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admission Successful",
                    Code = 1,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetStudentDetails")]
        public async Task<IActionResult> GetStudentDetailsData(string groupCode, string branchCode, long SessionId, string RegistrationId, string AddressType)
        {
            if (string.IsNullOrWhiteSpace(groupCode) ||string.IsNullOrWhiteSpace(branchCode))
            {
                return BadRequest("GroupCode and BranchCode are required.");
            }
            var data = await _service.GetStudentDetailsData(groupCode, branchCode, SessionId, RegistrationId, AddressType);
            if (data == null)
                return NotFound(_localizer["RecordNotFound", "student"]);
            return Ok(data);
        }

        [HttpPost("StudentList")]
        public async Task<IActionResult> AdmintStudentListData([FromBody] StudentListRequest request)
        {
            try
            {
                var result = await _service.AdmintStudentListData(request);
                var response = new ApiResponse<IEnumerable<StudentListResponse>>
                {
                    Success = true,
                    Message = "Student list fetched successfully.",
                    Data = result
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };

                return StatusCode(500, response);
            }
        }

        [HttpGet("SiblingDetail")]
        public async Task<IActionResult> GetSiblingDetail(string groupCode, string branchCode, int SessionId, string siblingID = null)
        {
            try
            {
                var result = await _service.GetSiblingDetail(groupCode, branchCode, SessionId, siblingID);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No sibling found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Sibling detail fetched successfully.",
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Data = null
                });
            }
        }

        [HttpGet("FeeHeadConcession")]
        public async Task<IActionResult> GetFeeHeadConcession(string groupCode, string branchCode, int SessionId, string concessionId, int isMappedOnly)
        {
            try
            {
                var result = await _service.GetFeeHeadConcession(
                    groupCode, branchCode, SessionId, concessionId, isMappedOnly);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPost("OnlineRegistration")]
        public async Task<IActionResult> OnlineRegistrationData([FromBody] OnlineRegistration online)
        {
            try
            {
                if (online == null)
                    return BadRequest("Invalid request");
                var result = await _service.InsertOnlineRegistration(online);
                return StatusCode(StatusCodes.Status201Created, new ApiResponse<object>
                {
                    Success = true,
                    Message = _localizer["Submit"],
                    Code = 1,
                    Data = result

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("RegistrationCancel")]
        public async Task<IActionResult> Registratiocancel([FromBody] CancelRegistration online)
        {
            try
            {
                int count = 0;
                if (online == null)
                {
                    return BadRequest("Invalid request");
                }
                var result = await _service.RegistrationCancel(online);
                if (result != null && !string.IsNullOrEmpty(result))
                {
                    count = 1;
                }
                return StatusCode(StatusCodes.Status200OK, new ApiResponse<object>
                {
                    Success = true,
                    Message = "",
                    Code = count,
                    Data = result

                });

            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost("RegistratioFormateType")]
        public async Task<IActionResult> RegistrationFormateType([FromBody] FormateType formate)
        {
            try
            {

                if (formate == null)
                    return BadRequest("Invalid request");
                var result = await _service.RFM_GetRegFormatTypeDate(formate);
                if (result != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<object>
                    {
                        Success = true,
                        Message = "",
                        Code = 1,
                        Data = result

                    });
                }

                return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<object>
                {
                    Success = false,
                    Message = "",
                    Code = 0,
                    Data = result

                });

            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost("AddOrUpdateFormateType")]
        public async Task<IActionResult> AddUpdateRegFormat([FromBody] CommonDomainLarge online)
        {
            try
            {
                if (online == null)
                    return BadRequest("Invalid request");
                var result = await _service.RFM_AddUpdateRegFormat(online);
                return result switch
                {
                    "-1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Regformate type already exists",
                        Code = -1

                    }),
                    "-2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "This class is already assigned to another format",
                        Code = -2
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Regformate type saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Regformate type updated successfully",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred"
                    })
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("ActiveFormate")]
        public async Task<IActionResult> ActiveFormateType([FromBody] int countryId)
        {
            try
            {
                if (countryId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.RFM_Active(countryId);

                return Ok(
                    ApiResponse<string>.Ok("Formate type status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
        [HttpPost("GetRegistrationDetails")]
        public async Task<IActionResult> GetRegistrationDetails(RegistrationSearchDto model)
        {
           
            try
            {
                var result = await _service.GetRegistrationDetails(model);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No registration found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "registration detail fetched successfully.",
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Data = null
                });
            }
        }
        [HttpPost("UpdateStudentStatus")]
        public async Task<IActionResult> updateStatusData(UpdateRegistrationStatusModel model)
        {
            try
            {
                var result = await _service.UpdateRegistrationStatus(model);
                if (result == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No registration found.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "registration detail fetched successfully.",
                    Code = 2
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Data = null
                });
            }
        }
    }
}
