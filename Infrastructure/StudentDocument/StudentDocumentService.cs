using ApplicationInterface.GenerateFile;
using ClosedXML.Excel;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Infrastructure.FileGenerate;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.StudentDocument
{
    public class StudentDocumentService : IPrintStudentRec
    {
        public async Task<byte[]> GenerateRegistrationExcel(List<RegistrationDto> students)
        {

            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");
            // ===== HEADER =====
            ws.Range("A2:Y2").Merge();
            ws.Cell("A2").Value = "V3M International School";
            ws.Cell("A2").Style.Font.Bold = true;
            ws.Cell("A2").Style.Font.FontSize = 25;
            ws.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A3:Y3").Merge();
            ws.Cell("A3").Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell("A3").Style.Font.Bold = true;
            ws.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:Y4").Merge();
            ws.Cell("A4").Value = "Registration Report";

            ws.Cell("A4").Style.Font.FontSize = 14;
            ws.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A5:Y5").Merge();
            ws.Cell("A5").Value = "Session : (2025-2026)";
            ws.Cell("A5").Style.Font.Bold = true;
            ws.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ===== TABLE HEADER =====
            ws.Cell("A7").Value = "S.No";
            ws.Cell("B7").Value = "Student Name";
            ws.Cell("C7").Value = "Class";
            ws.Cell("D7").Value = "Father Name";
            ws.Cell("E7").Value = "Mobile";
            ws.Cell("F7").Value = "Gender";
            ws.Cell("G7").Value = "Status";
            ws.Cell("H7").Value = "Date";

            var headerRange = ws.Range("A7:H7");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //   headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== DATA =====
            int row = 8;
            int sr = 1;

            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = sr++;
                ws.Cell(row, 2).Value = s.StudentName;
                ws.Cell(row, 3).Value = s.ClassName;
                ws.Cell(row, 4).Value = s.FatherName;
                ws.Cell(row, 5).Value = s.FatherContactNo;
                ws.Cell(row, 6).Value = s.Gender;
                ws.Cell(row, 7).Value = s.ApplicationStatus;
                ws.Cell(row, 8).Value = s.AppliedDate;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentPdf(RegistrationDto students)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = new CreateDocument { SelectedRow = students };


            var pdfBytes = document.GeneratePdf();
            return pdfBytes;
        }
        public async Task<byte[]> GenerateStudentListPdf(List<RegistrationDto> students)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new StudentListPdfDocument(students);

            return document.GeneratePdf();
        }
        public async Task<byte[]> GenerateStudentListExcel(List<RegistrationDto> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");

            // ===== TITLE =====
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 10).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ===== HEADERS =====
            int headerRow = 3;

            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Reg. No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Father Name";
            ws.Cell(headerRow, 5).Value = "Mobile No.";
            ws.Cell(headerRow, 6).Value = "Gender";
            ws.Cell(headerRow, 7).Value = "Class";
            ws.Cell(headerRow, 8).Value = "DOB";
            ws.Cell(headerRow, 9).Value = "App. Date";
            ws.Cell(headerRow, 10).Value = "Fee";

            ws.Range(headerRow, 1, headerRow, 10).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // headers 2-9 center

            // ===== DATA =====
            int row = headerRow + 1;
            int srNo = 1;
            decimal totalFee = 0;

            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = srNo++; // Sr.No left
                ws.Cell(row, 2).Value = s.RegistrationNo;
                ws.Cell(row, 3).Value = s.StudentName;
                ws.Cell(row, 4).Value = s.FatherName;
                ws.Cell(row, 5).Value = s.FatherContactNo;
                ws.Cell(row, 6).Value = s.Gender;
                ws.Cell(row, 7).Value = s.ClassName;
                ws.Cell(row, 8).Value = s.DateOfBirth;
                ws.Cell(row, 9).Value = s.AppliedDate;
                ws.Cell(row, 10).Value = s.RegistrationFee;

                // Alignments
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // Sr.No
                ws.Range(row, 1, row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // 2-9 center
                ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";

                totalFee += s.RegistrationFee ?? 0;
                row++;
            }

            // ===== TOTAL ROW =====
            ws.Range(row, 1, row, 9).Merge();
            ws.Cell(row, 1).Value = "Total Amount";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(row, 10).Value = totalFee;
            ws.Cell(row, 10).Style.Font.Bold = true;
            ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";

            // ===== FORMATTING =====
            ws.Column(8).Style.DateFormat.Format = "dd-MM-yyyy";
            ws.Column(9).Style.DateFormat.Format = "dd-MM-yyyy";

            ws.Columns().AdjustToContents();
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== EXPORT =====
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentEnquerySummaryData(List<EnquiryListResponse> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");

            int totalColumns = 5;
            ws.Range(1, 1, 1, totalColumns).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, totalColumns).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int headerRow = 3;
            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Class.";
            ws.Cell(headerRow, 3).Value = "No. Of Enquiry";
            ws.Cell(headerRow, 4).Value = "Convert Enq to Reg";
            ws.Cell(headerRow, 5).Value = "Convert Reg to Adm.";

            ws.Range(headerRow, 1, headerRow, totalColumns).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, totalColumns).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int row = headerRow + 1;
            int srNo = 1;

            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = srNo++;
                ws.Cell(row, 2).Value = s.ClassName;
                ws.Cell(row, 3).Value = s.EnquiryNo;
                ws.Cell(row, 4).Value = s.EnquiryConverttoReg;
                ws.Cell(row, 5).Value = s.ConvertedRegtoAdm;

                ws.Range(row, 1, row, totalColumns).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }
            ws.Columns(1, totalColumns).AdjustToContents();

            ws.Range(1, 1, row - 1, totalColumns).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, row - 1, totalColumns).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentEnqueryListExcelData(List<EnquiryListResponse> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");
            ws.Range(1, 1, 1, 23).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 23).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int headerRow = 3;
            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Enquiry No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Gender";
            ws.Cell(headerRow, 5).Value = "Date Of Birth.";
            ws.Cell(headerRow, 6).Value = "Class Name";
            ws.Cell(headerRow, 7).Value = "Father Name";
            ws.Cell(headerRow, 8).Value = "Mother Name";
            ws.Cell(headerRow, 9).Value = "Email";
            ws.Cell(headerRow, 10).Value = "Mobile No";
            ws.Cell(headerRow, 11).Value = "Contact No.";
            ws.Cell(headerRow, 12).Value = "Address.";
            ws.Cell(headerRow, 13).Value = "Source Of Enquiry";
            ws.Cell(headerRow, 14).Value = "Remarks";
            ws.Cell(headerRow, 15).Value = "Enquiry Status.";
            ws.Cell(headerRow, 16).Value = "Enquiry Date";
            ws.Cell(headerRow, 17).Value = "Followup Status";
            ws.Cell(headerRow, 18).Value = "Next Followup Date";
            ws.Cell(headerRow, 19).Value = "FollowUp Remark";
            ws.Cell(headerRow, 20).Value = "Enquiry Convert to Reg";
            ws.Cell(headerRow, 21).Value = "Registration Date";
            ws.Cell(headerRow, 22).Value = "Converted Reg to Adm.";
            ws.Cell(headerRow, 23).Value = "Admission Date";
            ws.Range(headerRow, 1, headerRow, 10).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // headers 2-9 center
            int row = headerRow + 1;
            int srNo = 1;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = srNo++; // Sr.No left
                ws.Cell(row, 2).Value = s.EnquiryNo;
                ws.Cell(row, 3).Value = s.StudentFirstName;
                ws.Cell(row, 4).Value = s.Gender;
                ws.Cell(row, 5).Value = s.DateOfBirth;
                ws.Cell(row, 6).Value = s.ClassName;
                ws.Cell(row, 7).Value = s.FatherName;
                ws.Cell(row, 8).Value = s.MotherName;
                ws.Cell(row, 9).Value = s.Email;
                ws.Cell(row, 10).Value = s.MobileNo;
                ws.Cell(row, 11).Value = s.ContactNo; // Sr.No left
                ws.Cell(row, 12).Value = s.Address;
                ws.Cell(row, 13).Value = s.SourceOfEnquiry;
                ws.Cell(row, 14).Value = s.Remarks;
                ws.Cell(row, 15).Value = s.EnquiryDate;
                ws.Cell(row, 16).Value = s.EnquiryStatus;
                ws.Cell(row, 17).Value = s.NextFollowupDate;
                ws.Cell(row, 18).Value = s.DateOfBirth;
                ws.Cell(row, 19).Value = s.FollowUpRemark;
                ws.Cell(row, 20).Value = s.EnquiryConverttoReg;
                ws.Cell(row, 21).Value = s.RegistrationDate;
                ws.Cell(row, 22).Value = s.ConvertedRegtoAdm;
                ws.Cell(row, 23).Value = s.AdmissionDate;
                // Alignments
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // Sr.No
                ws.Range(row, 1, row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // 2-9 center
                ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";
                //totalFee += s.RegistrationFee ?? 0;
                row++;
            }

            ws.Cell(row, 23).Style.Font.Bold = true;
            ws.Cell(row, 23).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 23).Style.NumberFormat.Format = "0.00";
            ws.Columns().AdjustToContents();
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== EXPORT =====
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentMapTransportListExcelData(List<TransportStudentDataModel> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");
            ws.Range(1, 1, 1, 16).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 16).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(3, 1, 3, 16).Merge();
            ws.Cell(3, 1).Value = "Student Transport List : 2025-2026";
            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int headerRow = 4;
            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Student No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Gender";
            ws.Cell(headerRow, 5).Value = "Class Section";
            ws.Cell(headerRow, 6).Value = "Father Name";
            ws.Cell(headerRow, 7).Value = "Mobile No.";
            ws.Cell(headerRow, 8).Value = "Mobile No1";
            ws.Cell(headerRow, 9).Value = "Father No";
            ws.Cell(headerRow, 10).Value = "Mother No";
            ws.Cell(headerRow, 11).Value = "Distance Name";
            ws.Cell(headerRow, 12).Value = "Transport Fee";
            ws.Cell(headerRow, 13).Value = "Pickup Route Name";
            ws.Cell(headerRow, 14).Value = "Transport Type";
            ws.Cell(headerRow, 15).Value = "Transport Applied Form";
            ws.Cell(headerRow, 16).Value = "Address";
            ws.Range(headerRow, 1, headerRow, 16).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            int row = headerRow + 1;
            int srNo = 1;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = srNo++;
                ws.Cell(row, 2).Value = s.StudentNo;
                ws.Cell(row, 3).Value = s.StudentName;
                ws.Cell(row, 4).Value = s.Gender;
                ws.Cell(row, 5).Value = s.ClassSection;
                ws.Cell(row, 6).Value = s.FatherName;
                ws.Cell(row, 7).Value = s.SMSMobileNo;
                ws.Cell(row, 8).Value = s.SMSMobileNo;
                ws.Cell(row, 9).Value = s.FatherContactNo;
                ws.Cell(row, 10).Value = s.MotherContactNo;
                ws.Cell(row, 11).Value = s.DistanceName;
                ws.Cell(row, 12).Value = s.Amount;
                ws.Cell(row, 13).Value = s.RouteName;
                ws.Cell(row, 14).Value = s.TptStatus;
                ws.Cell(row, 15).Value = s.TransportAppliedFrom;
                ws.Cell(row, 16).Value = s.CurrentAddress;
                // Alignments
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(row, 1, row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";
                row++;
            }

            ws.Cell(row, 16).Style.Font.Bold = true;
            ws.Cell(row, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 16).Style.NumberFormat.Format = "0.00";
            ws.Columns().AdjustToContents();
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== EXPORT =====
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentConcessionListExcelData(List<StudentWithConcessionDto> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Students");
            ws.Range(1, 1, 1, 11).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 11).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int headerRow = 3;
            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Student No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "ClassSection";
            ws.Cell(headerRow, 5).Value = "MotherName";
            ws.Cell(headerRow, 6).Value = "Father Name";
            ws.Cell(headerRow, 7).Value = "Contact No";
            ws.Cell(headerRow, 8).Value = "Concession";
            ws.Cell(headerRow, 9).Value = "Valid From";
            ws.Cell(headerRow, 10).Value = "Valid Upto";
            ws.Cell(headerRow, 11).Value = "Remarks";
            ws.Range(headerRow, 1, headerRow, 11).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            int row = headerRow + 1;
            int srNo = 1;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = srNo++;
                ws.Cell(row, 2).Value = s.ControlNo;
                ws.Cell(row, 3).Value = s.StudentName;
                ws.Cell(row, 4).Value = s.ClassSection;
                ws.Cell(row, 5).Value = s.MotherName;
                ws.Cell(row, 6).Value = s.FatherName;
                ws.Cell(row, 7).Value = s.SMSMobileNo;
                ws.Cell(row, 8).Value = s.Concession;
                ws.Cell(row, 9).Value = s.ValidFrom;
                ws.Cell(row, 10).Value = s.ValidUpto;
                ws.Cell(row, 11).Value = s.Remarks;
                // Alignments
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(row, 1, row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";
                row++;
            }

            ws.Cell(row, 11).Style.Font.Bold = true;
            ws.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 11).Style.NumberFormat.Format = "0.00";
            ws.Columns().AdjustToContents();
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== EXPORT =====
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public Task<byte[]> GenerateStudentExcel(List<RegistrationDto> students)
        {
            throw new NotImplementedException();
        }
        public async Task<byte[]> GenerateAdmissionSlip(List<StudentListResponse> model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new AdmissionSlipDocument(model);
            return document.GeneratePdf();
        }
        public async Task<byte[]> ExportStudentPeriodType(List<IMSWFTPeriodType> model)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Student Report");

                int row = 1;

                // ===== HEADER =====
                worksheet.Range("A1:F1").Merge();
                worksheet.Cell("A1").Value = "CAMBRIDGE SCHOOL - Noida";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A2:F2").Merge();
                worksheet.Cell("A2").Style.Font.Bold = true;
                worksheet.Cell("A2").Value = "Kotla Chail Road Kandaghat, Himachal Pradesh, 173215";
                worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A3:F3").Merge();
                worksheet.Cell("A3").Style.Font.Bold = true;
                worksheet.Cell("A3").Value = "www.dwpschail.com";
                worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A4:F4").Merge();
                worksheet.Cell("A4").Value = "Student With FeePeriod Type List : 2026-2027";
                worksheet.Cell("A4").Style.Font.Bold = true;
                worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row = 6;

                // ===== TABLE HEADER =====
                var headers = new[]
                {
                "Sr No","Roll No","Student No","Student Name","Class-Section","Period Type"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(row, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                row++;

                // ===== DATA =====
                int srNo = 1;

                foreach (var item in model)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.RollNo;
                    worksheet.Cell(row, 3).Value = item.StudentNo;
                    worksheet.Cell(row, 4).Value = item.StudentName;
                    worksheet.Cell(row, 5).Value = item.ClassSection;
                    worksheet.Cell(row, 6).Value = item.PeriodType;

                    for (int col = 1; col <= 6; col++)
                    {
                        var cell = worksheet.Cell(row, col);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

                // ===== ALIGNMENT =====
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task<byte[]> ExportStudentReport(List<StudentListResponse> model)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Student Report");

                int row = 1;

                // ===== HEADER =====
                worksheet.Range("A1:K1").Merge();
                worksheet.Cell("A1").Value = "CAMBRIDGE SCHOOL - Noida";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A2:K2").Merge();
                worksheet.Cell("A2").Value = "Address : Bethesda Christian Academy, Chattarpur";
                worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A3:K3").Merge();
                worksheet.Cell("A3").Value = "Website : http://www.bethesdachristianacademy.in/";
                worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A4:K4").Merge();
                worksheet.Cell("A4").Value = "Student Report : 2025-2026";
                worksheet.Cell("A4").Style.Font.Bold = true;
                worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row = 6;

                // ===== TABLE HEADER =====
                var headers = new[]
                {
                "Roll No","Admission No","Student Name","Gender","DateOfBirth",
                "ClassSection","SMSMobileNo","FatherName","MotherName",
                "DistanceName","AdmissionDate"
            };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                row++;

                // ===== DATA =====
                foreach (var item in model)
                {
                    worksheet.Cell(row, 1).Value = item.RollNo;
                    worksheet.Cell(row, 2).Value = item.AdmissionNo;
                    worksheet.Cell(row, 3).Value = item.StudentName;
                    worksheet.Cell(row, 4).Value = item.Gender;
                    worksheet.Cell(row, 5).Value = item.DateOfBirth;
                    worksheet.Cell(row, 6).Value = item.ClassSection;
                    worksheet.Cell(row, 7).Value = item.SMSMobileNo;
                    worksheet.Cell(row, 8).Value = item.FatherName;
                    worksheet.Cell(row, 9).Value = item.MotherName;
                    worksheet.Cell(row, 10).Value = item.DistanceName;
                    worksheet.Cell(row, 11).Value = item.AdmissionDate;

                    for (int col = 1; col <= 11; col++)
                    {
                        worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task<byte[]> GenerateRegForms(List<StudentListResponse> model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new RegistrationFormDocument(model);
            return document.GeneratePdf();
        }
        public async Task<byte[]> BonafideCertificate(StudentListResponse model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new BonafideCertificateDocument(model);
            return document.GeneratePdf();
        }
        public async Task<byte[]> ExportStudentToExcel(List<StudentListResponse> model)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Student Report");

                int row = 1;

                // ===== HEADER =====
                worksheet.Range("A1:K1").Merge();
                worksheet.Cell("A1").Value = "CAMBRIDGE SCHOOL - Noida";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range("A2:K2").Merge();
                worksheet.Cell("A2").Value = "Registration Fee Report From (02-07-2025 To 03-03-2026)";
                worksheet.Cell("A2").Style.Font.Bold = true;
                worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row = 4;

                // ===== TABLE HEADER =====
                var headers = new[]
                {
                "Registration No","Name","Class",
                "Amount","ReceiptDate","PaymentMode"
            };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                row++;

                // ===== DATA =====
                foreach (var item in model)
                {
                    worksheet.Cell(row, 1).Value = item.RollNo;
                    worksheet.Cell(row, 2).Value = item.AdmissionNo;
                    worksheet.Cell(row, 3).Value = item.StudentName;
                    worksheet.Cell(row, 4).Value = item.Gender;
                    worksheet.Cell(row, 5).Value = item.DateOfBirth;
                    worksheet.Cell(row, 6).Value = item.ClassSection;

                    for (int col = 1; col <= 6; col++)
                    {
                        worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task<byte[]> GenerateStudentNotPromotedlistData(List<StudentNotPromotedModel> students)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("StudentNotPromoted");
            ws.Range(1, 1, 1, 7).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 7).Merge();
            ws.Cell(2, 1).Value = "Not Promotion List Of Students";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(3, 1, 3, 7).Merge();
            ws.Cell(3, 1).Value = "Session 2025-2026";
            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(4, 1, 4, 7).Merge();
            ws.Cell(4, 1).Value = $"{DateTime.Now:dd-MM-yyyy hh:mm tt}";
            ws.Cell(4, 1).Style.Font.Bold = true;
            ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            int headerRow = 5;
            ws.Cell(headerRow, 1).Value = "Sl.No.";
            ws.Cell(headerRow, 2).Value = "Admission No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Class Section";
            ws.Cell(headerRow, 5).Value = "EWS";
            ws.Cell(headerRow, 6).Value = "Due";
            ws.Cell(headerRow, 7).Value = "Active";
            ws.Range(headerRow, 1, headerRow, 7).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(headerRow, 3, headerRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(headerRow, 5, headerRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int row = headerRow + 1;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = s.SlNo;
                ws.Cell(row, 2).Value = s.AdmissionNo;
                ws.Cell(row, 3).Value = s.StudentName;
                ws.Cell(row, 4).Value = s.ClassSection;
                ws.Cell(row, 5).Value = s.EWS;
                ws.Cell(row, 6).Value = s.Due;
                ws.Cell(row, 7).Value = s.Active;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 3, row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                row++;
            }

            ws.Cell(row, 23).Style.Font.Bold = true;
            ws.Cell(row, 23).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 23).Style.NumberFormat.Format = "0.00";
            ws.Columns().AdjustToContents();
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentNotPromotedListPdf(List<StudentNotPromotedModel> students)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = new StudentNotPromotedPdfDocument(students);
            return document.GeneratePdf();
        }
        public async Task<byte[]> GenerateViewStudentListExcelData(List<ViewStudentModal> students)
        {

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student");

            ws.Range(1, 1, 1, 12).Merge();

            ws.Cell(1, 1).Value = "V3M International School";

            ws.Cell(1, 1).Style.Font.Bold = true;

            ws.Cell(1, 1).Style.Font.FontSize = 20;

            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 12).Merge();

            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";

            ws.Cell(2, 1).Style.Font.Bold = true;

            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(3, 1, 3, 12).Merge();

            ws.Cell(3, 1).Value = "Student Report : 2026-2027";

            ws.Cell(3, 1).Style.Font.Bold = true;

            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int headerRow = 4;

            ws.Cell(headerRow, 1).Value = "Sr.No.";

            ws.Cell(headerRow, 2).Value = "Roll.No.";

            ws.Cell(headerRow, 3).Value = "Admission No.";

            ws.Cell(headerRow, 4).Value = "Student Name";

            ws.Cell(headerRow, 5).Value = "Gender";

            ws.Cell(headerRow, 6).Value = "DateOfBirth";

            ws.Cell(headerRow, 7).Value = "ClassSection";

            ws.Cell(headerRow, 8).Value = "SMSMobileNo";

            ws.Cell(headerRow, 9).Value = "FatherName";

            ws.Cell(headerRow, 10).Value = "MotherName";

            ws.Cell(headerRow, 11).Value = "DistanceName";

            ws.Cell(headerRow, 12).Value = "AdmissionDate";

            ws.Range(headerRow, 1, headerRow, 12).Style.Font.Bold = true;

            ws.Range(headerRow, 1, headerRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = headerRow + 1;

            int srNo = 1;

            foreach (var s in students)

            {

                ws.Cell(row, 1).Value = srNo++;

                ws.Cell(row, 2).Value = s.RollNo;

                ws.Cell(row, 3).Value = s.AdmissionNo;

                ws.Cell(row, 4).Value = s.StudentName;

                ws.Cell(row, 5).Value = s.Gender;

                ws.Cell(row, 6).Value = Convert.ToString(s.DateOfBirth);


                ws.Cell(row, 7).Value = s.ClassSection;

                ws.Cell(row, 8).Value = s.SMSMobileNo;

                ws.Cell(row, 9).Value = s.FatherName;

                ws.Cell(row, 10).Value = s.MotherName;

                ws.Cell(row, 11).Value = s.DistanceName;

                ws.Cell(row, 12).Value = Convert.ToString(s.AdmissionDate);

                ws.Range(row, 1, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                row++;

            }

            ws.Cell(row, 12).Style.Font.Bold = true;

            ws.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Columns().AdjustToContents();

            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();

        }
        public async Task<byte[]> GenerateStudentInvoicePrintPdf(List<InvoiceDetailsResponse> students)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = new FeeChallanPdfDocument(students);
            return document.GeneratePdf();
        }
        public async Task<byte[]> GenerateAllStudentInvoicePrintPdf(List<StudentDuesModel> students)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new AllStudentPdfChallanDuesDocument(students);

            return document.GeneratePdf();
        }
        public async Task<byte[]> GenerateAllStudentDuesChallanExcelData(List<StudentDuesModel> students)
        {

            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("ChallanDuesReports");
            ws.Range(1, 1, 1, 12).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, 2, 12).Merge();
            ws.Cell(2, 1).Value = "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(3, 1, 3, 12).Merge();

            ws.Cell(3, 1).Value = "Fee Due Report";

            ws.Cell(3, 1).Style.Font.Bold = true;

            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int headerRow = 4;

            ws.Cell(headerRow, 1).Value = "Sr.No.";
            ws.Cell(headerRow, 2).Value = "Student.No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Father Name";
            ws.Cell(headerRow, 5).Value = "Address";
            ws.Cell(headerRow, 6).Value = "Class-Section";
            ws.Cell(headerRow, 7).Value = "Category";
            ws.Cell(headerRow, 8).Value = "Mobile No.";
            ws.Cell(headerRow, 9).Value = "Contact Email";
            ws.Cell(headerRow, 10).Value = "Due Amount";
            ws.Cell(headerRow, 11).Value = "CommentText";

            ws.Range(headerRow, 1, headerRow, 11).Style.Font.Bold = true;
            ws.Cell(headerRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(headerRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(headerRow, 3, headerRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(headerRow, 7, headerRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(headerRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(headerRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(headerRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            int row = headerRow + 1;

            int srNo = 1;

            foreach (var s in students)

            {

                ws.Cell(row, 1).Value = srNo++;
                ws.Cell(row, 2).Value = s.ControlNo;
                ws.Cell(row, 3).Value = s.StudentName;
                ws.Cell(row, 4).Value = s.FatherName;
                ws.Cell(row, 5).Value = s.CurrentAddress;
                ws.Cell(row, 6).Value = s.ClassSection;
                ws.Cell(row, 7).Value = s.Category;
                ws.Cell(row, 8).Value = s.SMSMobileNo;
                ws.Cell(row, 9).Value = s.FatherEMail;
                ws.Cell(row, 10).Value = s.Amount;
                ws.Cell(row, 11).Value = s.CommentText;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, 3,row,6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(row, 7,row,8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                row++;

            }

            ws.Cell(row, 11).Style.Font.Bold = true;

            ws.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Columns().AdjustToContents();

            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();

        }

    }
}
