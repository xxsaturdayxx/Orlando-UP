# Relatório — leva 04, etapa 1: a migration `RemoveActiveFlagStoreDefaultsAndAddAuditEntries` (parada P1)

**Data:** 2026-09-08. **HEAD ao escrever:** `bc8b762`. **Spec:** `Docs/spec-04-admin-catalog.md`
com as notas **`EMENDA-04-01`** e **`EMENDA-04-02`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-07`, "LEVA 04 — CATALOG ADMINISTRATION", ainda `aguardando`.
**Plano liberado** pela `EMENDA-04-02` — *"Verdict: execute. P1 is open."*

**Prova de leitura exigida pela `EMENDA-04-02`:** ocorrências da cadeia `EMENDA-04-02` neste
arquivo, contadas com `grep -c "EMENDA-04-02" Docs/relatorio-leva-04-etapa-1.md`: **7**.

**A migration NÃO foi aplicada.** `dotnet ef database update` não rodou. O `OrlandoUpDb` continua
com as duas migrations da leva 01 e da leva 02, e é isso que a §3 mostra medido. Nada foi empurrado.

**O que esta parada pede:** a leitura do script SQL nas duas direções, a classificação pela skill
`revisao-migration-efcore`, a medição da A11 por inteiro — que é a que contradiz a prosa da D32 e
da D34 e vira decisão sua — e **um achado que o plano não carregava**, na §7: uma quinta coluna
booleana continua com padrão no banco depois desta migration, e ela não está na lista das quatro.

---

## 1. O que está commitado neste commit

Este relatório **e o código da P1**, como na leva 02: a migration é reversível por arquivo
(`ef migrations remove`) e nada dela tocou banco nenhum.

| Arquivo | O quê |
|---|---|
| `Domain/AuditEntry.cs` | **novo** — POCO sem navegação e sem chave estrangeira para o Identity |
| `Domain/Enums.cs` | `AuditAction` com os quatro números escritos (`Created = 1` … `Reactivated = 4`) |
| `Infrastructure/Data/Configurations/AuditEntryConfiguration.cs` | **novo** — tabela, tamanhos, índice descendente, **nenhum `HasDefaultValue`** |
| `Infrastructure/Data/AppDbContext.cs` | um `DbSet<AuditEntry>`, duas linhas |
| `Configurations/ProductConfiguration.cs` | sai `.HasDefaultValue(true)` de `IsActive`; **nada mais**, o comentário das linhas 28–34 fica |
| `Configurations/AddOnConfiguration.cs` | idem |
| `Configurations/DeliveryZoneConfiguration.cs` | idem |
| `Configurations/DeliveryLocationConfiguration.cs` | idem |
| `Migrations/20260908215113_RemoveActiveFlagStoreDefaultsAndAddAuditEntries.cs` + `.Designer.cs` | **gerados, não editados à mão** |
| `Migrations/AppDbContextModelSnapshot.cs` | gerado |

Nenhum arquivo fora da §11.1 da spec foi tocado. **`Program.cs` aparece em zero linhas do
`git diff --stat`** — as duas linhas de registro de serviço que a A2 autoriza ali pertencem à etapa
das telas, não a esta.

`dotnet build OrlandoUp.sln --nologo -v q` **limpo, 0 avisos**. `dotnet test` **138 passando, 0
falhando** — o mesmo número de antes, que é o esperado: nenhum teste desta leva existe ainda, e o
par forma+comportamento da A1 é da etapa 3.

**BOM removido dos três arquivos gerados antes de qualquer `git add`.** Os três nasceram com
`EF BB BF`; medido depois da remoção, os três primeiros bytes passaram a ser `75 73 69` (`usi`),
`2F 2F 20` (`// `) e `2F 2F 20`. O `.githooks/pre-commit` recusaria os três com a marca.

---

## 2. Passo 0, remedido em `bc8b762` — nada copiado do plano

O plano mediu em `ec01a99`. A `EMENDA-04-02` entrou depois dele, em `bc8b762`, então remedi tudo.

### 2.1 Árvore

| O que a §0 da spec afirma | Medido em 08/09, em `bc8b762` | Veredicto |
|---|---|---|
| HEAD descendente de `cacd725` | sim | confere |
| `git status --porcelain` vazio | vazio | confere |
| `git ls-files` conta 157 | **158** | A12 (i), já corrigido pela emenda |
| 109 arquivos sob `src/` | **109** | confere |
| `origin/main...main` lê `0 0` | **`0 3`** | A12 (i) — agora três, com `bc8b762`. **São seus para empurrar** |
| `foundation.tsv` 18, `public-site.tsv` 17 | 18 e 17 | confere |

Nada modificado e nada não rastreado fora de `scratchpad/`.

### 2.2 Os 35 controles existentes, antes de eu tocar em qualquer arquivo

```
bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv
18 controles, 0 fora do esperado, HEAD bc8b762, árvore limpa.

bash Docs/medir-controles.sh verificar Docs/controles/public-site.tsv
17 controles, 0 fora do esperado, HEAD bc8b762, árvore limpa.
```

**35 de 35 no alvo, inclusive o C14 e o C15**, que o revisor não conseguiu medir por não ter
`dotnet` no shell da ponte e que a linha da fila mandou eu medir:

| Controle | Comando | Medido |
|---|---|---|
| C14 `foundation` | `dotnet build OrlandoUp.sln --nologo -v q` | **0** — build limpo, **0 avisos** |
| C15 `foundation` | `dotnet test OrlandoUp.sln --nologo -v q` | **0** — **138 passando**, 0 falhando |

Depois de escrita a migration, com a árvore suja, os 35 continuam no alvo — §8.1.

### 2.3 Ferramentas, banco e segredos

```
dotnet --list-sdks   8.0.424 · 9.0.102 · 10.0.301 · 10.0.400
dotnet ef --version  Entity Framework Core .NET Command-line Tools 10.0.11
sqllocaldb info      MSSQLLocalDB
```

`SELECT DB_NAME()` pela conexão configurada, antes de qualquer consulta de dado (D12) — a string
saiu do user-secret por leitura programática e **não foi impressa em lugar nenhum**:

```
OrlandoUpDb
```

**Segredos:** os três user-secrets da leva 01 estão postos —
`ConnectionStrings:DefaultConnection`, `AdminSeed:Email`, `AdminSeed:Password`. **Nenhum novo, e
nenhum valor lido para este relatório.**

### 2.4 Identificadores novos, esperado zero

Varridos sobre `src` e `tests` com `-I` e sem `bin`/`obj`, que é a armadilha que a
`EMENDA-04-02` B2 nomeia:

| Identificador | Ocorrências |
|---|---:|
| `AuditEntry` | 0 |
| `AuditAction` | 0 |
| `AuditTrail` | 0 |
| `CatalogWriter` | 0 |
| `TestAuthHandler` | 0 |
| `FormPoster` | 0 |
| `AdminCrudTests` | 0 |
| `.Record(` | 0 |

Os dois primeiros deixam de ser zero **neste commit**, que é o que a P1 escreve. Os outros seis
continuam em zero — §9.

### 2.5 `quem-ancora` e `proibidos`

`quem-ancora` sobre os oito arquivos que esta etapa cria ou altera: **120 ancoragens, 15 controles
por arquivo, nenhum controle órfão a deslocar** — só os dois `.tsv` existentes aparecem.

`proibidos` rodado **antes do primeiro comentário**. O que morde nos arquivos desta etapa, e não
foi escrito em nenhum deles nem em comentário: leitura de relógio local (C05 `foundation`), as
quatro formas de criação de schema na subida (C09), as duas formas de coalescer ausência em zero
(C17), os onze identificadores de catálogo (C16 `public-site`), o hex, os tokens e a família
aposentados (C03, C05 e C07 de `public-site`).

---

## 3. Contagens do banco, medidas ANTES de escrever a migration

Uma consulta só, precedida do `SELECT DB_NAME()` da §2.3.

| Tabela | Linhas |
|---|---:|
| **`Products`** | **7** |
| **`AddOns`** | **6** |
| **`DeliveryZones`** | **4** |
| **`DeliveryLocations`** | **10** |
| `Units` | 10 |
| `PricingTiers` | 8 |
| `ProductAddOns` | 12 |
| `ProductTranslations` | 14 |
| `__EFMigrationsHistory` | 2 |

As quatro tabelas que a migration toca somam **27 linhas — nenhuma vazia**. As duas migrations
gravadas são `20260904233355_InitialCreate` e `20260906162133_AddIsBookableAndOptionalDimensions`;
a desta leva **não** está lá.

**Distribuição da coluna que a migration mexe**, porque a §4.1 da spec afirma que nenhuma linha
precisa ser preenchida e isso é verificável:

| Tabela | `IsActive = 1` | `IsActive = 0` | Nulas |
|---|---:|---:|---:|
| `Products` | 7 | 0 | 0 (coluna `NOT NULL`) |
| `AddOns` | 6 | 0 | 0 |
| `DeliveryZones` | 4 | 0 | 0 |
| `DeliveryLocations` | 10 | 0 | 0 |

Toda linha existente já carrega valor. **A afirmação da §4.1 se sustenta, e a §5 a prova lendo o
script gerado.**

---

## 4. A migration, lida no script SQL e não no C#

Gerada com `dotnet ef migrations add RemoveActiveFlagStoreDefaultsAndAddAuditEntries --project
src/OrlandoUp.Web`, **sem uma edição à mão** — ao contrário da leva 02, que precisou de duas.

O script do `Up` saiu com `dotnet ef migrations script <anterior> <esta> --idempotent`; o do `Down`,
com os dois argumentos trocados. Os dois estão em `scratchpad/leva04/`, não commitados.

### 4.1 O `Up`, em prosa

Quatro vezes o mesmo bloco, uma vez por tabela, e **ele não é um `ALTER COLUMN`**:

```sql
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]
                              AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'IsActive');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var + ';');
```

Tipo e nulabilidade não mudam, então o gerador **não** emite `ALTER COLUMN`: ele só derruba a
restrição de padrão, pelo nome que ela tiver naquele banco. Nenhuma linha é reescrita. Depois disso:

```sql
CREATE TABLE [AuditEntries] (
    [Id] int NOT NULL IDENTITY,
    [OccurredAtUtc] datetime2 NOT NULL,
    [ActorEmail] nvarchar(256) NOT NULL,
    [EntityType] nvarchar(40) NOT NULL,
    [EntityId] int NOT NULL,
    [Action] int NOT NULL,
    [Summary] nvarchar(400) NOT NULL,
    CONSTRAINT [PK_AuditEntries] PRIMARY KEY ([Id])
);
CREATE INDEX [IX_AuditEntries_OccurredAtUtc] ON [AuditEntries] ([OccurredAtUtc] DESC);
```

**Nenhuma sentença de dado.** O único `INSERT` do script inteiro é a linha do
`__EFMigrationsHistory`. A §4.1 da spec manda parar se as quatro tabelas tiverem linha e o `Up`
trouxer sentença de dado: elas têm 27 linhas e **o `Up` não traz nenhuma**, então não é parada.

### 4.2 O `Down`, e ele é honesto

Derruba `AuditEntries` e repõe os quatro padrões com `ALTER TABLE [x] ADD DEFAULT CAST(1 AS bit)
FOR [IsActive]`. **Repor um padrão não inventa valor para linha que já tem um** — é a assimetria da
leva 02 ao contrário, e lá o `Down` teve de ser escrito à mão justamente porque *removia* uma coluna
medida. Aqui não há o que inventar.

O `Down` **apaga a tabela de auditoria e tudo que ela tiver**. É o que voltar atrás significa, e é
por isso que ele aparece como destrutivo na §5.2.

---

## 5. Classificação pela skill `revisao-migration-efcore`

`python scripts/classificar.py` sobre os dois scripts gerados, não sobre o `.cs`.

### 5.1 `Up` — 28 sentenças, **0 destrutivas**, 5 de atenção, 2 aditivas

| Item de atenção | Justificativa |
|---|---|
| `DROP CONSTRAINT` em `Products.IsActive` | **é o objeto da migration**, não parte de uma recriação. A D34 pede que a coluna booleana seja `IsRequired()` e nada mais. A coluna continua `bit NOT NULL` e as 7 linhas continuam com o valor que têm |
| `DROP CONSTRAINT` em `DeliveryZones.IsActive` | idem, 4 linhas |
| `DROP CONSTRAINT` em `DeliveryLocations.IsActive` | idem, 10 linhas |
| `DROP CONSTRAINT` em `AddOns.IsActive` | idem, 6 linhas |
| `INSERT (seed)` | **falso positivo do classificador**: o `INSERT` é a linha do `__EFMigrationsHistory` que toda migration grava. **Não há `HasData` em lugar nenhum deste repositório** — medido, `grep -rn "HasData" src` devolve zero |

O classificador fecha com *"puramente aditivo no essencial, com itens de atenção"*.

### 5.2 `Down` — 24 sentenças, **2 destrutivas**, 4 de atenção, 4 aditivas

| Item destrutivo | O que exatamente se perde |
|---|---|
| `DROP TABLE [AuditEntries]` | a tabela e **toda** a auditoria acumulada até ali. Hoje: zero linhas, porque a tabela ainda não existe. É intencional — voltar atrás de "passou a haver auditoria" é "deixou de haver" |
| `DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260908215113_…'` | uma linha, a desta migration. O `WHERE` foi conferido: nomeia a migration inteira, com carimbo de data e nome |

Os quatro `DROP CONSTRAINT` do `Down` **são** parte de uma recriação — cada um é seguido do
`ADD DEFAULT` correspondente, uma linha abaixo. É exatamente o caso que o rótulo do classificador
descreve, e por isso ficam em atenção e não em destrutivo.

**Ninguém aplica o `Down`.** Ele está aqui porque uma migration que não sabe voltar é uma migration
que ninguém leu.

### 5.3 Armadilhas conhecidas da skill, uma a uma

| Armadilha | Aplica-se? | Medição |
|---|---|---|
| `HasData` é estado declarado, não carga inicial | **não** | `grep -rn "HasData" src` = **0**. O catálogo entra pelo `CatalogSeeder`, em tempo de execução, por comando explícito |
| Enum persistido por posição | **sim, e está tratada** | `AuditAction` nasce com os quatro números escritos (`Created = 1`, `Updated = 2`, `Deactivated = 3`, `Reactivated = 4`), como todo enum de `Domain/Enums.cs`. A coluna é `int` |
| Índice único filtrado | **não** | o índice novo **não** é único e **não** tem filtro: `CREATE INDEX … ([OccurredAtUtc] DESC)`. Não há `CREATE UNIQUE INDEX` no script |
| Auto-referência com `SET NULL` | **não** | `AuditEntries` não tem chave estrangeira nenhuma — nem para si, nem para o Identity, e por decisão: a linha tem de sobreviver à conta que a escreveu |
| Data de calendário × instante | **sim, e está tratada** | `OccurredAtUtc` é **instante**, `datetime2`, nome terminado em `Utc` (D16). Será lido do `IClock` quando a etapa 3 escrever o serviço; nenhum padrão de banco entrega "agora" |
| Tipos | **conferido** | `datetime2` para o instante; `nvarchar` com tamanho nas três colunas de texto (256, 40, 400), **nenhum `nvarchar(max)`**; não há coluna monetária nesta tabela |
| Renomear × recriar | **não** | não há `sp_rename` nem par `DROP COLUMN` + `ADD COLUMN` no script. Nenhuma coluna é removida |
| Coluna nova obrigatória em tabela com dados | **não** | nenhuma coluna é acrescentada a tabela existente. As sete colunas obrigatórias nascem numa tabela nova e vazia |

### 5.4 Veredito

```
Migration: 20260908215113_RemoveActiveFlagStoreDefaultsAndAddAuditEntries
Classificação: aditiva com atenção — 0 destrutivos no Up
Operações: 1 tabela nova, 0 colunas novas, 1 índice novo,
           4 restrições de padrão removidas, 0 sentenças de dado
Itens de atenção: os quatro DROP CONSTRAINT são o objeto da migration (D34);
                  o INSERT é a linha do histórico de migrations, não seed
Armadilhas conferidas: as oito da etapa 2 da skill, na tabela da §5.3
Recomendação: APLICAR. A operação é neutra sobre as 27 linhas existentes,
              e a §6 mostra medido que ela também é neutra sobre o comportamento.
```

---

## 6. A medição da A11, por inteiro

A `EMENDA-04-01` A11 manda este relatório carregar a medição inteira, porque ela **contradiz a
prosa da D32, da D34 e do comentário de `ProductConfiguration.cs:28-34`**, e a correção dessa prosa
é sua, datada, e vira decisão nova. Eu **não** editei nenhum dos três: os dois primeiros estão na
lista negativa e no terceiro a §11.1 me autoriza a tirar o padrão de banco e nada mais.

**Rodei de novo, não copiei do plano.** Projeto descartável fora do repositório, **EF Core
10.0.11**, provedor **`Microsoft.EntityFrameworkCore.Sqlite` 10.0.11**, banco em memória. Três
colunas booleanas, três formas de declarar (ou não declarar) o padrão, todas escritas com um `false`
**explícito** e lidas de volta em contexto novo:

```
EF Core runtime: 10.0.11.0

model  ByValue    sentinel=True   defaultValue=True   defaultValueSql=(none)  valueGenerated=OnAdd
model  BySql      sentinel=False  defaultValue=False  defaultValueSql=1       valueGenerated=OnAdd
model  NoDefault  sentinel=False  defaultValue=False  defaultValueSql=(none)  valueGenerated=Never

read back  ByValue=False  BySql=True  NoDefault=False
```

**O mecanismo, e ele não é o que a prosa diz.** O EF decide se manda uma coluna comparando o valor
da propriedade com o **sentinela** dela. Declarar o padrão **por valor** (`HasDefaultValue(true)`)
faz o EF mover o sentinela para `true`: aí um `false` explícito **difere** do sentinela, é mandado,
e volta `false`. Declarar **por SQL** (`HasDefaultValueSql`) deixa o sentinela no padrão da
linguagem, que é `false`: aí um `false` explícito **coincide** com o sentinela, o EF omite a coluna,
e o padrão do banco escreve `true` por cima. **É a forma por SQL que engole o `false`, não a forma
por valor** — e este repositório não declara nenhuma por SQL.

Dito de outro jeito: a forma por valor não perde informação, ela só troca **qual** valor fica
indizível — e o valor que fica indizível é exatamente o que o padrão escreveria de qualquer modo.

**O achado é sobre o pipeline de atualização do EF, não sobre o provedor.** O SQLite entra aqui só
como lugar barato de rodar: a decisão de incluir ou omitir a coluna é tomada dentro do EF, antes de
qualquer provedor ver a sentença; o provedor recebe a lista de colunas já escolhida.

**Isso derruba uma frase da spec, e a `EMENDA-04-01` já a retirou** (A1): o teste de ida-e-volta que
a §8.2 item 1 pedia passaria **antes e depois** da migration — verde falso por construção, que a
`Docs/regras-de-controle.md` proíbe. O par que a substitui (asserção sobre o modelo + comportamento)
é da etapa 3.

**E não derruba a migration.** O que a justifica é a D34 como higiene de schema, decisão sua: o
schema passa a dizer o que o modelo quer dizer, e a regra *coluna booleana é `IsRequired()` e nada
mais* fecha a porta para a forma por SQL, que é a que morde de verdade. **A neutralidade
comportamental está provada acima, não afirmada.**

**O que continua sendo seu:** a prosa da D32, da D34 e do comentário de `ProductConfiguration.cs`
descreve o mecanismo ao contrário. Uma linha numerada nova em `Docs/decisions.md` corrige a decisão;
o comentário eu posso corrigir na etapa 3 se você abrir a §11.1 para isso — hoje ela não abre.

---

## 7. Um achado que o plano não carregava: uma **quinta** coluna booleana continua com padrão no banco

Medido em `bc8b762`, antes de escrever a migration, direto no `sys.default_constraints` do
`OrlandoUpDb`:

| Tabela | Coluna | Restrição | Definição |
|---|---|---|---|
| `Products` | `IsActive` | `DF__Products__IsActi__440B1D61` | `(CONVERT([bit],(1)))` |
| `AddOns` | `IsActive` | `DF__AddOns__IsActive__38996AB5` | `(CONVERT([bit],(1)))` |
| `DeliveryZones` | `IsActive` | `DF__DeliveryZ__IsAct__403A8C7D` | `(CONVERT([bit],(1)))` |
| `DeliveryLocations` | `IsActive` | `DF__DeliveryL__IsAct__59063A47` | `(CONVERT([bit],(1)))` |
| **`Products`** | **`IsBookable`** | **`DF__Products__IsBook__6E01572D`** | **`(CONVERT([bit],(0)))`** |
| `Products` | `TurnaroundDays` | `DF__Products__Turnar__4316F928` | `((0))` — não é booleana |
| `DeliveryZones` | `SalesTaxRate` | `DF__DeliveryZ__Sales__3F466844` | `((0.0))` — não é booleana |

**`Products.IsBookable` tem padrão no banco, e o modelo não sabe disso.** A origem está medida: a
migration da leva 02 acrescentou a coluna com `AddColumn<bool>(… defaultValue: false)`, porque uma
coluna `NOT NULL` nova numa tabela com sete linhas precisa dizer o que as linhas antigas significam.
O SQL Server materializa esse `defaultValue` como restrição **permanente**; o EF compara modelo com
modelo, o snapshot nunca carregou essa restrição, e por isso **nenhuma migration posterior a remove
— inclusive esta**. O `Up` desta leva não menciona `IsBookable`.

**Consequência para a D34, na letra:** depois de aplicada esta migration, a afirmação *"nenhuma
coluna booleana carrega padrão de banco"* continua **falsa no banco**, por uma coluna. Ela fica
verdadeira **no modelo**, que é o que o controle C01 mede — e o controle está certo em medir o
modelo, porque é o modelo que decide o que o EF manda.

**Consequência prática: nenhuma, e isto também é medido, não suposto.** Segunda parte do experimento
da §6, com uma coluna cuja tabela tem padrão e cujo modelo não declara nenhum — a forma exata do
`IsBookable`:

```
model  sentinel=False  valueGenerated=Never

INSERT INTO "Rows" ("ModelSilentTableDefaulted")   [Parameters=[@p0='False']]
read back = False
```

Com `valueGenerated=Never` o EF **sempre** nomeia a coluna no `INSERT`. O padrão do banco nunca é
consultado por escrita que venha do EF; ele só valeria para um `INSERT` cru que omitisse a coluna, e
não existe nenhum neste repositório. Depois desta migration, as quatro colunas `IsActive` passam a
estar exatamente nessa condição — que é, aliás, o que torna a migration comportamentalmente neutra.

**Eu não a removi, e o motivo é de escopo, não de dificuldade.** A §4.1 da spec nomeia **quatro**
colunas; `IsBookable` não é uma delas. Removê-la exigiria uma sentença escrita à mão na migration —
o nome da restrição é gerado pelo servidor e difere entre bancos, então seria o mesmo
`DECLARE`/`EXEC` dinâmico que o gerador emite —, o que é acrescentar operação a uma migration que a
spec descreve fechada. **É decisão sua**, e ela tem três saídas legítimas:

- **(a)** deixar como está e registrar em `Docs/backlog-conhecido.md` que o banco carrega uma
  restrição inerte que o modelo não conhece;
- **(b)** eu acrescento o quinto bloco a **esta** migration, com sua autorização explícita — ainda
  na P1, antes de você aplicar, e este relatório ganha uma seção de revisão;
- **(c)** vira migration própria, de higiene, numa leva futura.

Não tenho preferência forte. **(b)** é a mais barata enquanto a migration ainda não foi aplicada, e
é a única que faz a D34 ficar verdadeira no banco no mesmo dia em que fica verdadeira no modelo.

---

## 8. Controles

### 8.1 Os 35 existentes, remedidos com a árvore suja

```
bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv
18 controles, 0 fora do esperado, HEAD bc8b762, árvore COM ALTERAÇÕES NÃO COMMITADAS.

bash Docs/medir-controles.sh verificar Docs/controles/public-site.tsv
17 controles, 0 fora do esperado, HEAD bc8b762, árvore COM ALTERAÇÕES NÃO COMMITADAS.
```

**Nenhum controle dos dois `.tsv` existentes se deslocou.** É o que a §11.1 da spec exige, e é a
razão de eles estarem na lista negativa.

### 8.2 Os quatro controles novos que esta etapa move

O `Docs/controles/admin-catalog.tsv` só nasce no commit de conteúdo; os comandos abaixo são os que a
`EMENDA-04-02` aprovou, rodados **a partir do arquivo de alvos gravado** em `scratchpad/leva04/`:

| Controle | No plano, em `ec01a99` | Agora, com a P1 escrita | Alvo no fim |
|---|---:|---:|---:|
| C01 D34 nenhuma coluna booleana declara padrão de banco, **nas duas direções** | 4 | **0** | 0 |
| C02 ALCANCE de C01 a varredura chega a pelo menos oito arquivos de configuração | sim | **sim** (11 arquivos) | sim |
| C03 D34 RELAÇÃO toda bandeira de visibilidade mantém o inicializador do C# | 0 | **0** | 0 |
| C04 ALCANCE de C03 o operando maior é maior ou igual a quatro | sim | **sim** | sim |

O C01 é o controle que esta parada fecha: **4 → 0**. O C03 é o que prova que fechá-lo não custou os
inicializadores `= true` do C# — os quatro continuam onde estavam, e a A3 depende disso: o produto
criado pela tela nasce oculto porque o *handler* escreve `false`, não porque o domínio mudou.

Os outros oito controles do `.tsv` novo pertencem às etapas 2 e 3 e não foram tocados aqui.

### 8.3 O snapshot, que é onde a A1 vai medir

Antes e depois, para cada uma das quatro colunas — é a forma que o par da A1 vai afirmar sobre o
modelo na etapa 3:

```diff
                     b.Property<bool>("IsActive")
-                        .ValueGeneratedOnAdd()
-                        .HasColumnType("bit")
-                        .HasDefaultValue(true);
+                        .HasColumnType("bit");
```

Depois da migration, `IsActive` no snapshot é **letra por letra** o que `IsBookable` já era: sem
`ValueGeneratedOnAdd`, sem padrão. O sentinela volta ao neutro, que é a metade "forma" do par da A1
— e, pela §6, é uma mudança de forma sem mudança de comportamento, o que é o que a A1 previu.

---

## 9. O que a P1 não fez, e o que eu preciso de você

**Não fiz, de propósito:**

- **não apliquei a migration.** `dotnet ef database update` não rodou. O `OrlandoUpDb` continua com
  duas migrations gravadas;
- **não empurrei nada.** `origin/main...main` lê `0 3` e os três commits são seus;
- **não escrevi serviço, tela, recurso, CSS nem teste.** `CatalogWriter`, `AuditTrail`,
  `TestAuthHandler`, `FormPoster`, `AdminCrudTests` e `.Record(` continuam medindo **0** em `src` e
  `tests`;
- **não toquei `Program.cs`**, nem `Docs/decisions.md`, nem o comentário de
  `ProductConfiguration.cs:28-34`.

**Preciso de você, nesta ordem:**

1. **A decisão da §7** — (a), (b) ou (c) para o padrão de banco do `Products.IsBookable`. Se for
   **(b)**, eu acrescento o bloco antes de você aplicar;
2. **aplicar a migration**, na sua máquina:

   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```

3. **a liberação da P2**, que constrói o arreio de teste antes de a primeira tela de CRUD existir.

Depois de aplicar, vale conferir três coisas — a skill pede e elas são baratas: que o
`__EFMigrationsHistory` passou a ter **3** linhas, que `AuditEntries` existe e aceita uma linha, e
que o `sys.default_constraints` de `Products` não lista mais `IsActive`.
