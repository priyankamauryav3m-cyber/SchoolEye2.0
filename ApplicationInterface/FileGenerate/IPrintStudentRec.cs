    using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.GenerateFile
{
    public interface IPrintStudentRec
    {
        public Task<byte[]> GenerateStudentPdf(RegistrationDto students);
        public Task<byte[]> GenerateStudentListPdf(List<RegistrationDto> students);
        public Task<byte[]> GenerateStudentExcel(List<RegistrationDto> students);
        public Task<byte[]> GenerateStudentListExcel(List<RegistrationDto> students);
        public Task<byte[]> GenerateRegistrationExcel(List<RegistrationDto> students);
        public Task<byte[]> GenerateAdmissionSlip(List<StudentListResponse> model);
        public Task<byte[]> ExportStudentReport(List<StudentListResponse> model);
        public Task<byte[]> ExportStudentPeriodType(List<IMSWFTPeriodType> model);
        public Task<byte[]> ExportStudentToExcel(List<StudentListResponse> model);
        public Task<byte[]> GenerateRegForms(List<StudentListResponse> model);
        public Task<byte[]> BonafideCertificate(StudentListResponse model);
        public Task<byte[]> GenerateStudentEnqueryListExcelData(List<EnquiryListResponseDto> students);
        public Task<byte[]> GenerateStudentEnquerySummaryData(List<EnquiryListResponseDto> students);
        public Task<byte[]> GenerateStudentConcessionListExcelData(List<StudentWithConcessionDto> students);
        public Task<byte[]> GenerateStudentMapTransportListExcelData(List<TransportStudentDataModel> students);
        public Task<byte[]> GenerateStudentNotPromotedlistData(List<StudentNotPromotedModel> students);
        public Task<byte[]> GenerateViewStudentListExcelData(List<ViewStudentModal> students);
        public Task<byte[]> GenerateStudentNotPromotedListPdf(List<StudentNotPromotedModel> students);
        public Task<byte[]> GenerateStudentInvoicePrintPdf(List<InvoiceDetailsResponse> students);
        public Task<byte[]> GenerateAllStudentInvoicePrintPdf(List<StudentDuesModel> students);
        public Task<byte[]> GenerateAllStudentDuesChallanExcelData(List<StudentDuesModel> students);
        public Task<byte[]> GenerateRegistrationReceiptExcel(List<RegistrationReceiptResponse> students);
        public Task<byte[]> GenerateStudentReceiptToPdfData(List<RegistrationReceiptResponse> students);
        public Task<byte[]> GenerateStudentPdf(RegistrationReceiptResponse students);
        public Task<byte[]> GeneratePublishingListExcel(List<PublishingListResponse> publishingList);
        public Task<byte[]> GeneratePublishingListPdf(List<PublishingListResponse> publishingList);
        //  public Task<byte[]> GenerateClassList(List<GetSearchedViewStudentModel> model);
        Task<byte[]> GenerateClassListPdf(ClassListRequest reques);
        Task<byte[]> StudentBoardRollNoPdf(List<AdmSearchedStudentResponse> reques);
        Task<byte[]> GenerateClassListExcel(ClassListRequest request);
    }
}
