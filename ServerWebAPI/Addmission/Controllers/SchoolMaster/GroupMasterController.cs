using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupMasterController : ControllerBase
    {
        private readonly IGroupMasterRepository _service;

        public GroupMasterController(IGroupMasterRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateGroup")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddUpdateGroup([FromForm] GroupMaster objgroup)
        {
            if (objgroup == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                string logoPath = null;
                if (objgroup.Logo != null)
                {
                    var month = DateTime.Now.Month.ToString("D2"); 
                    var day = DateTime.Now.Day.ToString("D2");    

                    var root = Path.Combine(month, day);

                    if (!Directory.Exists(root))
                        Directory.CreateDirectory(root);

                    var fileName = Path.GetFileName(objgroup.Logo.FileName);
                    var filePath = Path.Combine(root, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await objgroup.Logo.CopyToAsync(stream);
                    }
                    logoPath = $"{month}/{day}_{fileName}";
                    objgroup.LogoPath = logoPath;
                }

                var returnValue = await _service.AddUpdateGroup(objgroup, logoPath);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Group name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Group added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Group updated successfully",
                        Code=2
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
        [HttpPost("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] int GroupId)
        {
            try
            {
                if (GroupId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteGroup(GroupId);

                return Ok(
                    ApiResponse<string>.Ok("Group status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

        [HttpGet("GroupMaster")]
        public async Task<IActionResult> GetGroupMaster()
        {
            try
            {
                var data = await _service.GetGroupMaster();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}

