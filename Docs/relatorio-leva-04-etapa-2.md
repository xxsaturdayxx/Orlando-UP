# Relatório — leva 04, etapa 2: o arreio de teste (parada P2)

**Data:** 2026-09-08. **HEAD ao escrever:** `ea3ad18`. **Spec:** `Docs/spec-04-admin-catalog.md`
com as notas **`EMENDA-04-01`**, **`EMENDA-04-02`**, **`EMENDA-04-03`** e **`EMENDA-04-04`** no
topo, que vencem o corpo onde discordarem. **Linha da fila:** `2026-09-07`,
"LEVA 04 — CATALOG ADMINISTRATION", ainda `aguardando`.

**Prova de leitura exigida pela `EMENDA-04-04`:** ocorrências da cadeia `EMENDA-04-04` neste
arquivo, contadas com `grep -c "EMENDA-04-04" Docs/relatorio-leva-04-etapa-2.md`: **6**.

**Nenhuma tela existe ainda.** Esta parada entrega três arquivos de teste e uma alteração num
quarto, todos sob `tests/`. `src/` aparece em **zero** linhas do `git diff --stat` — nenhum
`Program.cs`, nenhuma chave de recurso, nenhum CSS, nenhum registro de serviço, como a
`EMENDA-04-04` D3 delimita.

---

## 0. As três conferências pós-aplicação (`EMENDA-04-04` D2)

Rodadas antes de qualquer outra coisa, porque uma migration aplicada que ninguém conferiu é um
schema aceito no fio do bigode — e porque o shell da ponte não alcança o `OrlandoUpDb`, então este
é o único lugar onde elas podem ser medidas.

`SELECT DB_NAME()` colado antes da primeira consulta (D12); a string de conexão saiu do user-secret
por leitura programática e **não foi impressa**:

```
OrlandoUpDb
```

### 0.1 O histórico de migrations carrega três linhas, e a terceira é a desta leva

```
3
20260904233355_InitialCreate
20260906162133_AddIsBookableAndOptionalDimensions
20260908215113_RemoveActiveFlagStoreDefaultsAndAddAuditEntries
```

**Passa.** Eram 2 na §3 do relatório da etapa 1.

### 0.2 `AuditEntries` existe e aceita uma linha, que em seguida sai

```
tabela existe: sim
linhas antes: 0
linhas depois do INSERT: 1
linhas depois do DELETE: 0
```

**Passa.** A linha de sonda carregou `EntityType = 'Probe'` e `ActorEmail = 'probe@local'`, valores
que não colidem com nada, e foi removida pelos dois no `WHERE`. A tabela volta a zero linhas, que é
como a etapa 3 a encontra.

### 0.3 Nenhuma bandeira booleana carrega padrão de banco — nem `IsActive`, nem `IsBookable`

Mesma consulta da §7 do relatório da etapa 1, sobre as quatro tabelas:

```
DeliveryZones|SalesTaxRate|DF__DeliveryZ__Sales__3F466844|((0.0))
Products|TurnaroundDays|DF__Products__Turnar__4316F928|((0))
```

**Passa, e é a linha mais importante deste relatório.** A tabela da §7 da etapa 1 trazia **sete**
restrições; agora traz **duas**, e as duas que sobraram são de coluna **não booleana** —
`decimal` e `int`. Sumiram as quatro `IsActive` e sumiu a `DF__Products__IsBook__6E01572D`, que é
a que só existia porque o C1 a escreveu à mão: nenhum diff de modelo a teria produzido. **Cinco a
menos, exatamente as cinco.**

A D34 passa a ser verdadeira **no modelo e no banco**, que era o motivo de a `EMENDA-04-03` C1 ter
escolhido (b).

### 0.4 A correção datada da `EMENDA-04-04` D1, registrada aqui porque o git não a registra

A migration **está aplicada**. A seção `## Revisão` do relatório da etapa 1 diz que não está, o que
era verdade quando foi escrita; a §R.0 daquela seção já estabelece que o corpo é o registro do que
era verdade quando a parada foi pedida. Este relatório é onde o estado atual fica escrito, e as
§§0.1 a 0.3 são a medição dele.

---

## 1. Passo 0 desta etapa, medido em `ea3ad18`

### 1.1 Os identificadores que a etapa cria, esperado zero

Varridos sobre `src` e `tests` com `-I` e sem `bin`/`obj`:

| Identificador | Antes | Depois |
|---|---:|---|
| `TestAuthHandler` | **0** | mora em `tests/OrlandoUp.Tests/TestAuthHandler.cs` |
| `FormPoster` | **0** | mora em `tests/OrlandoUp.Tests/FormPoster.cs` |
| `AdminCrudTests` | **0** | mora em `tests/OrlandoUp.Tests/AdminCrudTests.cs` |
| `CatalogWriter` | **0** | continua **0** — é da etapa 3 |
| `AuditTrail` | **0** | continua **0** — é da etapa 3 |

### 1.2 A armadilha da `EMENDA-04-02` B2, reproduzida na árvore antes de escrever qualquer coisa

O B2 diz que o controle que a A8 pediu nasceu verde falso e que três coisas o consertam: `-I`,
`--exclude-dir=bin --exclude-dir=obj`, e trocar o nome `FormPost` por `FormPoster`. Reproduzi as
duas direções em `ea3ad18`, com o nome **antigo**, para não aceitar a explicação de segunda mão:

```
forma ingênua, nome antigo FormPost:                sim
   casa dentro de: Microsoft.AspNetCore.Components.Web.dll
                   Microsoft.Identity.Client.dll
                   Microsoft.IdentityModel.Protocols.dll
                   Microsoft.IdentityModel.Protocols.OpenIdConnect.dll
a mesma varredura com -I e sem bin/obj:             nao
```

Quatro DLLs da plataforma, sob `tests/OrlandoUp.Tests/bin/`, davam o `sim` antes de a costura
existir. **As duas correções funcionam de forma independente** — as bandeiras sozinhas já derrubam
o falso verde, e o nome novo sozinho também. O controle C12 leva as duas.

### 1.3 `quem-ancora` e `proibidos`

`quem-ancora` sobre os quatro arquivos desta etapa: **zero ancoragens**. Nenhum controle dos dois
`.tsv` existentes olha para `tests/` — é o que a §4 do plano já dizia, e vale medido. Os controles
C11 e C12 do `.tsv` novo serão os primeiros a ancorar ali.

`proibidos`: pela mesma razão, nenhum padrão negativo dos dois `.tsv` morde arquivo sob `tests/`.
Os quatro arquivos foram escritos assim mesmo — sem relógio local, sem coalescência de ausência em
zero, sem identificador de catálogo.

### 1.4 Duas afirmações da spec conferidas, e uma delas mede diferente

| Afirmação | Medido em `ea3ad18` | Veredicto |
|---|---|---|
| D2/04: não há configuração de antiforgery em lugar nenhum (`grep -rni antiforgery src tests`, fora de `obj/`) | **0** | confere — a aplicação roda no padrão do Razor Pages, que valida todo handler não-GET |
| D1/04: `grep -nE "Testing\|IsEnvironment" src/OrlandoUp.Web/Program.cs` devolve "apenas o `IsDevelopment()` da linha 138" | **nada**, zero linhas | **a afirmação que importa é mais forte do que a spec diz**, não mais fraca: `IsDevelopment` não casa com nenhum dos dois termos do comando, então o comando devolve vazio. Não há ramo de ambiente nenhum em `Program.cs`, e esta etapa não acrescenta nenhum |

---

## 2. As três peças do arreio (§8.1 da spec)

Três arquivos novos e um alterado, **todos sob `tests/`**, nenhum em `src/` — a D1/04 escrita como
arquivo e, com o C11/C12, como controle.

| Arquivo | O quê |
|---|---|
| `tests/OrlandoUp.Tests/TestAuthHandler.cs` | **novo** — o handler de autenticação de teste |
| `tests/OrlandoUp.Tests/FormPoster.cs` | **novo** — o auxiliar de antiforgery |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs` | **novo** — as três provas desta parada |
| `tests/OrlandoUp.Tests/SiteFactory.cs` | **alterado** — `ConfigureTestServices` e o cliente autenticado, 35 linhas acrescentadas e 0 removidas |

**Por que as provas moram em `AdminCrudTests.cs` e não num arquivo de nome próprio:** a §11.1 nomeia
esse arquivo e o plano contou o identificador dele em zero no passo 0. Um `AdminHarnessTests.cs`
seria um nome que nenhuma das duas coisas cobre. A etapa 3 acrescenta a este mesmo arquivo as
asserções da §8.2.

### 2.1 O handler, e as duas propriedades que importam mais que o código

**Autentica só quem traz o cabeçalho `X-Test-Staff`.** Sem ele devolve `NoResult`, e quem chamou
continua anônimo. É isso que mantém o cliente padrão anônimo e mantém
`SiteBehaviourTests.An_anonymous_visitor_to_the_administration_is_sent_to_the_login_page`
provando o portão da aplicação em vez de provar o arreio.

**Só o esquema de AUTENTICAÇÃO padrão é trocado; o de DESAFIO continua sendo o cookie do Identity.**
Registrado assim, em `ConfigureTestServices`, que roda depois de a aplicação ter registrado tudo:

```csharp
services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

services.Configure<AuthenticationOptions>(options =>
    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName);
```

O `AddIdentity` da linha 47 do `Program.cs` põe `Identity.Application` nos dois esquemas; este
`Configure` é o último a rodar e move **um** deles. Se movesse o de desafio, o anônimo levaria 401
seco em vez de 302 para `/admin/login`, e o teste do portão passaria a medir o arreio.

O principal carrega `ClaimTypes.Name`, `ClaimTypes.NameIdentifier` e `ClaimTypes.Role` com
`Roles.Admin` — o papel que a política `StaffOnly` do `Program.cs:80` exige. O nome é
`harness@orlandoup.test`, para que a asserção de auditoria da etapa 3 tenha o que ler.

**`Program.cs` não sabe de nada disso.** Nenhum ramo de ambiente foi acrescentado, e a §1.4 mede que
não havia nenhum antes.

### 2.2 O auxiliar de antiforgery

`FormPoster` faz `GET` da página, extrai o `__RequestVerificationToken` do HTML **devolvido** e
posta com ele. A antiforgery fica **ligada**; o cookie viaja porque o cliente do
`WebApplicationFactory` guarda cookie.

Ele **lança** quando a página não traz token, em vez de devolver cadeia vazia. Um auxiliar que
devolvesse vazio em silêncio transformaria toda asserção de "o token foi aceito" em teste de coisa
nenhuma — é a mesma família de defeito que a `Docs/regras-de-controle.md` chama de verde falso.

Tem também um `PostWithoutTokenAsync`, que existe **só** para a terceira prova. Sem ele a segunda
prova não teria como mostrar que mede alguma coisa.

### 2.3 Os dados da fixture — conferido, não suposto

A `SeedAsync()` do `SiteFactory` roda o `CatalogSeeder` sobre o `CatalogSeedData`, o mesmo par que
escreveu o `OrlandoUpDb`. As cardinalidades que a §8.2 vai ler:

| O que a etapa 3 lê | Onde está medido | Valor |
|---|---|---:|
| produtos | `SeedingTests.cs:40`, asserção que já passa | 7 |
| traduções de produto | `SeedingTests.cs:41` | 14 |
| adicionais | `SeedingTests.cs:42` | 6 |
| zonas | `SeedingTests.cs:43` | 4 |
| locais | `SeedingTests.cs:44` | 10 |
| unidades | `SeedingTests.cs:49` | 10 |
| produtos reserváveis / não reserváveis | `SeedingTests.cs:50-51` | 3 / 4 |
| **faixas de preço** | contadas no `CatalogSeedData.cs`: 3 + 3 + 2 nos três produtos reserváveis | **8** |
| **vínculos produto-adicional** | contados no `CatalogSeedData.cs`: 3 produtos × 4 códigos | **12** |

As duas últimas não são afirmadas por teste nenhum; contei-as na fonte, e as duas batem com o que o
`OrlandoUpDb` respondeu na §3 do relatório da etapa 1 — 8 e 12 —, que é o mesmo semeador rodando
sobre os mesmos dados. **A fixture é suficiente para tudo que a §8.2 vai ler.**

---

## 3. As três provas da P2

Quatro testes em `AdminCrudTests.cs`, e são um argumento só. **Todos passam.**

| # | Teste | O que afirma | Resultado |
|---|---|---|---|
| 1 | `An_authenticated_request_reaches_a_page_behind_the_administration_gate` | `GET /admin/products` com a costura → **200**, e o endereço que respondeu **é** `/admin/products` | passa |
| 2 | `The_same_page_without_the_seam_still_sends_the_visitor_to_the_login_page` | o mesmo `GET` **sem** o cabeçalho → **302** para `/admin/login` | passa |
| 3 | `A_post_carrying_the_antiforgery_token_is_accepted` | `POST /admin/logout` **com** o token → **302** para `/admin/login` | passa |
| 4 | `The_same_post_without_the_antiforgery_token_is_refused` | o mesmo `POST` **sem** o campo do token → **400** | passa |

**As três provas que a `EMENDA-04-01` exige são a 1, a 3 e a 4.** A 2 não estava na lista e está
aqui pelo mesmo motivo que a 4: as duas metades da mesma asserção. A 1 e a 2 são a **mesma
requisição** diferindo só no cabeçalho, e a 3 e a 4 são o **mesmo POST** diferindo só no campo do
token. Cada par mostra que o outro lado mede alguma coisa.

**A 1 tem meia asserção de presença dentro dela**, como a §8.2 exige de toda ausência: um 200 vindo
de uma cadeia de redirecionamentos que terminasse na tela de login também seria 200, então o teste
afirma que o caminho que respondeu é o pedido, e que a página traz o campo de antiforgery.

**A 4 lê o token antes de postar sem ele.** Sem essa leitura o cliente não teria o **cookie** de
antiforgery, e o 400 viria por falta de cookie e não por falta do campo — passaria pelo motivo
errado, que é o defeito que esta parada existe para não deixar entrar.

**O formulário usado é o de sair da sessão do `_AdminLayout.cshtml`**, que é o único POST
autenticado que existe antes de as telas de CRUD existirem. Ele é um
`<form method="post" asp-page="/Admin/Logout">` de verdade, que é a única forma na qual o Razor
injeta o campo oculto — se fosse markup à mão, a prova 3 falharia na leitura do token, não na
asserção.

### 3.1 Nada do que a §8.3 protege quebrou

`dotnet test OrlandoUp.sln --nologo -v q`: **142 passando, 0 falhando** — eram 138, e as quatro
novas são as desta parada. Passam, entre elas,
`SeoTests.cs:201`, `SeoTests.cs:88`, `CultureRoutingTests.cs:64`, `SiteBehaviourTests.cs:273`,
`SeedingTests.cs:31` e `ArchitectureTests.cs:32`, mais o teste do portão que o arreio poderia ter
apagado sem barulho.

`dotnet build OrlandoUp.sln --nologo -v q`: **limpo, 0 avisos**.

---

## 4. Controles

### 4.1 Os 35 existentes

```
bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv
18 controles, 0 fora do esperado, HEAD ea3ad18, árvore COM ALTERAÇÕES NÃO COMMITADAS.

bash Docs/medir-controles.sh verificar Docs/controles/public-site.tsv
17 controles, 0 fora do esperado, HEAD ea3ad18, árvore COM ALTERAÇÕES NÃO COMMITADAS.
```

Nenhum se deslocou.

### 4.2 A FORMA do comando do C05, não só o resultado (A7)

A `EMENDA-04-01` A7 pede que este relatório traga a forma, porque ancorar contagem de ponto de
entrada no **tipo de retorno** repete dentro do controle o buraco que a reflexão existe para fechar.
A âncora é o **nome** do handler:

```sh
a=$(grep -rhoE "OnPost[A-Za-z]*Async" --include=*.cs src/OrlandoUp.Web/Pages/Admin | wc -l; true)
c=$(grep -rhoE "[.]Record[(]"          --include=*.cs src/OrlandoUp.Web/Pages/Admin | wc -l; true)
echo $(( ${a:-0} - ${c:-0} ))
```

O operando de auditoria é `[.]Record[(]` e não `AuditTrail.Record(`, porque a A2 pôs o serviço em
DI e a chamada no handler será `_audit.Record(...)`. Medido agora:

| Operando | Valor |
|---|---:|
| A — handlers de POST sob `Pages/Admin` | **2** (`Login`, `Logout`) |
| B — registros de auditoria | **0** |
| **C05 = A − B** | **2** |

Os 2 são a permissão dos dois handlers de sessão. No fim da leva serão 6 e 4, e a diferença continua
2 — é invariante de relação, não de contagem, e por isso mede o mesmo antes e depois **de
propósito**. Quem prova que a frente mexeu nele é o irmão de alcance C06, que hoje mede `nao`
(A ≥ 6 é falso com A = 2) e só vira `sim` quando as quatro telas de escrita existirem.

### 4.3 Os controles que ESTA etapa move

| Controle | No plano | Antes da P2 | Depois da P2 | Alvo no fim |
|---|---|---|---|---|
| C11 D1-04 a costura de teste não existe em nenhum arquivo do produto | 0 | **0** | **0** | 0 |
| C12 ALCANCE de C11 os mesmos identificadores existem em ao menos um arquivo de teste | nao | **nao** | **sim** | sim |

**O par é a D1/04 virada em medição.** O C11 sozinho seria zero medindo zero — foi o que a
`EMENDA-04-02` B2 pegou. Com o C12 ao lado, o `0` do C11 passa a significar "a costura existe e não
está no produto", e não "a costura não existe". Os quatro arquivos que o C12 alcança são
`TestAuthHandler.cs`, `FormPoster.cs`, `SiteFactory.cs` e `AdminCrudTests.cs`.

Os demais controles do `.tsv` novo: o C01 e o C03 continuam em **0** (fechados na P1); o C05 em
**2** e o C06 em `nao` (§4.2); o C07 em **3** e o C08 em `nao`, que são das telas; o C09 em **0** e
o C10 em `sim`, que são negativo de escopo e já estão no alvo.

**O `Docs/controles/admin-catalog.tsv` ainda não desceu do `scratchpad/`.** A `EMENDA-04-03` C4
aceita isso desde que ele aterrisse **antes** do portão de fim de sessão, que verifica três
arquivos. Ele entra no commit de conteúdo da etapa 3.

---

## 5. O que a P2 não fez, e o que vem depois

**Não fiz, de propósito:**

- **nenhuma tela.** `Pages/Admin/` tem os mesmos cinco page models de antes;
- **nenhuma chave de recurso, nenhum CSS**;
- **nenhum registro em `Program.cs`.** As duas linhas que a A2 autoriza são da etapa 3;
- **nenhum serviço.** `CatalogWriter` e `AuditTrail` continuam medindo **0**;
- **nada empurrado.**

**A liberação que peço é a da etapa 3**: telas, recursos, CSS, os testes da §8.2 — inclusive o teste
da A3 (produto nasce oculto), o par da A1 (asserção sobre o modelo + comportamento) e o de realces
da A5 com `<`, `>` e `&` —, o `.tsv` novo descendo para `Docs/controles/`, e o comentário de
`ProductConfiguration.cs:28-34` reescrito, que a `EMENDA-04-03` C2 autorizou e que é a única coisa
fora da §11.1 original a entrar.
