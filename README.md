# Removedor de Perfis Windows

Aplicativo interno de TI para listar e remover perfis locais de usuários em computadores Windows remotos.

O objetivo correto do projeto é remover perfis locais do Windows, não contas de usuários.

## Estado atual

Esta etapa permite informar um computador remoto e listar perfis locais do Windows usando WMI/Win32_UserProfile com o usuário atual ou com uma credencial administrativa informada apenas para a operação atual.

A listagem classifica perfis comuns como disponíveis para análise e bloqueia perfis especiais, em uso, protegidos, de sistema/serviço ou com caminho fora de `C:\Users`.

Também é detectado o usuário interativo logado no computador remoto quando `Win32_ComputerSystem.UserName` está disponível. Perfis com o mesmo nome desse usuário são bloqueados, mesmo que o WMI indique que não estão carregados.

A opção `Calcular tamanho dos perfis` usa acesso somente leitura ao compartilhamento administrativo `C$` do computador remoto para calcular o tamanho das pastas em `C:\Users`. Essa opção fica desmarcada por padrão, roda em segundo plano, calcula um perfil por vez e pode ser cancelada pelo botão `Cancelar cálculo`.

A opção `Mostrar perfis de sistema/serviço` exibe perfis técnicos que ficam ocultos por padrão para manter a listagem mais limpa.

## Modo simples e configurações avançadas

O modo simples mostra apenas as informações necessárias para o uso diário: seleção, perfil, último uso, uso atual, tamanho e status. Esse é o modo recomendado para estagiários e técnicos novos.

As configurações avançadas exibem opções e detalhes técnicos, como credencial administrativa, perfis de sistema/serviço, SID, caminho completo, resultado da remoção e observações.

Perfis bloqueados continuam sem seleção e não podem ser removidos, mesmo quando os detalhes técnicos estão visíveis.

Ainda não há implementação de:

- exclusão de contas de usuário locais ou do domínio/AD.

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

Esta etapa permite remover apenas perfis locais selecionados e confirmados. Ela não exclui contas locais, contas do domínio ou objetos do Active Directory.

Senhas não são salvas em arquivo, configuração, README ou log. A credencial administrativa opcional é usada apenas durante a consulta atual.

Perfis bloqueados aparecem na grade, mas não podem ser selecionados.

Perfis com nomes duplicados em `C:\Users` recebem observação na grade e aviso no log para análise cuidadosa.

Perfis de sistema/serviço ficam ocultos por padrão. Mesmo quando exibidos, permanecem bloqueados, não entram no cálculo de tamanho e não podem ser removidos.

O cálculo de tamanho não altera arquivos. Ele ignora perfis bloqueados, junctions e links simbólicos, usa timeout por perfil e continua a listagem se algum caminho falhar. Os resultados podem aparecer como `Sem acesso ao C$`, `Tempo excedido`, `Cancelado` ou `Erro ao calcular`.

Qualquer remoção usa mecanismos administrativos legítimos, como `Win32_UserProfile`, e exige confirmação explícita antes da operação.

## Remoção segura de perfis

A remoção usa `Win32_UserProfile` pelo SID do perfil selecionado. A ferramenta não exclui contas locais, contas do domínio ou objetos do Active Directory.

Antes de remover, a aplicação mostra os perfis selecionados, seus caminhos locais e exige digitar o nome do computador remoto para confirmação. Perfis em uso, protegidos, especiais, de sistema/serviço, fora de `C:\Users` ou do usuário atualmente logado são bloqueados.

A ferramenta não apaga pastas manualmente antes da remoção via WMI. Após chamar a remoção, o SID é consultado novamente no WMI para confirmar o resultado.
