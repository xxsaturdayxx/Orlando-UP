# Relatório — leva 04b, etapa 2: as telas da frota e das configurações (parada P2)

**Data:** 2026-09-10. **HEAD ao escrever:** `772f3b5`. **Spec:** `Docs/spec-04b-fleet-batteries.md`
com as notas **`EMENDA-04B-01`** a **`EMENDA-04B-03`** no topo, que vencem o corpo onde discordarem.
**Linha da fila:** `2026-09-09`, "LEVA 04B — FLEET BATTERIES", ainda `aguardando`.

**Prova de leitura exigida pela `EMENDA-04B-03`:** ocorrências da cadeia `EMENDA-04B-03` neste
arquivo, contadas com `grep -c "EMENDA-04B-03" Docs/relatorio-leva-04b-etapa-2.md`: **5**.

**Tudo escrito, suíte verde, os quatro `.tsv` verificados.** O que falta é a conferência visual, que
é sua, preenchendo o roteiro que já existe. Nada foi empurrado.

| Portão | Valor |
|---|---|
| `dotnet build OrlandoUp.sln --nologo -v q` | **limpo, 0 avisos** |
| `dotnet test OrlandoUp.sln --nologo -v q` | **181 passando, 0 falhando** (eram 170 na P1) |
| `foundation.tsv` | **18, 0 fora do esperado** |
| `public-site.tsv` | **17, 0 fora do esperado** |
| `admin-catalog.tsv` | **12, 0 fora do esperado** |
| **`fleet-batteries.tsv`** | **7, 0 fora do esperado** — desceu do `scratchpad/` neste commit |

---

## 0. As quatro conferências pós-aplicação

Rodadas antes de qualquer outra coisa, do jeito que a `EMENDA-04-04` D2 estabeleceu na leva 04: uma
migration aplicada que ninguém conferiu é um schema aceito no fio do bigode, e o shell da ponte não
alcança o `OrlandoUpDb`. `SELECT DB_NAME()` colado antes da primeira consulta (D12); a string saiu do
user-secret por leitura programática e **não foi impressa**:

```
OrlandoUpDb
```

| # | O que | Esperado | Medido | |
|---|---|---|---|---|
| 1 | histórico de migrations | 4, a quarta é a desta leva | **4**, e a quarta é `20260910031613_AddBatteriesAndOperationalSettings` | ✅ |
| 2 | `Batteries` | 12 | **12** | ✅ |
| 3 | `OperationalSettings` | 1 linha, 14 / 8.00 / 30.00 | **1**, `charger=14 second=8.00 lost=30.00` | ✅ |
| 4 | `sys.default_constraints` | continua **2**, as duas de coluna não booleana | **2** | ✅ **D6/04b provada, não afirmada** |

**O item 4 é a linha que importa.** A D6/04b diz que esta leva não cria padrão de banco nenhum, e a
D35 é a lição de que um padrão criado por migration sobrevive invisível ao modelo. As duas tabelas
novas somam **zero** restrições: o total continua nas duas de `DeliveryZones.SalesTaxRate` e
`Products.TurnaroundDays`, que são de coluna **não booleana** e são onde a leva 04 as deixou.

### 0.1 A leitura que a `EMENDA-04B-03` C1 pede, e ela saiu certa

O C1 manda ler a ordem dos modelos **antes** de semear, porque o seed resolvia o modelo por posição.
Medido depois do fato, que é o que dá para fazer agora — e o resultado é o certo:

```
ordem dos scooters (SortOrder | Id | Slug):
1 | 8 | drive-scout-4
2 | 9 | drive-spitfire-ex
```

E as doze linhas que o seed gravou, conferidas uma a uma contra o modelo:

```
BSC-01 … BSC-05  ->  drive-scout-4      kind=1  range=9.0
BSC-06           ->  drive-scout-4      kind=2  range=14.0
BSP-01 … BSP-06  ->  drive-spitfire-ex  kind=1  range=9.0

Extended Range em: drive-scout-4 (BSC-06)
```

**A de autonomia estendida está no Drive Scout, que é o que a D37 diz.** As etiquetas podem ser
impressas: nenhuma delas está na máquina errada.

---

## 1. O C1, e o que ele conserta e o que não conserta

`BatterySeeder.cs` ganhou a linha:

```csharp
.OrderBy(product => product.SortOrder)
.ThenBy(product => product.Id)
```

**O que ela conserta:** `SortOrder` é editável pela tela que a leva 04 construiu, e nada proíbe dois
produtos carregarem o mesmo. Com valores iguais a ordem é indefinida e pode diferir entre duas
consultas, então qual modelo é "o primeiro" podia virar entre um ensaio e a rodada de verdade. `Id`
é estável e monotônico, então o par é determinístico.

**O que ela não conserta, e o comentário no arquivo diz isso:** um `SortOrder` que alguém reordenou
de verdade. Contra isso o instrumento é a leitura da §0.1 — e é por isso que ela acontece **antes**
do seed e não depois, porque o que sai da tela vai para o adesivo.

**O teste `The_seed_writes_twelve_batteries_and_running_it_twice_changes_nothing` afirma a mesma
ordenação** — seis por modelo e a estendida no primeiro scooter por ordem de exibição —, então uma
regressão do desempate cai na suíte e não numa etiqueta colada na bateria errada.

---

## 2. O que a etapa escreveu

### 2.1 Quatro telas

| Rota | O quê |
|---|---|
| `/admin/batteries` | etiqueta, modelo, tipo, autonomia, situação, data; ordenada por modelo e etiqueta; **as aposentadas ficam na lista, marcadas**; o total disponível no alto |
| `/admin/batteries/create` | uma bateria |
| `/admin/batteries/edit/{id:int}` | uma bateria |
| `/admin/settings` | os três números, e **só isso** |

**A barra de navegação passa de quatro para seis destinos**, dentro do `_AdminLayout.cshtml` e nunca
em parcial sob `Pages/Shared/` — o C10 do `public-site.tsv` exclui `Pages/Admin/` e filtra as linhas
do `_AdminLayout` pelo nome, mas **não** exclui `Pages/Shared/`. **O painel passa de três para cinco
contagens**, com baterias e carregadores.

**A tela de configurações edita e não faz mais nada.** Não cria a linha e não a apaga: a migration a
fez, e o C03 do `.tsv` novo é a proibição escrita como controle e não como costume. Um banco sem a
linha é estado que a aplicação não consegue produzir — o C09 do `foundation.tsv` mantém criação de
schema fora da subida —, então a tela **relata a ausência** em vez de inventar valores que ninguém
decidiu.

**Nenhuma marcação de administração ramifica por cultura.** Medido: o C09 do `public-site.tsv`
continua devolvendo a lista literal de dois caminhos, `_AdminLayout.cshtml,_Layout.cshtml`.

### 2.2 Nenhuma linha de CSS, e isso é resultado e não esquecimento

A §11.1 autoriza o `site.css` **"só se um controle genuinamente ainda não tiver estilo"**. Medido:
as quatro telas usam `section`, `section__lead`, `page`, `field`, `field__help`, `error-summary`,
`notice`, `table-scroll`, `todo`, `button`, `button--primary` e `button--quiet` — **todos existem**,
os de formulário desde a leva 04. `select`, `textarea` e a caixa de marcação já ganharam regra lá.
**O arquivo aparece em zero linhas do diff da faixa.**

### 2.3 Auditoria: três handlers de escrita novos, três registros

```
OnPost[A-Za-z]*Async  sob Pages/Admin  ->  9   (eram 6)
[.]Record[(]          sob Pages/Admin  ->  7   (eram 4)
C05 de admin-catalog = 9 - 7           ->  2
```

**É exatamente o que a `EMENDA-04B-02` B5 previu contra o que a §11.2 item 3 dizia:** três handlers
e não quatro, operando **9** e não 10, diferença firme em **2**. `EntityType` sai de `nameof` nos
três; a tela de bateria distingue `Deactivated` quando ela é aposentada e `Reactivated` quando volta,
o que é o enum da leva 04 sendo usado para o que ele existe.

---

## 3. Os dez itens da §8, mais um

**11 testes novos; a suíte vai de 170 para 181.** Todos verdes.

| § | O que | Teste |
|---|---|---|
| 1 | o seed escreve doze, seis por modelo, e rodado duas vezes não muda nada | `The_seed_writes_twelve_batteries_and_running_it_twice_changes_nothing` |
| 2 | etiqueta duplicada é recusada como validação, e outra salva | `A_duplicate_battery_tag_is_refused_as_validation_and_a_different_one_saves` |
| 3 | bateria não se prende a produto que não é scooter, e numa scooter salva | `A_battery_cannot_be_attached_to_a_product_that_is_not_a_scooter` |
| 4 | autonomia ausente sobrevive à ida e volta e **volta vazia** | `An_absent_battery_range_survives_the_round_trip` |
| 5 | aposentar mantém na lista, marcada, e a contagem de disponíveis cai em um | `Retiring_a_battery_keeps_it_on_the_list_and_drops_the_available_count` |
| 6 | **exatamente uma** linha de auditoria por handler de escrita | três testes, um por handler |
| 7 | a linha de configuração é editada, nunca criada nem apagada | `The_settings_screen_edits_the_row_and_leaves_exactly_one_audit_line` + **C03** |
| 8 | contagem e valor negativos recusados, **zero aceito** | `A_negative_charger_count_and_a_negative_amount_are_refused_and_zero_is_accepted` |
| 9 | os membros de `BatteryKind` têm chave nas duas culturas | o teste de interpolação, estendido — **B6** |
| 10 | nenhuma coluna das duas tabelas declara padrão de banco | `No_battery_column_declares_a_store_default` |
| — | **toda etiqueta semeada tem a forma da D38, e a grade não está nela** | `Every_seeded_tag_carries_the_shape_D38_fixed_and_the_grade_is_not_in_it` |

**O item 10 lê as anotações e nunca o `GetDefaultValue()`** — que responde o padrão da linguagem para
propriedade que não declara nada e passaria dos dois jeitos. É a lição da D35 e do par A1 da leva 04,
aplicada às quinze colunas das duas tabelas, com a presença afirmando que o laço percorreu quinze.

**O item 7 é a metade que a suíte consegue provar**, e a `EMENDA-04B-02` B1 é o motivo: a suíte cria
schema com `EnsureCreatedAsync` e **nunca roda migration**, então a linha é **arranjada** no teste e
não esperada. A outra metade — que a **migration** cria a linha — está provada na §4.2 do relatório
da etapa 1, lendo o `InsertData`, e de novo no **item 3 da §0** deste, lendo os três valores no banco
real. Um comentário no arquivo de teste diz isso, para ninguém reescrever o teste como se a suíte
visse migrations.

**A B6 tem número:** o cardinal do teste de paridade foi de **13** para **15**, com os dois membros
de `BatteryKind`.

### 3.1 Nada do que a §8 protege quebrou

`SeoTests.cs:201`, `SeoTests.cs:88`, `CultureRoutingTests.cs:64`, `SeedingTests.cs:31`,
`DomainTests.cs:201` (`Only_a_product_on_sale_carries_units` — baterias não são unidades) e
`ArchitectureTests.cs:32` passam. Nenhum endereço entrou em `SiteBehaviourTests.PublicPathList`,
nada liga `/admin` do leiaute público, o `CatalogSeedData` não foi tocado, e o escritor ficou em
`Infrastructure/Data/`.

---

## 4. Os controles

### 4.1 Os sete novos, do início ao fim

| Controle | No plano | Na P1 | Agora | Alvo |
|---|---|---|---|---|
| C01 nenhuma consulta filtra bateria por tipo | 0 | 0 | **0** | 0 |
| C02 ALCANCE de C01, o tipo é lido para exibição | nao | nao | **sim** | sim |
| C03 nenhuma tela cria nem apaga a linha de configuração | 0 | 0 | **0** | 0 |
| C04 ALCANCE de C03, a linha é alcançada por uma tela | nao | nao | **sim** | sim |
| C05 ALCANCE do C16 de `public-site`, o seed alcança o modelo por consulta | nao | **sim** | **sim** | sim |
| C06 nenhuma bandeira booleana de visibilidade nova nasce no domínio | 5 | 5 | **5** | 5 |
| C07 ALCANCE de C06, dois portadores de `UnitStatus` | nao | **sim** | **sim** | sim |

**Os quatro irmãos de alcance se moveram todos** — dois na P1, dois agora. Os três negativos medem o
mesmo antes e depois de propósito, que é o que um negativo faz.

**O C06 é o que a `EMENDA-04B-02` B3 consertou**, e vale repetir por que: o operando antigo
`public bool IsActive` respondia **0** contra um `Domain/` com `IsVisible` e `IsRetiredFromFleet` —
passava exatamente no que existia para recusar — e não via a quinta bandeira que já estava na árvore,
`Product.cs:43`. Com `public bool Is[A-Za-z]+` ele mede **5** e continua **5**, porque a `Battery`
carrega `Status` e não bandeira, que é a D5/04b.

### 4.2 O C05 e o C06 do `admin-catalog.tsv` — remedidos **na linha existente**

A linha da fila autoriza mexer nesses dois e só nesses dois, na linha que já existe e nunca em linha
paralela. O C05 continua **2** e não precisou de edição. **O C06 precisou, e o motivo é o que este
projeto passa o tempo pegando:**

```diff
-… | wc -l; true) -ge 6 && echo sim || echo nao   C06 ALCANCE de C05 o operando maior e maior ou igual a seis
+… | wc -l; true) -ge 9 && echo sim || echo nao   C06 ALCANCE de C05 o operando maior e maior ou igual a nove
```

Com o limiar em 6 ele media `sim` **antes** desta leva e `sim` depois — irmão de alcance que não
alcança nada. Com 9, ele afirma que os três handlers novos existem. É uma linha, na linha existente,
e o `admin-catalog.tsv` aparece no diff com **1 acrescentada e 1 removida**.

### 4.3 Os 47 permanentes

Todos no alvo. Os que esta leva mais arriscava:

| Controle | O que ele guarda aqui | Medido |
|---|---|---|
| **C16 de `public-site`** | nenhum identificador de catálogo digitado em `src/` fora do arquivo de nome `CatalogSeedData.cs` — **e o seed de baterias não é ele** | **0** |
| C17 de `foundation` | ausência nunca coalesce para zero; `RangeMiles` é `decimal?` do formulário até a coluna | **0** |
| C06 de `foundation` | o relógio real lido num arquivo só | no alvo |
| C09 de `foundation` | nenhuma criação de schema na subida | **0** |
| C16 de `foundation` | os quatro marcadores `TODO-` continuam em `appsettings.json` — a D4/04b existe para que continuem | no alvo |
| C09 de `public-site` | só os dois leiautes ramificam por cultura | `_AdminLayout.cshtml,_Layout.cshtml` |
| C11 e C12 de `admin-catalog` | a costura de teste não existe em `src/` | no alvo |

---

## 5. Escopo, medido contra a faixa de commits

`git diff --numstat 13412f2` sobre a lista negativa inteira: **`appsettings.json`,
`CompanyOptions.cs`, `CatalogSeedData.cs`, `CatalogSeeder.cs`, `CatalogQueries.cs`,
`CatalogViews.cs`, `TranslationPicker.cs`, `RichText.cs`, `PricingTierRules.cs`,
`StructuredData.cs`, `SitemapEndpoints.cs`, os quatro de `Infrastructure/Localization/`,
`Domain/Unit.cs`, `UnitConfiguration.cs`, as páginas de `Units/`, `CLAUDE.md`,
`Docs/decisions.md`, `Docs/conferencia-leva-04b.md`, `Docs/controles/foundation.tsv`,
`Docs/controles/public-site.tsv`, `.githooks/pre-commit`, `.gitattributes`,
`.github/workflows/ci.yml`, `Docs/fila-cc.md` e `wwwroot/css/site.css` aparecem em ZERO linhas.**

**`Program.cs`: 1 acrescentada, 1 removida** — a palavra da B2, que é a única linha que a §11.1
concede. **As páginas de `Units/`: zero**, que é a `EMENDA-04B-01` A2 respeitada — as etiquetas de
scooter e cadeira são suas, pela tela, e não desta frente.

**`Docs/spec-04b-fleet-batteries.md` aparece na faixa e não é meu:** os dois commits que o tocam são
`13a574c` e `772f3b5`, as emendas do revisor. `git log --oneline 13412f2..HEAD -- <o arquivo>`
devolve esses dois e mais nenhum.

**Arquivo de teste tocado: um só, `AdminCrudTests.cs`**, que é o que a §11.1 nomeia.

**BOM:** nenhum arquivo da faixa começa com `EF BB BF`.

---

## 6. O que falta, e é seu

1. **A conferência visual**, preenchendo `Docs/conferencia-leva-04b.md`, que já existe com a coluna
   Resultado em branco. **Eu não escrevo resultado nele** — está na lista negativa, e conferência que
   o agente escreve é conferência que ninguém fez. As pré-condições estão no topo do roteiro; a de
   número 4 é a prova de build fresco, e agora ela tem número: **a barra tem seis destinos e o painel
   mostra cinco contagens**. Se tiver quatro e três, o binário é velho.
2. **Feche o site quando terminar.** Na primeira volta do plano desta leva o `dotnet run` da
   conferência anterior travava o `bin/` e deixava o C14 e o C15 vermelhos por motivo que não era
   código; custou uma rodada.
3. **O fechamento em dois commits**, depois da conferência: o de conteúdo, com os resultados que você
   escreveu no roteiro, e o de fechamento, que grava o hash do primeiro na coluna Commit desta linha
   da fila e passa o Estado para `concluido`.
4. **O push**, que é seu: `origin/main...main` lê `0 7`.

**Uma nota de conteúdo, não de código:** o item 9 do roteiro manda mudar a multa de carregador e
reabrir. Se aproveitar, o valor da segunda bateria — os **8.00** que eu escolhi como meio da faixa
que a D37 nomeia, e que a `EMENDA-04B-03` C2 deixou de pé — é editável na mesma tela. Uma edição, sem
migration, que é exatamente o que a D4/04b comprou.

---

## 7. Fechamento — uma correção entrou depois desta parada, em commit próprio

**A etapa 2 foi aprovada pela `EMENDA-04B-04` com uma correção, e ela está aplicada antes da
conferência visual, num commit só dela.**

**D1 — o painel transformava linha ausente em zero carregadores, e a D15 diz que ausência nunca é
zero.** `Pages/Admin/Index.cshtml.cs` projetava a contagem num `int` não anulável, então um banco sem
a linha de configuração devolvia `0`, e a tela imprimia **"0 carregadores"** ao lado de baterias —
que é uma afirmação sobre a frota e não sobre uma linha que falta. O comentário que eu tinha escrito
ali dizia a troca em voz alta — *"mostra zero em vez de lançar"* — e a troca estava errada: a
terceira saída é **mostrar a ausência**, e ela já existia nesta frente, uma tela ao lado, em
`Settings/Index.cshtml`.

**A correção, duas linhas:** a propriedade vira `int?`, a projeção vira `(int?)row.ChargerCount`, e a
marca renderiza `Admin_NotSet` sob `class="todo"` — **sem chave de recurso nova e sem estilo novo**.

**Nenhum controle pega isso, e essa é a metade que importa.** O C17 do `foundation.tsv` chama-se
*preço ausente nunca coalesce para zero* e o operando dele nomeia **duas** formas, `?? 0` e
`GetValueOrDefault(`. Uma projeção de tipo de valor por `FirstOrDefault` é um **terceiro membro da
mesma classe**, e o C17 media **0** com o defeito presente — exatamente a forma que a
`EMENDA-04B-02` B3 nomeou no C07: o rótulo nomeia uma classe, o operando conta membros dela.
**O C17 não foi alargado**, e o motivo é medido: **onze** das doze chamadas `FirstOrDefault*` de
`src/` caem sobre entidade ou cadeia e estão certas como estão, então um grep que separasse
projeção de tipo de valor por forma é o tipo de fórmula esperta que acaba verde falso.

**O instrumento é teste, e um deles é asserção de tipo, porque tipo é barreira e hábito não é:**

| Teste | O que afirma |
|---|---|
| `The_dashboard_charger_count_is_nullable_so_absence_cannot_become_zero` | `ChargerCount` é `int?` |
| `The_dashboard_shows_the_chargers_as_not_set_when_the_settings_row_is_missing` | com a linha ausente a marca aparece e **não** há estatística de carregador lendo zero; **e a de baterias lê 0 na mesma página**, porque tabela vazia honestamente tem zero — ausência e zero são respostas diferentes e o painel passa a dar uma para cada; com a linha posta, a mesma estatística lê **14** |

**Os dois eram vermelhos em `8c98d26`, e isso está medido e não suposto:** `git show` daquele commit
mostra `<span class="stat__value">@Model.ChargerCount</span>` sem o `id`, e
`public int ChargerCount` sem interrogação.

**O roteiro não muda.** O item 0 já lê as cinco contagens do painel, e no seu banco a linha existe —
então ele passaria dos dois jeitos. **É por isso que quem carrega este achado é o teste e não o
olho.**

**Portões depois da correção:** `dotnet build` limpo com **0 avisos**; `dotnet test` **183 passando,
0 falhando** (eram 181); os quatro `.tsv` com **54 controles, 0 fora do esperado**; o C17 continua
medindo **0**, agora sem o defeito por baixo.
