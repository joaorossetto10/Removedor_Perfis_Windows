using RemovedorPerfisWindows.Forms;
using RemovedorPerfisWindows.Helpers;

namespace RemovedorPerfisWindows;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        AppIdentityHelper.Apply();
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}
