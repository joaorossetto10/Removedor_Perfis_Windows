# Instruções do Projeto - Removedor de Perfis Windows

## Objetivo
Este projeto é um aplicativo Windows Forms em C#/.NET 8 para uso interno de TI.

A ferramenta deve listar e remover perfis locais de usuários em computadores Windows remotos usando mecanismos administrativos legítimos, como WMI/CIM/Win32_UserProfile.

## Regras obrigatórias
- Não excluir contas do Active Directory.
- Não excluir usuários locais diretamente.
- Não burlar UAC, antivírus, firewall, EDR ou políticas da empresa.
- Não armazenar senhas em texto puro.
- Não executar ações destrutivas sem confirmação explícita.
- Não remover perfis em uso.
- Não remover perfis especiais do Windows.
- Não apagar pastas manualmente antes de tentar remoção via Win32_UserProfile.

## Tecnologia
- C#
- Windows Forms
- .NET 8
- Windows 10/11
- Projeto editável no VS Code

## Organização
Separar responsabilidades:
- Forms: interface
- Services: regras de negócio
- Models: modelos de dados
- Helpers: funções auxiliares

## Padrão de código
- Código limpo e legível.
- Textos da interface em português do Brasil.
- Classes, métodos e propriedades podem ficar em inglês.
- Tratamento de erros amigável.
- Logs claros para o usuário de TI.
- Manter o projeto compilável a cada etapa.

## Segurança
Qualquer ação de remoção deve ter:
1. Lista dos perfis selecionados.
2. Confirmação visual.
3. Digitação do nome do computador remoto.
4. Log da operação.
5. Resultado por perfil.
