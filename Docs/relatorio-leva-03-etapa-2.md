# Relatório — leva 03, etapa 2 (P2): domínio, regras e carregadores verdes, **nenhuma tela**

**Data:** 2026-09-13. **HEAD ao escrever:** `14e80e4`. **Spec:** `Docs/spec-03-booking-core.md`
com a **EMENDA-03-01** no topo. **Revisão da P1:** aprovada, veredito *EXECUTAR*, sem correção.

**Nenhum arquivo `.cshtml` foi criado.** A regra de disponibilidade e a cotação estão escritas e
medidas antes de existir tela que as mostre, que é a razão de esta parada existir: regra errada
debaixo de tela pronta é o defeito que parece pronto.

---

## 1. A leitura que a P1 deixou pendente: o `sys.default_constraints` depois de aplicada

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d OrlandoUpDb -E -C -Q "SELECT DB_NAME();"
OrlandoUpDb

__EFMigrationsHistory: 5 linhas, a última 20260913215438_AddBookings
```

| Antes (linha de base da P1 §3.3) | Depois |
|---|---|
| `DeliveryZones.SalesTaxRate` | `DeliveryZones.SalesTaxRate` |
| `Products.TurnaroundDays` | `Products.TurnaroundDays` |
| **2 no total** | **2 no total** |

**A correção 1 da EMENDA-03-01 funcionou, e está medida:** `OperationalSettings.NextDayCutoffHour`
**não tem constraint nenhum**, e a linha única lê **`NextDayCutoffHour = 18`**. O valor ficou; o
padrão de banco não ficou. É o par que a D35 comprou caro e que a D34 protege, e desta vez foi
tratado no `Up` em vez de descoberto depois.

Os dois constraints que sobraram são os mesmos de sempre e **continuam legítimos** — o modelo os
declara (`ProductConfiguration.cs:25`, `DeliveryZoneConfiguration.cs:21`). Um terceiro teria sido
parada; não há terceiro.

### 1.1 Uma correção de número que devo à P1

Na P1 escrevi **10 índices e 7 chaves estrangeiras**; a revisão registrou 11 e 8. Contei agora no
banco construído, que é o árbitro: **10 índices fora das chaves primárias, 1 deles único
(`IX_Bookings_Number`), e 7 chaves estrangeiras** nas quatro tabelas. Os números da P1 estavam
certos. Registro aqui porque os dois estão em artefato commitado e a diferença não se resolve
sozinha.

### 1.2 O passo humano 3 ainda não foi feito, e isso é esperado

`TurnaroundDays` lê **0** nos sete produtos, e `single-stroller` continua `IsBookable = 1`. Nada
disso bloqueia o P2 — o host de teste semeia o seu próprio catálogo, com o **1** que o seed agora
carrega, e é contra ele que as regras foram medidas. Mas **a disponibilidade do site que roda
continua um dia mais generosa do que deveria** até você abrir `/admin/produtos`. A recomendação da
revisão (desmarcar "Reservável" no carrinho simples) também está pendente.

---

## 2. O que esta etapa escreveu

**Regras, em `Domain/`, sem banco à vista** — o teste de arquitetura continua proibindo esta pasta
de referenciar qualquer outra, e é ele que impede um `DbContext` de entrar em `Availability.cs`:

- **`BookingStatusRules.cs`** — `HoldsInventory` (as cinco situações que seguram equipamento) e
  `CanTransition` (a máquina inteira da `Docs/architecture.md` §3, escrita agora para que nenhuma
  frente futura a redesenhe de memória).
- **`Availability.cs`** — a função pura que conta **unidades E baterias E carregadores por dia**,
  com o intervalo aplicado às linhas existentes e comparado contra o pedido cru.
- **`Quote.cs`** — a cotação que **falha fechada**: lista de preços com vão, comprimento fora de
  1–60, valor negativo — cada um devolve o motivo nomeado e **nenhum valor**.
- **`BookingRules.cs`** — os limites (1 a 60 dias), o primeiro dia possível para visitante e para
  equipe, e o formato do número.
- **`DeliveryWindows.cs`** já veio na P1.

**Os dois únicos membros da aplicação que atribuem situação**, ambos em `Domain/Booking.cs`:
`Booking.CreateByStaff(...)` e `Booking.Cancel(...)`. **Medido: `Status = BookingStatus.` ocorre
0 vezes sob `Pages/` e `Infrastructure/`, e exatamente 2 vezes sob `Domain/`** — que é o C03/C04 do
`booking-core.tsv` fechando pela primeira vez, e é o que a correção 2 da EMENDA-03-01 pediu.

**Carregadores, em `Infrastructure/Data/`:** `AvailabilityQueries`, `QuoteBuilder`, `BookingWriter`
e `BookingTimeline`, os quatro registrados escopados em `Program.cs`.

**O relógio:** `IClock` ganhou `NowInOrlando()`, `SystemClock` o implementa a partir do fuso que já
resolvia, e `tests/OrlandoUp.Tests/FakeClock.cs` **delega a `new SystemClock(() => _utcNow)`**
(EMENDA-03-01, correção 4). **`foundation.tsv` C06 continua lendo exatamente
`src/OrlandoUp.Web/Infrastructure/SystemClock.cs`** — medido, não suposto.

---

## 3. As quatro cenas da §5.4, reproduzidas como teste com os números

| Cena | O que o teste afirma | Medido |
|---|---|---|
| **1 — a bateria acaba primeiro** | Três Scouts com segunda bateria ocupam as seis baterias. O quarto Scout, **sem** segunda: unidades livres **1**, baterias livres **0** → recusado. | `UnitsFree = 1`, `BatteriesFree = 0`, `IsAvailable = false` |
| **2 — o intervalo** | Scout de 10 a 12/12 ocupa, com o intervalo, de 9 a 13. No dia 13: `q = 4` recusado, `q = 3` aceito. No dia 14: `q = 4` aceito. | `UnitsFree = 3` no dia 13 |
| **3 — cancelar devolve** | Situação `Cancelled` não segura estoque, então a reserva some do diário e o dia 13 volta a aceitar `q = 4`. | provado também pelo serviço, §4 |
| **4 — o corte** | 05/12 22:59 UTC = 17:59 em Orlando → 06/12; 23:00 UTC = 18:00 → 07/12. Gêmea de julho: 05/07 21:59 UTC → 06/07; 22:00 UTC → 07/07. | as quatro linhas passam |

A cena 4 é a que compra o relógio falso: os quatro instantes atravessam a virada do horário de
verão, e a diferença entre dezembro e julho é uma hora de deslocamento — que é exatamente o erro
que um teste lendo o relógio da máquina não veria.

**Cada cena tem metade de presença.** A cena 1 afirma que o mesmo pedido contra um diário vazio é
aceito, para que a recusa seja das reservas e não da regra recusando tudo. É a regra da spec —
*"todo teste de ausência afirma uma presença antes, no mesmo método"* — e vale para todos os testes
desta etapa.

### 3.1 A cena do carregador, que a §9.1 pede com aritmética própria

Três Scouts com segunda e três Spitfires com segunda: **12 carregadores dos 14**, e cada pool de
baterias exatamente cheio em 6. Um Scout a mais sem segunda seria 13 ≤ 14 no carregador — e é
**recusado pelo pool do próprio modelo**, que está em zero. O teste afirma `ChargersFree = 2` e
`BatteriesFree = 0` no mesmo método, porque é a única forma de dizer **qual dos três limites
mordeu**.

---

## 4. O que os carregadores provam contra o schema de verdade

Onze testes novos rodam contra o SQLite com o catálogo semeado, as doze baterias e a linha de
configuração, e passam por `BookingWriter`/`AvailabilityQueries` — nunca por uma tela:

- a frota semeada **é** 4 unidades, 6 baterias, intervalo **1** e 14 carregadores, lida do banco e
  não reafirmada;
- uma reserva lançada pela equipe nasce `OU-` + seis dígitos, `Confirmed`, `Staff`, **Days 5**,
  **Subtotal 160 / baterias 40 / extras 5 / entrega 0 / imposto 0 / total 205** — os números da §9.2;
- **uma linha em `BookingEvents` e ZERO linhas em `AuditEntries` para `nameof(Booking)`** — a
  presença e a ausência no mesmo método;
- o quinto scooter sobre quatro é **recusado** com `MaxQuantity = 0`, e a recusa **não escreve
  nada**: nem rascunho, nem evento;
- o mesmo pedido com a caixa marcada grava `IsOverbooked = true` e a palavra *overbooked* na
  primeira linha do histórico;
- cancelar devolve as máquinas **medido pela disponibilidade antes e depois**, e escreve a segunda
  linha de histórico; cancelar de novo estoura no domínio e **não escreve uma terceira**;
- o nome do produto guardado na linha **sobrevive** a renomear o produto pelo catálogo depois — e o
  teste afirma que o catálogo realmente mudou, senão não provaria nada sobre instantâneo;
- uma lista de preços com vão faz a reserva **falhar fechada**, sem `Breakdown` nenhum, e **zero**
  reservas escritas;
- sem a linha de `OperationalSettings` o carregador **estoura** em vez de contar zero carregadores —
  e a metade de presença, com a linha lá, responde.

---

## 5. Portão medido ao fechar esta etapa

```
dotnet build   Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test    Passed! Failed: 0, Passed: 251, Skipped: 0, Total: 251
```

**251 testes**, contra 183 ao fim da P1 — **68 novos**. Atributos `[Fact]`/`[Theory]`: **148**
(eram 107 na medição de abertura da leva; a §9.5 manda relatar o número, nunca afirmá-lo).

| Arquivo de controle | Resultado |
|---|---|
| `admin-catalog.tsv` | 12 controles, **0 fora do esperado** |
| `public-site.tsv` | 17 controles, **0 fora do esperado** |
| `fleet-batteries.tsv` | 7 controles, **0 fora do esperado** |
| `foundation.tsv` | 18 controles, **0 fora do esperado** |

### 5.1 Os controles do `booking-core.tsv` futuro, medidos hoje

O arquivo nasce na P3 (§11.1), mas os comandos já são os do plano e já medem:

| Controle | No HEAD inicial | Agora | Alvo na P3 |
|---|---|---|---|
| C02 alcance — identificadores de catálogo no `QuoteBuilder` | nao | **sim** (4) | sim |
| C03 — `Status = BookingStatus.` sob `Pages/`+`Infrastructure/` | 0 | **0** | 0 |
| C04 alcance — o mesmo sob `Domain/` | nao | **sim** (2) | sim |
| C07 — arquivos que leem `HoldsInventory` | nenhum | **2 dos 3** | os 3 |
| C08 — `FirstOrDefault` em `AvailabilityQueries` | 0 | **0** | 0 |
| C09 alcance — `SingleAsync` ali | nao | **sim** (2) | sim |
| C12 — `TurnaroundDays = 0` no seeder | 1 | **0** | 0 |
| C14 — `SumAsync`/`MaxAsync` nos serviços de reserva | 0 | **0** | 0 |
| C15 alcance — `.Sum(`/`.Max(` em `Availability.cs` | nao | **sim** (5) | sim |

O **C07 lê 2 e não 3** de propósito: o terceiro leitor é o painel (`Pages/Admin/Index.cshtml.cs`),
que ganha a contagem de reservas ativas na P3. Os C01/C05/C06/C10/C11/C13 dependem de telas e de
recursos, e movem-se lá.

---

## 6. Uma coisa que eu decidi e você pode desfazer

A `Quote` devolve `QuoteProblem.NoTierCovers` para o caso de nenhuma faixa cobrir o comprimento
pedido. **Esse caso é inalcançável pelas telas**, porque a lista já foi validada como `None` duas
linhas antes e uma lista válida cobre todo comprimento. Escrevi assim mesmo, e o comentário diz por
quê: a alternativa a nomear o impossível é desreferenciá-lo. Se você preferir que essa hipótese
vire uma exceção em vez de um valor de retorno, é uma linha.

---

## 7. O que vem na P3, e o que é seu agora

**P3 (eu):** as quatro telas (`/book` pública, `/admin/bookings` Index/Create/Details), os dois
`.resx` com paridade, o botão desabilitado de `Pages/Rentals/Details.cshtml` trocado pelo link, o
sétimo destino do `_AdminLayout`, a contagem do painel, o campo de corte em Configurações, os
testes de host, os cardinais que se movem (15 → 32 nas chaves por interpolação, 21 → 23 na lista
pública), a nota datada de **seis pontos** no `Docs/architecture.md` (EMENDA-03-01, correção 3), o
`Docs/controles/booking-core.tsv` e o `verificar` nos **cinco** `.tsv`.

**Você, quando puder — não bloqueia a P3:**

1. `/admin/produtos`: "Dias de intervalo" = **1** em *Drive Scout 4*, *Drive Spitfire EX* e
   *Cadeira de rodas Drive*;
2. no *Carrinho simples*, desmarcar **"Reservável"** (recomendação da revisão da P1) — ou pôr **1**
   no intervalo dele também, se quiser mantê-lo à venda com zero unidades;
3. aprovar esta etapa.

**O push é seu.** Nenhum commit desta sessão foi empurrado.

---

## Revisão (Claude Web, 2026-09-14)

**Conferido abrindo o código, não o relato:** `Domain/Availability.cs` (padding nas linhas existentes
contra o pedido cru; pior dia decide; pool vazio de scooter recusa; piso zero nos máximos),
`Domain/Quote.cs` (validador único, faixa que cobre, imposto arredondado uma vez `AwayFromZero`),
`Domain/BookingStatusRules.cs` (cinco status seguram estoque; `from == to` é falso),
`Domain/Booking.cs` (fábrica e `Cancel` são as duas únicas atribuições de `Status`),
`Infrastructure/Data/AvailabilityQueries.cs` (`SingleAsync` na linha de configuração; janela
alargada pelo maior `TurnaroundDays` lido, não assumido; `HoldsInventory` filtrado em memória;
nenhum agregado de dinheiro em SQL), `BookingWriter.cs` (número na mesma transação),
`FakeClock.cs` (delegação a `SystemClock(Func)`, como a EMENDA-03-01 pediu) e o `SiteFactory`
(o relógio entra em `ConfigureTestServices`, fora do instantâneo de registros). **148** atributos
de teste nos arquivos, 251 casos executados (relato). O `sys.default_constraints` lê dois, nenhum
em `OperationalSettings` — a correção 1 da EMENDA-03-01 está medida no banco.

**Correção que devo à P1:** a revisão escreveu 11 índices e 8 chaves estrangeiras; o relatório da P1
e o banco dizem **10 e 7** (contei de novo no SQL: 5 + 2 + 2 + 1 índices; 2 + 1 + 2 + 2 FKs). O
agente estava certo nas duas contagens.

**Duas correções antes de qualquer tela, e a primeira inverte resultado:**

1. **O limite de carregadores (e de pool) não soma as linhas do MESMO pedido.**
   `BookingWriter.CreateByStaffAsync` chama `ForProductAsync` por linha, e cada linha é conferida
   contra o diário existente **sozinha**. Cena que passa errado: 11 carregadores ocupados no diário;
   pedido de 1 Scout + segunda bateria e 1 Spitfire + segunda bateria — cada linha pede 2, cada
   uma sozinha lê 13 ≤ 14 e passa; juntas pedem 4 → 15 > 14, e a reserva nasce **acima da frota
   sem `IsOverbooked`**. É exatamente o modo de falha silencioso que a D4/03 existe para marcar.
   **Saída:** `AvailabilityQueries.ForProductAsync` ganha um parâmetro
   `IReadOnlyList<HoldingLine> alsoHolding`, e o `BookingWriter` passa, ao conferir a linha *i*, as
   outras linhas do mesmo pedido como `HoldingLine` sobre as datas do pedido (produto, `IsScooter` e
   `TurnaroundDays` lidos de `ProductFacts`, quantidade e segundas baterias do pedido). O
   `Availability.For` não muda. **Teste (DomainTests ou AdminCrudTests):** a cena acima, recusada
   com o `Shortfall` da segunda linha nomeando o carregador (`MaxExtras` < pedido), e a mesma cena
   com 10 carregadores ocupados, aceita.
2. **A linha do pedido não é validada em lugar nenhum abaixo da tela.** `Quote.For` aceita
   `Quantity = 0` (linha de total zero), `ExtraBatteryCount > Quantity` (a disponibilidade conta
   `q + e` e o cliente paga baterias que a D36 não permite) e segunda bateria numa cadeira de rodas
   (o `Availability` ignora, a `Quote` cobra). A spec §7.3 põe isso na tela, mas *"toda escrita
   passa por serviço para que os invariantes vivam num lugar só"* (`architecture.md` §2), e um
   invariante que só a tela conhece não sobrevive à segunda tela (03b). **Saída, no domínio:**
   `QuoteLineRequest` ganha `bool IsScooter` (o `QuoteBuilder` já carrega o produto e o preenche
   de `Category == MobilityScooter`); `Quote.For` devolve `QuoteProblem.QuantityOutOfRange` para
   `Quantity < 1`, `ExtrasAboveQuantity` para `ExtraBatteryCount < 0` ou `> Quantity`, e
   `ExtrasOnNonScooter` para extra > 0 sem scooter — três membros novos no fim do enum. As telas
   da P3 traduzem os três para as chaves `Book_ErrorExtraOnlyScooters`,
   `Book_ErrorExtraAboveQuantity` e as `Admin_Error…` correspondentes, em vez de validar de novo.
   **Teste:** um caso por problema, e o caso `Quantity = 1, Extra = 1, IsScooter` continua precificado.

**Desvio aceito (§6 do relatório):** `QuoteProblem.NoTierCovers` fica como valor de retorno
nomeado. Correto.

**Confirmado, e não é para "consertar":** `Active → Cancelled` é ilegal na tabela (equipamento em
uso não se cancela; encerra-se), e `Cancelled → Refunded` é a única porta do reembolso; a fábrica
congela a cotação linha a linha; o relógio falso não move teste nenhum.

**Backlog (escrito pelo revisor em `Docs/backlog-conhecido.md`):** a conferência de disponibilidade
e o `INSERT` não são atômicos — duas escritas simultâneas podem passar as duas. Com uma pessoa
lançando reservas é tolerável; o fluxo público da 03b, com hold, precisa de serialização
(transação serializável ou verificação repetida dentro dela).

**Três lembretes para a P3, não correções:** o handler de cancelar confere `CanTransition` antes de
chamar o `CancelAsync` e responde `Admin_ErrorCannotCancel` — a exceção do domínio é a segunda
barreira, não a primeira; `CancelAsync` com id inexistente lança (`SingleAsync`) — a página
responde 404 antes; a nota de seis pontos no `architecture.md` (EMENDA-03-01, correção 3).

**Veredito: executar após as correções 1 e 2** — as duas nascem com os testes delas **antes** do
primeiro `.cshtml` da P3, e o relatório da P3 abre dizendo os dois números (a cena do carregador
recusada; os três `QuoteProblem` novos com um teste cada).

**Cole no Claude Code:** *"leia `Docs/relatorio-leva-03-etapa-2.md` e execute a seção Revisão — a P2 está aprovada com as correções 1 e 2, que vêm antes de qualquer tela; siga à P3."*

