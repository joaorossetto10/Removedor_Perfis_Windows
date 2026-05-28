using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;

namespace RemovedorPerfisWindows.Helpers;

public static class WmiErrorHelper
{
    private const int AccessDenied = unchecked((int)0x80070005);
    private const int RpcServerUnavailable = unchecked((int)0x800706BA);
    private const int NetworkPathNotFound = unchecked((int)0x80070035);
    private const int ServerUnavailable = unchecked((int)0x80041013);

    public static string GetFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "Acesso negado. Execute com uma conta autorizada no computador remoto.",
            ManagementException managementException => GetManagementMessage(managementException),
            COMException comException => GetComMessage(comException),
            Win32Exception win32Exception => GetWin32Message(win32Exception),
            _ => "Falha ao consultar os perfis. Verifique o nome do computador, rede, permissões e WMI/RPC."
        };
    }

    private static string GetManagementMessage(ManagementException exception)
    {
        return exception.ErrorCode switch
        {
            ManagementStatus.AccessDenied => "Acesso negado ao WMI no computador remoto.",
            ManagementStatus.InvalidNamespace => "Namespace WMI root\\cimv2 indisponível no computador remoto.",
            ManagementStatus.NotFound => "Classe Win32_UserProfile não encontrada no computador remoto.",
            _ when exception.HResult == ServerUnavailable => "Serviço WMI indisponível no computador remoto.",
            _ => $"Erro WMI: {exception.Message}"
        };
    }

    private static string GetComMessage(COMException exception)
    {
        return exception.HResult switch
        {
            AccessDenied => "Acesso negado ao computador remoto.",
            RpcServerUnavailable => "RPC indisponível. Verifique se o computador está ligado, acessível e com WMI/RPC permitido.",
            NetworkPathNotFound => "Computador inacessível ou caminho de rede não encontrado.",
            ServerUnavailable => "Serviço WMI indisponível no computador remoto.",
            _ => $"Erro RPC/WMI: {exception.Message}"
        };
    }

    private static string GetWin32Message(Win32Exception exception)
    {
        return exception.NativeErrorCode switch
        {
            5 => "Acesso negado ao computador remoto.",
            53 => "Computador inacessível ou caminho de rede não encontrado.",
            1722 => "RPC indisponível no computador remoto.",
            _ => $"Erro de comunicação: {exception.Message}"
        };
    }
}
