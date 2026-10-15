using System.Reflection;
using ApplicationInterface.SuperAdmin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;
using MyApp.Common;
using SchoolEye.Tests.TestHelpers;
using ServerWebAPI.Addmission.Controllers.Admin;

namespace SchoolEye.Tests.Addmission.Admin;

public class RegistrationControllerTests
{
    private const string Group = "GRP-TEST";
    private const string Branch = "BR-TEST";
    private const long Session = 7;
    private const long RegId = 1001;

    private readonly Mock<IRegistrationRepository> _repo = new();
    private readonly RegistrationController _controller;

    public RegistrationControllerTests()
    {
        _controller = new RegistrationController(_repo.Object, LocalizerMock.Create().Object);
    }

    private static Exception DbFailure() => new InvalidOperationException(ResultAssert.SecretExceptionText);

    #region Security

    [Fact]
    public void Controller_HasAuthorizeAttribute()
    {
        Assert.True(SecurityAssert.HasAuthorize(typeof(RegistrationController)));
        Assert.True(SecurityAssert.IsApiController(typeof(RegistrationController)));
    }

    [Fact]
    public void Controller_OnlyOnlineRegistrationIsAnonymous()
    {
        Assert.Equal(new[] { nameof(RegistrationController.OnlineRegistrationData) },
                     SecurityAssert.AnonymousActions(typeof(RegistrationController)));
    }

    // CLAUDE.md §8 (Insecure API Endpoints): the only anonymous, public write endpoint must be rate limited; policies exist in Program.cs but are not applied.
    [Fact]
    public void OnlineRegistration_IsRateLimited()
    {
        var method = typeof(RegistrationController).GetMethod(nameof(RegistrationController.OnlineRegistrationData))!;
        var limited = method.GetCustomAttribute<EnableRateLimitingAttribute>() != null
                   || typeof(RegistrationController).GetCustomAttribute<EnableRateLimitingAttribute>() != null;
        Assert.True(limited);
    }

    #endregion

    #region SearchRegistrationData (POST SearchRegistration)

    [Fact]
    public async Task SearchRegistration_PopulatedList_ReturnsOkWithData()
    {
        var search = new RegistrationSearchDto();
        var data = new List<RegistrationDto> { new() { RegistrationNo = "REG-T1" } };
        _repo.Setup(r => r.SearchAsync(search)).ReturnsAsync(data);

        var result = await _controller.SearchRegistrationData(search);

        var api = ResultAssert.Api<IEnumerable<RegistrationDto>>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.SearchAsync(It.Is<RegistrationSearchDto>(s => ReferenceEquals(s, search))), Times.Once);
    }

    [Fact]
    public async Task SearchRegistration_EmptyList_ReturnsOkWithEmptyData()
    {
        _repo.Setup(r => r.SearchAsync(It.IsAny<RegistrationSearchDto>())).ReturnsAsync(Enumerable.Empty<RegistrationDto>());

        var result = await _controller.SearchRegistrationData(new RegistrationSearchDto());

        var api = ResultAssert.Api<IEnumerable<RegistrationDto>>(result, 200);
        Assert.True(api.Success);
        Assert.Empty(api.Data!);
    }

    [Fact]
    public async Task SearchRegistration_RepositoryThrows_Returns500WithLocalizedError()
    {
        _repo.Setup(r => r.SearchAsync(It.IsAny<RegistrationSearchDto>())).ThrowsAsync(DbFailure());

        var result = await _controller.SearchRegistrationData(new RegistrationSearchDto());

        var api = ResultAssert.Api<string>(result, 500);
        Assert.False(api.Success);
        Assert.Equal("Error", api.Message);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region StudentRegistration (POST AddRegistration)

    [Fact]
    public async Task AddRegistration_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.StudentRegistration(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.Equal("InvalidData", api.Message);
        _repo.Verify(r => r.AddStudentRegistration(It.IsAny<RegistrationModal>()), Times.Never);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 400 response must not report Success = true.
    [Fact]
    public async Task AddRegistration_NullModel_ReturnsSuccessFalse()
    {
        var result = await _controller.StudentRegistration(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
    }

    [Fact]
    public async Task AddRegistration_RepositoryReturnsRegistrationNo_Returns201Code1()
    {
        var model = new RegistrationModal();
        _repo.Setup(r => r.AddStudentRegistration(model)).ReturnsAsync("REG-T-0001");

        var result = await _controller.StudentRegistration(model);

        var api = ResultAssert.Api<object>(result, 201);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("REG-T-0001", api.Data);
        Assert.Equal("Submit", api.Message);
        _repo.Verify(r => r.AddStudentRegistration(It.Is<RegistrationModal>(m => ReferenceEquals(m, model))), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddRegistration_RepositoryReturnsBlank_ReturnsFailedCode0(string? registrationNo)
    {
        _repo.Setup(r => r.AddStudentRegistration(It.IsAny<RegistrationModal>())).ReturnsAsync(registrationNo!);

        var result = await _controller.StudentRegistration(new RegistrationModal());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(0, api.Code);
        Assert.Equal("Registration failed.", api.Message);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): "Registration failed." must not report Success = true.
    [Fact]
    public async Task AddRegistration_RepositoryReturnsBlank_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.AddStudentRegistration(It.IsAny<RegistrationModal>())).ReturnsAsync(string.Empty);

        var result = await _controller.StudentRegistration(new RegistrationModal());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    [Fact]
    public async Task AddRegistration_RepositoryThrows_Returns500WithSafeMessage()
    {
        _repo.Setup(r => r.AddStudentRegistration(It.IsAny<RegistrationModal>())).ThrowsAsync(DbFailure());

        var result = await _controller.StudentRegistration(new RegistrationModal());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.False(api.Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region RegistrationChildDetails (POST registrationchildupdate)

    [Fact]
    public async Task ChildUpdate_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.RegistrationChildDetails(null!);

        ResultAssert.Api<object>(result, 400);
        _repo.Verify(r => r.RegistrationChildDetails(It.IsAny<ChildDetails>()), Times.Never);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 400 response must not report Success = true.
    [Fact]
    public async Task ChildUpdate_NullModel_ReturnsSuccessFalse()
    {
        var result = await _controller.RegistrationChildDetails(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
    }

    [Fact]
    public async Task ChildUpdate_RepositoryReturnsNoRecordUpdated_ReturnsCode0()
    {
        _repo.Setup(r => r.RegistrationChildDetails(It.IsAny<ChildDetails>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationChildDetails(new ChildDetails());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(0, api.Code);
        Assert.Equal("No Record Updated", api.Message);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): "No Record Updated" must not report Success = true.
    [Fact]
    public async Task ChildUpdate_RepositoryReturnsNoRecordUpdated_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.RegistrationChildDetails(It.IsAny<ChildDetails>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationChildDetails(new ChildDetails());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    [Fact]
    public async Task ChildUpdate_RepositoryReturnsOther_ReturnsUpdatedCode1()
    {
        var model = new ChildDetails();
        _repo.Setup(r => r.RegistrationChildDetails(model)).ReturnsAsync("1");

        var result = await _controller.RegistrationChildDetails(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("Updated", api.Message);
        _repo.Verify(r => r.RegistrationChildDetails(It.Is<ChildDetails>(m => ReferenceEquals(m, model))), Times.Once);
    }

    [Fact]
    public async Task ChildUpdate_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.RegistrationChildDetails(It.IsAny<ChildDetails>())).ThrowsAsync(DbFailure());

        var result = await _controller.RegistrationChildDetails(new ChildDetails());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.False(api.Success);
        Assert.Equal("Error", api.Message);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GET by registrationId: childParent / otherInformation / child / FamilyDetails

    [Fact]
    public async Task GetParentsChild_RegistrationIdZero_ReturnsBadRequestWithoutCallingRepository()
    {
        var result = await _controller.GetRegistrationParentsChildData(Group, Branch, Session, 0);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        Assert.Equal("RequiredField", ResultAssert.Prop(ResultAssert.ValueOf(result), "message")?.ToString());
        _repo.Verify(r => r.GetRegistrationParentsChildById(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetParentsChild_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetRegistrationParentsChildById(Group, Branch, Session, RegId)).ReturnsAsync((ParentsDetails)null!);

        var result = await _controller.GetRegistrationParentsChildData(Group, Branch, Session, RegId);

        Assert.Equal(404, ResultAssert.StatusOf(result));
        Assert.Equal("NoData", ResultAssert.Prop(ResultAssert.ValueOf(result), "message")?.ToString());
    }

    [Fact]
    public async Task GetParentsChild_Found_ReturnsOkAndForwardsTenantScope()
    {
        var data = new ParentsDetails();
        _repo.Setup(r => r.GetRegistrationParentsChildById(Group, Branch, Session, RegId)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationParentsChildData(Group, Branch, Session, RegId);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
        _repo.Verify(r => r.GetRegistrationParentsChildById(Group, Branch, Session, RegId), Times.Once);
    }

    [Fact]
    public async Task GetParentsChild_RepositoryThrows_Returns500WithSafeMessage()
    {
        _repo.Setup(r => r.GetRegistrationParentsChildById(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationParentsChildData(Group, Branch, Session, RegId);

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task GetOtherInformation_RegistrationIdZero_ReturnsBadRequest()
    {
        var result = await _controller.GetRegistrationChildotherInformation(Group, Branch, Session, 0);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.GetRegistrationChildotherInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetOtherInformation_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetRegistrationChildotherInformation(Group, Branch, Session, RegId)).ReturnsAsync((PointsCriteria)null!);

        var result = await _controller.GetRegistrationChildotherInformation(Group, Branch, Session, RegId);

        Assert.Equal(404, ResultAssert.StatusOf(result));
        Assert.Equal("RecordNotFound", ResultAssert.Prop(ResultAssert.ValueOf(result), "message")?.ToString());
    }

    [Fact]
    public async Task GetOtherInformation_Found_ReturnsOk()
    {
        var data = new PointsCriteria();
        _repo.Setup(r => r.GetRegistrationChildotherInformation(Group, Branch, Session, RegId)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationChildotherInformation(Group, Branch, Session, RegId);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
    }

    [Fact]
    public async Task GetOtherInformation_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetRegistrationChildotherInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationChildotherInformation(Group, Branch, Session, RegId);

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task GetChild_RegistrationIdZero_ReturnsBadRequest()
    {
        var result = await _controller.GetRegistrationChildData(Group, Branch, Session, 0);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.GetRegistrationChildData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetChild_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetRegistrationChildData(Group, Branch, Session, RegId)).ReturnsAsync((ChildDetails)null!);

        var result = await _controller.GetRegistrationChildData(Group, Branch, Session, RegId);

        Assert.Equal(404, ResultAssert.StatusOf(result));
    }

    [Fact]
    public async Task GetChild_Found_ReturnsOkAndForwardsTenantScope()
    {
        var data = new ChildDetails();
        _repo.Setup(r => r.GetRegistrationChildData(Group, Branch, Session, RegId)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationChildData(Group, Branch, Session, RegId);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
        _repo.Verify(r => r.GetRegistrationChildData(Group, Branch, Session, RegId), Times.Once);
    }

    [Fact]
    public async Task GetChild_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetRegistrationChildData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationChildData(Group, Branch, Session, RegId);

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task GetFamilyDetails_RegistrationIdZero_ReturnsBadRequest()
    {
        var result = await _controller.GetRegistrationFamilyDetails(Group, Branch, Session, 0);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.GetRegistrationFamilyDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetFamilyDetails_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetRegistrationFamilyDetails(Group, Branch, Session, RegId)).ReturnsAsync((List<FamilyDetails>)null!);

        var result = await _controller.GetRegistrationFamilyDetails(Group, Branch, Session, RegId);

        Assert.Equal(404, ResultAssert.StatusOf(result));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task GetFamilyDetails_ListReturned_ReturnsOk(int count)
    {
        var data = Enumerable.Range(0, count).Select(_ => new FamilyDetails()).ToList();
        _repo.Setup(r => r.GetRegistrationFamilyDetails(Group, Branch, Session, RegId)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationFamilyDetails(Group, Branch, Session, RegId);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
    }

    [Fact]
    public async Task GetFamilyDetails_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetRegistrationFamilyDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationFamilyDetails(Group, Branch, Session, RegId);

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    // CLAUDE.md §5.11/§7.3 (Validate input): a negative registrationId is invalid and must be rejected before reaching the database (only 0 is checked).
    [Fact]
    public async Task GetByRegistrationId_NegativeId_ReturnsBadRequest()
    {
        var statuses = new[]
        {
            ResultAssert.StatusOf(await _controller.GetRegistrationParentsChildData(Group, Branch, Session, -5)),
            ResultAssert.StatusOf(await _controller.GetRegistrationChildotherInformation(Group, Branch, Session, -5)),
            ResultAssert.StatusOf(await _controller.GetRegistrationChildData(Group, Branch, Session, -5)),
            ResultAssert.StatusOf(await _controller.GetRegistrationFamilyDetails(Group, Branch, Session, -5)),
            ResultAssert.StatusOf(await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, -5, "P")),
        };

        Assert.All(statuses, s => Assert.Equal(400, s));
    }

    #endregion

    #region RegistrationAdditionalInformation (POST registrationInformation)

    [Fact]
    public async Task AdditionalInfo_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.RegistrationAdditionalInformation(null!);

        ResultAssert.Api<object>(result, 400);
        _repo.Verify(r => r.RegistrationAdditionalInformation(It.IsAny<PointsCriteria>()), Times.Never);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 400 response must not report Success = true.
    [Fact]
    public async Task AdditionalInfo_NullModel_ReturnsSuccessFalse()
    {
        var result = await _controller.RegistrationAdditionalInformation(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
    }

    [Fact]
    public async Task AdditionalInfo_RepositoryReturnsNoRecordUpdated_ReturnsSuccessFalseCode0()
    {
        _repo.Setup(r => r.RegistrationAdditionalInformation(It.IsAny<PointsCriteria>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationAdditionalInformation(new PointsCriteria());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
    }

    [Fact]
    public async Task AdditionalInfo_RepositoryReturnsMessage_ReturnsSuccessCode1WithMessage()
    {
        var model = new PointsCriteria();
        _repo.Setup(r => r.RegistrationAdditionalInformation(model)).ReturnsAsync("Saved");

        var result = await _controller.RegistrationAdditionalInformation(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("Saved", api.Message);
        _repo.Verify(r => r.RegistrationAdditionalInformation(It.Is<PointsCriteria>(m => ReferenceEquals(m, model))), Times.Once);
    }

    [Fact]
    public async Task AdditionalInfo_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.RegistrationAdditionalInformation(It.IsAny<PointsCriteria>())).ThrowsAsync(DbFailure());

        var result = await _controller.RegistrationAdditionalInformation(new PointsCriteria());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region RegistrationAddressInformation (POST registrationAddressInfo)

    [Fact]
    public async Task AddressInfo_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.RegistrationAddressInformation(null!);

        ResultAssert.Api<object>(result, 400);
        _repo.Verify(r => r.RegistrationAddressInformation(It.IsAny<AddressDetails>()), Times.Never);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 400 response must not report Success = true.
    [Fact]
    public async Task AddressInfo_NullModel_ReturnsSuccessFalse()
    {
        var result = await _controller.RegistrationAddressInformation(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
    }

    [Fact]
    public async Task AddressInfo_RepositoryReturnsNoRecordUpdated_ReturnsCode0()
    {
        _repo.Setup(r => r.RegistrationAddressInformation(It.IsAny<AddressDetails>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationAddressInformation(new AddressDetails());

        Assert.Equal(0, ResultAssert.Api<object>(result, 200).Code);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): "No Record Updated" must not report Success = true.
    [Fact]
    public async Task AddressInfo_RepositoryReturnsNoRecordUpdated_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.RegistrationAddressInformation(It.IsAny<AddressDetails>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationAddressInformation(new AddressDetails());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    [Fact]
    public async Task AddressInfo_RepositoryReturnsOther_ReturnsUpdatedCode1()
    {
        var model = new AddressDetails();
        _repo.Setup(r => r.RegistrationAddressInformation(model)).ReturnsAsync("1");

        var result = await _controller.RegistrationAddressInformation(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("Updated", api.Message);
    }

    [Fact]
    public async Task AddressInfo_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.RegistrationAddressInformation(It.IsAny<AddressDetails>())).ThrowsAsync(DbFailure());

        var result = await _controller.RegistrationAddressInformation(new AddressDetails());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetRegistrationChildAddressInformation (GET get-registration-AddressInformation)

    [Fact]
    public async Task GetAddress_RegistrationIdZero_ReturnsBadRequest()
    {
        var result = await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, 0, "P");

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.GetRegistrationChildAddressInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAddress_NotFound_Returns404WithNullData()
    {
        _repo.Setup(r => r.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, "P")).ReturnsAsync((AddressDetails)null!);

        var result = await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, "P");

        var api = ResultAssert.Api<AddressDetails>(result, 404);
        Assert.Null(api.Data);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 404 response must not report Success = true.
    [Fact]
    public async Task GetAddress_NotFound_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, "P")).ReturnsAsync((AddressDetails)null!);

        var result = await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, "P");

        Assert.False(ResultAssert.Api<AddressDetails>(result, 404).Success);
    }

    [Theory]
    [InlineData("P")]
    [InlineData("C")]
    public async Task GetAddress_Found_ReturnsOkAndForwardsAddressType(string addressType)
    {
        var data = new AddressDetails();
        _repo.Setup(r => r.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, addressType)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, addressType);

        var api = ResultAssert.Api<AddressDetails>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, addressType), Times.Once);
    }

    [Fact]
    public async Task GetAddress_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetRegistrationChildAddressInformation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationChildAddressInformation(Group, Branch, Session, RegId, "P");

        Assert.False(ResultAssert.Api<string>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region SubmitFamilyInfoDetails (POST InsertFamilyDetails)

    [Fact]
    public async Task InsertFamily_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.SubmitFamilyInfoDetails(null!);

        ResultAssert.Api<object>(result, 400);
        _repo.Verify(r => r.SubmitFamilyInfoDetails(It.IsAny<FamilyDetails>()), Times.Never);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a 400 response must not report Success = true.
    [Fact]
    public async Task InsertFamily_NullModel_ReturnsSuccessFalse()
    {
        var result = await _controller.SubmitFamilyInfoDetails(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
    }

    [Fact]
    public async Task InsertFamily_RepositoryReturnsId_Returns201Code1()
    {
        var model = new FamilyDetails();
        _repo.Setup(r => r.SubmitFamilyInfoDetails(model)).ReturnsAsync("55");

        var result = await _controller.SubmitFamilyInfoDetails(model);

        var api = ResultAssert.Api<object>(result, 201);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("55", api.Data);
        Assert.Equal("Inserted", api.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task InsertFamily_RepositoryReturnsBlank_ReturnsCode0(string? value)
    {
        _repo.Setup(r => r.SubmitFamilyInfoDetails(It.IsAny<FamilyDetails>())).ReturnsAsync(value);

        var result = await _controller.SubmitFamilyInfoDetails(new FamilyDetails());

        Assert.Equal(0, ResultAssert.Api<object>(result, 200).Code);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a failed insert must not report Success = true.
    [Fact]
    public async Task InsertFamily_RepositoryReturnsBlank_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.SubmitFamilyInfoDetails(It.IsAny<FamilyDetails>())).ReturnsAsync((string?)null);

        var result = await _controller.SubmitFamilyInfoDetails(new FamilyDetails());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    [Fact]
    public async Task InsertFamily_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.SubmitFamilyInfoDetails(It.IsAny<FamilyDetails>())).ThrowsAsync(DbFailure());

        var result = await _controller.SubmitFamilyInfoDetails(new FamilyDetails());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region RegistrationParentDetails (POST registration-parent-detail)

    [Fact]
    public async Task ParentDetail_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.RegistrationParentDetails(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.RegistrationParentDetails(It.IsAny<ParentsDetails>()), Times.Never);
    }

    [Fact]
    public async Task ParentDetail_InvalidModelState_ReturnsBadRequestWithoutCallingRepository()
    {
        _controller.ModelState.AddModelError("FatherName", "Required");

        var result = await _controller.RegistrationParentDetails(new ParentsDetails());

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.RegistrationParentDetails(It.IsAny<ParentsDetails>()), Times.Never);
    }

    [Fact]
    public async Task ParentDetail_NoRecordUpdated_Returns404()
    {
        _repo.Setup(r => r.RegistrationParentDetails(It.IsAny<ParentsDetails>())).ReturnsAsync("No Record Updated");

        var result = await _controller.RegistrationParentDetails(new ParentsDetails());

        Assert.Equal(404, ResultAssert.StatusOf(result));
    }

    [Fact]
    public async Task ParentDetail_Updated_ReturnsOkWithUpdatedMessage()
    {
        var model = new ParentsDetails();
        _repo.Setup(r => r.RegistrationParentDetails(model)).ReturnsAsync("1");

        var result = await _controller.RegistrationParentDetails(model);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Equal("Updated", ResultAssert.Prop(ResultAssert.ValueOf(result), "message")?.ToString());
        _repo.Verify(r => r.RegistrationParentDetails(It.Is<ParentsDetails>(m => ReferenceEquals(m, model))), Times.Once);
    }

    [Fact]
    public async Task ParentDetail_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.RegistrationParentDetails(It.IsAny<ParentsDetails>())).ThrowsAsync(DbFailure());

        var result = await _controller.RegistrationParentDetails(new ParentsDetails());

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region StudentDirectAdmission (POST StudentDirectAdmission)

    [Fact]
    public async Task DirectAdmission_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.StudentDirectAdmission(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.StudentDirectAdmissionData(It.IsAny<StudentDirectAdmissionModel>()), Times.Never);
    }

    [Fact]
    public async Task DirectAdmission_RepositoryReturnsMinus1_ReturnsBadRequest()
    {
        _repo.Setup(r => r.StudentDirectAdmissionData(It.IsAny<StudentDirectAdmissionModel>())).ReturnsAsync("-1");

        var result = await _controller.StudentDirectAdmission(new StudentDirectAdmissionModel());

        Assert.Equal(400, ResultAssert.StatusOf(result));
        Assert.Equal("Admission Failed", ResultAssert.ValueOf(result));
    }

    [Fact]
    public async Task DirectAdmission_RepositoryReturnsStudentNo_ReturnsOkCode1()
    {
        var model = new StudentDirectAdmissionModel();
        _repo.Setup(r => r.StudentDirectAdmissionData(model)).ReturnsAsync("STU-T-01");

        var result = await _controller.StudentDirectAdmission(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("STU-T-01", api.Data);
    }

    [Fact]
    public async Task DirectAdmission_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.StudentDirectAdmissionData(It.IsAny<StudentDirectAdmissionModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.StudentDirectAdmission(new StudentDirectAdmissionModel());

        Assert.Equal(500, ResultAssert.StatusOf(result));
    }

    // CLAUDE.md §8B: raw exception text must not reach the client (currently StatusCode(500, ex.Message)).
    [Fact]
    public async Task DirectAdmission_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.StudentDirectAdmissionData(It.IsAny<StudentDirectAdmissionModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.StudentDirectAdmission(new StudentDirectAdmissionModel());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetStudentDetailsData (GET GetStudentDetails)

    [Theory]
    [InlineData(null, Branch)]
    [InlineData("", Branch)]
    [InlineData(" ", Branch)]
    [InlineData(Group, null)]
    [InlineData(Group, "")]
    public async Task GetStudentDetails_MissingTenant_ReturnsBadRequest(string? group, string? branch)
    {
        var result = await _controller.GetStudentDetailsData(group!, branch!, Session, "1001", "P");

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.GetStudentDetailsData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetStudentDetails_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetStudentDetailsData(Group, Branch, Session, "1001", "P")).ReturnsAsync((GetRegistrationModel)null!);

        var result = await _controller.GetStudentDetailsData(Group, Branch, Session, "1001", "P");

        Assert.Equal(404, ResultAssert.StatusOf(result));
    }

    [Fact]
    public async Task GetStudentDetails_Found_ReturnsOkAndForwardsArguments()
    {
        var data = new GetRegistrationModel();
        _repo.Setup(r => r.GetStudentDetailsData(Group, Branch, Session, "1001", "P")).ReturnsAsync(data);

        var result = await _controller.GetStudentDetailsData(Group, Branch, Session, "1001", "P");

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
        _repo.Verify(r => r.GetStudentDetailsData(Group, Branch, Session, "1001", "P"), Times.Once);
    }

    // CLAUDE.md §7.6/§8B: exceptions must be caught and returned as a safe 500 (action has no try/catch and there is no global exception handler).
    [Fact]
    public async Task GetStudentDetails_RepositoryThrows_ReturnsSafe500()
    {
        _repo.Setup(r => r.GetStudentDetailsData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(DbFailure());

        IActionResult? result = null;
        var ex = await Record.ExceptionAsync(async () => result = await _controller.GetStudentDetailsData(Group, Branch, Session, "1001", "P"));

        Assert.Null(ex);
        Assert.Equal(500, ResultAssert.StatusOf(result!));
        ResultAssert.DoesNotLeak(result!);
    }

    #endregion

    #region AdmintStudentListData (POST StudentList)

    [Fact]
    public async Task StudentList_PopulatedList_ReturnsOk()
    {
        var request = new StudentListRequest();
        var data = new List<StudentListResponse> { new(), new() };
        _repo.Setup(r => r.AdmintStudentListData(request)).ReturnsAsync(data);

        var result = await _controller.AdmintStudentListData(request);

        var api = ResultAssert.Api<IEnumerable<StudentListResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.AdmintStudentListData(It.Is<StudentListRequest>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Fact]
    public async Task StudentList_EmptyList_ReturnsOkWithEmptyData()
    {
        _repo.Setup(r => r.AdmintStudentListData(It.IsAny<StudentListRequest>())).ReturnsAsync(Enumerable.Empty<StudentListResponse>());

        var result = await _controller.AdmintStudentListData(new StudentListRequest());

        Assert.Empty(ResultAssert.Api<IEnumerable<StudentListResponse>>(result, 200).Data!);
    }

    [Fact]
    public async Task StudentList_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.AdmintStudentListData(It.IsAny<StudentListRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.AdmintStudentListData(new StudentListRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task StudentList_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.AdmintStudentListData(It.IsAny<StudentListRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.AdmintStudentListData(new StudentListRequest());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetSiblingDetail (GET SiblingDetail)

    [Fact]
    public async Task SiblingDetail_NotFound_Returns404SuccessFalse()
    {
        _repo.Setup(r => r.GetSiblingDetail(Group, Branch, 7, "S1")).ReturnsAsync((SiblingDetailResponse?)null);

        var result = await _controller.GetSiblingDetail(Group, Branch, 7, "S1");

        Assert.False(ResultAssert.Api<object>(result, 404).Success);
    }

    [Fact]
    public async Task SiblingDetail_Found_ReturnsOk()
    {
        var data = new SiblingDetailResponse();
        _repo.Setup(r => r.GetSiblingDetail(Group, Branch, 7, "S1")).ReturnsAsync(data);

        var result = await _controller.GetSiblingDetail(Group, Branch, 7, "S1");

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.GetSiblingDetail(Group, Branch, 7, "S1"), Times.Once);
    }

    [Fact]
    public async Task SiblingDetail_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetSiblingDetail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetSiblingDetail(Group, Branch, 7, "S1");

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetFeeHeadConcession (GET FeeHeadConcession)

    [Fact]
    public async Task FeeHeadConcession_ReturnsOkAndForwardsArguments()
    {
        var data = new List<CommonDomain> { new() };
        _repo.Setup(r => r.GetFeeHeadConcession(Group, Branch, 7, "C1", 1)).ReturnsAsync(data);

        var result = await _controller.GetFeeHeadConcession(Group, Branch, 7, "C1", 1);

        Assert.Equal(200, ResultAssert.StatusOf(result));
        Assert.Same(data, ResultAssert.ValueOf(result));
        _repo.Verify(r => r.GetFeeHeadConcession(Group, Branch, 7, "C1", 1), Times.Once);
    }

    [Fact]
    public async Task FeeHeadConcession_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetFeeHeadConcession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetFeeHeadConcession(Group, Branch, 7, "C1", 1);

        Assert.Equal(500, ResultAssert.StatusOf(result));
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region OnlineRegistrationData (POST OnlineRegistration, anonymous)

    [Fact]
    public async Task OnlineRegistration_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.OnlineRegistrationData(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.InsertOnlineRegistration(It.IsAny<OnlineRegistration>()), Times.Never);
    }

    [Fact]
    public async Task OnlineRegistration_RepositoryReturnsNo_Returns201Code1()
    {
        var model = new OnlineRegistration();
        _repo.Setup(r => r.InsertOnlineRegistration(model)).ReturnsAsync("ONL-T-01");

        var result = await _controller.OnlineRegistrationData(model);

        var api = ResultAssert.Api<object>(result, 201);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("ONL-T-01", api.Data);
    }

    // Consistent ApiResponse (CLAUDE.md §7.2/§7.4): an empty insert result must not be reported as 201 Created / Success.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task OnlineRegistration_RepositoryReturnsBlank_IsNotReportedAsCreated(string? value)
    {
        _repo.Setup(r => r.InsertOnlineRegistration(It.IsAny<OnlineRegistration>())).ReturnsAsync(value!);

        var result = await _controller.OnlineRegistrationData(new OnlineRegistration());

        Assert.NotEqual(201, ResultAssert.StatusOf(result));
    }

    [Fact]
    public async Task OnlineRegistration_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.InsertOnlineRegistration(It.IsAny<OnlineRegistration>())).ThrowsAsync(DbFailure());

        var result = await _controller.OnlineRegistrationData(new OnlineRegistration());

        Assert.Equal(500, ResultAssert.StatusOf(result));
    }

    // CLAUDE.md §8B: raw exception text must not reach the client — critical here because this endpoint is anonymous.
    [Fact]
    public async Task OnlineRegistration_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.InsertOnlineRegistration(It.IsAny<OnlineRegistration>())).ThrowsAsync(DbFailure());

        var result = await _controller.OnlineRegistrationData(new OnlineRegistration());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region Registratiocancel (POST RegistrationCancel)

    [Fact]
    public async Task Cancel_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.Registratiocancel(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.RegistrationCancel(It.IsAny<RegistrationStatus>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_RowsAffected_ReturnsCode1()
    {
        var model = new RegistrationStatus();
        _repo.Setup(r => r.RegistrationCancel(model)).ReturnsAsync("1");

        var result = await _controller.Registratiocancel(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(1, api.Code);
        Assert.Equal("1", api.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Cancel_RepositoryReturnsBlank_ReturnsCode0(string? value)
    {
        _repo.Setup(r => r.RegistrationCancel(It.IsAny<RegistrationStatus>())).ReturnsAsync(value!);

        var result = await _controller.Registratiocancel(new RegistrationStatus());

        Assert.Equal(0, ResultAssert.Api<object>(result, 200).Code);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): RegistrationService.RegistrationCancel returns rows.ToString(), so "0" (nothing cancelled) is non-empty and reported as Code 1.
    [Fact]
    public async Task Cancel_ZeroRowsAffected_ReturnsCode0()
    {
        _repo.Setup(r => r.RegistrationCancel(It.IsAny<RegistrationStatus>())).ReturnsAsync("0");

        var result = await _controller.Registratiocancel(new RegistrationStatus());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(0, api.Code);
        Assert.False(api.Success);
    }

    // CLAUDE.md §7.6/§8B: exceptions must be caught and returned as a safe 500 ApiResponse (currently rethrown).
    [Fact]
    public async Task Cancel_RepositoryThrows_ReturnsSafe500()
    {
        _repo.Setup(r => r.RegistrationCancel(It.IsAny<RegistrationStatus>())).ThrowsAsync(DbFailure());

        IActionResult? result = null;
        var ex = await Record.ExceptionAsync(async () => result = await _controller.Registratiocancel(new RegistrationStatus()));

        Assert.Null(ex);
        Assert.Equal(500, ResultAssert.StatusOf(result!));
    }

    #endregion

    #region RegistrationFormateType (POST RegistratioFormateType)

    [Fact]
    public async Task FormatType_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.RegistrationFormateType(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.RFM_GetRegFormatTypeDate(It.IsAny<FormateType>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task FormatType_ListReturned_ReturnsOkCode1(int count)
    {
        var model = new FormateType();
        var data = Enumerable.Range(0, count).Select(_ => new CommonDomainLarge()).ToList();
        _repo.Setup(r => r.RFM_GetRegFormatTypeDate(model)).ReturnsAsync(data);

        var result = await _controller.RegistrationFormateType(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Same(data, api.Data);
    }

    [Fact]
    public async Task FormatType_NullResult_Returns404SuccessFalse()
    {
        _repo.Setup(r => r.RFM_GetRegFormatTypeDate(It.IsAny<FormateType>())).ReturnsAsync((IEnumerable<CommonDomainLarge>)null!);

        var result = await _controller.RegistrationFormateType(new FormateType());

        var api = ResultAssert.Api<object>(result, 404);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
    }

    // CLAUDE.md §7.6/§8B: exceptions must be caught and returned as a safe 500 ApiResponse (currently rethrown).
    [Fact]
    public async Task FormatType_RepositoryThrows_ReturnsSafe500()
    {
        _repo.Setup(r => r.RFM_GetRegFormatTypeDate(It.IsAny<FormateType>())).ThrowsAsync(DbFailure());

        IActionResult? result = null;
        var ex = await Record.ExceptionAsync(async () => result = await _controller.RegistrationFormateType(new FormateType()));

        Assert.Null(ex);
        Assert.Equal(500, ResultAssert.StatusOf(result!));
    }

    #endregion

    #region AddUpdateRegFormat (POST AddOrUpdateFormateType)

    [Fact]
    public async Task AddUpdateFormat_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.AddUpdateRegFormat(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        _repo.Verify(r => r.RFM_AddUpdateRegFormat(It.IsAny<CommonDomainLarge>()), Times.Never);
    }

    [Theory]
    [InlineData("1", 1, "Regformate type saved successfully")]
    [InlineData("2", 2, "Regformate type updated successfully")]
    [InlineData("-1", -1, "Regformate type already exists")]
    [InlineData("-2", -2, "This class is already assigned to another format")]
    public async Task AddUpdateFormat_KnownReturnCodes_MapToCodeAndMessage(string returnValue, int expectedCode, string expectedMessage)
    {
        var model = new CommonDomainLarge();
        _repo.Setup(r => r.RFM_AddUpdateRegFormat(model)).ReturnsAsync(returnValue);

        var result = await _controller.AddUpdateRegFormat(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(expectedCode, api.Code);
        Assert.Equal(expectedMessage, api.Message);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    public async Task AddUpdateFormat_SavedOrUpdated_ReturnsSuccessTrue(string returnValue)
    {
        _repo.Setup(r => r.RFM_AddUpdateRegFormat(It.IsAny<CommonDomainLarge>())).ReturnsAsync(returnValue);

        var result = await _controller.AddUpdateRegFormat(new CommonDomainLarge());

        Assert.True(ResultAssert.Api<object>(result, 200).Success);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): duplicate / conflict outcomes ("-1", "-2") must not report Success = true.
    [Theory]
    [InlineData("-1")]
    [InlineData("-2")]
    public async Task AddUpdateFormat_Conflict_ReturnsSuccessFalse(string returnValue)
    {
        _repo.Setup(r => r.RFM_AddUpdateRegFormat(It.IsAny<CommonDomainLarge>())).ReturnsAsync(returnValue);

        var result = await _controller.AddUpdateRegFormat(new CommonDomainLarge());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("")]
    [InlineData(null)]
    public async Task AddUpdateFormat_UnknownValue_ReturnsSuccessFalse(string? returnValue)
    {
        _repo.Setup(r => r.RFM_AddUpdateRegFormat(It.IsAny<CommonDomainLarge>())).ReturnsAsync(returnValue!);

        var result = await _controller.AddUpdateRegFormat(new CommonDomainLarge());

        Assert.False(ResultAssert.Api<object>(result, 200).Success);
    }

    // CLAUDE.md §7.6/§8B: exceptions must be caught and returned as a safe 500 ApiResponse (currently rethrown).
    [Fact]
    public async Task AddUpdateFormat_RepositoryThrows_ReturnsSafe500()
    {
        _repo.Setup(r => r.RFM_AddUpdateRegFormat(It.IsAny<CommonDomainLarge>())).ThrowsAsync(DbFailure());

        IActionResult? result = null;
        var ex = await Record.ExceptionAsync(async () => result = await _controller.AddUpdateRegFormat(new CommonDomainLarge()));

        Assert.Null(ex);
        Assert.Equal(500, ResultAssert.StatusOf(result!));
    }

    #endregion

    #region ActiveFormateType (POST ActiveFormate)

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task ActiveFormat_InvalidId_ReturnsBadRequest(int id)
    {
        var result = await _controller.ActiveFormateType(id);

        var api = ResultAssert.Api<string>(result, 400);
        Assert.False(api.Success);
        Assert.Equal("Invalid ID", api.Message);
        _repo.Verify(r => r.RFM_Active(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ActiveFormat_ValidId_ReturnsOkAndForwardsId()
    {
        _repo.Setup(r => r.RFM_Active(12)).ReturnsAsync(1);

        var result = await _controller.ActiveFormateType(12);

        var api = ResultAssert.Api<string>(result, 200);
        Assert.True(api.Success);
        _repo.Verify(r => r.RFM_Active(12), Times.Once);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): RFM_Active returns rows affected; 0 rows means the format was not found and must not report success.
    [Fact]
    public async Task ActiveFormat_ZeroRowsAffected_ReturnsFailure()
    {
        _repo.Setup(r => r.RFM_Active(12)).ReturnsAsync(0);

        var result = await _controller.ActiveFormateType(12);

        Assert.False(((ApiResponse<string>)ResultAssert.ValueOf(result)!).Success);
    }

    [Fact]
    public async Task ActiveFormat_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.RFM_Active(It.IsAny<int>())).ThrowsAsync(DbFailure());

        var result = await _controller.ActiveFormateType(12);

        Assert.False(ResultAssert.Api<string>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetRegistrationDetails (POST GetRegistrationDetails)

    [Fact]
    public async Task GetRegistrationDetails_NullResult_Returns404()
    {
        _repo.Setup(r => r.GetRegistrationDetails(It.IsAny<RegistrationSearchDto>())).ReturnsAsync((IEnumerable<RegistrationDto>)null!);

        var result = await _controller.GetRegistrationDetails(new RegistrationSearchDto());

        Assert.False(ResultAssert.Api<object>(result, 404).Success);
    }

    [Fact]
    public async Task GetRegistrationDetails_Found_ReturnsOk()
    {
        var model = new RegistrationSearchDto();
        var data = new List<RegistrationDto> { new() };
        _repo.Setup(r => r.GetRegistrationDetails(model)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationDetails(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
    }

    [Fact]
    public async Task GetRegistrationDetails_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetRegistrationDetails(It.IsAny<RegistrationSearchDto>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationDetails(new RegistrationSearchDto());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region updateStatusData (POST UpdateStudentStatus)

    [Fact]
    public async Task UpdateStatus_Returns0_Returns404()
    {
        _repo.Setup(r => r.UpdateRegistrationStatus(It.IsAny<UpdateRegistrationStatusModel>())).ReturnsAsync(0);

        var result = await _controller.updateStatusData(new UpdateRegistrationStatusModel());

        Assert.False(ResultAssert.Api<object>(result, 404).Success);
    }

    [Fact]
    public async Task UpdateStatus_Returns2_ReturnsOkCode2()
    {
        var model = new UpdateRegistrationStatusModel();
        _repo.Setup(r => r.UpdateRegistrationStatus(model)).ReturnsAsync(2);

        var result = await _controller.updateStatusData(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(2, api.Code);
        _repo.Verify(r => r.UpdateRegistrationStatus(It.Is<UpdateRegistrationStatusModel>(m => ReferenceEquals(m, model))), Times.Once);
    }

    // CLAUDE.md §7.4 (clear, safe messages): a status update reports "registration detail fetched successfully." — copy-paste message.
    [Fact]
    public async Task UpdateStatus_Returns2_MessageDescribesUpdate()
    {
        _repo.Setup(r => r.UpdateRegistrationStatus(It.IsAny<UpdateRegistrationStatusModel>())).ReturnsAsync(2);

        var result = await _controller.updateStatusData(new UpdateRegistrationStatusModel());

        Assert.Contains("updated", ResultAssert.Api<object>(result, 200).Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task UpdateStatus_OtherReturnValue_Returns500(int value)
    {
        _repo.Setup(r => r.UpdateRegistrationStatus(It.IsAny<UpdateRegistrationStatusModel>())).ReturnsAsync(value);

        var result = await _controller.updateStatusData(new UpdateRegistrationStatusModel());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
    }

    [Fact]
    public async Task UpdateStatus_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.UpdateRegistrationStatus(It.IsAny<UpdateRegistrationStatusModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.updateStatusData(new UpdateRegistrationStatusModel());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region DirectAdmissionStatusData (POST DirectAdmissionStatus)

    [Fact]
    public async Task DirectAdmissionStatus_NullModel_ReturnsBadRequestSuccessFalse()
    {
        var result = await _controller.DirectAdmissionStatusData(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
        _repo.Verify(r => r.DirectAdmissionStatusData(It.IsAny<RegistrationStatus>()), Times.Never);
    }

    [Fact]
    public async Task DirectAdmissionStatus_ReturnsData_ReturnsOkCode1()
    {
        var model = new RegistrationStatus();
        _repo.Setup(r => r.DirectAdmissionStatusData(model)).ReturnsAsync("ACTIVE");

        var result = await _controller.DirectAdmissionStatusData(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("ACTIVE", api.Data);
    }

    [Fact]
    public async Task DirectAdmissionStatus_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.DirectAdmissionStatusData(It.IsAny<RegistrationStatus>())).ThrowsAsync(DbFailure());

        var result = await _controller.DirectAdmissionStatusData(new RegistrationStatus());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.False(api.Success);
        Assert.Equal(-1, api.Code);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region AdmitChild (POST AdmitChild)

    [Fact]
    public async Task AdmitChild_NullModel_ReturnsBadRequestSuccessFalse()
    {
        var result = await _controller.AdmitChild(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
        _repo.Verify(r => r.AdmitChildAsync(It.IsAny<AdmitChildRequest>()), Times.Never);
    }

    [Fact]
    public async Task AdmitChild_NullResult_ReturnsAdmissionFailedCode0()
    {
        _repo.Setup(r => r.AdmitChildAsync(It.IsAny<AdmitChildRequest>())).ReturnsAsync((StudentListResponse)null!);

        var result = await _controller.AdmitChild(new AdmitChildRequest());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
        Assert.Equal("Student admission failed.", api.Message);
    }

    [Fact]
    public async Task AdmitChild_Admitted_ReturnsTypedStudentCode1()
    {
        var model = new AdmitChildRequest();
        var student = new StudentListResponse();
        _repo.Setup(r => r.AdmitChildAsync(model)).ReturnsAsync(student);

        var result = await _controller.AdmitChild(model);

        var api = ResultAssert.Api<StudentListResponse>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Same(student, api.Data);
    }

    [Fact]
    public async Task AdmitChild_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.AdmitChildAsync(It.IsAny<AdmitChildRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.AdmitChild(new AdmitChildRequest());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.Equal(-1, api.Code);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region VerifyAndTakeDocument / DeActivateDocumentData

    [Fact]
    public async Task VerifyDocument_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.VerifyAndTakeDocument(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
        _repo.Verify(r => r.VerifyAndTakeDocument(It.IsAny<StudentDocumentModel>()), Times.Never);
    }

    [Theory]
    [InlineData("1", 1, true)]
    [InlineData("2", 2, true)]
    [InlineData("0", 0, false)]
    [InlineData("-1", 0, false)]
    [InlineData(null, 0, false)]
    public async Task VerifyDocument_ReturnCodes_MapCorrectly(string? value, int expectedCode, bool expectedSuccess)
    {
        _repo.Setup(r => r.VerifyAndTakeDocument(It.IsAny<StudentDocumentModel>())).ReturnsAsync(value!);

        var result = await _controller.VerifyAndTakeDocument(new StudentDocumentModel());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(expectedSuccess, api.Success);
        Assert.Equal(expectedCode, api.Code);
    }

    [Fact]
    public async Task VerifyDocument_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.VerifyAndTakeDocument(It.IsAny<StudentDocumentModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.VerifyAndTakeDocument(new StudentDocumentModel());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task DeactivateDocument_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.DeActivateDocumentData(null!);

        Assert.False(ResultAssert.Api<object>(result, 400).Success);
        _repo.Verify(r => r.DeActivateDocumentData(It.IsAny<StudentDocumentModel>()), Times.Never);
    }

    [Theory]
    [InlineData("1", 1, true)]
    [InlineData("2", 0, false)]
    [InlineData("0", 0, false)]
    [InlineData(null, 0, false)]
    public async Task DeactivateDocument_ReturnCodes_MapCorrectly(string? value, int expectedCode, bool expectedSuccess)
    {
        _repo.Setup(r => r.DeActivateDocumentData(It.IsAny<StudentDocumentModel>())).ReturnsAsync(value!);

        var result = await _controller.DeActivateDocumentData(new StudentDocumentModel());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(expectedSuccess, api.Success);
        Assert.Equal(expectedCode, api.Code);
    }

    [Fact]
    public async Task DeactivateDocument_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.DeActivateDocumentData(It.IsAny<StudentDocumentModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.DeActivateDocumentData(new StudentDocumentModel());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetClassRegistrationDocuments / GetRegistrationStatusData

    [Fact]
    public async Task ClassDocuments_NullOrEmpty_Returns404()
    {
        _repo.SetupSequence(r => r.GetClassRegistrationDocumentsAsync(It.IsAny<ClassRegistrationDocumentsRequest>()))
             .ReturnsAsync((IEnumerable<ClassRegistrationDocumentsResponse>)null!)
             .ReturnsAsync(Enumerable.Empty<ClassRegistrationDocumentsResponse>());

        var first = await _controller.GetClassRegistrationDocuments(new ClassRegistrationDocumentsRequest());
        var second = await _controller.GetClassRegistrationDocuments(new ClassRegistrationDocumentsRequest());

        Assert.False(ResultAssert.Api<IEnumerable<ClassRegistrationDocumentsResponse>>(first, 404).Success);
        Assert.False(ResultAssert.Api<IEnumerable<ClassRegistrationDocumentsResponse>>(second, 404).Success);
    }

    [Fact]
    public async Task ClassDocuments_Populated_ReturnsOkCode1()
    {
        var request = new ClassRegistrationDocumentsRequest();
        var data = new List<ClassRegistrationDocumentsResponse> { new() };
        _repo.Setup(r => r.GetClassRegistrationDocumentsAsync(request)).ReturnsAsync(data);

        var result = await _controller.GetClassRegistrationDocuments(request);

        var api = ResultAssert.Api<IEnumerable<ClassRegistrationDocumentsResponse>>(result, 200);
        Assert.Equal(1, api.Code);
        Assert.Same(data, api.Data);
    }

    [Fact]
    public async Task ClassDocuments_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetClassRegistrationDocumentsAsync(It.IsAny<ClassRegistrationDocumentsRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetClassRegistrationDocuments(new ClassRegistrationDocumentsRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task RegistrationStatus_NullOrEmpty_Returns404()
    {
        _repo.SetupSequence(r => r.GetRegistrationStatusData(It.IsAny<RegistrationRequest>()))
             .ReturnsAsync((IEnumerable<RegistrationStatusResponse>)null!)
             .ReturnsAsync(Enumerable.Empty<RegistrationStatusResponse>());

        var first = await _controller.GetRegistrationStatusData(new RegistrationRequest());
        var second = await _controller.GetRegistrationStatusData(new RegistrationRequest());

        Assert.False(ResultAssert.Api<object>(first, 404).Success);
        Assert.False(ResultAssert.Api<object>(second, 404).Success);
    }

    [Fact]
    public async Task RegistrationStatus_Populated_ReturnsOkCode1()
    {
        var request = new RegistrationRequest();
        var data = new List<RegistrationStatusResponse> { new() };
        _repo.Setup(r => r.GetRegistrationStatusData(request)).ReturnsAsync(data);

        var result = await _controller.GetRegistrationStatusData(request);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Same(data, api.Data);
    }

    [Fact]
    public async Task RegistrationStatus_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetRegistrationStatusData(It.IsAny<RegistrationRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetRegistrationStatusData(new RegistrationRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion
}
