using ApplicationInterface.FinanceMNGT;
using Azure.Core;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FinanceMNGT
{
    public class ViewStudentService : IViewStudentRepository
    {

        private readonly string _connectionString;
        public ViewStudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ViewStudentModal>> GetViewStudentListData(StudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@SectionId", request.SectionId);
                parameters.Add("@Gender", request.Gender);
                parameters.Add("@StudentNo", request.StudentNo);
                parameters.Add("@StudentName", request.StudentName);
                parameters.Add("@IsSearchOnAdmDate", request.IsSearchOnAdmDate);
                parameters.Add("@AdmFromDate", request.AdmFromDate);
                parameters.Add("@StudentStatus", request.StudentStatus);
                parameters.Add("@AdmToDate", request.AdmToDate);
                parameters.Add("@ValidStatus", request.ValidStatus);
                parameters.Add("@OrderBy", request.OrderBy);
                var result = await con.QueryAsync<ViewStudentModal>(
                    "USP_GetStudentList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<ViewStudentModal>();
            }
        }

        public async Task<IEnumerable<GetSearchedViewStudentModel>> GetSearchedStudentListData(GetSearchedStudentRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@SectionCode", request.SectionCode);
                parameters.Add("@Gender", request.Gender);
                parameters.Add("@ControlNo", request.ControlNo);
                parameters.Add("@StudentName", request.StudentName);
                parameters.Add("@IsEWS", request.IsEWS);
                parameters.Add("@JoinType", request.JoinType);
                var result = await con.QueryAsync<GetSearchedViewStudentModel>(
                    "USP_GetSearchedStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<GetSearchedViewStudentModel>();
            }
        }
        public async Task<StudentViewDetailsModel?> GetStudentDetailsAsync(SearchAnyRequestModel requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@StudentId", requestModel.StudentId);

                var result = await con.QueryFirstOrDefaultAsync<StudentViewDetailsModel>("USP_GetStudentViewDetails", parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<StudentParentDetailsModel> GetStudentParentDetails(SearchAnyRequestModel requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@StudentId", requestModel.StudentId);

                var result = await con.QueryFirstOrDefaultAsync<StudentParentDetailsModel>("USP_GetStudentParentDetails", parameters, commandType: CommandType.StoredProcedure);
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }

        }
        public async Task<SiblingDetailsModel> GetSiblingDetailsData(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@SiblingID", request.RequestName);
                var result = await con.QueryFirstOrDefaultAsync<SiblingDetailsModel>("Sp_GetSiblingDetail", parameters, commandType: CommandType.StoredProcedure);
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> AddUpdateSiblingAsync(AddSiblingRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@ParentStudentId", request.ParentStudentId);
                parameters.Add("@childStudentId", request.ChildStudentId);
                parameters.Add("@ResultStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync("Usp_AddUpdateSibling", parameters, commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("@ResultStatus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> UpdateSiblingData(SearchAnyRequestModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@StudentId", model.StudentId);
                var rowsAffected = await con.ExecuteScalarAsync<int>(
                    "Usp_UpdateSiblingId",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SiblingDetailsModel>> GetSiblingListData(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@SessionId", request.SessionId);
                var result = await con.QueryAsync<SiblingDetailsModel>("ADM_GetSiblingDetailList", parameters, commandType: CommandType.StoredProcedure);
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<StudentAddressDetailsModel> GetStudentAddressDetails(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@StudentId", request.StudentId);
                param.Add("@AddressType", request.RequestName);
                var result = await con.QueryFirstOrDefaultAsync<StudentAddressDetailsModel>(
                    "USP_GetStudentAddress",
                    param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public async Task<int> AddOrUpdateStudentPersonalDetails(StudentViewDetailsModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@StudentId", model.StudentId);
                param.Add("@SessionId", model.SessionId);
                param.Add("@FirstName", model.FirstName);
                param.Add("@MiddleName", model.MiddleName);
                param.Add("@LastName", model.LastName);
                param.Add("@Gender", model.Gender);
                param.Add("@DateOfBirth", model.DateOfBirth);
                param.Add("@BloodGroup", model.BloodGroup);
                param.Add("@Nationality", model.Nationality);
                param.Add("@SocialCategory", model.SocialCategory);
                param.Add("@Religion", model.Religion);
                param.Add("@Background", model.Background);
                param.Add("@MotherTongue", model.MotherTongue);
                param.Add("@HouseNo", model.HouseNo);
                param.Add("@IsNRI", model.IsNRI);
                param.Add("@AdmissionNo", model.AdmissionNo);
                param.Add("@SpecialComments", model.SpecialComments);
                param.Add("@CreatedBy", model.CreatedBy);
                param.Add("@AadhaarNo", model.AadhaarNo);
                param.Add("@isEWS", model.IsEmailRequired);
                param.Add("@IsSMSAllow", model.IsSMSRequired);
                param.Add("@IsEmailAllow", model.IsEmailRequired);
                param.Add("@StudentCategory", model.SocialCategory);
                param.Add("@ImagePath", model.ImagePath);
                param.Add("@GsrnNo", model.GSRNNo);
                param.Add("@IsDisability", model.IsDisability);
                param.Add("@NatureOfDisability", model.NatureOfDisability);
                param.Add("@LastSchool", model.LastSchool);
                param.Add("@StateCode", model.StateCode);
                param.Add("@EWSId", model.EWSId);
                param.Add("@Caste", model.Caste);
                param.Add("@RecommendationBy", model.RecommendationBy);
                param.Add("@EmployeeApproched", model.EmployeeApproched);
                param.Add("@RecommendationDocPath", model.RecommendationDocPath);
                param.Add("@RecommendationDocFile", model.RecommendationDocFile);
                param.Add("@ApaarId", model.ApaarId);
                param.Add("@PenNo", model.PENNo);
                param.Add("@FamilyId", model.FamilyId);
                param.Add("@EmailId", model.EmailId);
                param.Add("@MobileNo", model.MobileNo);
                param.Add("@IsTrasportReq", model.IsTransportRequired);
                param.Add("@DistanceId", model.RouteDistance);
                param.Add("@TransportAppliedFrom", model.TransportAppliedFrom);
                param.Add("@FeeTemplateId", model.FeeTemplateId);
                param.Add("@CBSEGamesId", model.CBSEGamesId);
                param.Add("@StuBankName", model.StudentBankName);
                param.Add("@StuAccountNo", model.StudentBankAccount);
                param.Add("@StuBankIFSC", model.IFSC);
                param.Add("@AccountHolderName", model.AccountHolderName);
                param.Add("@LastSchool", model.LastSchool);
                param.Add("@LastClassStudied", model.LastClassStudied);
                param.Add("@LastAcademicSession", model.LastAcademicSession);
                param.Add("@LastSchoolResult", model.LastSchoolResult);
                param.Add("@Attendance", model.Attendance);
                param.Add("@CreatedBy", model.CreatedBy);
                param.Add("@IsCustody", model.IsCustody);
                param.Add("@CustodyStatus", model.CustodyStatus);
                param.Add("@LastSchoolAddress", model.LastSchoolAddress);
                param.Add("@LastSchoolTcNo", model.LastSchoolTcNo);
                param.Add("@TcDate", model.TcDate);
                param.Add("@LastSchoolBoard", model.LastSchoolBoard);
                param.Add("@Percentage", model.Percentage);
                param.Add("@Subjects", model.Subjects);
                param.Add("@IsTCAttach", model.IsTCAttach);
                param.Add("@IsSubjectApproved", model.IsSubjectApproved);
                param.Add("@ResultStatus", dbType: DbType.Int32,direction: ParameterDirection.Output);
                await con.ExecuteAsync( "Usp_StudentPersonalDetails", param, commandType: CommandType.StoredProcedure);
                return param.Get<int>("@ResultStatus");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public  async Task<StudentPassportVisaModel> GetStudentOtherDetails(SearchAnyRequestModel requestModel)

        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@StudentId", requestModel.StudentId);
                var result = await con.QueryFirstOrDefaultAsync<StudentPassportVisaModel>("USP_GetStudentPassportDetail", parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> SaveStudentParentDetailsData(StudentParentDetailsModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@FatherTitle", request.FatherTitle);
                parameters.Add("@FatherName", request.FatherName);
                parameters.Add("@FatherQualification", request.FatherQualification);
                parameters.Add("@FatherOccupation", request.FatherOccupation);
                parameters.Add("@FatherOccupationOther", request.FatherOccupationOther);
                parameters.Add("@FatherDesignation", request.FatherDesignation);
                parameters.Add("@FatherEmail", request.FatherEMail);
                parameters.Add("@FatherAnnualIncome", request.FatherAnnualIncome);
                parameters.Add("@FatherContactNo", request.FatherContactNo);
                parameters.Add("@FatherAchievement", request.FatherAchievement);
                parameters.Add("@FatherImagePath", request.FatherImagePath);
                parameters.Add("@MotherTitle", request.MotherTitle);
                parameters.Add("@MotherName", request.MotherName);
                parameters.Add("@MotherQualification", request.MotherQualification);
                parameters.Add("@MotherOccupation", request.MotherOccupation);
                parameters.Add("@MotherOccupationOther", request.MotherOccupationOther);
                parameters.Add("@MotherDesignation", request.MotherDesignation);
                parameters.Add("@MotherEmail", request.MotherEMail);
                parameters.Add("@MotherAnnualIncome", request.MotherAnnualIncome);
                parameters.Add("@MotherContactNo", request.MotherContactNo);
                parameters.Add("@MotherAchievement", request.MotherAchievement);
                parameters.Add("@MotherImagePath", request.MotherImagePath);
                parameters.Add("@SMSMobileNo", request.SMSMobileNo);
                parameters.Add("@EmergencyPersonName", request.EmergencyPersonName);
                parameters.Add("@EmergencyPersonRelationship", request.EmergencyPersonRelationShip);
                parameters.Add("@EmergencyPersonContactNo", request.EmergencyPersonContactNo);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@FatherOfficeContactNo", request.FatherOfficeContactNo);
                parameters.Add("@FatherOfficeAddress", request.FatherOfficeAddress);
                parameters.Add("@MotherOfficeContactNo", request.MotherOfficeContactNo);
                parameters.Add("@MotherOfficeAddress", request.MotherOfficeAddress);
                parameters.Add("@ContactEmail", request.ContactEmail);
                parameters.Add("@EmergencyPersonAddress", request.EmergencyPersonAddress);
                parameters.Add("@MotherCollege", request.MotherCollege);
                parameters.Add("@MotherOrganisation", request.MotherOrganisation);
                parameters.Add("@MotherDOB", request.MotherDOB);
                parameters.Add("@FatherDOB", request.FatherDOB);
                parameters.Add("@FatherCollege", request.FatherCollege);
                parameters.Add("@FatherOrganisation", request.FatherOrganisation);
                parameters.Add("@FatherAadhaar", request.FatherAadhaarNo);
                parameters.Add("@MotherAadhaar", request.MotherAadhaarNo);
                parameters.Add("@GuardianName", request.GuardianName);
                parameters.Add("@GuardianEmail", request.GuardianEmail);
                parameters.Add("@GuardianContactNo", request.GuardianContactNo);
                parameters.Add("@GuardianRelationship", request.GuardianRelationship);
                parameters.Add("@GuardianAddress", request.GuardianAddress);
                var result = await con.ExecuteAsync(
                    "Usp_StudentParentDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"SQL Error Number: {ex.Number}, Message: {ex.Message}",
                    ex);
            }
        }
        public async Task<int> SaveStudentAddressData(StudentAddressDetailsModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@Line1", request.Line1);
                parameters.Add("@Line2", request.Line2);
                ;parameters.Add("@PinCode", request.PinCode);
                parameters.Add("@ContactNo", request.ContactNo);
                parameters.Add("@AddressTo", request.AddressTo);
                parameters.Add("@AddressType", request.AddressType);
                parameters.Add("@CreatedBy", request.CreatedBy);
                var result = await con.ExecuteAsync(
                    "Usp_StudentAddressDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"SQL Error Number: {ex.Number}, Message: {ex.Message}",
                    ex);
            }
        }
        public async Task<int> SavePassportDetailsData(StudentPassportVisaModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@PassportNo", request.PassportNo);
                parameters.Add("@PassportType", request.PassportType);
                parameters.Add("@IsPassRegReq", request.PassportRegistrationRquired);
                parameters.Add("@PassportIssueDate", request.PassportIssueDate);
                parameters.Add("@PassportExpiryDate", request.PassportExpiryDate);
                parameters.Add("@VisaNo", request.VisaNo);
                parameters.Add("@RecommendationBy", request.RecommendationBy);
                parameters.Add("@RecommendationDocFile", request.RecommendationDocFile);
                parameters.Add("@RecommendationDocPath", request.RecommendationDocPath);
                parameters.Add("@EmployeeApproched", request.EmployeeApproched);
                parameters.Add("@SpecialComments", request.SpecialComments);
                parameters.Add("@VisaType", request.VisaType);
                parameters.Add("@IsVisaRegReq", request.VisaRegistrationRequired);
                parameters.Add("@VisaIssueDate", request.VisaIssueDate);
                parameters.Add("@VisaExpiryDate", request.VisaExpiryDate);
                parameters.Add("@CreatedBy", request.CreatedBy);
                var result = await con.ExecuteAsync(
                    "Usp_StudentPassportDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"SQL Error Number: {ex.Number}, Message: {ex.Message}",
                    ex);
            }
        }
        public async Task<StudentVisitorsModel> GetStudentVisitorsData(SearchAnyRequestModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@StudentId", model.StudentId);
                var result = await con.QueryFirstOrDefaultAsync<StudentVisitorsModel>(
                    "USP_GetStudentVisitorsDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while Class Wise Student Promotion List", ex);
            }
        }
        public async Task<int> SaveStudentVisitorsData(StudentVisitorsModel res)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", res.GroupCode);
                param.Add("@BranchCode", res.BranchCode);
                param.Add("@StudentId", res.StudentId);
                param.Add("@IsFatherAllowed", res.IsFatherAllowed);
                param.Add("@IsMotherAllowed", res.IsMotherAllowed);
                param.Add("@Visitor1Name", res.Visitor1Name);
                param.Add("@Visitor1Relationship", res.Visitor1Relation);
                param.Add("@IsVisitor1Allowed", res.IsVisitor1Allowed);
                param.Add("@Visitor1Email", res.Visitor1Email);
                param.Add("@Visitor1ContactNo", res.Visitor1ContactNo);
                param.Add("@Visitor1Remarks", res.Visitor1Remarks);
                param.Add("@Visitor2Name", res.Visitor2Name);
                param.Add("@Visitor2Relationship", res.Visitor2Relation);
                param.Add("@IsVisitor2Allowed", res.IsVisitor2Allowed);
                param.Add("@Visitor2Email", res.Visitor2Email);
                param.Add("@Visitor2ContactNo", res.Visitor2ContactNo);
                param.Add("@Visitor2Remarks", res.Visitor2Remarks);
                param.Add("@Visitor3Name", res.Visitor3Name);
                param.Add("@Visitor3Relationship", res.Visitor3Relation);
                param.Add("@IsVisitor3Allowed", res.IsVisitor3Allowed);
                param.Add("@Visitor3Email", res.Visitor3Email);
                param.Add("@Visitor3ContactNo", res.Visitor3ContactNo);
                param.Add("@Visitor3Remarks", res.Visitor3Remarks);
                param.Add("@Visitor4Name", res.Visitor4Name);
                param.Add("@Visitor4Relationship", res.Visitor4Relation);
                param.Add("@IsVisitor4Allowed", res.IsVisitor4Allowed);
                param.Add("@Visitor4Email", res.Visitor4Email);
                param.Add("@Visitor4ContactNo", res.Visitor4ContactNo);
                param.Add("@Visitor4Remarks", res.Visitor4Remarks);
                param.Add("@Visitor1ImagePath", res.Visitor1ImagePath);
                param.Add("@Visitor2ImagePath", res.Visitor2ImagePath);
                param.Add("@Visitor3ImagePath", res.Visitor3ImagePath);
                param.Add("@Visitor4ImagePath", res.Visitor4ImagePath);
                param.Add("@Visitor1SignImagePath", res.Visitor1SignImagePath);
                param.Add("@Visitor2SignImagePath", res.Visitor2SignImagePath);
                param.Add("@Visitor3SignImagePath", res.Visitor3SignImagePath);
                param.Add("@Visitor4SignImagePath", res.Visitor4SignImagePath);
                param.Add("@VisitorFSignImagePath", res.VisitorFSignImagePath);
                param.Add("@VisitorMSignImagePath", res.VisitorMSignImagePath); 
                param.Add("@CreatedBy", res.CreatedBy);
                return await con.ExecuteAsync(
                    "Usp_StudentVisitorsDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while saving Student Visitor Details: {ex.Message}", ex);
            }
        }
        public async Task<int> RemoveProfileImageData(ProfileImageModal model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@StudentId", model.StudentId);
                param.Add("@ImageFor", model.ImageFor);
                param.Add("@UpdatedBy", model.UpdatedBy);
                param.Add(
                    "@ResultStatus",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "Usp_RemoveProfileImage",
                    param,
                    commandType: CommandType.StoredProcedure);
                return param.Get<int>("@ResultStatus");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while saving Student Visitor Details: {ex.Message}", ex);
            }
        }

    }
}
