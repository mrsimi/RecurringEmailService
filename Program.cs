using Hangfire;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSendGrid(options => options.ApiKey = builder.Configuration["SendGridApiKey"]);

builder.Services.AddHangfire(configuration => 
    configuration.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();

var app = builder.Build();

app.MapHangfireDashboard("");

await app.StartAsync();
RecurringJob.AddOrUpdate<EmailJob>(emailJob => emailJob.SendEmail(), "*/15 * * * * *");
await app.WaitForShutdownAsync();

public class EmailJob
{
    private readonly ILogger<EmailJob> logger;
    private readonly ISendGridClient sendGridClient;

    public EmailJob(ILogger<EmailJob> logger, ISendGridClient sendGridClient)
    {
        this.logger = logger;
        this.sendGridClient = sendGridClient;
    }
    
    public async Task SendEmail()
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("[SENDER_EMAIL]", "[SENDER_NAME]"),
            Subject = "A Recurring Email using Twilio SendGrid",
            PlainTextContent = "Hello and welcome to the world of periodic emails with Hangfire and SendGrid. "
        };
        msg.AddTo(new EmailAddress("[RECIPIENT_EMAIL]", "[RECIPIENT_NAME]"));
        
        var response = await sendGridClient.SendEmailAsync(msg);

        if(response.IsSuccessStatusCode) logger.LogInformation( "Email queued successfully!");
        else logger.LogError("Failed to queue email");
    }

}