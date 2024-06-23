using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SMTPDiag3.Common;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable SYSLIB0039 // SslProtocols.Tls and SslProtocols.Tls11 are obsolete

namespace SMTPDiag3.Net;

[MustDisposeResource]
public sealed partial class Smtp : IAsyncDisposable
{
    public string Host { get; init; } = "";
    public short Port { get; init; } = 25;
    public AuthenticationMode AuthenticationMode { get; init; } = AuthenticationMode.None;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string From { get; init; } = "";
    public List<string> To { get; init; } = [];
    public List<string> Cc { get; init; } = [];
    public List<string> Bcc { get; init; } = [];
    public string Subject { get; init; } = "";
    public string Body { get; init; } = "";
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30); 
    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(30); 

    private const string Ready = "220";
    private const string ClosingChannel = "221";
    private const string Authenticated = "235";
    private const string Ok = "250";
    private const string ServerChallenge = "334";
    private const string StartInput = "354";
    private readonly MessageHandler _messageHandler;
    private readonly TcpClient _client;
    private NetworkStream? _networkStream;
    private SslStream? _sslStream;
    private string _fqdn;
    private bool _ehloSupport;
    private bool _plainSupport;
    private bool _loginSupport;
    private bool _cramMd5Support;
    private bool _startTls;
    private Stopwatch _stopwatch = new();

    public Smtp(MessageHandler messageHandler)
    {
        _messageHandler = messageHandler;
        _client = new TcpClient();
        _fqdn = "";
    }

    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        Initialize();
        
        cts.CancelAfter(ConnectionTimeout);
        await Connect(cts.Token);

        cts.CancelAfter(CommandTimeout);
        await ExtendedHello(cts.Token);
        if (_startTls)
        {
            cts.CancelAfter(ConnectionTimeout);
            await StartTls(cts.Token);

            cts.CancelAfter(CommandTimeout);
            await ExtendedHello(cts.Token);
        }
        
        cts.CancelAfter(CommandTimeout);
        await Authenticate(cts.Token);

        cts.CancelAfter(CommandTimeout);
        await MailFrom(cts.Token);

        cts.CancelAfter(CommandTimeout);
        await RcptTo(To, cancellationToken);

        cts.CancelAfter(CommandTimeout);
        await RcptTo(Cc, cancellationToken);
        
        cts.CancelAfter(CommandTimeout);
        await RcptTo(Bcc, cancellationToken);
        
        cts.CancelAfter(CommandTimeout);
        await Data(cts.Token);
        
        cts.CancelAfter(CommandTimeout);
        await Quit(cts.Token);
    }

    private void Initialize()
    {
        _stopwatch = Stopwatch.StartNew();

        _fqdn = Dns.GetFqdn();
        
        var hostnameMessage = $"[Machine's hostname resolved to {_fqdn}]\r\n";
        _messageHandler(_stopwatch.ElapsedMilliseconds, hostnameMessage);

        var ipAddresses = Dns.GetIpAddresses(Host);
        
        var ipAddressesMessage = $"[{Host} resolved to {ipAddresses}]\r\n";
        _messageHandler(_stopwatch.ElapsedMilliseconds, ipAddressesMessage);
    }

    private async Task Connect(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(Host, Port, cancellationToken);
        
        var connectedMessage = $"[Connected to {Host}:{Port}]\r\n";
        _messageHandler(_stopwatch.ElapsedMilliseconds, connectedMessage);

        _networkStream = _client.GetStream();
        
        var response = await Response(cancellationToken);
        if (response.Code != Ready)
            throw new SmtpException(response.Message);
    }

    private async Task ExtendedHello(CancellationToken cancellationToken)
    {
        var command = $"EHLO {_fqdn}\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != Ok)
        {
            await BasicHello(cancellationToken);
            return;
        }

        _ehloSupport = true;
        var lines = response.Message.ToUpper().SplitLines();
        foreach (var line in lines)
        {
            if (line.Length < 4)
                continue;

            var code = line[..3];
            if (code != Ok)
                continue;

            var data = line[4..];
            if (data.StartsWith("AUTH"))
            {
                if (data.Contains("PLAIN"))
                    _plainSupport = true;

                if (data.Contains("LOGIN"))
                    _loginSupport = true;

                if (data.Contains("CRAM-MD5"))
                    _cramMd5Support = true;
            }

            if (data == "STARTTLS")
            {
                _startTls = true;
            }
        }
    }

    private async Task BasicHello(CancellationToken cancellationToken)
    {
        var command = $"HELO {_fqdn}\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != Ok)
            throw new SmtpException(response.Message);
    }

    private async Task StartTls(CancellationToken cancellationToken)
    {
        const string command = "STARTTLS\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != Ready)
            throw new SmtpException(response.Message);

        if (_networkStream is null)
            throw new InvalidOperationException("Stream has not been initialized.");
        
        var sslStream = new SslStream(_networkStream, true, ValidateServerCertificate, null);
        await sslStream.AuthenticateAsClientAsync(_fqdn);
        _sslStream = sslStream;

        var protocol = _sslStream.SslProtocol switch
        {
            SslProtocols.Tls => "TLS",
            SslProtocols.Tls11 => "TLS 1.1",
            SslProtocols.Tls12 => "TLS 1.2",
            SslProtocols.Tls13 => "TLS 1.3",
            SslProtocols.Ssl2 => "SSL 2.0",
            SslProtocols.Ssl3 => "SSL 3.0",
            _ => "",
        };
        var connectionUpgradedMessage = $"[Upgraded connection using {protocol}]\r\n";
        _messageHandler(_stopwatch.ElapsedMilliseconds, connectionUpgradedMessage);
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    
    private async Task Authenticate(CancellationToken cancellationToken)
    {
        if (Username.Length > 0 && Password.Length > 0)
        {
            if (AuthenticationMode is AuthenticationMode.Plain or AuthenticationMode.ForcePlain)
            {
                if (AuthenticationMode == AuthenticationMode.Plain && _ehloSupport && !_plainSupport)
                    throw new SmtpException("PLAIN authentication is not announced as supported by the SMTP server.");

                await AuthPlain(cancellationToken);
            }

            if (AuthenticationMode is AuthenticationMode.Login or AuthenticationMode.ForceLogin)
            {
                if (AuthenticationMode == AuthenticationMode.Login && _ehloSupport && !_loginSupport)
                    throw new SmtpException("LOGIN authentication is not announced as supported by the SMTP server.");

                await AuthLogin(cancellationToken);
            }

            if (AuthenticationMode is AuthenticationMode.CramMd5 or AuthenticationMode.ForceCramMd5)
            {
                if (AuthenticationMode == AuthenticationMode.CramMd5 && _ehloSupport && !_cramMd5Support)
                    throw new SmtpException("CRAM-MD5 authentication is not announced as supported by the SMTP server.");

                await AuthCramMd5(cancellationToken);
            }
        }
    }

    private async Task AuthPlain(CancellationToken cancellationToken)
    {
        const string command = "AUTH PLAIN\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != ServerChallenge)
            throw new SmtpException(response.Message);
        
        var plainCredentials = $"{Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}{Password}"))}\r\n";
        await Write(plainCredentials, cancellationToken);
        
        response = await Response(cancellationToken);
        if (response.Code != Authenticated)
            throw new SmtpException(response.Message);
    }
    
    private async Task AuthLogin(CancellationToken cancellationToken)
    {
        const string command = "AUTH LOGIN\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != ServerChallenge)
            throw new SmtpException(response.Message);
        
        var username = $"{Convert.ToBase64String(Encoding.ASCII.GetBytes(Username))}\r\n";
        await Write(username, cancellationToken);
        
        response = await Response(cancellationToken);
        if (response.Code != ServerChallenge)
            throw new SmtpException(response.Message);
        
        var password = $"{Convert.ToBase64String(Encoding.ASCII.GetBytes(Password))}\r\n";
        await Write(password, cancellationToken);
        
        response = await Response(cancellationToken);
        if (response.Code != Authenticated)
            throw new SmtpException(response.Message);
    }
    
    private async Task AuthCramMd5(CancellationToken cancellationToken)
    {
        const string command = "AUTH CRAM-MD5\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != ServerChallenge)
            throw new SmtpException(response.Message);

        var challenge = response.Data;
        var hash = GenerateMd5Hash(Password, challenge);

        var challengeResponse = $"{Username} {hash}";
        var challengeResponseCommand = $"{Convert.ToBase64String(Encoding.ASCII.GetBytes(challengeResponse))}\r\n";
        await Write(challengeResponseCommand, cancellationToken);

        response = await Response(cancellationToken);
        if (response.Code != Authenticated)
            throw new SmtpException(response.Message);
    }

    private static string GenerateMd5Hash(string password, string challenge)
    {
        var passwordBytes = Encoding.ASCII.GetBytes(password);
        var challengeBytes = Convert.FromBase64String(challenge);
        var hashBytes = HMACMD5.HashData(passwordBytes, challengeBytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        return hash;
    }

    private async Task MailFrom(CancellationToken cancellationToken)
    {
        var command = $"MAIL FROM:<{From}>\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != Ok)
            throw new SmtpException(response.Message);
    }

    private async Task RcptTo(List<string> recipients, CancellationToken cancellationToken)
    {
        foreach (var address in recipients)
        {
            var command = $"RCPT TO:<{address}>\r\n";
            await Write(command, cancellationToken);
            
            var response = await Response(cancellationToken);
            if (response.Code != Ok)
                throw new SmtpException(response.Message);
        }
    }

    private async Task Data(CancellationToken cancellationToken)
    {
        const string command = "DATA\r\n";
        await Write(command, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (response.Code != StartInput)
            throw new SmtpException(response.Message);

        var builder = new StringBuilder();
        builder.Append($"From: {From}\r\n");
        foreach (var address in To)
            builder.Append($"To: {address}\r\n");
        foreach (var address in Cc)
            builder.Append($"Cc: {address}\r\n");
        builder.Append($"Subject: {Subject}\r\n");
        builder.Append("\r\n");
        builder.Append(Body);
        builder.Append("\r\n");
        builder.Append(".\r\n");
        var message = builder.ToString();
        
        await Write(message, cancellationToken);
        response = await Response(cancellationToken);
        if (response.Code != Ok)
            throw new SmtpException(response.Message);
    }

    private async Task Quit(CancellationToken cancellationToken)
    {
        const string message = "QUIT\r\n";
        await Write(message, cancellationToken);
        
        var response = await Response(cancellationToken);
        if (!response.Message.Contains(ClosingChannel, StringComparison.Ordinal))
            throw new SmtpException(response.Message);
    }

    private async Task Write(string command, CancellationToken cancellationToken)
    {
        if (_networkStream is null && _sslStream is null)
            throw new InvalidOperationException("Stream has not been initialized.");

        var buffer = Encoding.ASCII.GetBytes(command);
        var memory = new ReadOnlyMemory<byte>(buffer);

        if (_sslStream is null)
            await _networkStream!.WriteAsync(memory, cancellationToken);
        else
            await _sslStream.WriteAsync(memory, cancellationToken);
        
        _messageHandler(_stopwatch.ElapsedMilliseconds, command);
    }

    private async Task<SmtpResponse> Response(CancellationToken cancellationToken)
    {
        if (_networkStream is null && _sslStream is null)
            throw new InvalidOperationException("Stream has not been initialized.");
        
        var buffer = new byte[1024];
        var memory = new Memory<byte>(buffer);
        
        int count;
        if (_sslStream is null)
            count = await _networkStream!.ReadAsync(memory, cancellationToken);
        else
            count = await _sslStream.ReadAsync(memory, cancellationToken);
        if (count == 0)
            return new SmtpResponse();
        
        var message = Encoding.ASCII.GetString(buffer, 0, count);
        var response = new SmtpResponse(message);
        
        _messageHandler(_stopwatch.ElapsedMilliseconds, message);
        
        return response;
    }

    public async ValueTask DisposeAsync()
    {
        if (_sslStream is not null)
            await _sslStream.DisposeAsync();
        if (_networkStream is not null)
            await _networkStream.DisposeAsync();
        _client.Dispose();
    }

    private sealed partial class SmtpResponse
    {
        public string Message { get; }

        public string Code
        {
            get
            {
                if (Message.Length > 3 && SmtpCodeRegex().IsMatch(Message))
                {
                    return Message[..3];
                }
                
                return "";
            }
        }

        public string Data => Message.Length > 4 ? Message[4..] : "";

        public SmtpResponse()
        {
            Message = "";
        }

        public SmtpResponse(string message)
        {
            Message = message;
        }

        [GeneratedRegex(@"([0-9]{3})\s")]
        private partial Regex SmtpCodeRegex();
    }
}
