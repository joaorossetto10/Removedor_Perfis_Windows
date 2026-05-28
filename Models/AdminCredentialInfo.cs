namespace RemovedorPerfisWindows.Models;

public sealed class AdminCredentialInfo
{
    public AdminCredentialInfo(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    public string UserName { get; private set; }
    public string Password { get; private set; }

    public bool HasCredential => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrEmpty(Password);

    public void Clear()
    {
        UserName = string.Empty;
        Password = string.Empty;
    }
}
