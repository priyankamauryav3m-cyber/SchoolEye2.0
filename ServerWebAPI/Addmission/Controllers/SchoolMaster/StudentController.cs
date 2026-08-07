using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _service;
        private readonly IWebHostEnvironment _env;

        public StudentController(IStudentService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // ---------------------------------------------------------
        // ADD / UPDATE
        // ---------------------------------------------------------
        [HttpPost("AddOrUpdateStudent")]
        public async Task<IActionResult> AddUpdateStudent([FromBody] StudentModel objStudent)
        {
            if (objStudent == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = string.Join(" | ", errors)
                });
            }

            try
            {
                var result = await _service.AddUpdateStudent(objStudent);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student updated successfully",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred"
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

        // ---------------------------------------------------------
        // GET (studentId = 0 returns all)
        // ---------------------------------------------------------
        [HttpGet("GetStudent")]
        public async Task<IActionResult> GetStudent(int studentId = 0)
        {
            try
            {
                var data = await _service.GetStudent(studentId);
                return Ok(new ApiResponse<List<StudentModel>>
                {
                    Success = true,
                    Message = "Data fetched successfully",
                    Data = data
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while fetching records."
                    });
            }
        }

        // ---------------------------------------------------------
        // DELETE / ACTIVATE / DEACTIVATE  (isActive must be 0 or 1)
        // ---------------------------------------------------------
        [HttpPost("DeleteStudent")]
        public async Task<IActionResult> DeleteStudent(int studentId, bool isActive)
        {
            try
            {
                var result = await _service.DeleteStudent(studentId, isActive);
                return Ok(new ApiResponse<object>
                {
                    Success = result,
                    Message = result
                        ? (isActive ? "Student activated successfully" : "Student deactivated successfully")
                        : "Student not found"
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while deleting record."
                    });
            }
        }

        // ---------------------------------------------------------
        // PHOTO UPLOAD (Optional. JPG or PNG, up to 2MB)
        // ---------------------------------------------------------
        [HttpPost("UploadPhoto")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No file uploaded."
                });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Only JPG or PNG files are allowed."
                });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "File size must not exceed 2MB."
                });

            try
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Photo uploaded successfully",
                    Data = $"/uploads/students/{fileName}"
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while uploading the photo."
                    });
            }
        }
    }
}
