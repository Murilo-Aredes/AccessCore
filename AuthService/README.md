# AuthService

Serviço centralizado de autenticação, autorização, sessões e auditoria em ASP.NET Core .NET 10, C#, Clean Architecture, Entity Framework Core e SQL Server.

Todo o domínio de negócio usa nomenclatura em português: `Usuario`, `SessaoUsuario`, `TokenAtualizacao`, `Aplicacao`, `Perfil`, `Permissao`, `EscopoUsuario`, `Auditoria`, `EventoSeguranca` e demais conceitos.

## Arquitetura

A solução segue Clean Architecture com CQRS leve:

```text
AuthService.Api
  Controllers, middleware, autenticação JWT, Swagger, ProblemDetails, CORS, rate limiting

AuthService.Application
  Casos de uso, comandos, consultas, validações, contratos e regras de aplicação

AuthService.Domain
  Entidades, enums e regras de domínio sem dependência de HttpContext

AuthService.Infrastructure
  EF Core, SQL Server, JWT, PasswordHasher, auditoria automática, outbox e notificações

AuthService.UnitTests
  Testes de login, sessões, refresh token, autorização e auditoria
```

## Configurar SQL Server

Edite `AuthService.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AuthService": "Server=(localdb)\\mssqllocaldb;Database=AuthService;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Em produção, configure também:

- `Jwt:ChaveSecreta` com uma chave forte.
- `Cors:OrigensPermitidas` com as origens reais.
- HTTPS/HSTS habilitados.
- SQL Server com usuário e permissões próprias para a aplicação.

O acesso ao SQL Server está configurado com `EnableRetryOnFailure`, que ajuda em falhas transitórias de conexão. Ele não substitui a criação do banco nem corrige falta de permissão.

## Restaurar, compilar e testar

Em um ambiente normal:

```powershell
dotnet restore AuthService.slnx
dotnet build AuthService.slnx
dotnet test AuthService.slnx
```

Neste workspace, caso o NuGet global esteja bloqueado, use a configuração local:

```powershell
$env:APPDATA = (Resolve-Path .\.appdata).Path
$env:LOCALAPPDATA = (Resolve-Path .\.appdata).Path
$env:DOTNET_CLI_HOME = (Resolve-Path .).Path
$env:NUGET_PACKAGES = "$env:USERPROFILE\.nuget\packages"

dotnet restore AuthService.slnx --configfile NuGet.Config --ignore-failed-sources
dotnet build AuthService.slnx --no-restore
dotnet test AuthService.slnx --no-build --no-restore
```

## Migrations

A migration inicial está em:

```text
AuthService.Infrastructure/Persistence/Migrations
```

Para criar ou atualizar o banco:

```powershell
dotnet ef database update `
  --project AuthService.Infrastructure\AuthService.Infrastructure.csproj `
  --startup-project AuthService.Api\AuthService.Api.csproj
```

Se aparecer erro como `Cannot open database "AuthService" requested by the login`, o banco ainda não existe ou o usuário da connection string não tem permissão nele. Execute o comando acima para criar/aplicar as tabelas, ou ajuste `ConnectionStrings:AuthService` para apontar para uma instância SQL Server onde o usuário tenha acesso.

Durante um teste local, se você quiser subir a API antes de preparar o banco, pode desabilitar temporariamente o processador de outbox:

```json
{
  "Outbox": {
    "Habilitado": false
  }
}
```

Para criar uma nova migration:

```powershell
dotnet ef migrations add NomeDaMigration `
  --project AuthService.Infrastructure\AuthService.Infrastructure.csproj `
  --startup-project AuthService.Api\AuthService.Api.csproj `
  --output-dir Persistence\Migrations
```

## Executar a aplicação

```powershell
dotnet run --project AuthService.Api\AuthService.Api.csproj
```

Swagger:

```text
https://localhost:{porta}/swagger
```

O Swagger está configurado com Bearer JWT pelo botão `Authorize`.

## Cadastrar usuário

Para cadastrar um usuário que já consiga autenticar:

```text
POST /api/usuarios
```

Exemplo:

```json
{
  "nome": "Murilo",
  "email": "murilo@example.com",
  "senha": "senha-segura-123",
  "enviarConfirmacaoEmail": true
}
```

O cadastro:

- normaliza o e-mail em `EmailNormalizado`
- impede e-mail duplicado
- armazena somente `SenhaHash`
- cria `TokenConfirmacaoEmail` somente como hash
- grava uma `MensagemOutbox` para envio da confirmação

## Criar usuário inicial

O projeto não armazena senha em texto puro. Para criar um usuário inicial:

Opção recomendada: use `POST /api/usuarios`.

Para criar manualmente no banco:

1. Gere o hash usando `PasswordHasher<Usuario>`.
2. Insira o usuário na tabela `Usuario` com `EmailNormalizado` em maiúsculas.
3. Vincule o usuário ao perfil administrador com `UsuarioPerfil`, se ele precisar de permissões administrativas.

O seed já cria:

- `Aplicacao`: `ADMINISTRACAO`
- `Perfil`: `ADMINISTRADOR`
- permissões administrativas
- vínculos `PerfilPermissao`

## Autenticação

Endpoint principal:

```text
POST /api/autenticacao/login
```

Fluxo implementado:

- valida aplicação ativa
- localiza usuário por `EmailNormalizado`
- valida senha com `PasswordHasher<Usuario>`
- bloqueia por tentativas inválidas conforme configuração
- registra `TentativaLogin`
- identifica dispositivo por `IdentificadorDispositivo`
- cria `SessaoUsuario`
- registra `EventoSeguranca`

Se o dispositivo for confiável:

- a sessão nasce confirmada
- gera `TokenAcesso`
- gera `TokenAtualizacao`

Se o dispositivo não for confiável:

- a sessão nasce pendente
- gera apenas `TokenSessaoPendente`
- salva `ConfirmacaoSessao` com hash do token
- cria mensagem na `MensagemOutbox`

## JWT

Claims mínimas:

- `sub`: `UsuarioId`
- `sid`: `SessaoUsuarioId`
- `application_id`: `AplicacaoId`
- `jti`: identificador único do token

O `TokenAcesso` não é salvo no banco.

O `TokenSessaoPendente` inclui:

- `token_type = sessao_pendente`

## TokenAtualizacao

O refresh token é representado por `TokenAtualizacao`.

Regras:

- o valor original nunca é armazenado
- somente `TokenHash` é persistido
- há rotação a cada uso
- o token usado recebe `DataUtilizacao`
- o novo token é vinculado por `TokenSubstitutoId`
- reutilização revoga a sessão e todos os tokens da sessão
- reutilização registra `EventoSeguranca.ReutilizacaoTokenDetectada`

Endpoint:

```text
POST /api/autenticacao/atualizar-token
```

## SessaoUsuario e dispositivo confiável

`SessaoUsuario` permite múltiplas sessões simultâneas por usuário.

Situações:

- `Pendente`
- `Confirmada`
- `Revogada`
- `Expirada`

Endpoints:

```text
POST /api/sessoes/{id}/confirmar
POST /api/sessoes/{id}/rejeitar
DELETE /api/sessoes/{id}
GET /api/usuario-atual/sessoes
DELETE /api/usuario-atual/sessoes
```

Ao confirmar uma sessão, o usuário pode marcar o dispositivo como confiável.

## Perfil, Permissao e EscopoUsuario

O projeto usa `Perfil`, não `Role`.

A avaliação de permissão segue esta ordem:

1. negação explícita do usuário
2. permissão explícita do usuário
3. permissão obtida pelo perfil
4. negar por padrão

Endpoint de acessos:

```text
GET /api/usuario-atual/acessos?aplicacaoId={id}
```

Retorna usuário, aplicação, perfis, permissões efetivas e escopos.

`EscopoUsuario` suporta `Grupo`, `Empresa` e `Unidade` sem criar tabelas específicas como `UsuarioEmpresa`.

## Recuperação de senha e confirmação de e-mail

Endpoints:

```text
POST /api/autenticacao/senha/esqueci
POST /api/autenticacao/senha/redefinir
POST /api/autenticacao/email/confirmar
POST /api/autenticacao/email/reenviar-confirmacao
```

Recuperação de senha não permite enumeração de usuários e sempre retorna:

```text
Se o usuário informado existir, as instruções serão enviadas.
```

Tokens de recuperação e confirmação são armazenados somente como hash.

## Auditoria

Auditoria automática via `SaveChangesInterceptor`.

Inclui:

- inclusão
- alteração
- exclusão
- dados antes/depois
- campos alterados
- `IdentificadorCorrelacao`
- usuário, sessão, aplicação, IP e dispositivo quando disponíveis

Nunca audita propriedades sensíveis como:

- `Senha`
- `SenhaHash`
- `TokenAcesso`
- `TokenAtualizacao`
- `TokenHash`
- `ChavesSecretas`
- `ApiKey`
- `ConnectionString`

Endpoints:

```text
GET /api/auditorias
GET /api/auditorias/{entidade}/{id}
```

## Correlation ID

Middleware próprio:

- aceita `X-Correlation-ID`
- gera `Guid` caso não enviado
- devolve o valor no response
- expõe `IContextoCorrelacao.IdentificadorCorrelacao`

## Outbox

O projeto implementa `MensagemOutbox` e `OutboxBackgroundService`.

Ao criar uma sessão pendente, `SessaoUsuario`, `ConfirmacaoSessao` e `MensagemOutbox` são gravadas na mesma transação do EF Core.

## Segurança

Incluído:

- HTTPS redirection
- HSTS fora de desenvolvimento
- JWT Bearer
- Rate Limiting no login
- CORS configurável
- headers de segurança
- refresh token rotation
- detecção de reutilização
- bloqueio temporário
- `ProblemDetails` sem stack trace em produção

## Validação executada

Nesta entrega foram executados:

```powershell
dotnet restore AuthService.slnx --configfile NuGet.Config --ignore-failed-sources
dotnet build AuthService.slnx --no-restore
dotnet test AuthService.slnx --no-build --no-restore
dotnet ef migrations add Inicial --project AuthService.Infrastructure\AuthService.Infrastructure.csproj --startup-project AuthService.Infrastructure\AuthService.Infrastructure.csproj --output-dir Persistence\Migrations --no-build
```

Resultado:

- build sem erros
- 21 testes passando
- migration inicial criada
- avisos `NU1900` ocorreram apenas porque a rede externa para auditoria de vulnerabilidades do NuGet estava indisponível neste ambiente
