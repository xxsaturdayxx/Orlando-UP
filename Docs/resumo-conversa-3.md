# Resumo — conversa 3: o site público no ar, e a revisão que virou seção de relatório

**Data:** 2026-09-07. **Par:** conversa 3 ↔ **leva 02**. **Anterior:** `Docs/resumo-conversa-2.md`
(conversa 2 ↔ decisões D26–D31). **Fecha em:** conteúdo `383fe27`, fechamento `f59c0e2`.

**Proveniência:** `[V]` medido nesta conversa, com o comando ao lado quando é barato repetir;
`[H]` herdado de documento anterior e não reconferido aqui.

---

## 1. Onde o projeto está

`[V]` `git ls-files` = **155**; árvore limpa; **13 commits à frente do remoto, nada empurrado** — o
push é do Rod. A fila tem **duas** linhas, ambas `concluido`: leva 01 em `5d538ba`, leva 02 em
`383fe27` `[V]`. Dois arquivos de controle: `foundation.tsv` com 18 e `public-site.tsv` com 17,
todos no alvo `[V]`, os 16 mensuráveis rodados pelo Claude Web a partir do arquivo commitado.

O site que roda em `https://localhost:7420` já não é andaime: paleta v1, as duas fontes hospedadas
no próprio site, a frota real no banco, uma página de áreas de entrega, sitemap e dados
estruturados. **137 testes** `[H]`, lidos do relato do agente — ver §6, é a única classe de número
que o revisor não consegue medir.

---

## 2. O que a leva 02 entregou

**Schema — uma migration, `20260906162133_AddIsBookableAndOptionalDimensions`, aplicada ao LocalDB
`[V]`.** `Products.IsBookable` (`bit NOT NULL`), `WidthIn` e `LengthIn` passam a aceitar nulo. Os
metadados reais lidos de `INFORMATION_SCHEMA` estão na §10 de
`Docs/relatorio-leva-02-etapa-1.md`.

**A frota real substituiu o catálogo placeholder** pelo bloco da §5.4 da spec, rodado uma vez:
Products 7 → 7, Units 7 → **10**, PricingTiers 16 → **8**, ProductAddOns 28 → **12**; Identity
intocado `[H, relatório da etapa 3]`. Dois scooters e uma cadeira reserváveis; quatro carrinhos
visíveis, "em breve", sem preço, sem unidade e sem adicional. A cadeira entra **sem uma única
medida** — a D26 não traz nenhuma, e a K2 (b) mandou alugá-la assim.

**Conteúdo:** as três respostas do Rod no P0 — K1 (b) nenhuma taxa de entrega publicada, K2 (b)
cadeira sem dimensões, K3 (a) mesmo preço nos dois scooters. FAQ de 8 para 10 perguntas, com a
contagem fora do código. `LegalName` e `Address` da D28 preenchidos; sobram **exatamente 4**
marcadores `TODO-` `[V]`.

**Máquina:** `/sitemap.xml` com as duas culturas de cada página, `hreflang` por cultura e
`x-default` apontando para a inglesa; `LocalBusiness` em toda página e `Product` na página de
produto, **sem `Offer`** (D7/02). Indexação continua fechada.

---

## 3. As armadilhas descobertas — o que vale além desta leva

| Armadilha | O que acontece | Como se descobriu |
|---|---|---|
| **Padrão de banco em `bool` não anulável** | com `HasDefaultValue`, o EF não distingue `false` explícito de omissão: o `false` é descartado e a linha entra com o padrão. Escrito como a emenda mandava, os quatro carrinhos teriam sido gravados **reserváveis**, com contagem e teste verdes | o agente parou na P1 e recusou a letra da correção `[V]` |
| **`IsActive` tem o mesmo defeito, desde a leva 01** | hoje é impossível inserir um produto oculto. Não afeta UPDATE nem esta leva | backlog, **resolver antes da leva 04** |
| **`Down` que inventa valor** | o `Down` gerado devolvia `0` às duas dimensões, e 0 × 0 está dentro de 30 × 48: o rollback publicaria "cabe nos ônibus da Disney" sobre máquina que ninguém mediu. Fechado com `THROW` que recusa enquanto houver dimensão nula | leitura do `Down` na P1 |
| **`AllowRange` não reproíbe o perigoso** | `JavaScriptEncoder` com `UnsafeRelaxedJsonEscaping` não escapa `<`; dentro de um `<script>` isso é caminho completo de uma caixa de edição do administrador até código no navegador do visitante. `Create(UnicodeRanges.All)` **não** bastou — só `ForbidCharacters('<','>','&')` fecha | o agente notou o `&` de "Tours & Travel" saindo literal `[V]` |
| **`latin` carrega os acentos do português, não `latin-ext`** | 28 de 28 no `latin`, 1 de 28 no `latin-ext`. A D7/01 e o `OFL.txt` diziam o contrário; nada renderiza errado porque os dois são servidos, mas a razão escrita era falsa | fontTools sobre os quatro `.woff2` `[V]` |
| **`unicode-range` é o que separa duas faces da mesma família** | sem ele, o arquivo `latin-ext` (4 glifos abaixo de U+0100) pode vencer a página inteira | leitura do `@font-face` `[V]` |
| **Slug digitado dentro de código é mina com data** | `Single(p => p.Slug == "standard-scooter")` no `/admin` daria 500 no primeiro acesso depois do reseed, e nenhum teste abre `/admin` autenticado. O controle que nasceu disso encontrou um irmão pior: `zone.Code == "disney-resorts"` com `FirstOrDefault`, que **degrada em silêncio** | compilar e ler, não a lista de arquivos |
| **`XmlWriter` toma a codificação do writer, não das settings** | o sitemap declarava `utf-16` e ia em `utf-8` | teste da declaração |
| **Filtro que recorta conteúdo em vez de caminho** | `grep -v /Admin/` sobre `caminho:linha` escondia dois links sem cultura porque o *href* deles continha `/Admin/` — o controle do D10/02 estava verde por coincidência | remedido nas três formas na revisão do P0 `[V]` |
| **`grep` com padrão que começa em hífen** | sem `-e`, o grep aborta e o controle devolve `0` medindo nada | o agente pegou ao escrever o controle de token |
| **Varredura `grep -r … src` alcança `bin/` e `obj/`** | os manifestos de static web assets citam nomes de arquivo de fonte: `nunito` dá 58 sem o corte e 8 com. **Sem `--exclude-dir`, o controle nunca chegaria a 0** | o agente corrigiu uma emenda do revisor com medição `[V]` |
| **`[InlineData(21, null)]` num `[Theory]` de `double?`** | não roda: o literal encaixota como `Int32` | sufixo `d` |
| **Acento sai como entidade hexadecimal** | o framework escreve `&#xE9;` e `WebUtility.HtmlEncode` escreve decimal — comparar texto em teste exige **decodificar o corpo**, nunca codificar a expectativa | um teste correto falhando |

---

## 4. O ritual: o que mudou no processo, e o que ele pegou

**Item 7 do protocolo, registrado em `7e7a0f8`:** a revisão de cada parada é a seção
`## Revisão (Claude Web, data)` no fim do próprio `Docs/relatorio-<leva>-etapa-N.md`, commitada; o
Rod cola no agente a frase constante *"leia `<relatório>` e execute a seção Revisão"*. A revisão do
**plano** não tem relatório onde morar — vira **nota de emenda datada no topo da spec**, com
identificador e comando que prova a leitura.

**Seis emendas nasceram assim**, e a spec as carrega no topo, vencendo o corpo onde discordarem:

| Emenda | Commit | O que fixou |
|---|---|---|
| `EMENDA-02-01` | `7e7a0f8` | 15 correções ao primeiro plano — a que inverteu resultado foi o filtro por conteúdo do controle de cultura |
| `EMENDA-02-02` | `c886f97` | duas delas **substituídas pela medição do próprio agente** (ordem da alternância; o corte de `bin`/`obj` como obrigatório, não precaução) |
| `EMENDA-02-03` | `3b2cd58` | `IsBookable` sem padrão no modelo; o `Down` com guarda; a suíte não prova migration |
| `EMENDA-02-04` | `a967e4b` | os subconjuntos de fonte; os eixos `wght`/`opsz`; a §5.4 movida para depois da E5 |
| `EMENDA-02-05` | `926d6a6` | parada dividida em P3a/P3b; três testes que passavam pelo motivo errado; o controle de slug |
| `EMENDA-02-06` | `d1c5891` | `hreflang` sem teste; um CS1998; as condições dos dois commits de fechamento |

**Duas emendas do revisor estavam erradas e o agente as corrigiu medindo.** Isso é o desenho
funcionando nos dois sentidos, e é o argumento mais forte a favor de "medir em vez de raciocinar" —
ver `Docs/atrito-conversa-3.md`.

**O que só o agente pegou:** o defeito do `/admin`, a injeção pelo `<script>`, a codificação do
sitemap, a ordem da §5.4. **O que só a revisão pegou:** o controle verde por coincidência, o `.tsv`
que dizia ter uma correção que não tinha, os subconjuntos de fonte, o `hreflang` sem teste.

---

## 5. Controles

`Docs/controles/public-site.tsv`, **17**, todos no alvo. Além dos invariantes da spec, dois nasceram
na execução: **C16** (nenhum identificador de catálogo digitado em código fora do
`CatalogSeedData.cs` — cobre os 7 slugs e os 4 códigos de zona) e seu alcance **C17**. A prova do
lado negativo do C16 foi reproduzida pelo revisor `[V]`: um literal acrescentado a uma cópia da
árvore move o controle de 0 para 1, inclusive dentro de comentário.

De `foundation.tsv`, **uma** linha mudou: o rótulo do C16, de `EXPIRA COM Q9` para
`EXPIRA COM Q12` `[V]`. Ele exige `≥ 4` marcadores e mede exatamente 4 — **verde encostado no
limiar, margem zero**. Quando a Q12 fechar, **aposentar o controle junto; nunca baixar o limiar.**

---

## 6. Uma limitação do revisor, para quem herdar o posto

**`dotnet` não é alcançável pelo shell da ponte de arquivos.** Rodar
`medir-controles.sh verificar foundation.tsv` do lado do Claude Web devolve `hoje=127` para os dois
controles de build e teste — `command not found`, o instrumento do revisor, não a árvore. Numa
parada isso quase virou relato de portão vermelho. **Todo número de build e de suíte deste resumo é
`[H]`, lido do relato do agente.** A correção proposta está em `Docs/atrito-conversa-3.md`.

---

## 7. Decisões permanentes

- **Item 7 do protocolo** — já em `Docs/protocolo-conversa.md` `[V]`.
- **Correção de revisão de plano nasce como nota de emenda datada no topo da spec**, nunca por
  reescrita do corpo, e termina com o comando que prova que o agente a leu — já no item 7.
- **Padrão de banco nunca em `bool` não anulável** — a regra é do EF, não do projeto, mas o projeto
  já pagou por ela. Vale para toda coluna booleana nova.
- **Identificador de catálogo é dado, não código** — controle C16 do `public-site.tsv`.
- **Toda varredura nova sobre `src/` leva `--exclude-dir=bin --exclude-dir=obj`.**
- **Conjunto de páginas públicas vem do framework** (`IActionDescriptorCollectionProvider`), nunca
  de lista digitada; lista digitada só sobrevive como segunda opinião, com teste afirmando que as
  duas coincidem.

---

## 8. Pendências

**Do Rod (validação humana que a automação não alcança):**

1. **Push** — 13 commits à frente do remoto `[V]`.
2. **Lighthouse**, categoria acessibilidade, em `/` e `/rentals`, alvo ≥ 95. É o item 13 da
   `Docs/conferencia-leva-02.md` e o único "done means" da fase 2 do `roadmap.md` sem número.
3. **Passeio de Tab no desktop e o site no telefone a 375 px** — itens 7, 8 e 9 da conferência têm a
   metade mecânica verde e a metade humana aberta.
4. **Q13** — ler as etiquetas: nomes exatos dos modelos e contagem por modelo. **Q12** — o número do
   WhatsApp; quando entrar, aposentar o C16 do `foundation.tsv`.

**Backlog consciente** (`Docs/backlog-conhecido.md`, quatro itens novos nesta conversa):
o padrão de `IsActive`, o acoplamento dos dois testes que sobem de `AppContext.BaseDirectory`, a
frase falsa sobre `latin-ext` na D7/01, e o `ForwardedHeaders` do App Service.

**De conversas anteriores, ainda de pé:** Q3–Q7 abertas; espanhol; caução como hold no cartão.

---

## 9. Próximas frentes candidatas

- **Leva 03 — reserva.** É o que a leva 02 deliberadamente não fez, e o que transforma o site em
  negócio. Precisa de Q3–Q6 respondidas antes da spec.
- **Leva 04 — CRUD do administrador.** Mais barata que a 03 e desbloqueia o Rod editar conteúdo sem
  passar por commit. **Leva junto o conserto do `IsActive`**, que é onde ele morde.
- **Imagens (D30).** Não é leva: é a skill `preparo-imagem-site` mais uma coluna que já funciona.
  Reversível, e muda muito a página.

---

## 10. Abertura da próxima conversa

1. **Você — ação:** empurrar o que está pronto.
   ```
   git push origin main
   ```
2. **Você — ação:** rodar o Lighthouse (Chrome DevTools → Lighthouse → só Accessibility) em
   `https://localhost:7420/` e `/rentals`, e anotar os dois números.
3. **Cole no Claude (Cowork):** *"abrindo a conversa 4 do Orlando Up. Leva 02 fechada em `383fe27`
   / `f59c0e2`. Lighthouse deu &lt;número&gt; em `/` e &lt;número&gt; em `/rentals`. Quero decidir a
   frente da conversa 4."* — depois do item 2, não antes, porque o número do Lighthouse pode virar
   uma frente de acessibilidade em vez da leva seguinte.
4. **Você — decisão:** leva 03 (reserva) ou leva 04 (CRUD)? A 03 precisa de Q3–Q6; a 04 não precisa
   de nada e leva o conserto do `IsActive` junto.
5. **Cole no Claude Code:** só depois de a spec da frente escolhida existir e de a linha nascer em
   `Docs/fila-cc.md`. Nada antes disso.

---

*O site público existe e é honesto: todo número nele veio da D26 ou da D27, e o que ninguém mediu
aparece como ausência em vez de valor inventado. O que falta para ele ser um negócio é a reserva —
e o que falta para ele ser publicável é um número de Lighthouse e um push.*
