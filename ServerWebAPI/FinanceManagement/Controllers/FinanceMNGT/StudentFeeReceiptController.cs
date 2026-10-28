using ApplicationInterface.FinanceMNGT.FeeMNGT;
using ApplicationInterface.GenerateFile;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using SchoolEye.ApplicationInterface.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentFeeReceiptController : ControllerBase
    {
        private readonly IStudentFeeReceiptRepository _service;
        private readonly IPrintStudentRec _generateFile;

        public StudentFeeReceiptController(IStudentFeeReceiptRepository service, IPrintStudentRec generateFile = null)
        {
            _service = service;
            _generateFile = generateFile;
        }

        [HttpPost("GetStudentFeeReceiptListData")]
        public async Task<IActionResult> GetStudentFeeReceiptList([FromBody] StudentFeeReceiptListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetStudentFeeReceiptList(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentFeeReceiptListResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentFeeReceiptListResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentFeeReceiptListResponse>>
                {
                    Success = true,
                    Message = "Student fee receipt list retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the student fee receipt list.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetClassGroup")]
        public async Task<IActionResult> GetClassGroup([FromBody] ClassGroupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Group code and branch code are required.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetClassGroup(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<ClassGroupResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<ClassGroupResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<ClassGroupResponse>>
                {
                    Success = true,
                    Message = "Class group retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the class group.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetStudentReceiptById")]
        public async Task<IActionResult> GetStudentReceiptById([FromBody] StudentReceiptByIdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            try
            {
                var data = await _service.GetStudentReceiptById(request);
                if (data == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "Receipt record was not found.", Code = 0 });
                }
                return Ok(new ApiResponse<StudentReceiptByIdResponse>
                {
                    Success = true,
                    Message = "Receipt retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the receipt.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetReceiptDetail")]
        public async Task<IActionResult> GetReceiptDetail([FromBody] StudentReceiptByIdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            try
            {
                var data = await _service.GetReceiptDetail(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<ReceiptDetailResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<ReceiptDetailResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<ReceiptDetailResponse>>
                {
                    Success = true,
                    Message = "Receipt details retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the receipt details.",
                    Code = -1
                });
            }
        }
        [HttpPost("GenerateStudentReceiptHeadWisePdf")]
        public async Task<IActionResult> GenerateStudentReceiptHeadWisePdf([FromBody] StudentReceiptHeadWiseRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Invalid request."
                });
            }

            if (request.FromDate == default || request.ToDate == default ||request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Please provide a valid date range."
                });
            }

            try
            {

                
                var data = await _service.GetWithModeReceiptHeadWiseDetail(request);

                var pdfBytes = await _generateFile.GenerateStudentReceiptHeadWisePdf(request, data);

                var base64Pdf = Convert.ToBase64String(pdfBytes);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = base64Pdf,
                    Message = "Student Receipt Head-Wise PDF generated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = $"PDF generation failed: {ex.Message}"
                });
            }
        }

        [HttpPost("GenerateStudentReceiptHeadWiseExcel")]
        public async Task<IActionResult> GenerateStudentReceiptHeadWiseExcel([FromBody] StudentReceiptHeadWiseRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Invalid request."
                });
            }

            if (request.FromDate == default ||
                request.ToDate == default ||
                request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Please provide a valid date range."
                });
            }

            try
            {
                var data = await _service.GetWithModeReceiptHeadWiseDetail(request);

                var excelBytes = await _generateFile
                    .GenerateStudentReceiptHeadWiseExcel(request, data);

                var base64Excel = Convert.ToBase64String(excelBytes);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = base64Excel,
                    Message = "Student Receipt Head-Wise Excel generated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = $"Excel generation failed: {ex.Message}"
                });
            }
        }

    }
}
