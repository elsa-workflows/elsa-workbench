using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Agents;
using Elsa.Email.Contracts;
using Microsoft.SemanticKernel;
using MimeKit;

namespace Elsa.Server.Agents.Web.AI.Plugins;

[Description("Provides functionality to send emails.")]
public class EmailPlugin(ISmtpService smtpService)
{
    [KernelFunction("send_email")]
    [Description("Sends an email to the specified recipient.")]
    public async Task SendEmailAsync(
        [Description("The recipient's email address.")] string recipient,
        [Description("The subject of the email.")] string subject,
        [Description("The body of the email.")] string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage
        {
            Subject = subject,
            Body = new TextPart("plain") { Text = body }
        };
        
        // Add the recipient to the message
        message.To.Add(MailboxAddress.Parse(recipient));
        
        // Optionally, you can set the sender's email address
        message.From.Add(new MailboxAddress("Loan Shark", "loan@shark.com"));
        
        // Send the email using the SMTP service
        await smtpService.SendAsync(message, cancellationToken);
    }
}

public class EmailPluginProvider : PluginProvider
{
    public override IEnumerable<PluginDescriptor> GetPlugins()
    {
        yield return PluginDescriptor.From<EmailPlugin>();
    }
}