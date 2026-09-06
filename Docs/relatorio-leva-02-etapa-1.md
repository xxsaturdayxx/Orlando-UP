# Relatório — leva 02, etapa 1: a migration `AddIsBookableAndOptionalDimensions` (parada P1)

**Data:** 2026-09-06. **HEAD ao escrever:** `c886f97`. **Spec:** `Docs/spec-02-public-site.md` com as
notas **`EMENDA-02-01`** e **`EMENDA-02-02`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-05`, "LEVA 02 — PUBLIC SITE", ainda `aguardando`. **Plano liberado** por
Rod em 06/09, com K1 (b), K2 (b) e K3 (a) respondidas.

**A migration NÃO foi aplicada.** `dotnet ef database update` não rodou; o banco continua com a
única migration da leva 01. Nada foi empurrado.

**O que esta parada pede:** a revisão do script SQL, das duas edições que fiz à mão na migration
gerada, e das três descobertas da §5 — em especial a 5.1, que é o motivo de a coluna nova **não**
ter valor padrão no modelo, contrariando a letra da A9 e cumprindo o que ela quer.

---

## 1. O que está commitado neste commit

Este relatório **e o código da E1**. Diferente da leva 01, não deixei a árvore suja esperando: a
migration é reversível por arquivo (`ef migrations remove`) e nada dela tocou banco. O que entrou:

| Arquivo | O quê |
|---|---|
| `Domain/Product.cs` | `WidthIn`/`LengthIn` → `decimal?`; `IsBookable` sem inicializador; `FitsDisneyTransport` → `bool?` |
| `Infrastructure/Data/Configurations/ProductConfiguration.cs` | as duas colunas deixam de ser `IsRequired()`; `IsBookable` **sem** `HasDefaultValue` (§5.1) |
| `Application/Catalog/CatalogViews.cs` | `ProductCard` e `ProductDetail` acompanham os tipos e ganham `IsBookable` |
| `Infrastructure/Data/CatalogQueries.cs` | as duas projeções (linhas 53 e 120–124 da EMENDA-02-01 A12) |
| `Pages/Shared/_ProductCard.cshtml` | badge só em `== true` |
| `Pages/Rentals/Details.cshtml` | badge só em `== true`; largura e comprimento viram linhas condicionais; três títulos passam a exigir conteúdo (B3) |
| `tests/OrlandoUp.Tests/DomainTests.cs` | badge com três respostas; produto novo nasce não reservável (A9) |
| `Migrations/20260906162133_AddIsBookableAndOptionalDimensions.cs` + `.Designer.cs` | gerados e **editados à mão** nas §§5.1 e 5.2 |
| `Migrations/AppDbContextModelSnapshot.cs` | gerado |

`dotnet build` limpo, **0 avisos**. `dotnet test` **69 passando, 0 falhando** (eram 65; as quatro
novas são as da A9 e as três do badge sem medida). BOM removido dos três arquivos gerados antes de
qualquer `git add` — os três nasceram com `EF BB BF`, como na leva 01.

---

## 2. Contagens do banco, medidas ANTES de aplicar

`SELECT DB_NAME()` impresso antes de qualquer consulta, como manda a D12:

```
OrlandoUpDb
Products|7      Units|7      PricingTiers|16      DeliveryZones|4
__EFMigrationsHistory: 20260904233355_InitialCreate
```

As 7 linhas de `Products` são as do catálogo placeholder da leva 01, e são exatamente as que o
`UPDATE` da §4 alcança. Elas morrem na E2, depois desta aprovação.

---

## 3. O SQL gerado, na íntegra

`dotnet ef migrations script 20260904233355_InitialCreate --output
scratchpad/leva02/migration-AddIsBookableAndOptionalDimensions.sql` (não commitado; reproduzível
pelo comando). O script foi lido do arquivo, não do `.cs` — é o que a skill exige.

```sql
BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'WidthIn');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Products] ALTER COLUMN [WidthIn] decimal(5,1) NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'LengthIn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Products] ALTER COLUMN [LengthIn] decimal(5,1) NULL;

ALTER TABLE [Products] ADD [IsBookable] bit NOT NULL DEFAULT CAST(0 AS bit);

UPDATE [Products] SET [IsBookable] = 1;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260906162133_AddIsBookableAndOptionalDimensions', N'10.0.11');

COMMIT;
GO
```

Uma transação só: ou tudo entra, ou nada entra.

---

## 4. Classificação, item por item

`scripts/classificar.py` da skill `revisao-migration-efcore`:

```
15 statement(s) — 0 destrutivo(s) — 6 de atencao — 1 aditivo(s)
VEREDITO: puramente aditivo no essencial, com itens de atencao.
```

**Zero destrutivos.** Os seis de atenção, cada um com a justificativa e a medição que a sustenta:

| # | Operação | Por que está aqui | Se o banco não estiver como espero |
|---|---|---|---|
| 1 | `DROP CONSTRAINT` do default de `WidthIn` | **é operação vazia.** Boilerplate que o EF emite antes de todo `ALTER COLUMN`. Medido em `sys.default_constraints`: `WidthIn` **não tem default** (só `IsActive` e `TurnaroundDays` têm), então `@var` é `NULL` e o `EXEC` não roda | se um default existisse, ele cairia e não seria recriado — mas então a coluna teria um default que ninguém declarou, e isso já seria a divergência |
| 2 | `ALTER COLUMN WidthIn decimal(5,1) NULL` | o alargamento da §4 da spec. **Mesmo tipo, mesma precisão, mesma escala** — muda só a nulidade. `NOT NULL` → `NULL` não trunca nada e não perde linha | nenhuma linha pode falhar: toda linha existente tem valor, e valor continua valendo |
| 3 | `DROP CONSTRAINT` do default de `LengthIn` | idem 1; medido: **sem default** | idem 1 |
| 4 | `ALTER COLUMN LengthIn decimal(5,1) NULL` | idem 2 | idem 2 |
| 5 | `UPDATE [Products] SET [IsBookable] = 1` | **escrito por mim**, não gerado. Alcança as 7 linhas da §2, que são as 7 do catálogo placeholder, todas à venda antes de a coluna existir. Sem ele, o `DEFAULT CAST(0 AS bit)` tiraria os sete do ar em silêncio | o `UPDATE` não tem `WHERE`: se a tabela tivesse uma linha que **não** deve ser reservável, ela seria marcada como reservável. Hoje não tem — as 7 são o catálogo inteiro e todas estão ativas. Medido antes de aplicar, e a contagem depois vai no acréscimo da E2 |
| 6 | `INSERT INTO __EFMigrationsHistory` | é o registro da própria migration, **não** um seed. O classificador o rotula como "seed" por casar `INSERT`; a armadilha do `HasData` não se aplica | — |

### Armadilhas da Etapa 2 da skill, conferidas

| Armadilha | Aplica? | Como conferi |
|---|---|---|
| `HasData` como estado declarado | **não** | `HasData(` em `src/`: **0**. *(O `grep` por `HasData` devolve 9 — todas `HasDatabaseName` dos índices do Identity. É exatamente a armadilha de substring da regra 1 de `Docs/regras-de-controle.md`, e ela me pegou enquanto eu conferia armadilhas.)* |
| Enum persistido por posição | **não** | nenhum enum novo; os 7 existentes carregam valor explícito (22 atribuições em `Domain/Enums.cs`) |
| Índice único filtrado | **não** | `CREATE UNIQUE INDEX` no script: **0** |
| Auto-referência com `SET NULL` | **não** | nenhuma FK nova |
| Data de calendário × instante UTC | **não** | nenhuma coluna de data nova |
| Tipos (dinheiro, data, texto) | **não** | nenhum tipo novo; `decimal(5,1)` é medida em polegadas, não dinheiro |
| Renomear × recriar | **não** | `sp_rename` e `DROP COLUMN` no script: **0** |
| Coluna obrigatória em tabela com dados | **sim, e tratada** | `ADD ... NOT NULL` com `DEFAULT CAST(0 AS bit)` — o SQL Server exige o default para preencher as linhas existentes, e o `UPDATE` seguinte corrige o valor. É o caminho de três passos da skill dentro de uma transação só |

---

## 5. Três descobertas que mudam o que a emenda mandava — a primeira é a que importa

### 5.1 A A9 pede uma combinação que o EF não sabe expressar para `bool`, e a letra dela produziria o defeito que a D32 existe para impedir

A **A9** manda: `bit NOT NULL DEFAULT 1` no banco (para preencher as 7 linhas) **e** padrão C# `false`
(falha fechada). Escrevi exatamente isso primeiro — `HasDefaultValue(true)` no modelo — e parei ao
perceber o que aconteceria na E5.

**Com um valor padrão de banco numa propriedade `bool` não anulável, o provedor não consegue
distinguir "o chamador disse `false`" de "o chamador não disse nada".** O `false` é tratado como
"não informado", omitido do `INSERT`, e a linha entra com o padrão — `true`. Aplicado à E5, isso
significa: **os quatro carrinhos "em breve" seriam gravados como reserváveis**, com preço e botão
de reserva, que é precisamente o defeito que a D32 foi escrita para impedir. E nada falharia: o
`seed-catalog` diria que gravou, os testes de contagem passariam, e só a página mostraria.

O que fiz, e que entrega o que a A9 **quer** por outro mecanismo:

- **modelo sem valor padrão** — `builder.Property(p => p.IsBookable).IsRequired()`. Assim o EF
  sempre manda o valor explícito, e um `false` é um `false`;
- **preenchimento das linhas existentes por um `UPDATE` escrito à mão** na migration, visível no
  script da §3 e classificado no item 5 da §4 — onde um revisor lê, em vez de depender da semântica
  do provedor;
- **o `DEFAULT CAST(0 AS bit)` que o SQL Server cria fica**, e é bom que fique: uma linha inserida
  por fora do EF sem dizer nada nasce **não reservável**, que é a resposta de falha fechada.

Resultado idêntico ao que a A9 descreve — 7 linhas reserváveis, toda linha nova fecha por padrão —
com a diferença de que o `false` é gravável. **Preciso que isto seja aprovado explicitamente: é um
desvio da letra da A9.**

### 5.2 O `Down` gerado inventava uma medida, e a medida inventada publicaria uma afirmação falsa

O `Down` do EF devolve as duas colunas a `NOT NULL` com `defaultValue: 0m`. Um produto de **0 por 0
polegadas** está dentro do limite de 30 × 48, então `FitsDisneyTransport` responderia `true` e a
página publicaria **"cabe nos ônibus da Disney"** para uma máquina que ninguém mediu — uma afirmação
falsa sobre um objeto real, criada por uma migration de rollback. É a D15 outra vez, num campo que
não é preço.

Escrevi à mão, no começo do `Down`, uma recusa que dispara só quando há o que inventar:

```sql
IF EXISTS (SELECT 1 FROM [Products] WHERE [WidthIn] IS NULL OR [LengthIn] IS NULL)
    THROW 50000, 'Down would have to invent a width or a length for a product nobody measured. ...', 1;
```

Rollback continua possível enquanto não existir dimensão nula; quando existir, ele para e um humano
decide o que fazer com esses produtos. **O `defaultValue: 0m` das duas `AlterColumn` do `Down` foi
deixado como o EF gerou** — depois do `THROW` ele é inalcançável quando importaria, e mexer nele
faria o `Down` divergir do que o EF sabe reverter.

### 5.3 A mesma armadilha já existe em `IsActive`, desde a leva 01 — e eu não a consertei

`IsActive` tem `HasDefaultValue(true)` no modelo. Pela mesma razão da §5.1, **é impossível hoje
inserir um produto com `IsActive = false`**: o `false` seria omitido e a linha entraria visível. Não
afeta `UPDATE` (esconder um produto pela administração vai funcionar), e não afeta esta leva, que
não insere nada escondido — a D32 justamente tirou esse peso do `IsActive`. **Não mexi:** é código
da leva 01, fora da lista de arquivos desta etapa, e o conserto merece a sua decisão. Registro para
não ser redescoberto num susto na leva 04, quando a administração ganhar "criar produto".

---

## 6. As correções da EMENDA-02-02 aplicadas nesta etapa

| Item | O que fiz |
|---|---|
| **B3** | Aplicada mais larga, como a emenda manda. Em `Pages/Rentals/Details.cshtml`: largura e comprimento viraram linhas condicionais como as outras quatro; o `<h2>` de especificações e o `<dl>` só saem quando **ao menos uma** linha tem valor (`hasSpecs`); a tabela de preço e seu título só saem com `PricingRows.Count > 0`. Destaques e adicionais já eram guardados. Com a K2 (b), a cadeira renderiza a página inteira sem nenhuma seção vazia |
| **B4** | Conferida e obedecida: `triple-stroller` **não** foi tocado. `SiteBehaviourTests.cs:91` continua verde e agora pelo motivo certo — um carrinho sem dimensão responde `null`, que não é `true` |
| **B6** | Este relatório traz o script e **nada aplicado**. A seção de metadados de `INFORMATION_SCHEMA` será acrescentada aqui pela E2, depois da sua aprovação, **acima** da seção `## Revisão`, que continua sendo a última do arquivo |
| **B1** | Sem efeito nesta etapa (é sobre os controles de token, que são da E4) |
| **B2** | Sem efeito nesta etapa (idem) |
| **B5** | Sem efeito nesta etapa (é sobre o teste do sitemap, da E7) |

**Formas proibidas:** `?? 0` e `GetValueOrDefault(` continuam em **0** ocorrências em `src/`, com as
duas colunas já anuláveis — toda leitura nova vai por `is decimal width`. `DateTime.Now`,
`DateTime.Today`, `Migrate(` e `EnsureCreated(` seguem em 0 em `src/`, e nenhum comentário que
escrevi transcreve qualquer uma dessas formas.

---

## 7. Uma armadilha de ferramenta, para não custar duas vezes

`[InlineData(21, null)]` num `[Theory]` de parâmetros `double?` **não roda**: o literal `21` é
encaixotado como `Int32` e a reflexão recusa entregá-lo a um `Nullable<double>`
(`Object of type 'System.Int32' cannot be converted to type 'System.Nullable\`1[System.Double]'`).
Com `double` não anulável a conversão acontece, e é por isso que os casos antigos do mesmo arquivo
sempre funcionaram. Sufixo `d` resolve, e o comentário no teste diz que o sufixo é estrutural.

---

## 8. Veredito, no formato da skill

```
Migration:      20260906162133_AddIsBookableAndOptionalDimensions
Classificação:  aditiva com atenção — 0 destrutivos
Operações:      0 tabelas novas, 1 coluna nova, 2 colunas alargadas para anulável,
                0 índices, 1 UPDATE de preenchimento escrito à mão
Itens de atenção: 6, todos justificados na §4 — 2 são operação vazia medida em
                sys.default_constraints, 2 são alargamento sem perda, 1 é o
                preenchimento intencional das 7 linhas, 1 é o registro da migration
Armadilhas conferidas: HasData (não se aplica, 0 ocorrências reais), enum por posição,
                índice único filtrado, auto-referência SET NULL, calendário × UTC,
                tipos, renomear × recriar, coluna obrigatória em tabela com dados
Recomendação:   aplicar, após aprovação explícita do desvio da §5.1 e da edição
                manual do Down da §5.2
```

---

## 9. O que acontece depois da aprovação (E2, nada antes)

1. `SELECT DB_NAME()` → tem de ler `OrlandoUpDb`;
2. `dotnet ef database update`;
3. ler de `INFORMATION_SCHEMA` o `IS_NULLABLE` de `WidthIn` e `LengthIn` e a presença e o padrão de
   `IsBookable`, **e acrescentar a seção a este arquivo, acima da Revisão** (A10 + B6);
4. apagar o catálogo em ordem de FK, sem tocar em tabela do Identity;
5. `seed-catalog`;
6. contagens depois: 7 produtos, 10 unidades, 6 adicionais, 4 zonas, 10 locais.

---

## Revisão (Claude Web, )
