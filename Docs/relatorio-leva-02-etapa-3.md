# Relatório — leva 02, etapa 3: a paleta v1 e a frota real no ar (parada P3)

**Data:** 2026-09-07. **HEAD ao escrever:** `a967e4b`. **Spec:** `Docs/spec-02-public-site.md` com
as notas **`EMENDA-02-01`**, **`EMENDA-02-02`**, **`EMENDA-02-03`** e **`EMENDA-02-04`** no topo,
que vencem o corpo onde discordarem. **Linha da fila:** `2026-09-05`, "LEVA 02 — PUBLIC SITE",
ainda `aguardando`.

**Etapas fechadas desde a P2:** E4 (paleta, tipografia, ilustração), E5 (frota real e conteúdo) e o
bloco destrutivo da §5.4, que a `EMENDA-02-04` D4 mandou rodar logo depois da E5. **Nada foi
empurrado.**

**O que esta parada pede:** conferir o que um visitante agora lê, o efeito do bloco destrutivo
sobre o banco, e as três descobertas da §6 — em especial a 6.1, que era uma quebra em produção
esperando o `seed-catalog` rodar.

**Testes: 126 passando, 0 falhando** (eram 69 na P1). **Portão da leva 01: 18 de 18 verdes.**

---

## 1. O bloco da §5.4 — o que entrou e o que saiu do banco

Rodado **depois** da E5, como a D4 manda, contra o LocalDB e nada mais. `SELECT DB_NAME()` foi
impresso antes, e o próprio script recusa rodar em qualquer banco cujo nome não seja `OrlandoUpDb`
— a regra da D12 escrita dentro do script em vez de confiada ao operador.

| Tabela | Antes | Depois | |
|---|---:|---:|---|
| Products | 7 | **7** | os mesmos sete lugares, outra frota |
| Units | 7 | **10** | 4 + 4 + 2; os quatro carrinhos não têm nenhuma |
| PricingTiers | 16 | **8** | 3 + 3 + 2; os carrinhos não têm preço |
| ProductAddOns | 28 | **12** | só os três produtos à venda |
| ProductTranslations | 14 | 14 | duas culturas para cada produto |
| AddOns / AddOnTranslations | 6 / 12 | 6 / 12 | inalterados |
| DeliveryZones / Translations / Locations | 4 / 8 / 10 | 4 / 8 / 10 | inalterados |
| **AspNetUsers** | **1** | **1** | **não tocado** |
| **AspNetRoles** | **2** | **2** | **não tocado** |

A frota como o banco a descreve agora:

```
drive-scout-4       reservavel=1  larg=20.5  compr=42.3  unidades=4  faixas=3
drive-spitfire-ex   reservavel=1  larg=19.5  compr=39.0  unidades=4  faixas=3
drive-wheelchair    reservavel=1  larg=(nulo) compr=(nulo) unidades=2  faixas=2
single-stroller     reservavel=0  larg=(nulo) compr=(nulo) unidades=0  faixas=0
double-stroller     reservavel=0  larg=(nulo) compr=(nulo) unidades=0  faixas=0
triple-stroller     reservavel=0  larg=(nulo) compr=(nulo) unidades=0  faixas=0
infant-stroller     reservavel=0  larg=(nulo) compr=(nulo) unidades=0  faixas=0
```

Os nulos são a prova de que a migration da P1 serviu para alguma coisa: sem ela, a cadeira e os
quatro carrinhos precisariam de uma medida inventada só para caber na tabela.

---

## 2. E4 — a paleta v1

`wwwroot/css/site.css` reescrito inteiro sobre os tokens da `Docs/architecture.md` §12. O cabeçalho
do arquivo traz a tabela de contraste **calculada da fórmula de luminância WCAG sobre os próprios
valores**, não copiada:

```
white on navy 16.38   ink on paper 15.74   navy on sun 10.50   link on paper 6.63
sun   on navy 10.50   ink on white 16.97   navy on paper 15.20  muted on paper 6.96
on-navy-muted on navy 10.88   fit-text on fit-bg 6.58   tint-text on tint 6.25
```

Duas divergências pequenas com o §12, e a medição é que vale: a pastilha verde dá **6,58** onde o
§12 arredonda para "≥ 7", e o cartão em destaque dá **6,25** onde o §12 diz 6,4. As duas passam
folgadas dos 4,5 da D9. O sol dá **1,45** contra o papel, que é exatamente por que ele é superfície
e nunca cor de texto num fundo claro.

**O anel de foco virou token**, porque tinha de passar de 3:1 nos dois fundos: `--color-focus` é
navy sobre papel (15,20) e vira sol dentro de painel escuro (10,50). Nenhuma regra usa
`outline: none` — o controle C12 conta isso e mede 0.

**D3, os eixos, declarados de propósito.** Os `@font-face` declaram `font-weight: 200 800`, que é o
que os **arquivos** carregam, e não 500–800/400–700, que é o que este desenho **usa** — declarar a
faixa usada faria o navegador sintetizar pesos que ele já tem. O segundo eixo do Bricolage, `opsz
12–96`, entra por `font-optical-sizing: auto` numa linha com o motivo escrito ao lado: um título de
84 px e um de 24 px ganham desenhos diferentes, que é para isso que o eixo existe.

**D5, a escala.** Os seis tamanhos do §12 são o máximo de desktop, cada um dentro de um `clamp()`
que encolhe no telefone. **O que não encolhe:** alvo de toque (`--tap: 44px`, aplicado a link de
menu, link de cartão, botão e campo) e o anel de foco, que tem espessura fixa de 3 px.

**Os três SVG redesenhados** em linha navy com o sol como superfície — sempre contornado de navy,
nunca como traço. **Os dois `nunito-*.woff2` apagados.**

**D1 e D2, a linha falsa corrigida.** O `OFL.txt` dizia que o subconjunto `latin-ext` carregava os
acentos do português. Não carrega: eles moram em U+00C0–U+00FF, que é o `latin`. A linha foi
substituída por seis linhas que dizem o que cada subconjunto cobre, por que o `latin-ext` continua
embarcando (nome de hóspede ou de hotel com letra do Leste Europeu) e por que ele é de graça
(`unicode-range` faz o navegador buscar um subconjunto só quando um caractere da página precisa).
O mesmo motivo verdadeiro está no cabeçalho do `site.css`.

---

## 3. E5 — o que um visitante lê agora

**A frota.** `drive-scout-4` e `drive-spitfire-ex` com os números que a D26 verificou e nenhum
além deles; `drive-wheelchair` **sem uma única medida**, porque a D26 não traz nenhuma e a K2 (b)
mandou alugá-la assim; os quatro carrinhos visíveis, marcados "em breve", sem preço, sem tabela,
sem adicionais e sem botão.

**Preço:** K3 (a), a mesma lista nos dois scooters — 1–2 d US$ 75 fixo, 3–6 d US$ 32/dia, 7+ d
US$ 27/dia. A faixa "reforçada" da leva 01 (95/38/33) foi aposentada: precificava um scooter de
400 lb que não existe na frota.

**Taxa de entrega:** K1 (b) obedecida. **Nenhuma página publica um número de entrega.** A página de
áreas diz que a entrega é orçada junto com a reserva, e o texto explica por quê — preferimos o
número real do endereço a uma média que sai errada na porta. O `Rentals_Intro` da leva 01, que
afirmava "delivery and pick-up are included at the hotels we serve", foi reescrito: era uma
afirmação de preço que a K1 (b) não autoriza.

**D31, a diferença entre as duas línguas, sem um `if` de cultura em página nenhuma.** Uma banda
nova na home, `Home_Team*`, uma chave só e duas mensagens:

- `en-US`: *"A local Orlando team — Orlando Up is run by Ronatrip, a travel company based in
  Orlando. The person who hands you the scooter is the person who answers when you call…"*
- `pt-BR`: *"Uma equipe brasileira em Orlando — … Você fala português do primeiro contato à
  devolução…"*

O controle C09 mede que só os dois leiautes ramificam por cultura na marcação.

**A página nova**, `/delivery-areas` e `/pt/delivery-areas`: as quatro zonas lidas do banco, na
ordem que um administrador definir, com a instrução de entrega em mãos vinda da **mesma coluna que
a reserva vai ler**, renderizada pelo `RichText` injetado — nenhum `using Markdig` novo, que é o
que a `EMENDA-02-01` A13 exigia. Os hotéis são apresentados como exemplos, com a frase que diz
que não é lista fechada.

**A FAQ** foi de 8 para 10 perguntas, e **o número de perguntas saiu do código**: a página lê as
chaves `Faq_Q<n>` do próprio recurso. Acrescentar a décima primeira é editar dois `.resx` e mais
nada. Duas respostas foram reescritas por serem falsas ou vazias hoje: a do cancelamento (apontava
para uma página de termos que é rascunho) e a do contato (mandava ligar para um número que não
existe).

**`appsettings.json`:** D28. `LegalName` e `Address` reais; `Phone`, `WhatsApp`, `Email` e `Hours`
seguem `TODO-`. São **exatamente 4**, e o C16/01 exige `≥ 4` — verde encostado no limiar, como a
`EMENDA-02-01` A1 previu. **Nenhuma página pública imprime `TODO-`**, e há teste para isso.

---

## 4. Testes: de 69 para 126

| Onde | O que passou a ser afirmado |
|---|---|
| `DomainTests` | lista de preço válida **para o que está à venda** e ausente **para o que não está** — os dois lados, com asserção de alcance provando que nenhum dos dois ramos ficou vazio; só produto à venda carrega unidade; largura e comprimento são ambos conhecidos ou ambos ausentes |
| `SeedingTests` | 7 produtos, **10 unidades**, 3 à venda e 4 não, e **zero unidades presas a produto fora de venda** |
| `SiteBehaviourTests` | 21 endereços públicos × 2 teorias: nenhum imprime `TODO-` e cada um tem exatamente um `<h1>`; produto "em breve" não mostra `US$` em lugar nenhum; o catálogo mostra preço e pastilha ao mesmo tempo; a página de áreas nomeia **todas** as zonas que a consulta devolve, nas duas culturas; a FAQ renderiza **todas** as perguntas que o recurso carrega |
| `RenderedTextTests` | 21 endereços (eram 14), incluindo a página nova, os slugs reais e as culturas que faltavam |

Duas asserções nasceram com presença antes de ausência, como a A11 pede: a do `TODO-` só vale
depois de 200 e corpo com mais de mil caracteres, e a do `US$` na página "em breve" só vale depois
de o mesmo texto ser encontrado na página que está à venda.

A lista de perguntas da FAQ e as zonas são lidas **por injeção de dependência**, da mesma fonte que
a página lê — nunca varrendo pasta. É a mesma regra que a `EMENDA-02-04` B5 fixou para o sitemap,
aplicada onde já dava para aplicar.

---

## 5. Controles

**`Docs/controles/foundation.tsv`: 18 de 18 verdes.**

**Proposta da leva 02: 13 dos 15 no alvo.** Os dois que faltam são da E6, que ainda não rodou:

| Controle | Hoje | Alvo |
|---|---|---|
| C01 a página de áreas existe | **sim** | sim |
| C02 as quatro woff2, e só elas | **as quatro** | as quatro |
| C03 / C05 / C07 hex, tokens e família aposentados | **0 / 0 / 0** | 0 |
| C04 / C06 / C08 alcance | **sim / sim / sim** | sim |
| C09 só os dois leiautes ramificam por cultura | **no alvo** | no alvo |
| C10 / C11 relação de cultura nos links | **0 / sim** | 0 / sim |
| C12 / C13 foco | **0 / sim** | 0 / sim |
| C14 dados estruturados num arquivo só | `nenhum` | `_StructuredData.cshtml` |
| C15 sitemap num arquivo só | `nenhum` | `SitemapEndpoints.cs` |

**C08 estreitado**, como a revisão da P2 mandou: o alvo passou de `src` para
`wwwroot/css/site.css`. Antes ele ficava verde por causa do `OFL.txt` — a família estava
*licenciada*, não *usada*, e o controle não sabia distinguir. Prova dos dois lados, rodada dos
arquivos gravados: contra o `site.css` de hoje devolve `sim`; contra um arquivo de estilo que não
nomeia a família devolve `nao`; e contra o `OFL.txt` sozinho devolveria `sim`, que é exatamente o
falso verde que o estreitamento remove.

**O arquivo `.tsv` ainda não foi commitado**, de propósito: com C14 e C15 fora do alvo, commitá-lo
agora poria dois vermelhos permanentes no portão de fim de sessão por trabalho que ainda não
começou. Ele entra na E7, junto com a troca do rótulo do C16 e a conferência visual.

---

## 6. Três descobertas

### 6.1 O painel do `/admin` quebraria na primeira execução do `seed-catalog` — e nenhuma emenda tinha nomeado esse arquivo

`Pages/Admin/Index.cshtml.cs` decidia se mostrava a faixa de placeholder assim:

```csharp
CatalogSeedData.Products.Single(product => product.Slug == "standard-scooter")
```

`standard-scooter` deixou de existir na E5. `Single` sobre nenhum elemento **lança**, então o painel
inteiro passaria a dar erro 500 no primeiro acesso depois do reseed. Não era teste falhando: nenhum
teste abre `/admin` autenticado. A lista de arquivos da `EMENDA-02-01` A12 é boa e não tinha este —
o que o encontrou foi compilar e ler, não a lista.

A faixa foi **removida**, não consertada: ela dizia "o catálogo ainda carrega dados de placeholder",
e isso deixou de ser verdade. A chave `Admin_PlaceholderBanner` saiu dos dois `.resx`. O painel
agora lê **7 / 10 / 10** — produtos, unidades, locais — e os dois primeiros números diferirem é o
ponto: unidade é frota, produto é catálogo.

### 6.2 O site escreve todo acento como entidade numérica, e isso derrubou um teste correto

`Hotéis` chega ao navegador como `Hot&#xE9;is`. É o padrão do ASP.NET Core e **não é defeito** —
renderiza certo, é HTML válido. Custou um teste: comparar o nome da zona com a string crua falha, e
comparar com `WebUtility.HtmlEncode` também falha, porque o framework escreve a forma hexadecimal e
a biblioteca de teste a decimal. **Duas grafias do mesmo caractere.** O teste passou a decodificar o
corpo antes de comparar, que é a pergunta certa: o que um leitor vê.

Fica a observação, sem ação: dá para configurar `WebEncoderOptions` com `UnicodeRanges.All` e o
HTML sairia legível e um pouco menor. É decisão com implicação de segurança, não cabe numa leva de
conteúdo, e não custa nada hoje.

### 6.3 Uma armadilha do xUnit que custa uma rodada inteira

`[InlineData(21, null)]` num `[Theory]` de parâmetros `double?` **não roda**: o literal é
encaixotado como `Int32` e a reflexão recusa entregá-lo a um `Nullable<double>`. Com `double` não
anulável a conversão acontece — é por isso que os casos antigos do mesmo arquivo sempre
funcionaram. Sufixo `d` resolve. Já estava na §7 do relatório da etapa 1; repito aqui porque é a
única armadilha desta leva que morde em silêncio.

---

## 7. O que falta para fechar a leva

| Etapa | O quê | Estado |
|---|---|---|
| **E6** | `Api/SitemapEndpoints.cs`, `Application/StructuredData.cs`, `Pages/Shared/_StructuredData.cshtml`, o mapeamento no `Program.cs` e o parcial no `_Layout` | não começou |
| **E7** | testes do sitemap e dos dados estruturados (A11 e B5); commitar `Docs/controles/public-site.tsv`; trocar o rótulo do C16 em `foundation.tsv`; `Docs/conferencia-leva-02.md` com os 13 itens, incluindo a A15 e o Lighthouse | não começou |
| **Fechamento** | commit de conteúdo e commit de fechamento gravando o hash na coluna Commit da linha da fila | não começou |

**Formas proibidas, conferidas depois de tudo escrito:** `?? 0` e `GetValueOrDefault(` seguem em
**0** em `src/`, com as duas colunas anuláveis em uso — toda leitura nova vai por `is decimal`.
`DateTime.Now`, `DateTime.Today`, `Migrate(` e `EnsureCreated(` em 0; `UtcNow` só em
`Infrastructure/SystemClock.cs`; `using Markdig` só em `Application/RichText.cs`, inclusive na
página nova. Nenhum comentário escrito nesta leva transcreve qualquer uma dessas formas.

**Varredura antes do commit:** nenhum arquivo com BOM ou UTF-16; nenhum arquivo da lista de
negativos da §12.1 da spec aparece no `git status`.

---

## Revisão (Claude Web, 2026-09-07)

**Veredito: aprovado, pode começar a E6.** Conferido abrindo a folha de estilo, o seed, os testes e
o diff — não o relato. Uma correção de nome (esta parada não é a P3 da §0), três correções que
entram na E7, e nenhuma que volte trabalho já feito. Tudo em `EMENDA-02-05`.

**O que remedi por conta própria, e bateu:**

| Afirmação | Como conferi | Resultado |
|---|---|---|
| a tabela de contraste foi **calculada**, não estimada | recalculei os 15 pares da fórmula WCAG sobre os hexes do próprio `:root` | **os 15 batem na casa centesimal** — inclusive as duas divergências que você aponta contra o §12 (6,58 onde ele arredonda para "≥ 7"; 6,25 onde ele diz 6,4) |
| os 13 controles mensuráveis estão no alvo | rodei os 13 do arquivo gravado | 13/13 — hex 0, tokens 0, nunito 0, fontes só as quatro, C10 `22 − 22 = 0` com o operando subindo de 18 para 22 |
| o foco passa nos dois fundos | `:focus-visible` único + `--color-focus` reatribuído | reatribuído em `.site-header`, `.hero`, `.section--navy`, `.site-footer`; `outline: none` = 0 |
| o seed não inventa número | li o `CatalogSeedData.cs` contra a D26 | `20.5 × 42.3`; `19.5 × 39` com assento 17 **só** no Spitfire; cadeira com todas as medidas nulas; carrinhos sem unidade, sem faixa e sem adicional; K3 (a) obedecida |
| K1 (b) obedecida | varredura por número de taxa em `Pages/` e `Resources/` | nenhum |
| paridade de recurso e faixa do C16 | `diff` das chaves e `grep -cF TODO-` | 163 = 163; `TODO-` = **4**, a margem zero que a A1 previu |
| `Admin_PlaceholderBanner` saiu | `grep` em todo `src/` | 0 |
| superfície, BOM, negativos | `git diff --name-only` e os três primeiros bytes de cada arquivo | 30 arquivos, todos previstos; nenhum BOM; nenhum negativo |

**E uma que eu não tinha pensado em conferir e que decide a leva:** os quatro `@font-face` trazem o
`unicode-range` certo. É ele que faz a D2 ser verdade — sem ele, duas faces da mesma família e do
mesmo peso colidem e o arquivo `latin-ext`, que tem **4 glifos** abaixo de U+0100, poderia vencer
para a página inteira. Você não afirmou isso no relatório; está certo no arquivo.

**A §6.1 é o achado da leva, e ele justifica o ritual inteiro.** `Single(... == "standard-scooter")`
lançaria no primeiro acesso ao painel depois do reseed, e nenhum teste abre `/admin` autenticado —
não havia teste para falhar. Você está certo também sobre o que o encontrou: não foi a lista da
A12, foi compilar e ler. Medi o entorno: **zero** literais de slug fora do `CatalogSeedData.cs` em
`src/`, e nenhum `Single(`/`First(` sobre a lista de seed. Isso vira controle na E7 (`EMENDA-02-05`
E4) — slug é dado, e slug digitado dentro de código é mina com data marcada.

**A correção de nome.** Esta parada não é a P3 da §0 — a P3 de lá é "tudo escrito, testes verdes,
antes do commit de conteúdo", com a conferência e os dois `.tsv`. Passa a ser **P3a**; a P3b é a
E6 + E7, em `Docs/relatorio-leva-02-etapa-4.md`. Parar aqui foi a leitura certa: o bloco destrutivo
tinha acabado de rodar e toda a superfície visível mudou. E segurar o `.tsv` em vez de commitar
dois vermelhos permanentes é o que um arquivo de controle é.

**As três que entram na E7, nenhuma bloqueia esta parada:**

1. **A página de áreas tem teste de nome de zona e não do texto que ela existe para publicar.** A
   D5/02 põe a página na leva porque a instrução de entrega em mãos tem de ser a mesma string que a
   reserva vai mostrar — e nada afirma que `InstructionsHtml` renderizou. Quatro nomes listados e a
   instrução perdida passa hoje.
2. **`The_catalog_page_prices_what_is_on_sale_and_only_that` promete mais do que afirma.** "from
   US$" presente + "Coming soon" presente + sem "US$ 0.00" é satisfeito por uma página que imprima
   "from US$ 32/day" debaixo de um carrinho. Conte: ocorrências de `from US$` iguais ao número de
   produtos reserváveis lido por injeção.
3. **`PublicPaths()` é lista digitada de 21 endereços.** Serve para asserção por página; a regra da
   B5 cai na E7, quando o conjunto de páginas do sitemap tiver de vir por injeção — e aí a lista
   digitada vira a segunda fonte independente, se você afirmar que as duas coincidem.

Uma observação sem ação: `No_seeded_product_carries_a_dimension_that_was_never_measured` não tem
asserção de alcance própria; ela passaria numa frota inteira medida. O teste ao lado, no mesmo
arquivo e sobre a mesma coleção, tem — então a cobertura existe, só não está onde o nome promete.

**Pode seguir para a E6 e a E7. A próxima parada é a P3b.**
