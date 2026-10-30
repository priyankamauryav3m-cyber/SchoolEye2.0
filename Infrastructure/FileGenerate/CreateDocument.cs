using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FileGenerate
{

    public class CreateDocument : IDocument
    {
        public RegistrationDto SelectedRow { get; set; }


        public DocumentMetadata GetMetadata() => new DocumentMetadata();

        public DocumentSettings GetSettings() => new DocumentSettings();

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().ShowOnce().Element(ComposeHeader);

                page.Content().Element(ComposeContent);

                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Border(1)
                .BorderColor(Colors.Black)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().AlignCenter().Text("V3M International School")
                        .Bold().FontSize(14);

                    col.Item().AlignCenter().Text(
                        "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017")
                        .FontSize(9);
                });
        }

        void ComposeContent(IContainer container)
        {
            if (SelectedRow == null)
                return;

            container
                 .Border(1)
                 .BorderColor(Colors.Black)
                 .Padding(15)
                 .Column(col =>
                 {
                     col.Spacing(6);

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text($"Registration No : {SelectedRow.RegistrationNo}");
                         row.RelativeItem().AlignRight().Text($"Registration Date : {SelectedRow.AppliedDate}");
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text($"Student Name : {SelectedRow.StudentName}");
                         row.RelativeItem().AlignRight().Text($"Father Name : {SelectedRow.FatherName}");
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text($"Applied Class : {SelectedRow.ClassName}");
                         row.RelativeItem().AlignRight().Text($"DOB : {SelectedRow.DateOfBirth}");
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text($"Contact No : {SelectedRow.FatherContactNo}");
                         row.RelativeItem().AlignRight().Text($"Amount : {SelectedRow.RegistrationFee}");
                     });

                     col.Item().PaddingTop(50)
                         .AlignLeft()
                         .Text("This receipt is computer generated and doesn't require signature.")
                         .FontSize(8)
                         .Italic();
                 });


        }

        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(40).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem()
                        .AlignLeft()
                        .Text("Generated on:")
                        .ExtraBold().Italic();

                    row.RelativeItem()
                        .AlignRight()
                        .Text($"{DateTime.Now:dd-MM-yyyy}")
                        .ExtraBold().Italic();
                });

                col.Item()
                    .PaddingTop(3).PaddingBottom(5)
                    .Height(1)
                    .Background(Colors.Grey.Darken1);
                col.Item().PaddingTop(3).AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }
    }

    #region-----------------Print StudentList Pdf Data--------------
    public class StudentListPdfDocument : IDocument
    {
        private readonly List<RegistrationDto> _students;

        public StudentListPdfDocument(List<RegistrationDto> students)
        {
            _students = students;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }
        private void ComposeHeader(IContainer container)
        {
            byte[] logoBytes = File.ReadAllBytes(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png")
            );

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(70).AlignMiddle().Image(logoBytes, ImageScaling.FitArea);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item()
                            .AlignCenter()
                            .Text("V3M International School")
                            .FontSize(24)
                            .ExtraBold();

                        c.Item()
                            .AlignCenter()
                            .PaddingTop(4)
                            .Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017")
                            .FontSize(11)
                            .SemiBold();
                    });
                    row.ConstantItem(160).AlignBottom().AlignRight()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9));
                            text.Span("Date: ").SemiBold();
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        });
                });
                col.Item().PaddingTop(10);
            });
        }
        private void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.ConstantColumn(38);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(38);
                        columns.ConstantColumn(38);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(50);
                    });
                table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Sr.No.").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Reg.No.").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Student Name").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Father Name").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Mobile No.").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Gender").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Class").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("DOB").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("App.Date").ExtraBold();
                        header.Cell().Element(HeaderCell).Text("Fee").ExtraBold();
                    });

                int i = 1;
                decimal totalFee = _students.Sum(x => x.RegistrationFee ?? 0);
                foreach (var hr in _students)
                {
                    table.Cell().Element(Cell).Text(i++.ToString());
                    table.Cell().Element(Cell).Text(hr.RegistrationNo);
                    table.Cell().Element(Cell).Text(hr.StudentName);
                    table.Cell().Element(Cell).Text(hr.FatherName);
                    table.Cell().Element(Cell).Text(hr.FatherContactNo);
                    table.Cell().Element(Cell).Text(hr.Gender);
                    table.Cell().Element(Cell).Text(hr.ClassName);
                    table.Cell().Element(Cell).Text(hr.DateOfBirth);
                    table.Cell().Element(Cell).Text(hr.AppliedDate);
                    table.Cell().Element(Cell).Text(hr.RegistrationFee?.ToString("0.00") ?? "0.00");
                }
                table.Cell().ColumnSpan(9).Border(0).AlignRight().PaddingTop(5).Text("Total :").SemiBold();
                table.Cell().Border(0).AlignRight().PaddingTop(5).Text(totalFee.ToString("0.00")).Bold();
                IContainer HeaderCell(IContainer container)
                {
                    return container.Border(1).Padding(4).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.FontSize(9).SemiBold()).MinHeight(22);
                }

                IContainer Cell(IContainer container)
                {
                    return container.Border(1).Padding(3).AlignMiddle().DefaultTextStyle(x => x.FontSize(8)).MinHeight(22);
                }
            });
        }
        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem()
                        .AlignLeft()
                        .Text("Generated on:")
                        .Italic()
                        .SemiBold();

                    row.RelativeItem()
                        .AlignRight()
                        .Text($"{DateTime.Now:dd-MM-yyyy}")
                        .Italic()
                        .SemiBold();
                });

                col.Item().PaddingVertical(5).Height(1).Background(Colors.Grey.Darken1);
                col.Item().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }

    }
    #endregion

    #region-----------------Print Admission Slip Pdf Data--------------
    public class AdmissionSlipDocument : IDocument
    {
        private readonly List<StudentListResponse> _listResponse;

        public AdmissionSlipDocument(List<StudentListResponse> listResponse)
        {
            _listResponse = listResponse;
        }


        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            foreach (var student in _listResponse)
            {
                var logoPath = Path.Combine("wwwroot", "uploads", "Logos", "logo (1).png");
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Row(row =>
                    {
                        row.AutoItem().Height(100).Image(logoPath).FitArea();
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("CAMBRIDGE SCHOOL ").FontSize(30).FontColor(Colors.Blue.Darken4);
                            col.Item().AlignRight().Text("Noida").FontSize(25).FontColor(Colors.Blue.Darken4);
                        });
                    });
                    page.Content().Column(col =>
                    {
                        //col.Spacing(8);
                        col.Item().Container()
                        .Border(1)
                        .Background(Colors.Grey.Lighten2)
                        .PaddingVertical(8)
                        .AlignCenter()
                        .Text("ADMISSION SLIP")
                        .Bold()
                        .FontSize(14);


                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            void Row(string label, string value)
                            {
                                table.Cell().Border(1).Padding(5).Text(label).Bold();
                                table.Cell().Border(1).Padding(5).Text(value ?? "");
                            }

                            Row("Registration No.", student.RegistrationNo);
                            Row("Admission No. Allotted", student.AdmissionNo);
                            Row("Date of Admission", student.AdmissionDate);
                            Row("Name of the Student", student.StudentName);
                            Row("Gender", student.Gender);
                            Row("Father's Name", student.FatherName);
                            Row("Mother's Name", student.MotherName);
                            Row("Date of Birth", student.DateOfBirth);
                            Row("Address", student.Address);
                            Row("SMS Mobile", student.SMSMobileNo);
                            Row("Father's Mobile", student.SMSMobileNo);
                            Row("Mother's Mobile", student.SMSMobileNo);
                            Row("Contact Email", student.FatherEmail);
                            Row("Class Admitted", student.ClassName);
                            Row("Date of Fee Deposit", "");
                            Row("Mode of Payment", student.PaymentMode);
                        });
                        col.Item().Container()
                       .Border(1)
                       .Background(Colors.Grey.Lighten2)
                       .PaddingVertical(6)
                       .AlignCenter()
                       .Text("Parent Portal / Mobile App Login Details")
                       .Bold()
                       .FontSize(14);


                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            void Row(string label, string value)
                            {
                                table.Cell().Border(1).Padding(5).Text(label).Bold();
                                table.Cell().Border(1).Padding(5).Text(value);
                            }

                            Row("User Name", "CSN9899");
                            Row("Default Password", "ab@123");
                        });

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Border(1).Height(100).AlignCenter().AlignMiddle()
                                .Text("QR CODE");

                            row.RelativeItem().Border(1).Height(100).AlignCenter().AlignMiddle()
                                .Text("QR CODE");

                            row.RelativeItem().Border(1).Height(100).AlignCenter().AlignMiddle()
                                .Text("QR CODE");
                        });

                        col.Item().PaddingTop(5).Text("Note: Please change the default password")
                            .FontColor(Colors.Red.Darken3).FontSize(10).Bold();

                        col.Item().PaddingTop(20).AlignLeft()
                            .Text("Headmistress / Principal");
                    });
                    page.Footer().Element(ComposeFooter);
                });

                void ComposeFooter(IContainer container)
                {
                    container.PaddingTop(10).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .AlignLeft()
                                .Text($"Generated on: {DateTime.Now:dd-MM-yyyy}")
                                .Italic()
                                .SemiBold();

                            row.RelativeItem()
                                .AlignRight()
                                .Text(text =>
                                {
                                    text.Span("Page ").Bold();
                                    text.CurrentPageNumber().Bold();
                                    text.Span(" of ").Bold();
                                    text.TotalPages().Bold();
                                });
                        });

                        col.Item()
                            .PaddingVertical(5)
                            .Height(1)
                            .Background(Colors.Grey.Darken1);
                    });
                }
            }
        }


    }

    #endregion

    #region-----------------Print Regitration Form Pdf Data--------------
    public class RegistrationFormDocument : IDocument
    {
        private readonly List<StudentListResponse> _listResponse;

        public RegistrationFormDocument(List<StudentListResponse> listResponse)
        {
            _listResponse = listResponse;
        }


        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            foreach (var model in _listResponse)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        // ===== HEADER =====
                        col.Item().AlignCenter().Text("CAMBRIDGE SCHOOL, NOIDA")
                            .Bold().FontSize(14);

                        col.Item().AlignCenter().Text("SECTOR-27, NOIDA, UTTAR PRADESH 201301");

                        col.Item().AlignCenter().Text("CLASS I NOIDA.CAMBRIDGESCHOOL.EDU.IN");

                        col.Item().PaddingVertical(5)
                            .Text("NOTE: PLEASE FILL THE FORM IN CAPITAL LETTERS")
                            .Bold().Underline();



                        // ===== TABLE =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(4);
                            });

                            void Row(string label, string value)
                            {
                                table.Cell().Border(1).Padding(5).Text(label);
                                table.Cell().Border(1).Padding(5).Text(value ?? "");
                            }

                            Row("ADMISSION NO", model.AdmissionNo);
                            Row("DATE OF ADMISSION", model.AdmissionDate);
                            Row("ROLL NO", model.RollNo?.ToString());
                            Row("NAME OF THE STUDENT", model.StudentName);
                            Row("DATE OF BIRTH", model.DateOfBirth);
                            Row("DATE OF BIRTH (IN WORDS)", "");
                            Row("GENDER", model.Gender);
                            Row("MOTHER’S NAME", model.MotherName);
                            Row("FATHER’S NAME", model.FatherName);
                            Row("CASTE (SC,ST,OBC)", "");
                            table.Cell().Border(1).Padding(5).Text("MINORITY");
                            table.Cell().Border(1).Padding(5).Row(r =>
                            {
                                r.RelativeItem().AlignCenter().Text("YES");
                                r.RelativeItem().AlignCenter().Text("NO");
                            });

                            Row("AADHAR NO OF STUDENT", "");

                            // ================= PWD ROW (Correct Layout) =================
                            table.Cell().Border(1).Padding(5).Text("PWD");

                            table.Cell().Border(1).Padding(0).Table(inner =>
                            {
                                inner.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                void PwdCell(string text)
                                {
                                    inner.Cell().Border(1).Padding(5).AlignCenter().Text(text);
                                }

                                PwdCell("NA");
                                PwdCell("BLIND");
                                PwdCell("DEAF");
                                PwdCell("SPASTIC");
                                PwdCell("HANDICAPPED");
                                PwdCell("DYSLEXIC");
                            });

                            Row("MOBILE NO", model.SMSMobileNo);
                            Row("ANNUAL INCOME OF PARENTS", "10000");

                            table.Cell().Border(1).Padding(5).Text("ONLY CHILD OF PARENTS");
                            table.Cell().Border(1).Padding(5).Row(r =>
                            {
                                r.RelativeItem().AlignCenter().Text("YES");
                                r.RelativeItem().AlignCenter().Text("NO");
                            });

                            Row("EMAIL ID", "");
                        });

                        // ===== SIGNATURE AREA =====
                        col.Item().PaddingTop(50).Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Text("Parent’s Signature");
                            row.RelativeItem().AlignCenter().Text("Student Signature");
                            row.RelativeItem().AlignRight().Text("Class In charge Signature");
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
                void ComposeFooter(IContainer container)
                {
                    container.PaddingTop(10).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .AlignLeft()
                                .Text($"Generated on: {DateTime.Now:dd-MM-yyyy}")
                                .Italic()
                                .SemiBold();

                            row.RelativeItem()
                                .AlignRight()
                                .Text(text =>
                                {
                                    text.Span("Page ").Bold();
                                    text.CurrentPageNumber().Bold();
                                    text.Span(" of ").Bold();
                                    text.TotalPages().Bold();
                                });
                        });

                        col.Item()
                            .PaddingVertical(5)
                            .Height(1)
                            .Background(Colors.Grey.Darken1);
                    });
                }
            }
        }


    }
    #endregion

    #region-----------------Print Bonafied Pdf Data--------------
    public class BonafideCertificateDocument : IDocument
    {
        private readonly StudentListResponse _model;

        public BonafideCertificateDocument(StudentListResponse model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().PaddingTop(50).Column(col =>
                {
                    col.Spacing(15);

                    // ===== Title =====
                    col.Item().AlignCenter().Text("PROFORMA BONAFIDE CERTIFICATE")
                        .Bold()
                        .FontSize(16)
                        .Underline();

                    // ===== Body =====
                    col.Item().PaddingTop(50).Text(text =>
                    {
                        text.Span("It is certified that Master/Baby/Mr./Ms ");
                        text.Span(_model.StudentName).Bold().Underline();
                        text.Span(" Admission No. ");
                        text.Span(_model.AdmissionNo).Bold().Underline();
                        text.Span("\n");

                        text.Span("Date of Birth ");
                        text.Span(_model.DateOfBirth).Bold().Underline();
                        text.Span(" Son/Daughter of Shri/Smt ");
                        text.Span(_model.FatherName).Bold().Underline();
                        text.Span("\n");

                        text.Span("studied in Class ");
                        text.Span(_model.ClassName).Bold().Underline();
                        text.Span(" during the previous Academic year from 2025 to 2026 in this School/Institution.");
                        text.Span("\n\n");

                        text.Span("This Institution/School affiliation/recognition number is 2132293.");
                        text.Span("\n\n");

                        text.Span("During the year Master/Baby/Mr./Ms ");
                        text.Span(_model.StudentName).Bold().Underline();
                        text.Span(" has resided in the residential complex (hostel) of the school and paid amount of Rs. __________ (Rupees ____________________) towards boarding and lodging.");
                        text.Span("\n\n");

                        text.Span("(Strike out if not applicable)").Italic();
                    });
                    col.Item().PaddingTop(60);

                    // ===== Signature Section =====
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Seal with Date");
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("Signature of the Head of the Institution/School");
                        });
                    });
                });
            });
        }
    }
    #endregion

    #region-----------------Print Not Promoted Student Pdf Data--------------
    public class StudentNotPromotedPdfDocument : IDocument
    {
        private readonly List<StudentNotPromotedModel> _students;

        public StudentNotPromotedPdfDocument(List<StudentNotPromotedModel> students)
        {
            _students = students;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(18);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }
        public void ComposeHeader(IContainer container)
        {
            byte[] logoBytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png"));

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(85).Height(50).AlignMiddle().Image(logoBytes, ImageScaling.FitArea);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("V3M International School").FontSize(22).ExtraBold();
                        c.Item().PaddingTop(2).AlignCenter().Text("Not Promotion List Of Students").FontSize(11).SemiBold();
                        c.Item().PaddingTop(2).AlignCenter().Text("Session 2025-2026").FontSize(10).SemiBold();
                    });

                    row.ConstantItem(170).AlignBottom().AlignRight()
                         .Text(text =>
                         {
                             text.DefaultTextStyle(x => x.FontSize(9));

                             text.Span("Print Date : ")
                                .SemiBold();

                             text.Span(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                         });
                });

                col.Item().PaddingTop(8);
            });
        }
        public void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(45);
                });
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Sl.No.");
                    header.Cell().Element(HeaderCell).Text("Admission No.");
                    header.Cell().Element(HeaderCell).Text("Student Name");
                    header.Cell().Element(HeaderCell).Text("Class Section");
                    header.Cell().Element(HeaderCell).Text("EWS");
                    header.Cell().Element(HeaderCell).Text("Due");
                    header.Cell().Element(HeaderCell).Text("Active");
                });

                foreach (var s in _students)
                {
                    table.Cell().Element(CompactCell).Text(s.SlNo.ToString());
                    table.Cell().Element(CompactCell).Text(s.AdmissionNo ?? "");
                    table.Cell().Element(NameCell).Text(s.StudentName ?? "");
                    table.Cell().Element(CompactCell).Text(s.ClassSection ?? "");
                    table.Cell().Element(CompactCell).Text(s.EWS ?? "");
                    table.Cell().Element(RightCell).Text(s.Due.ToString("0.00"));
                    table.Cell().Element(CompactCell).Text(s.Active ?? "");
                }
            });
        }

        static IContainer HeaderCell(IContainer container) =>
        container.Border(1).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(8).ExtraBold());
        static IContainer CompactCell(IContainer container) =>
          container.Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));

        static IContainer NameCell(IContainer container) =>
          container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        static IContainer RightCell(IContainer container) =>
        container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        public void ComposeFooter(IContainer container)
        {
            container.PaddingTop(25).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic();
                    row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic();
                });

                col.Item().PaddingTop(3).PaddingBottom(5).Height(1).Background(Colors.Grey.Darken1);

                col.Item().PaddingTop(3).AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").Bold();
                        text.CurrentPageNumber().Bold();
                        text.Span(" of ").Bold();
                        text.TotalPages().Bold();
                    });
            });
        }
    }
    #endregion

    #region-----------------Print All Student Challan Dues Pdf Data--------------

    public class AllStudentPdfChallanDuesDocument : IDocument
    {
        private readonly List<StudentDuesModel> _students;

        public AllStudentPdfChallanDuesDocument(List<StudentDuesModel> students)
        {
            _students = students;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }
        public void ComposeHeader(IContainer container)
        {
            byte[] logoBytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png"));

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(85).Height(50).AlignMiddle().Image(logoBytes, ImageScaling.FitArea);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("V3M International School").FontSize(22).ExtraBold();
                        c.Item().PaddingTop(2).AlignCenter().Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017").FontSize(11).SemiBold();
                        c.Item().PaddingTop(2).AlignCenter().Text("Fee Due Report").FontSize(10).SemiBold();
                    });

                    row.ConstantItem(170).AlignBottom().AlignRight()
                         .Text(text =>
                         {
                             text.DefaultTextStyle(x => x.FontSize(9));
                             text.Span("Print Date : ").SemiBold();
                             text.Span(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                         });
                });

                col.Item().PaddingTop(8);
            });
        }
        public void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(90);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(34);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(50);
                });
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Sr.No.");
                    header.Cell().Element(HeaderCell).Text("Student No.");
                    header.Cell().Element(NameHeaderCell).Text("Student Name");
                    header.Cell().Element(NameHeaderCell).Text("Father Name");
                    header.Cell().Element(NameHeaderCell).Text("Address");
                    header.Cell().Element(HeaderCell).Text("Class Section");
                    header.Cell().Element(HeaderCell).Text("Category");
                    header.Cell().Element(HeaderCell).Text("Mobile No.");
                    header.Cell().Element(HeaderCell).Text("Contact Email");
                    header.Cell().Element(HeaderCell).Text("Due Amount");
                });

                int srNo = 1;

                foreach (var s in _students)
                {
                    table.Cell().Element(CompactCell).Text(srNo.ToString());
                    table.Cell().Element(CompactCell).Text(s.ControlNo ?? "");
                    table.Cell().Element(NameCell).Text(s.StudentName ?? "");
                    table.Cell().Element(NameCell).Text(s.FatherName ?? "");
                    table.Cell().Element(NameCell).Text(s.CurrentAddress ?? "");
                    table.Cell().Element(CompactCell).Text(s.ClassSection ?? "");
                    table.Cell().Element(CompactCell).Text(s.Category ?? "");
                    table.Cell().Element(CompactCell).Text(s.SMSMobileNo ?? "");
                    table.Cell().Element(CompactCell).Text(s.FatherEMail ?? "");
                    table.Cell().Element(RightCell).Text(s.Amount.ToString("0.00"));
                    srNo++;
                }
            });
        }
        static IContainer NameHeaderCell(IContainer container) =>
          container.Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).ExtraBold());
        static IContainer HeaderCell(IContainer container) =>
          container.Border(1).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).ExtraBold());
        static IContainer CompactCell(IContainer container) =>
          container.Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));

        static IContainer NameCell(IContainer container) =>
          container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        static IContainer RightCell(IContainer container) =>
        container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));


        public void ComposeFooter(IContainer container)
        {
            container.PaddingTop(25).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic();
                    row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic();
                });

                col.Item().PaddingTop(3).PaddingBottom(5).Height(1).Background(Colors.Grey.Darken1);

                col.Item().PaddingTop(3).AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").Bold();
                        text.CurrentPageNumber().Bold();
                        text.Span(" of ").Bold();
                        text.TotalPages().Bold();
                    });
            });
        }
    }
    #endregion

    #region-----------------Print Challan Pdf Data--------------

    public class FeeChallanPdfDocument : IDocument
    {
        private readonly List<InvoiceDetailsResponse> _model;
        public FeeChallanPdfDocument(List<InvoiceDetailsResponse> model)
        {
            _model = model;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public void Compose(IDocumentContainer container)
        {
            foreach (var student in _model)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(7);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Row(row =>
                    {
                        row.RelativeItem()
                            .Element(x => ComposeSlip(x, "PARENT COPY", student));

                        row.ConstantItem(10);

                        row.RelativeItem()
                            .Element(x => ComposeSlip(x, "SCHOOL COPY", student));

                        row.ConstantItem(10);

                        row.RelativeItem()
                            .Element(x => ComposeSlip(x, "BANK COPY", student));
                    });
                });
            }
        }

        private void ComposeSlip(IContainer container, string copyName, InvoiceDetailsResponse student)
        {
            container.Column(col =>
            {
                col.Item().Element(x => ComposeHeader(x, copyName));

                col.Item().PaddingTop(5).Element(x => ComposeStudentDetails(x, student));

                col.Item().PaddingTop(3).Element(x => ComposeFeeTable(x, student));

                col.Item().PaddingTop(2).Element(ComposeChequeSection);

                col.Item().PaddingTop(2).Element(ComposeNoteSection);
            });
        }
        private void ComposeHeader(IContainer container, string copyName)
        {
            var logo = Path.Combine("wwwroot", "uploads", "Logos", "logo (1).png");

            container.Column(col =>
            {
                col.Item()
                    .AlignCenter()
                    .Text($"FEE SLIP - {copyName} (2026-27)")
                    .Bold()
                    .FontSize(10);

                col.Item().PaddingTop(3);

                col.Item().Row(row =>
                {
                    if (File.Exists(logo))
                    {
                        row.ConstantItem(55).Image(logo);
                    }

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("V3M International School").FontSize(15).Bold();

                        c.Item().AlignCenter().Text("FF-231 232, Palam Corporate Plaza");

                        c.Item().AlignCenter().Text("Palam Vihar, 122017");

                        c.Item().AlignCenter().Text("08373918357");
                    });
                });

                col.Item().PaddingTop(5).Text("BANKERS : INDIAN OVERSEAS BANK, Alpha-I, Greater Noida").Bold();
            });
        }
        private void ComposeStudentDetails(IContainer container, InvoiceDetailsResponse student)
        {
            var master = student.InvoiceMaster?.FirstOrDefault();

            container.Column(col =>
            {
                AddDetailRow(col, "Admission No.", master?.ControlNo ?? "");
                AddDetailRow(col, "Student Name", master?.StudentName ?? "");
                AddDetailRow(col, "Class-Section", master?.ClassSection ?? "");
                AddDetailRow(col, "Period (Qtr)", master?.PeriodName ?? "");
                AddDetailRow(col, "Father's Name", master?.FatherName ?? "");
                AddDetailRow(col, "Mobile No.", master?.SMSMobileNo ?? "");
            });
        }
        private void AddDetailRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(110).Text(label);
                r.ConstantItem(10).Text(":");
                r.RelativeItem().Text(value).Bold();
            });
        }
        private void ComposeFeeTable(IContainer container, InvoiceDetailsResponse student)
        {
            var master = student.InvoiceMaster?.FirstOrDefault();
            decimal totalPayable = student.FeeHeadList.Sum(x => x.Payable);
            decimal totalConcession = student.FeeHeadList.Sum(x => x.Concession);
            decimal lateFee = master?.StudentLateFee ?? 0;
            decimal grandTotal = totalPayable + lateFee;

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.ConstantColumn(70);
                    c.ConstantColumn(70);
                    c.ConstantColumn(70);
                });

                table.Cell().ColumnSpan(4).Border(1).Padding(2)
                    .Background(Colors.Grey.Lighten2).AlignCenter()
                    .Text($"Fee for - {master?.PeriodName ?? " "} {master?.DueDate.ToString(" yyyy") ?? ""}").SemiBold();
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten3).Text("Fee Head").SemiBold();
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten3).AlignRight().Text("Amount").SemiBold();
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten3).AlignRight().Text("Concession").SemiBold();
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten3).AlignRight().Text("Payable").SemiBold();
                foreach (var fee in student.FeeHeadList)
                {
                    table.Cell().Border(1).Padding(2).Text(fee.FeeHeadName);
                    table.Cell().Border(1).Padding(2).AlignRight().Text(fee.FeeHeadAmount.ToString("N2"));
                    table.Cell().Border(1).Padding(2).AlignRight().Text(fee.Concession.ToString("N2"));
                    table.Cell().Border(1).Padding(2).AlignRight().Text(fee.Payable.ToString("N2"));
                }
                table.Cell().Border(1).Padding(2).Text("Total").Bold();
                table.Cell().Border(1).Padding(2).AlignRight().Text("").Bold();
                table.Cell().Border(1).Padding(2).AlignRight().Text(totalConcession.ToString("N2")).Bold();
                table.Cell().Border(1).Padding(2).AlignRight().Text(totalPayable.ToString("N2")).Bold();
                table.Cell().Border(1).Padding(2).Text("Fine / Late Fee").SemiBold();
                table.Cell().ColumnSpan(2).Border(1);
                table.Cell().Border(1).Padding(2).AlignRight().Text(lateFee > 0 ? lateFee.ToString("N2") : "").SemiBold();
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten2).Text("Total Payable").Bold();
                table.Cell().ColumnSpan(2).Border(1).Background(Colors.Grey.Lighten2);
                table.Cell().Border(1).Padding(2).Background(Colors.Grey.Lighten2).AlignRight().Text(grandTotal.ToString("N2")).Bold();
            });
        }
        private void ComposeChequeSection(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                AddChequeRow(table, "Cheque/DD No. & Date:");
                AddChequeRow(table, "Name of the Bank:");
                AddChequeRow(table, "Amount (Rs.):");
            });
        }
        private void AddChequeRow(TableDescriptor table, string text)
        {
            table.Cell().Border(1).Padding(2).Text(text).SemiBold();
            table.Cell().Border(1).Padding(2).Text("");
        }
        private void ComposeNoteSection(IContainer container)
        {
            container.ShowEntire().Column(col =>
            {
                col.Spacing(2);

                col.Item().Text($"Printed on : {DateTime.Now:dd MMM yyyy hh:mm:ss tt}");

                col.Item().PaddingTop(3);

                col.Item().Text("NOTE:").Bold();

                col.Item().Text("1. Payment by cheque is always convenient however, If cheque is dishonoured due to insufficient funds, bank charges will be levied.");
                col.Item().Text("2. Please write admission no., name, class of the student at the back of cheque.");
                col.Item().Text("3. Please issue separate cheques for separate child in case of siblings.");
                col.Item().Text("4. Please issue cheque/DD in favour of 'V3M INTERNATIONAL SCHOOL'.");
                col.Item().Text("5. Duplicate fee bill will not be issued.");
                col.Item().Text("6. Please visit our website to pay fee online.");
                col.Item().Text("7. CASH DEPOSITS at INDIAN OVERSEAS BANK, Alpha I, Greater Noida only.");
            });
        }
    }

    #endregion


    #region---------------- Print Receipt --------------
    public class RegistrationReceiptPdfDocument : IDocument
    {
        private readonly List<RegistrationReceiptResponse> _receipts;
        private readonly DateTime? _fromDate;
        private readonly DateTime? _toDate;

        public RegistrationReceiptPdfDocument(List<RegistrationReceiptResponse> receipts)
        {
            _receipts = receipts;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(18);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Black));
                page.Header().Element(ComposeHeader);

                page.Content().Element(ComposeContent);

                page.Footer().Element(ComposeFooter);
            });
        }
        private void ComposeHeader(IContainer container)
        {
            byte[] logoBytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png"));

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(85).Height(50).AlignMiddle().Image(logoBytes, ImageScaling.FitArea);
                    row.RelativeItem()
                       .Column(c =>
                       {
                           c.Item().AlignCenter().Text("V3M International School").FontSize(18).ExtraBold();
                           c.Item().PaddingTop(2).AlignCenter().Text("Registration Fee Collection Report").FontSize(11).SemiBold();

                           c.Item().PaddingTop(2).AlignCenter().Text(text => { text.Span("Registration Fee Receipt").FontSize(10).SemiBold(); });
                       });
                    row.ConstantItem(140).AlignBottom().AlignRight()
                    .Text(text =>
                   {
                       text.DefaultTextStyle(x => x.FontSize(8));
                       text.Span("Print Date : ").SemiBold();
                       text.Span(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                   });
                });

                col.Item().PaddingTop(8);
            });
        }
        private void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {

                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.ConstantColumn(65);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(55);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Sr.");
                    header.Cell().Element(HeaderCell).Text("Reg. No.");
                    header.Cell().Element(HeaderCell).Text("Student Name");
                    header.Cell().Element(HeaderCell).Text("Class");
                    header.Cell().Element(HeaderCell).Text("Amount");
                    header.Cell().Element(HeaderCell).Text("Date");
                    header.Cell().Element(HeaderCell).Text("Mode");
                });

                int srNo = 1;

                foreach (var item in _receipts)
                {
                    table.Cell().Element(CompactCell).Text(srNo.ToString());
                    table.Cell().Element(CompactCell).Text(item.RegistrationNo ?? "");
                    table.Cell().Element(NameCell).Text(item.StudentName ?? "");
                    table.Cell().Element(CompactCell).Text(item.ClassName ?? "");
                    table.Cell().Element(RightCell).Text(item.Amount.ToString("0.00"));
                    table.Cell().Element(CompactCell).Text(item.ReceiptDate ?? "");
                    table.Cell().Element(CompactCell).Text(item.PaymentMode ?? "");

                    srNo++;
                }

                if (_receipts.Any())
                {
                    table.Cell().ColumnSpan(4).Element(TotalLabelCell).Text("Total");
                    table.Cell().Element(TotalAmountCell).Text(_receipts.Sum(x => x.Amount).ToString("0.00"));
                    table.Cell().ColumnSpan(2).Element(TotalLabelCell).Text("");
                }
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(8).ExtraBold());
        }

        private static IContainer CompactCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer NameCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer RightCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer TotalLabelCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignRight().AlignMiddle().DefaultTextStyle(x => x.FontSize(8).Bold());
        }

        private static IContainer TotalAmountCell(IContainer container)
        {
            return container.Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignRight().AlignMiddle().DefaultTextStyle(x => x.FontSize(8).Bold());
        }

        private void ComposeFooter(IContainer container)
        {
            container
                .PaddingTop(15)
                .Column(col =>
                {
                    col.Item()
                       .Row(row =>
                       {
                           row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic();
                           row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic();
                       });

                    col.Item().PaddingTop(3).PaddingBottom(5).Height(1).Background(Colors.Grey.Darken1);

                    col.Item().PaddingTop(3).AlignRight()
                       .Text(text =>
                       {
                           text.Span("Page ").Bold();

                           text.CurrentPageNumber().Bold();

                           text.Span(" of ").Bold();

                           text.TotalPages().Bold();
                       });
                });
        }
    }

    public class PrinReceiptDocument : IDocument
    {
        public RegistrationReceiptResponse SelectedRow { get; set; }


        public DocumentMetadata GetMetadata() => new DocumentMetadata();

        public DocumentSettings GetSettings() => new DocumentSettings();

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().ShowOnce().Element(ComposeHeader);

                page.Content().Element(ComposeContent);

                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Border(1)
                .BorderColor(Colors.Black)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().AlignCenter().Text("V3M International School")
                        .Bold().FontSize(14);

                    col.Item().AlignCenter().Text(
                        "FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017")
                        .FontSize(9);
                });
        }

        void ComposeContent(IContainer container)
        {
            if (SelectedRow == null)
                return;

            container
                 .Border(1)
                 .BorderColor(Colors.Black)
                 .Padding(15)
                 .Column(col =>
                 {
                     col.Spacing(6);

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text(text =>
                         {
                             text.Span("Registration No : ").Bold();
                             text.Span(SelectedRow.RegistrationNo ?? " ");
                         });

                         row.RelativeItem().AlignRight().Text(text =>
                         {
                             text.Span("Registration Date : ").Bold();
                             text.Span(SelectedRow.ReceiptDate ?? " ");
                         });
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text(text =>
                         {
                             text.Span("Student Name : ").Bold();
                             text.Span(SelectedRow.StudentName ?? "");
                         });

                         row.RelativeItem().AlignRight().Text(text =>
                         {
                             text.Span("Father Name : ").Bold();
                             text.Span(SelectedRow.FatherName ?? " ");
                         });
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text(text =>
                         {
                             text.Span("Applied Class : ").Bold();
                             text.Span(SelectedRow.ClassName ?? " ");
                         });

                         row.RelativeItem().AlignRight().Text(text =>
                         {
                             text.Span("DOB : ").Bold();
                             text.Span(SelectedRow.DOB ?? " ");
                         });
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text(text =>
                         {
                             text.Span("Contact No : ").Bold();
                             text.Span(SelectedRow.SMSMobileNo ?? " ");
                         });

                         row.RelativeItem().AlignRight().Text(text =>
                         {
                             text.Span("Amount : ").Bold();
                             text.Span(SelectedRow.Amount.ToString("0.00"));
                         });
                     });

                     col.Item().Row(row =>
                     {
                         row.RelativeItem().Text(text =>
                         {
                             text.Span("Payment Mode : ").Bold();
                             text.Span(SelectedRow.PaymentMode ?? " ");
                         });
                     });

                     col.Item().PaddingTop(50)
                         .AlignLeft()
                         .Text("This receipt is computer generated and doesn't require signature.")
                         .FontSize(8)
                         .Italic();
                 });


        }

        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(40).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem()
                        .AlignLeft()
                        .Text("Generated on:")
                        .ExtraBold().Italic();

                    row.RelativeItem()
                        .AlignRight()
                        .Text($"{DateTime.Now:dd-MM-yyyy}")
                        .ExtraBold().Italic();
                });

                col.Item()
                    .PaddingTop(3).PaddingBottom(5)
                    .Height(1)
                    .Background(Colors.Grey.Darken1);
                col.Item().PaddingTop(3).AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }
    }
    #endregion
    #region ---------------- Publishing List PDF ----------------

    public class PublishingListPdfDocument : IDocument
    {
        private readonly List<PublishingListResponse> _publishingList;

        public PublishingListPdfDocument(List<PublishingListResponse> publishingList)
        {
            _publishingList = publishingList ?? new List<PublishingListResponse>();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(14);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Black));
                page.Header().ShowOnce().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            byte[]? logoBytes = null;

            string logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "Logos",
                "logo (1).png");

            if (File.Exists(logoPath))
            {
                logoBytes = File.ReadAllBytes(logoPath);
            }

            int listNo = _publishingList.FirstOrDefault()?.ListNo ?? 0;

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(75).Height(58).AlignMiddle().AlignLeft().Element(logo =>
                    {
                        if (logoBytes != null)
                        {
                            logo.Image(logoBytes, ImageScaling.FitArea);
                        }
                    });

                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter().Text("Cambridge School, Noida").FontSize(16).ExtraBold();

                        c.Item().AlignCenter().Text("Sector-27, Noida, Uttar Pradesh 201301").FontSize(9).ExtraBold();

                        c.Item().AlignCenter().Text("noida.cambridgeschool.edu.in").FontSize(8).Bold();
                    });

                    row.ConstantItem(75);
                });

                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(text =>
                    {
                        text.Span("Selected Student Of ").Bold();
                        text.Span($"Publish List {listNo}").Bold();
                        text.Span(" for session ");
                        text.Span("");
                    });

                    row.ConstantItem(150).AlignRight().Text(text =>
                    {
                        text.Span("Date: ").Bold();
                        text.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    });
                });

                col.Item().PaddingBottom(3);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(27);
                    columns.ConstantColumn(65);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(2.1f);
                    columns.RelativeColumn(2.1f);
                    columns.ConstantColumn(43);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(38);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("S. No.");
                    header.Cell().Element(HeaderCell).Text("Reg No.");
                    header.Cell().Element(HeaderCell).Text("Student Name");
                    header.Cell().Element(HeaderCell).Text("Father Name");
                    header.Cell().Element(HeaderCell).Text("Mother Name");
                    header.Cell().Element(HeaderCell).Text("Gender");
                    header.Cell().Element(HeaderCell).Text("Class Name");
                    header.Cell().Element(HeaderCell).Text("Mobile No");
                    header.Cell().Element(HeaderCell).Text("Points");
                });

                int srNo = 1;

                foreach (var item in _publishingList)
                {
                    table.Cell().Element(CenterCell).Text(srNo.ToString());
                    table.Cell().Element(CenterCell).Text(item.RegistrationNo ?? "");
                    table.Cell().Element(NameCell).Text(item.StudentName ?? "");
                    table.Cell().Element(NameCell).Text(item.FatherName ?? "");
                    table.Cell().Element(NameCell).Text(item.MotherName ?? "");
                    table.Cell().Element(CenterCell).Text(item.Gender ?? "");
                    table.Cell().Element(CenterCell).Text(item.ClassName ?? "");
                    table.Cell().Element(CenterCell).Text(item.SMSMobileNo ?? "");
                    table.Cell().Element(CenterCell).Text(item.Points.ToString());

                    srNo++;
                }
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).ExtraBold());
        }

        private static IContainer CenterCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(6.5f));
        }

        private static IContainer NameCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(3).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(6.5f));
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(8).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic().FontSize(7);

                    row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic().FontSize(7);
                });

                col.Item().PaddingTop(3).PaddingBottom(3).Height(1).Background(Colors.Grey.Darken1);

                col.Item().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }
    }

    #endregion
    #region----------------- Class List Pdf -----------------

    public class ClassListPdfDocument : IDocument
    {
        private readonly List<GetSearchedViewStudentModel> _students;
        private readonly int _blankColumnCount;
        private readonly string? _sessionName;
        private readonly string? _className;
        private readonly string? _sectionName;
        private const int StudentsPerPage = 30;

        public ClassListPdfDocument(ClassListRequest request)
        {
            _students = request?.Students ?? new List<GetSearchedViewStudentModel>();
            _blankColumnCount = Math.Clamp(request?.BlankColumnCount ?? 1, 1, 5);
            _sessionName = request?.SessionName;
            _className = request?.ClassName;
            _sectionName = request?.SectionName;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            var studentPages = _students
                .Select((student, index) => new { Student = student, Index = index })
                .GroupBy(x => x.Index / StudentsPerPage)
                .Select(x => x.Select(y => y.Student).ToList())
                .ToList();

            if (!studentPages.Any())
            {
                studentPages.Add(new List<GetSearchedViewStudentModel>());
            }

            foreach (var pageStudents in studentPages)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    page.Header().Element(ComposeHeader);
                    page.Content().PaddingTop(5).Element(content => ComposeContent(content, pageStudents));
                    page.Footer().Element(ComposeFooter);
                });
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(100);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("CAMBRIDGE SCHOOL, NOIDA").FontSize(11).Bold();
                        c.Item().PaddingTop(3).AlignCenter().Text("CLASS LIST").FontSize(9).Bold();
                        c.Item().PaddingTop(2).AlignCenter().Text($"{_className ?? ""} - {_sectionName ?? ""} (SESSION {_sessionName ?? ""})").FontSize(8).Bold();
                        c.Item().PaddingTop(2).AlignCenter().Text("CLASS TEACHER : Prakshi Jain").FontSize(8).Bold();
                    });

                    row.ConstantItem(150).AlignRight().AlignTop().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(6));
                        text.Span("Date: ").Bold();
                        text.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    });
                });

                col.Item().PaddingBottom(5);
            });
        }

        private void ComposeContent(IContainer container, List<GetSearchedViewStudentModel> pageStudents)
        {
            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(65);
                        columns.RelativeColumn(3);

                        for (int i = 0; i < _blankColumnCount; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("S. NO.");
                        header.Cell().Element(HeaderCell).Text("ROLL NO.");
                        header.Cell().Element(HeaderCell).Text("ADM. NO.");
                        header.Cell().Element(HeaderCell).Text("STUDENT NAME");

                        for (int i = 0; i < _blankColumnCount; i++)
                        {
                            header.Cell().Element(HeaderCell).Text("---");
                        }
                    });

                    foreach (var item in pageStudents)
                    {
                        int srNo = _students.IndexOf(item) + 1;

                        table.Cell().Element(DataCell).Text(srNo.ToString());
                        table.Cell().Element(DataCell).Text(item.RollNo ?? "");
                        table.Cell().Element(DataCell).Text(item.ControlNo ?? "");

                        table.Cell().Element(NameCell).Text(text =>
                        {
                            var name = text.Span(item.StudentName ?? "");

                            if (item.Gender?.Equals("Female", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                name.Bold();
                            }
                        });

                        for (int i = 0; i < _blankColumnCount; i++)
                        {
                            table.Cell().Element(DataCell).Text("");
                        }
                    }

                    int blankRows = StudentsPerPage - pageStudents.Count;
                    int totalColumns = 4 + _blankColumnCount;

                    for (int i = 0; i < blankRows; i++)
                    {
                        for (int j = 0; j < totalColumns; j++)
                        {
                            table.Cell().Element(DataCell).Text("");
                        }
                    }
                });

                col.Item().PaddingTop(10).AlignCenter().Element(summary => ComposeSummary(summary, pageStudents));
            });
        }

        private void ComposeSummary(IContainer container, List<GetSearchedViewStudentModel> pageStudents)
        {
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

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(120);
                });

                table.Cell().ColumnSpan(2).Element(SummaryHeaderCell).Text("GENERAL");
                table.Cell().ColumnSpan(2).Element(SummaryHeaderCell).Text("EWS");
                table.Cell().RowSpan(2).Element(SummaryHeaderCell).Text("TOTAL");

                table.Cell().Element(SummaryHeaderCell).Text("Boys");
                table.Cell().Element(SummaryHeaderCell).Text("Girls");
                table.Cell().Element(SummaryHeaderCell).Text("Boys");
                table.Cell().Element(SummaryHeaderCell).Text("Girls");

                table.Cell().Element(SummaryValueCell).Text(generalBoys.ToString());
                table.Cell().Element(SummaryValueCell).Text(generalGirls.ToString());
                table.Cell().Element(SummaryValueCell).Text(ewsBoys.ToString());
                table.Cell().Element(SummaryValueCell).Text(ewsGirls.ToString());
                table.Cell().Element(SummaryValueCell).Text(totalStudents.ToString());
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(0.8f).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).Bold());
        }

        private static IContainer DataCell(IContainer container)
        {
            return container.Border(0.6f).Height(16).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer NameCell(IContainer container)
        {
            return container.Border(0.6f).Height(16).PaddingHorizontal(4).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer SummaryHeaderCell(IContainer container)
        {
            return container.Border(0.8f).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).Bold());
        }

        private static IContainer SummaryValueCell(IContainer container)
        {
            return container.Border(0.8f).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).Bold());
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(8).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic().FontSize(7);
                    row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic().FontSize(7);
                });

                col.Item().PaddingTop(3).PaddingBottom(3).Height(1).Background(Colors.Grey.Darken1);

                col.Item().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }
    }

    #endregion

    #region---------------- Print Student With Board ROll No ------------------

    public class StudentBoardRollNoPdfDocument : IDocument
    {
        private readonly List<AdmSearchedStudentResponse> _students;
        private readonly bool _isBoardRollNo;
        public StudentBoardRollNoPdfDocument(List<AdmSearchedStudentResponse> students,bool isBoardRollNo)
        {
            _students = students ?? new List<AdmSearchedStudentResponse>();
            _isBoardRollNo = isBoardRollNo;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public DocumentSettings GetSettings()
        {
            return DocumentSettings.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Black));
                page.Header().Element(ComposeHeader);
                page.Content().PaddingTop(5).Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            byte[]? logoBytes = null;

            string logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "Logos",
                "logo (1).png");

            if (File.Exists(logoPath))
            {
                logoBytes = File.ReadAllBytes(logoPath);
            }

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(80).Height(65).AlignLeft().AlignMiddle().Element(logo =>
                    {
                        if (logoBytes != null)
                        {
                            logo.Image(logoBytes, ImageScaling.FitArea);
                        }
                    });

                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter().Text("Cambridge School, Noida").FontSize(16).ExtraBold();

                        c.Item().AlignCenter().Text("Sector-27, Noida, Uttar Pradesh 201301").FontSize(8).ExtraBold();

                        c.Item().PaddingTop(2).AlignCenter().Text("noida.cambridgeschool.edu.in").FontSize(8).Bold();

                        c.Item().PaddingTop(6).AlignCenter().Text(_isBoardRollNo ? "Student Board Roll No" : "Student CBSE Registration No").FontSize(11).Bold();
                    });

                    row.ConstantItem(80);
                });

                col.Item().PaddingTop(4).AlignRight().Text(text =>
                {
                    text.Span("Date : ").FontSize(7).Bold();

                    text.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")).FontSize(7);
                });

                col.Item().PaddingBottom(3);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(70);
                    columns.RelativeColumn(2.2f);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Sl No");
                    header.Cell().Element(HeaderCell).Text("Student No");
                    header.Cell().Element(HeaderCell).Text("Student Name");
                    header.Cell().Element(HeaderCell).Text("Class");
                    header.Cell().Element(HeaderCell).Text("Section");
                    header.Cell().Element(HeaderCell).Text("Date of Birth");
                    header.Cell().Element(HeaderCell).Text(_isBoardRollNo ? "Board Roll No" : "CBSE Reg No");
                });

                int srNo = 1;

                foreach (var item in _students)
                {
                    table.Cell().Element(CenterCell).Text(srNo.ToString());
                    table.Cell().Element(CenterCell).Text(item.ControlNo ?? "");
                    table.Cell().Element(NameCell).Text(item.StudentName ?? "");
                    table.Cell().Element(CenterCell).Text(item.ClassName ?? "");
                    table.Cell().Element(CenterCell).Text(item.SectionName ?? "");
                    table.Cell().Element(CenterCell).Text(item.DateOfBirth.HasValue ? item.DateOfBirth.Value.ToString("dd-MM-yyyy") : "");
                    table.Cell().Element(CenterCell).Text(_isBoardRollNo ? item.BoardRollNo ?? "" : item.CBSERegNo ?? "");

                    srNo++;
                }
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(5).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7).ExtraBold());
        }

        private static IContainer CenterCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private static IContainer NameCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(4).AlignLeft().AlignMiddle().DefaultTextStyle(x => x.FontSize(7));
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(8).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("Generated on:").ExtraBold().Italic().FontSize(7);

                    row.RelativeItem().AlignRight().Text($"{DateTime.Now:dd-MM-yyyy}").ExtraBold().Italic().FontSize(7);
                });

                col.Item().PaddingTop(3).PaddingBottom(3).Height(1).Background(Colors.Grey.Darken1);

                col.Item().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber().Bold();
                    text.Span(" of ").Bold();
                    text.TotalPages().Bold();
                });
            });
        }
    }

    #endregion

    #region---------------- Print Admit Child Receipt --------------
    // school copy and parent copy side by side on one A4 page
    public class AdmitChildReceiptPdfDocument : IDocument
    {
        private readonly AdmitChildReceiptPrintModel _receipt;

        public AdmitChildReceiptPdfDocument(AdmitChildReceiptPrintModel receipt)
        {
            _receipt = receipt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));

                page.Content().Row(row =>
                {
                    row.RelativeItem().Element(x => ComposeCopy(x, "SCHOOL COPY"));
                    row.ConstantItem(12);
                    row.RelativeItem().Element(x => ComposeCopy(x, "PARENT COPY"));
                });

                // only View Student Receipt asks for it; Fee Collection / Admit Child prints stay unchanged
                if (_receipt.ShowGeneratedFooter)
                    page.Footer().Element(StudentReceiptListPdfDocument.ComposeFooter);
            });
        }

        // also used by MultiReceiptPdfDocument (copyName null -> title "Receipt";
        // joined -> fee table sits right under the student details, one continuous box)
        internal void ComposeCopy(IContainer container, string? copyName, bool joined = false)
        {
            container.Column(col =>
            {
                col.Item().AlignCenter().Text(string.IsNullOrWhiteSpace(copyName) ? "Receipt" : $"Receipt ({copyName})").FontSize(8);
                col.Item().Border(1).Padding(5).Column(c =>
                {
                    c.Item().Element(ComposeHeader);
                    c.Item().PaddingVertical(3).Height(1).Background(Colors.Black);
                    c.Item().Element(ComposeStudentDetails);
                });
                col.Item().PaddingTop(joined ? 0 : 10).Element(x => ComposeFeeTable(x, joined));
                col.Item().Border(1).BorderTop(0).Padding(4).Element(ComposeSummary);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png");

            container.Row(row =>
            {
                if (File.Exists(logo))
                    row.ConstantItem(35).Height(30).AlignMiddle().Image(logo).FitArea();

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("V3M International School").FontSize(12).Bold();
                    c.Item().AlignCenter().Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017").FontSize(7);
                });
            });
        }

        private void ComposeStudentDetails(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(62);
                    c.RelativeColumn();
                    c.ConstantColumn(48);
                    c.RelativeColumn();
                });

                void Pair(string label, string? value)
                {
                    table.Cell().PaddingVertical(2).Text(label).Bold();
                    table.Cell().PaddingVertical(2).Text(value ?? "");
                }

                Pair("Receipt No. :", _receipt.ReceiptNo);
                Pair("Receipt Date :", _receipt.ReceiptDate.ToString("dd-MM-yyyy"));
                Pair("Registration No. :", _receipt.RegistrationNo);
                Pair("Student No. :", _receipt.StudentNo);
                Pair("Student Name :", _receipt.StudentName);
                Pair("Father Name :", _receipt.FatherName);
                Pair("Class :", _receipt.ClassName);
                Pair("Fee Session :", _receipt.SessionName);
                Pair("Period :", _receipt.PeriodName);
                Pair("", "");
            });
        }

        private void ComposeFeeTable(IContainer container, bool joined)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });

                // joined: the details box already draws the top line, so no double border
                table.Header(header =>
                {
                    header.Cell().Border(1).BorderTop(joined ? 0 : 1).PaddingVertical(5).AlignCenter().Text("Fee Head").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                    header.Cell().Border(1).BorderTop(joined ? 0 : 1).PaddingVertical(5).AlignCenter().Text(string.IsNullOrWhiteSpace(_receipt.AmountHeader) ? "Total" : _receipt.AmountHeader).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                });

                // joined (multi receipt print): tighter rows so more fee heads fit in half a page
                var rowPadding = joined ? 2 : 4;
                foreach (var fee in _receipt.FeeHeads)
                {
                    table.Cell().Border(1).PaddingHorizontal(4).PaddingVertical(rowPadding).Text(fee.FeeHeadName ?? "").FontSize(9);
                    table.Cell().Border(1).PaddingHorizontal(4).PaddingVertical(rowPadding).AlignRight().Text(fee.Amount.ToString("N2"));
                }
            });
        }

        private void ComposeSummary(IContainer container)
        {
            var payable = _receipt.TotalAmount - _receipt.Concession;

            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(45);
                        c.RelativeColumn();
                        c.ConstantColumn(55);
                        c.RelativeColumn();
                    });

                    void Pair(string label, string value)
                    {
                        table.Cell().PaddingVertical(2).Text(label).Bold();
                        table.Cell().PaddingVertical(2).Text(value);
                    }

                    if (_receipt.Concession > 0)
                    {
                        Pair("Total :", _receipt.TotalAmount.ToString("N2"));
                        Pair("Concession :", _receipt.Concession.ToString("N2"));
                    }
                    Pair("Payable :", payable.ToString("N2"));
                    Pair("Paid :", _receipt.PaidAmount.ToString("N2"));
                    Pair("Balance :", _receipt.Balance.ToString("N2"));
                    Pair("Pay Mode :", _receipt.PaymentMode ?? "");
                });

                col.Item().PaddingTop(8).Row(row =>
                {
                    row.ConstantItem(45).Text("In Words").Bold();
                    row.RelativeItem().Text(AmountInWords(_receipt.PaidAmount));
                });

                col.Item().PaddingTop(4).AlignRight().Text("Accountant").Bold();
            });
        }

        // Indian numbering: 71700 -> "Seventy One Thousand Seven Hundred Only"
        internal static string AmountInWords(decimal amount)
        {
            var rupees = (long)Math.Floor(amount);
            var paise = (int)Math.Round((amount - rupees) * 100);

            var words = rupees == 0 ? "Zero" : NumberToWords(rupees);
            if (paise > 0)
                words += $" and {NumberToWords(paise)} Paise";
            return words + " Only";
        }

        private static string NumberToWords(long number)
        {
            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string BelowHundred(long n) =>
                n < 20 ? ones[n] : (tens[n / 10] + (n % 10 > 0 ? " " + ones[n % 10] : ""));

            var parts = new List<string>();
            void Add(long value, string unit)
            {
                if (value > 0)
                    parts.Add(BelowHundred(value) + unit);
            }

            if (number >= 10000000)
            {
                parts.Add(NumberToWords(number / 10000000) + " Crore");
                number %= 10000000;
            }
            Add(number / 100000, " Lakh");
            number %= 100000;
            Add(number / 1000, " Thousand");
            number %= 1000;
            Add(number / 100, " Hundred");
            number %= 100;
            Add(number, "");

            return string.Join(" ", parts);
        }
    }
    #endregion

    #region---------------- Print Multi Receipt --------------
    // View Student Receipt -> Print Multi Receipt: 2 receipts per A4 page (one above the other,
    // dashed cut line between them), same receipt layout as Print
    public class MultiReceiptPdfDocument : IDocument
    {
        private const int ReceiptsPerPage = 2;
        // A4 = 842pt high; 15pt margins + footer leave room for two receipts of 375pt with the cut line between
        private const float CellHeight = 375;
        private const float CutLineSpace = 14;

        private readonly List<AdmitChildReceiptPrintModel> _receipts;

        public MultiReceiptPdfDocument(List<AdmitChildReceiptPrintModel> receipts)
        {
            _receipts = receipts;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));

                page.Content().Column(col =>
                {
                    for (var i = 0; i < _receipts.Count; i += ReceiptsPerPage)
                    {
                        if (i > 0)
                            col.Item().PageBreak();

                        var pageReceipts = _receipts.Skip(i).Take(ReceiptsPerPage).ToList();
                        col.Item().Element(x => ComposeCell(x, pageReceipts[0]));
                        // cut line below the first receipt
                        col.Item().Height(CutLineSpace).AlignMiddle()
                            .LineHorizontal(1).LineColor(Colors.Grey.Darken1).LineDashPattern(new[] { 4f, 3f });
                        if (pageReceipts.Count > 1)
                            col.Item().Element(x => ComposeCell(x, pageReceipts[1]));
                    }
                });

                page.Footer().Element(StudentReceiptListPdfDocument.ComposeFooter);
            });
        }

        // a receipt with many fee heads is scaled down to fit its half of the page
        private static void ComposeCell(IContainer container, AdmitChildReceiptPrintModel receipt)
        {
            container.Height(CellHeight).ScaleToFit()
                .Element(x => new AdmitChildReceiptPdfDocument(receipt).ComposeCopy(x, null, joined: true));
        }
    }
    #endregion

    #region---------------- Print Student Receipt List --------------
    // View Student Receipt -> Print -> PDF: landscape list with totals at the end
    public class StudentReceiptListPdfDocument : IDocument
    {
        private readonly StudentReceiptListPrintRequest _request;

        public StudentReceiptListPdfDocument(StudentReceiptListPrintRequest request)
        {
            _request = request;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(8);
                page.MarginVertical(12);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8f));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                if (!_request.IsStudentPrint)
                    page.Footer().Element(ComposeFooter);
            });
        }

        // same footer as the other generated list PDFs; also used by the receipt prints of View Student Receipt
        internal static void ComposeFooter(IContainer container)
        {
            container.PaddingTop(6).Column(col =>
            {
                col.Item().Height(1).Background(Colors.Grey.Darken1);
                col.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem()
                        .AlignLeft()
                        .Text($"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}")
                        .Italic()
                        .SemiBold();

                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ").Bold();
                            text.CurrentPageNumber().Bold();
                            text.Span(" of ").Bold();
                            text.TotalPages().Bold();
                        });
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png");

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (File.Exists(logo))
                        row.ConstantItem(70).Height(40).AlignMiddle().Image(logo).FitArea();
                    else
                        row.ConstantItem(70);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("V3M International School").FontSize(16).Bold();
                        c.Item().AlignCenter().Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017").FontSize(9).SemiBold();
                        c.Item().AlignCenter().Text($"Student Receipts between {_request.FromDate:dd'/'MM'/'yyyy} and {_request.ToDate:dd'/'MM'/'yyyy}").FontSize(10).SemiBold();
                    });

                    row.ConstantItem(70);
                });

                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8));
                        text.Span("Pages : ");
                        text.CurrentPageNumber();
                        text.Span("/");
                        text.TotalPages();
                    });
                    row.RelativeItem().AlignRight().Text($"Date :{DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}").FontSize(8);
                });
                col.Item().PaddingTop(2);
            });
        }

        private void ComposeContent(IContainer container)
        {
            var receipts = _request.Receipts ?? new List<StudentFeeReceiptListResponse>();

            var totalAmount = receipts.Sum(x => x.TotalAmount ?? 0);
            var totalPaid = receipts.Sum(x => x.PaidAmount ?? 0);
            var totalConcession = receipts.Sum(x => x.Concession ?? 0);
            var studentPrint = _request.IsStudentPrint;

            var titles = new List<string> { "S.No.", "Receipt No.", "Student No.", "Student Name", "Class Section", "Total", "Paid",
                "Concession", "Receipt Date", "Mode", "Chq. No", "ChequeBank", "ChequeDate", "PeriodName", "Remarks" };
            if (!studentPrint)
                titles.Add("Receipt Type");

            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    // all relative -> the table always takes the full page width
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.2f);   // S.No.
                        c.RelativeColumn(3.6f);   // Receipt No.
                        c.RelativeColumn(3.6f);   // Student No.
                        c.RelativeColumn(7.5f);   // Student Name
                        c.RelativeColumn(3.8f);   // Class Section
                        c.RelativeColumn(4.2f);   // Total
                        c.RelativeColumn(4.2f);   // Paid
                        c.RelativeColumn(3.8f);   // Concession
                        c.RelativeColumn(4.2f);   // Receipt Date
                        c.RelativeColumn(3.4f);   // Mode
                        c.RelativeColumn(3.6f);   // Chq. No
                        c.RelativeColumn(4.6f);   // Cheque Bank
                        c.RelativeColumn(4.2f);   // Cheque Date
                        c.RelativeColumn(5.5f);   // Period Name
                        c.RelativeColumn(5.5f);   // Remarks
                        if (!studentPrint)
                            c.RelativeColumn(3.4f);   // Receipt Type
                    });

                    table.Header(header =>
                    {
                        foreach (var title in titles)
                        {
                            header.Cell().Element(HeaderCell).Text(title);
                        }
                    });

                    var sNo = 1;
                    foreach (var r in receipts)
                    {
                        table.Cell().Element(Cell).AlignCenter().Text((sNo++).ToString());
                        table.Cell().Element(Cell).AlignCenter().Text(r.ReceiptNo ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.StudentNo ?? "");
                        table.Cell().Element(Cell).Text(r.StudentName ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(string.IsNullOrWhiteSpace(r.ClassSection) ? (r.ClassName ?? "") : r.ClassSection);
                        table.Cell().Element(Cell).AlignCenter().Text((r.TotalAmount ?? 0).ToString("0.00"));
                        table.Cell().Element(Cell).AlignCenter().Text((r.PaidAmount ?? 0).ToString("0.00"));
                        table.Cell().Element(Cell).AlignCenter().Text((r.Concession ?? 0).ToString("0.00"));
                        table.Cell().Element(Cell).AlignCenter().Text(r.ReceiptDate?.ToString("dd'/'MM'/'yyyy") ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.PaymentMode ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.ChequeNo ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.ChequeBank ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.ChequeDate?.ToString("dd'/'MM'/'yyyy") ?? "");
                        table.Cell().Element(Cell).Text(r.PeriodName ?? "");
                        table.Cell().Element(Cell).AlignCenter().Text(r.Remark ?? "");
                        if (!studentPrint)
                            table.Cell().Element(Cell).AlignCenter().Text(r.Valid == true ? "Active" : "Cancel");
                    }

                    if (!studentPrint)
                    {
                        // last row of the table: totals under Total / Paid / Concession
                        table.Cell().ColumnSpan(5).Element(TotalCell).AlignRight().Text($"Total ({receipts.Count} Receipts)");
                        table.Cell().Element(TotalCell).AlignCenter().Text(totalAmount.ToString("N2"));
                        table.Cell().Element(TotalCell).AlignCenter().Text(totalPaid.ToString("N2"));
                        table.Cell().Element(TotalCell).AlignCenter().Text(totalConcession.ToString("N2"));
                        table.Cell().ColumnSpan(8).Element(TotalCell).Text("");
                    }

                    IContainer HeaderCell(IContainer c) =>
                        c.Border(1).Background(Colors.Grey.Lighten3).Padding(2).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.FontSize(7.5f).Bold()).MinHeight(20);

                    IContainer Cell(IContainer c) =>
                        c.Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignMiddle().DefaultTextStyle(x => x.FontSize(7.5f));

                    IContainer TotalCell(IContainer c) =>
                        c.Border(1).Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(3).AlignMiddle().DefaultTextStyle(x => x.FontSize(8.5f).Bold());
                });

                // Student Print: totals as label : value lines below the table, under the amount columns
                if (studentPrint)
                {
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem(68);
                        row.RelativeItem(32).Column(t =>
                        {
                            foreach (var (label, value) in new[] { ("Total Amount", totalAmount), ("Paid Amount", totalPaid), ("Concession", totalConcession) })
                            {
                                t.Item().Row(r =>
                                {
                                    r.ConstantItem(80).Text(label).FontSize(10).Bold();
                                    r.ConstantItem(20).Text(":").FontSize(10).Bold();
                                    r.RelativeItem().Text(value.ToString("0.##")).FontSize(10).Bold();
                                });
                            }
                        });
                    });
                }
            });
        }
    }
    #endregion

    #region---------------- Cheque Report (Chq Combine / Print) --------------
    // View Student Receipt -> Chq Report: shared header and totals box of the two cheque report PDFs
    internal static class StudentChequeReportLayout
    {
        internal static void ComposeHeader(IContainer container, DateTime fromDate, DateTime toDate)
        {
            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png");

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (File.Exists(logo))
                        row.ConstantItem(70).Height(40).AlignMiddle().Image(logo).FitArea();
                    else
                        row.ConstantItem(70);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("V3M International School").FontSize(16).Bold();
                        c.Item().AlignCenter().Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017").FontSize(9).SemiBold();
                        c.Item().AlignCenter().Text($"Student Cheque Receipts between {fromDate:dd'/'MM'/'yyyy} and {toDate:dd'/'MM'/'yyyy}").FontSize(11).SemiBold();
                    });

                    row.ConstantItem(70);
                });

                col.Item().PaddingTop(4).AlignRight().Text($"Date :{DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}").FontSize(8);
                col.Item().PaddingTop(2);
            });
        }

        // label : value lines in a bordered box, right side under the amount columns; last line is highlighted
        internal static void ComposeTotals(IContainer container, IList<(string Label, string Value)> lines)
        {
            container.PaddingTop(8).AlignRight().Width(230).Border(1).Column(col =>
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    var isLast = i == lines.Count - 1;
                    var item = col.Item();
                    if (i > 0)
                        item = item.BorderTop(1);
                    if (isLast)
                        item = item.Background(Colors.Grey.Lighten3);

                    item.PaddingVertical(3).PaddingHorizontal(6).Row(r =>
                    {
                        r.RelativeItem().Text(lines[i].Label).FontSize(9).Bold();
                        r.ConstantItem(10).Text(":").FontSize(9).Bold();
                        r.ConstantItem(90).AlignRight().Text(lines[i].Value).FontSize(9).Bold();
                    });
                }
            });
        }

        internal static IContainer HeaderCell(IContainer c) =>
            c.Border(1).Background(Colors.Grey.Lighten3).Padding(2).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.FontSize(7.5f).Bold()).MinHeight(22);

        internal static IContainer Cell(IContainer c) =>
            c.Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignMiddle().DefaultTextStyle(x => x.FontSize(7.5f));
    }

    // Chq Combine -> PDF (Fin_GetOnlyChqCombineReport): portrait, one row per cheque
    public class ChqCombineReportPdfDocument : IDocument
    {
        private readonly StudentChequeReportRequest _request;
        private readonly List<ChqCombineReportResponse> _data;

        public ChqCombineReportPdfDocument(StudentChequeReportRequest request, List<ChqCombineReportResponse> data)
        {
            _request = request;
            _data = data ?? new List<ChqCombineReportResponse>();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(20);
                page.MarginVertical(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8f));

                page.Header().Element(c => StudentChequeReportLayout.ComposeHeader(c, _request.FromDate, _request.ToDate));
                page.Content().Element(ComposeContent);
                page.Footer().Element(StudentReceiptListPdfDocument.ComposeFooter);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2f);     // S.No.
                        c.RelativeColumn(3.6f);   // Receipt No.
                        c.RelativeColumn(4.6f);   // Student No.
                        c.RelativeColumn(5.6f);   // Chq. No.
                        c.RelativeColumn(3.8f);   // Chq. Date
                        c.RelativeColumn(4f);     // Cheque Bank
                        c.RelativeColumn(2.8f);   // Paid
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "S.No.", "Receipt No.", "Student No.", "Chq. No.", "Chq. Date", "Cheque Bank", "Paid" })
                            header.Cell().Element(StudentChequeReportLayout.HeaderCell).Text(title);
                    });

                    foreach (var r in _data)
                    {
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.SNo.ToString());
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ReceiptNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.StudentNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChqNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChqDate ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChequeBank ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignRight().Text((r.Paid ?? 0).ToString("0.00"));
                    }
                });

                col.Item().Element(c => StudentChequeReportLayout.ComposeTotals(c, new List<(string, string)>
                {
                    ("Total Cheques", _data.Count.ToString()),
                    ("Total Amount", _data.Sum(x => x.Paid ?? 0).ToString("N2"))
                }));
            });
        }
    }

    // Chq Report -> Print -> PDF (V3M_FIN_PrintViewStudentReceipts): landscape receipt list
    public class PrintViewReceiptsPdfDocument : IDocument
    {
        private readonly StudentChequeReportRequest _request;
        private readonly List<PrintViewStudentReceiptResponse> _data;

        public PrintViewReceiptsPdfDocument(StudentChequeReportRequest request, List<PrintViewStudentReceiptResponse> data)
        {
            _request = request;
            _data = data ?? new List<PrintViewStudentReceiptResponse>();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(8);
                page.MarginVertical(12);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8f));

                page.Header().Element(c => StudentChequeReportLayout.ComposeHeader(c, _request.FromDate, _request.ToDate));
                page.Content().Element(ComposeContent);
                page.Footer().Element(StudentReceiptListPdfDocument.ComposeFooter);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.2f);   // S.No.
                        c.RelativeColumn(3.6f);   // Receipt No.
                        c.RelativeColumn(3.6f);   // Student No.
                        c.RelativeColumn(6.5f);   // Student Name
                        c.RelativeColumn(3.8f);   // Class Section
                        c.RelativeColumn(4.2f);   // Total
                        c.RelativeColumn(4.2f);   // Paid
                        c.RelativeColumn(3.8f);   // Concession
                        c.RelativeColumn(4.2f);   // Concession Name
                        c.RelativeColumn(4.2f);   // Receipt Date
                        c.RelativeColumn(3.4f);   // Mode
                        c.RelativeColumn(3.6f);   // Chq. No
                        c.RelativeColumn(4.6f);   // Cheque Bank
                        c.RelativeColumn(4.2f);   // Cheque Date
                        c.RelativeColumn(5f);     // Period Name
                        c.RelativeColumn(4.6f);   // Remarks
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "S.No.", "Receipt No.", "Student No.", "Student Name", "Class Section", "Total", "Paid",
                                     "Concession", "ConcessionName", "Receipt Date", "Mode", "Chq. No", "ChequeBank", "ChequeDate", "PeriodName", "Remarks" })
                            header.Cell().Element(StudentChequeReportLayout.HeaderCell).Text(title);
                    });

                    foreach (var r in _data)
                    {
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.SNo.ToString());
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ReceiptNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.StudentNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).Text(r.StudentName ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ClassSection ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text((r.Total ?? 0).ToString("0.00"));
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text((r.Paid ?? 0).ToString("0.00"));
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text((r.Concession ?? 0).ToString("0.00"));
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ConcessionName ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ReceiptDate ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.Mode ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChqNo ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChequeBank ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.ChequeDate ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).Text(r.PeriodName ?? "");
                        table.Cell().Element(StudentChequeReportLayout.Cell).AlignCenter().Text(r.Remarks ?? "");
                    }
                });

                col.Item().Element(c => StudentChequeReportLayout.ComposeTotals(c, new List<(string, string)>
                {
                    ("Total Receipts", _data.Count.ToString()),
                    ("Total Amount", _data.Sum(x => x.Total ?? 0).ToString("N2")),
                    ("Concession", _data.Sum(x => x.Concession ?? 0).ToString("N2")),
                    ("Paid Amount", _data.Sum(x => x.Paid ?? 0).ToString("N2"))
                }));
            });
        }
    }
    #endregion

    #region---------------- Print Fee Receipt (Parent Format) --------------
    // single fee receipt on A4: header, student details, S.No. / Description / Due / Paid, total in words, note
    public class ParentFormatReceiptPdfDocument : IDocument
    {
        private readonly ParentFormatReceiptPrintModel _receipt;

        public ParentFormatReceiptPdfDocument(ParentFormatReceiptPrintModel receipt)
        {
            _receipt = receipt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Content().Column(col =>
                {
                    col.Item().Element(ComposeHeader);
                    col.Item().PaddingTop(15).Border(1).Column(c =>
                    {
                        c.Item().BorderBottom(1).PaddingVertical(5).AlignCenter().Text($"FEE RECEIPT ({ShortSession(_receipt.SessionName)})");
                        c.Item().BorderBottom(1).Padding(6).Element(ComposeStudentDetails);
                        c.Item().Element(ComposeFeeTable);
                        c.Item().Padding(6).Text(text =>
                        {
                            text.Span("NOTE : 1. Fees, Charges once paid are not refundable     2. Cheque payment subject to realization").Bold().FontSize(9);
                        });
                    });
                });

                page.Footer().DefaultTextStyle(x => x.FontSize(8)).Element(StudentReceiptListPdfDocument.ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png");

            container.Row(row =>
            {
                if (File.Exists(logo))
                    row.ConstantItem(70).Height(70).AlignMiddle().Image(logo).FitArea();

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("V3M International School").FontSize(24).Bold();
                    c.Item().AlignCenter().Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017").FontSize(9);
                });
            });
        }

        private void ComposeStudentDetails(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.ConstantColumn(12);
                    c.RelativeColumn(3);
                    c.RelativeColumn(3);
                    c.ConstantColumn(12);
                    c.RelativeColumn(3);
                });

                void Pair(string label, string? value)
                {
                    table.Cell().PaddingVertical(4).Text(label);
                    table.Cell().PaddingVertical(4).Text(string.IsNullOrEmpty(label) ? "" : ":");
                    table.Cell().PaddingVertical(4).Text(value ?? "");
                }

                Pair("Receipt No.", _receipt.ReceiptNo);
                Pair("Date", _receipt.ReceiptDate.ToString("dd'/'MM'/'yyyy"));
                Pair("Admission No.", _receipt.AdmissionNo);
                Pair("Class & Section", _receipt.ClassSection);
                Pair("Name of the Student", _receipt.StudentName);
                Pair("Father Name", _receipt.FatherName);
                Pair("Instalment", _receipt.Instalment);
                Pair("Mobile No.", _receipt.MobileNo);
                Pair("Mode of Payment", _receipt.PaymentMode);
                Pair("", "");
            });
        }

        private void ComposeFeeTable(IContainer container)
        {
            var totalDue = _receipt.FeeHeads.Sum(x => x.DueAmount);
            var totalPaid = _receipt.FeeHeads.Sum(x => x.PaidAmount);

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(45);
                    c.RelativeColumn(4);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text("S.No.");
                    header.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text("Description");
                    header.Cell().BorderBottom(1).BorderRight(1).Padding(4).AlignRight().Text("Actual Amount Due");
                    header.Cell().BorderBottom(1).Padding(4).AlignRight().Text("Paid Amount");
                });

                var sNo = 1;
                foreach (var fee in _receipt.FeeHeads)
                {
                    table.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text($"{sNo++}.");
                    table.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text(fee.Description ?? "");
                    table.Cell().BorderBottom(1).BorderRight(1).Padding(4).AlignRight().Text(fee.DueAmount.ToString("N2"));
                    table.Cell().BorderBottom(1).Padding(4).AlignRight().Text(fee.PaidAmount.ToString("N2"));
                }

                table.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text("");
                table.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text("Total");
                table.Cell().BorderBottom(1).BorderRight(1).Padding(4).AlignRight().Text(totalDue.ToString("N2"));
                table.Cell().BorderBottom(1).Padding(4).AlignRight().Text(totalPaid.ToString("N2"));

                table.Cell().BorderBottom(1).BorderRight(1).Padding(4).Text("");
                table.Cell().ColumnSpan(3).BorderBottom(1).Padding(4).Text($"Total Amount in words :   {AdmitChildReceiptPdfDocument.AmountInWords(totalPaid)}");
            });
        }

        // "2026-2027" -> "2026-27"; anything else is printed as it is
        private static string ShortSession(string? sessionName)
        {
            if (string.IsNullOrWhiteSpace(sessionName))
                return string.Empty;

            var parts = sessionName.Trim().Split('-');
            return parts.Length == 2 && parts[0].Length == 4 && parts[1].Length == 4
                ? $"{parts[0]}-{parts[1].Substring(2)}"
                : sessionName.Trim();
        }
    }
    #endregion

    #region---------------------- Head Details Report------------------
    public class StudentReceiptHeadWisePdfDocument : IDocument
    {
        private readonly StudentReceiptHeadWiseRequest _request;
        private readonly List<StudentReceiptHeadWiseResponse> _receipts;

        private static readonly string[] FixedColumns =
        {
            "Sl. No",
            "Receipt No",
            "Student No",
            "Student Name",
            "Class Name",
            "Category",
            "Reciept Date"
        };

        private static readonly HashSet<string> NonAmountColumns =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Sl. No",
                "Receipt No",
                "Student No",
                "Student Name",
                "Class Name",
                "Category",
                "Reciept Date",
                "Payment Mode",
                "Cheque No.",
                "Cheque Date",
                "Period",
                "Remarks"
            };

        public StudentReceiptHeadWisePdfDocument(
            StudentReceiptHeadWiseRequest request,
            List<StudentReceiptHeadWiseResponse> receipts)
        {
            _request = request;
            _receipts = receipts ?? new();
        }

        public DocumentMetadata GetMetadata() =>
            DocumentMetadata.Default;

        public DocumentSettings GetSettings() =>
            DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(8);
                page.MarginVertical(8);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            var logo = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "Logos",
                "logo (1).png");

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (File.Exists(logo))
                    {
                        row.ConstantItem(95)
                            .Height(48)
                            .AlignMiddle()
                            .Image(logo)
                            .FitArea();
                    }
                    else
                    {
                        row.ConstantItem(95);
                    }

                    row.RelativeItem().Column(center =>
                    {
                        center.Item().AlignCenter()
                            .Text("V3M International School")
                            .FontSize(14)
                            .Bold();

                        center.Item().AlignCenter()
                            .Text("FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017")
                            .FontSize(8);

                        center.Item().AlignCenter()
                            .Text(
                                $"Receipt Head-Wise Detail | " +
                                $"{_request.FromDate:dd/MM/yyyy} - " +
                                $"{_request.ToDate:dd/MM/yyyy}")
                            .FontSize(9)
                            .SemiBold();
                    });

                    row.ConstantItem(120)
                        .AlignRight()
                        .AlignMiddle()
                        .Text($"Printed on : {DateTime.Now:dd MMM yyyy hh:mm:ss tt}")
                        .FontSize(6);
                });

                col.Item().PaddingTop(5);
            });
        }

        private void ComposeContent(IContainer container)
        {
            var columns = GetColumns();

            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(definition =>
                    {
                        foreach (var column in columns)
                        {
                            definition.RelativeColumn(GetColumnWidth(column));
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var column in columns)
                        {
                            header.Cell()
                                .Element(HeaderCell)
                                .Text(column);
                        }
                    });

                    foreach (var receipt in _receipts)
                    {
                        foreach (var column in columns)
                        {
                            receipt.Values.TryGetValue(
                                column, out var value);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(FormatValue(column, value));
                        }
                    }

                    if (_receipts.Count > 0)
                    {
                        for (var i = 0; i < columns.Count; i++)
                        {
                            var column = columns[i];

                            if (i == 0)
                            {
                                table.Cell()
                                    .Element(TotalCell)
                                    .Text($"Grand Total ({_receipts.Count})");
                            }
                            else if (IsAmountColumn(column))
                            {
                                table.Cell()
                                    .Element(TotalCell)
                                    .AlignRight()
                                    .Text(GetColumnTotal(column));
                            }
                            else
                            {
                                table.Cell()
                                    .Element(TotalCell)
                                    .Text("");
                            }
                        }
                    }
                    else
                    {
                        table.Cell()
                            .ColumnSpan((uint)columns.Count)
                            .Element(BodyCell)
                            .AlignCenter()
                            .Text("No records found");
                    }
                });

                col.Item()
                    .PaddingTop(8)
                    .AlignRight()
                    .Element(ComposeSummary);
            });
        }

        private List<string> GetColumns()
        {
            if (_receipts.Count == 0)
            {
                return FixedColumns
                    .Concat(new[]
                    {
                        "Payment Mode",
                        "Cheque No.",
                        "Cheque Date",
                        "Cheque Collection",
                        "Cash Collection",
                        "Total Amount",
                        "Balance",
                        "Period",
                        "Remarks"
                    })
                    .ToList();
            }

            var columns = _receipts[0].Values.Keys.ToList();

            var ordered = FixedColumns
                .Where(fixedColumn => columns.Any(
                    x => x.Equals(
                        fixedColumn,
                        StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ordered.AddRange(
                columns.Where(column =>
                    !ordered.Contains(
                        column,
                        StringComparer.OrdinalIgnoreCase) &&
                    !IsEndingColumn(column)));

            var endingColumns = new[]
            {
                "Previous Balance",
                "Late Fee",
                "Payment Mode",
                "Cheque No.",
                "Cheque Date",
                "Cheque Collection",
                "Cash Collection",
                "Total Amount",
                "Balance",
                "Period",
                "Remarks"
            };

            foreach (var endingColumn in endingColumns)
            {
                if (columns.Any(x => x.Equals(
                    endingColumn,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    ordered.Add(endingColumn);
                }
            }

            return ordered;
        }

        private static bool IsEndingColumn(string column)
        {
            return new[]
            {
                "Previous Balance",
                "Late Fee",
                "Payment Mode",
                "Cheque No.",
                "Cheque Date",
                "Cheque Collection",
                "Cash Collection",
                "Total Amount",
                "Balance",
                "Period",
                "Remarks"
            }.Contains(column, StringComparer.OrdinalIgnoreCase);
        }

        private static float GetColumnWidth(string column)
        {
            if (column.Equals("Student Name",
                StringComparison.OrdinalIgnoreCase))
                return 2.5f;

            if (column.Equals("Receipt No",
                StringComparison.OrdinalIgnoreCase) ||
                column.Equals("Student No",
                StringComparison.OrdinalIgnoreCase))
                return 1.25f;

            if (column.Equals("Period",
                StringComparison.OrdinalIgnoreCase) ||
                column.Equals("Remarks",
                StringComparison.OrdinalIgnoreCase))
                return 1.8f;

            if (column.Equals("Payment Mode",
                StringComparison.OrdinalIgnoreCase) ||
                column.Equals("Cheque No.",
                StringComparison.OrdinalIgnoreCase))
                return 1.5f;

            if (NonAmountColumns.Contains(column))
                return 1.4f;

            return 1.15f;
        }

        private static bool IsAmountColumn(string column)
        {
            return !NonAmountColumns.Contains(column);
        }

        private string GetColumnTotal(string column)
        {
            decimal total = 0;

            foreach (var receipt in _receipts)
            {
                if (receipt.Values.TryGetValue(column, out var value))
                    total += ToDecimal(value);
            }

            return total.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static decimal ToDecimal(object? value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToDecimal(
                    value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return decimal.TryParse(
                    value.ToString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var result)
                    ? result
                    : 0;
            }
        }

        private static string FormatValue(
            string column, object? value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            if (IsAmountColumn(column))
            {
                return ToDecimal(value).ToString(
                    "N0", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(
                value, CultureInfo.InvariantCulture) ?? "";
        }

        private void ComposeSummary(IContainer container)
        {
            decimal cash = GetColumnTotalValue("Cash Collection");
            decimal cheque = GetColumnTotalValue("Cheque Collection");
            decimal grandTotal = GetColumnTotalValue("Total Amount");

            container.Column(col =>
            {
                col.Item().AlignRight().Row(row =>
                {
                    row.ConstantItem(110).AlignRight()
                        .Text("Total By Cash :").Bold();

                    row.ConstantItem(90).AlignRight()
                        .Text(cash.ToString("N0")).Bold();
                });

                col.Item().AlignRight().Row(row =>
                {
                    row.ConstantItem(110).AlignRight()
                        .Text("Total By Cheque :").Bold();

                    row.ConstantItem(90).AlignRight()
                        .Text(cheque.ToString("N0")).Bold();
                });

                col.Item().AlignRight().Row(row =>
                {
                    row.ConstantItem(110).AlignRight()
                        .Text("Grand Total :").Bold();

                    row.ConstantItem(90).AlignRight()
                        .Text(grandTotal.ToString("N0")).Bold();
                });
            });
        }

        private decimal GetColumnTotalValue(string column)
        {
            decimal total = 0;

            foreach (var receipt in _receipts)
            {
                if (receipt.Values.TryGetValue(column, out var value))
                    total += ToDecimal(value);
            }

            return total;
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Border(0.7f)
                .Background(Colors.Grey.Lighten3)
                .Padding(2)
                .AlignMiddle()
                .AlignCenter()
                .DefaultTextStyle(x =>
                    x.FontSize(5.5f).Bold());
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .Border(0.5f)
                .PaddingVertical(2)
                .PaddingHorizontal(1.5f)
                .AlignMiddle()
                .DefaultTextStyle(x => x.FontSize(5.5f));
        }

        private static IContainer TotalCell(IContainer container)
        {
            return container
                .Border(0.7f)
                .Background(Colors.Grey.Lighten3)
                .Padding(2)
                .AlignMiddle()
                .DefaultTextStyle(x =>
                    x.FontSize(6).Bold());
        }

        private static void ComposeFooter(IContainer container)
        {
            container.PaddingTop(4).Row(row =>
            {
                row.RelativeItem()
                    .Text($"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}")
                    .FontSize(6)
                    .Italic();

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").FontSize(6);
                        text.CurrentPageNumber().FontSize(6);
                        text.Span(" of ").FontSize(6);
                        text.TotalPages().FontSize(6);
                    });
            });
        }
    }
    #endregion

}
