using ApplicationInterface.SchoolMaster;
using ApplicationInterface.SuperAdmin;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.SchoolMaster;
using DomainModel.Admin;
using Infrastructure.SchoolMaster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using Microsoft.AspNetCore.Authorization;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAdminController : ControllerBase
    {
        private readonly IRoleAdminMaster _repo;
        public RoleAdminController(IRoleAdminMaster repo)
        {
            _repo = repo;
        }

        [HttpPost("Add_Role")]
        public async Task<IActionResult> AddRole(SuperAdminDomain Role)
        {
            try
            {
                await _repo.AddRoleData(Role);

                return Ok(
                    ApiResponse<string>.Ok("Country created successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

        [HttpGet("GetAdd_Role")]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var result = await _repo.GetAddRole();

                if (result == null || !result.Any())
                {
                    return Ok(ApiResponse<List<SuperAdminDomain>>.Fail("No roles found")
                    );
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

        [HttpPost("Add_RoleEdit")]
        public async Task<IActionResult> Add_RoleEdit(SuperAdminDomain Role)
        {
            try
            {
                int result = await _repo.Add_RoleEditData(Role);

                if (result == -1)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Role already exists!")
                    );
                }

                if (result > 0)
                {
                    return Ok(
                        ApiResponse<string>.Ok("Role Updated successfully!")
                    );
                }

                return StatusCode(
                    500,
                    ApiResponse<string>.Fail("Something went wrong")
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    ApiResponse<string>.Fail("Something went wrong")
                );
            }
        }
        [HttpPost("Add_RoleDelete")]
        public async Task<IActionResult> Delete([FromBody] int RoleId)
        {
            try
            {
                if (RoleId == 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _repo.Add_RoleDeleteData(RoleId);

                return Ok(
                    ApiResponse<string>.Ok("Role status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
