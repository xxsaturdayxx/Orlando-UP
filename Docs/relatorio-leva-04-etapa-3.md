# Relatório — leva 04, etapa 3: as telas que escrevem (parada P3)

**Data:** 2026-09-08. **HEAD ao escrever:** `b523c57`. **Spec:** `Docs/spec-04-admin-catalog.md`
com as notas **`EMENDA-04-01`** a **`EMENDA-04-05`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-07`, "LEVA 04 — CATALOG ADMINISTRATION", ainda `aguardando`.

**Prova de leitura exigida pela `EMENDA-04-05`:** ocorrências da cadeia `EMENDA-04-05` neste
arquivo, contadas com `grep -c "EMENDA-04-05" Docs/relatorio-leva-04-etapa-3.md`: **6**.

**Tudo escrito, suíte verde, os três `.tsv` verificados.** O que falta é a conferência visual, que
é sua, na sua máquina. Nada foi empurrado.

| Portão | Valor |
|---|---|
| `dotnet build OrlandoUp.sln --nologo -v q` | **limpo, 0 avisos** |
| `dotnet test OrlandoUp.sln --nologo -v q` | **168 passando, 0 falhando** (eram 142 na P2, 138 na P1) |
| `Docs/controles/foundation.tsv` | **18, 0 fora do esperado** |
| `Docs/controles/public-site.tsv` | **17, 0 fora do esperado** |
| **`Docs/controles/admin-catalog.tsv`** | **12, 0 fora do esperado** — desceu do `scratchpad/` neste commit |

---

## 1. O E1 está feito, e é a primeira coisa desta etapa

A `EMENDA-04-05` E1 pediu que a asserção da prova 3 fosse exata em vez de por conteúdo, porque o
redirecionamento de autorização que um POST **não** autenticado produz também é um `Found` cujo
`Location` contém `/admin/login`. A linha nova, com o motivo escrito ao lado dela e não num
auxiliar:

```csharp
Assert.Equal("/admin/login", response.Headers.Location?.OriginalString);
```

`LogoutModel.OnPostAsync` devolve `RedirectToPage("/Admin/Login")`, cujo `Location` é `/admin/login`
e mais nada; o desafio do cookie acrescenta `?ReturnUrl=%2Fadmin%2Flogout`. A igualdade separa os
dois; a continência aceitava os dois. **O teste continua verde**, o que era esperado — o revisor já
tinha medido que ele não passava pelo motivo errado —, mas agora a discriminação está no corpo do
teste, onde um afrouxamento futuro dela apareceria.

**E2 — o erro da §3 da spec fica registrado como do revisor, não meu.** A §1.4 do relatório da
etapa 2 já trazia a medição; a `EMENDA-04-05` E2 a aceita e nomeia o padrão: três itens (A6, A8,
E2) em que uma medição foi parafraseada em vez de refeita. Nada a corrigir no código.

---

## 2. O que a etapa escreveu

### 2.1 Serviços — `Infrastructure/Data/`, com escopo em DI (A2)

| Arquivo | O quê |
|---|---|
| `CatalogWriter.cs` | tudo o que a administração escreve no catálogo: achar para editar, checar unicidade de endereço e de etiqueta, criar produto e unidade, aplicar tradução, substituir faixas e vínculos, e a serialização dos destaques |
| `AuditTrail.cs` | `Record(...)`, que **encena** a linha, e `RecentAsync(...)`, que lê as 100 mais recentes |

Os dois são **classes de instância com escopo**, espelhando `CatalogQueries` (`Program.cs:111`), e
não estáticas: uma estática segurando um `DbContext` estaria segurando a requisição de outra
pessoa. Moram em `Infrastructure/Data/` e não em `Application/`, que é o que
`ArchitectureTests.The_application_layer_knows_nothing_about_infrastructure_or_pages` guarda.

`AuditTrail.Record` **não salva**: quem salva é o handler. Uma escrita que falha não deixa linha
dizendo que aconteceu.

**Duas salvas e uma transação nos dois handlers de criação.** Uma linha nova não tem chave até ser
escrita, e a linha de auditoria precisa nomear a chave. Então: salva a linha, encena a auditoria com
a chave que ela passou a ter, salva de novo — as duas dentro de uma transação, para que uma falha
entre elas não deixe um produto que registro nenhum explica.

### 2.2 As sete rotas, todas em minúsculas e escritas à mão na diretiva `@page`

```
@page "/admin"                          (existia)
@page "/admin/audit"                    NOVA
@page "/admin/language/{culture}"       (existia)
@page "/admin/login"                    (existia)
@page "/admin/logout"                   (existia)
@page "/admin/products"                 (existia, ganhou editar e novo)
@page "/admin/products/create"          NOVA
@page "/admin/products/edit/{id:int}"   NOVA
@page "/admin/units"                    NOVA
@page "/admin/units/create"             NOVA
@page "/admin/units/edit/{id:int}"      NOVA
```

**Seis handlers de POST e quatro registros de auditoria**, um por handler de escrita, medido:

```
Login.cshtml.cs:36            OnPostAsync
Logout.cshtml.cs:19           OnPostAsync
Products/Create.cshtml.cs:51  OnPostAsync   ->  :85   _audit.Record(
Products/Edit.cshtml.cs:148   OnPostAsync   ->  :205  _audit.Record(
Units/Create.cshtml.cs:51     OnPostAsync   ->  :103  _audit.Record(
Units/Edit.cshtml.cs:71       OnPostAsync   ->  :114  _audit.Record(
```

6 − 4 = **2**, que é a permissão dos dois handlers de sessão e o valor do controle C05.

### 2.3 As decisões que a spec e as emendas fixaram, e onde cada uma está no código

| Decisão | Onde |
|---|---|
| **A9** — criar pede só o mínimo e redireciona para o editor | `Products/Create.cshtml.cs:110` |
| **A3** — o produto nasce **oculto** | `CatalogWriter.AddProduct`, `IsActive = false` explícito; o inicializador `= true` do C# fica onde estava (C03 = 0) |
| **A4** — `UpdatedAtUtc` escrito em todo save de produto, do `IClock` | `CatalogWriter.TouchProduct`, chamado em `Products/Edit.cshtml.cs:203` |
| **A10 K1** — o produto da unidade é editável e a auditoria nomeia os dois | `Units/Edit.cshtml.cs:114-119` |
| **A10 K2** — `/admin/audit` mostra as 100 mais recentes, sem filtro | `AuditTrail.RecentCount` |
| **A10 K3** — nada é salvo e o formulário volta inteiro com o problema nomeado | `Products/Edit.cshtml.cs:162-169`; o Razor re-renderiza das propriedades ligadas, então a digitação volta |
| **D3/04** — nenhuma marcação de admin ramifica por cultura; nenhum parcial em `Pages/Shared/` | o editor mostra as duas traduções lado a lado, sem condição; a barra mora dentro do `_AdminLayout.cshtml` |
| **D6/04** — realces entram e saem como uma linha por item; JSON cru nunca aparece | `CatalogWriter.SplitLines` / `ReadHighlights` |
| **D8/04** — a regra de preço não é reimplementada | `PricingTierRules.Validate` sobre o conjunto proposto inteiro |
| **D11/04** — o nome em inglês é obrigatório | `Products/Edit.cshtml.cs:229` |
| **D15 / C17** — ausência é valor de primeira classe | os campos são `decimal?`/`int?` do formulário até a coluna; **nenhuma coalescência em nenhum ponto**, e o C17 mede 0 |
| **D16** — `OccurredAtUtc` e `CreatedAtUtc` do `IClock`; `PurchasedOn` é data de calendário | `AuditTrail`, `CatalogWriter`; o C06 continua prendendo a leitura real num arquivo só |

### 2.4 Um defeito que eu escrevi e que o teste pegou, e vai escrito porque ele é fácil de repetir

O par caixa-de-marcação + campo oculto que carrega um `bool` **tem ordem**, e eu a inverti: pus o
oculto antes da caixa. O ligador lê o **primeiro** valor de um nome repetido, então toda caixa
marcada chegava como `false` — o editor teria despublicado silenciosamente todo produto salvo.
Nenhuma tela reclama disso; o que pegou foi o teste do nome em branco vindo com a mensagem errada.
A ordem certa é a que o *tag helper* da plataforma usa: **caixa primeiro, oculto depois**, e o
motivo está num comentário ao lado dos dois campos em `Products/Edit.cshtml`.

### 2.5 Recursos

**100 chaves novas nos dois `.resx`, 1 removida.** `Admin_ProductsReadOnly` sai dos dois — a tela
deixou de ser somente leitura, e a remoção é simétrica ou a parity test cai. Os dois arquivos passam
de 163 para **262** entradas, e o prefixo `Admin_` de 23 para **122** chaves em cada um.

**Nada de valor vazio, e os dois conjuntos de chaves são idênticos** — `LocalizationParityTests`
mede isso e o C12 de `foundation.tsv` mede de novo a partir dos arquivos.

**As chaves que as telas montam por interpolação ganharam teste próprio.** `Admin_Status{...}`,
`Admin_Seat{...}`, `Admin_Tier{...}` e `Admin_Action{...}` são montadas com o nome do membro do
enum, e a parity test — que compara os dois arquivos **entre si** — não veria uma que ninguém
escreveu: faltaria nos dois. O teste
`Every_enum_member_a_screen_names_has_a_key_in_both_cultures` percorre os 13 membros e afirma que
nenhum cai em `ResourceNotFound`.

**Uma chave ficou órfã e eu não a removi:** `Admin_BackToDashboard` mede **0** usos depois que a
navegação virou barra (D9/04). Tirá-la seria uma segunda remoção, e a §11.1 autoriza **uma**. Ela
não quebra nada — vai como candidata a `Docs/backlog-conhecido.md`, que está na lista negativa desta
frente.

### 2.6 CSS

Só a seção Administração, e só controles que não existiam: `select`, `textarea`, caixa de marcação
(com alvo de toque de 1,25rem e rótulo que continua rótulo), a linha repetida da faixa de preço, o
aviso de salvo e a barra de navegação. **Nenhum contorno de foco removido** (C12 de `public-site`
mede 0), **nenhum hex aposentado**, **nenhum token v1**, **nenhuma família aposentada** — os três
medem 0 e os três irmãos de alcance continuam `sim`.

---

## 3. O negativo de escopo da A2, medido contra a FAIXA de commits

A regra é da própria A2: negativo de execução se mede contra a faixa, nunca contra a árvore, e
expira com a frente — por isso ele mora aqui e **não** no `.tsv` permanente.

```
git diff --numstat bc8b762 -- src/OrlandoUp.Web/Program.cs
2       0       src/OrlandoUp.Web/Program.cs
```

E as duas linhas, coladas inteiras:

```diff
+builder.Services.AddScoped<CatalogWriter>();
+builder.Services.AddScoped<AuditTrail>();
```

**Exatamente 2 acrescentadas, 0 removidas, as duas registro de serviço.** Qualquer outra coisa
naquele arquivo seria parada.

### 3.1 O resto da lista negativa, sobre a mesma faixa

Rodado arquivo por arquivo com `git diff --numstat bc8b762 -- <arquivo>`: **`CatalogSeedData.cs`,
`CatalogSeeder.cs`, `CatalogQueries.cs`, `Application/Catalog/CatalogViews.cs`,
`TranslationPicker.cs`, `RichText.cs`, `PricingTierRules.cs`, `Api/SitemapEndpoints.cs`, os quatro
arquivos de `Infrastructure/Localization/`, `appsettings.json`, `CLAUDE.md`,
`Docs/architecture.md`, `Docs/roadmap.md`, `Docs/open-questions.md`, `Docs/market-notes.md`,
`Docs/backlog-conhecido.md`, `Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`,
`Docs/medir-controles.sh`, `Docs/controles/foundation.tsv`, `Docs/controles/public-site.tsv`,
`.githooks/pre-commit`, `.gitattributes`, `.github/workflows/ci.yml` e `Docs/fila-cc.md` aparecem
em ZERO linhas.**

**Dois arquivos da lista negativa aparecem na faixa, e nenhum deles é meu.**
`Docs/decisions.md` e `Docs/spec-04-admin-catalog.md` foram tocados por três commits, e os três são
do revisor:

```
b523c57 Claude Web  docs: P2 aceita e etapa 3 liberada - EMENDA-04-05
ea3ad18 Claude Web  docs: C1 aceito e P2 liberada - EMENDA-04-04
4369e27 Claude Web  docs: revisao da P1 - EMENDA-04-03 ... e D35
```

`git log --oneline bc8b762..HEAD -- Docs/decisions.md Docs/spec-04-admin-catalog.md` devolve esses
três e mais nenhum. **Nenhum commit meu toca os dois.**

### 3.2 Um arquivo de teste que a §11.1 não nomeava, e o que eu fiz com ele

As asserções da §8.2 nasceram em `AdminCatalogTests.cs`, que **não** está na §11.1 — ela nomeia
`AdminCrudTests.cs` "e os arquivos de arreio da §8.1", e o negativo diz que todo arquivo de teste
fora dos dois modificados aparece em zero linhas. Em vez de pedir emenda para um nome, **dobrei o
conteúdo para dentro de `AdminCrudTests.cs`**, que a §11.1 nomeia: o arquivo passa a ter as três
provas do arreio e, abaixo delas, as asserções das telas. `AdminCatalogTests.cs` não existe.

**Um arquivo novo de teste continua fora da lista literal e eu o declaro em vez de escondê-lo:**
`tests/OrlandoUp.Tests/FormFields.cs`. Ele é a outra metade da peça 2 da §8.1 — o auxiliar de
antiforgery —, separada em arquivo próprio porque este projeto escreve um tipo por arquivo. Ele lê
os campos que o formulário **realmente renderizou** em vez de deixar cada teste digitar os nomes,
que é o que faz "o editor salvou o que recebeu" querer dizer alguma coisa: um campo que a página
parar de renderizar some do POST também, e o teste que dependia dele cai. Se você preferir, ele se
funde em `FormPoster.cs` num commit de ajuste.

---

## 4. O que a leva prova (§8.2), item por item

Todos verdes. **26 testes novos**; a suíte vai de 142 para **168**.

| § | O que | Teste |
|---|---|---|
| **A1 forma** | as quatro bandeiras não declaram padrão de banco **e o sentinela voltou ao neutro** | `No_visibility_flag_declares_a_store_default_and_every_sentinel_is_neutral` |
| **A1 comportamento** | um `false` explícito sobrevive à ida e volta nas quatro | `An_explicit_false_survives_the_round_trip_on_all_four_flags` |
| **A3** | o produto criado pelo POST lê `IsActive = false` | `A_product_created_through_the_screen_is_born_hidden` |
| **A4** | salvar um produto carimba o instante da edição | `Saving_a_product_stamps_the_moment_of_the_edit` |
| **A5** | realces voltam inteiros, com aspas, contrabarra **e `<`, `>`, `&`** | `Highlights_round_trip_as_lines_and_the_awkward_characters_come_back_intact` |
| 8.2/2 | o editor escreve o que recebeu e a `/rentals` lê | `The_editor_writes_what_it_was_given_and_the_public_page_reads_it` |
| 8.2/3 | dimensão ausente sobrevive e o badge não aparece | `An_absent_dimension_survives_the_round_trip_and_the_badge_stays_off` |
| 8.2/4 | **um teste por membro de `PricingTierSetProblem`** — seis | `A_product_on_sale_is_refused_with_a_broken_price_list` (Theory, 6 casos) |
| 8.2/5 | o nome em inglês não pode ser apagado | `The_english_name_cannot_be_blanked` |
| 8.2/6 | etiqueta duplicada é recusada como validação, e outra salva | `A_duplicate_asset_tag_is_refused_as_validation_and_a_different_one_saves` |
| 8.2/7 | **exatamente uma** linha de auditoria por handler de escrita | quatro testes, um por handler |
| 8.2/8 | o produto oculto continua na lista da administração | `A_hidden_product_stays_on_the_administration_list_and_leaves_the_public_one` |
| 8.2/9 | realces entram e saem como linhas | o mesmo da A5 |
| — | endereço duplicado recusado, e um livre salva | `A_duplicate_slug_is_refused_by_name_and_a_free_one_saves` |
| — | bloco em português todo em branco remove a linha de tradução | `A_portuguese_block_left_entirely_blank_removes_the_translation_row` |
| — | produto fora de venda é recusado com faixa de preço | `A_product_that_is_not_on_sale_is_refused_a_price_band` |
| — | a tela de registro mostra a linha que uma escrita deixou | `The_record_screen_shows_the_line_a_write_left` |
| — | toda chave montada por interpolação existe | `Every_enum_member_a_screen_names_has_a_key_in_both_cultures` |

**Toda asserção de ausência afirma uma presença no mesmo método**, como a §8.2 manda: o badge
ausente vem com o comprimento que ficou; o produto oculto que some de `/rentals` vem com o mesmo
endereço presente em `/admin/products`; as quatro colunas que leem `false` vêm com uma quinta linha
que ainda lê `true`; a auditoria é `Assert.Single` e não "ao menos uma", porque um handler que
registrasse duas vezes estaria tão errado quanto um que não registrasse.

### 4.1 A frase da A5, que o relatório tem de dizer em uma frase

**Esta leva torna `Name` e `Tagline` editáveis por tela pela primeira vez, e a guarda que os protege
está na serialização — o codificador de `StructuredData.cs` chama `ForbidCharacters('<','>','&')`
depois de `AllowRange(UnicodeRanges.All)` —, então ela vale seja qual for a origem do texto.** Os
realces, que o teste da A5 carrega com os três caracteres, nunca chegam ao bloco JSON-LD: só `Name`
e `Tagline` chegam.

### 4.2 Nada do que a §8.3 protege quebrou

`SeoTests.cs:201`, `SeoTests.cs:88`, `CultureRoutingTests.cs:64`, `SiteBehaviourTests.cs:273`,
`SeedingTests.cs:31` e `ArchitectureTests.cs:32` passam. Nenhum endereço foi acrescentado a
`SiteBehaviourTests.PublicPathList` (C09 = 0), nada liga `/admin` do leiaute público, esta leva não
registra remetente nenhum, o `CatalogSeedData` não foi tocado, e o serviço de escrita ficou em
`Infrastructure/Data/`.

---

## 5. Os controles

### 5.1 Os 47, verificados com a árvore desta etapa

```
Docs/controles/foundation.tsv      18 controles, 0 fora do esperado
Docs/controles/public-site.tsv     17 controles, 0 fora do esperado
Docs/controles/admin-catalog.tsv   12 controles, 0 fora do esperado
```

### 5.2 Os doze novos, do início ao fim da leva

| Controle | No plano | Na P1 | Na P2 | Agora | Alvo |
|---|---|---|---|---|---|
| C01 nenhuma coluna booleana declara padrão de banco, nas duas direções | 4 | **0** | 0 | **0** | 0 |
| C02 ALCANCE de C01, ao menos oito arquivos de configuração | sim | sim | sim | **sim** (11) | sim |
| C03 RELAÇÃO toda bandeira mantém o inicializador do C# | 0 | 0 | 0 | **0** | 0 |
| C04 ALCANCE de C03, operando maior ≥ 4 | sim | sim | sim | **sim** | sim |
| C05 RELAÇÃO POST menos auditoria é a permissão dos dois de sessão | 2 | 2 | 2 | **2** (6 − 4) | 2 |
| C06 ALCANCE de C05, operando maior ≥ 6 | nao | nao | nao | **sim** | sim |
| C07 a chave que dizia que a tela só lê sumiu de `src` | 3 | 3 | 3 | **0** | 0 |
| C08 ALCANCE de C07, mais de trinta chaves com o prefixo | nao | nao | nao | **sim** (122) | sim |
| C09 nenhum endereço de admin na lista pública digitada | 0 | 0 | 0 | **0** | 0 |
| C10 ALCANCE de C09, ao menos vinte endereços | sim | sim | sim | **sim** | sim |
| C11 a costura de teste não existe em nenhum arquivo do produto | 0 | 0 | **0** | **0** | 0 |
| C12 ALCANCE de C11, a costura existe em ao menos um arquivo de teste | nao | nao | **sim** | **sim** | sim |

**Seis mediram, em algum momento, valor diferente do alvo** — C01, C06, C07, C08, C12 e o operando
maior do C05. Nenhum nasceu verde por construção, que é o que a `Docs/regras-de-controle.md` cobra.
Os três que medem o mesmo antes e depois de propósito (C05, C09, C11) têm irmãos de alcance que se
moveram (C06, C10 e C12) — o C10 é o único que não se moveu, e ele é negativo de escopo, cujo alcance
já estava satisfeito antes.

**Nenhum controle dos dois `.tsv` existentes se deslocou**, o que é a condição que a §11.1 impõe.

---

## 6. O que falta, e é seu

1. **A conferência visual da §9 da spec**, na sua máquina, em `https://localhost:7420`, com a
   migration já aplicada. As sete rotas, o editor inteiro, a recusa de preço, a lista de frota, o
   registro, a barra de navegação, o contraste e o foco visível. O resultado vai em
   `Docs/conferencia-leva-04.md` — **inclusive o que não der para alcançar, com o motivo medido**.
   Eu não criei esse arquivo: ele é o seu registro, não o meu.
2. **O fechamento em dois commits**, depois da conferência: o de conteúdo — este — e o de
   fechamento, que grava o hash curto dele na coluna Commit desta linha da fila e passa o Estado
   para `concluido`. Saldo: de 1 linha `aguardando` para 0; o total de linhas da tabela não muda.
3. **O push, que é seu.** `origin/main...main` continua atrás dos commits desta conversa.

E três coisas pequenas que ficam de resto, nenhuma delas bloqueando:

- `Admin_BackToDashboard` está órfã (§2.5);
- `FormFields.cs` é o arquivo de teste que declaro fora da lista literal (§3.2);
- o `Docs/backlog-conhecido.md` está na lista negativa desta frente, então as duas linhas acima não
  entraram nele por mim.
