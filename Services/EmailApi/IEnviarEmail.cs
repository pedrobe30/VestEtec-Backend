using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Backend_Vestetec_App.Services
{
    public interface IEnviarEmail
    {
        Task<bool> EnviarCodigoVerificacao(string email, string codigo);
    }
}