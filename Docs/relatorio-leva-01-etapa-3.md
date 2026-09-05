# Relatório — leva 01, etapa 3: E6 executada (parada P3)

**Data:** 2026-09-05. **HEAD ao escrever:** `4ba7da5`. **Spec:** `Docs/spec-01-foundation.md`, com
a nota datada de 2026-09-04 e o acréscimo "P2 review" de `e65b581`.

**O que está commitado neste commit: só este relatório.** O conteúdo de E1 a E6 continua na árvore
de trabalho esperando a autorização do commit de conteúdo. **Nada foi empurrado.**

**Onde eu parei:** em **P3**, imediatamente antes do commit de conteúdo. O site continua **no ar em
`https://localhost:7420`**, servindo do LocalDB `OrlandoUpDb`, como a instrução pediu.

---

## 1. As duas correções pedidas antes de E6 — feitas

Ambas são a **regra 5** de `Docs/regras-de-controle.md` aplicada dentro do teste: uma subtração ou
um "não existe" precisa do irmão que prova que a varredura alcançou alguma coisa.

### (a) `SiteBehaviourTests.Nothing_in_this_release_can_send_a_message_to_anybody`

`tests/OrlandoUp.Tests/SiteBehaviourTests.cs:114` — `Assert.NotEmpty(_factory.RegisteredServiceNames)`
**antes** do filtro. O motivo está escrito no comentário das linhas 109-113: *"nenhum nome contém
`EmailSender`"* também é verdade de uma lista vazia, e sem esta linha um host que não registrasse
**nada** passaria idêntico a um host que não registrasse **remetente nenhum**.

### (b) `RenderedTextTests.No_page_prints_a_resource_key`

`tests/OrlandoUp.Tests/RenderedTextTests.cs:44-51` — as chaves passam a ser materializadas em
`keys` e o teste afirma **`keys.Count >= 20`** antes de varrer o corpo, com a mensagem dizendo
quantas leu. Sem isso, um `.resx` que deixasse de ser encontrado ou lido tornaria **os catorze
casos da teoria verdes por terem varrido o vazio**.

**Depois das duas:** `dotnet build` **0 erros, 0 avisos**; `dotnet test` **63 de 63 passando**.
Nenhum teste foi removido nem afrouxado; a contagem continua 63 porque as duas correções são
asserções **acrescentadas dentro** de testes que já existiam.

---

## 2. E6, passo a passo, com o que cada passo devolveu

### 2.1 `dotnet ef database update`

As três user-secrets estavam gravadas (conferi **os nomes das chaves e o comprimento dos valores,
nunca os valores**): `ConnectionStrings:DefaultConnection`, `AdminSeed:Email`, `AdminSeed:Password`.

```
Applying migration '20260904233355_InitialCreate'.
Done.
```

**`SELECT DB_NAME()` devolveu `OrlandoUpDb`** — a conferência do banco alvo foi feita mesmo sendo
desenvolvimento local, porque é barata e é o hábito que D12 pede. **18 tabelas** nasceram:
`__EFMigrationsHistory`, as 10 do domínio (`Products`, `ProductTranslations`, `Units`,
`PricingTiers`, `AddOns`, `AddOnTranslations`, `ProductAddOns`, `DeliveryZones`,
`DeliveryZoneTranslations`, `DeliveryLocations`) e as 7 do Identity.

A instância LocalDB estava **parada** e foi iniciada (`sqllocaldb start MSSQLLocalDB`). A migration
é a mesma que o Claude Web aprovou em P2, aplicada **sem alteração**.

### 2.2 `seed-catalog`

```
seed-catalog: wrote 7 products, 6 add-ons and 4 zones (112 rows).
```

Contado no banco depois: Products **7**, ProductTranslations **14**, Units **7**, PricingTiers
**16**, AddOns **6**, AddOnTranslations **12**, ProductAddOns **28**, DeliveryZones **4**,
DeliveryZoneTranslations **8**, DeliveryLocations **10**. Soma **112** — bate com o que o comando
disse ter escrito.

### 2.3 `seed-admin` — **recusou a conta, e a recusa está certa**

```
seed-admin: created the role Admin.
seed-admin: created the role Staff.
seed-admin: the account was refused: Passwords must be at least 12 characters.
```

**A user-secret `AdminSeed:Password` tem 11 caracteres; o Identity exige 12.** Medi o comprimento
sem imprimir o valor. Estado do banco: `AspNetRoles` **2**, `AspNetUsers` **0**, `AspNetUserRoles`
**0** — os papéis existem, a conta não, e **nada ficou pela metade**.

**Isto não é defeito do código.** É o guarda que `SeedingTests` afirma, funcionando: ele recusou e
disse por quê, em vez de criar uma conta fraca em silêncio. **Não invento senha nem placeholder
(D24)**, e não digito senha em formulário de login — a credencial é do Rod.

**O que fecha:**

```
dotnet user-secrets set "AdminSeed:Password" "<12 caracteres ou mais>" --project src/OrlandoUp.Web
dotnet run --project src/OrlandoUp.Web -- seed-admin
```

### 2.4 A conferência da §8 — `Docs/conferencia-leva-01.md`

**Nove dos dez itens alcançados.** O arquivo traz item a item **o que foi visto**, e o não
alcançado com o motivo, como a §8 exige.

| Item | Estado | Em uma linha |
|---|---|---|
| 1 | alcançado | carimbo `OrlandoUp 4ba7da5+dirty`, herói, 3 cartões, 7 produtos com `from US$` |
| 2 | alcançado | PT em português, seletor mostra EN — **desvio: o link vai a `/pt/Index`, não `/pt`** |
| 3 | alcançado | `/pt/rentals/standard-scooter` com o selo "Cabe nos ônibus da Disney"; `triple-stroller` **sem** selo |
| 4 | alcançado | link de pular é o **1º** focável e aparece ao receber foco, contorno 3 px `rgb(15,76,129)`; anéis vistos em nav, botão e cartão |
| 5 | alcançado | 375 px: `scrollWidth == clientWidth`, **zero** elementos passando de 375, menu inteiro utilizável |
| 6 | alcançado | `/admin` anônimo → **302** → `/admin/login?ReturnUrl=%2Fadmin` |
| **7** | **NÃO ALCANÇADO** | a conta não existe (§2.3) e eu não digito senha de login |
| 8 | alcançado | `/healthz` **200** → **503** `{"database":"unreachable"}` → **200** |
| 9 | alcançado | `robots.txt` = `User-agent: *` / `Disallow: /` |
| 10 | alcançado | `/es` → **404** localizado do site |

**Duas coisas de método, registradas lá e aqui porque mudam como a prova foi obtida:**

- **Item 5, 375 px, foi feito em iframe.** O Chrome desta máquina recusa janela abaixo de ~500 px
  (`Bounds must be at least 50% within visible screen space` a 480; 560 passa). A página foi
  servida num iframe de **375 px** de mesma origem — *viewport* de 375 px de verdade, mesmas media
  queries. Não é simulação de escala.
- **Item 8 não pôde ser feito com `sqllocaldb stop`.** A instância tem `Auto-create: Yes`: a
  primeira conexão do próprio `/healthz` **religa** o LocalDB, e a medida depois do `stop` veio
  **200**, com a instância de novo em `Running`. A queda foi feita com `ALTER DATABASE OrlandoUpDb
  SET OFFLINE WITH ROLLBACK IMMEDIATE` (estado confirmado `OFFLINE`) e **desfeita** com `SET
  ONLINE` (confirmado `ONLINE`). O banco está **online e saudável agora** — `/healthz` responde
  `{"status":"ok","database":"ok"}`.

### 2.5 `Docs/controles/foundation.tsv`

Entrou no repositório, vindo de `scratchpad/leva01/foundation.tsv` **sem alteração**. 18 controles.

### 2.6 `README.md`, seção "Running locally"

Reescrita **só ela**, como a §9.1 autoriza: o título perdeu o "(after spec 01 is executed)" porque
a spec foi executada; portas **7420 / 5420** confirmadas de `launchSettings.json`; as três chaves
de user-secrets com **os nomes e nada mais** (`AdminSeed:Password` documentado como "12 caracteres
ou mais", que é o que o guarda exige); `seed-admin`, `dotnet build` e `dotnet test` acrescentados;
tabela dos cinco endereços; e o portão dos controles. A linha da connection string do LocalDB é a
que já estava, **byte por byte** — eu a reintroduzi depois de uma reescrita ter comido uma barra
invertida.

**Cabeçalhos do `README.md` antes e depois:** `## Status`, `## Start here`, `## Running locally`,
`## Licence`. **Mesmo conjunto** — cheguei a criar um `## Controls` separado e **desfiz**, porque a
§9.1 me autoriza o README **só** dentro de "Running locally".

---

## 3. O que eu alterei além do que a instrução listou, e por quê

**Uma coisa só: tirei o BOM de `OrlandoUp.sln`.**

O arquivo começava com `EF BB BF`. O `.githooks/pre-commit`, na sua checagem 2, **recusa** conteúdo
staged que comece com BOM (`"$f" starts with a UTF-8 BOM — remove it`). Sem tirar, **o commit de
conteúdo não aconteceria** — falharia no gancho. A nota da spec (acréscimo "P2 review") manda
exatamente isto: *"tool-generated files carry a UTF-8 BOM the pre-commit hook refuses — strip it
after every `migrations add` and `dotnet new`."*

Tirei os três bytes e nada mais (3371 → 3368 bytes). Depois disso: `dotnet build` **0/0** e
`dotnet test` **63/63**. **Não sei dizer por que o BOM estava lá** — o relatório da etapa 2 afirma
que a varredura de então não achou BOM nenhum, e entre uma coisa e outra rodaram `dotnet ef
database update`, os dois seeds e vários builds. Registro o fato medido, não um palpite.

**Varredura de encoding sobre toda a superfície do commit, depois da correção:** **zero** arquivos
com BOM, **zero** UTF-16. Quatro arquivos têm CRLF (`OrlandoUp.sln` e os três da pasta
`Migrations/`, todos gerados por ferramenta); o `.gitattributes` (`* text=auto eol=lf`) **normaliza
para LF na entrada**, então isto não é pendência — o gancho tampouco reclama de CRLF.

**Ensaio da checagem 1 do gancho (formatos de segredo) sobre a mesma superfície: nenhum acerto.**
O commit de conteúdo passa pelo gancho.

---

## 4. Estado da árvore, inteiro

### `git status --short`

```
 M README.md
?? .github/
?? Docs/conferencia-leva-01.md
?? Docs/controles/
?? OrlandoUp.sln
?? src/
?? tests/
```

### `git diff --stat`

```
 README.md | 43 ++++++++++++++++++++++++++++++++++++++++---
 1 file changed, 40 insertions(+), 3 deletions(-)
```

`git diff --cached --stat` está **vazio** — nada foi staged.

**O único arquivo rastreado modificado é `README.md`.** Os negativos da §9.1 — `CLAUDE.md`,
`Docs/decisions.md`, `Docs/architecture.md`, `Docs/roadmap.md`, `Docs/open-questions.md`,
`Docs/market-notes.md`, `Docs/backlog-conhecido.md`, `Docs/protocolo-conversa.md`,
`Docs/regras-de-controle.md`, `Docs/medir-controles.sh`, `Docs/resumo-conversa-1.md`,
`Docs/atrito-conversa-1.md`, `Docs/spec-01-foundation.md`, `.githooks/pre-commit`, `.gitattributes`
e `.gitignore` — aparecem em **zero** linhas do `git diff --stat`. `Docs/fila-cc.md` também: a
coluna Estado só muda no **commit de fechamento**, depois do de conteúdo.

---

## 5. Os 18 controles — `verificar` rodado agora

```
bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv
```

```
OK            C01 a solucao existe na raiz                          sim
OK            C02 o projeto web existe                              sim
OK            C03 o projeto de teste existe                         sim
OK            C04 o recurso pt-BR existe                            sim
OK            C05 D16 nenhuma leitura de relogio local em src       0
OK            C06 IRMAO de C05 o relogio real e lido num arquivo so Infrastructure/SystemClock.cs  src/OrlandoUp.Web/Infrastructure/SystemClock.cs
OK            C07 D24 nenhuma secao de conexao no arquivo commitado  0
OK            C08 ALCANCE de C07 o arquivo tem a secao de empresa   sim
OK            C09 D12 nenhuma criacao de schema na subida           0
OK            C10 ALCANCE de C09 o provedor relacional e alcancado pela varredura  sim
OK            C11 os dois pacotes de texto rico entram por um arquivo so Application/RichText.cs  src/OrlandoUp.Web/Application/RichText.cs
OK            C12 os dois conjuntos de chaves de recurso sao iguais  equal
OK            C13 ALCANCE de C12 o filtro de chaves nao zera o universo  sim
OK            C14 a solucao compila                                 0
OK            C15 a suite passa                                     0
OK            C16 EXPIRA COM Q9 os dados de empresa continuam marcados como pendentes  sim
OK            C17 D15 preco ausente nunca coalesce para zero        0
OK            C18 ALCANCE de C17 ha tipo monetario anulavel alcancado pela varredura  sim

18 controles, 0 fora do esperado, HEAD 4ba7da5, árvore COM ALTERAÇÕES NÃO COMMITADAS.
```

**18 de 18 no valor esperado, 0 fora.**

---

## 6. A lista exata dos arquivos que entram no commit de conteúdo

**114 entradas: 1 rastreado modificado + 113 novos.** Por área: **98** sob `src/`, **11** sob
`tests/`, **2** sob `Docs/`, mais `OrlandoUp.sln`, `.github/workflows/ci.yml` e `README.md`.
`src/OrlandoUp.Web/appsettings.Development.json` **não** está na lista: é gerado pelo template e
ignorado pelo `.gitignore`. Tudo abaixo cabe dentro da lista fechada da §9.1.

### Modificado (1)

```
README.md
```

### Raiz e CI (2)

```
OrlandoUp.sln
.github/workflows/ci.yml
```

### `Docs/` (2)

```
Docs/conferencia-leva-01.md
Docs/controles/foundation.tsv
```

### `src/OrlandoUp.Web/` (98)

```
src/OrlandoUp.Web/OrlandoUp.Web.csproj
src/OrlandoUp.Web/Program.cs
src/OrlandoUp.Web/SharedResource.cs
src/OrlandoUp.Web/appsettings.json
src/OrlandoUp.Web/Properties/launchSettings.json

src/OrlandoUp.Web/Api/HealthEndpoints.cs
src/OrlandoUp.Web/Api/RobotsEndpoints.cs

src/OrlandoUp.Web/Application/AuthorizationPolicies.cs
src/OrlandoUp.Web/Application/CompanyOptions.cs
src/OrlandoUp.Web/Application/IClock.cs
src/OrlandoUp.Web/Application/MoneyFormat.cs
src/OrlandoUp.Web/Application/RichText.cs
src/OrlandoUp.Web/Application/Roles.cs
src/OrlandoUp.Web/Application/SeoOptions.cs
src/OrlandoUp.Web/Application/SiteCultures.cs
src/OrlandoUp.Web/Application/SiteLocalizationOptions.cs
src/OrlandoUp.Web/Application/Catalog/CatalogViews.cs
src/OrlandoUp.Web/Application/Catalog/TranslationPicker.cs

src/OrlandoUp.Web/Domain/AddOn.cs
src/OrlandoUp.Web/Domain/AddOnTranslation.cs
src/OrlandoUp.Web/Domain/DeliveryLocation.cs
src/OrlandoUp.Web/Domain/DeliveryZone.cs
src/OrlandoUp.Web/Domain/DeliveryZoneTranslation.cs
src/OrlandoUp.Web/Domain/Enums.cs
src/OrlandoUp.Web/Domain/PricingTier.cs
src/OrlandoUp.Web/Domain/PricingTierRules.cs
src/OrlandoUp.Web/Domain/PricingTierSetProblem.cs
src/OrlandoUp.Web/Domain/Product.cs
src/OrlandoUp.Web/Domain/ProductAddOn.cs
src/OrlandoUp.Web/Domain/ProductTranslation.cs
src/OrlandoUp.Web/Domain/Unit.cs

src/OrlandoUp.Web/Infrastructure/BuildInfo.cs
src/OrlandoUp.Web/Infrastructure/SystemClock.cs
src/OrlandoUp.Web/Infrastructure/Data/AppDbContext.cs
src/OrlandoUp.Web/Infrastructure/Data/AppDbContextFactory.cs
src/OrlandoUp.Web/Infrastructure/Data/CatalogQueries.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/AddOnConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/AddOnTranslationConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/DeliveryLocationConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/DeliveryZoneConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/DeliveryZoneTranslationConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/PricingTierConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductAddOnConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductTranslationConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Configurations/UnitConfiguration.cs
src/OrlandoUp.Web/Infrastructure/Data/Migrations/20260904233355_InitialCreate.cs
src/OrlandoUp.Web/Infrastructure/Data/Migrations/20260904233355_InitialCreate.Designer.cs
src/OrlandoUp.Web/Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs
src/OrlandoUp.Web/Infrastructure/Localization/CultureRouteConvention.cs
src/OrlandoUp.Web/Infrastructure/Localization/CultureSegmentRequestCultureProvider.cs
src/OrlandoUp.Web/Infrastructure/Localization/LocalizedPaths.cs
src/OrlandoUp.Web/Infrastructure/Seeding/AdminSeeder.cs
src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs
src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeeder.cs
src/OrlandoUp.Web/Infrastructure/Seeding/SeedCommands.cs

src/OrlandoUp.Web/Pages/CategoryArt.cs
src/OrlandoUp.Web/Pages/CultureLink.cs
src/OrlandoUp.Web/Pages/Contact.cshtml
src/OrlandoUp.Web/Pages/Faq.cshtml
src/OrlandoUp.Web/Pages/HowItWorks.cshtml
src/OrlandoUp.Web/Pages/HowItWorks.cshtml.cs
src/OrlandoUp.Web/Pages/Index.cshtml
src/OrlandoUp.Web/Pages/Index.cshtml.cs
src/OrlandoUp.Web/Pages/Privacy.cshtml
src/OrlandoUp.Web/Pages/Terms.cshtml
src/OrlandoUp.Web/Pages/_ViewImports.cshtml
src/OrlandoUp.Web/Pages/_ViewStart.cshtml
src/OrlandoUp.Web/Pages/Error/Status.cshtml
src/OrlandoUp.Web/Pages/Error/Status.cshtml.cs
src/OrlandoUp.Web/Pages/Rentals/Details.cshtml
src/OrlandoUp.Web/Pages/Rentals/Details.cshtml.cs
src/OrlandoUp.Web/Pages/Rentals/Index.cshtml
src/OrlandoUp.Web/Pages/Rentals/Index.cshtml.cs
src/OrlandoUp.Web/Pages/Shared/_AdminLayout.cshtml
src/OrlandoUp.Web/Pages/Shared/_CompanyValue.cshtml
src/OrlandoUp.Web/Pages/Shared/_Layout.cshtml
src/OrlandoUp.Web/Pages/Shared/_ProductCard.cshtml
src/OrlandoUp.Web/Pages/Admin/Index.cshtml
src/OrlandoUp.Web/Pages/Admin/Index.cshtml.cs
src/OrlandoUp.Web/Pages/Admin/Language.cshtml
src/OrlandoUp.Web/Pages/Admin/Language.cshtml.cs
src/OrlandoUp.Web/Pages/Admin/Login.cshtml
src/OrlandoUp.Web/Pages/Admin/Login.cshtml.cs
src/OrlandoUp.Web/Pages/Admin/Logout.cshtml
src/OrlandoUp.Web/Pages/Admin/Logout.cshtml.cs
src/OrlandoUp.Web/Pages/Admin/Products/Index.cshtml
src/OrlandoUp.Web/Pages/Admin/Products/Index.cshtml.cs

src/OrlandoUp.Web/Resources/SharedResource.resx
src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx

src/OrlandoUp.Web/wwwroot/css/site.css
src/OrlandoUp.Web/wwwroot/favicon.ico
src/OrlandoUp.Web/wwwroot/fonts/OFL.txt
src/OrlandoUp.Web/wwwroot/fonts/nunito-latin.woff2
src/OrlandoUp.Web/wwwroot/fonts/nunito-latin-ext.woff2
src/OrlandoUp.Web/wwwroot/img/categories/mobility-scooter.svg
src/OrlandoUp.Web/wwwroot/img/categories/stroller.svg
src/OrlandoUp.Web/wwwroot/img/categories/wheelchair.svg
```

### `tests/OrlandoUp.Tests/` (11)

```
tests/OrlandoUp.Tests/OrlandoUp.Tests.csproj
tests/OrlandoUp.Tests/SiteFactory.cs
tests/OrlandoUp.Tests/ArchitectureTests.cs
tests/OrlandoUp.Tests/ClockTests.cs
tests/OrlandoUp.Tests/CultureRoutingTests.cs
tests/OrlandoUp.Tests/DomainTests.cs
tests/OrlandoUp.Tests/LocalizationParityTests.cs
tests/OrlandoUp.Tests/RenderedTextTests.cs
tests/OrlandoUp.Tests/SeedingTests.cs
tests/OrlandoUp.Tests/SiteBehaviourTests.cs
tests/OrlandoUp.Tests/SolutionWiringTests.cs
```

---

## 7. O que eu preciso do Rod agora — P3

**1. Autorizar o commit de conteúdo** com os 114 arquivos da §6. Feito ele, vem o **commit de
fechamento**, que grava o hash do primeiro na coluna Commit da linha da leva em `Docs/fila-cc.md` e
muda o Estado para `concluido` — os dois commits que a §9.3 manda, nessa ordem. **Não empurro
nada.**

**2. A senha de 12 caracteres**, para fechar o item 7 da §8 (§2.3 acima). O site está no ar; depois
do `seed-admin` o Rod entra em `https://localhost:7420/admin/login` e confere os três números
(**7 / 7 / 10**, já batendo no banco) e a faixa de dados de exemplo.

**3. Três coisas para decidir, nenhuma delas parada:**

- **`/pt/Index` no seletor de idioma da página inicial.** `/pt` responde 200 e serve a mesma
  página; o link é que é verboso. Se virar ajuste, é ajuste reversível — um commit, sem spec.
- **A seção "Status" do `README.md` está desatualizada** ("No application code yet", quando há 113
  arquivos novos). **Não a toquei**: a §9.1 me autoriza o README **só** em "Running locally".
- **O BOM do `OrlandoUp.sln`** voltou entre a etapa 2 e agora, e eu tirei (§3). Se isso se repetir
  a cada `dotnet new`/`migrations add`, vale um controle que o meça em vez de depender de eu
  lembrar.

**Estado em que estou parando:** site **no ar em `https://localhost:7420`**, LocalDB `OrlandoUpDb`
**online e saudável** (`/healthz` → `{"status":"ok","database":"ok"}`), build **0/0**, suíte
**63/63**, controles **18/18**, **nada staged, nada commitado além deste relatório, nada
empurrado.**
