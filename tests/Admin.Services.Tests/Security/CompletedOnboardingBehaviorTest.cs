using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using ATMS.Admin.Contracts.Security;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security;
using ATMS.Application.Exceptions.Conflict;
using MediatR;

namespace Admin.Services.Tests.Security;

public sealed class CompletedOnboardingBehaviorTest : Admin.Services.Tests.Handlers.BaseHandlerTest
{
    [Theory]
    [InlineData(typeof(GetProfileRequest))]
    [InlineData(typeof(UpdateSettingsCommand))]
    [InlineData(typeof(UpdateLanguageCommand))]
    [InlineData(typeof(ChangePasswordCommand))]
    public void ProfileAndPasswordRequests_RequireCompletedOnboarding(Type requestType)
    {
        Assert.True(requestType.IsDefined(typeof(CompletedOnboardingAccessAttribute), false));
    }

    [Fact]
    public async Task Handle_IncompleteOnboarding_RejectsBeforeHandler()
    {
        CurrentUserMock.Setup(x => x.HasCompletedOnboarding).Returns(false);
        var behavior = new CompletedOnboardingBehavior<GetProfileRequest, ProfileModel>(CurrentUserMock.Object);
        var called = false;

        var error = await Assert.ThrowsAsync<ConflictException>(() => behavior.Handle(
            new GetProfileRequest(),
            _ =>
            {
                called = true;
                return Task.FromResult(new ProfileModel());
            },
            CancellationToken.None));

        Assert.Equal(OnboardingMessages.OnboardingNotCompleted, error.Message);
        Assert.False(called);
    }

    [Fact]
    public async Task Handle_IncompleteOnboarding_RejectsAllProfileAndPasswordOperations()
    {
        CurrentUserMock.Setup(x => x.HasCompletedOnboarding).Returns(false);

        await AssertRejected<GetProfileRequest, ProfileModel>(new GetProfileRequest());
        await AssertRejected<UpdateSettingsCommand, ProfileModel>(new UpdateSettingsCommand
        {
            Name = "Jane", Surname = "Doe", PhoneNumber = "+994501234567", Position = "Developer",
            GenderId = 1, MaritalStatusId = 1
        });
        await AssertRejected<UpdateLanguageCommand, Unit>(new UpdateLanguageCommand { Language = "EN" });
        await AssertRejected<ChangePasswordCommand, AccessInfoModel>(new ChangePasswordCommand
        {
            OldPassword = "OldPass123!", NewPassword = "NewPass123!", ConfirmPassword = "NewPass123!"
        });
    }

    [Fact]
    public async Task Handle_CompletedOnboarding_AllowsHandler()
    {
        CurrentUserMock.Setup(x => x.HasCompletedOnboarding).Returns(true);
        var behavior = new CompletedOnboardingBehavior<GetProfileRequest, ProfileModel>(CurrentUserMock.Object);
        var model = new ProfileModel { Name = "Jane" };

        var result = await behavior.Handle(
            new GetProfileRequest(),
            _ => Task.FromResult(model),
            CancellationToken.None);

        Assert.Same(model, result);
    }

    [Fact]
    public async Task Handle_AnonymousResetRequest_DoesNotReadCurrentUser()
    {
        var behavior = new CompletedOnboardingBehavior<ResetPasswordCommand, string>(CurrentUserMock.Object);

        var result = await behavior.Handle(new ResetPasswordCommand
        {
            Token = "token", Password = "NewPass123!", ConfirmPassword = "NewPass123!"
        }, _ => Task.FromResult("handled"), CancellationToken.None);

        Assert.Equal("handled", result);
        CurrentUserMock.VerifyGet(x => x.HasCompletedOnboarding, Moq.Times.Never);
    }

    private async Task AssertRejected<TRequest, TResponse>(TRequest request) where TRequest : notnull
    {
        var behavior = new CompletedOnboardingBehavior<TRequest, TResponse>(CurrentUserMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() => behavior.Handle(
            request,
            _ => throw new InvalidOperationException("The handler must not run."),
            CancellationToken.None));
    }
}
