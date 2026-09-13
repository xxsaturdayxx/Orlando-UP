# Relatório — leva 03, etapa 1 (P1): migration `AddBookings` escrita e **NÃO aplicada**

**Data:** 2026-09-13. **HEAD ao escrever:** `70d6d68`. **Spec:** `Docs/spec-03-booking-core.md`
com a nota **EMENDA-03-01** no topo, que vence o corpo onde discordarem.
**Linha da fila:** `2026-09-12`, "LEVA 03 — BOOKING CORE".

**Prova de leitura exigida pela emenda:** `grep -c "EMENDA-03-01" Docs/relatorio-leva-03-etapa-1.md`
devolve **5** — o comando conta **linhas** que contêm a cadeia, e não ocorrências; aqui os dois
números coincidem porque não há linha com duas, e medi os dois (`grep -o … | wc -l` também dá 5)
em vez de supor que fossem a mesma coisa.

**A migration não foi aplicada.** `__EFMigrationsHistory` do `OrlandoUpDb` continua com as mesmas
quatro linhas de antes, e a quinta só entra quando você rodar o comando da §6.

---

## 1. As duas correções da EMENDA-03-01 que mudaram o que eu escrevi, ditas por mim

**Correção 1 — o default constraint sai no mesmo `Up`, e não "se tiver sobrado".** Eu ia ler o
`sys.default_constraints` depois de aplicar e decidir ali. A emenda tirou a decisão de depois e a
pôs no código: `ADD [NextDayCutoffHour] int NOT NULL DEFAULT 18` **preenche a linha única que já
existe**, e é só para isso que o 18 está ali — mas o SQL Server não sabe disso e transforma o
argumento num constraint **permanente**, com nome gerado, que nenhuma migration seguinte desfaz.
O resultado seria o modelo dizendo *"esta coluna não tem padrão de banco"* e o banco tendo um para
sempre: a D35 na letra, na coluna que a D34 existe para proteger. Então o `Up` procura o constraint
pelo par tabela/coluna em `sys.default_constraints` e o derruba pelo nome, em SQL dinâmico porque o
nome é gerado. **O valor 18 fica na linha; o constraint não fica.**

Registro também que **a ferramenta gerou `defaultValue: 0`, não 18** — eu troquei. Um zero teria
significado que todo visitante já vê depois de amanhã como primeiro dia de entrega possível,
porque `now.Hour < 0` é falso sempre.

**Correção 2 — a barreira é o tipo, não a disciplina.** Eu tinha proposto só a fábrica em
`Domain/`. A emenda acrescentou o que faz a proposta valer: **`Booking.Status` é declarado
`{ get; private set; }`**. O EF Core materializa por setter privado, então a persistência não muda;
o que muda é que uma página que tentasse `booking.Status = x` **não compila**. O C03 do
`booking-core.tsv` continua existindo como cinto, mas ele mede o que já é impossível — e essa é a
ordem certa das duas coisas.

---

## 2. O banco antes, medido — `SELECT DB_NAME()` colado

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d OrlandoUpDb -E -C -Q "SELECT DB_NAME();"
OrlandoUpDb
```

| Tabela | Linhas em 13/09 |
|---|---:|
| `Products` | **7** |
| `Units` | **10** |
| `Batteries` | **13** |
| `OperationalSettings` | **1** |
| `PricingTiers` | **10** |
| `AddOns` | **6** |
| `DeliveryZones` | **4** |
| `DeliveryLocations` | **10** |
| `AuditEntries` | **14** |

`__EFMigrationsHistory`: quatro linhas, a última `20260910031613_AddBatteriesAndOperationalSettings`.

### 2.1 Três coisas no banco que a spec dá como outra coisa — nenhuma é parada, uma muda o passo humano

A §3 da spec traz esses fatos como `[H]` — *herdados, não reconferidos*. Reconferi, e três mudaram:

**(a) `Batteries` tem 13 linhas, não 12.** A décima terceira é `BSC-22`, `Status = 3` (**Baixada**),
no primeiro modelo de scooter. É a bateria que o cliente quebrou, lançada pela tela. **A conta da
spec continua inteira:** a regra de disponibilidade conta `Status == Available`, e o pool disponível
é 6 + 6 = **12**, exatamente o número que a D36 usa. É também, de graça, o caso real do teste
*"uma bateria baixada não conta"* da §9.1.

**(b) `LostChargerFee` lê 32,00, e a migration da leva 04b escreveu 30,00.** Você editou pela tela
de Configurações, que é o que ela existe para fazer. Nenhuma consequência nesta leva — a cotação
não lê esse valor.

**(c) `single-stroller` está `IsBookable = 1` no banco, com lista de preços própria** (1–4 dias
por dia **15,00**; 5+ por dia **12,00**). O seed escreve `false` para os quatro carrinhos (D26) e
continua escrevendo; quem pôs o carrinho à venda foi você, pela tela da leva 04. **Três consequências,
e a primeira é sua:**

1. **O passo humano 3 da §0 fala em "os três produtos reserváveis". São quatro no banco que roda.**
   O carrinho simples também precisa de **1** em "Dias de intervalo", ou a disponibilidade dele
   nasce um dia mais generosa que a das scooters.
2. A página `/book` oferece produtos `IsActive && IsBookable`, então **o carrinho vai aparecer no
   seletor** do site que roda. É o comportamento certo da regra; só não é o que a prosa da §7 supõe.
3. O carrinho tem **zero unidades** (as 10 são 4 + 4 + 2). Com zero unidades a regra responde
   *esgotado* em qualquer data — honesto, mas é uma opção no seletor que nunca pode ser vendida.
   **Isso eu não resolvo sozinho**: some o carrinho do seletor enquanto não tiver unidade, deixe
   como está, ou compre as unidades. Fica para a sua decisão no P3, e não bloqueia o P2.

**Nada disso muda o host de teste**, onde o seed decide e continua dizendo `false` — os testes da
§9.3 que afirmam que `/rentals/single-stroller` ainda imprime a frase de "em breve" seguem válidos.

---

## 3. A migration, classificada

Gerado o SQL antes de ler o C#, como a skill `revisao-migration-efcore` manda:

```
dotnet ef migrations script 20260910031613_AddBatteriesAndOperationalSettings \
  20260913215438_AddBookings --project src/OrlandoUp.Web --idempotent
```

```
38 statement(s) — 0 destrutivo(s) — 6 de atencao — 11 aditivo(s)
VEREDITO (script): puramente aditivo no essencial, com itens de atencao.
```

```
Migration: 20260913215438_AddBookings
Classificação: PURAMENTE ADITIVA
Operações: 4 tabelas novas, 1 coluna nova, 10 índices (1 único), 7 chaves estrangeiras,
           1 bloco Sql que derruba um constraint que a própria migration criou.
           ZERO DROP TABLE, ZERO DROP COLUMN, ZERO instrução de dado.
Recomendação: APLICAR.
```

### 3.1 Os seis itens de atenção, um a um

| # | Item | Justificativa |
|---|---|---|
| 1 | `DROP CONSTRAINT` em `OperationalSettings` | É a correção 1 da EMENDA-03-01, e **não** é parte de uma recriação: derruba o constraint que o `DEFAULT 18` da instrução anterior acabou de criar. O valor 18 permanece gravado na linha; o constraint não permanece. |
| 2–4 | Três FK `ON DELETE CASCADE` | `BookingEvents → Bookings`, `BookingLines → Bookings`, `BookingAddOns → BookingLines`. Intencionais: linha, extra e evento **nunca sobrevivem à reserva**, e não há o que guardar de um extra cuja linha sumiu. |
| 5 | `CREATE UNIQUE INDEX IX_Bookings_Number` | A tabela é **criada vazia na mesma migration**: não existe linha, logo não existe duplicata, e a armadilha de "conferir duplicatas antes do índice único" não tem como morder. O índice **não é filtrado**, então não depende de `QUOTED_IDENTIFIER`. |
| 6 | "INSERT (seed)" | **Falso positivo do classificador.** O único `INSERT` do script é a linha de escrituração em `__EFMigrationsHistory` (linha 220), que o EF escreve em toda migration. Confirmado: `HasData` não existe em `src/` — as 18 ocorrências que o `grep` acha são `HasDatabaseName`, substring. |

### 3.2 As armadilhas da skill, conferidas contra este script

- **`HasData` — a mais traiçoeira: não se aplica.** O projeto não usa `HasData` em lugar nenhum.
  A linha de configuração de leva 04b foi escrita por `InsertData` dentro do `Up`, que é instrução
  de dado e não estado declarado, e esta migration não escreve dado nenhum.
- **Enum persistido por posição:** os quatro enums novos nascem com **número explícito**, e o
  `BookingStatus` ainda deixa vão de propósito (1–8 enquanto viva, 20+ para os fins), para que um
  membro futuro entre no fim da sua própria faixa. Nenhum enum existente foi reordenado — o diff de
  `Enums.cs` é **59 linhas, todas acrescentadas no fim do arquivo**.
- **Índice único filtrado:** o único índice único não tem filtro. Conferido no SQL.
- **Auto-referência com `SetNull`:** não existe FK para a própria tabela. As sete FK são
  **3 cascata + 4 `NO ACTION`**, e não há caminho de cascata duplo para tabela nenhuma
  (`Bookings` alcança `BookingAddOns` só via `BookingLines`), então o erro 1785 não tem como nascer.
- **Data de calendário × instante:** `StartDate` e `EndDate` são `date` — dia de entrega e de
  retirada no calendário de Orlando, sem fuso. `CreatedAtUtc`, `CancelledAtUtc` e `OccurredAtUtc`
  são `datetime2` e vêm do `IClock`. D16 cumprida, e a categoria de cada coluna está conferida uma
  a uma.
- **Tipos:** todo dinheiro é `decimal(10,2)` — a skill diz `decimal(18,2)`, a **D15 deste projeto
  diz `(10,2)`**, e a decisão do projeto vence. A exceção é `Booking.TaxRate`, `decimal(5,4)`, que
  é alíquota e não valor, espelhando `DeliveryZone.SalesTaxRate`. Todo texto tem `HasMaxLength`;
  não há `nvarchar(max)`.
- **Renomear × recriar:** não há renomeação nesta migration.
- **Coluna nova obrigatória em tabela com dados:** é exatamente o caso de `NextDayCutoffHour` em
  `OperationalSettings`, que tem **1 linha** — e é por isso que o `DEFAULT 18` está ali. Sem ele o
  `ADD ... NOT NULL` falharia. Com ele a linha recebe 18 e o constraint é derrubado logo em seguida.
- **O script é transacional:** `BEGIN TRANSACTION` na linha 1, `COMMIT` na 224. Se algo falhar,
  nada fica pela metade.

### 3.3 O `sys.default_constraints` **antes**, para a leitura de depois ser inequívoca

```
tabela         coluna          constraint                     definicao
DeliveryZones  SalesTaxRate    DF__DeliveryZ__Sales__3F466844  ((0.0))
Products       TurnaroundDays  DF__Products__Turnar__4316F928  ((0))
```

**Dois, e os dois são legítimos** — conferi o modelo antes de chamar qualquer um de órfão:
`ProductConfiguration.cs:25` declara `HasDefaultValue(0)` e `DeliveryZoneConfiguration.cs:21`
declara `HasDefaultValue(0m)`. Banco e modelo concordam, e a C01 do `admin-catalog.tsv` proíbe
`HasDefaultValue(true|false)` — booleano —, que não é o caso de nenhum dos dois. **Não são defeito
e eu não os toquei.**

**A asserção que o P2 vai medir depois de você aplicar**, e que é mais forte que "zero linhas para
a coluna nova": o banco continua com **exatamente estes dois** constraints, e **nenhum deles está
em `OperationalSettings`**. Um terceiro é parada.

---

## 4. Arquivos desta etapa — todos dentro da §11.1

**Novos (10):**
`Domain/Booking.cs`, `Domain/BookingLine.cs`, `Domain/BookingAddOn.cs`, `Domain/BookingEvent.cs`,
`Domain/DeliveryWindows.cs`;
`Infrastructure/Data/Configurations/{Booking,BookingLine,BookingAddOn,BookingEvent}Configuration.cs`;
`Infrastructure/Data/Migrations/20260913215438_AddBookings.cs` e `.Designer.cs`.

**Modificados (9):** `Domain/Enums.cs` (+59), `Domain/OperationalSettings.cs` (+13),
`Infrastructure/Data/AppDbContext.cs` (+8), `.../Configurations/OperationalSettingsConfiguration.cs` (+6),
`.../Migrations/AppDbContextModelSnapshot.cs` (+358), `Infrastructure/Seeding/CatalogSeedData.cs`,
`Infrastructure/Seeding/CatalogSeeder.cs`, `tests/OrlandoUp.Tests/AdminCrudTests.cs`,
`Docs/controles/fleet-batteries.tsv` (**1 linha, só a célula Esperado**).

**O snapshot regenerado é 358 inserções e ZERO remoções** — a regeneração não apagou nada do que
as quatro levas anteriores tinham posto lá, que é o que se quer conferir num arquivo gerado.

**O BOM UTF-8 foi retirado dos três arquivos que a ferramenta escreveu** (`_AddBookings.cs`,
`_AddBookings.Designer.cs`, `AppDbContextModelSnapshot.cs`) antes de qualquer `git add` —
`CLAUDE.md:82-83`; o hook recusa `efbbbf`.

**Cinco arquivos que a §11.1 declara novos ainda NÃO existem, e é de propósito:**
`BookingStatusRules.cs`, `Availability.cs`, `Quote.cs`, `BookingRules.cs` e os quatro serviços de
`Infrastructure/Data/` são regra, e regra entra no P2 **com o teste que a prende**. Pela mesma
razão o `Booking.cs` desta etapa **não** traz ainda a fábrica nem o `Cancel`: os dois membros que
atribuem situação nascem ao lado da tabela de transições que consultam. O setter privado já está
em vigor desde agora, então nada fora daquele arquivo consegue passar na frente deles.

---

## 5. Portão medido ao fechar esta etapa

```
dotnet build   Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test    Passed! Failed: 0, Passed: 183, Skipped: 0, Total: 183
```

| Arquivo de controle | Resultado |
|---|---|
| `admin-catalog.tsv` | 12 controles, **0 fora do esperado** |
| `public-site.tsv` | 17 controles, **0 fora do esperado** |
| `fleet-batteries.tsv` | 7 controles, **0 fora do esperado** (depois da rebase do C06) |
| `foundation.tsv` | 18 controles, **0 fora do esperado** |

### 5.1 Os dois números que se moveram nesta etapa, e por quê

**`AdminCrudTests.No_battery_column_declares_a_store_default`: 15 → 16.** É o cardinal que a §9.5
manda mover, e ele foi **vermelho antes de eu tocar nele** — a suíte acusou a coluna nova sozinha,
que é exatamente o que esse teste existe para fazer. Mudei o 15 para 16 e deixei escrito no
comentário do teste o par que ele mantém separado: **o modelo não declara padrão, e isso é o que
este teste mede; se o banco guardou um é outra pergunta, lida do catálogo dele depois da
aplicação** (§3.3).

**`fleet-batteries.tsv` C06: 5 → 6**, remedido na linha existente, no mesmo commit, sem linha
paralela. A sexta é `Booking.IsOverbooked`. E aqui vai, verbatim, a frase do plano que a
EMENDA-03-01 mandou trazer para este relatório:

> O rótulo diz *"nenhuma bandeira booleana **de visibilidade** nova nasce no dominio"*; o operando é
> `public bool Is[A-Za-z]+`, que conta **toda** bandeira booleana do domínio, de visibilidade ou não.
> **`Booking.IsOverbooked` não é bandeira de visibilidade** — não esconde nada de ninguém, marca uma
> reserva lançada acima da frota (D4/03). Rebasear a linha de 5 para 6 grava, sob um rótulo que fala
> de visibilidade, uma bandeira que não é de visibilidade.

Quem ler `6` daqui a três levas precisa deste parágrafo para saber que a sexta não é do tipo que o
rótulo nomeia. Corrigir o rótulo é edição de `.tsv` de outra frente, e a §11.1 só autoriza o número.

**O C05 do `admin-catalog.tsv` continua em 2** e ainda não se mexeu: os dois handlers novos e os
dois `.Record(` que os equilibram chegam no P3, com as telas. Medido agora: `9 − 7 = 2`.

---

## 6. O que é seu, e nesta ordem

**1. Aplicar a migration** (PowerShell, na raiz do repositório):

```
dotnet ef database update --project src/OrlandoUp.Web
```

**2. Depois de aplicada, eu leio o `sys.default_constraints`** e registro o resultado na abertura do
P2, contra a linha de base da §3.3: dois constraints, nenhum em `OperationalSettings`. Se aparecer
um terceiro, é parada e o `Up` ganha o `DROP CONSTRAINT` explícito que faltou.

**3. Pôr "Dias de intervalo" = 1 nos produtos reserváveis, em `/admin/produtos`.** Atenção ao
achado (c) da §2.1: **são quatro e não três** — *Drive Scout 4*, *Drive Spitfire EX*,
*Cadeira de rodas Drive* **e o carrinho simples**, que você pôs à venda. Enquanto isso não for
feito, a disponibilidade do site que roda é um dia mais generosa do que deveria, e o item 0b do
roteiro mede isso.

**4. Aprovar esta etapa**, para eu seguir ao P2 (domínio, regras, carregadores e os testes deles —
nenhuma tela).

**O push é seu.** Nenhum commit desta sessão foi empurrado.

---

## Revisão (Claude Web, 2026-09-13)

**Conferido abrindo o código e o SQL, não o relato.** `grep -c EMENDA-03-01` neste relatório lê **5**.
`git diff --stat 70d6d68..fb2439c`: 21 arquivos, todos dentro da §11.1 da spec. `Booking.Status`
tem `private set` (correção 2) e a fábrica fica para o P2, junto do `Cancel` e dos testes que os
prendem — aceito, é o lugar certo. `HasDefaultValue` não aparece em nenhuma configuração nova;
`HasPrecision(10, 2)` aparece **13** vezes e `HasPrecision(5, 4)` **1** vez nas quatro configurações
de reserva, e o domínio de reserva tem **14** propriedades `decimal` — batem. Os quatro enums têm
número explícito. O `fleet-batteries.tsv` mudou em **1** linha, só a célula Esperado (5 → 6). O
cardinal `AdminCrudTests` 15 → 16 foi movido com o motivo escrito ao lado.

**Migration `AddBookings`, classificada pelo revisor a partir de `scratchpad/leva03/migration-AddBookings.sql`:**

```
Classificação: puramente aditiva, com dois itens de atenção justificados
Operações: 4 tabelas novas, 1 coluna nova, 11 índices (1 único), 8 chaves estrangeiras
Itens de atenção:
  1. DROP CONSTRAINT dinâmico em OperationalSettings — é a correção 1 da EMENDA-03-01; derruba
     apenas o constraint que o DEFAULT 18 da instrução anterior acabou de criar, na mesma
     transação; o 18 fica gravado na linha única. Nada pré-existente é tocado.
  2. CREATE UNIQUE INDEX em Bookings.Number — tabela recém-criada, vazia; duplicata é impossível.
Armadilhas conferidas: sem HasData; enums com número explícito e novos membros só no fim da banda;
  sem índice filtrado (QUOTED_IDENTIFIER irrelevante); sem SET NULL — cascata só Booking → Lines →
  AddOns e Booking → Events, e NO ACTION em Products, AddOns, DeliveryZones, DeliveryLocations,
  sem ciclo; datas de calendário em `date` (StartDate, EndDate) e instantes em `datetime2`
  (CreatedAtUtc, CancelledAtUtc, OccurredAtUtc); todo texto com tamanho; dinheiro decimal(10,2)
  (D15) e a taxa decimal(5,4); coluna nova NOT NULL em tabela com 1 linha coberta pelo DEFAULT.
Recomendação: APLICAR.
```

**Os três achados da §2.1 são a razão de o `[H]` existir, e o terceiro pede uma ação do operador:**

1. **`BSC-22` baixada (13 linhas, pool disponível 12)** — confirma a spec e vira o caso real do
   teste *"bateria baixada não conta"*. Nada a fazer.
2. **`LostChargerFee` = 32,00** — edição legítima pela tela; nada a fazer.
3. **`single-stroller` reservável no banco, com preço e ZERO unidades.** A regra responderia
   *esgotado* em toda data — honesto, mas é opção no seletor que nunca vende. **Recomendação ao
   operador: desmarcar "Reservável" no carrinho simples pela tela de produto até as unidades
   existirem** (a D32 define reservável como *"unidades e lista de preços existem"*; a validação
   da leva 04 confere a lista, não as unidades). Se preferir mantê-lo, o passo humano 3 vale para
   **quatro** produtos, como o relatório diz. Vai para o backlog: a tela de produto não avisa
   "reservável sem unidade" — leitura do tipo "quem lê o estado", frente própria.

**Veredito: EXECUTAR.** Sem correção antes do P2. Três lembretes para o P2, não correções:

1. a fábrica `Booking.CreateByStaff(...)` e o `Cancel(...)` nascem no P2 **antes** dos carregadores,
   e o C04 do `.tsv` novo (≥ 2 atribuições em `Domain/`) passa a `sim` nele — o P2 mede;
2. a leitura de `sys.default_constraints` depois do `database update` entra na abertura do
   relatório do P2 com o número, contra a linha de base da §3.3 (dois, nenhum em `OperationalSettings`);
3. `FakeClock` por delegação a `new SystemClock(() => _utcNow)` (EMENDA-03-01, correção 4).

**Você — ação, nesta ordem (PowerShell na raiz do repositório):**

1. aplicar a migration:
   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```
2. em `/admin/produtos`: "Dias de intervalo" = **1** em *Drive Scout 4*, *Drive Spitfire EX* e
   *Cadeira de rodas Drive*; e no *Carrinho simples*, desmarcar "Reservável" (recomendado) — ou pôr
   também **1** em "Dias de intervalo", se quiser mantê-lo à venda.

**Cole no Claude Code:** *"leia `Docs/relatorio-leva-03-etapa-1.md` e execute a seção Revisão — a P1 está aprovada; a migration foi aplicada; siga ao P2."*

