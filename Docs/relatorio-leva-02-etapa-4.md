# Relatório — leva 02, etapa 4: sitemap, dados estruturados e os dois portões (parada P3b)

**Data:** 2026-09-07. **HEAD ao escrever:** `926d6a6`. **Spec:** `Docs/spec-02-public-site.md` com
as notas **`EMENDA-02-01`** a **`EMENDA-02-05`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-05`, "LEVA 02 — PUBLIC SITE", ainda `aguardando`.

**Etapas fechadas desde a P3a:** E6 (encanamento de máquina) e E7 (testes, controles,
conferência). **Nada foi empurrado.**

**Esta é a parada que a `EMENDA-02-05` E1 chama de P3b**, e é a última antes do commit de conteúdo.

| | |
|---|---|
| Testes | **137 passando, 0 falhando** (69 na P1, 126 na P3a) |
| `Docs/controles/foundation.tsv` | **18 de 18** |
| `Docs/controles/public-site.tsv` | **17 de 17**, commitado agora pela primeira vez |
| Conferência | `Docs/conferencia-leva-02.md`, 14 linhas — 11 verdes, 1 parcial declarada, 2 suas |

**O que esta parada pede:** conferir o que o site passou a dizer a uma máquina, e as **três
descobertas da §4** — uma delas é um caminho de injeção de script que eu mesmo abri e fechei dentro
desta etapa.

---

## 1. E6 — o encanamento de máquina

### 1.1 `/sitemap.xml`

Um arquivo (`Api/SitemapEndpoints.cs`), ao lado do `robots.txt`. **30 endereços**: 8 páginas
públicas × 2 culturas + 7 produtos × 2. Cada entrada carrega o conjunto completo de alternativas
(`en-US`, `pt-BR`, `x-default`), que é o mesmo conjunto que a página já põe no próprio `<head>`.

**O conjunto de páginas vem por injeção** (`EMENDA-02-04` B5), de `PublicPages`, que pergunta ao
framework quais páginas Razor existem e descarta `/Admin`, `/Error`, `/Shared` e as que exigem um
parâmetro que o sitemap não teria como inventar — hoje só a de produto, cujos endereços vêm do
catálogo, um por linha que existe. **Uma página nova entra no sitemap no dia em que passa a
existir**, sem ninguém lembrar de nada.

Sem `lastmod`: o schema não tem data de modificação por página, e uma data inventada ensina um
robô a voltar por mudanças que nunca aconteceram. É também o motivo mecânico de este arquivo não
ler relógio nenhum — o C06/01 exige que `UtcNow` viva num arquivo só.

### 1.2 Dados estruturados

Um parcial (`Pages/Shared/_StructuredData.cshtml`), um tipo que decide o conteúdo
(`Application/StructuredData.cs`), renderizado pelo leiaute em toda página. `LocalBusiness` sempre;
`Product` quando a página é de produto.

- **Todo campo de empresa ainda marcado é omitido**, nunca emitido. Telefone e e-mail simplesmente
  não aparecem no documento — um marcador publicado é pior que uma lacuna, porque parece dado.
- **Nenhum `Offer`** (D7/02). Um `Offer` carrega preço e disponibilidade, e um buscador tem o
  direito de mostrar os dois e um visitante de tentar agir sobre eles. Ele chega com o checkout.
- **Nenhum `brand`, e isso é ausência decidida.** A D7/02 lista `brand` entre os campos. O schema
  não tem coluna de fabricante, então escrever um aqui seria digitar dado de frota dentro de
  código — exatamente o defeito da §6.1 da P3a, que esta etapa transformou em controle. Volta
  quando uma linha de produto puder carregá-lo.

---

## 2. As três correções da `EMENDA-02-05`

**E2 — a página de áreas afirmava o nome da zona e não a frase que ela existe para publicar.**
Corrigido: o teste agora tira as marcações da instrução de entrega em mãos, exige que ela tenha
mais de 80 caracteres (alcance) e afirma que os primeiros 60 aparecem na página. Quatro nomes
listados com as instruções silenciosamente perdidas deixa de passar.

**E3 — `The_catalog_page_prices_what_is_on_sale_and_only_that` prometia um negativo que não
afirmava.** Corrigido: o teste lê por injeção quantos produtos estão à venda e quantos não estão, e
afirma **igualdade** — ocorrências de `from US$` iguais ao número de reserváveis, pastilhas "em
breve" iguais ao número dos outros. Um preço impresso debaixo de um carrinho passava antes e não
passa mais. As duas contagens vêm com asserção de alcance: nenhum dos dois lados pode ser zero.

**E4 — o defeito da §6.1 virou controle, e ele encontrou um irmão.** O controle novo conta
identificadores de catálogo digitados em código fora do `CatalogSeedData.cs`. Ao escrevê-lo, medi
por *forma* em vez de por lista e apareceu um que a emenda não conhecia:

```
src/OrlandoUp.Web/Pages/HowItWorks.cshtml.cs:25:  zone.Code == "disney-resorts"
```

Mesma família, sintoma mais silencioso: renomeie a zona na administração e a página de como
funciona **para de mostrar o bloco de entrega em mãos sem erro nenhum** — usa `FirstOrDefault`,
então degrada em vez de estourar. Consertado na raiz em vez de tolerado: `ZoneInstructions` passou
a carregar o `HandoverMode`, e a página pede *"a zona onde a entrega é em mãos"*, que é o que ela
sempre quis dizer. O controle cobre os 7 slugs **e** os 4 códigos de zona, e mede **0**.

**B5 — as duas listas de páginas.** `PublicPaths()` continua existindo em `SiteBehaviourTests` e é
útil justamente por ser digitada e independente. Um teste novo afirma que ela e o conjunto derivado
do framework **coincidem exatamente**, e a mensagem de falha nomeia quem está sobrando de que lado.
Uma página acrescentada e esquecida numa das duas aparece com nome, não como mistério.

---

## 3. Os controles

**`Docs/controles/public-site.tsv` commitado com 17 controles, todos no alvo.** Ele foi segurado
até agora de propósito: com C14 e C15 fora do alvo, commitá-lo na P3a poria dois vermelhos
permanentes no portão por trabalho que não tinha começado.

| | Controle | Hoje |
|---|---|---|
| C01 | a página de áreas existe | sim |
| C02 | as quatro woff2, e só elas | as quatro |
| C03 / C04 | a cor de ação aposentada sumiu / a varredura alcança a em uso | 0 / sim |
| C05 / C06 | os quatro nomes de token aposentados sumiram / alcance | 0 / sim |
| C07 / C08 | a família aposentada sumiu / o **estilo** nomeia a nova | 0 / sim |
| C09 | só os dois leiautes ramificam por cultura na marcação | os dois |
| C10 / C11 | todo link público declara a cultura na mesma linha / alcance | 0 / sim |
| C12 / C13 | nenhum anel de foco removido / o estilo define foco visível | 0 / sim |
| C14 | dados estruturados por um arquivo só | `_StructuredData.cshtml` |
| C15 | o endereço do sitemap por um arquivo só | `SitemapEndpoints.cs` |
| **C16** | **nenhum identificador de catálogo digitado em código fora do seed** | **0** |
| **C17** | ALCANCE de C16 — a varredura reconhece os identificadores dentro do seed | sim |

**Prova dos dois lados do C16**, rodada de uma cópia da árvore fora do repositório: acrescentei
`// var x = "drive-scout-4";` a um modelo de página e o controle passou de **0 para 1**. Sabe dizer
não, e diz não inclusive em comentário — que é o que a regra 2 de `Docs/regras-de-controle.md`
exige.

**`Docs/controles/foundation.tsv`: só o rótulo do C16 mudou**, de "EXPIRA COM Q9" para "EXPIRA COM
Q12", como a §12.1 da spec autoriza. Tipo, alvo, padrão e esperado intocados; ele continua exigindo
`≥ 4` e medindo exatamente 4.

---

## 4. Três descobertas, e a primeira é a séria

### 4.1 O encoder que eu escolhi abria um caminho de injeção de script pelo catálogo

O JSON-LD é escrito **dentro de um `<script>`**, e o conteúdo de um elemento `script` é *raw text*:
o navegador não decodifica referência de caractere ali, só procura a tag de fechamento. Eu
serializei com `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, que **não escapa `<` nem `>`**.

Consequência: um nome ou uma chamada de produto — colunas que um administrador edita — contendo os
seis caracteres que fecham um `script` encerraria o bloco, e o que viesse depois rodaria como
código. **Caminho completo de uma caixa de edição até execução no navegador do visitante.**

Percebi ao ler o que tinha acabado de renderizar: o `&` de "Ronatrip Tours & Travel" saiu literal.
A primeira correção — trocar para `JavaScriptEncoder.Create(UnicodeRanges.All)` — **não bastou**, e
esse é o detalhe que vale guardar: permitir uma faixa Unicode **não** reproíbe o que é perigoso.
A forma correta é construir o encoder à mão:

```csharp
settings.AllowRange(UnicodeRanges.All);      // acento continua acento
settings.ForbidCharacters('<', '>', '&');    // e o que fecha o script, não
```

Fechado por teste direto, e não por leitura: `Catalog_text_cannot_end_the_structured_data_block_early`
monta um produto cujo nome é `</script><script>alert(1)</script>` e afirma que o JSON produzido não
contém `<`, `>` nem `</script>` — e, no mesmo teste, que `ação` continua chegando com cedilha e til,
que é a razão de o encoder ser construído à mão em vez de usar o padrão.

### 4.2 O sitemap declarava uma codificação e mandava outra

`XmlWriter` toma a codificação do *writer* que recebe, não a das próprias configurações, e um
`StringWriter` é UTF-16. O documento saía dizendo `encoding="utf-16"` enquanto a resposta ia em
UTF-8 — um cabeçalho contradizendo os próprios bytes, que um parser tem o direito de recusar.
Corrigido com um `StringWriter` que declara UTF-8, e o teste afirma a declaração.

### 4.3 O tipo do `<script>` saía com o `+` codificado

`type="application/ld&#x2B;json"`. Todo parser HTML de verdade decodifica isso de volta e lê certo,
então **não estava quebrado** — mas fica errado aos olhos de quem confere a página e de qualquer
validador que case texto em vez de analisar. O elemento passou a ser escrito cru, por essa razão e
só por ela; o JSON de dentro continua escapado pelo serializador, que é o que a §4.1 acabou de
tornar obrigatório.

---

## 5. Uma observação para a fase 5, sem ação agora

**O esquema das URLs do sitemap segue o esquema da requisição.** No host de teste isso dá
`http://orlandoup.com/...`; num navegador dará `https`. É a mesma propriedade que o `<link
rel="canonical">` do leiaute tem desde a leva 01, então não é regressão — mas atrás do App Service
ela depende de cabeçalhos encaminhados estarem configurados, senão um sitemap `http` aponta para um
site `https`. **Fica anotado para o portão de deploy da fase 5**, que é onde `ForwardedHeaders` é
decidido; mexer nisso agora seria configurar um ambiente que ainda não existe.

---

## 6. Estado da árvore e formas proibidas

**Varredura antes do commit:** nenhum arquivo com BOM ou UTF-16; nenhum arquivo da lista de
negativos da §12.1 tocado. De `Docs/controles/foundation.tsv`, exatamente **uma** linha alterada, e
é o rótulo do C16.

**Formas proibidas, remedidas com tudo escrito:** `?? 0` e `GetValueOrDefault(` em **0** em `src/`;
`DateTime.Now` e `DateTime.Today` em **0**; `Migrate(`/`EnsureCreated(` em **0**; `UtcNow` só em
`Infrastructure/SystemClock.cs` — o sitemap não lê relógio, que é o que a A13 exigia; `using
Markdig` só em `Application/RichText.cs`, inclusive na página de áreas, que renderiza a instrução
pelo `RichText` injetado.

---

## 7. O que falta para fechar a leva

1. **O commit de conteúdo** — tudo o que está na árvore agora.
2. **O commit de fechamento**, gravando o hash do primeiro na coluna Commit da linha da fila e
   passando o Estado para `concluido`.
3. **O push é seu** (`CLAUDE.md`).
4. **O item 13 da conferência é seu**: Lighthouse, categoria acessibilidade, em `/` e `/rentals`.
   É o único "done means" da fase 2 do roadmap ainda sem número.

---

## Revisão (Claude Web, 2026-09-07)

**Veredito: executar após duas correções, ambas dentro do commit de conteúdo.** Conferido abrindo o
endpoint, o encoder, os dois arquivos de controle e os testes — não o relato. Nada volta; as duas
correções são um teste a mais e uma palavra a menos.

**F1 — o sitemap emite `hreflang` e `x-default` e nenhum teste os afirma.** A §8 exige o par
`xhtml:link rel="alternate"` por cultura e o `x-default` apontando para a inglesa, e o
`SitemapEndpoints.cs` escreve os três (linhas 117–132). O `SeoTests` cobre o conjunto de `<loc>`
pelos dois lados — páginas derivadas, produtos, o produto escondido saindo — e **nunca abre um
`xhtml:link`**: medido, as strings `hreflang` e `x-default` não aparecem em nenhum fonte de teste.
Uma regressão que derrube os alternates passa nos 137. É o artefato para o qual essa classe existe,
justamente porque ninguém o abre de novo. Três asserções ao lado do teste que já lê o XML: cada
`<url>` tem um `xhtml:link` por cultura mais o `x-default`; o `href` do `x-default` é o endereço
inglês daquela mesma página; e um alcance provando que algum alternate foi encontrado.

**F2 — um método avisa.** `SeoTests.The_two_lists_of_public_pages_agree_with_each_other` é
`async Task` e não espera nada (CS1998) — o único da suíte, medido varrendo os corpos. Tire o
`async`. O padrão da leva 01 é build sem aviso, e aviso permanente é aviso que ninguém lê.

**O que remedi por conta própria, e bateu:**

| Afirmação | Como conferi | Resultado |
|---|---|---|
| 17 controles no alvo | rodei os 16 `cmd` do arquivo **commitado** | **16 de 16** no esperado |
| o C16 sabe dizer não | copiei a árvore e acrescentei `// var x = "drive-scout-4";` | 0 na árvore real, **1** na cópia — e pega dentro de comentário |
| o `foundation.tsv` mudou só o rótulo | `git diff --numstat` e leitura das duas linhas | **1 linha**, `Q9` → `Q12`; padrão, alvo e esperado idênticos |
| a fila não foi tocada | `git diff -- Docs/fila-cc.md` | intacta, como tem de estar até o commit de fechamento |
| a B5 foi cumprida na fonte | li o `PublicPages.cs` | conjunto derivado de `IActionDescriptorCollectionProvider`; e o teste que compara as duas listas transforma a lista digitada em segunda opinião de verdade |
| a §4.1 fechou o buraco | li o `ScriptSafeEncoder()` | `AllowRange(UnicodeRanges.All)` **seguido de** `ForbidCharacters('<','>','&')` |
| E2, E3 e E4 da emenda anterior | li os três testes | aplicadas; a E2 afirma 60 caracteres da instrução com alcance de 80, a E3 afirma **igualdade** com alcance dos dois lados, a E4 virou C16/C17 |
| superfície, BOM, negativos | `git diff --name-only` e os três primeiros bytes | 16 arquivos, todos previstos; nenhum BOM; nenhum negativo |

**A §4.1 é o achado da leva e é de segurança.** Permitir uma faixa Unicode não reproíbe o que é
perigoso — é uma armadilha que a documentação não grita, e ela estava num caminho completo de uma
caixa de edição do administrador até código rodando no navegador do visitante. Você fechou por
teste que dirige um `</script>` pelo nome do produto, não por leitura. E a §4.2 (o `XmlWriter`
tomando a codificação do writer, não das settings) é a mesma classe: verde que mentia.

**A E4 encontrou um irmão que a emenda não conhecia**, e a correção certa foi na raiz: `zone.Code ==
"disney-resorts"` com `FirstOrDefault` degradava em silêncio — renomeie a zona e a página de como
funciona simplesmente para de mostrar o bloco, sem erro nenhum. `ZoneInstructions` passou a carregar
o `HandoverMode` e a página pede a zona pelo que ela **faz**. É a diferença entre consertar o
sintoma e tirar o dado de dentro do código.

**Sobre a §5 (esquema das URLs atrás do App Service):** concordo em não mexer agora — configurar
`ForwardedHeaders` para um ambiente que não existe é adivinhar. Registrado em
`Docs/backlog-conhecido.md` com destino ao portão de deploy da fase 5.

**O fechamento, para não ser improvisado** (`EMENDA-02-06` F3): commit de conteúdo com F1 e F2
dentro; depois o commit de fechamento cuja **única** mudança é a linha `2026-09-05` da
`Docs/fila-cc.md` — Estado para `concluido`, Commit com o hash curto do primeiro, número total de
linhas inalterado, nenhuma outra célula reescrita. O push é seu. **A leva fecha com o item 13 da
conferência aberto**: o número do Lighthouse é do portão da fase 2 do roadmap, não do conteúdo
desta leva, e fica registrado na conferência como seu.

**Faça F1 e F2, commite o conteúdo, feche a fila. Não há mais parada.**
