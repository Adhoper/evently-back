using Evently.Api.Services.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace Evently.Api.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BrevoEmailService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendPasswordResetAsync(
            string email,
            string firstName,
            string resetUrl)
        {
            var apiKey =
                _configuration["Email:ApiKey"];

            var senderEmail =
                _configuration["Email:SenderEmail"];

            var senderName =
                _configuration["Email:SenderName"]
                ?? "Evently";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "La API Key del servicio de correo no está configurada.");
            }

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException(
                    "El correo remitente no está configurado.");
            }

            var safeName =
                WebUtility.HtmlEncode(
                    firstName);

            var safeUrl =
                WebUtility.HtmlEncode(
                    resetUrl);

            var htmlContent = $"""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                    <meta charset="UTF-8">
                </head>

                <body style="
                    margin:0;
                    padding:0;
                    background:#f1f5f9;
                    font-family:Arial,Helvetica,sans-serif;
                    color:#0f172a;
                ">
                    <table
                        width="100%"
                        cellpadding="0"
                        cellspacing="0"
                        style="padding:40px 16px;"
                    >
                        <tr>
                            <td align="center">

                                <table
                                    width="100%"
                                    cellpadding="0"
                                    cellspacing="0"
                                    style="
                                        max-width:560px;
                                        background:#ffffff;
                                        border-radius:20px;
                                        overflow:hidden;
                                        border:1px solid #e2e8f0;
                                    "
                                >
                                    <tr>
                                        <td
                                            style="
                                                background:#0f172a;
                                                padding:30px;
                                                text-align:center;
                                            "
                                        >
                                            <div
                                                style="
                                                    font-size:24px;
                                                    font-weight:800;
                                                    color:#ffffff;
                                                "
                                            >
                                                EVENTLY
                                            </div>

                                            <div
                                                style="
                                                    width:35px;
                                                    height:4px;
                                                    margin:10px auto 0;
                                                    background:#facc15;
                                                    border-radius:20px;
                                                "
                                            ></div>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style="padding:36px 32px;">

                                            <h1
                                                style="
                                                    margin:0;
                                                    font-size:25px;
                                                "
                                            >
                                                Restablece tu contraseña
                                            </h1>

                                            <p
                                                style="
                                                    margin:20px 0 0;
                                                    line-height:1.7;
                                                    color:#64748b;
                                                "
                                            >
                                                Hola {safeName},
                                            </p>

                                            <p
                                                style="
                                                    line-height:1.7;
                                                    color:#64748b;
                                                "
                                            >
                                                Recibimos una solicitud para
                                                cambiar la contraseña de tu
                                                cuenta de Evently.
                                            </p>

                                            <div
                                                style="
                                                    text-align:center;
                                                    margin:32px 0;
                                                "
                                            >
                                                <a
                                                    href="{safeUrl}"
                                                    style="
                                                        display:inline-block;
                                                        background:#2563eb;
                                                        color:#ffffff;
                                                        text-decoration:none;
                                                        padding:14px 24px;
                                                        border-radius:10px;
                                                        font-weight:700;
                                                    "
                                                >
                                                    Restablecer contraseña
                                                </a>
                                            </div>

                                            <p
                                                style="
                                                    line-height:1.7;
                                                    color:#64748b;
                                                    font-size:14px;
                                                "
                                            >
                                                Este enlace expira en
                                                30 minutos y solo puede
                                                utilizarse una vez.
                                            </p>

                                            <p
                                                style="
                                                    line-height:1.7;
                                                    color:#64748b;
                                                    font-size:14px;
                                                "
                                            >
                                                Si no solicitaste este cambio,
                                                puedes ignorar este correo.
                                            </p>

                                            <hr
                                                style="
                                                    border:none;
                                                    border-top:1px solid #e2e8f0;
                                                    margin:30px 0;
                                                "
                                            >

                                            <p
                                                style="
                                                    margin:0;
                                                    text-align:center;
                                                    font-size:12px;
                                                    color:#94a3b8;
                                                "
                                            >
                                                © 2026 Evently.
                                                Todos los derechos reservados.
                                                <br>
                                                Hecho con ❤️ por Adhoper.
                                            </p>
                                        </td>
                                    </tr>
                                </table>

                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;

            var payload = new
            {
                sender = new
                {
                    name = senderName,
                    email = senderEmail
                },

                to = new[]
                {
                    new
                    {
                        email,
                        name = firstName
                    }
                },

                subject =
                    "Restablece tu contraseña de Evently",

                htmlContent
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "smtp/email");

            request.Headers.Add(
                "api-key",
                apiKey);

            request.Content =
                JsonContent.Create(
                    payload);

            var response =
                await _httpClient
                    .SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent =
                    await response.Content
                        .ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"Brevo rechazó el envío del correo. Status: {(int)response.StatusCode}. Response: {responseContent}");
            }
        }
    }
}