using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Backend_Vestetec_App.Services
{
    public class MailjetEmailService : IEnviarEmail
    {
        private readonly IConfiguration _configuration;

        public MailjetEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarCodigoVerificacao(string email, string codigo)
        {
            // Obter as chaves da configuração
            var apiKey = _configuration["Mailjet:ApiKey"];
            var secretKey = _configuration["Mailjet:SecretKey"];
            var senderEmail = _configuration["Mailjet:SenderEmail"];
            var senderName = _configuration["Mailjet:SenderEmail"];

            var cliente = new MailjetClient(apiKey, secretKey);

            var email_obj = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(senderEmail, senderName))
                .WithSubject($"Seu codigo de Verificação - Vestetec")
                .WithHtmlPart($"<h3> seu codigo de verificação é: {codigo}</h3> <p>use o codigo para verificar sua conta no vestetec.</p> ")
                .WithTo(new SendContact(email))
                .Build();

            var response = await cliente.SendTransactionalEmailAsync(email_obj);

            return response.Messages != null && response.Messages.All(m => m.Status == "success");
        }

    }
}