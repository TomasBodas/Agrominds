using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Services.Email
{
 /// <summary>
 /// Email sender using MailKit. Reads SMTP settings from App/Web.config.
 /// AppSettings keys:
 /// - SmtpHost
 /// - SmtpPort (int)
 /// - SmtpUser
 /// - SmtpPass
 /// - SmtpFrom (email)
 /// - SmtpFromName (optional)
 /// - SmtpUseStartTls (true/false) or SmtpUseSsl (true/false)
 /// - SmtpProtocolLogPath (optional file path for protocol logs)
 /// - SmtpDisableXOAUTH2 (optional true/false)
 /// </summary>
 public class EmailService
 {
 private readonly string _host = ConfigurationManager.AppSettings["SmtpHost"];
 private readonly int _port = ParseInt(ConfigurationManager.AppSettings["SmtpPort"],587);
 private readonly string _user = ConfigurationManager.AppSettings["SmtpUser"];
 private readonly string _pass = ConfigurationManager.AppSettings["SmtpPass"];
 private readonly string _from = ConfigurationManager.AppSettings["SmtpFrom"];
 private readonly string _fromName = ConfigurationManager.AppSettings["SmtpFromName"] ?? "AgroMinds";
 private readonly bool _useStartTls = ParseBool(ConfigurationManager.AppSettings["SmtpUseStartTls"], true);
 private readonly bool _useSsl = ParseBool(ConfigurationManager.AppSettings["SmtpUseSsl"], false);
 private readonly string _protocolLogPath = ConfigurationManager.AppSettings["SmtpProtocolLogPath"];
 private readonly bool _disableXOAUTH2 = ParseBool(ConfigurationManager.AppSettings["SmtpDisableXOAUTH2"], true);

 public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
 {
 if (string.IsNullOrWhiteSpace(_host) || string.IsNullOrWhiteSpace(_from))
 return false;

 var message = new MimeMessage();
 message.From.Add(new MailboxAddress(_fromName, _from));
 message.To.Add(MailboxAddress.Parse(toEmail));
 message.Subject = subject ?? string.Empty;
 message.Body = new BodyBuilder { HtmlBody = htmlBody ?? string.Empty }.ToMessageBody();

 // Preferred option from settings
 var preferred = SecureSocketOptions.None;
 if (_useSsl) preferred = SecureSocketOptions.SslOnConnect;
 else if (_useStartTls) preferred = SecureSocketOptions.StartTls;

 // Build options to try (in order)
 var optionsToTry = new SecureSocketOptions[]
 {
 preferred,
 SecureSocketOptions.StartTlsWhenAvailable,
 SecureSocketOptions.SslOnConnect,
 SecureSocketOptions.Auto,
 SecureSocketOptions.None
 };

 Exception lastError = null;
 foreach (var opt in optionsToTry)
 {
 try
 {
 using (var client = CreateClient())
 {
 client.Timeout =15000;
 await client.ConnectAsync(_host, _port, opt, ct).ConfigureAwait(false);
 if (_disableXOAUTH2)
 {
 client.AuthenticationMechanisms.Remove("XOAUTH2");
 }
 if (!string.IsNullOrEmpty(_user))
 {
 await client.AuthenticateAsync(_user, _pass, ct).ConfigureAwait(false);
 }
 await client.SendAsync(message, ct).ConfigureAwait(false);
 await client.DisconnectAsync(true, ct).ConfigureAwait(false);
 return true;
 }
 }
 catch (Exception ex)
 {
 lastError = ex;
 Trace.TraceWarning($"MailKit send attempt failed (opt={opt}): {ex.GetType().Name} - {ex.Message}");
 // try next option
 }
 }

 Trace.TraceError($"MailKit send failed after all attempts: {lastError}");
 return false;
 }

 private SmtpClient CreateClient()
 {
 if (!string.IsNullOrEmpty(_protocolLogPath))
 {
 try
 {
 string path = _protocolLogPath;
 // permitir ruta relativa
 if (!Path.IsPathRooted(path))
 {
 path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
 }
 var dir = Path.GetDirectoryName(path);
 if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
 {
 Directory.CreateDirectory(dir);
 }
 return new SmtpClient(new MailKit.ProtocolLogger(path));
 }
 catch (Exception ex)
 {
 Trace.TraceWarning("SMTP protocol log deshabilitado: " + ex.Message);
 // fallback sin logger
 }
 }
 return new SmtpClient();
 }

 /// <summary>
 /// Helper for password change notification email.
 /// </summary>
 public Task<bool> SendPasswordChangedAsync(string toEmail, string userName)
 {
 var subject = "Contraseña actualizada";
 var body = $@"<p>Hola {Escape(userName)},</p>
<p>Tu contraseña ha sido actualizada correctamente.</p>
<p>Si no fuiste vos, por favor contacta al soporte de AgroMinds inmediatamente.</p>
<p>Saludos,<br/>AgroMinds</p>";
 return SendEmailAsync(toEmail, subject, body);
 }

 /// <summary>
 /// Helper for password reset email with a temporary password.
 /// </summary>
 public Task<bool> SendPasswordResetAsync(string toEmail, string userName, string tempPassword)
 {
 var subject = "Recuperación de contraseña";
 var body = $@"<p>Hola {Escape(userName)},</p>
<p>Generamos una contraseña temporal para tu cuenta:</p>
<p><strong>{Escape(tempPassword)}</strong></p>
<p>Inicia sesión y cambia la contraseña desde el menú.</p>
<p>Saludos,<br/>AgroMinds</p>";
 return SendEmailAsync(toEmail, subject, body);
 }

 private static int ParseInt(string s, int def)
 {
 int v; return int.TryParse(s, out v) ? v : def;
 }
 private static bool ParseBool(string s, bool def)
 {
 bool v; return bool.TryParse(s, out v) ? v : def;
 }
 private static string Escape(string s)
 {
 return (s ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;");
 }
 }
}
