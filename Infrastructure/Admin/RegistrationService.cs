using ApplicationInterface.SuperAdmin;
using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SuperAdmin
{
    public class RegistrationService : IRegistrationRepository
    {
        private readonly string _connectionString;
        public RegistrationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<RegistrationDto>> SearchAsync(RegistrationSearchDto search)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var param = new DynamicParameters();

                param.Add("@GroupCode", search.GroupCode);
                param.Add("@BranchCode", search.BranchCode);
                param.Add("@SessionId", search.SessionId);
                param.Add("@ClassCode", search.ClassCode);
                param.Add("@Gender", search.Gender);
                param.Add("@DateFrom", search.DateFrom);
                param.Add("@DateTo", search.DateTo);
                param.Add("@RegistrationNO", search.RegistrationNo);
                param.Add("@EWS", search.EWS);
                param.Add("@EWSSrc", search.EWSSrc);
                param.Add("@Sibling", search.Sibling);
                param.Add("@IsTransport", search.IsTransport);
                param.Add("@DocumentSelected", search.DocumentSelected);
                param.Add("@TransportDistance", search.TransportDistance);
                param.Add("@RegistrationFrom", search.RegistrationFrom);
                param.Add("@RegistrationTo", search.RegistrationTo);
                param.Add("@PointsFrom", search.PointsFrom);
                param.Add("@PointsTo", search.PointsTo);
                param.Add("@StatusSrc", search.StatusSrc);
                param.Add("@StudentName", search.StudentName);
                param.Add("@FatherName", search.FatherName);
                param.Add("@PaymentMode", search.PaymentMode);
                param.Add("@StudentCategory", search.StudentCategory);
                param.Add("@MotherName", search.MotherName);
                var result = await connection.QueryAsync<RegistrationDto>(
                    "ADM_UspGetRegistration",
                    param,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch
            {
                return Enumerable.Empty<RegistrationDto>();
            }
        }
        public async Task<string> AddStudentRegistration(RegistrationModal res)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("ADM_UspRegistration", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // ===== Required Parameters =====
                        cmd.Parameters.AddWithValue("@GroupCode", res.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", res.BranchCode);
                        cmd.Parameters.AddWithValue("@SessionId", res.SessionId);
                        cmd.Parameters.AddWithValue("@FirstName", res.FirstName);
                        cmd.Parameters.AddWithValue("@MiddleName", res.MiddleName ?? "");
                        cmd.Parameters.AddWithValue("@LastName", res.LastName ?? "");
                        cmd.Parameters.AddWithValue("@Gender", res.Gender);
                        cmd.Parameters.AddWithValue("@DateOfBirth", res.DateOfBirth);
                        cmd.Parameters.AddWithValue("@ClassCode", res.ClassCode);
                        cmd.Parameters.AddWithValue("@StreamCode", res.StreamCode ?? "");
                        cmd.Parameters.AddWithValue("@MotherTitle", res.MotherTitle ?? "");
                        cmd.Parameters.AddWithValue("@MotherName", res.MotherName ?? "");
                        cmd.Parameters.AddWithValue("@MotherContactNo", res.MotherContactNo ?? "");
                        cmd.Parameters.AddWithValue("@MotherEmailId", res.MotherEmailId ?? "");
                        cmd.Parameters.AddWithValue("@FatherTitle", res.FatherTitle ?? "");
                        cmd.Parameters.AddWithValue("@FatherName", res.FatherName ?? "");
                        cmd.Parameters.AddWithValue("@FatherContactNo", res.FatherContactNo ?? "");
                        cmd.Parameters.AddWithValue("@FatherEmailId", res.FatherEmailId ?? "");
                        cmd.Parameters.AddWithValue("@SMSMobileNo", res.SMSMobileNo ?? "");
                        cmd.Parameters.AddWithValue("@AddressToWhome", res.AddressToWhome ?? "");
                        cmd.Parameters.AddWithValue("@AddressLine1", res.AddressLine1 ?? "");
                        cmd.Parameters.AddWithValue("@AddressLine2", res.AddressLine2 ?? "");
                        cmd.Parameters.AddWithValue("@Pincode", res.Pincode ?? "");
                        cmd.Parameters.AddWithValue("@ContactNo", res.ContactNo ?? "");
                        cmd.Parameters.AddWithValue("@EWS", res.EWS);
                        cmd.Parameters.AddWithValue("@Sibling", res.Sibling);
                        cmd.Parameters.AddWithValue("@RegistrationFee", res.RegistrationFee);
                        cmd.Parameters.AddWithValue("@CreatedBy", res.CreatedBy);
                        cmd.Parameters.AddWithValue("@PaymentMode", res.PaymentMode ?? "");
                        cmd.Parameters.AddWithValue("@Remarks", res.Remarks ?? "");
                        cmd.Parameters.AddWithValue("@CBSERollNo", res.CBSERollNo ?? "");
                        cmd.Parameters.AddWithValue("@StudentCategory", res.StudentCategory);
                        cmd.Parameters.AddWithValue("@EnquiryId", res.EnquiryId);
                        // ===== OUTPUT PARAMETER =====
                        var registrationNoParam = new SqlParameter("@RegistrationNo", SqlDbType.VarChar, 20)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(registrationNoParam);
                        var result = await cmd.ExecuteNonQueryAsync();
                        return registrationNoParam.Value?.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ParentsDetails?> GetRegistrationParentsChildById(string groupCode, string branchCode, long sessionId, long registrationId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@SessionId", sessionId);
                param.Add("@RegistrationId", registrationId);
                return await db.QueryFirstOrDefaultAsync<ParentsDetails>(
                    "ADM_GetChildParentDetails",
                    param,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                return null;
            }
        }
        public async Task<ChildDetails?> GetRegistrationChildData(string groupCode, string branchCode, long sessionId, long registrationId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode);
                param.Add("@BranchCode", branchCode);
                param.Add("@SessionId", sessionId);
                param.Add("@RegistrationId", registrationId);
                return await db.QueryFirstOrDefaultAsync<ChildDetails>(
                    "ADM_GetChildDetails",
                    param,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> RegistrationAdditionalInformation(PointsCriteria dto)
        {
            try

            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                // ===== BASIC =====

                parameters.Add("@GroupCode", dto.GroupCode);
                parameters.Add("@BranchCode", dto.BranchCode);
                parameters.Add("@SessionId", dto.SessionId);
                parameters.Add("@RegistrationNo", dto.RegistrationId);

                // ===== OTHER INFO =====

                parameters.Add("@DistanceFromSchool", dto.DistanceFromSchool);
                parameters.Add("@CoreCategory", dto.CoreCategory);
                parameters.Add("@IsReservedSeat", dto.IsReservedSeat);
                parameters.Add("@FatherTransferableJob", dto.FatherTransferableJob);
                parameters.Add("@MoterTransferableJob", dto.MotherTransferableJob);
                parameters.Add("@GirlChildOnly", dto.IsGirlChildOnly);
                parameters.Add("@DefencePerson", dto.DefencePerson);
                parameters.Add("@DefenceDetail", dto.DefenceDetail);
                parameters.Add("@Sibling", dto.Sibling);
                parameters.Add("@SingleParent", dto.SingleParent);
                parameters.Add("@SingleFatherName", dto.SingleFatherName);
                parameters.Add("@SingleMotherName", dto.SingleMotherName);
                parameters.Add("@SingleParentComment", dto.SingleParentComment);
                parameters.Add("@IsLegalDocumentHave", dto.IsLegalDocumentHave);
                // ===== ALUMNI =====

                parameters.Add("@isFatherAlumni", dto.IsFatherAlumni);
                parameters.Add("@FatherPassingYear", dto.FatherPassingYear);
                parameters.Add("@isMotherAlumni", dto.IsMotherAlumni);
                parameters.Add("@MotherPassingYear", dto.MotherPassingYear);
                // ===== CHILD INFO =====
                parameters.Add("@IsFirstBornChild", dto.IsFirstBornChild);
                parameters.Add("@IsStaffWard", dto.IsStaffWard);
                parameters.Add("@IsChildBelowAge", dto.IsChildBelowAge);
                parameters.Add("@IsTwinChild", dto.IsTwinChild);
                parameters.Add("@IsAdoptedChild", dto.IsAdoptedChild);

                // ===== EXTRA =====
                parameters.Add("@fatherAlumniBranch", dto.FatherAlumniBranch);
                parameters.Add("@motherAlumniBranch", dto.MotherAlumniBranch);
                parameters.Add("@childCustody", dto.ChildCustody);
                parameters.Add("@hearingFrom", dto.HearingFrom);
                parameters.Add("@FatherAlumniClass", dto.FatherAlumniClass);
                parameters.Add("@MotherAlumniClass", dto.MotherAlumniClass);
                parameters.Add("@CreatedBy", dto.CreatedBy);
                int rows = await connection.ExecuteAsync(
                    "ADM_UpdateOtherInformation",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return rows > 0 ? "Saved Successfully" : "No Record Updated";
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    "Database error while updating other information.", ex);
            }
        }
        public async Task<string> RegistrationAddressInformation(AddressDetails add)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", add.GroupCode);
                parameters.Add("@BranchCode", add.BranchCode);
                parameters.Add("@SessionId", add.SessionId);
                parameters.Add("@RegistrationId", add.RegistrationId);
                parameters.Add("@line1", add.Line1);
                parameters.Add("@line2", add.Line2);
                parameters.Add("@Pincode", add.PinCode);
                parameters.Add("@ContactNo", add.ContactNo);
                parameters.Add("@AddressTo", add.AddressTo);
                parameters.Add("@AddressType", add.AddressType);
                parameters.Add("@CreatedBy", add.CreatedBy);
                int rows = await connection.ExecuteAsync(
                    "ADM_AddUpdateRegNoAddressInfo",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return rows > 0 ? "Inserted Successfully" : "No Record Updated";

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error while updating Address details.", ex);
            }
        }
        public async Task<AddressDetails?> GetRegistrationChildAddressInformation(string groupCode, string branchCode, long sessionId, long registrationId, string AddressType)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@SessionId", sessionId);
                param.Add("@RegistrationId", registrationId);
                param.Add("@AddressType", AddressType);
                return await db.QueryFirstOrDefaultAsync<AddressDetails>(
                    "ADM_GetChildAddressDetails",
                    param,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<PointsCriteria?> GetRegistrationChildotherInformation(string groupCode, string branchCode, long sessionId, long registrationId)
        {
            using var db = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@SessionId", sessionId);
            param.Add("@RegistrationId", registrationId);
            return await db.QueryFirstOrDefaultAsync<PointsCriteria>(
                "ADM_GetChildOtherInformation",
                param,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<string> SubmitFamilyInfoDetails(FamilyDetails res)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", res.GroupCode);
                parameters.Add("@BranchCode", res.BranchCode);
                parameters.Add("@SessionId", res.SessionId);
                parameters.Add("@RegistrationId", res.RegistrationId);
                parameters.Add("@ChildName", res.FamilyChildName);
                parameters.Add("@ChildClass", res.FamilyChildclass);
                parameters.Add("@ChildSchool", res.FamilyChildSchool);
                parameters.Add("@CreatedBy", res.CreatedBy);
                await db.ExecuteAsync(
                    "ADM_SubmitFamilyInfoDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return "Family information saved successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<List<FamilyDetails?>> GetRegistrationFamilyDetails(string groupCode, string branchCode, long sessionId, long registrationId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode);
                param.Add("@BranchCode", branchCode);
                param.Add("@SessionId", sessionId);
                param.Add("@RegistrationId", registrationId);
                var result = await db.QueryAsync<FamilyDetails>(
                    "ADM_GetFamilyChildInfoDetails",
                    param,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching registration family details", ex);
            }
        }
        public async Task<string> RegistrationParentDetails(ParentsDetails res)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                // ===== BASIC =====
                parameters.Add("@GroupCode", res.GroupCode);
                parameters.Add("@BranchCode", res.BranchCode);
                parameters.Add("@SessionId", res.SessionId);
                parameters.Add("@RegistrationId", res.RegistrationId);

                // ===== FATHER =====
                parameters.Add("@FatherTitle", res.FatherTitle);
                parameters.Add("@FatherName", res.FatherName);
                parameters.Add("@FatherMiddleName", res.FatherMiddleName);
                parameters.Add("@FatherLName", res.FatherLName);
                parameters.Add("@FatherQualification", res.FatherQualification);
                parameters.Add("@FatherOccupation", res.FatherOccupation);
                parameters.Add("@FatherOtherOccupation", res.FatherOtherOccupation);
                parameters.Add("@FatherDesignation", res.FatherDesignation);
                parameters.Add("@FatherEMail", res.FatherEMail);
                parameters.Add("@FatherAnnualIncome", res.FatherAnnualIncome);
                parameters.Add("@FatherContactNo", res.FatherContactNo);
                parameters.Add("@FatherAchievement", res.FatherAchievement);
                parameters.Add("@fatherNationality", res.FatherNationality);
                parameters.Add("@fatherDOB", res.FatherDOB);
                parameters.Add("@fatherofficeAddress", res.FatherOfficeAddress);
                parameters.Add("@fatherOfficeContactNo", res.FatherOfficeContactNo);
                parameters.Add("@fatherOrgnisation", res.FatherOrganisation);
                parameters.Add("@fatherCollege", res.FatherCollege);
                parameters.Add("@fatherMotherTongue", res.FatherMotherTongue);
                parameters.Add("@fatherSchool", res.FatherSchool);
                parameters.Add("@fatherAadharNo", res.FatherAadharNo);
                parameters.Add("@FatherPlaceOfBirth", res.FatherPlaceOfBirth);

                // ===== MOTHER =====
                parameters.Add("@MotherTitle", res.MotherTitle);
                parameters.Add("@MotherName", res.MotherName);
                parameters.Add("@MotherMiddleName", res.MotherMiddleName);
                parameters.Add("@MotherLName", res.MotherLName);
                parameters.Add("@MotherQualification", res.MotherQualification);
                parameters.Add("@MotherOccupation", res.MotherOccupation);
                parameters.Add("@MotherOtherOccupation", res.MotherOtherOccupation);
                parameters.Add("@MotherDesignation", res.MotherDesignation);
                parameters.Add("@MotherEMail", res.MotherEMail);
                parameters.Add("@MotherAnnualIncome", res.MotherAnnualIncome);
                parameters.Add("@MotherContactNo", res.MotherContactNo);
                parameters.Add("@MotherAchievement", res.MotherAchievement);
                parameters.Add("@motherNationality", res.MotherNationality);
                parameters.Add("@motherDOB", res.MotherDOB);
                parameters.Add("@motherofficeAddress", res.MotherOfficeAddress);
                parameters.Add("@motherOfficeContactNo", res.MotherOfficeContactNo);
                parameters.Add("@motherOrgnisation", res.MotherOrganisation);
                parameters.Add("@motherCollege", res.MotherCollege);
                parameters.Add("@motherMotherTongue", res.MotherMotherTongue);
                parameters.Add("@motherMaidenSurName", res.MotherMaidenSurname);
                parameters.Add("@motherSchool", res.MotherSchool);
                parameters.Add("@motherAadharNo", res.MotherAadharNo);
                parameters.Add("@MotherPlaceOfBirth", res.MotherPlaceOfBirth);
                // ===== COMMON =====
                parameters.Add("@SMSMobileNo", res.SMSMobileNo);
                parameters.Add("@contactEmailId", res.ContactEmailId);
                parameters.Add("@CreatedBy", res.CreatedBy);

                int rows = await connection.ExecuteAsync(
                    "ADM_ParentsDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return rows > 0 ? "Updated Successfully" : "No Record Updated";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error while updating parent details.", ex);
            }
        }
        public async Task<string> RegistrationChildDetails(ChildDetails dto)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", dto.GroupCode);
                parameters.Add("@BranchCode", dto.BranchCode);
               parameters.Add("@SessionId", dto.SessionId);
                parameters.Add("@RegistrationId", dto.RegistrationId);
                parameters.Add("@ChildFirstName", dto.ChildFirstName);
                parameters.Add("@ChildMiddleName", dto.ChildMiddleName);
                parameters.Add("@ChildLastName", dto.ChildLastName);
                parameters.Add("@Gender", dto.Gender);
                parameters.Add("@DateOfBirth", dto.DateOfBirth);
                parameters.Add("@ClassCode", dto.ClassCode);
                parameters.Add("@StreamCode", dto.StreamCode);
                parameters.Add("@BloodGroup", dto.BloodGroup);
                parameters.Add("@MedicalInformation", dto.MedicalInformation);
                parameters.Add("@Nationality", dto.Nationality);
                parameters.Add("@SocialCategory", dto.SocialCategory);
                parameters.Add("@PreviousClass", dto.PreviousClass);
                parameters.Add("@PreviousSchool", dto.PreviousSchool);
                parameters.Add("@MediumOfInstruction", dto.MediumOfInstruction);
                parameters.Add("@PreviousSchoolAddress", dto.PreviousSchoolAddress);
                parameters.Add("@Religion", dto.Religion);
                parameters.Add("@Background", dto.Background);
                parameters.Add("@MotherTongue", dto.MotherTongue);
                parameters.Add("@IsTransportRequired", dto.IsTransportRequired);
                parameters.Add("@TransportDetails", dto.TransportDetails);
                parameters.Add("@BirthPalace", dto.BirthPalace);
                parameters.Add("@OtherHealthProblem", dto.OtherHealthProblem);
                parameters.Add("@IsHostelRequired", dto.IsHostelRequired);
                parameters.Add("@IsNRI", dto.IsNRI);
                parameters.Add("@EmergencyPersonName", dto.EmergencyPersonName);
                parameters.Add("@EmergencyPersonRelationShip", dto.EmergencyPersonRelationShip);
                parameters.Add("@EmergencyPersonContactNo", dto.EmergencyPersonContactNo);
                parameters.Add("@PassportNo", dto.PassportNo);
                parameters.Add("@PassportType", dto.PassportType);
                parameters.Add("@IsPassportRegReq", dto.IsPassportRegReq);
                parameters.Add("@PassportIssueDate", dto.PassportIssueDate);
                parameters.Add("@PassportExpiryDate", dto.PassportExpiryDate);
                parameters.Add("@VisaNo", dto.VisaNo);
                parameters.Add("@VisaType", dto.VisaType);
                parameters.Add("@IsVisaRegReq", dto.IsVisaRegReq);
                parameters.Add("@VisaIssueDate", dto.VisaIssueDate);
                parameters.Add("@VisaExpiryDate", dto.VisaExpiryDate);
                parameters.Add("@CreatedBy", dto.CreatedBy);
                parameters.Add("@IsDisability", dto.IsDisability);
                parameters.Add("@CBSERollNo", dto.CBSERollNo);
                parameters.Add("@AadhaarNo", dto.AadhaarNo);
                parameters.Add("@IsDayCare", dto.IsDayCare);
                int rows = await connection.ExecuteAsync(
                    "ADM_UpdateChildDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return rows > 0
                    ? "Updated Successfully"
                    : "No Record Updated";
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    "Database error while updating child details.", ex);
            }
        }
        public async Task<IEnumerable<StudentAdmissionModel>> GetStudentDetailsData(string groupCode, string branchCode, string? studentNo, string? sessionName)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode);
                param.Add("@BranchCode", branchCode);
                param.Add("@StudentNo", string.IsNullOrWhiteSpace(studentNo) ? null : studentNo);
                param.Add("@SessionName", string.IsNullOrWhiteSpace(sessionName) ? null : sessionName);
                var data = await con.QueryAsync<StudentAdmissionModel>(
                    "ADM_GetStudentDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching student details data", ex);
            }
        }
        public async Task<GetRegistrationModel> GetStudentDetailsData(string groupCode, string branchCode, long SessionId, string RegistrationId, string AddressType)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode);
                param.Add("@BranchCode", branchCode);
                param.Add("@SessionId", SessionId);
                param.Add("@RegistrationId", RegistrationId);
                param.Add("@AddressType", AddressType);
                var data = await con.QueryFirstOrDefaultAsync<GetRegistrationModel>(
                "Sp_RegiparentAddressDetails",
                param,
                commandType: CommandType.StoredProcedure);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching student details: " + ex.Message);
            }
        }
        public async Task<string> StudentDirectAdmissionData(StudentDirectAdmissionModel model)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            try
            {
                #region 1️⃣ Admission
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@FirstName", model.FirstName);
                param.Add("@MiddleName", model.MiddleName);
                param.Add("@LastName", model.LastName);
                param.Add("@Gender", model.Gender);
                param.Add("@ClassCode", model.ClassCode);
                param.Add("@DateOfBirth", model.DateOfBirth?.ToString("yyyy-MM-dd"));
                param.Add("@Religion", model.Religion);
                param.Add("@SocialCategoryId", model.SocialCategoryId);
                param.Add("@NationalityId", model.NationalityId);
                param.Add("@IsUsingTpt", model.IsUsingTpt);
                param.Add("@RouteDistance", model.RouteDistance);
                param.Add("@FatherTitle", model.FatherTitle);
                param.Add("@FatherFirstName", model.FatherFirstName);
                param.Add("@FatherMiddleName", model.FatherMiddleName);
                param.Add("@FatherLastName", model.FatherLastName);
                param.Add("@FatherContactNo", model.FatherContactNo);
                param.Add("@FatherEmailId", model.FatherEmailId);
                param.Add("@FatherOccupation", model.FatherOccupation);
                param.Add("@FatherOccupationOther", model.FatherOccupationOther);
                param.Add("@MotherTitle", model.MotherTitle);
                param.Add("@MotherFirstName", model.MotherFirstName);
                param.Add("@MotherMiddleName", model.MotherMiddleName);
                param.Add("@MotherLastName", model.MotherLastName);
                param.Add("@MotherContactNo", model.MotherContactNo);
                param.Add("@MotherEmailId", model.MotherEmailId);
                param.Add("@MotherOccupation", model.MotherOccupation);
                param.Add("@MotherOccupationOther", model.MotherOccupationOther);
                param.Add("@PreviousBranchCode", model.PreviousBranchCode);
                param.Add("@AdmissionDate", model.AdmissionDate);
                param.Add("@SMSContactNo", model.SMSContactNo);
                param.Add("@AddressLine1", model.AddressLine1);
                param.Add("@AddressLine2", model.AddressLine2);
                param.Add("@PinCode", model.PinCode);
                param.Add("@AddContactNo", model.AddContactNo);
                param.Add("@AddressTo", model.AddressTo);
                param.Add("@CreatedBy", model.CreatedBy);
                param.Add("@PreviousBranchCode", model.PreviousBranchCode);
                param.Add("@SocietyId", model.SocietyId);
                param.Add("@SiblingID", model.SiblingID);
                param.Add("@StudentAadharNo", model.StudentAadharNo);
                param.Add("@FatherAadharNo", model.FatherAadharNo);
                param.Add("@MotherAadharNo", model.MotherAadharNo);
                param.Add("@RegId", model.RegId);
                param.Add("@FeeTemplateId", model.FeeTemplateId);
                param.Add("@stuManualAdmNo", model.stuManualAdmNo);
                param.Add("@ApaarId", model.ApaarId);
                param.Add("@PenNo", model.PenNo);
                param.Add("@Caste", model.Caste);
                param.Add("@MapConAndChallan", model.MapConAndChallan);
                param.Add("@ConcessionId", model.ConcessionId);
                param.Add("@ConcessionFromDate", model.ConcessionFromDate);
                param.Add("@ConcessionToDate", model.ConcessionToDate);
                param.Add("@ConcessionDetails", model.ConcessionDetails);
                param.Add("@ConcessionRemarks", model.ConcessionRemarks);
                param.Add("@SectionId", model.SectionId);
                param.Add("@StudentNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                // param.Add("@StudentId", model.StudentId != null ? (object)Convert.ToInt64(model.StudentId) : DBNull.Value);
                param.Add("@StudentControlStudentNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                param.Add("@ControlNo", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);
                param.Add("@LoginId", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                DataTable dtConcession = new DataTable();
                dtConcession.Columns.Add("FeeHeadId", typeof(int));
                dtConcession.Columns.Add("ConcessionType", typeof(int));
                dtConcession.Columns.Add("ConcessionValue", typeof(decimal));

                foreach (var item in model.FeeHeadList ?? new())
                {
                    if (item.ConcessionValue > 0)
                    {
                        dtConcession.Rows.Add(
                            Convert.ToInt32(item.FeeHeadId),
                          item.ConcessionType == 1 ? 1 : 0,
                            item.ConcessionValue
                        );
                    }
                }
                param.Add("@ConcessionDetails", dtConcession.AsTableValuedParameter("dbo.tblConcessionDetails"));
                await con.ExecuteAsync("ADM_UspStudentDirectAdmission", param, commandType: CommandType.StoredProcedure);
                string studentNo = param.Get<string>("@StudentNo");
                string ControlNo = param.Get<string>("@ControlNo");
                string StudentControlStudentNo = param.Get<string>("@StudentControlStudentNo");
                string loginId = param.Get<string>("@LoginId");
            
                if (string.IsNullOrEmpty(studentNo) || string.IsNullOrEmpty(loginId))
                    return studentNo;
                #endregion
                #region 2️⃣ Get Student Details

                var studentData = await con.QueryFirstOrDefaultAsync<dynamic>(
                   @"SELECT *,convert(varchar(50),DateOfBirth,103) DOB,(select top 1 isnull(SessionName,'') 
                from MstBranchSession where CurrentSession=1 and IsValid=1) CurrentSession
                FROM vwStudentDetails WHERE StudentNo=@StudentNo AND GroupCode=@GroupCode AND BranchCode=@BranchCode AND SessionId=@SessionId",
                  new
                  {
                      StudentNo = studentNo,
                      GroupCode = model.GroupCode,     
                      BranchCode = model.BranchCode,   
                      SessionId = model.SessionId  
                  }
                );
                if (studentData == null)
                    return studentNo;
                string studentName = studentData?.StudentName?.ToString();
                if (string.IsNullOrEmpty(studentName))
                    return studentNo;
                if (studentData?.DateOfBirth == null)
                    return studentNo;
                DateTime dob = Convert.ToDateTime(studentData.DateOfBirth);
                #endregion
                #region 3️⃣ Generate Password
                var pwdParam = new DynamicParameters();
                pwdParam.Add("@GroupCode", model.GroupCode);
                pwdParam.Add("@BranchCode", model.BranchCode);
                pwdParam.Add("@SessionId", model.SessionId);
                pwdParam.Add("@StudentName", studentName);
                pwdParam.Add("@StudentDOBYear", dob.Year);
                pwdParam.Add("@StudentPassword", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                await con.ExecuteAsync("UspGenerateStudentPassword", pwdParam, commandType: CommandType.StoredProcedure);
                string plainPassword = pwdParam.Get<string>("@StudentPassword");
                if (string.IsNullOrEmpty(plainPassword))
                    return studentNo;
                #endregion
                #region 4️⃣ Encrypt + Update Password
                string encryptedPassword = EncryptPassword(plainPassword.Replace("V3M", "+"), true);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@LoginId", loginId);
                parameters.Add("@Password", encryptedPassword);
                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync("UspUpdateStudentPassword", parameters, commandType: CommandType.StoredProcedure);

                #endregion
                #region 5️⃣ Branch Setting Check

                string keyValue = await con.ExecuteScalarAsync<string>(@"SELECT KeyValue FROM DefaultSettings WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode 
        AND KeyName='AdmitStudentSMSRequire'", new { model.GroupCode, model.BranchCode });
                #endregion
                return studentNo;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message} | Inner Error: {ex.InnerException?.Message}";
            }
        }
        private string EncryptPassword(string password, bool useHashing)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            if (useHashing)
            {
                using var md5 = MD5.Create();
                bytes = md5.ComputeHash(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
        public async Task<IEnumerable<StudentListResponse>> AdmintStudentListData(StudentListRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                param.Add("@Gender", request.Gender);
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsSearchOnAdmDate", request.IsSearchOnAdmDate);
                param.Add("@AdmFromDate", request.AdmFromDate);
                param.Add("@AdmToDate", request.AdmToDate);
                param.Add("@ValidStatus", request.ValidStatus);
                param.Add("@StudentStatus", request.StudentStatus);
                param.Add("@OrderBy", request.OrderBy, DbType.String);
                return await con.QueryAsync<StudentListResponse>(
                    "Sp_GetStudentList",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<SiblingDetailResponse?> GetSiblingDetail(string groupCode, string branchCode, int SessionId, string siblingID = "")
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode, DbType.String);
                param.Add("@BranchCode", branchCode, DbType.String);
                param.Add("@SiblingID", siblingID, DbType.String);
                param.Add("@SessionId", SessionId);
                return await con.QueryFirstOrDefaultAsync<SiblingDetailResponse>(
                    "Sp_GetSiblingDetail",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<CommonDomain>> GetFeeHeadConcession(string groupCode, string branchCode, int SessionId, string concessionId, int isMappedOnly)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode);
                param.Add("@BranchCode", branchCode);
                param.Add("@SessionId", SessionId);
                param.Add("@ConcessionId", concessionId);
                param.Add("@IsMappedOnly", isMappedOnly);
                return await con.QueryAsync<CommonDomain>(
                    "Sp_GetFeeHeadConcession",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<string> InsertOnlineRegistration(OnlineRegistration online)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", online.GroupCode);
                param.Add("@BranchCode", online.BranchCode);
                param.Add("@SessionName", online.SessionName);
                param.Add("@ClassCode", online.ClassCode);
                param.Add("@ChildFirstName", online.ChildFirstName?.ToUpper());
                param.Add("@ChildMiddleName", online.ChildMiddleName ?? "");
                param.Add("@ChildLastName", online.ChildLastName?.ToUpper());
                param.Add("@DateOfBirth", online.DateOfBirth);
                param.Add("@Gender", online.Gender);
                param.Add("@MotherTongue", online.MotherTongue);
                param.Add("@Nationality", online.Nationality);
                param.Add("@PreviousSchool", online.PreviousSchool);
                param.Add("@StreamCode", online.StreamCode);
                param.Add("@LastClass", online.LastClass);
                param.Add("@LastClassYear", online.LastClassYear);
                param.Add("@LastClassMonth", online.LastClassMonth);
                param.Add("@IsSpecialNeed", online.SpecialNeeds);
                param.Add("@SpecialneedsDetail", online.SpecialneedsDetail);
                param.Add("@StudentAddress", online.StudentAddress);
                param.Add("@FatherFName", online.FatherFName);
                param.Add("@FatherMName", online.FatherMName);
                param.Add("@FatherLName", online.FatherLName);
                param.Add("@FatherOccupation", online.FatherOccupation);
                param.Add("@FatherQualification", online.FatherQualification);
                param.Add("@FatherMobile", online.FatherMobile);
                param.Add("@FatherEmailId", online.FatherEmailId);
                param.Add("@MotherFName", online.MotherFName);
                param.Add("@MotherMName", online.MotherMName);
                param.Add("@MotherLName", online.MotherLName);
                param.Add("@MotherOccupation", online.MotherOccupation);
                param.Add("@MotherQualification", online.MotherQualification);
                param.Add("@MotherMobile", online.MotherMobile);
                param.Add("@MotherEmailId", online.MotherEmailId);
                param.Add("@IsCBSE", online.IsCBSE);
                param.Add("@IsCBSEDetails", online.IsCBSEDetails);
                param.Add("@CompulsorySubject", online.CompulsorySubject);
                param.Add("@Subject4", online.Subject4);
                param.Add("@Subject5", online.Subject5);
                param.Add("@MathsMarks", online.MathsMarks);
                param.Add("@OptionalMarks", online.OptionalMarks);
                param.Add("@SocialSciencemarks", online.SocialSciencemarks);
                param.Add("@ScienceMarks", online.ScienceMarks);
                param.Add("@EnglishMarks", online.EnglishMarks);
                param.Add("@MathsT2Marks", online.MathsT2Marks);
                param.Add("@OptionalT2Marks", online.OptionalT2Marks);
                param.Add("@SocialScienceT2marks", online.SocialScienceT2marks);
                param.Add("@ScienceT2Marks", online.ScienceT2Marks);
                param.Add("@EnglishT2Marks", online.EnglishT2Marks);
                param.Add("@docIds", online.DocIds ?? "0");
                param.Add("@CreatedBy", online.CreatedBy);
                param.Add("@PreviousSchoolAdderss", online.PreviousSchoolAddress);
                param.Add("@StudentResidentialAdd", online.StudentResidentialAdd);
                param.Add("@StuResidentialAddPinCode", online.StuResidentialAddPinCode);
                param.Add("@StudentPermanentAdd", online.StudentPermanentAdd);
                param.Add("@StuPermanentAddPinCode", online.StuPermanentAddPinCode);
                param.Add("@FatherOccupationOther", online.FatherOccupationOther);
                param.Add("@FatherAnnualIncome", online.FatherAnnualIncome);
                param.Add("@MotherOccupationOther", online.MotherOccupationOther);
                param.Add("@MotherAnnualIncome", online.MotherAnnualIncome);
                param.Add("@MotherOtherQualification", online.MotherOtherQualification);
                param.Add("@FatherOtherQualification", online.FatherOtherQualification);
                param.Add("@ChildAadharNo", online.ChildAadharNo);
                param.Add("@FatherAadharNo", online.FatherAadharNo);
                param.Add("@MotherAadharNo", online.MotherAadharNo);
                param.Add("@Subject3", online.Subject3);
                param.Add("@Subject6", online.Subject6);
                // Output Parameter
                param.Add("@RegistrationNo",
                    dbType: DbType.String,
                    size: 20,
                    direction: ParameterDirection.Output);
                var result = await con.ExecuteAsync(
                    "ADM_UspOnlineRegistration_Cambridge_Indirapuram",
                    param,
                    commandType: CommandType.StoredProcedure
                );
                return param.Get<string>("@RegistrationNo");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> RegistrationCancel(CancelRegistration online)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", online.GroupCode);
                param.Add("@BranchCode", online.BranchCode);
                param.Add("@SessionName", online.SessionName);
                param.Add("@RegistrationNo", online.RegistrationNo);
                param.Add("@CreatedBy", online.CreatedBy);
                param.Add("@AppStatus", online.AppStatus);
                int rows = await con.ExecuteAsync(
                    "V3M_ADM_InsertUpdateRegistrationStatus",
                    param,
                    commandType: CommandType.StoredProcedure);
                return rows.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<CommonDomainLarge>> RFM_GetRegFormatTypeDate(FormateType commonClass)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", commonClass.GroupCode);
                param.Add("@BranchCode", commonClass.BranchCode);
                param.Add("@SessionId", commonClass.SessionId);
                param.Add("@IsValid", commonClass.Mode);
                param.Add("@FormatName", commonClass.formateType);
                var res= await con.QueryAsync<CommonDomainLarge>("Usp_GetRegFormatType", param,commandType: CommandType.StoredProcedure);
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> RFM_AddUpdateRegFormat(CommonDomainLarge commonDomainLarge)
        {
            try
            {

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@FormatTypeID", commonDomainLarge.FormatTypeID);
                param.Add("@GroupCode", commonDomainLarge.GroupCode);
                param.Add("@BranchCode", commonDomainLarge.BranchCode);
                param.Add("@SessionId", commonDomainLarge.SessionId);
                param.Add("@ClassCode", commonDomainLarge.ClassCode);
                param.Add("@FormatName", commonDomainLarge.Name);
                param.Add("@StartDate", commonDomainLarge.StartDate);
                param.Add("@EndDate", commonDomainLarge.EndDate);
                param.Add("@isEmail", commonDomainLarge.isEmail);
                param.Add("@isSMS", commonDomainLarge.isEmail);
                param.Add("@isWhatsapp", commonDomainLarge.isWhatsapp);
                param.Add("@EmailText", commonDomainLarge.EmailText);
                param.Add("@SMSText", commonDomainLarge.SMSText);
                param.Add("@StartTime", commonDomainLarge.StartTime);
                param.Add("@EndTime", commonDomainLarge.EndTime);
                param.Add("@Description", commonDomainLarge.Description);
                param.Add("@CreatedBy", commonDomainLarge.CreatedBy);
                param.Add("@EmailOnPayment", commonDomainLarge.EmailTextOnPayment);
                param.Add("@SMSTextOnPayment", commonDomainLarge.SMSTextOnPayment);
                param.Add("@WhatsappTextOnPayment", commonDomainLarge.WhatsappTextOnPayment);
                param.Add("@WhatsappText", commonDomainLarge.WhatsappText);
                param.Add("@RegFee", commonDomainLarge.RegFee);
                param.Add("@MinAge", commonDomainLarge.MinAge);
                param.Add("@MaxAge", commonDomainLarge.MaxAge);
                param.Add("@Status", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                await con.ExecuteAsync("Usp_RegFormatType", param, commandType: CommandType.StoredProcedure);
                string statuscode = param.Get<string>("@Status");
                return statuscode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

          public async Task<int> RFM_Active(int Id){
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE ADM_RegFormatType SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE FormatTypeID = @FormatTypeID";
                return await con.ExecuteAsync(sql, new { FormatTypeID = Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<RegistrationDto>> GetRegistrationDetails(RegistrationSearchDto model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@StatusSrc", model.AppStatus);
                param.Add("@ClassCode", model.ClassCode);
                param.Add("@Gender", model.Gender);
                param.Add("@DateFrom", model.DateFrom);
                param.Add("@DateTo", model.DateTo);
                param.Add("@RegistrationNo", model.RegistrationNo);
                param.Add("@EWS", model.EWS);
                param.Add("@Sibling", model.Sibling);
                param.Add("@IsTransport", model.IsTransport);
                param.Add("@DocumentSelected", model.DocumentSelected);
                param.Add("@TransportDistance", model.TransportDistance);
                param.Add("@RegistrationFrom", model.RegistrationFrom);
                param.Add("@RegistrationTo", model.RegistrationTo);
                param.Add("@PointsFrom", model.PointsFrom);
                param.Add("@PointsTo", model.PointsTo);
                param.Add("@StudentName", model.StudentName);
                return await con.QueryAsync<RegistrationDto>(
                    "USP_GetRegistrationEnterDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> UpdateRegistrationStatus(UpdateRegistrationStatusModel model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ADM_UpdateregistrationStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GroupCode", model.GroupCode);
                    cmd.Parameters.AddWithValue("@BranchCode", model.BranchCode);
                    cmd.Parameters.AddWithValue("@SessionId", model.SessionId);
                    cmd.Parameters.AddWithValue("@RegistrationId", model.RegistrationId);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParameter);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    return Convert.ToInt32(returnParameter.Value);
                }
            }
        }
    }
}