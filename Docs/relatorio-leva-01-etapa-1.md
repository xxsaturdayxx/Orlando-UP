# Relatório — leva 01, etapa 1: a migration `InitialCreate` (parada P2)

**Data:** 2026-09-04. **HEAD ao escrever:** `a11a5ad`. **Spec:** `Docs/spec-01-foundation.md` com a
nota datada de 2026-09-04 da conversa 2, que vence o corpo onde discordarem. **Linha da fila:**
`2026-09-04`, "LEVA 01 — APPLICATION FOUNDATION", ainda `aguardando`.

**O que está commitado neste commit: só este relatório.** Todo o código de E1 e E2 está na árvore
de trabalho, **não rastreado**, esperando a revisão da migration. Nada foi aplicado a banco nenhum:
`dotnet ef database update` **não** foi executado, e não há nem connection string configurada.
Nada foi empurrado.

**O que esta parada pede:** a revisão da migration pelo Claude Web, na conversa. Os anexos A, B e C
trazem, colados, a migration, o *model snapshot* e o SQL que a migration produz — o SQL porque é
ele que roda, e o C# descreve intenção, não efeito.

---

## 1. O que esta rodada fez

**E1 — a solução existe e a suíte roda.**

| Item | Estado |
|---|---|
| `OrlandoUp.sln` na raiz, com os dois projetos | feito |
| `src/OrlandoUp.Web` (Razor Pages, `net10.0`), `RootNamespace` `OrlandoUp` | feito |
| `tests/OrlandoUp.Tests` (xunit, `net10.0`), com referência ao projeto web | feito |
| Pacotes fixados em versão explícita (D2/01) | feito — a lista está em §3.5 |
| `.github/workflows/ci.yml` — restore, build, test, Ubuntu, .NET 10 | feito |
| `launchSettings.json` com HTTPS 7420 e HTTP 5420 (D6/01) | feito |
| `dotnet build OrlandoUp.sln` | **0 erros, 0 avisos** |
| `dotnet test OrlandoUp.sln` | **1 teste, 1 passou, 0 falharam** |

**E2 — o domínio, a aplicação, a infraestrutura e a migration.**

| Camada | Arquivos |
|---|---|
| `Domain/` | 13 arquivos: 7 enums em `Enums.cs`, 10 entidades, `PricingTierRules` + `PricingTierSetProblem` |
| `Application/` | `IClock`, `RichText`, `CompanyOptions`, `SeoOptions`, `SiteLocalizationOptions` |
| `Infrastructure/` | `SystemClock` |
| `Infrastructure/Data/` | `AppDbContext`, `AppDbContextFactory` (design-time), 10 configurações de entidade |
| `Infrastructure/Data/Migrations/` | `20260904233355_InitialCreate.cs`, `.Designer.cs`, `AppDbContextModelSnapshot.cs` |

`FitsDisneyTransport` é calculado no domínio (30 in × 48 in) e **explicitamente ignorado** pelo
mapeamento — não vira coluna. `FromPricePerDay()` devolve `decimal?` e **devolve ausência como
ausência** (D15): quando nenhuma faixa sabe dizer um valor diário, o retorno é nulo, nunca zero.

**Somas do schema, medidas no SQL gerado, não digitadas:**

| Medida | Valor |
|---|---|
| instruções no script | 40 |
| destrutivas | **0** |
| tabelas criadas | 18 (10 do catálogo, 7 de Identity, 1 de histórico de migrations) |
| índices | 18 — 10 únicos, 8 comuns |
| chaves estrangeiras em cascata | 12 |
| chaves estrangeiras sem ação (restrict) | 2 |
| `HasData` / seed de conteúdo | **0** (D5/01) |

---

## 2. As nove correções da nota, respondidas uma a uma

**1. `IDesignTimeDbContextFactory` lendo user-secrets e ambiente.** Feito, em
`src/OrlandoUp.Web/Infrastructure/Data/AppDbContextFactory.cs`: monta a configuração com
`AddUserSecrets(typeof(Program).Assembly, optional: true)` e `AddEnvironmentVariables()`, lê
`ConnectionStrings:DefaultConnection` e usa a string quando ela existe; só na ausência dela cai
para a sobrecarga sem argumento. **A sobrecarga sem argumento funciona** — foi por ela que a
migration desta rodada foi gerada, com a chave ainda não configurada. Nenhuma string de conexão
entra em arquivo nenhum; o controle C07 mede `0` no `appsettings.json`.

**2. O fail-fast contornável pelo host de teste.** Ainda **não implementado**, e não podia ser:
o fail-fast mora no `Program.cs`, que é de E3, e o `WebApplicationFactory` é de E5. A forma está
fixada no plano §5.6 e será executada lá: a checagem lê a configuração, o teste injeta a chave por
`UseSetting` e troca `DbContextOptions` por SQLite em memória. **Nesta rodada o `Program.cs` ainda
é o do template** — não há fail-fast a contornar, e por isso `dotnet test` passa.

**3. C05 mais largo e o irmão do relógio real.** Feito, e os dois já medem no valor final:

- C05 passou a cobrir também a leitura de relógio local do tipo com deslocamento. Mede **0**.
- C06 deixou de ser "existe `IClock` em algum lugar" e virou **discriminante**: lista os arquivos
  de `src/` que leem o relógio de verdade e devolve **o caminho**. Mede
  `src/OrlandoUp.Web/Infrastructure/SystemClock.cs` — exatamente um arquivo, e o rótulo o nomeia.

A troca (em vez de acréscimo) mantém os 18 controles que a nota fixa, e está justificada no plano
§6.1: o irmão discriminante **é** a asserção de alcance, e mais forte — se a varredura deixasse de
alcançar `src/`, a saída seria vazia e o controle ficaria vermelho.

**4. C11 discriminante.** Feito, com o comando exatamente como a nota o escreveu. Mede
`src/OrlandoUp.Web/Application/RichText.cs`.

**5. Nenhum comentário de `src/` transcreve o que C05, C09 e C17 procuram.** Conferido pela via
que não depende da minha memória: os três controles varrem `src/` inteiro, comentário incluído —
`grep` não sabe o que é comentário —, e **os três medem 0**. Se um comentário meu tivesse escrito
qualquer uma das três formas, o número não seria zero. Os comentários que explicam essas escolhas
nomeiam o controle e descrevem a forma sem escrevê-la.

**6. Ordem do middleware com comentário no `Program.cs`.** É de **E3**; não entra nesta rodada. A
ordem está fixada no plano §5.3.

**7. `asp-route-culture=""` no link para o inglês.** É de **E4** (o trocador mora no layout); não
entra nesta rodada. Está fixado no plano §5.3.

**8. `dotnet dev-certs https --check`.** Medido no passo 0:
`A trusted certificate was found: 7E5F9B792F91A6E393FFD6557D983964D7C87387 - CN=localhost - Valid
from 2026-07-14 to 2027-07-14`, saída **0**. O certificado já é **confiável**; não é preciso rodar
`--trust`.

**9. As duas lacunas decididas.** São de E4 (o filtro de ativos nas páginas) e de E4/E5 (a queda de
tradução para `en-US`); não há página nesta rodada. O schema já as comporta: `IsActive` existe em
`Products`, e `ProductTranslations` tem chave única `(ProductId, Culture)`, que é o que permite
procurar a cultura pedida e, na falta, a `en-US`.

---

## 3. Contradições e desvios que a execução produziu

Cinco. Nenhum é conserto de arquivo negativo; todos estão dentro de `src/`, `tests/` ou da
declaração de arquivos que a frente cria.

### 3.1 `dotnet new sln` agora cria `.slnx`, não `.sln`

O SDK 10.0.400 cria **`OrlandoUp.slnx`** por padrão. A spec, os controles (C01, C14, C15) e o
`.githooks/pre-commit` (`if [ -f OrlandoUp.sln ]`) nomeiam **`OrlandoUp.sln`**. Apaguei o `.slnx` e
recriei com `dotnet new sln --format sln`. **A spec vence, e o formato clássico continua
plenamente suportado** — se eu tivesse deixado o `.slnx`, o passo 4 do hook nunca rodaria o build
e C01 mediria `nao` para sempre, com o arquivo existindo. É o caso exato de verde falso que as
regras querem impedir.

### 3.2 As ferramentas escrevem UTF-8 **com BOM**, e o hook recusa BOM

`dotnet ef migrations add` e o template Razor gravaram **12 arquivos com BOM** — inclusive os três
da migration. O passo 2 do `.githooks/pre-commit` reprova qualquer arquivo de texto que comece com
BOM. Removi o BOM dos 12 (só os bytes iniciais; nenhum outro byte tocado), reconstruí e rodei a
suíte: build e testes seguem verdes. **Isto vai se repetir a cada `migrations add`** — fica
registrado aqui para que a próxima leva não descubra de novo no meio de um commit reprovado.

Arquivos que tinham BOM: os três de `Infrastructure/Data/Migrations/`, `Pages/Error.cshtml`,
`Pages/Index.cshtml`, `Pages/Privacy.cshtml`, `Pages/Privacy.cshtml.cs`,
`Pages/Shared/_Layout.cshtml`, `Pages/Shared/_Layout.cshtml.css`, `Pages/_ViewImports.cshtml`,
`Pages/_ViewStart.cshtml`, `wwwroot/js/site.js`.

### 3.3 `LocalizationOptions` virou `SiteLocalizationOptions` — **desvio de nome, declarado**

A spec §6 nomeia a classe de opções `LocalizationOptions`. O framework já tem
`Microsoft.Extensions.Localization.LocalizationOptions`, e o `Program.cs` de E3 vai usar os dois
espaços de nome na mesma linha de configuração: duas classes com o mesmo nome ali é ambiguidade
que se resolve com apelido de espaço de nome, e apelido é a forma que ninguém lembra de manter.
**Chamei a nossa de `SiteLocalizationOptions`.** É o único nome desta rodada que não é o da spec;
se o Rod preferir o nome literal, troco e escrevo o apelido — é uma renomeação de um arquivo e uma
linha.

### 3.4 O template Razor trouxe Bootstrap e jQuery; D7/01 proíbe framework

`dotnet new razor` criou `wwwroot/lib/` com Bootstrap, jQuery, jquery-validation e
jquery-validation-unobtrusive — cerca de 60 arquivos. **D7/01 diz CSS escrito à mão, sem
framework e sem passo de build.** Apaguei `wwwroot/lib/` inteiro e o
`_ValidationScriptsPartial.cshtml` que só existe para carregá-los. `Pages/Shared/_Layout.cshtml`
ainda é o do template e ainda **referencia** esses caminhos; ele é reescrito em E4 e por isso não
mexi nele agora — registro para que a referência pendurada não seja lida como esquecimento.

### 3.5 Pacotes: o major novo do runner de teste **funcionou**

O plano declarou risco em `Microsoft.NET.Test.Sdk` **18.9.0** (o template propõe 17.14.1) e
prometeu cair para 17.14.1 se `dotnet test` quebrasse. **Não quebrou**: com 18.9.0 e
`xunit.runner.visualstudio` **4.0.0**, `dotnet test` acha e roda a suíte. Nenhuma troca a relatar.
As versões que entraram, todas explícitas:

| Pacote | Versão | Projeto |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.11 | Web |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.11 | Web (`PrivateAssets=all`) |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.11 | Web (`PrivateAssets=all`) |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.11 | Web |
| `Markdig` | 1.3.2 | Web |
| `HtmlSanitizer` | 9.2.1039 | Web |
| `Microsoft.NET.Test.Sdk` | 18.9.0 | Tests |
| `xunit` | 2.9.3 | Tests |
| `xunit.runner.visualstudio` | 4.0.0 | Tests |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.11 | Tests |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.11 | Tests |
| `NetArchTest.Rules` | 1.3.2 | Tests |

**Nota sobre `.gitignore`:** o template criou `src/OrlandoUp.Web/appsettings.Development.json`. Ele
já está coberto pela lista atual (linha `appsettings.Development.json`), então **não toquei no
`.gitignore`** — o arquivo existe no disco e nunca entra no repositório, que é o comportamento
desejado. `bin/` e `obj/` também já estavam cobertos. Nenhuma alteração naquele arquivo.

---

## 4. Revisão da migration

Feita com a skill `revisao-migration-efcore`, na ordem que ela manda: **o SQL foi gerado e lido**,
não o `.cs`.

```
dotnet ef migrations script --project src/OrlandoUp.Web --output scratchpad/leva01/migration-InitialCreate.sql
```

### 4.1 Classificação automática (saída colada, não digitada)

O cabeçalho da saída de `classificar.py`; a lista completa dos 21 itens de atenção está logo
abaixo, item a item, com a justificativa que a skill exige.

```
Script: migration-InitialCreate.sql
40 statement(s) · 0 destrutivo(s) · 21 de atencao · 16 aditivo(s)
```

### 4.2 Os 21 itens de atenção, justificados

**Doze `FK ON DELETE CASCADE`.** Divididos em dois grupos:

- **Seis são das tabelas de Identity** (`AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserLogins`,
  `AspNetUserRoles`, `AspNetUserTokens` e o par de papéis). São o padrão do framework: apagar um
  usuário apaga os *claims* e os *logins* dele, que é a intenção e não tem alternativa sensata —
  um *claim* órfão não significa nada.
- **Seis são nossas, e todas apontam de um pai para linhas que só existem por causa dele:**
  `ProductTranslations` → `Products`, `PricingTiers` → `Products`, `ProductAddOns` → `Products` e
  → `AddOns`, `AddOnTranslations` → `AddOns`, `DeliveryZoneTranslations` → `DeliveryZones`. Uma
  tradução sem produto, uma faixa de preço sem produto ou uma linha de ligação sem as duas pontas
  são lixo, não dado. **As duas que NÃO cascateiam são exatamente as que guardam história:**
  `Units` → `Products` e `DeliveryLocations` → `DeliveryZones` saem como `ON DELETE NO ACTION`,
  para que apagar um produto ou uma zona com unidade ou local pendurado **falhe**, em vez de
  levar o histórico junto. É a intenção da spec §4, e o SQL confirma nas linhas 143 e 200.

**Dez `CREATE UNIQUE INDEX`.** Todos sobre **tabela vazia**: esta é a primeira migration do banco,
as tabelas nascem no mesmo script. Não há duplicata possível para o índice tropeçar. A conferência
de duplicatas "sobre todas as linhas" que D12 exige aplica-se a índice único criado sobre tabela
**com dados**, e não é o caso de nenhum destes.

**Dois deles são filtrados** e vieram do Identity: `RoleNameIndex` sobre `AspNetRoles` e
`UserNameIndex` sobre `AspNetUsers`, ambos com `WHERE [...] IS NOT NULL`. Índice filtrado exige
`QUOTED_IDENTIFIER ON` na sessão que o cria **e** em todo `INSERT` posterior na tabela. O cliente
SQL do .NET liga essa opção por padrão, e é por ele que `dotnet ef database update` e a aplicação
falam com o banco — por isso o caminho previsto não tem problema. **O caminho que teria** é
aplicar o script à mão por uma ferramenta que não ligue a opção; se um dia isso for feito, o
`SET QUOTED_IDENTIFIER ON` tem de vir antes.

**Um `INSERT (seed)` — falso positivo, e vale dizer por quê.** A skill marca todo `INSERT` porque
`HasData` é estado declarado, não carga. O único `INSERT` deste script é a linha do próprio
`__EFMigrationsHistory` que registra a migration aplicada. **Não há `HasData` nenhum no modelo** —
D5/01 proíbe, e a semeadura será por comando explícito (`seed-catalog`, `seed-admin`, em E5). Uma
busca por `HasData` nos três arquivos gerados só encontra `HasDatabaseName`, que é outra coisa.

### 4.3 As armadilhas da skill, uma a uma

| Armadilha | Aplica? | Conferência |
|---|---|---|
| `HasData` como carga inicial | **não** | não há `HasData` no modelo; a semeadura é por comando (D5/01) |
| Inicializador de instante numa propriedade com seed | **não** | pela mesma razão: não há seed no modelo |
| Enum persistido por posição | **sim, e está fechado** | os 7 enums declaram **número explícito** (`MobilityScooter = 1`, `Other = 9`, …); acrescentar membro no meio não desloca nada |
| Índice único filtrado | **sim** | os dois do Identity; justificados em §4.2 |
| Duplicatas antes de índice único | **não** | tabelas nascem vazias neste mesmo script |
| Auto-referência com `SET NULL` | **não** | nenhuma tabela aponta para si mesma; nenhum `ON DELETE SET NULL` no script |
| Calendário × instante | **sim, e está fechado** | `PurchasedOn` é `date` (data de calendário em Orlando); `CreatedAtUtc` e `UpdatedAtUtc` são `datetime2` e carregam o sufixo no nome (D16) |
| Tipos: dinheiro, data-hora, texto | **sim, e está fechado** | dinheiro em `decimal(10,2)` (D15 — a spec fixa 10,2, não 18,2); dimensões em `decimal(5,1)`; taxa em `decimal(6,4)`; data-hora em `datetime2`; todo texto com tamanho, e `nvarchar(max)` só onde é Markdown ou JSON de tamanho aberto (`Description`, `Highlights`, `Instructions`, `Notes`) e nas colunas do próprio Identity |
| Renomear × recriar | **não** | primeira migration; não há o que renomear |
| Coluna obrigatória nova em tabela com dados | **não** | primeira migration; não há tabela com dados |

**Uma diferença deliberada com a skill, dita em voz alta:** a skill diz "valor monetário é
`decimal(18,2)`". Este projeto usa **`decimal(10,2)`**, porque é o que `CLAUDE.md` e D15 fixam para
o domínio — aluguel em dólares, valores de duas a quatro casas antes da vírgula. Onde a skill
(escrita para o projeto irmão) e a decisão numerada deste repositório discordarem, vale a decisão.

### 4.4 Restrições de verificação

O script traz as três `CHECK` que a spec §4 pede, e o SQL as mostra literalmente:

```
CONSTRAINT [CK_PricingTiers_Amount] CHECK ([Amount] > 0),
CONSTRAINT [CK_PricingTiers_MaxDays] CHECK ([MaxDays] IS NULL OR [MaxDays] >= [MinDays]),
CONSTRAINT [CK_PricingTiers_MinDays] CHECK ([MinDays] >= 1),
```

A regra que **não** é de banco — faixas de um produto não se sobrepõem e cobrem de um dia ao
aberto — está no domínio, em `PricingTierRules.Validate`, e é ela que os testes de E5 exercitam.
Banco checa a linha; o domínio checa o conjunto.

### 4.5 Veredito

```
Migration: 20260904233355_InitialCreate
Classificação: PURAMENTE ADITIVA
Operações: 18 tabelas novas (10 do catálogo, 7 do Identity, 1 de histórico),
           18 índices (10 únicos, 8 comuns), 14 chaves estrangeiras
           (12 em cascata, 2 sem ação), 3 restrições CHECK, 0 destrutivas, 0 seed
Itens de atenção: 21, todos justificados em 4.2 — as cascatas são de linhas que só existem
           por causa do pai, as duas relações com história saem sem ação, e os índices únicos
           nascem sobre tabela vazia
Armadilhas conferidas: as dez da skill; três se aplicam e estão fechadas (enum com número
           explícito, índice filtrado do Identity, data de calendário × instante)
Recomendação: APLICAR — depois de o Rod configurar a user-secret da connection string (P1),
           e só ao LocalDB OrlandoUpDb, que é o único banco desta fase
```

**A recomendação é minha; a decisão de aplicar não é.** Não rodei `database update` e não vou
rodar até a revisão desta parada e a parada P1.

---

## 5. Os controles neste ponto

`Docs/controles/foundation.tsv` **ainda não existe no repositório** — ele nasce com o commit de
conteúdo, em E6, como a spec §9.1 manda. O arquivo de alvos está em
`scratchpad/leva01/foundation.tsv`, com os 18 controles decididos na nota, e a tabela abaixo é
**saída do medidor rodada agora**, com o código de E1 e E2 na árvore.

Onze dos dezoito já estão no valor final. Os sete que faltam dependem de E3–E6 e estão nomeados.

<!-- Gerado por Docs/medir-controles.sh em 04/09/2026, HEAD a11a5ad, árvore COM ALTERAÇÕES NÃO COMMITADAS.
     Nenhuma linha desta tabela foi escrita à mão. -->

**Controles medidos na árvore viva em 04/09/2026, HEAD `a11a5ad` (árvore COM ALTERAÇÕES NÃO COMMITADAS):**

| Controle | Comando | Hoje |
|---|---|---:|
| C01 a solucao existe na raiz | `ls -A . \| grep -qxF 'OrlandoUp.sln'` | **sim** |
| C02 o projeto web existe | `ls -A src/OrlandoUp.Web \| grep -qxF 'OrlandoUp.Web.csproj'` | **sim** |
| C03 o projeto de teste existe | `ls -A tests/OrlandoUp.Tests \| grep -qxF 'OrlandoUp.Tests.csproj'` | **sim** |
| C04 o recurso pt-BR existe | `ls -A src/OrlandoUp.Web/Resources \| grep -qxF 'SharedResource.pt-BR.resx'` | **ALVO-AUSENTE** |
| C05 D16 nenhuma leitura de relogio local em src | `grep -rIE 'DateTime(Offset)?[.](Now\|Today)' --include='*.cs' --include='*.cshtml' src \| wc -l` | **0** |
| C06 IRMAO de C05 o relogio real e lido num arquivo so Infrastructure/SystemClock.cs | `grep -rIlE '(DateTime\|DateTimeOffset)[.]UtcNow' --include='*.cs' src \| sort \| paste -sd,` | **src/OrlandoUp.Web/Infrastructure/SystemClock.cs** |
| C07 D24 nenhuma secao de conexao no arquivo commitado | `grep -cF 'ConnectionStrings' src/OrlandoUp.Web/appsettings.json` | **0** |
| C08 ALCANCE de C07 o arquivo tem a secao de empresa | `grep -qF 'Company' src/OrlandoUp.Web/appsettings.json` | **nao** |
| C09 D12 nenhuma criacao de schema na subida | `grep -rIE '(Migrate\|MigrateAsync\|EnsureCreated\|EnsureCreatedAsync)[(]' --include='*.cs' src \| wc -l` | **0** |
| C10 ALCANCE de C09 o provedor relacional e alcancado pela varredura | `test $(grep -rIl 'UseSqlServer' --include='*.cs' src \| wc -l) -ge 1 && echo sim \|\| echo nao` | **sim** |
| C11 os dois pacotes de texto rico entram por um arquivo so Application/RichText.cs | `grep -rIlE 'using (Markdig\|Ganss)' --include='*.cs' src \| sort \| paste -sd,` | **src/OrlandoUp.Web/Application/RichText.cs** |
| C12 os dois conjuntos de chaves de recurso sao iguais | `if [ -f src/OrlandoUp.Web/Resources/SharedResource.resx ] && [ -f src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx ]; then diff <(grep -o '<data name="[^"]*"' src/OrlandoUp.Web/Resources/SharedResource.resx \| sed 's/.*name="//' \| sort) <(grep -o '<data name="[^"]*"' src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx \| sed 's/.*name="//' \| sort) >/dev/null && echo equal \|\| echo diferente; else echo ausente; fi` | **ausente** |
| C13 ALCANCE de C12 o filtro de chaves nao zera o universo | `n=$(grep -c '<data name=' src/OrlandoUp.Web/Resources/SharedResource.resx); test ${n:-0} -ge 20 && echo sim \|\| echo nao` | **nao** |
| C14 a solucao compila | `dotnet build OrlandoUp.sln --nologo -v q >/dev/null 2>&1; echo $?` | **0** |
| C15 a suite passa | `dotnet test OrlandoUp.sln --nologo -v q >/dev/null 2>&1; echo $?` | **0** |
| C16 EXPIRA COM Q9 os dados de empresa continuam marcados como pendentes | `n=$(grep -cF 'TODO-' src/OrlandoUp.Web/appsettings.json); test ${n:-0} -ge 4 && echo sim \|\| echo nao` | **nao** |
| C17 D15 preco ausente nunca coalesce para zero | `grep -rIE '[?][?] *0\|GetValueOrDefault[(]' --include='*.cs' --include='*.cshtml' src \| wc -l` | **0** |
| C18 ALCANCE de C17 ha tipo monetario anulavel alcancado pela varredura | `test $(grep -rIlE 'decimal[?]' --include='*.cs' src \| wc -l) -ge 1 && echo sim \|\| echo nao` | **sim** |

**Onde cada um chega, e por qual etapa:** C04, C12 e C13 dependem dos arquivos de recurso (E3);
C08 e C16 dependem do `appsettings.json` com a secao de empresa (E3). C01, C02, C03, C05, C06,
C07, C09, C10, C11, C14 e C15 **ja estao no valor final**.

---

## 6. Asserções de dois lados (regra 8), rodadas a partir de arquivo gravado

Cada padrão negativo foi rodado contra um arquivo que contém a forma proibida e contra outro que
contém a forma certa. Os arquivos estão em `scratchpad/leva01/assercoes/`.

| Controle | Casa a forma proibida | Não casa a forma certa |
|---|---|---|
| C05 (relógio local) | **3** de 3 linhas proibidas | **0** de 8 linhas corretas |
| C06 (relógio real) | **2** de 2 leituras reais | **0** — nem a declaração da propriedade nem a chamada pelo abstrato |
| C09 (criação de schema) | **3** de 3 linhas proibidas | **0** — nem `CreateTable` da migration nem `UseSqlServer` |
| C11 (texto rico) | **2** de 2 linhas de importação | **0** de 2 importações vizinhas |
| C17 (coalescer preço) | **3** de 3 formas, incluindo o sufixo decimal e a leitura com valor padrão | **0** — inclusive a linha que usa o mesmo operador com um limite que não é zero |

A última coluna de C17 importa: `PricingTierRules` usa o operador de coalescência com um valor que
**não** é zero, para ordenar a faixa aberta por último. O padrão não o casa, que é o que separa
"coalescer preço para zero" de "usar o operador".

---

## 7. Estado da árvore

```
git status --short
?? .github/
?? OrlandoUp.sln
?? src/
?? tests/

git diff --stat
(vazio — nenhum arquivo rastreado foi modificado)
```

**Nenhum arquivo negativo foi tocado.** `CLAUDE.md`, `Docs/decisions.md`,
`Docs/architecture.md`, `Docs/roadmap.md`, `Docs/open-questions.md`, `Docs/market-notes.md`,
`Docs/backlog-conhecido.md`, `Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`,
`Docs/medir-controles.sh`, `Docs/resumo-conversa-1.md`, `Docs/atrito-conversa-1.md`,
`Docs/spec-01-foundation.md`, `.githooks/pre-commit`, `.gitattributes` e `.gitignore` aparecem em
**zero** linhas do `git diff --stat`. `README.md` também não foi tocado — a seção "Running
locally" é de E6.

**55 arquivos novos na árvore**, todos dentro da lista fechada da spec §9.1 (`OrlandoUp.sln`,
`src/OrlandoUp.Web/`, `tests/OrlandoUp.Tests/`, `.github/workflows/ci.yml`). Um deles,
`src/OrlandoUp.Web/appsettings.Development.json`, é gerado pelo template e **ignorado** pelo
`.gitignore`; ele nunca entra no repositório.

**Hashes dos três arquivos gerados pelo EF, para que a revisão saiba do que está falando:**

| Arquivo | md5 | linhas |
|---|---|---|
| `20260904233355_InitialCreate.cs` | `2349603d0db872eac9fd60c0d83510e9` | 546 |
| `20260904233355_InitialCreate.Designer.cs` | `99a4acff0cb688195a52473548f0d988` | 780 |
| `AppDbContextModelSnapshot.cs` | `922eaec46ebfece3d6b65b2500e61b4e` | 777 |

O `.Designer.cs` e o `AppDbContextModelSnapshot.cs` descrevem o **mesmo** modelo — é a primeira
migration, então o instantâneo do modelo naquele ponto e o instantâneo corrente coincidem. Por
isso o anexo B traz o snapshot e não repete o designer.

---

## 8. O que eu preciso da revisão, e o que faço depois

1. **A migration está aprovada?** Se sim, sigo para E3 (localização, `Program.cs` inteiro,
   `/healthz`, `robots.txt`, páginas de erro) e daí até E5, sem tocar em banco.
2. **O desvio de nome de §3.3** (`SiteLocalizationOptions`) fica ou volta a ser
   `LocalizationOptions` com apelido de espaço de nome?
3. **Nada mais.** As user-secrets são de P1, depois de E5; não preciso delas para E3, E4 nem E5.

Se a revisão pedir mudança de schema, ela é barata **agora**: a migration não foi aplicada a banco
nenhum, então corrigi-la é apagar e regerar, não escrever uma migration corretiva.

---
## Anexo A — a migration, inteira

`src/OrlandoUp.Web/Infrastructure/Data/Migrations/20260904233355_InitialCreate.cs`,
md5 `2349603d0db872eac9fd60c0d83510e9`, 546 linhas. Nao rastreada ainda.

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrlandoUp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddOns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PricingMode = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    HandoverMode = table.Column<int>(type: "int", nullable: false),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<int>(type: "int", nullable: true),
                    MaxRiderWeightLb = table.Column<int>(type: "int", nullable: true),
                    WidthIn = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: false),
                    LengthIn = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: false),
                    SeatWidthIn = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    RangeMiles = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    TurnaroundDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddOnTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddOnId = table.Column<int>(type: "int", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddOnTranslations_AddOns_AddOnId",
                        column: x => x.AddOnId,
                        principalTable: "AddOns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryLocations_DeliveryZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "DeliveryZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryZoneTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryZoneTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryZoneTranslations_DeliveryZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "DeliveryZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MinDays = table.Column<int>(type: "int", nullable: false),
                    MaxDays = table.Column<int>(type: "int", nullable: true),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.Id);
                    table.CheckConstraint("CK_PricingTiers_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_PricingTiers_MaxDays", "[MaxDays] IS NULL OR [MaxDays] >= [MinDays]");
                    table.CheckConstraint("CK_PricingTiers_MinDays", "[MinDays] >= 1");
                    table.ForeignKey(
                        name: "FK_PricingTiers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAddOns",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AddOnId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddOns", x => new { x.ProductId, x.AddOnId });
                    table.ForeignKey(
                        name: "FK_ProductAddOns_AddOns_AddOnId",
                        column: x => x.AddOnId,
                        principalTable: "AddOns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAddOns_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Highlights = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AssetTag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddOns_Code",
                table: "AddOns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddOnTranslations_AddOnId_Culture",
                table: "AddOnTranslations",
                columns: new[] { "AddOnId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLocations_ZoneId_Name",
                table: "DeliveryLocations",
                columns: new[] { "ZoneId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryZones_Code",
                table: "DeliveryZones",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryZoneTranslations_ZoneId_Culture",
                table: "DeliveryZoneTranslations",
                columns: new[] { "ZoneId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingTiers_ProductId_MinDays",
                table: "PricingTiers",
                columns: new[] { "ProductId", "MinDays" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddOns_AddOnId",
                table: "ProductAddOns",
                column: "AddOnId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductId_Culture",
                table: "ProductTranslations",
                columns: new[] { "ProductId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_AssetTag",
                table: "Units",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_ProductId",
                table: "Units",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddOnTranslations");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DeliveryLocations");

            migrationBuilder.DropTable(
                name: "DeliveryZoneTranslations");

            migrationBuilder.DropTable(
                name: "PricingTiers");

            migrationBuilder.DropTable(
                name: "ProductAddOns");

            migrationBuilder.DropTable(
                name: "ProductTranslations");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DeliveryZones");

            migrationBuilder.DropTable(
                name: "AddOns");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
```

---

## Anexo B — o instantaneo do modelo, inteiro

`src/OrlandoUp.Web/Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`,
md5 `922eaec46ebfece3d6b65b2500e61b4e`, 777 linhas.

```csharp
// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrlandoUp.Infrastructure.Data;

#nullable disable

namespace OrlandoUp.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.AddOn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<int>("PricingMode")
                        .HasColumnType("int");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("AddOns", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.AddOnTranslation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AddOnId")
                        .HasColumnType("int");

                    b.Property<string>("Culture")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Description")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.HasKey("Id");

                    b.HasIndex("AddOnId", "Culture")
                        .IsUnique();

                    b.ToTable("AddOnTranslations", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryLocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<string>("Notes")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<int>("ZoneId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ZoneId", "Name")
                        .IsUnique();

                    b.ToTable("DeliveryLocations", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryZone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<decimal>("DeliveryFee")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.Property<int>("HandoverMode")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<int>("Kind")
                        .HasColumnType("int");

                    b.Property<decimal>("SalesTaxRate")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(6, 4)
                        .HasColumnType("decimal(6,4)")
                        .HasDefaultValue(0m);

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("DeliveryZones", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryZoneTranslation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Culture")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Instructions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<int>("ZoneId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ZoneId", "Culture")
                        .IsUnique();

                    b.ToTable("DeliveryZoneTranslations", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.PricingTier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.Property<int?>("MaxDays")
                        .HasColumnType("int");

                    b.Property<int>("MinDays")
                        .HasColumnType("int");

                    b.Property<int>("Mode")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductId", "MinDays");

                    b.ToTable("PricingTiers", null, t =>
                        {
                            t.HasCheckConstraint("CK_PricingTiers_Amount", "[Amount] > 0");

                            t.HasCheckConstraint("CK_PricingTiers_MaxDays", "[MaxDays] IS NULL OR [MaxDays] >= [MinDays]");

                            t.HasCheckConstraint("CK_PricingTiers_MinDays", "[MinDays] >= 1");
                        });
                });

            modelBuilder.Entity("OrlandoUp.Domain.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Category")
                        .HasColumnType("int");

                    b.Property<int?>("Configuration")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(260)
                        .HasColumnType("nvarchar(260)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<decimal>("LengthIn")
                        .HasPrecision(5, 1)
                        .HasColumnType("decimal(5,1)");

                    b.Property<int?>("MaxRiderWeightLb")
                        .HasColumnType("int");

                    b.Property<decimal?>("RangeMiles")
                        .HasPrecision(5, 1)
                        .HasColumnType("decimal(5,1)");

                    b.Property<decimal?>("SeatWidthIn")
                        .HasPrecision(5, 1)
                        .HasColumnType("decimal(5,1)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<int>("TurnaroundDays")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("WidthIn")
                        .HasPrecision(5, 1)
                        .HasColumnType("decimal(5,1)");

                    b.HasKey("Id");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.ProductAddOn", b =>
                {
                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("AddOnId")
                        .HasColumnType("int");

                    b.HasKey("ProductId", "AddOnId");

                    b.HasIndex("AddOnId");

                    b.ToTable("ProductAddOns", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.ProductTranslation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Culture")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Highlights")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<string>("Tagline")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("ProductId", "Culture")
                        .IsUnique();

                    b.ToTable("ProductTranslations", (string)null);
                });

            modelBuilder.Entity("OrlandoUp.Domain.Unit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssetTag")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<DateOnly?>("PurchasedOn")
                        .HasColumnType("date");

                    b.Property<string>("SerialNumber")
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AssetTag")
                        .IsUnique();

                    b.HasIndex("ProductId");

                    b.ToTable("Units", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OrlandoUp.Domain.AddOnTranslation", b =>
                {
                    b.HasOne("OrlandoUp.Domain.AddOn", "AddOn")
                        .WithMany("Translations")
                        .HasForeignKey("AddOnId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AddOn");
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryLocation", b =>
                {
                    b.HasOne("OrlandoUp.Domain.DeliveryZone", "Zone")
                        .WithMany("Locations")
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryZoneTranslation", b =>
                {
                    b.HasOne("OrlandoUp.Domain.DeliveryZone", "Zone")
                        .WithMany("Translations")
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("OrlandoUp.Domain.PricingTier", b =>
                {
                    b.HasOne("OrlandoUp.Domain.Product", "Product")
                        .WithMany("PricingTiers")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OrlandoUp.Domain.ProductAddOn", b =>
                {
                    b.HasOne("OrlandoUp.Domain.AddOn", "AddOn")
                        .WithMany("Products")
                        .HasForeignKey("AddOnId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OrlandoUp.Domain.Product", "Product")
                        .WithMany("AddOns")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AddOn");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OrlandoUp.Domain.ProductTranslation", b =>
                {
                    b.HasOne("OrlandoUp.Domain.Product", "Product")
                        .WithMany("Translations")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OrlandoUp.Domain.Unit", b =>
                {
                    b.HasOne("OrlandoUp.Domain.Product", "Product")
                        .WithMany("Units")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OrlandoUp.Domain.AddOn", b =>
                {
                    b.Navigation("Products");

                    b.Navigation("Translations");
                });

            modelBuilder.Entity("OrlandoUp.Domain.DeliveryZone", b =>
                {
                    b.Navigation("Locations");

                    b.Navigation("Translations");
                });

            modelBuilder.Entity("OrlandoUp.Domain.Product", b =>
                {
                    b.Navigation("AddOns");

                    b.Navigation("PricingTiers");

                    b.Navigation("Translations");

                    b.Navigation("Units");
                });
#pragma warning restore 612, 618
        }
    }
}
```

---

## Anexo C — o SQL que a migration produz, inteiro

Gerado com `dotnet ef migrations script`, md5 `dad2e5fd70054541252fd121ab21ff1f`,
244 linhas. **E este o texto que roda**; os anexos A e B descrevem a intencao.
O arquivo fica em `scratchpad/leva01/`, que nao e commitado — por isso ele esta colado aqui.

```sql
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AddOns] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(40) NOT NULL,
    [PricingMode] int NOT NULL,
    [Amount] decimal(10,2) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_AddOns] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [DeliveryZones] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(40) NOT NULL,
    [Kind] int NOT NULL,
    [DeliveryFee] decimal(10,2) NOT NULL,
    [HandoverMode] int NOT NULL,
    [SalesTaxRate] decimal(6,4) NOT NULL DEFAULT 0.0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_DeliveryZones] PRIMARY KEY ([Id])
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Slug] nvarchar(80) NOT NULL,
    [Category] int NOT NULL,
    [Configuration] int NULL,
    [MaxRiderWeightLb] int NULL,
    [WidthIn] decimal(5,1) NOT NULL,
    [LengthIn] decimal(5,1) NOT NULL,
    [SeatWidthIn] decimal(5,1) NULL,
    [RangeMiles] decimal(5,1) NULL,
    [TurnaroundDays] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SortOrder] int NOT NULL,
    [ImagePath] nvarchar(260) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);

CREATE TABLE [AddOnTranslations] (
    [Id] int NOT NULL IDENTITY,
    [AddOnId] int NOT NULL,
    [Culture] nvarchar(10) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Description] nvarchar(400) NULL,
    CONSTRAINT [PK_AddOnTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AddOnTranslations_AddOns_AddOnId] FOREIGN KEY ([AddOnId]) REFERENCES [AddOns] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DeliveryLocations] (
    [Id] int NOT NULL IDENTITY,
    [ZoneId] int NOT NULL,
    [Name] nvarchar(160) NOT NULL,
    [Address] nvarchar(300) NULL,
    [Notes] nvarchar(400) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_DeliveryLocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryLocations_DeliveryZones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [DeliveryZones] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DeliveryZoneTranslations] (
    [Id] int NOT NULL IDENTITY,
    [ZoneId] int NOT NULL,
    [Culture] nvarchar(10) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Instructions] nvarchar(max) NULL,
    CONSTRAINT [PK_DeliveryZoneTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryZoneTranslations_DeliveryZones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [DeliveryZones] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PricingTiers] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [MinDays] int NOT NULL,
    [MaxDays] int NULL,
    [Mode] int NOT NULL,
    [Amount] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_PricingTiers] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PricingTiers_Amount] CHECK ([Amount] > 0),
    CONSTRAINT [CK_PricingTiers_MaxDays] CHECK ([MaxDays] IS NULL OR [MaxDays] >= [MinDays]),
    CONSTRAINT [CK_PricingTiers_MinDays] CHECK ([MinDays] >= 1),
    CONSTRAINT [FK_PricingTiers_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductAddOns] (
    [ProductId] int NOT NULL,
    [AddOnId] int NOT NULL,
    CONSTRAINT [PK_ProductAddOns] PRIMARY KEY ([ProductId], [AddOnId]),
    CONSTRAINT [FK_ProductAddOns_AddOns_AddOnId] FOREIGN KEY ([AddOnId]) REFERENCES [AddOns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductAddOns_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductTranslations] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Culture] nvarchar(10) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Tagline] nvarchar(200) NULL,
    [Description] nvarchar(max) NOT NULL,
    [Highlights] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ProductTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductTranslations_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Units] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [AssetTag] nvarchar(40) NOT NULL,
    [SerialNumber] nvarchar(80) NULL,
    [Status] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [PurchasedOn] date NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Units_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_AddOns_Code] ON [AddOns] ([Code]);

CREATE UNIQUE INDEX [IX_AddOnTranslations_AddOnId_Culture] ON [AddOnTranslations] ([AddOnId], [Culture]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_DeliveryLocations_ZoneId_Name] ON [DeliveryLocations] ([ZoneId], [Name]);

CREATE UNIQUE INDEX [IX_DeliveryZones_Code] ON [DeliveryZones] ([Code]);

CREATE UNIQUE INDEX [IX_DeliveryZoneTranslations_ZoneId_Culture] ON [DeliveryZoneTranslations] ([ZoneId], [Culture]);

CREATE INDEX [IX_PricingTiers_ProductId_MinDays] ON [PricingTiers] ([ProductId], [MinDays]);

CREATE INDEX [IX_ProductAddOns_AddOnId] ON [ProductAddOns] ([AddOnId]);

CREATE UNIQUE INDEX [IX_Products_Slug] ON [Products] ([Slug]);

CREATE UNIQUE INDEX [IX_ProductTranslations_ProductId_Culture] ON [ProductTranslations] ([ProductId], [Culture]);

CREATE UNIQUE INDEX [IX_Units_AssetTag] ON [Units] ([AssetTag]);

CREATE INDEX [IX_Units_ProductId] ON [Units] ([ProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260904233355_InitialCreate', N'10.0.11');

COMMIT;
GO
```
