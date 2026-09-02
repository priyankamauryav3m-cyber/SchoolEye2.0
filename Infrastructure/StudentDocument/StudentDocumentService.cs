using ApplicationInterface.GenerateFile;
using Azure.Core;
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
        public async Task<byte[]> GenerateStudentPdf(RegistrationReceiptResponse students)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = new PrinReceiptDocument { SelectedRow = students };
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
        public async Task<byte[]> GenerateStudentEnquerySummaryData(List<EnquirySummaryTableResponse> students)
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
                ws.Cell(row, 2).Value = s.Class;
                ws.Cell(row, 3).Value = s.NoOfEnquiry;
                ws.Cell(row, 4).Value = s.ConvertEnqToReg;
                ws.Cell(row, 5).Value = s.ConvertRegToAdm;

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
        public async Task<byte[]> GenerateStudentEnqueryListExcelData(List<EnquiryListResponseDto> students)
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
                ws.Range(row, 3, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(row, 7, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
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
        public async Task<byte[]> GenerateRegistrationReceiptExcel(List<RegistrationReceiptResponse> receipts)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Registration Fee Report");
            ws.Range(1, 1, 1, 7).Merge();
            ws.Cell(1, 1).Value = "V3M International School";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, 2, 7).Merge();
            ws.Cell(2, 1).Value = "Registration Fee Report";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(3, 1, 3, 7).Merge();

            ws.Cell(3, 1).Value = "Registration Fee Receipt";

            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(4, 1, 4, 7).Merge();
            ws.Cell(4, 1).Value = $"Print Date : {DateTime.Now:dd-MM-yyyy hh:mm tt}";

            ws.Cell(4, 1).Style.Font.Bold = true;
            ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            int headerRow = 5;

            ws.Cell(headerRow, 1).Value = "Sr.";
            ws.Cell(headerRow, 2).Value = "Reg. No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Class";
            ws.Cell(headerRow, 5).Value = "Amount";
            ws.Cell(headerRow, 6).Value = "Receipt Date";
            ws.Cell(headerRow, 7).Value = "Payment Mode";

            var headerRange = ws.Range(headerRow, 1, headerRow, 7);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            int row = headerRow + 1;
            int srNo = 1;
            foreach (var item in receipts)
            {
                ws.Cell(row, 1).Value = srNo++;
                ws.Cell(row, 2).Value = item.RegistrationNo ?? "";
                ws.Cell(row, 3).Value = item.StudentName ?? "";
                ws.Cell(row, 4).Value = item.ClassName ?? "";
                ws.Cell(row, 5).Value = item.Amount;
                if (DateTime.TryParse(item.ReceiptDate, out DateTime receiptDate))
                {
                    ws.Cell(row, 6).Value = receiptDate;
                    ws.Cell(row, 6).Style.DateFormat.Format = "dd-MMM-yy";
                }
                else
                {
                    ws.Cell(row, 6).Value = item.ReceiptDate ?? "";
                }

                ws.Cell(row, 7).Value = item.PaymentMode ?? "";
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.00";

                row++;
            }
            var usedRange = ws.RangeUsed();
            if (usedRange != null)
            {
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 28;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 18;
            ws.Column(7).Width = 18;
            ws.Rows().AdjustToContents();
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.Margins.Top = 0.5;
            ws.PageSetup.Margins.Bottom = 0.5;
            ws.PageSetup.Margins.Left = 0.3;
            ws.PageSetup.Margins.Right = 0.3;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
        public async Task<byte[]> GenerateStudentReceiptToPdfData(List<RegistrationReceiptResponse> students)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new RegistrationReceiptPdfDocument(students);

            return document.GeneratePdf();
        }
        public async Task<byte[]> GeneratePublishingListExcel(List<PublishingListResponse> publishingList)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Publishing List");

            const int totalColumns = 11;

            // =========================================================
            // SCHOOL NAME
            // =========================================================

            ws.Range(1, 1, 1, totalColumns).Merge();

            ws.Cell(1, 1).Value = "CAMBRIDGE School - Noida";

            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 20;
            ws.Cell(1, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 1).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =========================================================
            // SCHOOL ADDRESS
            // =========================================================

            ws.Range(2, 1, 2, totalColumns).Merge();

            ws.Cell(2, 1).Value =
                "Sector-27, NOIDA, G.B. Nagar, Noida UP - 201301";

            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(2, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 1).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =========================================================
            // BLANK ROW
            // =========================================================

            ws.Range(3, 1, 3, totalColumns).Merge();


            // =========================================================
            // REPORT TITLE
            // =========================================================

            ws.Range(5, 1, 5, 3).Merge();

            ws.Cell(5, 1).Value = "Selected Student Of";

            ws.Cell(5, 1).Style.Font.Bold = true;
            ws.Cell(5, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;
            ws.Cell(5, 1).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =========================================================
            // PUBLISH LIST
            // =========================================================

            ws.Range(5, 4, 5, 7).Merge();

            int listNo = publishingList
                .FirstOrDefault()?.ListNo ?? 0;

            string sessionName = publishingList
                .FirstOrDefault()?.SessionName ?? "";

            ws.Cell(5, 4).Value =
                $"PublishList : {listNo} for Session ({sessionName})";

            ws.Cell(5, 4).Style.Font.Bold = true;
            ws.Cell(5, 4).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            ws.Cell(5, 4).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =========================================================
            // PRINT DATE
            // =========================================================

            ws.Range(5, 8, 5, totalColumns).Merge();

            ws.Cell(5, 8).Value =
                $"Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";

            ws.Cell(5, 8).Style.Font.Bold = true;
            ws.Cell(5, 8).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Right;
            ws.Cell(5, 8).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =========================================================
            // HEADER
            // =========================================================

            int headerRow = 6;

            ws.Cell(headerRow, 1).Value = "S. No.";
            ws.Cell(headerRow, 2).Value = "Reg. No.";
            ws.Cell(headerRow, 3).Value = "Student Name";
            ws.Cell(headerRow, 4).Value = "Father Name";
            ws.Cell(headerRow, 5).Value = "Mother Name";
            ws.Cell(headerRow, 6).Value = "Gender";
            ws.Cell(headerRow, 7).Value = "Class Name";
            ws.Cell(headerRow, 8).Value = "Mobile No";
            ws.Cell(headerRow, 9).Value = "Points";
            ws.Cell(headerRow, 10).Value = "Application Status";
            ws.Cell(headerRow, 11).Value = "Application Status Date";

            var headerRange = ws.Range(
                headerRow,
                1,
                headerRow,
                totalColumns);

            headerRange.Style.Font.Bold = true;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            headerRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            headerRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;


            // =========================================================
            // DATA
            // =========================================================

            int row = headerRow + 1;
            int srNo = 1;

            foreach (var item in publishingList)
            {
                ws.Cell(row, 1).Value = srNo++;

                ws.Cell(row, 2).Value =
                    item.RegistrationNo ?? "";

                ws.Cell(row, 3).Value =
                    item.StudentName ?? "";

                ws.Cell(row, 4).Value =
                    item.FatherName ?? "";

                ws.Cell(row, 5).Value =
                    item.MotherName ?? "";

                ws.Cell(row, 6).Value =
                    item.Gender ?? "";

                ws.Cell(row, 7).Value =
                    item.ClassName ?? "";

                ws.Cell(row, 8).Value =
                    item.SMSMobileNo ?? "";

                ws.Cell(row, 9).Value =
                    item.Points;

                ws.Cell(row, 10).Value =
                    item.ApplicationStatus ?? "";

                // Application Status Date
                if (item.AdmissionDate.HasValue)
                {
                    ws.Cell(row, 11).Value =
                        item.AdmissionDate.Value;

                    ws.Cell(row, 11)
                        .Style
                        .DateFormat
                        .Format = "dd-MM-yyyy";
                }
                else
                {
                    ws.Cell(row, 11).Value = "";
                }


                // =====================================================
                // ALIGNMENT
                // =====================================================

                ws.Cell(row, 1).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 2).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 3).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Left;

                ws.Cell(row, 4).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Left;

                ws.Cell(row, 5).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Left;

                ws.Cell(row, 6).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 7).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 8).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 9).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 10).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Left;

                ws.Cell(row, 11).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                row++;
            }


            // =========================================================
            // BORDER
            // =========================================================

            var usedRange = ws.RangeUsed();

            if (usedRange != null)
            {
                usedRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                usedRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
            }


            // =========================================================
            // AUTO COLUMN WIDTH
            // =========================================================

            ws.ColumnsUsed().AdjustToContents();


            // =========================================================
            // ROW HEIGHT
            // =========================================================

            ws.Row(1).Height = 30;
            ws.Row(2).Height = 22;
            ws.Row(3).Height = 8;
            ws.Row(5).Height = 24;
            ws.Row(6).Height = 25;


            // =========================================================
            // FREEZE HEADER
            // =========================================================

            ws.SheetView.FreezeRows(6);


            // =========================================================
            // PAGE SETUP
            // =========================================================

            ws.PageSetup.PageOrientation =
                XLPageOrientation.Landscape;

            ws.PageSetup.PaperSize =
                XLPaperSize.A4Paper;

            ws.PageSetup.Margins.Top = 0.5;
            ws.PageSetup.Margins.Bottom = 0.5;
            ws.PageSetup.Margins.Left = 0.3;
            ws.PageSetup.Margins.Right = 0.3;


            // Repeat header on every printed page
            ws.PageSetup.SetRowsToRepeatAtTop(1, 6);

            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;


            // =========================================================
            // SAVE
            // =========================================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }
        public async Task<byte[]> GeneratePublishingListPdf(List<PublishingListResponse> publishingList)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new PublishingListPdfDocument(publishingList);

            return document.GeneratePdf();
        }
        public async Task<byte[]> GenerateClassListPdf(ClassListRequest request)
        {

            QuestPDF.Settings.License = LicenseType.Community;

            var document = new ClassListPdfDocument(request);

            return await Task.FromResult(document.GeneratePdf());
        }
        public async Task<byte[]> GenerateClassListExcel(ClassListRequest request)
        {
            request ??= new ClassListRequest();

            var students = request.Students ?? new List<GetSearchedViewStudentModel>();
            string className = request.ClassName ?? "";
            string sectionName = request.SectionName ?? "";
            string sessionName = request.SessionName ?? "";
            string classSessionText = string.Join(" - ",
           new[] { className, sectionName }
           .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!string.IsNullOrWhiteSpace(sessionName))
            {
                classSessionText += $" (SESSION {sessionName})";
            }
            int blankColumnCount = Math.Clamp(request.BlankColumnCount, 1, 5);


            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Class List");

            const int studentsPerPage = 36;

            int totalColumns = 4 + blankColumnCount;

            int currentRow = 1;
            int srNo = 1;

            var studentPages = students
                .Select((student, index) => new
                {
                    Student = student,
                    Index = index
                })
                .GroupBy(x => x.Index / studentsPerPage)
                .Select(x => x.Select(y => y.Student).ToList())
                .ToList();

            if (!studentPages.Any())
            {
                studentPages.Add(new List<GetSearchedViewStudentModel>());
            }

            foreach (var pageStudents in studentPages)
            {
                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

                ws.Cell(currentRow, 1).Value = "CAMBRIDGE SCHOOL, NOIDA";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 16;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

                ws.Cell(currentRow, 1).Value = "CLASS LIST";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 12;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

                ws.Cell(currentRow, 1).Value = classSessionText;
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

                ws.Cell(currentRow, 1).Value = "CLASS TEACHER : Prakshi Jain";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

                ws.Cell(currentRow, 1).Value = $"Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;

                int headerRow = currentRow;

                ws.Cell(headerRow, 1).Value = "S. NO.";
                ws.Cell(headerRow, 2).Value = "ROLL NO.";
                ws.Cell(headerRow, 3).Value = "ADM. NO.";
                ws.Cell(headerRow, 4).Value = "STUDENT NAME";

                for (int i = 1; i <= blankColumnCount; i++)
                {
                    ws.Cell(headerRow, 4 + i).Value = "---";
                }

                var headerRange = ws.Range(headerRow, 1, headerRow, totalColumns);

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;

                foreach (var item in pageStudents)
                {
                    ws.Cell(currentRow, 1).Value = srNo++;
                    ws.Cell(currentRow, 2).Value = item.RollNo ?? "";
                    ws.Cell(currentRow, 3).Value = item.ControlNo ?? "";
                    ws.Cell(currentRow, 4).Value = item.StudentName ?? "";

                    if (item.Gender?.Equals("Female", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        ws.Cell(currentRow, 4).Style.Font.Bold = true;
                    }

                    for (int i = 1; i <= blankColumnCount; i++)
                    {
                        ws.Cell(currentRow, 4 + i).Value = "";
                    }

                    currentRow++;
                }

                int blankRows = studentsPerPage - pageStudents.Count;

                for (int i = 0; i < blankRows; i++)
                {
                    for (int col = 1; col <= totalColumns; col++)
                    {
                        ws.Cell(currentRow, col).Value = "";
                    }

                    currentRow++;
                }

                int generalBoys = pageStudents.Count(x =>
                    x.StudentCategoryName?.Equals("General", StringComparison.OrdinalIgnoreCase) == true &&
                    x.Gender?.Equals("Male", StringComparison.OrdinalIgnoreCase) == true);

                int generalGirls = pageStudents.Count(x =>
                    x.StudentCategoryName?.Equals("General", StringComparison.OrdinalIgnoreCase) == true &&
                    x.Gender?.Equals("Female", StringComparison.OrdinalIgnoreCase) == true);

                int ewsBoys = pageStudents.Count(x =>
                    x.StudentCategoryName?.Equals("EWS", StringComparison.OrdinalIgnoreCase) == true &&
                    x.Gender?.Equals("Male", StringComparison.OrdinalIgnoreCase) == true);

                int ewsGirls = pageStudents.Count(x =>
                    x.StudentCategoryName?.Equals("EWS", StringComparison.OrdinalIgnoreCase) == true &&
                    x.Gender?.Equals("Female", StringComparison.OrdinalIgnoreCase) == true);

                int totalStudents = pageStudents.Count;

                int summaryRow = currentRow + 1;

                ws.Range(summaryRow, 1, summaryRow, 2).Merge();
                ws.Cell(summaryRow, 1).Value = "GENERAL";

                ws.Range(summaryRow, 3, summaryRow, 4).Merge();
                ws.Cell(summaryRow, 3).Value = "EWS";

                ws.Range(summaryRow, 5, summaryRow + 1, totalColumns).Merge();
                ws.Cell(summaryRow, 5).Value = "TOTAL";

                int subHeaderRow = summaryRow + 1;

                ws.Cell(subHeaderRow, 1).Value = "Boys";
                ws.Cell(subHeaderRow, 2).Value = "Girls";
                ws.Cell(subHeaderRow, 3).Value = "Boys";
                ws.Cell(subHeaderRow, 4).Value = "Girls";

                int valueRow = summaryRow + 2;

                ws.Cell(valueRow, 1).Value = generalBoys;
                ws.Cell(valueRow, 2).Value = generalGirls;
                ws.Cell(valueRow, 3).Value = ewsBoys;
                ws.Cell(valueRow, 4).Value = ewsGirls;

                ws.Range(valueRow, 5, valueRow, totalColumns).Merge();
                ws.Cell(valueRow, 5).Value = totalStudents;

                var summaryRange = ws.Range(summaryRow, 1, valueRow, totalColumns);

                summaryRange.Style.Font.Bold = true;
                summaryRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                summaryRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                var dataRange = ws.Range(headerRow, 1, currentRow - 1, totalColumns);

                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow = valueRow + 3;
            }

            ws.Column(1).Width = 8;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 28;

            for (int column = 5; column <= totalColumns; column++)
            {
                ws.Column(column).Width = 10;
            }

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }
        public async Task<byte[]> StudentBoardRollNoPdf(List<AdmSearchedStudentResponse> request, bool isBoardRollNo)
        {

            QuestPDF.Settings.License = LicenseType.Community;

            var document = new StudentBoardRollNoPdfDocument(request, isBoardRollNo);

            return await Task.FromResult(document.GeneratePdf());
        }

        public async Task<byte[]> GenerateStudentBoardRollNoExcel(List<AdmSearchedStudentResponse> students, bool isBoardRollNo)
        {
            students ??= new List<AdmSearchedStudentResponse>();


            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet(isBoardRollNo ? "Student Board Roll No" : "Student CBSE Reg No");

            const int totalColumns = 7;

            int currentRow = 1;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "Cambridge School, Noida";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 16;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "Sector-27, Noida, Uttar Pradesh 201301";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "noida.cambridgeschool.edu.in";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 9;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = isBoardRollNo ? "Student Board Roll No" : "Student CBSE Registration No";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 11;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            int tableHeaderRow = currentRow;

            ws.Cell(currentRow, 1).Value = "Sl No";
            ws.Cell(currentRow, 2).Value = "Student No";
            ws.Cell(currentRow, 3).Value = "Student Name";
            ws.Cell(currentRow, 4).Value = "Class";
            ws.Cell(currentRow, 5).Value = "Section";
            ws.Cell(currentRow, 6).Value = "Date of Birth";
            ws.Cell(currentRow, 7).Value = isBoardRollNo ? "Board Roll No" : "CBSE Reg No";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 22;

            currentRow++;

            int srNo = 1;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = srNo;
                ws.Cell(currentRow, 2).Value = item.ControlNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";
                ws.Cell(currentRow, 4).Value = item.ClassName ?? "";
                ws.Cell(currentRow, 5).Value = item.SectionName ?? "";

                if (item.DateOfBirth.HasValue)
                {
                    ws.Cell(currentRow, 6).Value = item.DateOfBirth.Value;
                    ws.Cell(currentRow, 6).Style.DateFormat.Format = "dd MMM yyyy";
                }

                ws.Cell(currentRow, 7).Value = isBoardRollNo ? item.BoardRollNo ?? "" : item.CBSERegNo ?? "";

                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Row(currentRow).Height = 20;

                srNo++;
                currentRow++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= tableHeaderRow)
            {
                var completeTableRange = ws.Range(tableHeaderRow, 1, lastDataRow, totalColumns);

                completeTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Column(1).Width = 7;
            ws.Column(2).Width = 14;
            ws.Column(3).Width = 28;
            ws.Column(4).Width = 8;
            ws.Column(5).Width = 10;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 16;

            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }

        public async Task<byte[]> GenerateAbscondedStudentExcel(List<GetAbscondedStudentResponse> students)
        {
            students ??= new List<GetAbscondedStudentResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student Report");

            const int totalColumns = 15;

            int currentRow = 1;

            ws.Style.Font.FontSize = 10;

            string sessionName = students.FirstOrDefault()?.SessionName ?? "";

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

            ws.Cell(currentRow, 1).Value = "CAMBRIDGE School - Noida";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Row(currentRow).Height = 20;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

            ws.Cell(currentRow, 1).Value = "Sector-27, NOIDA, G.B. Nagar, Noida UP - 201301";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Row(currentRow).Height = 18;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

            ws.Cell(currentRow, 1).Value = $"Student Report : {sessionName}";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Row(currentRow).Height = 18;

            currentRow++;

            int tableHeaderRow = currentRow;

            ws.Cell(currentRow, 1).Value = "RollNo";
            ws.Cell(currentRow, 2).Value = "ControlNo";
            ws.Cell(currentRow, 3).Value = "StudentName";
            ws.Cell(currentRow, 4).Value = "Gender";
            ws.Cell(currentRow, 5).Value = "DateOfBirth";
            ws.Cell(currentRow, 6).Value = "ClassName";
            ws.Cell(currentRow, 7).Value = "SectionName";
            ws.Cell(currentRow, 8).Value = "SMSMobileNo";
            ws.Cell(currentRow, 9).Value = "FatherName";
            ws.Cell(currentRow, 10).Value = "FatherContactNo";
            ws.Cell(currentRow, 11).Value = "MotherName";
            ws.Cell(currentRow, 12).Value = "MotherContactNo";
            ws.Cell(currentRow, 13).Value = "IsReservedSeat";
            ws.Cell(currentRow, 14).Value = "AdmissionDate";
            ws.Cell(currentRow, 15).Value = "StudentStatus";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 20;

            currentRow++;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = item.RollNo ?? "";
                ws.Cell(currentRow, 2).Value = item.ControlNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";
                ws.Cell(currentRow, 4).Value = item.Gender ?? "";

                if (item.DateOfBirth.HasValue)
                {
                    ws.Cell(currentRow, 5).Value = item.DateOfBirth.Value;
                    ws.Cell(currentRow, 5).Style.DateFormat.Format = "dd-MM-yyyy";
                }
                else
                {
                    ws.Cell(currentRow, 5).Value = "";
                }

                ws.Cell(currentRow, 6).Value = item.ClassName ?? "";
                ws.Cell(currentRow, 7).Value = item.SectionName ?? "";
                ws.Cell(currentRow, 8).Value = item.SMSMobileNo ?? "";
                ws.Cell(currentRow, 9).Value = item.FatherName ?? "";
                ws.Cell(currentRow, 10).Value = item.FatherContactNo ?? "";
                ws.Cell(currentRow, 11).Value = item.MotherName ?? "";
                ws.Cell(currentRow, 12).Value = item.MotherContactNo ?? "";
                ws.Cell(currentRow, 13).Value = item.IsReservedSeat ?? "";

                if (item.AdmissionDate.HasValue)
                {
                    ws.Cell(currentRow, 14).Value = item.AdmissionDate.Value;
                    ws.Cell(currentRow, 14).Style.DateFormat.Format = "dd-MM-yyyy";
                }
                else
                {
                    ws.Cell(currentRow, 14).Value = "";
                }

                ws.Cell(currentRow, 15).Value = item.StudentStatus ?? "";

                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Row(currentRow).Height = 18;

                currentRow++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= tableHeaderRow)
            {
                var completeTableRange = ws.Range(
                    tableHeaderRow,
                    1,
                    lastDataRow,
                    totalColumns);

                completeTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns(1, totalColumns).AdjustToContents();

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;

            ws.PageSetup.Margins.Top = 0.15;
            ws.PageSetup.Margins.Bottom = 0.15;
            ws.PageSetup.Margins.Left = 0.1;
            ws.PageSetup.Margins.Right = 0.1;
            ws.PageSetup.Margins.Header = 0;
            ws.PageSetup.Margins.Footer = 0;

            ws.PageSetup.SetRowsToRepeatAtTop(
                tableHeaderRow,
                tableHeaderRow);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }
        public async Task<byte[]> GenerateStudentRTEExcel(List<StudentRTEResponse> students)
        {
            students ??= new List<StudentRTEResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student RTE");

            const int totalColumns = 19;

            int currentRow = 1;

            ws.Cell(currentRow, 1).Value = "Sl. No";
            ws.Cell(currentRow, 2).Value = "Student No";
            ws.Cell(currentRow, 3).Value = "Student Name";
            ws.Cell(currentRow, 4).Value = "Admission Date";
            ws.Cell(currentRow, 5).Value = "Class Admitted";
            ws.Cell(currentRow, 6).Value = "Admission No";
            ws.Cell(currentRow, 7).Value = "Class";
            ws.Cell(currentRow, 8).Value = "Gender";
            ws.Cell(currentRow, 9).Value = "Registration No.";
            ws.Cell(currentRow, 10).Value = "Father Name";
            ws.Cell(currentRow, 11).Value = "Mother Name";
            ws.Cell(currentRow, 12).Value = "SMS Mobile No.";
            ws.Cell(currentRow, 13).Value = "Admission Session";
            ws.Cell(currentRow, 14).Value = "Bank Name";
            ws.Cell(currentRow, 15).Value = "Account Holder Name";
            ws.Cell(currentRow, 16).Value = "IFSC CODE";
            ws.Cell(currentRow, 17).Value = "Account Number";
            ws.Cell(currentRow, 18).Value = "Student RTE Category";
            ws.Cell(currentRow, 19).Value = "Student RTE SubCategory";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 22;

            currentRow++;

            int srNo = 1;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = srNo;
                ws.Cell(currentRow, 2).Value = item.ControlNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";

                if (item.AdmissionDate.HasValue)
                {
                    ws.Cell(currentRow, 4).Value = item.AdmissionDate.Value;
                    ws.Cell(currentRow, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                }

                ws.Cell(currentRow, 5).Value = item.ClassName ?? "";
                ws.Cell(currentRow, 6).Value = item.AdmissionNo ?? "";
                ws.Cell(currentRow, 7).Value = item.ClassSection ?? "";
                ws.Cell(currentRow, 8).Value = item.Gender ?? "";
                ws.Cell(currentRow, 9).Value = item.RegistrationNo ?? "";
                ws.Cell(currentRow, 10).Value = item.FatherName ?? "";
                ws.Cell(currentRow, 11).Value = item.MotherName ?? "";
                ws.Cell(currentRow, 12).Value = item.SMSMobileNo ?? "";
                ws.Cell(currentRow, 13).Value = item.AdmissionSession ?? "";
                ws.Cell(currentRow, 14).Value = item.BankName ?? "";
                ws.Cell(currentRow, 15).Value = item.AccountHolderName ?? "";
                ws.Cell(currentRow, 16).Value = item.IFSCCODE ?? "";
                ws.Cell(currentRow, 17).Value = item.AccountNumber ?? "";
                ws.Cell(currentRow, 18).Value = item.CategoryName ?? "";
                ws.Cell(currentRow, 19).Value = item.SubCategoryName ?? "";

                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 19).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                //ws.Row(currentRow).Height = 20;
                ws.Row(currentRow).AdjustToContents();

                currentRow++;
                srNo++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= 1)
            {
                var completeRange = ws.Range(1, 1, lastDataRow, totalColumns);

                completeRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;
            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }
        public async Task<byte[]> GenerateDisabilityStudentExcel(List<DisabilityStudentResponse> students)
        {
            students ??= new List<DisabilityStudentResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Disability Student");

            const int totalColumns = 9;

            int currentRow = 1;

            ws.Cell(currentRow, 1).Value = "Sl. No";
            ws.Cell(currentRow, 2).Value = "Roll No";
            ws.Cell(currentRow, 3).Value = "Student No";
            ws.Cell(currentRow, 4).Value = "Student Name";
            ws.Cell(currentRow, 5).Value = "Class";
            ws.Cell(currentRow, 6).Value = "Gender";
            ws.Cell(currentRow, 7).Value = "Date Of Birth";
            ws.Cell(currentRow, 8).Value = "Is Disability";
            ws.Cell(currentRow, 9).Value = "Description of the Disability";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            int srNo = 1;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = srNo;
                ws.Cell(currentRow, 2).Value = item.RollNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentNo ?? "";
                ws.Cell(currentRow, 4).Value = item.StudentName ?? "";
                ws.Cell(currentRow, 5).Value = item.ClassSection ?? "";
                ws.Cell(currentRow, 6).Value = item.Gender ?? "";

                if (item.DateOfBirth.HasValue)
                {
                    ws.Cell(currentRow, 7).Value = item.DateOfBirth.Value;
                    ws.Cell(currentRow, 7).Style.DateFormat.Format = "dd/MM/yyyy";
                }

                ws.Cell(currentRow, 8).Value = item.IsDisability ?? "";
                ws.Cell(currentRow, 9).Value = item.NatureOfDisability ?? "";

                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Row(currentRow).Style.Alignment.WrapText = true;

                currentRow++;
                srNo++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= 1)
            {
                var completeRange = ws.Range(1, 1, lastDataRow, totalColumns);

                completeRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();

            ws.Rows().AdjustToContents();

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;

            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }

        public async Task<byte[]> GenerateSiblingListExcel(List<SiblingListResponse> students)
        {
            students ??= new List<SiblingListResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student Sibling");

            const int totalColumns = 8;

            int currentRow = 1;

            string sessionName = students.FirstOrDefault()?.SessionName ?? "";


            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();

            ws.Cell(currentRow, 1).Value =
                $"Student Sibling report for session {sessionName}";

            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 16;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Range(currentRow, 1, currentRow, totalColumns).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 28;

            currentRow++;

            ws.Cell(currentRow, 1).Value = "BranchName";
            ws.Cell(currentRow, 2).Value = "Student No";
            ws.Cell(currentRow, 3).Value = "Student Name";
            ws.Cell(currentRow, 4).Value = "Class";
            ws.Cell(currentRow, 5).Value = "Father Name";
            ws.Cell(currentRow, 6).Value = "Mother Name";
            ws.Cell(currentRow, 7).Value = "Mobile No";
            ws.Cell(currentRow, 8).Value = "SiblingID";

            var headerRange = ws.Range(
                currentRow,
                1,
                currentRow,
                totalColumns
            );

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 25;

            currentRow++;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = item.BranchName ?? "";
                ws.Cell(currentRow, 2).Value = item.StudentNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";
                ws.Cell(currentRow, 4).Value = item.ClassSection ?? "";
                ws.Cell(currentRow, 5).Value = item.FatherName ?? "";
                ws.Cell(currentRow, 6).Value = item.MotherName ?? "";
                ws.Cell(currentRow, 7).Value = item.SMSMobileNo ?? "";
                ws.Cell(currentRow, 8).Value = item.SiblingId ?? "";


                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Cell(currentRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Row(currentRow).Style.Alignment.WrapText = true;

                currentRow++;
            }


            int lastDataRow = currentRow - 1;

            if (lastDataRow >= 2)
            {
                var completeRange = ws.Range(
                    2,
                    1,
                    lastDataRow,
                    totalColumns
                );

                completeRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                completeRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
            }


            ws.Columns().AdjustToContents();


            if (ws.Column(1).Width < 25)
                ws.Column(1).Width = 25;

            if (ws.Column(1).Width > 30)
                ws.Column(1).Width = 30;

            if (ws.Column(2).Width < 14)
                ws.Column(2).Width = 14;

            if (ws.Column(3).Width < 22)
                ws.Column(3).Width = 22;

            if (ws.Column(3).Width > 30)
                ws.Column(3).Width = 30;

            if (ws.Column(4).Width < 14)
                ws.Column(4).Width = 14;

            if (ws.Column(4).Width > 20)
                ws.Column(4).Width = 20;

            if (ws.Column(5).Width < 22)
                ws.Column(5).Width = 22;

            if (ws.Column(5).Width > 30)
                ws.Column(5).Width = 30;

            if (ws.Column(6).Width < 22)
                ws.Column(6).Width = 22;

            if (ws.Column(6).Width > 30)
                ws.Column(6).Width = 30;

            if (ws.Column(7).Width < 15)
                ws.Column(7).Width = 15;

            if (ws.Column(7).Width > 18) ws.Column(7).Width = 18;

            if (ws.Column(8).Width < 30) ws.Column(8).Width = 30;

            if (ws.Column(8).Width > 38) ws.Column(8).Width = 38;


            if (lastDataRow >= 2)
            {
                ws.Rows(2, lastDataRow).AdjustToContents();

                for (int row = 3; row <= lastDataRow; row++)
                {
                    if (ws.Row(row).Height < 20)
                        ws.Row(row).Height = 20;
                }
            }


            ws.SheetView.FreezeRows(2);

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;

            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;

            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.PagesTall = 0;

            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            ws.PageSetup.SetRowsToRepeatAtTop(1, 2);
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return await Task.FromResult(stream.ToArray());
        }

        public async Task<byte[]> GenerateStudentBirthdayExcel(List<BirthdayStudentResponse> students)
        {
            students ??= new List<BirthdayStudentResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student Birthday Report");

            const int totalColumns = 9;

            int currentRow = 1;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "CAMBRIDGE School - Noida";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            var birthdayLabel = students.Count > 0 && !string.IsNullOrWhiteSpace(students[0].BirthdayDate)
                ? $"Birthday on : {students[0].BirthdayDate}"
                : "Birthday on : ";

            ws.Range(currentRow, 1, currentRow, 6).Merge();
            ws.Cell(currentRow, 1).Value = birthdayLabel;
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Range(currentRow, 7, currentRow, 9).Merge();
            ws.Cell(currentRow, 7).Value = $"Dated: {DateTime.Now:dd-MM-yyyy hh:mm tt}";
            ws.Cell(currentRow, 7).Style.Font.Bold = true;
            ws.Cell(currentRow, 7).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            int tableHeaderRow = currentRow;

            ws.Cell(currentRow, 1).Value = "Sr No";
            ws.Cell(currentRow, 2).Value = "Student No";
            ws.Cell(currentRow, 3).Value = "Student Name";
            ws.Cell(currentRow, 4).Value = "Date of Birth";
            ws.Cell(currentRow, 5).Value = "Class";
            ws.Cell(currentRow, 6).Value = "Father Name";
            ws.Cell(currentRow, 7).Value = "Mobile No";
            ws.Cell(currentRow, 8).Value = "Email Id";
            ws.Cell(currentRow, 9).Value = "Birthday Date";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Row(currentRow).Height = 22;

            currentRow++;

            int srNo = 1;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = srNo;
                ws.Cell(currentRow, 2).Value = item.ControlNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";

                if (item.DateOfBirth.HasValue)
                {
                    ws.Cell(currentRow, 4).Value = item.DateOfBirth.Value;
                    ws.Cell(currentRow, 4).Style.DateFormat.Format = "dd-MM-yyyy";
                }

                ws.Cell(currentRow, 5).Value = item.ClassSection ?? "";
                ws.Cell(currentRow, 6).Value = item.FatherName ?? "";
                ws.Cell(currentRow, 7).Value = item.SMSMobileNo ?? "";
                ws.Cell(currentRow, 8).Value = item.FatherEMail ?? item.MotherEMail ?? "";
                ws.Cell(currentRow, 9).Value = item.BirthdayDate ?? "";

                for (int col = 1; col <= totalColumns; col++)
                {
                    ws.Cell(currentRow, col).Style.Alignment.Horizontal =
                        col == 3 || col == 6 ? XLAlignmentHorizontalValues.Left : XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                ws.Row(currentRow).Height = 20;

                srNo++;
                currentRow++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= tableHeaderRow)
            {
                var completeTableRange = ws.Range(tableHeaderRow, 1, lastDataRow, totalColumns);

                completeTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Column(1).Width = 7;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 22;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 12;
            ws.Column(6).Width = 20;
            ws.Column(7).Width = 14;
            ws.Column(8).Width = 24;
            ws.Column(9).Width = 14;

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }

        public async Task<byte[]> GenerateStudentSiblingExcel(List<SiblingListResponse> students)
        {
            students ??= new List<SiblingListResponse>();

            using var workbook = new XLWorkbook();

            var ws = workbook.AddWorksheet("Student Sibling Report");

            const int totalColumns = 8;

            int currentRow = 1;

            var sessionName = students.Count > 0 ? students[0].SessionName : null;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "Cambridge School, Noida";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 16;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = "Sector-27, Noida, Uttar Pradesh 201301";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 10;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
            ws.Cell(currentRow, 1).Value = string.IsNullOrWhiteSpace(sessionName)
                ? "Student Sibling Report"
                : $"Student Sibling report for session {sessionName}";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Font.FontSize = 11;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            int tableHeaderRow = currentRow;

            ws.Cell(currentRow, 1).Value = "Sr. No";
            ws.Cell(currentRow, 2).Value = "ControlNo";
            ws.Cell(currentRow, 3).Value = "StudentName";
            ws.Cell(currentRow, 4).Value = "ClassSection";
            ws.Cell(currentRow, 5).Value = "FatherName";
            ws.Cell(currentRow, 6).Value = "MotherName";
            ws.Cell(currentRow, 7).Value = "SMSMobileNo";
            ws.Cell(currentRow, 8).Value = "SiblingDetails";

            var headerRange = ws.Range(currentRow, 1, currentRow, totalColumns);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            int srNo = 1;

            foreach (var item in students)
            {
                ws.Cell(currentRow, 1).Value = srNo;
                ws.Cell(currentRow, 2).Value = item.ControlNo ?? "";
                ws.Cell(currentRow, 3).Value = item.StudentName ?? "";
                ws.Cell(currentRow, 4).Value = item.ClassSection ?? "";
                ws.Cell(currentRow, 5).Value = item.FatherName ?? "";
                ws.Cell(currentRow, 6).Value = item.MotherName ?? "";
                ws.Cell(currentRow, 7).Value = item.SMSMobileNo ?? "";
                ws.Cell(currentRow, 8).Value = item.SiblingDetails ?? "";

                for (int col = 1; col <= totalColumns; col++)
                {
                    ws.Cell(currentRow, col).Style.Alignment.Horizontal =
                        col == 3 || col == 5 || col == 6 || col == 8
                            ? XLAlignmentHorizontalValues.Left
                            : XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                ws.Cell(currentRow, 8).Style.Alignment.WrapText = true;

                srNo++;
                currentRow++;
            }

            int lastDataRow = currentRow - 1;

            if (lastDataRow >= tableHeaderRow)
            {
                var completeTableRange = ws.Range(tableHeaderRow, 1, lastDataRow, totalColumns);

                completeTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                completeTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns(1, totalColumns).AdjustToContents();

            if (lastDataRow >= tableHeaderRow)
            {
                if (ws.Column(8).Width > 40)
                {
                    ws.Column(8).Width = 40;
                }

                ws.Rows(tableHeaderRow, lastDataRow).AdjustToContents();
            }

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.PagesWide = 1;
            ws.PageSetup.Margins.Top = 0.3;
            ws.PageSetup.Margins.Bottom = 0.3;
            ws.PageSetup.Margins.Left = 0.2;
            ws.PageSetup.Margins.Right = 0.2;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return await Task.FromResult(stream.ToArray());
        }
    }


}
