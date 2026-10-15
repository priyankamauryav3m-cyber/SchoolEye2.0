using ApplicationInterface.Admin;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SchoolEye.Tests.TestHelpers;
using ServerWebAPI.Addmission.Controllers.Admin;

namespace SchoolEye.Tests.Addmission.Admin;

public class EnquiryControllerTests
{
    private readonly Mock<IEnquiryRepository> _repo = new();
    private readonly EnquiryController _controller;

    public EnquiryControllerTests()
    {
        _controller = new EnquiryController(_repo.Object);
    }

    private static Exception DbFailure() => new InvalidOperationException(ResultAssert.SecretExceptionText);

    #region Security

    [Fact]
    public void Controller_IsApiController()
    {
        Assert.True(SecurityAssert.IsApiController(typeof(EnquiryController)));
    }

    // CLAUDE.md §8A/§8 (Broken Authorization): enquiry data holds student/parent contact details and must require a logged-in user.
    [Fact]
    public void Controller_HasAuthorizeAttribute()
    {
        Assert.True(SecurityAssert.HasAuthorize(typeof(EnquiryController)));
    }

    [Fact]
    public void Controller_HasNoAnonymousActions()
    {
        Assert.Empty(SecurityAssert.AnonymousActions(typeof(EnquiryController)));
    }

    #endregion

    #region SubmitEnquiry (POST AddOrUpdateEnquiry)

    [Fact]
    public async Task SubmitEnquiry_NullModel_ReturnsBadRequest()
    {
        var result = await _controller.SubmitEnquiry(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.False(api.Success);
        Assert.Equal("Data not found.", api.Message);
        _repo.Verify(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>()), Times.Never);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1025")]
    [InlineData("ENQ-0001")]
    public async Task SubmitEnquiry_RepositoryReturnsNewId_ReturnsSavedCode1(string returnValue)
    {
        var model = new EnquiryListResponse();
        _repo.Setup(r => r.SubmitEnquiryData(model)).ReturnsAsync(returnValue);

        var result = await _controller.SubmitEnquiry(model);

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal("Enquiry saved successfully", api.Message);
        Assert.Equal(returnValue, api.Data);
    }

    [Fact]
    public async Task SubmitEnquiry_RepositoryReturns2_ReturnsUpdatedCode2()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync("2");

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(2, api.Code);
        Assert.Equal("Enquiry updated successfully", api.Message);
    }

    [Fact]
    public async Task SubmitEnquiry_RepositoryReturnsMinus2_ReturnsNotFoundCode()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync("-2");

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.Equal(-2, api.Code);
        Assert.Equal("Enquiry Data Not Found", api.Message);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): a "not found" outcome must not report Success = true.
    [Fact]
    public async Task SubmitEnquiry_RepositoryReturnsMinus2_ReturnsSuccessFalse()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync("-2");

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.False(api.Success);
    }

    // Consistent ApiResponse (CLAUDE.md §7.4): negative return codes are failures; "-1" currently falls into the "saved" branch.
    [Fact]
    public async Task SubmitEnquiry_RepositoryReturnsMinus1_IsNotReportedAsSaved()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync("-1");

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.False(api.Success);
        Assert.NotEqual(1, api.Code);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("")]
    [InlineData(null)]
    public async Task SubmitEnquiry_RepositoryReturnsZeroOrEmpty_ReturnsUnknownCode0(string? returnValue)
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync(returnValue!);

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 200);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
        Assert.Equal("Unknown operation result", api.Message);
    }

    [Fact]
    public async Task SubmitEnquiry_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ThrowsAsync(DbFailure());

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.False(api.Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task SubmitEnquiry_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ThrowsAsync(DbFailure());

        var result = await _controller.SubmitEnquiry(new EnquiryListResponse());

        ResultAssert.DoesNotLeak(result);
    }

    [Fact]
    public async Task SubmitEnquiry_ForwardsSameModelToRepository()
    {
        var model = new EnquiryListResponse();
        _repo.Setup(r => r.SubmitEnquiryData(It.IsAny<EnquiryListResponse>())).ReturnsAsync("1");

        await _controller.SubmitEnquiry(model);

        _repo.Verify(r => r.SubmitEnquiryData(It.Is<EnquiryListResponse>(m => ReferenceEquals(m, model))), Times.Once);
    }

    #endregion

    #region GetEnquiryListofData (POST GetEnquiryList)

    [Fact]
    public async Task GetEnquiryList_PopulatedList_ReturnsOkWithData()
    {
        var data = new List<EnquiryListResponse> { new(), new() };
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ReturnsAsync(data);

        var result = await _controller.GetEnquiryListofData(new EnquiryRequestDto());

        var api = ResultAssert.Api<IEnumerable<EnquiryListResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
    }

    [Fact]
    public async Task GetEnquiryList_EmptyList_ReturnsOkWithEmptyData()
    {
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ReturnsAsync(new List<EnquiryListResponse>());

        var result = await _controller.GetEnquiryListofData(new EnquiryRequestDto());

        var api = ResultAssert.Api<IEnumerable<EnquiryListResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Empty(api.Data!);
    }

    [Fact]
    public async Task GetEnquiryList_ForwardsSameRequestToRepository()
    {
        var request = new EnquiryRequestDto();
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ReturnsAsync(new List<EnquiryListResponse>());

        await _controller.GetEnquiryListofData(request);

        _repo.Verify(r => r.GetEnquiryListofData(It.Is<EnquiryRequestDto>(m => ReferenceEquals(m, request))), Times.Once);
    }

    // CLAUDE.md §7.2: a server-side failure is a 500, not a client error (currently returns 400).
    [Fact]
    public async Task GetEnquiryList_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetEnquiryListofData(new EnquiryRequestDto());

        Assert.Equal(500, ResultAssert.StatusOf(result));
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task GetEnquiryList_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetEnquiryListofData(new EnquiryRequestDto());

        ResultAssert.DoesNotLeak(result);
    }

    // CLAUDE.md §7.4/§8B: error responses must use ApiResponse<T> (currently an anonymous object).
    [Fact]
    public async Task GetEnquiryList_RepositoryThrows_ReturnsApiResponseShape()
    {
        _repo.Setup(r => r.GetEnquiryListofData(It.IsAny<EnquiryRequestDto>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetEnquiryListofData(new EnquiryRequestDto());

        Assert.IsAssignableFrom<MyApp.Common.ApiResponse<object>>(ResultAssert.ValueOf(result));
    }

    #endregion

    #region GetFollowupDetails (POST GetFollowupDetails)

    [Fact]
    public async Task GetFollowupDetails_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.GetFollowupDetails(null!);

        Assert.Equal(400, ResultAssert.StatusOf(result));
        Assert.Equal("Invalid parameters", ResultAssert.ValueOf(result));
        _repo.Verify(r => r.GetFollowupDetails(It.IsAny<SearchAnyRequestModel>()), Times.Never);
    }

    [Fact]
    public async Task GetFollowupDetails_PopulatedList_ReturnsOkWithData()
    {
        var data = new List<FollowupDetailsResponse> { new() };
        var request = new SearchAnyRequestModel();
        _repo.Setup(r => r.GetFollowupDetails(request)).ReturnsAsync(data);

        var result = await _controller.GetFollowupDetails(request);

        var api = ResultAssert.Api<IEnumerable<FollowupDetailsResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Same(data, api.Data);
        _repo.Verify(r => r.GetFollowupDetails(It.Is<SearchAnyRequestModel>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Fact]
    public async Task GetFollowupDetails_EmptyList_ReturnsOkWithEmptyData()
    {
        _repo.Setup(r => r.GetFollowupDetails(It.IsAny<SearchAnyRequestModel>())).ReturnsAsync(new List<FollowupDetailsResponse>());

        var result = await _controller.GetFollowupDetails(new SearchAnyRequestModel());

        var api = ResultAssert.Api<IEnumerable<FollowupDetailsResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Empty(api.Data!);
    }

    [Fact]
    public async Task GetFollowupDetails_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.GetFollowupDetails(It.IsAny<SearchAnyRequestModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetFollowupDetails(new SearchAnyRequestModel());

        var api = ResultAssert.Api<string>(result, 500);
        Assert.False(api.Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task GetFollowupDetails_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.GetFollowupDetails(It.IsAny<SearchAnyRequestModel>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetFollowupDetails(new SearchAnyRequestModel());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region AddFollowupData (POST AddFollowup)

    [Fact]
    public async Task AddFollowup_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.AddFollowupData(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.False(api.Success);
        Assert.Equal("Data not found.", api.Message);
        _repo.Verify(r => r.AddFollowupDetails(It.IsAny<AddFollowupRequest>()), Times.Never);
    }

    [Fact]
    public async Task AddFollowup_RepositoryReturns2_ReturnsSuccessCode2()
    {
        var request = new AddFollowupRequest();
        _repo.Setup(r => r.AddFollowupDetails(request)).ReturnsAsync("2");

        var result = await _controller.AddFollowupData(request);

        var api = ResultAssert.Api<string>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(2, api.Code);
        Assert.Equal("Followup inserted and updated successfully", api.Message);
        _repo.Verify(r => r.AddFollowupDetails(It.Is<AddFollowupRequest>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Fact]
    public async Task AddFollowup_RepositoryReturnsMinus1_ReturnsEnquiryNotFound()
    {
        _repo.Setup(r => r.AddFollowupDetails(It.IsAny<AddFollowupRequest>())).ReturnsAsync("-1");

        var result = await _controller.AddFollowupData(new AddFollowupRequest());

        var api = ResultAssert.Api<string>(result, 200);
        Assert.False(api.Success);
        Assert.Equal(-1, api.Code);
        Assert.Equal("Enquiry data not found", api.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-2")]
    [InlineData("")]
    [InlineData(null)]
    public async Task AddFollowup_RepositoryReturnsOtherValue_ReturnsUnknownCode0(string? returnValue)
    {
        _repo.Setup(r => r.AddFollowupDetails(It.IsAny<AddFollowupRequest>())).ReturnsAsync(returnValue!);

        var result = await _controller.AddFollowupData(new AddFollowupRequest());

        var api = ResultAssert.Api<string>(result, 200);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
        Assert.Equal("Unknown operation result", api.Message);
    }

    [Fact]
    public async Task AddFollowup_RepositoryThrows_Returns500()
    {
        _repo.Setup(r => r.AddFollowupDetails(It.IsAny<AddFollowupRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.AddFollowupData(new AddFollowupRequest());

        var api = ResultAssert.Api<string>(result, 500);
        Assert.False(api.Success);
    }

    // CLAUDE.md §8B: raw exception text must not reach the client.
    [Fact]
    public async Task AddFollowup_RepositoryThrows_DoesNotLeakExceptionMessage()
    {
        _repo.Setup(r => r.AddFollowupDetails(It.IsAny<AddFollowupRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.AddFollowupData(new AddFollowupRequest());

        ResultAssert.DoesNotLeak(result);
    }

    #endregion

    #region GetEnquirySummaryData (POST GetEnquirySummaryData)

    [Fact]
    public async Task GetEnquirySummary_NullRequest_ReturnsBadRequestCode0()
    {
        var result = await _controller.GetEnquirySummaryData(null!);

        var api = ResultAssert.Api<object>(result, 400);
        Assert.False(api.Success);
        Assert.Equal(0, api.Code);
        _repo.Verify(r => r.GetEnquirySummaryData(It.IsAny<EnquirySummaryTableRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetEnquirySummary_RepositoryReturnsNull_ReturnsNoRecordsWithEmptyData()
    {
        _repo.Setup(r => r.GetEnquirySummaryData(It.IsAny<EnquirySummaryTableRequest>()))
             .ReturnsAsync((IEnumerable<EnquirySummaryTableResponse>)null!);

        var result = await _controller.GetEnquirySummaryData(new EnquirySummaryTableRequest());

        var api = ResultAssert.Api<IEnumerable<EnquirySummaryTableResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(0, api.Code);
        Assert.Equal("No records found.", api.Message);
        Assert.NotNull(api.Data);
        Assert.Empty(api.Data!);
    }

    [Fact]
    public async Task GetEnquirySummary_EmptyList_ReturnsNoRecordsCode0()
    {
        _repo.Setup(r => r.GetEnquirySummaryData(It.IsAny<EnquirySummaryTableRequest>()))
             .ReturnsAsync(Enumerable.Empty<EnquirySummaryTableResponse>());

        var result = await _controller.GetEnquirySummaryData(new EnquirySummaryTableRequest());

        var api = ResultAssert.Api<IEnumerable<EnquirySummaryTableResponse>>(result, 200);
        Assert.Equal(0, api.Code);
        Assert.Empty(api.Data!);
    }

    [Fact]
    public async Task GetEnquirySummary_PopulatedList_ReturnsCode1WithData()
    {
        var request = new EnquirySummaryTableRequest();
        var data = new List<EnquirySummaryTableResponse> { new(), new(), new() };
        _repo.Setup(r => r.GetEnquirySummaryData(request)).ReturnsAsync(data);

        var result = await _controller.GetEnquirySummaryData(request);

        var api = ResultAssert.Api<IEnumerable<EnquirySummaryTableResponse>>(result, 200);
        Assert.True(api.Success);
        Assert.Equal(1, api.Code);
        Assert.Equal(3, api.Data!.Count());
        _repo.Verify(r => r.GetEnquirySummaryData(It.Is<EnquirySummaryTableRequest>(m => ReferenceEquals(m, request))), Times.Once);
    }

    [Fact]
    public async Task GetEnquirySummary_RepositoryThrows_Returns500WithSafeMessage()
    {
        _repo.Setup(r => r.GetEnquirySummaryData(It.IsAny<EnquirySummaryTableRequest>())).ThrowsAsync(DbFailure());

        var result = await _controller.GetEnquirySummaryData(new EnquirySummaryTableRequest());

        var api = ResultAssert.Api<object>(result, 500);
        Assert.False(api.Success);
        Assert.Equal(-1, api.Code);
        ResultAssert.DoesNotLeak(result);
    }

    #endregion
}
