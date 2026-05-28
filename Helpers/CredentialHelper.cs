using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Helpers;

public static class CredentialHelper
{
    public static bool TryCreate(string userName, string password, out AdminCredentialInfo? credential, out string validationMessage)
    {
        credential = null;
        validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(userName))
        {
            validationMessage = "Informe o usuário administrativo.";
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            validationMessage = "Informe a senha da credencial administrativa.";
            return false;
        }

        credential = new AdminCredentialInfo(userName.Trim(), password);
        return true;
    }
}
