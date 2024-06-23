using SMTPDiag3.Forms;

namespace SMTPDiag3;

internal static class Program
{
    /// <summary>The main entry point for the application.</summary>
    [STAThread]
    internal static void Main()
    {
        ApplicationConfiguration.Initialize();

        var mainForm = new MainForm();
        Application.Run(mainForm);
    }
}
