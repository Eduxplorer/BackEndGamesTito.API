using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BackEndGamesTito.API.Service
{
    public class SmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public SmsService(IConfiguration config)
        {
            var settings = config.GetSection("SmsSettings");
            _accountSid = settings["AccountSid"];
            _authToken = settings["AuthToken"];
            _fromNumber = settings["FromPhoneNumber"];

            // Inicializa o cliente do Twilio
            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendSmsAsync(string toPhoneNumber, string messageBody)
        {
            try
            {
                var message = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber(toPhoneNumber)
                );

                // Em produção, verificamos o message.Status para saber se foi entregue
                Console.WriteLine($"SMS enviado! SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                // Twilio lança exceções se o número for inválido ou se acabar o crédito
                throw new Exception($"Falha ao enviar SMS: {ex.Message}");
            }
        }
    }
}
