# Removedor de Perfis Windows

Aplicativo interno de TI para listar e remover perfis locais de usuários em computadores Windows remotos.

O objetivo correto do projeto é remover perfis locais do Windows, não contas de usuários.

## Estado atual

Esta etapa permite informar um computador remoto e listar perfis locais do Windows usando WMI/Win32_UserProfile com as credenciais do usuário atual.

Ainda não há implementação de:

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

Esta etapa não remove perfis, não apaga arquivos, não exclui contas e não solicita credenciais.

As futuras ações de remoção devem usar mecanismos administrativos legítimos, como `Win32_UserProfile`, e exigir confirmação explícita antes de qualquer operação destrutiva.
