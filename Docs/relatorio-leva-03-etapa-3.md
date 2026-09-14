# Relatório — leva 03, etapa 3 (P3): tudo escrito, suíte verde, cinco `.tsv` verificados

**Data:** 2026-09-14. **HEAD ao escrever:** `3994f1a`. **Spec:** `Docs/spec-03-booking-core.md`
com a **EMENDA-03-01**. **Revisão da P2:** *executar após as correções 1 e 2*, feitas e commitadas
em `3994f1a` **antes do primeiro `.cshtml`**, como a revisão mandou.

---

## 1. Os dois números que a revisão da P2 pediu na abertura

**A cena do carregador, recusada.** Duas linhas de um mesmo pedido — um Scout e um Spitfire, cada
um com segunda bateria — pedem **4 carregadores**; com 3 livres a reserva é recusada, com 4 ela
passa. Antes da correção as duas linhas eram medidas sozinhas contra o diário e **cada uma lia
2 ≤ 3 e passava**, escrevendo uma reserva acima da frota **sem `IsOverbooked`**. Provei que o teste
morde: revertendo o passo de irmãos, **2 dos 5 testes falham**.

**Os três `QuoteProblem` novos, um teste cada:** `QuantityOutOfRange` (linha de zero máquinas),
`ExtrasAboveQuantity` (mais segundas baterias que máquinas, e também negativo) e
`ExtrasOnNonScooter` (segunda bateria em cadeira de rodas). Cada um afirma também que a linha
ordinária continua precificada, e que a recusa nomeia o produto e **não carrega preço nenhum**.

### 1.1 Uma correção à cena que a revisão descreveu

A revisão pediu a cena com **11 carregadores ocupados**. Medi: ela **não é alcançável com a frota
de hoje**. Carregadores ocupados é a soma do consumo de bateria dos dois modelos, cada um limitado
a um pool de seis, então **no máximo 12 dos 14 carregadores podem estar fora** — o limite de
carregador nunca morde primeiro. Pior: para sobrar folga de 1 máquina + 1 segunda em cada modelo,
cada um usa no máximo 4, e o teto cai para **8**.

**O defeito é real e latente, não vivo.** Acorda no dia em que um carregador se perde ou uma
bateria é comprada. Os testes por isso compram um **estoque menor de carregadores** (7 e 8) em vez
de arranjar um diário maior, e um teste irmão afirma a folga de hoje com número: 12 baterias
utilizáveis contra 14 carregadores. Exercitar a regra com a frota que a esconde teria sido um teste
verde medindo nada.

---

## 2. As telas

**`/book` e `/pt/book`** — pública, sem JavaScript, o estado no query string. Responde uma de três
coisas: *Available for these dates* com a tabela de preço; *Sold out for these dates*; ou **a frase
que nenhum outro site de aluguel de Orlando diz** — *The second battery is not available for these
dates* — com o preço **sem** ela. Taxa zero imprime *Taxes included* em vez de `US$ 0.00`. Termina
sempre dizendo que o pagamento chega na próxima entrega.

**`/rentals/{slug}`** — o botão desabilitado e a nota saíram; no lugar, o link para `/book` já
carregando o produto. `Product_BookingSoon` ocorre em **0** arquivos de `src`.

**`/admin/bookings`** (lista, com a tarja *Acima da frota*), **`/admin/bookings/create`** (uma linha
por produto reservável, a caixa de lançar acima da frota) e **`/admin/bookings/{id}`** (toda coluna
guardada, o histórico, e o cancelamento quando a situação permite).

**`_AdminLayout`** passa a **sete** destinos — a prova de build-fresh do roteiro, e há um teste que
conta os `<a>` da navegação. **Painel:** mais uma contagem, *Reservas ativas*. **Configurações:**
mais um campo, o corte para o dia seguinte, com `24` recusado pela mensagem de faixa.

### 2.1 Uma decisão de desenho que a §6.3 da spec me obrigou a rever

A spec diz que o `BookingWriter` grava o evento `Created`. Escrevi assim na P2. **Na P3 isso
quebrou o C05 do `admin-catalog.tsv`**, que conta handlers de POST menos chamadas a `.Record(`
**dentro de `Pages/Admin`** — e a §11.1 **não** me autoriza a mover esse controle.

Medi o padrão da casa antes de decidir: **`CatalogWriter` não toca na trilha de auditoria; quem
chama `.Record(` são as sete páginas**. O serviço muda o dado, a página registra o que fez. Movi os
dois `Record` para os handlers, e o `BookingWriter` deixou de depender do `BookingTimeline`. O C05
fecha em **2** por desenho (11 − 9) e não por sorte, e o comentário do writer diz em que padrão ele
está.

**O que isso custou, e digo porque é uma troca e não uma melhoria pura:** a reserva e o seu primeiro
evento passam a ser gravados em dois `SaveChanges`, não num só. Se o segundo falhasse, existiria uma
reserva sem a primeira linha de histórico. É a mesma janela que toda outra tela de administração já
tem desde a leva 04, e não inventei uma exceção para esta. Se você preferir a atomicidade ao
controle, é o writer que volta a gravar e o C05 que precisa de emenda — sua decisão, não minha.

---

## 3. Achados medidos enquanto escrevia as telas

**(a) A cena da §9.3 da spec não produz a frase que ela afirma.** A spec diz: *"com três Scouts
cada um com segunda bateria já reservados → 'The second battery is not available for these dates'"*.
Aritmética: 3 máquinas + 3 segundas = **6 baterias**, o pool inteiro. Um quarto Scout **sem**
segunda ainda precisa da sua própria bateria — a sétima, que não existe. Isso é **esgotado**, não
"falta a segunda". A frase exige exatamente **uma** bateria livre: o teste ocupa **3 máquinas + 2
segundas** (5 de 6), e aí o quarto Scout sai e a segunda dele não.

**(b) A condição que eu tinha escrito para essa frase estava errada** e foi o teste que mostrou:
eu comparava a quantidade com `MaxQuantity`, o que não distingue *"a máquina está livre e a bateria
reserva não"* de *"não há bateria nem para a máquina"*. Agora a página **pergunta de novo, sem a
segunda bateria**, e só usa a frase quando essa segunda pergunta é aceita.

**(c) Introduzi e removi, antes de compilar, a própria EMENDA-04B-04.** A primeira versão da página
pública lia `NextDayCutoffHour` por projeção de `int` com `SingleOrDefaultAsync` — que devolve
**zero** quando a linha não existe. Um corte de zero não é ausência: é a afirmação de que o
escritório para de aceitar entrega para amanhã à meia-noite, e moveria o primeiro dia possível de
todo visitante. Agora a linha é lida como **linha**, a ausência sobrevive como ausência, e há teste:
sem a linha de configuração a página diz que não consegue conferir e **não mostra preço**.

**(d) O `Razor` codifica todo caractere não-ASCII**, então as frases com acento chegam como
entidades e uma asserção crua falha com a página certa. As asserções em português passam por
`HtmlDecode`, como as outras deste arquivo já faziam.

**(e) A rota precisava ser declarada.** `Pages/Book/Index.cshtml` sem rota explícita produzia
`/Book` e `/pt/Book/Index`. O padrão da casa é `@page "/kebab-case"`; com `@page "/book"` a
convenção de cultura gera `/pt/book`, e o teste que compara a lista derivada com a digitada voltou
a fechar.

---

## 4. Portão medido ao fechar

```
dotnet build   Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test    Passed! Failed: 0, Passed: 286, Skipped: 0, Total: 286
```

| Arquivo de controle | Resultado |
|---|---|
| `admin-catalog.tsv` | 12 controles, **0 fora do esperado** |
| `public-site.tsv` | 17 controles, **0 fora do esperado** |
| `fleet-batteries.tsv` | 7 controles, **0 fora do esperado** |
| `foundation.tsv` | 18 controles, **0 fora do esperado** |
| `booking-core.tsv` | **15 controles, 0 fora do esperado** |

**Sessenta e nove controles em cinco arquivos, todos no alvo.**

### 4.1 Os quinze do `booking-core.tsv`, do HEAD inicial até agora

| Controle | HEAD `7d0384b` | Agora |
|---|---|---|
| C01 a tela de detalhe nao le preco de catalogo | 0 | **0** |
| C02 ALCANCE os identificadores existem na cotacao | nao | **sim** |
| C03 nenhuma pagina nem servico atribui situacao | 0 | **0** |
| C04 ALCANCE a atribuicao existe no dominio | nao | **sim** |
| C05 a pagina publica nunca lanca acima da frota | 0 | **0** |
| C06 ALCANCE a marca existe na tela da equipe | nao | **sim** |
| C07 quem le `HoldsInventory` | nenhum | **os três caminhos exatos** |
| C08 sem `FirstOrDefault` no carregador | 0 | **0** |
| C09 ALCANCE `SingleAsync` ali | nao | **sim** |
| C10 a chave do botao desabilitado sumiu | 3 | **0** |
| C11 ALCANCE a pagina de produto chama a nova tela | nao | **sim** |
| C12 o semeador nao escreve intervalo literal | 1 | **0** |
| C13 ALCANCE o intervalo e dado do seed | nao | **sim** |
| C14 sem soma nem maximo pedidos ao banco | 0 | **0** |
| C15 ALCANCE a conta em memoria existe | nao | **sim** |

Os sete irmãos de alcance foram de `nao` a `sim`, o C07 de `nenhum` aos três caminhos, o C10 de 3 a
0 e o C12 de 1 a 0 — **é isso que prova que a leva aconteceu**, e não os cinco negativos, que liam
zero antes só porque os arquivos não existiam.

### 4.2 Cardinais e números que se moveram

| Medida | Antes | Agora |
|---|---|---|
| `AdminCrudTests` chaves por interpolação | 15 | **32** (11 situações + 4 janelas + 2 origens) |
| `AdminCrudTests` colunas sem padrão de banco | 15 | **16** (movido na P1) |
| `PublicPathList` | 21 | **23** |
| `admin-catalog` C05 (handlers − registros) | 9 − 7 = 2 | **11 − 9 = 2** |
| `public-site` C10 (`asp-page` − `asp-route-culture`) | 22 − 22 = 0 | **24 − 24 = 0** |
| `fleet-batteries` C06 | 5 | **6** (movido na P1) |
| Chaves em cada `.resx` | 293 | **401** (110 novas, 2 removidas) |
| Atributos `[Fact]`/`[Theory]` | 107 | **176** — relatado, nunca afirmado (§9.5) |
| Casos executados | 183 | **286** |

Uma chave da lista da §8 **já existia** e eu a reusei em vez de criar uma segunda com o mesmo
texto: `Admin_ColStatus`, escrita na leva 04. Por isso 110 novas e não 111.

---

## 5. Fora do escopo, conferido

`git status --short` inteiro lista **16** entradas, e todas caem na lista fechada da §11.1.
Nenhum arquivo negativo foi tocado: nem `CLAUDE.md`, nem `decisions.md`, `roadmap.md`,
`open-questions.md`, `medir-controles.sh`, `.githooks/`, `.github/`, `appsettings.json`, nem as
specs, conferências e relatórios das levas anteriores, nem `Docs/controles/{foundation,public-site}.tsv`.
`Docs/controles/admin-catalog.tsv`: **zero** linhas alteradas — o C05 fechou sozinho e o piso do
C06 não precisou mover. `Docs/architecture.md`: uma nota datada, a de seis pontos que a
EMENDA-03-01 autorizou. **`Docs/conferencia-leva-03.md` não foi tocado** — a coluna Resultado é sua.

---

## 6. O que falta, e é seu

1. **A conferência visual** (`Docs/conferencia-leva-03.md`), que é o roteiro que já está na árvore.
   O item 0 é a prova de build-fresh: a navegação da administração tem **sete** destinos.
2. **`/admin/produtos`: "Dias de intervalo" = 1.** Continua pendente — medi de novo hoje e os sete
   produtos leem **0**. Até isso, a disponibilidade do site que roda é um dia mais generosa.
3. **O carrinho simples continua `Reservável` com zero unidades.** A recomendação da revisão da P1
   é desmarcar. **Agora isso é visível**: ele aparece no seletor de `/book` e responde *esgotado*
   em qualquer data.
4. **Aprovar a P3**, para eu fazer o commit de fechamento que grava o hash do commit de conteúdo na
   coluna Commit da linha da fila.

**O push é seu.** Nenhum commit desta sessão foi empurrado.

---

## Revisão (Claude Web, 2026-09-14)

**Medido pelo revisor no HEAD `84dea21`:** `verificar` nos cinco `.tsv` — `admin-catalog` 12/12,
`booking-core` 15/15, `fleet-batteries` 7/7, `public-site` 17/17, `foundation` 16/18 (C14 e C15
respondem 127 ao shell da ponte, como sempre; o relato do agente os dá verdes com 286 testes).
`public-site` C10: 24 e 24. `admin-catalog` C05: 11 − 9 = 2. Os dois `.resx` com **401** chaves cada,
`Product_BookingSoon` em **0** arquivos, a rota `@page "/book"`, `PublicPathList` com 23 endereços,
`RenderedTextTests` com `/book` e `/pt/book`, a nota de seis pontos no `architecture.md`, as duas
correções da P2 no código (`alsoHolding` no carregador e no writer; os três `QuoteProblem` 8, 9 e
10 no fim do enum). As catorze frases em português que o roteiro cita foram lidas do `.resx` e
batem palavra por palavra. A superfície do diff (27 arquivos) está inteira dentro da §11.1.

**Aceito como melhoria, e volta para a spec (EMENDA-03-02):** a cena da §9.3 *"três Scouts com
segunda → a segunda não está disponível"* estava errada — 3 + 3 = 6 esgota o pool e o quarto Scout
não sai nem sem segunda; a frase exige **uma** bateria livre (3 máquinas + 2 segundas). E a cena do
carregador com 11 ocupados é inalcançável com a frota de hoje (dois pools de 6 limitam a 12 de 14
carregadores), então os testes compram um estoque menor e um teste irmão registra a folga real. Os
dois achados são do agente e estão certos.

**Uma correção antes do fechamento, e ela desfaz um recuo da P2:**

1. **O evento `Created` saiu da transação.** Na P2 o `BookingWriter` gravava o evento **dentro** da
   transação que insere a reserva e o número; na P3 os dois `Record` foram movidos para os handlers
   (`Create.cshtml.cs:176`, `Details.cshtml.cs:87`) e o writer não grava mais nada — para que o C05
   do `admin-catalog.tsv` continuasse contando *handler menos registro*. O controle moldou o código,
   e o custo é duplo: a reserva é commitada e **só depois** o evento nasce (uma falha entre os dois
   deixa reserva sem histórico), e o invariante *"toda reserva nasce com o seu evento"* passa a
   morar na página — disciplina, não barreira — justamente onde a 03b, que não tem handler de
   administração, vai esquecê-lo. **Saída:** os dois `_timeline.Record(` voltam para
   `BookingWriter.CreateByStaffAsync` (antes do segundo `SaveChanges`, dentro da transação) e
   `CancelAsync`; os handlers só chamam o writer. O C05 do `admin-catalog.tsv` é **emendado, e a
   emenda não vem sozinha**: o operando `c` passa a contar `[.]Record[(]` **ou**
   `_writer[.](CreateByStaffAsync|CancelAsync)[(]`, com o rótulo dizendo *"registro de auditoria
   ou escrita pelo writer de reserva"*; e o `booking-core.tsv` ganha **C16** — `_timeline[.]Record[(]`
   em `Infrastructure/Data/BookingWriter.cs` lê **2** (um por método de escrita) — e **C17**, seu
   alcance: `_writer[.]` em `Pages/Admin/Bookings/` ≥ 2. **Teste:** chamar `CreateByStaffAsync`
   direto (sem página) e afirmar que o evento `Created` existe com o número no texto; o par de
   testes que conta **2** eventos continua passando. *Se passar sem isso:* a 03b cria reservas por
   outro caminho e a primeira reserva online nasce sem histórico, com tudo verde.

**Confirmado, não é para "consertar":** o guarda duplo do cancelar (`CanTransition` na página,
exceção no domínio); a linha de configuração lida como linha na página pública (achado (c) do
agente, a EMENDA-04B-04 evitada antes de compilar); `HtmlDecode` nas asserções em português; o
`Docs/conferencia-leva-03.md` intocado.

**Veredito: executar após a correção 1** — ela entra no commit de conteúdo, com o `verificar` nos
cinco `.tsv` refeito. Depois dela, a conferência visual é do operador (`Docs/conferencia-leva-03.md`),
e o fechamento em dois commits vem depois da conferência, nunca antes.

**Cole no Claude Code:** *"leia `Docs/relatorio-leva-03-etapa-3.md` e execute a seção Revisão — a correção 1 vai para o commit de conteúdo; depois pare e espere a conferência visual."*

---

## 8. Correção 1 da revisão da P3, executada em 2026-09-14

**O evento `Created` voltou para dentro da transação do writer**, e os handlers voltaram a só
chamar o writer. A revisão está certa e o meu recuo da P3 estava errado pelo motivo que ela dá: o
invariante *"toda reserva nasce com o seu evento"* passou a morar na página, e a 03b cria reservas
**sem handler de administração nenhum** — disciplina não alcança aquele chamador, uma escrita dentro
desta transação alcança. O comentário do `BookingWriter` agora diz isso, e diz também por que o
padrão das telas de catálogo (serviço muda, página registra) não vale aqui: lá a trilha é um
registro que ninguém lê de volta, aqui o histórico **é** o registro da reserva.

**O controle foi emendado, não contornado.** O C05 do `admin-catalog.tsv` passa a contar, no operando
menor, `[.]Record[(]` **ou** `_writer[.](CreateByStaffAsync|CancelAsync)[(]`, e o rótulo passa a
dizer *"registro de auditoria ou escrita pelo writer de reserva"*. Uma linha alterada, a do C05 —
e registro que isso **alarga** o que a §11.1 me autorizava naquele arquivo (só o piso do C06), por
instrução explícita da revisão. Medido: `a = 11`, `c = 9`, **diferença 2**.

**E a emenda não veio sozinha**, que é o que impede o operando novo de virar uma porta aberta:

| Controle novo | O que afirma | Medido |
|---|---|---|
| `booking-core` **C16** | `_timeline[.]Record[(]` em `BookingWriter.cs` | **2** — um por método de escrita |
| `booking-core` **C17** | ALCANCE: `_writer[.]` em `Pages/Admin/Bookings/` ≥ 2 | **sim** (2) |

**O teste que a revisão pediu existe e morde.** `CreateByStaffAsync` é chamado **sem página
nenhuma** e o teste afirma que o evento `Created` existe, com o ator e **com o número da reserva no
texto**. Provei que morde: neutralizando o `Record` do writer, **5 testes falham**, entre eles os
dois pares que contam 2 eventos. Os testes de serviço voltaram a afirmar o histórico — e agora isso
significa algo que os testes de tela não diziam: o caminho sem tela também nasce com histórico.

### 8.1 Portão refeito

```
dotnet build   Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test    Passed! Failed: 0, Passed: 286, Skipped: 0, Total: 286
```

| Arquivo de controle | Resultado |
|---|---|
| `admin-catalog.tsv` | 12 controles, **0 fora do esperado** |
| `public-site.tsv` | 17 controles, **0 fora do esperado** |
| `fleet-batteries.tsv` | 7 controles, **0 fora do esperado** |
| `foundation.tsv` | 18 controles, **0 fora do esperado** |
| `booking-core.tsv` | **17 controles, 0 fora do esperado** |

**Setenta e um controles em cinco arquivos, todos no alvo.** O `booking-core.tsv` foi de 15 para 17.

### 8.2 O que NÃO fiz, e é de propósito

**Não fiz o commit de fechamento.** A revisão diz que ele vem depois da conferência visual, nunca
antes, e a coluna Estado da linha da fila continua lendo `aguardando`. **O commit de conteúdo cuja
hash o fechamento vai gravar é o desta correção**, não o `84dea21` — porque é este que deixa a
árvore no estado que a conferência vai conferir.

`Docs/conferencia-leva-03.md` continua intocado. A bola é sua: o roteiro, o "Dias de intervalo" = 1
nos produtos reserváveis (medido hoje: os sete ainda leem **0**), e a decisão sobre o carrinho
simples.

