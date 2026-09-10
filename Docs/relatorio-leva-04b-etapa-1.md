# Relatório — leva 04b, etapa 1: a migration `AddBatteriesAndOperationalSettings` (parada P1)

**Data:** 2026-09-10. **HEAD ao escrever:** `13a574c`. **Spec:** `Docs/spec-04b-fleet-batteries.md`
com as notas **`EMENDA-04B-01`** e **`EMENDA-04B-02`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-09`, "LEVA 04B — FLEET BATTERIES", ainda `aguardando`.

**Prova de leitura exigida pela `EMENDA-04B-02`:** ocorrências da cadeia `EMENDA-04B-02` neste
arquivo, contadas com `grep -c "EMENDA-04B-02" Docs/relatorio-leva-04b-etapa-1.md`: **4**.

**A migration NÃO foi aplicada.** `dotnet ef database update` não rodou; o `OrlandoUpDb` continua
com as três migrations de antes, e é isso que a §3 mostra medido. Nada foi empurrado.

**As sete correções da `EMENDA-04B-02` estão aplicadas**, cada uma com o número que a confirma na
§1. Duas invertiam um resultado — a B1 e a B3 — e as duas foram remedidas por mim, não copiadas.

---

## 1. As sete correções, uma a uma

### B1 — a linha de configuração: a resposta é **(d)**, e as minhas três opções estavam erradas

Eu recomendei **(a)**, o comando de seed garantindo a linha. **Remedi e a emenda está certa: (a) não
alcança o problema.** O `SiteFactory` cria o schema em **`:118`** e **`:133`** — não em `:83`/`:98`,
como o meu plano dizia, e a correção é minha — e o `SeedAsync` chama **`CatalogSeeder.RunAsync`**
na linha **123**, nunca o `SeedCommands`. Uma guarda dentro do comando de bateria **jamais roda no
host de teste**:

```
tests/OrlandoUp.Tests/SiteFactory.cs
118:        await db.Database.EnsureCreatedAsync();
123:        await CatalogSeeder.RunAsync(db, clock, logger, CancellationToken.None);
133:        await db.Database.EnsureCreatedAsync();
```

**E é guarda para um estado que não pode ocorrer:** o C09 do `foundation.tsv` afirma que a aplicação
nunca constrói schema a partir do modelo, então todo banco implantado recebeu a linha da migration.

**O que eu faço, então:** o `AdminCrudTests.cs` arranja a linha no teste que precisa dela — arquivo
já nomeado na §11.1, nada se abre. E o **teste 7 da §8 se parte em dois**, porque um instrumento não
prova as duas metades:

| Metade | Instrumento |
|---|---|
| a tela edita e **nunca cria nem apaga** | a suíte, mais o **C03** do `.tsv` novo, que é proibição estática sobre `Pages/Admin/` |
| **a migration cria a linha** | **este relatório**, §4.2, lendo o `InsertData` do arquivo escrito — e de novo o **item 8 do roteiro**, que lê os três valores no banco real |

A suíte nunca vê uma migration e não será escrita como se visse. As duas metades são das etapas 2 e
3; aqui fica a primeira prova da segunda.

### B2 — `Program.cs:119`, `two` → `three`

Feito. É a única linha que a §11.1 concede e ela foi gasta em verdade, não em fiação. **O registro
do comando continua custando zero linhas:** o despacho é todo dentro do `SeedCommands.cs`.

```
git diff --numstat src/OrlandoUp.Web/Program.cs
1       1       src/OrlandoUp.Web/Program.cs
```

### B3 — o C07 estava verde medindo a coisa errada, e esta é a correção que inverte um resultado

Remedido por mim, fora do repositório, contra o alvo sintético que a emenda descreve — um `Domain/`
com `public bool IsVisible` e `public bool IsRetiredFromFleet`:

```
forma ANTIGA (public bool IsActive)     contra o alvo sintético:  0     <- passa no que devia recusar
forma NOVA   (public bool Is[A-Za-z]+)  contra o alvo sintético:  2     <- recusa
forma NOVA   no repositório, hoje:                                5
   AddOn.cs:15             public bool IsActive
   DeliveryLocation.cs:19  public bool IsActive
   DeliveryZone.cs:20      public bool IsActive
   Product.cs:35           public bool IsActive
   Product.cs:43           public bool IsBookable
```

A quinta bandeira já estava na árvore e o comando antigo não a via. O controle passa a ser **C06**
depois da B4, com o rótulo que ele consegue carregar — *nenhuma bandeira booleana de visibilidade
nova nasce no domínio* — e esperado **5** no fim, porque a `Battery` carrega `Status` e não bandeira.

### B4 — o C05 proposto sai; ele era subconjunto estrito do C16 de `public-site`

Conferido lendo os dois comandos: o C16 varre **`src/` inteiro**, exclui o mesmo arquivo pelo mesmo
nome e cobre **onze** identificadores; o meu varria uma pasta e cobria dois. **O `.tsv` desce com
sete controles**, e o cabeçalho dele nomeia o C16 entre os permanentes que esta leva remede em vez de
duplicar. **O antigo C06 sobrevive como irmão de alcance do C16** e o rótulo diz isso.

### B5 — a §11.2 item 3 estava errada e o meu §6.4 estava certo

Medido em `13a574c`, antes de eu escrever qualquer tela:

```
OnPost[A-Za-z]*Async  sob Pages/Admin  ->  6
[.]Record[(]          sob Pages/Admin  ->  4
C05 de admin-catalog = 6 - 4           ->  2
```

A §5 lista quatro rotas e só **três** escrevem. **O operando do C06 sobe de 6 para 9, não para 10**,
e a diferença só continua 2 se cada um dos três handlers novos carregar exatamente um `.Record(`.
O número 10 está riscado; nunca foi medido.

### B6 — o cardinal do teste de paridade tem nome e endereço

`AdminCrudTests.cs:644` afirma `Assert.Equal(13, keys.Count)`. Com o `BatteryKind` e seus dois
membros, passa a **15**. Anotado aqui porque cardinal esquecido fica vermelho como falha de verdade
e custa uma rodada; ele muda na etapa 2, junto com as telas.

### B7 — os dois números que eu afirmei sem medir

Remedidos agora, não copiados:

| O que | O plano dizia | Medido em `13a574c` |
|---|---|---|
| `git ls-files` | 190 | **189** |
| distância do 187 da §0 | três arquivos | **dois** |
| `src/` · `tests/` | 127 · 16 | **127** · **16** |
| `origin/main...main` | `0 4` | **`0 5`** — seus para empurrar |

Nenhum muda decisão nenhuma. Ficam escritos porque eu os tinha afirmado e a regra é medir.

---

## 2. Passo 0, e o portão que estava fechado

**Os 47 controles existentes, medidos com a árvore limpa antes de eu tocar em qualquer arquivo:**

```
Docs/controles/foundation.tsv      18 controles, 0 fora do esperado
Docs/controles/public-site.tsv     17 controles, 0 fora do esperado
Docs/controles/admin-catalog.tsv   12 controles, 0 fora do esperado
```

**O C14 e o C15 estavam vermelhos na primeira volta do plano, e não por código.** O `dotnet build`
não conseguia copiar para `bin/` porque **o site da conferência da leva 04 continuava de pé** —
`OrlandoUp.Web (39492)`, iniciado às 17:25 de 09/09. Você fechou; medidos agora:

| Controle | Medido |
|---|---|
| C14 `a solucao compila` | **0** — build limpo, **0 avisos** |
| C15 `a suite passa` | **0** — **170 passando**, 0 falhando |

**Isso quita a marca `[H]` da §3 da spec**, que herdava esses dois números do relatório da leva 04
sem tê-los medido.

**Identificadores novos, esperado zero, antes de existirem:** `Battery`, `BatteryKind`,
`OperationalSettings`, `BatterySeedData`, `BatterySeeder`, `AddBatteriesAndOperationalSettings` e
`seed-batteries` — **todos 0**.

**`quem-ancora`** sobre os doze arquivos da §11.1: **213 ancoragens, nenhum controle órfão**.
**`proibidos`**: nove controles negativos; o que mais morde nesta leva é o C16 do
`public-site.tsv`, e a §6 mostra o que eu fiz com ele.

---

## 3. Contagens do banco, medidas ANTES de escrever a migration

Uma consulta só, precedida do `SELECT DB_NAME()` (D12); a string de conexão saiu do user-secret por
leitura programática e **não foi impressa**:

```
OrlandoUpDb
```

| Tabela | Linhas |
|---|---:|
| `Products` | 7 |
| `Units` | 10 |
| `AuditEntries` | 8 |
| `ProductTranslations` | 14 |
| `AddOns` | 6 |
| `__EFMigrationsHistory` | **3** |

```
Batteries exists: NAO
OperationalSettings exists: NAO
```

**Os dois modelos de scooter, lidos por consulta:** `Category = 1` devolve **2** produtos, ids **8**
e **9**. É exatamente assim que o seeder os alcança — §6.

**O `sys.default_constraints` de hoje, que é a linha de base da D6/04b:**

```
DeliveryZones . SalesTaxRate   DF__DeliveryZ__Sales__3F466844   ((0.0))
Products . TurnaroundDays      DF__Products__Turnar__4316F928   ((0))
total de restricoes de padrao: 2
```

**Duas, as duas de coluna não booleana** — é onde a leva 04 as deixou. **A D6/04b diz que esta leva
não cria nenhuma, e a leitura depois de aplicada é o item 0 do relatório da etapa 2**, do mesmo jeito
que a `EMENDA-04-04` D2 fez na leva 04: o número tem de continuar **2**.

---

## 4. A migration, lida no SQL e não no C#

Gerada com `dotnet ef migrations add AddBatteriesAndOperationalSettings --project src/OrlandoUp.Web`
e **editada à mão num ponto só**, a §4.2.

### 4.1 O `Up`

Duas tabelas, um índice único, um índice comum:

```sql
CREATE TABLE [Batteries] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [AssetTag] nvarchar(40) NOT NULL,
    [Kind] int NOT NULL,
    [RangeMiles] decimal(5,1) NULL,
    [Status] int NOT NULL,
    [SerialNumber] nvarchar(80) NULL,
    [Notes] nvarchar(400) NULL,
    [PurchasedOn] date NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Batteries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Batteries_Products_ProductId] FOREIGN KEY ([ProductId])
        REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [OperationalSettings] (
    [Id] int NOT NULL,
    [ChargerCount] int NOT NULL,
    [SecondBatteryPerDay] decimal(10,2) NOT NULL,
    [LostChargerFee] decimal(10,2) NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_OperationalSettings] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Batteries_AssetTag] ON [Batteries] ([AssetTag]);
CREATE INDEX [IX_Batteries_ProductId] ON [Batteries] ([ProductId]);
```

**Nenhum `DEFAULT` em nenhuma coluna das duas tabelas** — a D6/04b lida no script, não no modelo.
`ON DELETE NO ACTION` é o `Restrict` da §4.1: um modelo com baterias não é apagado por baixo delas.

### 4.2 A única edição à mão, e a metade da B1 que este relatório prova

A §4.2 da spec manda **a migration criar a linha única**, e o gerador não a criaria: ela é dado, não
schema. Acrescentei o `InsertData` com o motivo ao lado, e é isto que o script emite:

```sql
INSERT INTO [OperationalSettings] ([Id], [ChargerCount], [SecondBatteryPerDay], [LostChargerFee], [UpdatedAtUtc])
VALUES (1, 14, 8.0, 30.0, NULL);
```

| Valor | De onde | Grau |
|---|---|---|
| `ChargerCount = 14` | D37 e a linha da fila | **fato** |
| `LostChargerFee = 30.00` | D37, *"US$ 30 por carregador"* | **fato** |
| `SecondBatteryPerDay = 8.00` | D37 registra *"entre US$ 5 e US$ 10 por dia — é o que 'cerca de US$ 8' queria dizer"* | **padrão, não fato** |
| `UpdatedAtUtc = NULL` | ninguém editou a linha | **ausência, e ela é o valor certo** |

**A §4.2 não dá número para o valor da segunda bateria, e eu não podia inventar um sem dizer.** Pus
o meio da faixa que a D37 nomeia, escrito no comentário do arquivo como padrão e não como fato. **É
o único valor desta migration que você talvez queira diferente**, e o item 9 do roteiro é exatamente
mudá-lo pela tela — o que também prova que a tela edita.

**Duas coisas do script que parecem alarme e não são.** O EF emite um par de
`SET IDENTITY_INSERT` cercado por `IF EXISTS (… sys.identity_columns …)`: como o `Id` é
`ValueGeneratedNever`, a coluna **não** é identidade e o `IF` não dispara. E o
`CREATE UNIQUE INDEX` cai sobre uma tabela **criada vazia na mesma migration**, então a armadilha de
duplicata que a skill nomeia não pode existir aqui.

### 4.3 O `Down`

```sql
DROP TABLE [Batteries];
DROP TABLE [OperationalSettings];
DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260910031613_…';
```

**Descer perde as doze baterias.** É por isso que elas são semeadas por um comando que **você** roda
e não pela migration: dado de frota que um rollback destrói não deveria ser dado que o rollback foi
mandado possuir. A linha de configuração vai junto com a tabela dela, que é a mesma frase lida duas
vezes. O comentário está no arquivo, ao lado dos dois `DropTable`.

---

## 5. Classificação pela skill `revisao-migration-efcore`

Sobre os dois scripts gerados, nunca sobre o `.cs`.

### 5.1 `Up` — 16 sentenças, **0 destrutivas**, 2 de atenção, 3 aditivas

| Item de atenção | Justificativa |
|---|---|
| `CREATE UNIQUE INDEX` | a tabela é **criada vazia nesta mesma migration**: não há linha, logo não há duplicata que faça o comando falhar no meio. O índice **não é filtrado**, então não há exigência de `QUOTED_IDENTIFIER` — medido: `grep -c "CREATE UNIQUE INDEX.*WHERE"` no script devolve **0** |
| `INSERT (seed)` | **é a linha da §4.2, e não `HasData`** — o classificador rotula todo `INSERT` assim. Medido: `grep -rn "HasData" src`, fora de `HasDatabaseName`, devolve **0**. A distinção importa: `HasData` é estado declarado e reconciliaria, sobrescrevendo a multa que você editar; um `InsertData` numa migration roda **uma vez** e nunca mais olha para a linha |

### 5.2 `Down` — 5 sentenças, **3 destrutivas**, 0 de atenção

| Item destrutivo | O que exatamente se perde |
|---|---|
| `DROP TABLE [Batteries]` | as doze baterias, e tudo que tiver sido acrescentado depois pela tela. Hoje: **zero linhas**, a tabela não existe |
| `DROP TABLE [OperationalSettings]` | a linha única e os três valores. Hoje: zero |
| `DELETE FROM [__EFMigrationsHistory]` | uma linha, a desta migration. O `WHERE` nomeia a migration inteira, com carimbo e nome |

**Ninguém aplica o `Down`.** Ele está aqui porque migration que não sabe voltar é migration que
ninguém leu.

### 5.3 As oito armadilhas da skill, medidas uma a uma

| Armadilha | Aplica-se? | Medição |
|---|---|---|
| `HasData` é estado declarado | **não** | `grep -rn HasData src` (sem `HasDatabaseName`) = **0** |
| Enum persistido por posição | **sim, tratada** | `BatteryKind` nasce com os números escritos — `Normal = 1`, `ExtendedRange = 2` — e **acrescentado no fim** de `Domain/Enums.cs`, como o `AuditAction` da leva 04. A coluna é `int` |
| Índice único filtrado | **não** | o índice não tem `WHERE`; medido **0** no script |
| Auto-referência com `SET NULL` | **não** | `grep -ci "SET NULL"` no script = **0**. A única FK é `Batteries → Products`, com `NO ACTION` |
| Data de calendário × instante | **sim, tratada** | `PurchasedOn` é `date` — calendário em Orlando; `CreatedAtUtc` e `UpdatedAtUtc` são `datetime2` e vêm do `IClock`. Nenhum padrão de banco entrega "agora" |
| Tipos | **conferido** | `decimal(10,2)` nos dois valores monetários e `decimal(5,1)` na autonomia; `datetime2` nos instantes; `nvarchar` com tamanho nas três colunas de texto — **`nvarchar(max)` medido 0 no script** |
| Renomear × recriar | **não** | `sp_rename` e `DROP COLUMN` medidos **0** no script |
| Coluna nova obrigatória em tabela com dados | **não** | medido **0**: nenhuma `ALTER TABLE … ADD … NOT NULL`. As colunas obrigatórias nascem em duas tabelas novas e vazias |

### 5.4 Veredito

```
Migration: 20260910031613_AddBatteriesAndOperationalSettings
Classificacao: aditiva com atencao — 0 destrutivos no Up
Operacoes: 2 tabelas novas, 0 colunas novas em tabela existente, 2 indices,
           1 sentenca de dado (a linha unica da §4.2), 0 restricoes de padrao
Itens de atencao: o indice unico cai sobre tabela criada vazia nesta migration;
                  o INSERT e a linha da §4.2 e nao HasData, que nao existe no repositorio
Armadilhas conferidas: as oito da etapa 2 da skill, na tabela da §5.3
Recomendacao: APLICAR. Nada existente e tocado — as duas tabelas sao novas e a unica
              sentenca de dado escreve na tabela que a propria migration acabou de criar.
```

---

## 6. O que está commitado neste commit

| Arquivo | O quê |
|---|---|
| `Domain/Battery.cs` | **novo** — espelha `Unit`, sem `UpdatedAtUtc` (D5/04b, §12.1) |
| `Domain/OperationalSettings.cs` | **novo** — linha única, `SingletonId = 1` |
| `Domain/Enums.cs` | `BatteryKind` **no fim**, números escritos |
| `Configurations/BatteryConfiguration.cs` | **novo** — índice único na etiqueta, FK `Restrict`, `date`, **nenhum `HasDefaultValue`** |
| `Configurations/OperationalSettingsConfiguration.cs` | **novo** — chave **`ValueGeneratedNever`** |
| `Infrastructure/Data/AppDbContext.cs` | dois `DbSet` |
| `Infrastructure/Seeding/BatterySeedData.cs` | **novo** — as doze da `EMENDA-04B-01` |
| `Infrastructure/Seeding/BatterySeeder.cs` | **novo** — insere só no vazio, resolve o modelo por consulta |
| `Infrastructure/Seeding/SeedCommands.cs` | o terceiro comando |
| `Program.cs` | **uma palavra**, a B2 |
| `Migrations/20260910031613_….cs` + `.Designer.cs` + snapshot | gerados; o `.cs` **editado à mão na §4.2** |

**BOM removido dos três arquivos gerados antes de qualquer `git add`** — nasceram com `EF BB BF` e
ficaram `75 73 69`, `2F 2F 20` e `2F 2F 20`.

**A chave da linha única é `ValueGeneratedNever`, e é decisão minha que vale dita:** uma tabela que
pudesse entregar uma segunda chave estaria descrevendo uma lista. Assim, quem insere escreve o `1`,
e a §4.2 continua sendo verdade sobre o schema e não só sobre o costume.

**O seed alcança o modelo por consulta, nunca por literal** — é o C16 do `public-site.tsv`, que varre
`src/` inteiro e exclui **um** arquivo pelo nome, `CatalogSeedData.cs`, que não é este. O
`BatterySeedData` declara a bateria por **posição entre os scooters** e o `BatterySeeder` resolve a
posição contra o banco, **recusando em voz alta** se não achar exatamente dois modelos. A recusa é o
que torna o acoplamento visível em vez de silencioso. Medido com a P1 na árvore: o C16 continua **0**.

**A etiqueta e a grade são colunas separadas** (D38): `BSC-06` não diz que é a Extended Range; a
coluna `Kind` diz. Nada no código lê grade a partir de texto de etiqueta.

---

## 7. Portões

| Portão | Valor |
|---|---|
| `dotnet build OrlandoUp.sln --nologo -v q` | **limpo, 0 avisos** |
| `dotnet test OrlandoUp.sln --nologo -v q` | **170 passando, 0 falhando** — o mesmo de antes, que é o esperado: nenhum teste desta leva existe ainda |
| `foundation.tsv` | **18, 0 fora do esperado** |
| `public-site.tsv` | **17, 0 fora do esperado** |
| `admin-catalog.tsv` | **12, 0 fora do esperado** |

**Nenhum controle dos três `.tsv` existentes se deslocou.**

**Os sete propostos, medidos com a P1 na árvore** (o arquivo continua em `scratchpad/leva04b/` e só
desce para `Docs/controles/` no commit de conteúdo):

| Controle | No plano | Agora | Alvo |
|---|---|---|---|
| C01 nenhuma consulta filtra bateria por tipo | 0 | **0** | 0 |
| C02 ALCANCE de C01, o tipo é lido para exibição | nao | **nao** | sim |
| C03 nenhuma tela cria nem apaga a linha de configuração | 0 | **0** | 0 |
| C04 ALCANCE de C03, a linha é alcançada por uma tela | nao | **nao** | sim |
| C05 ALCANCE do C16, o seed alcança o modelo por consulta | nao | **sim** | sim |
| C06 nenhuma bandeira booleana nova nasce no domínio | 5 | **5** | 5 |
| C07 ALCANCE de C06, dois portadores de `UnitStatus` | nao | **sim** | sim |

**Dois já se moveram nesta parada** — o C05 e o C07 —, que é o que se espera de irmãos de alcance de
coisas que a P1 escreve. O C02 e o C04 se movem com as telas, na etapa 2.

---

## 8. O que a P1 não fez, e o que preciso de você

**Não fiz, de propósito:**

- **não apliquei a migration.** O `__EFMigrationsHistory` continua com **3** linhas;
- **não rodei o seed.** `Batteries` não existe, e escrever frota real é seu;
- **não escrevi tela, recurso, CSS nem teste.** As três telas de escrita e o `AdminCrudTests` são da
  etapa 2, e é lá que o cardinal da B6 vira **15**;
- **não toquei em `Domain/Unit.cs`, `UnitConfiguration.cs` nem nas páginas de `Units/`** — a
  `EMENDA-04B-01` A2 deixa as etiquetas de scooter e cadeira fora desta leva, e elas estão na lista
  negativa;
- **nada de QR** (A3);
- **não empurrei nada.**

**Preciso de você, nesta ordem:**

1. **Uma palavra sobre o `SecondBatteryPerDay = 8.00`** da §4.2 — é o único valor que eu escolhi, e
   escolhi o meio da faixa que a D37 nomeia. Se for outro, ele muda antes de você aplicar;
2. **aplicar a migration:**

   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```

3. **rodar o seed das doze baterias:**

   ```
   dotnet run --project src/OrlandoUp.Web -- seed-batteries
   ```

4. **a liberação da etapa 2**, que escreve as três telas, os recursos, os testes da §8 e o `.tsv`
   novo, e remede o C05/C06 do `admin-catalog.tsv` na linha existente.

Depois de aplicar e semear, quatro conferências baratas — e as quatro entram como item 0 do relatório
da etapa 2, do jeito que a `EMENDA-04-04` D2 fez na leva 04: o `__EFMigrationsHistory` com **4**
linhas; `Batteries` com **12**; `OperationalSettings` com **1**, lendo 14, 8.00 e 30.00; e o
`sys.default_constraints` ainda com **2** restrições, as duas de coluna não booleana — que é a
D6/04b provada em vez de afirmada.
