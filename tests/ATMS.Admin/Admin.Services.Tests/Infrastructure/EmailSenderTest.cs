using System.Globalization;
using ATMS.Email.Models;
using ATMS.Email.Services;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Infrastructure;

public class EmailSenderTest
{
    [Theory]
    [InlineData("ru", "ru-RU", "Подтвердите аккаунт")]
    [InlineData("RU", "ru-RU", "Подтвердите аккаунт")]
    [InlineData("az", "az-Latn-AZ", "Hesabı təsdiqləyin")]
    [InlineData("en", "en-US", "Confirm your account")]
    public async Task SendAsync_UsesTheRecipientCultureAndRestoresThePreviousOne(
        string language,
        string cultureName,
        string subject)
    {
        var previousCulture = new CultureInfo("fr-FR");
        var previousUiCulture = new CultureInfo("de-DE");
        CultureInfo.CurrentCulture = previousCulture;
        CultureInfo.CurrentUICulture = previousUiCulture;
        string? seenCulture = null;
        string? seenUiCulture = null;
        string? seenSubject = null;
        var email = FluentEmail(seen =>
        {
            seenCulture = CultureInfo.CurrentCulture.Name;
            seenUiCulture = CultureInfo.CurrentUICulture.Name;
        }, value => seenSubject = value);

        try
        {
            await new EmailSender(email.factory.Object, NullLogger<EmailSender>.Instance)
                .SendAsync("user@baim.az", language, Invite(), CancellationToken.None);

            Assert.Equal(cultureName, seenCulture);
            Assert.Equal(cultureName, seenUiCulture);
            Assert.Equal(subject, seenSubject);
            Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
            Assert.Equal("de-DE", CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }

    [Fact]
    public async Task SendAsync_WhenSendingThrows_RestoresThePreviousCulture()
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        CultureInfo.CurrentUICulture = new CultureInfo("de-DE");
        var email = FluentEmail(_ => { }, _ => { }, fail: true);

        try
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
                new EmailSender(email.factory.Object, NullLogger<EmailSender>.Instance)
                    .SendAsync("user@baim.az", "ru", Invite(), CancellationToken.None));

            Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
            Assert.Equal("de-DE", CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }

    private static InviteModel Invite() => new()
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        Email = "user@baim.az",
        Password = "Temporary1!",
        Link = "https://baim.az/confirm"
    };

    private static (Mock<IFluentEmailFactory> factory, Mock<IFluentEmail> email) FluentEmail(
        Action<string> duringRender,
        Action<string> subject,
        bool fail = false)
    {
        var email = new Mock<IFluentEmail>();
        email.Setup(x => x.To(It.IsAny<string>())).Returns(email.Object);
        email.Setup(x => x.Subject(It.IsAny<string>()))
            .Callback<string>(subject)
            .Returns(email.Object);
        email.Setup(x => x.UsingTemplateFromFile(It.IsAny<string>(), It.IsAny<InviteModel>(), It.IsAny<bool>()))
            .Callback(() => duringRender("render"))
            .Returns(email.Object);
        email.Setup(x => x.SendAsync(It.IsAny<CancellationToken?>()))
            .ReturnsAsync(() =>
            {
                var response = new SendResponse();
                if (fail)
                {
                    response.ErrorMessages.Add("SMTP unavailable");
                }

                return response;
            });
        var factory = new Mock<IFluentEmailFactory>();
        factory.Setup(x => x.Create()).Returns(email.Object);
        return (factory, email);
    }
}
