using System.Globalization;

namespace ATMS.Email.Resources;

public static class EmailMessages
{
    private static readonly System.Resources.ResourceManager ResourceManager = new(
        "ATMS.Email.Resources.EmailMessages",
        typeof(EmailMessages).Assembly);

    // culture is the recipient's, not the worker thread's
    public static string Get(string name, CultureInfo? culture = null) =>
        ResourceManager.GetString(name, culture ?? CultureInfo.CurrentUICulture) ?? name;

    public static string Greeting => Get(nameof(Greeting));
    public static string GreetingNoName => Get(nameof(GreetingNoName));
    public static string FooterAutomated => Get(nameof(FooterAutomated));
    public static string FooterNoReply => Get(nameof(FooterNoReply));
    public static string FooterCompany => Get(nameof(FooterCompany));
    public static string FooterAddress => Get(nameof(FooterAddress));
    public static string ButtonFallbackBefore => Get(nameof(ButtonFallbackBefore));
    public static string ButtonFallbackAfter => Get(nameof(ButtonFallbackAfter));
    public static string ConfirmationLink => Get(nameof(ConfirmationLink));
    public static string PasswordResetLink => Get(nameof(PasswordResetLink));
    public static string TaskLink => Get(nameof(TaskLink));
    public static string CommentLink => Get(nameof(CommentLink));
    public static string ProjectLink => Get(nameof(ProjectLink));
    public static string Someone => Get(nameof(Someone));
    public static string TaskKind => Get(nameof(TaskKind));
    public static string SubtaskKind => Get(nameof(SubtaskKind));
    public static string ProjectLabel => Get(nameof(ProjectLabel));
    public static string LinkValidUntil => Get(nameof(LinkValidUntil));
    public static string PasswordLabel => Get(nameof(PasswordLabel));
    public static string LoginLabel => Get(nameof(LoginLabel));

    public static string InviteSubject => Get(nameof(InviteSubject));
    public static string InviteHeading => Get(nameof(InviteHeading));
    public static string InviteReadyTitle => Get(nameof(InviteReadyTitle));
    public static string InviteReadyText => Get(nameof(InviteReadyText));
    public static string InvitedToBaim => Get(nameof(InvitedToBaim));
    public static string InvitedToProjectBefore => Get(nameof(InvitedToProjectBefore));
    public static string InvitedToProjectAfter => Get(nameof(InvitedToProjectAfter));
    public static string InviteCredentials => Get(nameof(InviteCredentials));
    public static string InviteButton => Get(nameof(InviteButton));

    public static string ForgotPasswordSubject => Get(nameof(ForgotPasswordSubject));
    public static string ForgotPasswordHeading => Get(nameof(ForgotPasswordHeading));
    public static string ForgotPasswordCardTitle => Get(nameof(ForgotPasswordCardTitle));
    public static string ForgotPasswordCardText => Get(nameof(ForgotPasswordCardText));
    public static string ForgotPasswordBody => Get(nameof(ForgotPasswordBody));
    public static string AccountLabel => Get(nameof(AccountLabel));
    public static string ForgotPasswordIgnore => Get(nameof(ForgotPasswordIgnore));
    public static string ForgotPasswordButton => Get(nameof(ForgotPasswordButton));

    public static string TaskAssignedSubject => Get(nameof(TaskAssignedSubject));
    public static string TaskAssignedTitle => Get(nameof(TaskAssignedTitle));
    public static string TaskAssignedHeading => Get(nameof(TaskAssignedHeading));
    public static string TaskAssignedAction => Get(nameof(TaskAssignedAction));
    public static string OpenTaskButton => Get(nameof(OpenTaskButton));

    public static string MentionedSubject => Get(nameof(MentionedSubject));
    public static string MentionedTitle => Get(nameof(MentionedTitle));
    public static string MentionedAction => Get(nameof(MentionedAction));
    public static string MentionedHint => Get(nameof(MentionedHint));
    public static string OpenCommentButton => Get(nameof(OpenCommentButton));

    public static string DueTodaySubject => Get(nameof(DueTodaySubject));
    public static string DueTodayHeading => Get(nameof(DueTodayHeading));
    public static string DueTodayBody => Get(nameof(DueTodayBody));
    public static string DueTodayHint => Get(nameof(DueTodayHint));

    public static string TaskOverdueSubject => Get(nameof(TaskOverdueSubject));
    public static string TaskOverdueHeading => Get(nameof(TaskOverdueHeading));
    public static string TaskOverdueBody => Get(nameof(TaskOverdueBody));
    public static string TaskOverdueHint => Get(nameof(TaskOverdueHint));

    public static string AddedToProjectSubject => Get(nameof(AddedToProjectSubject));
    public static string AddedToProjectTitle => Get(nameof(AddedToProjectTitle));
    public static string AddedToProjectHeading => Get(nameof(AddedToProjectHeading));
    public static string AddedToProjectCard => Get(nameof(AddedToProjectCard));
    public static string AddedToProjectBefore => Get(nameof(AddedToProjectBefore));
    public static string AddedToProjectAfter => Get(nameof(AddedToProjectAfter));
    public static string AddedToProjectHint => Get(nameof(AddedToProjectHint));
    public static string OpenProjectButton => Get(nameof(OpenProjectButton));
}
