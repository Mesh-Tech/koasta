using Koasta.Shared.Configuration;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    internal class MailjetRecipient
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    internal class MailjetVariables {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Copyright { get; set; }
    }

    internal class MailjetRequestBody
    {
        public MailjetRecipient From { get; set; }
        public List<MailjetRecipient> To { get; set; }
        public string Subject { get; set; }
        public string TextPart { get; set; }
        public string HTMLPart { get; set; }
        public int TemplateID { get; set; }
        public bool TemplateLanguage { get; set; }
        public string CustomID { get; set; }
        public MailjetVariables Variables { get; set; }
    }

    public class MailjetEmailSender : IEmailSender
    {
        private readonly MailjetClient client;
        private readonly bool ready;
        private readonly ILogger logger;

        public MailjetEmailSender(ISettings settings, ILoggerFactory loggerFactory)
        {
            ready = !string.IsNullOrWhiteSpace(settings.Auth.MailjetApiKey) && !string.IsNullOrWhiteSpace(settings.Auth.MailjetSecretKey);
            logger = loggerFactory.CreateLogger(nameof(MailjetEmailSender));

            if (!ready)
            {
                return;
            }

            client = new MailjetClient(settings.Auth.MailjetApiKey, settings.Auth.MailjetSecretKey) {
                Version = ApiVersion.V3_1
            };
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!ready)
            {
                logger.LogInformation($"SendMail: {WebUtility.HtmlDecode(htmlMessage)}");
                return;
            }

            var body = new List<MailjetRequestBody> {
                new MailjetRequestBody {
                    From = new MailjetRecipient {
                        Email = "hello@koasta.com",
                        Name = "Koasta"
                    },
                    To = new List<MailjetRecipient> {
                        new MailjetRecipient {
                            Email = email
                        }
                    },
                    Subject = subject,
                    TemplateID = 1582766,
                    TemplateLanguage = true,
                    CustomID = "KoastaWebEmail",
                    Variables = new MailjetVariables {
                        Title = subject,
                        Content = htmlMessage,
                        Copyright = "Copyright ⓒ 2020, Mesh Services Limited."
                    }
                }
            };

            var request = new MailjetRequest
            {
                Resource = Send.Resource
            }.Property(Send.Messages, JArray.FromObject(body));

            var response = await client.PostAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("SendMail", response.GetErrorMessage());
            }
        }
    }
}
