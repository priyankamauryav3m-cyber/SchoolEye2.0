using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
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

                            Row("Registration No.", student.StudentNo);
                            Row("Admission No. Allotted", student.AdmissionNo);
                            Row("Date of Admission", student.AdmissionDate);
                            Row("Name of the Student", student.StudentName);
                            Row("Gender", student.Gender);
                            Row("Father's Name", student.FatherName);
                            Row("Mother's Name", student.MotherName);
                            Row("Date of Birth", student.DateOfBirth);
                            Row("Address", "GGN, Gkdjkh - U2331");
                            Row("SMS Mobile", student.SMSMobileNo);
                            Row("Father's Mobile", student.SMSMobileNo);
                            Row("Mother's Mobile", student.SMSMobileNo);
                            Row("Contact Email", "abhishek@gmail.com");
                            Row("Class Admitted", student.ClassName);
                            Row("Date of Fee Deposit", "");
                            Row("Mode of Payment", "Cash");
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
                table.Cell().Border(1).Padding(2).Text("Arrear").SemiBold();
                table.Cell().ColumnSpan(2).Border(1);
                table.Cell().Border(1).Padding(2).AlignRight().Text(master?.ArrearAmount > 0 ? master.ArrearAmount.ToString("N2") : "").SemiBold();
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
}
