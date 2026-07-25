namespace ECommerse.Identity.Services;

public class NoOpEmailSender : IEmailSender<IdentityUser>
{
    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
        => Task.CompletedTask;

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
        => Task.CompletedTask;

    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
        => Task.CompletedTask;
}
