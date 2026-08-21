using ApplicationInterface.GenerateFile;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Infrastructure.FileGenerate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using QuestPDF.Fluent;
using System.Threading.Tasks;

namespace ServerWebAPI.Addmission.Controllers.FileGenrate
{
    [ApiExplorerSettings(GroupName = "Admission")]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IPrintStudentRec _generateFile;
        private readonly HttpClient _http;

        public ExportController(IPrintStudentRec generateFile, HttpClient http = null)
        {
            _generateFile = generateFile;
            _http = http;
        }
        [HttpPost("GenerateStudentPdfData")]
        public async Task<IActionResult> GeneratePdf([FromBody] RegistrationDto hrList)
        {
            var pdfBytes = await _generateFile.GenerateStudentPdf(hrList);

            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDF generated successfully"
            });
        }
        [HttpPost("GenerateStudentPdf")]
        public async Task<IActionResult> GenerateListPdf([FromBody] List<RegistrationDto> hrList)
        {
            var pdfBytes = await _generateFile.GenerateStudentListPdf(hrList);
            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("GenerateStudentListExcel")]
        public async Task<IActionResult> GenerateStudentListExcelData([FromBody] List<RegistrationDto> students)
        {
            var bytes = await _generateFile.GenerateStudentListExcel(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        [HttpPost("registration-excel")]
        public async Task<IActionResult> RegistrationExcel([FromBody] List<RegistrationDto> students)
        {
            var fileBytes = await  _generateFile.GenerateRegistrationExcel(students);
            var base64Pdf = Convert.ToBase64String(fileBytes);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
       

    
        [HttpPost("Send")]
        public async Task<IActionResult> SendSms([FromBody] SmsGatewayRequest request)
        {
            try
            {
                string authKey = "YOUR_MSG91_AUTH_KEY";   // 🔴 MSG91 key
                string sender = "V3MSCH";                  // approved sender
                string route = "4";

                var url =
                    $"https://api.msg91.com/api/sendhttp.php" +
                    $"?authkey={authKey}" +
                    $"&mobiles={request.Mobile}" +
                    $"&message={Uri.EscapeDataString(request.Message)}" +
                    $"&sender={sender}" +
                    $"&route={route}" +
                    $"&country=91";

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Add("accept", "application/json");

                var response = await _http.SendAsync(httpRequest);
                var result = await response.Content.ReadAsStringAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = response.IsSuccessStatusCode,
                    Data = result,
                    Message = "SMS Sent Successfully"
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("GenerateSlip")]
        public async Task<IActionResult> GenerateSlip(List<StudentListResponse> model)
        {
            var pdfBytes = await _generateFile.GenerateAdmissionSlip(model);
            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "AdmissionSlip generated successfully"
            });
        }
        [HttpPost("ExportStudentReport")]
        public async Task<IActionResult> ExportReport(List<StudentListResponse> model)
        {
            var fileBytes = await _generateFile.ExportStudentReport(model);
            var base64Pdf = Convert.ToBase64String(fileBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        [HttpPost("GenerateAdmissionForms")]
        public async Task<IActionResult> GenerateAdmissionForms(List<StudentListResponse> model)
        {
            var fileBytes = await _generateFile.GenerateRegForms(model);
            var base64Pdf = Convert.ToBase64String(fileBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Reg.Form generated successfully"
            });
        }
        [HttpPost("GenerateBonafideLetter")]
        public async Task<IActionResult> GenerateBonafide(StudentListResponse model)
        {
            var fileBytes = await _generateFile.BonafideCertificate(model);
            var base64Pdf = Convert.ToBase64String(fileBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Reg.Form generated successfully"
            });
           
        }

        [HttpPost("ExportStudentPeriod")]
        public async Task<IActionResult> ExportToReport(List<IMSWFTPeriodType> model)
        {
            var fileBytes = await _generateFile.ExportStudentPeriodType(model);
            var base64Pdf = Convert.ToBase64String(fileBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }

        [HttpPost("GenerateStudentEnqueryListExcel")]
        public async Task<IActionResult> GenerateStudentEnqueryListExcelData([FromBody] List<EnquiryListResponseDto> students)
        {
            var bytes = await _generateFile.GenerateStudentEnqueryListExcelData(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }

        [HttpPost("GenerateStudentEnquerySummary")]
        public async Task<IActionResult> GenerateStudentEnquerySummaryData([FromBody] List<EnquiryListResponseDto> students)
        {   
            var bytes = await _generateFile.GenerateStudentEnquerySummaryData(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        public class SmsGatewayRequest
        {
            public string Mobile { get; set; }
            public string Message { get; set; }
        }



        [HttpPost("GetStudentConcessionExcelData")]
        public async Task<IActionResult> GenerateStudentConcessionExcelData([FromBody] List<StudentWithConcessionDto> students)
        {
            var bytes = await _generateFile.GenerateStudentConcessionListExcelData(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        [HttpPost("PrintStudentTransportExcelData")]
        public async Task<IActionResult> GenerateStudentTeansportnExcelData([FromBody] List<TransportStudentDataModel> students)
        {
            var bytes = await _generateFile.GenerateStudentMapTransportListExcelData(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }

        [HttpPost("PrintStudentNotPromotedExcelData")]
        public async Task<IActionResult> GenerateStudentNotPromotedExcelData([FromBody] List<StudentNotPromotedModel> students)
        {
            var bytes = await _generateFile.GenerateStudentNotPromotedlistData(students);
            var base64Pdf = Convert.ToBase64String(bytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        [HttpPost("GenerateNotPrommotedStudentPdf")]
        public async Task<IActionResult> GenerateNotPromotedListPdf([FromBody] List<StudentNotPromotedModel> student)
        {
            var pdfBytes = await _generateFile.GenerateStudentNotPromotedListPdf(student);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("ViewStudentExcelData")]
        public async Task<IActionResult> GenerateStudentExcelData([FromBody] List<ViewStudentModal> students)
        {
            var bytes = await _generateFile.GenerateViewStudentListExcelData(students);
            var base64Pdf = Convert.ToBase64String(bytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Excel generated successfully"
            });
        }
        [HttpPost("GenerateAllStudentInvoicePdf")]
        public async Task<IActionResult> GenerateInvoicePdf([FromBody] List<StudentDuesModel> hrList)
        {
            var pdfBytes = await _generateFile.GenerateAllStudentInvoicePrintPdf(hrList);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("GenerateAllStudentChallnExcel")]
        public async Task<IActionResult> GenerateAllInvoiceExcel([FromBody] List<StudentDuesModel> hrList)
        {
            var pdfBytes = await _generateFile.GenerateAllStudentDuesChallanExcelData(hrList);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("GenerateAllStudentReceiptPdf")]
        public async Task<IActionResult> GenerateReceiptPdf([FromBody] List<RegistrationReceiptResponse> hrList)
        {
            var pdfBytes = await _generateFile.GenerateStudentReceiptToPdfData(hrList);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("GenerateAllStudentReceiptExcel")]
        public async Task<IActionResult> GenerateAllReceiptExcel([FromBody] List<RegistrationReceiptResponse> hrList)
        {
            var pdfBytes = await _generateFile.GenerateRegistrationReceiptExcel(hrList);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("GenerateStudentReceiptPdfData")]
        public async Task<IActionResult> GenerateRecepitPdf([FromBody] RegistrationReceiptResponse registrationReceipt)
        {
            var pdfBytes = await _generateFile.GenerateStudentPdf(registrationReceipt);

            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDF generated successfully"
            });
        }
        [HttpPost("GeneratePublishingListExcel")]
        public async Task<IActionResult> GeneratePublishingListExcel([FromBody] List<PublishingListResponse> publishingList)
        {
            var excelBytes = await _generateFile  .GeneratePublishingListExcel(publishingList);

            var base64Excel = Convert.ToBase64String(excelBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Excel,
                Message = "Publishing List Excel generated successfully"
            });
        }
        [HttpPost("GeneratePublishingListPdfData")]
        public async Task<IActionResult> GeneratePublishingListPdf([FromBody] List<PublishingListResponse> publishingList)
        {
            var pdfBytes = await _generateFile.GeneratePublishingListPdf(publishingList);

            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Publishing List PDF generated successfully"
            });
        }
        [HttpPost("GenerateClassListPdf")]
        public async Task<IActionResult> GenerateClassListPdf([FromBody] ClassListRequest request)
        {
            var pdfBytes = await _generateFile.GenerateClassListPdf( request);

            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Class List PDF generated successfully"
            });
        }
        [HttpPost("GenerateClassListExcel")]
        public async Task<IActionResult> GenerateClassListExcel([FromBody] ClassListRequest request)
        {
            var excelBytes = await _generateFile.GenerateClassListExcel( request);

            var base64Excel = Convert.ToBase64String(excelBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Excel,
                Message = "Class List Excel generated successfully"
            });
        }
        [HttpPost("GenerateStudentBoardRollNoPdf")]
        public async Task<IActionResult> GenerateBoardRollNoPdf([FromBody] List<AdmSearchedStudentResponse> request)
        {
            var pdfBytes = await _generateFile.StudentBoardRollNoPdf(request);

            var base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "Class List PDF generated successfully"
            });
        }
        [HttpPost("GenerateStudentBoardRollNoExcelData")]
        public async Task<IActionResult> GenerateStudentBoardRollNoExcel([FromBody] List<AdmSearchedStudentResponse> request)
        {
            var excelBytes = await _generateFile.GenerateStudentBoardRollNoExcel(request);

            var base64Excel = Convert.ToBase64String(excelBytes);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Excel,
                Message = "Class List Excel generated successfully"
            });
        }
    }

}
