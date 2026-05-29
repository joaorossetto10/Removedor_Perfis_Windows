# Removedor de Perfis Windows

Aplicativo interno de TI para listar e remover perfis locais de usuários em computadores Windows remotos.

O objetivo correto do projeto é remover perfis locais do Windows, não contas de usuários.

## Estado atual

Esta etapa permite informar um computador remoto e listar perfis locais do Windows usando WMI/Win32_UserProfile com o usuário atual ou com uma credencial administrativa informada apenas para a operação atual.

A listagem classifica perfis comuns como disponíveis para análise e bloqueia perfis especiais, em uso, protegidos, de sistema/serviço ou com caminho fora de `C:\Users`.

Também é detectado o usuário interativo logado no computador remoto quando `Win32_ComputerSystem.UserName` está disponível. Perfis com o mesmo nome desse usuário são bloqueados, mesmo que o WMI indique que não estão carregados.

A opção `Calcular tamanho dos perfis` usa acesso somente leitura ao compartilhamento administrativo `C$` do computador remoto para calcular o tamanho das pastas em `C:\Users`. Essa opção fica desmarcada por padrão, roda em segundo plano, calcula um perfil por vez e pode ser cancelada pelo botão `Cancelar cálculo`.

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

## Credencial administrativa opcional

Marque `Usar credencial administrativa` antes de clicar em `Carregar perfis` para informar usuário e senha em uma janela temporária.

Formatos aceitos para o usuário:

- `DOMINIO\usuario`;
- `computador\usuario`;
- `usuario@dominio.local`.

## Segurança

Esta etapa não remove perfis, não apaga arquivos e não exclui contas.

Senhas não são salvas em arquivo, configuração, README ou log. A credencial administrativa opcional é usada apenas durante a consulta atual.

Perfis bloqueados aparecem na grade, mas não podem ser selecionados.

Perfis com nomes duplicados em `C:\Users` recebem observação na grade e aviso no log para análise cuidadosa.

O cálculo de tamanho não altera arquivos. Ele ignora perfis bloqueados, junctions e links simbólicos, usa timeout por perfil e continua a listagem se algum caminho falhar. Os resultados podem aparecer como `Sem acesso ao C$`, `Tempo excedido`, `Cancelado` ou `Erro ao calcular`.

As futuras ações de remoção devem usar mecanismos administrativos legítimos, como `Win32_UserProfile`, e exigir confirmação explícita antes de qualquer operação destrutiva.
