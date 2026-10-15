using ApplicationInterface.Admin;
using DomainModel.Admin;
using Moq;
using SchoolEye.Tests.TestHelpers;
using ServerWebAPI.Addmission.Controllers.Admin;

namespace SchoolEye.Tests.Addmission.Admin;

public class AdmissionDateControllerTests
{
    private readonly Mock<IAdmissionDateRepository> _repo = new();
    private readonly AdmissionDateController _controller;

    public AdmissionDateControllerTests()
    {
        _controller = new AdmissionDateController(_repo.Object);
    }

    private static Exception DbFailure() => new InvalidOperationException(ResultAssert.SecretExceptionText);

    #region Security

    [Fact]
    public void Controller_HasAuthorizeAndNoAnonymousActions()
    {
        Assert.True(SecurityAssert.HasAuthorize(typeof(AdmissionDateController)));
        Assert.True(SecurityAssert.IsApiController(typeof(AdmissionDateController)));
        Assert.Empty(SecurityAssert.AnonymousActions(typeof(AdmissionDateController)));
    }

    #endregion

    #region UpdateStudentAdmissionDate (POST UpdateStudentAdmissionDate)

    [Fact]
    public async Task UpdateAdmissionDate_Returns1_ReturnsOkCode1()
    {
        var request = new UpdateStudentAdmissionDateRequest();
        _repo.Setup(r => r.UpdateStudentAdmissionDate(request)).ReturnsAsync("1");

        var result = await _controller.UpdateStudentAdmissionDate(request);

        var api = ResultAssert.Api<string>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("1", api.Data);
        _repo.Verify(r => r.UpdateStudentAdmissionDate(It.Is<UpdateStudentAdmissionDateRequest>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("Admission date cannot be before registration date")]
    [InlineData("")]
    public async Task UpdateAdmissionDate_OtherValue_ReturnsBadRequestWithMessage(string value)
    {
        _repo.Setup(r => r.UpdateStudentAdmissionDate(It.IsAny<UpdateStudentAdmissionDateRequest>())).ReturnsAsync(value);

        var result = await _controller.UpdateStudentAdmissionDate(new UpdateStudentAdmissionDateRequest());

        var api = ResultAssert.Api<string>(result, 400);
        Assert.False(api.Success);
        Assert.Equal(value, api.Message);
        Assert.Null(api.Data);
    }

    [Fact]
    public async Task UpdateAdmissionDate_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.UpdateStudentAdmissionDate(It.IsAny<UpdateStudentAdmissionDateRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.UpdateStudentAdmissionDate(new UpdateStudentAdmissionDateRequest());

        Assert.False(ResultAssert.Api<string>(result, 500).Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task UpdateAdmissionDate_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.UpdateStudentAdmissionDate(It.IsAny<UpdateStudentAdmissionDateRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.UpdateStudentAdmissionDate(new UpdateStudentAdmissionDateRequest());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetSearchedStudent (POST GetSearchedStudent)

    [Fact]
    public async Task SearchedStudent_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.GetSearchedStudent(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.False(api.Success);
        _repo.Verify(r => r.GetSearchedStudent(It.IsAny<StuSearchedStudentRequest>()), Times.Never);
    }

    [Fact]
    public async Task SearchedStudent_NullOrEmpty_ReturnsNoStudentsCode0()
    {
        _repo.SetupSequence(r => r.GetSearchedStudent(It.IsAny<StuSearchedStudentRequest>()))
             .ReturnsAsync((IEnumerable<StuSearchedStudentResponse>)null!)
             .ReturnsAsync(Enumerable.Empty<StuSearchedStudentResponse>());

        foreach (var _ in Enumerable.Range(0, 2))
        {
            var result = await _controller.GetSearchedStudent(new StuSearchedStudentRequest());

            var api = ResultAssert.Api<object>(result, 200);
            Assert.False(api.Success);
            Assert.Equal(0, api.Code);
            Assert.Equal("No students found.", api.Message);
            Assert.Empty(Assert.IsAssignableFrom<IEnumerable<StuSearchedStudentResponse>>(api.Data));
        }
    }

    [Fact]
    public async Task SearchedStudent_Populated_ReturnsOkWithData()
    {
        var request = new StuSearchedStudentRequest();
        var data = new List<StuSearchedStudentResponse> { new(), new() };
        _repo.Setup(r => r.GetSearchedStudent(request)).ReturnsAsync(data);

        var result = await _controller.GetSearchedStudent(request);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.GetSearchedStudent(It.Is<StuSearchedStudentRequest>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Fact]
    public async Task SearchedStudent_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetSearchedStudent(It.IsAny<StuSearchedStudentRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetSearchedStudent(new StuSearchedStudentRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetClassRegistrationDocuments (POST GetClassRegistrationDocuments)

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
        var data = new List<ClassRegistrationDocumentsResponse> { new() };
        _repo.Setup(r => r.GetClassRegistrationDocumentsAsync(It.IsAny<ClassRegistrationDocumentsRequest>())).ReturnsAsync(data);

        var result = await _controller.GetClassRegistrationDocuments(new ClassRegistrationDocumentsRequest());

        var api = ResultAssert.Api<IEnumerable<ClassRegistrationDocumentsResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
    }

    [Fact]
    public async Task ClassDocuments_RepositoryThrows_Returns500Safe()
    {
        _repo.Setup(r => r.GetClassRegistrationDocumentsAsync(It.IsAny<ClassRegistrationDocumentsRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetClassRegistrationDocuments(new ClassRegistrationDocumentsRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetClassSectionDetail (POST GetClassSectionDetail)

    [Fact]
    public async Task ClassSection_Found_ReturnsOk()
    {
        var request = new StuSearchedStudentRequest();
        var data = new ClassSectionDetailResponse();
        _repo.Setup(r => r.GetClassSectionDetail(request)).ReturnsAsync(data);

        var result = await _controller.GetClassSectionDetail(request);

        var api = ResultAssert.Api<ClassSectionDetailResponse>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a missing record must not report Success = true with null Data.
    [Fact]
    public async Task ClassSection_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetClassSectionDetail(It.IsAny<StuSearchedStudentRequest>())).ReturnsAsync((ClassSectionDetailResponse)null!);

        var result = await _controller.GetClassSectionDetail(new StuSearchedStudentRequest());

        var api = Assert.IsAssignableFrom<MyApp.Common.ApiResponse<ClassSectionDetailResponse>>(ResultAssert.ValueOf(result));
        Assert.False(api.Success);
    }

    [Fact]
    public async Task ClassSection_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetClassSectionDetail(It.IsAny<StuSearchedStudentRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetClassSectionDetail(new StuSearchedStudentRequest());

        Assert.False(ResultAssert.Api<object>(result, 500).Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task ClassSection_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.GetClassSectionDetail(It.IsAny<StuSearchedStudentRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetClassSectionDetail(new StuSearchedStudentRequest());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion
}
