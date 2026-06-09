using System.Runtime.InteropServices;

namespace RemovedorPerfisWindows.Helpers;

internal static class AppIdentityHelper
{
    private const string AppUserModelId = "JoaoRossetto.RPW.RemovedorPerfisWindows";

    public static void Apply()
    {
        try
        {
            SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
        }
        catch
        {
            // A identidade visual continua funcionando pelo ícone do executável.
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);
}
