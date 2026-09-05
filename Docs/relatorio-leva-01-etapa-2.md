# Relatório — leva 01, etapa 2: E3, E4 e E5 fechadas (parada P1)

**Data:** 2026-09-04. **HEAD ao escrever:** `e65b581`. **Spec:** `Docs/spec-01-foundation.md` com a
nota datada de 2026-09-04 e o acréscimo "P2 review" do commit `e65b581`, que aprovou a migration
`20260904233355_InitialCreate` sem alteração e registrou `SiteLocalizationOptions`, o
`--format sln` e o BOM como desvios aceitos.

**O que está commitado neste commit: só este relatório.** Todo o código de E1 a E5 continua na
árvore de trabalho, **não rastreado**, esperando as duas user-secrets e a autorização de commit.

**Onde eu parei, e por quê:** em **P1**, imediatamente antes de `dotnet ef database update`.
Nenhum banco foi tocado nesta rodada: nenhuma migration foi aplicada, nenhum seed foi executado
contra o LocalDB, nenhuma connection string existe em lugar nenhum. Nada foi empurrado.

---

## 1. O que esta rodada entregou

| Etapa | O que entrou | Fecha quando | Estado |
|---|---|---|---|
| **E3** | convenção de rota de cultura, provedor de cultura, os dois `.resx`, `Program.cs` inteiro, `/healthz`, `robots.txt`, página de erro | build limpo | **fechada** |
| **E4** | layout, sete páginas públicas, quatro páginas de administração, `site.css` com os tokens, Nunito self-hosted, três ilustrações SVG, o `meta generator` com a marca de árvore suja | build limpo | **fechada** |
| **E5** | `seed-catalog`, `seed-admin`, e os sete grupos de teste da §7 da spec | suíte verde | **fechada** |

**Os números, medidos e não digitados:**

| Medida | Valor |
|---|---|
| arquivos novos na árvore | **111** |
| `dotnet build OrlandoUp.sln` | **0 erros, 0 avisos** |
| `dotnet test OrlandoUp.sln` | **63 testes, 63 passaram, 0 falharam** |
| classes de teste | 9 |
| controles do `.tsv` no valor final | **18 de 18** |
| chaves de recurso, iguais nas duas culturas | 145 |
| carimbo de build lido do assembly | `1.0.0+e65b581+dirty` |

---

## 2. As quatro correções que esta rodada tinha de responder, com arquivo e linha

### Correção 2 — o fail-fast da connection string, contornável pelo host de teste

**A recusa, no `Program.cs`:** `src/OrlandoUp.Web/Program.cs:32-41`. A linha 32 lê
`ConnectionStrings:DefaultConnection` da configuração; a 34 testa; a 36 lança, e a mensagem da
linha 38 **nomeia o comando `dotnet user-secrets set` sem nunca conter um valor**. Só depois disso,
na linha 45, o provedor de SQL Server é registrado com a string lida.

**O contorno, no host de teste:** `tests/OrlandoUp.Tests/SiteFactory.cs:45` injeta a chave por
`UseSetting` com um valor que **nunca é discado**; as linhas 52-63 removem do contêiner tudo o que
a aplicação registrou a respeito das opções do contexto, e a linha 66 põe SQLite em memória no
lugar. O fail-fast continua valendo para o processo real e o teste passa por cima dele pela porta
da frente.

**Uma coisa que só apareceu na execução, e que a nota não podia prever:** remover
`DbContextOptions<AppDbContext>` **não basta** nesta versão do EF Core. O provedor escolhido pela
aplicação chega ao contêiner como um registro de *configuração* de opções, e deixá-lo para trás
punha dois provedores no mesmo contexto — o EF recusa, com `Only a single database provider can be
registered`. A remoção agora varre os descritores por tipo de serviço (linhas 52-57). Está
comentado no arquivo para não ser "simplificado" de volta.

### Correção 6 — a ordem do middleware, escrita com o motivo

`src/OrlandoUp.Web/Program.cs:124-140` é o comentário que enumera os sete passos e diz por que cada
um está onde está. A ordem executada logo abaixo: `app.UseRouting()` na linha **156**,
`app.UseRequestLocalization()` na **158** — depois do roteamento, porque o provedor de cultura lê um
**route value** que não existe antes de a rota casar — e então `app.UseAuthentication()` na **160**
e `app.UseAuthorization()` na **161**, depois da localização, para que o que a autorização devolve
já saia na língua do visitante.

### Correção 7 — o link para o inglês sobrescreve o valor ambiente

`src/OrlandoUp.Web/Pages/Shared/_Layout.cshtml:86` emite `asp-route-culture=""` explicitamente, e o
comentário das linhas **80-81** diz por quê. O link para o português, na linha **92**, emite
`asp-route-culture="pt"`. Os dois lados escritos, nenhum implícito.

**E aqui a nota estava mais certa do que o plano.** O plano §5.3 apostava que os **outros** links de
uma página `/pt/...` seguiriam sozinhos, pelo valor ambiente do gerador de URL, e dizia que o teste
§7.3 seria quem descobriria. Descobriu: o teste
`Every_internal_link_of_a_Portuguese_page_stays_under_the_prefix` reprovou com **seis** endereços
escapando para o inglês (`/how-it-works`, `/faq`, `/contact`, `/rentals`, `/rentals/<slug>` e mais).
O motivo é que um valor ambiente viaja para a **mesma** página, não para outra. A saída declarada
no plano foi executada: **todo link interno agora diz qual cultura quer**, por
`src/OrlandoUp.Web/Pages/CultureLink.cs`, e o `Current` dele devolve o prefixo ou a cadeia vazia —
que é uma resposta de verdade, significando "sem prefixo". O teste passa e a troca está relatada
aqui, como o plano prometeu.

### Correção 9 — as duas lacunas decididas

**(a) Páginas públicas listam só o que está ativo.**
`src/OrlandoUp.Web/Infrastructure/Data/CatalogQueries.cs:29` filtra a lista de cartões, a **65**
filtra a página de produto, a **96** deixa de fora um adicional desativado e a **134** filtra as
zonas. Um produto escondido devolve **404** em `src/OrlandoUp.Web/Pages/Rentals/Details.cshtml.cs:26`,
com a mesma resposta que um produto inexistente — de propósito: distinguir os dois vazaria o
catálogo do que não está à venda. A administração continua vendo tudo
(`Pages/Admin/Products/Index.cshtml.cs`), que é o motivo de o esconder ser um sinalizador e não um
apagar.

**(b) Tradução faltando cai para o inglês, nunca 404.**
`src/OrlandoUp.Web/Application/Catalog/TranslationPicker.cs:29` é a segunda varredura: não achando a
cultura pedida, procura a linha `en-US`. A regra e o motivo estão no comentário das linhas 5-11. Só
quando **nem** a linha inglesa existe é que não há o que mostrar, e aí a decisão volta para quem
chamou.

---

## 3. O que a execução descobriu, além do previsto

### 3.1 O localizador procurava o recurso pelo nome do assembly, e a página mostrava a CHAVE

O achado mais sério desta rodada, e ele não fazia barulho nenhum. O projeto tem
`AssemblyName` `OrlandoUp.Web` e `RootNamespace` `OrlandoUp`. Os arquivos `.resx` são embutidos com
o nome do **root namespace** (`OrlandoUp.Resources.SharedResource`), mas o localizador do framework,
quando o assembly não declara nada, monta o nome que vai procurar a partir do **nome do assembly**.
Resultado: ele procurava `OrlandoUp.Web.Resources.OrlandoUp.SharedResource`, não achava, e fazia
exatamente o que foi projetado para fazer — **imprimir a chave**.

O site subia. Toda página devolvia 200. E o título da página de catálogo era, literalmente,
`Rentals_Title`.

**A correção** é uma linha: `[assembly: RootNamespace("OrlandoUp")]`, em
`src/OrlandoUp.Web/SharedResource.cs`, com o comentário explicando o porquê ao lado.

**A lição, que virou teste:** o teste de paridade dos `.resx` **não pega isso** — ele lê os dois
arquivos do disco e os dois estavam perfeitos. O que pegou foi a asserção do §7.3 que exige uma
palavra portuguesa de verdade na página. Acrescentei
`tests/OrlandoUp.Tests/RenderedTextTests.cs`: ele lê **as 145 chaves** do `.resx` inglês e afirma
que **nenhuma delas aparece no corpo** de 14 endereços, nas duas culturas. Uma chave impressa numa
página passa a ser vermelho, e não mais um detalhe que só um olho humano notaria.

### 3.2 `LocalizationOptions` continua sendo `SiteLocalizationOptions`

Confirmado pela nota. O arquivo é `src/OrlandoUp.Web/Application/SiteLocalizationOptions.cs` e o
comentário de classe registra o motivo (o framework já é dono do nome `LocalizationOptions`).

### 3.3 O template do Razor não deixou dívida

`wwwroot/lib/` e o parcial de scripts de validação já tinham saído na etapa 1; nesta rodada o
`_Layout.cshtml` foi **reescrito por inteiro**, então a referência pendurada ao Bootstrap que o
relatório anterior registrou **deixou de existir**. Não há JavaScript nenhum no site: a pasta
`wwwroot/js/` foi removida, e a única interação é HTML e CSS.

### 3.4 Nunito veio dos arquivos de verdade

`wwwroot/fonts/` tem `nunito-latin.woff2` (39.128 bytes), `nunito-latin-ext.woff2` (35.588 bytes) e
`OFL.txt` (4.385 bytes), baixados hoje e conferidos como WOFF2 de verdade (`Web Open Font Format
(Version 2), TrueType`). São dois subsets de peso variável 400–700 — não um arquivo por peso — e
cobrem todos os acentos do português. O `@font-face` que os declara está em `site.css`, com o
`unicode-range` de cada um.

### 3.5 O carimbo de build funciona, e a marca de árvore suja é a prova (K5, decisão a)

O alvo MSBuild acrescentado ao `csproj` chama `git rev-parse --short HEAD` e `git status
--porcelain` e alimenta `SourceRevisionId`. Lido do assembly compilado agora:
`1.0.0+e65b581+dirty`. O layout emite `<meta name="generator" content="OrlandoUp e65b581+dirty">`.
O sufixo só aparece porque o build veio desta árvore de trabalho, com alteração pendente — que é
precisamente o que a conferência §8 precisa provar.

---

## 4. O que existe agora, por camada

**`Domain/` (13 arquivos)** — 7 enums com valor numérico explícito, 10 entidades,
`PricingTierRules` e o enumerado dos problemas que ela devolve.

**`Application/` (9 arquivos)** — `IClock`, `RichText` (a única porta dos dois pacotes de texto
rico), `CompanyOptions`, `SeoOptions`, `SiteLocalizationOptions`, `Roles`,
`AuthorizationPolicies`, `SiteCultures`, `MoneyFormat`; e em `Application/Catalog/` os registros de
leitura e o `TranslationPicker`.

**`Infrastructure/`** — `SystemClock` (o único arquivo que lê o relógio real, e C06 afirma isso pelo
caminho), `BuildInfo`, `Data/` com o contexto, a fábrica de tempo de design, as 10 configurações e
a migration, `Localization/` com a convenção de rota, o provedor de cultura e o cálculo dos
endereços gêmeos, e `Seeding/` com os dois comandos e os dados de exemplo.

**`Pages/`** — sete páginas públicas (`/`, `/rentals`, `/rentals/{slug}`, `/how-it-works`, `/faq`,
`/contact`, `/privacy`, `/terms`), a página de erro `/error/{code}`, e quatro de administração
(`/admin/login`, `/admin`, `/admin/products`, `/admin/logout`, mais o trocador de idioma por
cookie). Cada página pública ganha a gêmea sob `/pt` pela convenção; a administração fica de fora
dela e lê cookie (D4/01).

**`Api/`** — `/healthz` e `/robots.txt`, e nada mais.

**Acessibilidade (D9)**, o que está no código: link de pular para o conteúdo como primeiro elemento
focável; `:focus-visible` com contorno de 3 px que nunca é removido; todo campo com `label`
associado; nenhuma cor sozinha carregando significado (o selo de transporte sempre traz as
palavras); tabelas com `th scope`; a de preços dentro de um contêiner que rola sozinho; 18 px de
corpo a partir de 768 px; e `prefers-reduced-motion` respeitado. Os pares de contraste e suas
razões medidas estão no cabeçalho do `site.css`.

---

## 5. Os testes, grupo a grupo (§7 da spec)

| # da spec | O que exige | Onde | Testes |
|---|---|---|---|
| 1 | arquitetura por camada, e os dois pacotes por um tipo só | `ArchitectureTests.cs` | 3 |
| 2 | paridade dos `.resx`, lida do disco, sem valor vazio | `LocalizationParityTests.cs` | 4 |
| 3 | roteamento por cultura, `/es` 404, links presos ao prefixo | `CultureRoutingTests.cs` | 6 |
| 4 | domínio: selo de transporte, faixas, preço a partir de | `DomainTests.cs` | 13 |
| 5 | relógio, nos dois lados da virada do horário de verão | `ClockTests.cs` | 4 |
| 6 | guardas dos dois comandos de semeadura | `SeedingTests.cs` | 6 |
| 7 | `/healthz`, `robots.txt`, portão do `/admin`, 404 localizado, e **nenhum remetente de e-mail registrado** | `SiteBehaviourTests.cs` | 9 |
| — | acréscimo: nenhuma página imprime uma chave de recurso | `RenderedTextTests.cs` | 14 |
| — | fiação da solução, de E1 | `SolutionWiringTests.cs` | 1 |

**Sobre os efeitos a neutralizar:** nesta leva não há nenhum, e o teste
`Nothing_in_this_release_can_send_a_message_to_anybody` afirma a ausência em vez de confiar nela —
ele varre os tipos de serviço que a aplicação registrou e exige que nenhum se chame remetente de
e-mail. No dia em que um for registrado, quem o registrar tem de vir explicar.

---

## 6. Os controles

`Docs/controles/foundation.tsv` **ainda não está no repositório**: ele nasce no commit de conteúdo,
como a §9.1 manda. O arquivo de alvos está em `scratchpad/leva01/foundation.tsv`, e a tabela abaixo
é **saída do medidor rodada agora**.

<!-- Gerado por Docs/medir-controles.sh em 04/09/2026, HEAD e65b581, árvore COM ALTERAÇÕES NÃO COMMITADAS.
     Nenhuma linha desta tabela foi escrita à mão. -->

**Controles medidos na árvore viva em 04/09/2026, HEAD `e65b581` (árvore COM ALTERAÇÕES NÃO COMMITADAS):**

| Controle | Comando | Hoje |
|---|---|---:|
| C01 a solucao existe na raiz | `ls -A . \| grep -qxF 'OrlandoUp.sln'` | **sim** |
| C02 o projeto web existe | `ls -A src/OrlandoUp.Web \| grep -qxF 'OrlandoUp.Web.csproj'` | **sim** |
| C03 o projeto de teste existe | `ls -A tests/OrlandoUp.Tests \| grep -qxF 'OrlandoUp.Tests.csproj'` | **sim** |
| C04 o recurso pt-BR existe | `ls -A src/OrlandoUp.Web/Resources \| grep -qxF 'SharedResource.pt-BR.resx'` | **sim** |
| C05 D16 nenhuma leitura de relogio local em src | `grep -rIE 'DateTime(Offset)?[.](Now\|Today)' --include='*.cs' --include='*.cshtml' src \| wc -l` | **0** |
| C06 IRMAO de C05 o relogio real e lido num arquivo so Infrastructure/SystemClock.cs | `grep -rIlE '(DateTime\|DateTimeOffset)[.]UtcNow' --include='*.cs' src \| sort \| paste -sd,` | **src/OrlandoUp.Web/Infrastructure/SystemClock.cs** |
| C07 D24 nenhuma secao de conexao no arquivo commitado | `grep -cF 'ConnectionStrings' src/OrlandoUp.Web/appsettings.json` | **0** |
| C08 ALCANCE de C07 o arquivo tem a secao de empresa | `grep -qF 'Company' src/OrlandoUp.Web/appsettings.json` | **sim** |
| C09 D12 nenhuma criacao de schema na subida | `grep -rIE '(Migrate\|MigrateAsync\|EnsureCreated\|EnsureCreatedAsync)[(]' --include='*.cs' src \| wc -l` | **0** |
| C10 ALCANCE de C09 o provedor relacional e alcancado pela varredura | `test $(grep -rIl 'UseSqlServer' --include='*.cs' src \| wc -l) -ge 1 && echo sim \|\| echo nao` | **sim** |
| C11 os dois pacotes de texto rico entram por um arquivo so Application/RichText.cs | `grep -rIlE 'using (Markdig\|Ganss)' --include='*.cs' src \| sort \| paste -sd,` | **src/OrlandoUp.Web/Application/RichText.cs** |
| C12 os dois conjuntos de chaves de recurso sao iguais | `if [ -f src/OrlandoUp.Web/Resources/SharedResource.resx ] && [ -f src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx ]; then diff <(grep -o '<data name="[^"]*"' src/OrlandoUp.Web/Resources/SharedResource.resx \| sed 's/.*name="//' \| sort) <(grep -o '<data name="[^"]*"' src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx \| sed 's/.*name="//' \| sort) >/dev/null && echo equal \|\| echo diferente; else echo ausente; fi` | **equal** |
| C13 ALCANCE de C12 o filtro de chaves nao zera o universo | `n=$(grep -c '<data name=' src/OrlandoUp.Web/Resources/SharedResource.resx); test ${n:-0} -ge 20 && echo sim \|\| echo nao` | **sim** |
| C14 a solucao compila | `dotnet build OrlandoUp.sln --nologo -v q >/dev/null 2>&1; echo $?` | **0** |
| C15 a suite passa | `dotnet test OrlandoUp.sln --nologo -v q >/dev/null 2>&1; echo $?` | **0** |
| C16 EXPIRA COM Q9 os dados de empresa continuam marcados como pendentes | `n=$(grep -cF 'TODO-' src/OrlandoUp.Web/appsettings.json); test ${n:-0} -ge 4 && echo sim \|\| echo nao` | **sim** |
| C17 D15 preco ausente nunca coalesce para zero | `grep -rIE '[?][?] *0\|GetValueOrDefault[(]' --include='*.cs' --include='*.cshtml' src \| wc -l` | **0** |
| C18 ALCANCE de C17 ha tipo monetario anulavel alcancado pela varredura | `test $(grep -rIlE 'decimal[?]' --include='*.cs' src \| wc -l) -ge 1 && echo sim \|\| echo nao` | **sim** |

**Os 18 no valor final, conferidos pelo modo que serve de portao:**

```
bash Docs/medir-controles.sh verificar scratchpad/leva01/foundation.tsv
18 controles, 0 fora do esperado, HEAD e65b581, arvore COM ALTERACOES NAO COMMITADAS.
```

Os cinco que ainda divergiam no relatorio anterior fecharam nesta rodada: C04, C12 e C13 com os
dois arquivos de recurso; C08 e C16 com a secao de empresa do `appsettings.json`, que carrega seis
marcadores pendentes enquanto Q9 estiver aberta.

**A regra 3 aplicada dentro de `src/` (correcao 5), conferida e nao lembrada:** C05, C09 e C17
varrem `src/` inteiro, comentario incluido, porque o `grep` nao sabe o que e comentario. Os tres
medem **0** com 111 arquivos na arvore. Se qualquer comentario meu tivesse transcrito uma leitura
de relogio local, uma criacao de schema na subida ou uma coalescencia de preco para zero, o numero
nao seria zero.

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

**Nenhum arquivo negativo foi tocado.** `CLAUDE.md`, `Docs/decisions.md`, `Docs/architecture.md`,
`Docs/roadmap.md`, `Docs/open-questions.md`, `Docs/market-notes.md`, `Docs/backlog-conhecido.md`,
`Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`,
`Docs/resumo-conversa-1.md`, `Docs/atrito-conversa-1.md`, `Docs/spec-01-foundation.md`,
`.githooks/pre-commit`, `.gitattributes` e `.gitignore` aparecem em **zero** linhas do
`git diff --stat`. `README.md` também não — a seção "Running locally" é de E6.

**111 arquivos novos**, todos dentro da lista fechada da §9.1. Um deles,
`src/OrlandoUp.Web/appsettings.Development.json`, é gerado pelo template e ignorado pelo
`.gitignore`; nunca entra no repositório. **Nenhum arquivo carrega BOM** — a varredura foi refeita
depois de tudo o que as ferramentas geraram nesta rodada.

---

## 8. O que eu preciso do Rod agora — P1

Preciso das duas user-secrets para seguir para E6 (`database update`, os dois seeds e a conferência
visual). **Não invento valor nem placeholder (D24).** Estes são os comandos, **com os nomes das
chaves e nada mais** — o valor entre os sinais de menor e maior é o Rod quem escreve:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<a string do LocalDB OrlandoUpDb>" --project src/OrlandoUp.Web
dotnet user-secrets set "AdminSeed:Email" "<o e-mail do primeiro admin>" --project src/OrlandoUp.Web
dotnet user-secrets set "AdminSeed:Password" "<a senha, 12 caracteres ou mais>" --project src/OrlandoUp.Web
```

O identificador de user-secrets deste projeto já está no `csproj` (`dotnet user-secrets init` foi
rodado em E1), então os três comandos funcionam como estão.

**O que acontece depois que ele rodar os três:**

1. `dotnet ef database update` no LocalDB `OrlandoUpDb` — o único banco desta fase, e o único que
   existe. A migration já foi revisada e aprovada em P2.
2. `dotnet run --project src/OrlandoUp.Web -- seed-catalog` e depois `-- seed-admin`.
3. A conferência visual da §8, com os dez itens, gravada em `Docs/conferencia-leva-01.md`.
4. `Docs/controles/foundation.tsv` entra no repositório, `README.md` ganha a seção de portas e
   comandos, e aí sim os dois commits de fechamento.

**Também vale registrar o que NÃO preciso:** o certificado de desenvolvimento já está válido e
confiável nesta máquina (medido no passo 0), e nada mais depende de decisão dele para E6.

---
