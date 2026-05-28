# Removedor de Perfis Windows

Aplicativo interno de TI para listar e remover perfis locais de usuários em computadores Windows remotos.

O objetivo correto do projeto é remover perfis locais do Windows, não contas de usuários.

## Estado atual

Esta etapa inicial cria apenas a estrutura base do aplicativo Windows Forms em C#/.NET 8.

Ainda não há implementação de:

- conexão WMI/CIM;
- listagem remota de perfis;
- remoção de perfis locais.

## Requisitos

- Windows 10 ou Windows 11;
- .NET SDK compatível com projetos `net8.0-windows`;
- Visual Studio Code ou Visual Studio.

## Como compilar

No diretório do projeto, execute:

```powershell
dotnet build
```

## Estrutura inicial

- `Forms`: telas Windows Forms;
- `Models`: modelos de dados;
- `Services`: regras de negócio e serviços;
- `Helpers`: funções auxiliares futuras.

## Segurança

As futuras ações de remoção devem usar mecanismos administrativos legítimos, como `Win32_UserProfile`, e exigir confirmação explícita antes de qualquer operação destrutiva.
