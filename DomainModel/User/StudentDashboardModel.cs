using System;

namespace DomainModel.User
{
    // POST api/StudentDashboard body: the user id saved in session at login.
    public class StudentDashboardRequest
    {
        public int UserId { get; set; }
    }

    // Logged-in student's own dashboard data (api/StudentDashboard).
    // Display fields only: never Aadhaar, bank details, income, password or tokens.
    public class StudentDashboardProfile
    {
        public long StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? FirstName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? ImagePath { get; set; }
        public string? AdmissionNo { get; set; }
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public int? RollNo { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? ApaarId { get; set; }
        public string? PenNo { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentMobile { get; set; }
        public bool IsActive { get; set; }
        public bool IsTransportRequired { get; set; }

        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? SessionName { get; set; }
        public string? HouseName { get; set; }
        public string? HouseColor { get; set; }
        public string? ReligionName { get; set; }
        public string? CategoryName { get; set; }
        public string? RouteName { get; set; }
        public string? BranchName { get; set; }
        public string? GroupName { get; set; }

        public string? FatherName { get; set; }
        public string? FatherContactNo { get; set; }
        public string? FatherEmail { get; set; }
        public string? FatherImagePath { get; set; }
        public string? MotherName { get; set; }
        public string? MotherContactNo { get; set; }
        public string? MotherEmail { get; set; }
        public string? MotherImagePath { get; set; }
        public string? EmergencyPersonName { get; set; }
        public string? EmergencyPersonRelationShip { get; set; }
        public string? EmergencyPersonContactNo { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public string? DistrictName { get; set; }
        public string? StateName { get; set; }
    }
}
