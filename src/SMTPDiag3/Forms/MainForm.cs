using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Reflection;
using SMTPDiag3.Common;
using SMTPDiag3.Net;
using SmtpException = SMTPDiag3.Net.SmtpException;

namespace SMTPDiag3.Forms;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class MainForm : Form
{
    private long _elapsedMs;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        authenticationComboBox.SelectedIndex = 2;
        var version = Assembly.GetExecutingAssembly()
                          .GetName()
                          .Version
                      ?? new Version(1, 0, 0);
        Text += $" - {version.Major}.{version.Minor} build {version.Build}";
    }

    private async void SendButton_Click(object sender, EventArgs e)
    {
        CancellationToken cancellationToken = default;
        try
        {
            Reset();
            ValidateInputs();
            CheckSpf();
            await TestSmtp(cancellationToken);

            resultsTextBox.Text += $"\r\nTotal Execution Time of {_elapsedMs}ms";
        }
        catch (SmtpException ex)
        {
            resultsTextBox.Text += $"\r\nSMTP Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            resultsTextBox.Text += $"\r\nApplication Error: {ex.Message}";
        }
    }

    private void Reset()
    {
        resultsTextBox.Text = "";
        _elapsedMs = 0;
    }

    private void ValidateInputs()
    {
        hostTextBox.Text = hostTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(hostTextBox.Text))
            throw new ArgumentException("Host is required.");
        
        portTextBox.Text = portTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(portTextBox.Text))
            throw new ArgumentException("Port is required.");
        if (!short.TryParse(portTextBox.Text, out _))
            throw new ArgumentException("Port is not a valid number.");
        
        usernameTextBox.Text = usernameTextBox.Text.Trim();
        
        passwordTextBox.Text = passwordTextBox.Text.Trim();
        
        fromAddressTextBox.Text = fromAddressTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(fromAddressTextBox.Text))
            throw new ArgumentException("From Address is required.");
        
        toAddressTextBox.Text = toAddressTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(toAddressTextBox.Text))
            throw new ArgumentException("To Address is required.");
    }

    private void CheckSpf()
    {
        var senderAddress = new MailAddress(fromAddressTextBox.Text);
        var senderDomainName = senderAddress.Host;
        
        var spfRecord = Dns.GetSpfRecord(senderDomainName);
        
        if (!string.IsNullOrWhiteSpace(spfRecord))
        {
            resultsTextBox.Text += $"          [Retrieved SPF record for {senderDomainName}]\r\n";
            resultsTextBox.Text += $"          {spfRecord}\r\n";
            resultsTextBox.Text += "\r\n";
        }
        else
        {
            resultsTextBox.Text += $"          [No SPF record found for {senderDomainName}]\r\n";
            resultsTextBox.Text += "\r\n";
        }
    }

    private async Task TestSmtp(CancellationToken cancellationToken)
    {
        var fqdn = Dns.GetFqdn();

        var host = hostTextBox.Text;
        var port = Convert.ToInt16(portTextBox.Text);
        var authenticationMode = (AuthenticationMode)authenticationComboBox.SelectedIndex;
        var username = usernameTextBox.Text;
        var password = passwordTextBox.Text;
        var from = fromAddressTextBox.Text;
        var to = new List<string> { toAddressTextBox.Text };
        var subject = $"SMTP test via {host}";
        var body = $"This is an SMTP test conducted through the server {host} on port {port} from {fqdn}.";

        await using var smtp = new Smtp(Smtp_Message)
        {
            Host = host,
            Port = port,
            AuthenticationMode = authenticationMode,
            Username = username,
            Password = password,
            From = from,
            To = to,
            Subject = subject,
            Body = body,
        };

        await smtp.SendAsync(cancellationToken);
    }

    private void Output(string text)
    {
        if (resultsTextBox.InvokeRequired)
        {
            var d = new OutputCallback(Output);
            Invoke(d, [text]);
        }
        else
        {
            resultsTextBox.Text += text;
        }
    }

    private void Smtp_Message(long elapsedMs, string message)
    {
        var ms = elapsedMs - _elapsedMs;
        
        var lines = message.SplitLines();
        for (var index = 0; index < lines.Length - 1; index++)
        {
            var gutter = index == 0 ? $"{ms,5}ms - " : "          ";
            var line = $"{gutter}{lines[index]}\r\n";
            Output(line);
        }
        
        _elapsedMs = elapsedMs;
    }

    private delegate void OutputCallback(string text);
}
