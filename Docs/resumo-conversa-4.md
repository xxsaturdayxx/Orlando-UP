# Resumo — conversa 4: a administração que escreve, e o mecanismo que estava descrito ao contrário

**Data:** 2026-09-09. **Par:** conversa 4 ↔ **leva 04**. **Anterior:** `Docs/resumo-conversa-3.md`
(conversa 3 ↔ leva 02). **Fecha em:** conteúdo `594ed39`, fechamento `a6f30bc`.

**Proveniência:** `[V]` medido nesta conversa, com o comando ao lado quando é barato repetir;
`[H]` herdado de relato do agente e não reconferido pelo revisor.

---

## 1. Onde o projeto está

`[V]` HEAD `a6f30bc`, árvore limpa. `git ls-files` = **185** (eram 157 na abertura), **127** sob
`src/` e **16** sob `tests/`. **12 commits à frente do remoto, nada empurrado** — o push é do Rod, e
esta conversa produziu **16** commits. A fila tem **três** linhas, todas `concluido`: leva 01 em
`5d538ba`, leva 02 em `383fe27`, leva 04 em `594ed39`; o total de linhas não mudou.

**Três arquivos de controle, 47 controles, 45 no alvo** `[V]`. As duas exceções são o C14 e o C15 do
`foundation.tsv`, que chamam `dotnet build` e `dotnet test` e respondem `127` ao shell do revisor —
`command not found`, o instrumento e não a árvore. O agente os mediu verdes: **build limpo, 0
avisos; 170 testes passando, 0 falhando** `[H]`.

A administração deixou de ser uma vitrine. Rod edita produto, tradução, faixa de preço, vínculo de
adicional e frota pela tela, e cada escrita deixa uma linha de auditoria com quem, quando e o quê.

---

## 2. O que a leva 04 entregou

**Schema — uma migration, `20260908215113_RemoveActiveFlagStoreDefaultsAndAddAuditEntries`,
aplicada ao LocalDB `[V, conferido em três consultas na §0 do relatório da etapa 2]`.** O padrão de
banco saiu de **cinco** colunas booleanas — as quatro `IsActive` que a D34 nomeava e a
`Products.IsBookable` que ninguém sabia que tinha uma. A tabela `AuditEntries` nasceu. Depois de
aplicada, a mesma consulta que antes listava **sete** restrições de padrão lista **duas**, e as duas
são de coluna não booleana.

**Sete telas novas**, todas herdando a proteção da pasta sem uma linha de registro: editor de
produto com os quatro blocos, criação mínima que redireciona para o editor, lista e edição de frota,
e o registro de auditoria. Mais a barra de navegação, que a administração não tinha.

**O arreio de teste que não existia.** Antes desta leva **nenhum teste do repositório conseguia
autenticar nem postar um formulário** `[V]`. Agora existe um handler de autenticação de teste que
vive só em `tests/`, um auxiliar que lê o token de antiforgery do HTML renderizado, e a antiforgery
continua **ligada** na suíte. A suíte foi de 138 para 170.

---

## 3. As armadilhas descobertas — o que vale além desta leva

| Armadilha | O que acontece | Como se descobriu |
|---|---|---|
| **O mecanismo do `HasDefaultValue` estava descrito ao contrário em três documentos** | a D32, a D34 e o comentário de `ProductConfiguration.cs` diziam que declarar o padrão **por valor** engole um `false` explícito. É o inverso: por valor, o EF move a **sentinela** da propriedade para aquele valor, então o `false` difere dela e **é enviado**. Quem engole é `HasDefaultValueSql`, que este repositório não usa | o agente montou um projeto descartável fora do repositório e mediu, duas vezes, em EF Core 10.0.11 — **D35** |
| **Coluna criada por `AddColumn(defaultValue:)` deixa restrição PERMANENTE que o modelo não vê** | o snapshot nunca carrega essa restrição, então **nenhuma migration posterior a remove** e nenhum controle de modelo a enxerga. Foi assim que a `Products.IsBookable` — a coluna que a D32 deliberadamente deixou sem padrão — ficou com `DEFAULT ((0))` no banco | o agente foi ao `sys.default_constraints` em vez de ao código, e parou para perguntar em vez de ampliar a migration sozinho |
| **O teste que a spec pedia para provar a migration seria verde antes e depois** | um ida-e-volta com `false` passa nos dois estados, porque a forma por valor nunca engoliu nada. Verde falso por construção | consequência direta da medição acima; a spec retirou a frase e o teste virou par forma + comportamento |
| **`grep -rl` sobre `tests/` casa dentro de DLL** | um controle de alcance nasceu `sim` **antes** de a costura existir, casando dentro de quatro DLLs da plataforma em `bin/`. Todo controle leva `-I` e `--exclude-dir=bin --exclude-dir=obj`, e identificador curto demais (`FormPost`) aparece em assembly compilado | a asserção de dois lados do próprio agente, rodada antes de escrever a costura |
| **Caixa de marcação e campo oculto de um `bool` têm ORDEM** | o ligador lê o **primeiro** valor de um nome repetido. Com o oculto antes da caixa, toda caixa marcada chega como `false` — o editor teria **despublicado silenciosamente todo produto salvo**, e nenhuma tela reclama disso | o agente escreveu invertido, um teste pegou, e ele registrou o defeito com o mecanismo em vez de só corrigir |
| **Chave de recurso montada por interpolação é invisível para a parity test** | `Admin_Status{...}` e as outras faltariam nos **dois** arquivos igualmente, e a comparação entre eles não veria nada. Uma varredura literal por chaves órfãs relata **catorze** e **treze são falsas** pelo mesmo motivo | o agente viu sozinho e escreveu `Every_enum_member_a_screen_names_has_a_key_in_both_cultures` |
| **O editor mostrava o código interno do adicional** | `cup-holder` onde o banco guarda *Porta-copos*. Única tela da administração com identificador no lugar de texto, e **nenhum teste a pegaria**: uma asserção de que a caixa existe passa dos dois jeitos | a conferência visual, olhando |
| **`role="alert"` não se testa clicando** | o leitor de tela ler a página quando se clica numa linha é navegação normal; não prova que a mensagem é **anunciada sozinha** quando aparece | o revisor separou as duas coisas antes de aceitar o item |

---

## 4. O ritual: sete emendas, e o que cada lado pegou

A spec `Docs/spec-04-admin-catalog.md` fechou com **1255 linhas** e **sete notas de emenda datadas
no topo**, que vencem o corpo. Toda nota termina com o comando que prova que o agente a leu, e todas
as sete provas voltaram maiores que zero `[V]`.

| Emenda | O que fixou |
|---|---|
| `EMENDA-04-01` | doze itens da revisão do plano — o teste verde-falso, os serviços em DI, o produto nascendo oculto, `UpdatedAtUtc`, três controles |
| `EMENDA-04-02` | plano aprovado; **duas correções do revisor retiradas pelo agente** |
| `EMENDA-04-03` | a quinta restrição de padrão entra na migration, com `Down` simétrico; o comentário liberado; a D35 |
| `EMENDA-04-04` | migration aplicada, e as três conferências pós-aplicação como item 0 da parada seguinte |
| `EMENDA-04-05` | asserção exata na prova do token; erro do revisor na §3 registrado |
| `EMENDA-04-06` | `FormFields.cs` regularizado; chave órfã; a sequência de fechamento com um dono por passo |
| `EMENDA-04-07` | os dois achados da conferência visual, com a forma do conserto fixada |

**O que só o agente pegou:** o sentinela do EF, a quinta restrição no banco, a armadilha da DLL, a
ordem da caixa de marcação, as chaves por interpolação, e o `Program.cs` que a lista negativa
fechava sem que o revisor tivesse notado que um serviço precisa de registro.

**O que só a revisão pegou:** o teste que seria verde antes e depois, o produto que nasceria
visível, o `UpdatedAtUtc` órfão desde a leva 01, e o controle da costura de teste que faltava.

**As duas metades funcionaram uma contra a outra, e é isso que o desenho compra.**

---

## 5. A conferência visual, e por que ela pagou

`Docs/conferencia-leva-04.md`: **treze linhas, nove verdes, cinco não exercidas**, cada uma com o
motivo escrito. Ela encontrou o que nenhum portão encontraria — o seletor de adicionais mostrando
código — e duas correções de conteúdo, uma das quais o Rod faz pela própria tela que a leva criou.

Também produziu um **alarme falso registrado como tal**: um "a página rola horizontalmente" que,
remedido com uma varredura de console, era a rolagem interna da tabela mais o modo *Fit to window*
do simulador. Está escrito no documento justamente para ninguém o levantar de novo.

**E produziu o achado mais caro da conversa, que não é da leva 04:** as baterias das scooters são
removíveis, identificadas individualmente, e um aluguel sai com uma ou duas. Nenhum documento do
projeto carregava isso. Virou **Q14**, a responder **antes da spec da leva 03**, porque muda o
schema dela e não só as telas.

---

## 6. Uma limitação do revisor, e um padrão dele

**`dotnet` continua fora do alcance do shell da ponte**, e o `OrlandoUpDb` também. Todo número de
build, de suíte e de banco deste resumo é `[H]`; as três conferências pós-migration só puderam ser
medidas pelo agente, e por isso viraram item obrigatório de relatório em vez de confiança.

**E um padrão que vale mais que a limitação: quatro vezes nesta conversa o revisor parafraseou uma
medição em vez de refazê-la** — o comando do controle de padrões booleanos que ele diagnosticou sem
ler, o controle de alcance que ele pediu sem levar a regra de varredura junto, a frase sobre
`Program.cs` que fundia dois fatos numa medição que não existia, e o nome de uma caixa de marcação
que ele mandou procurar duas vezes e que não está escrito em lugar nenhum da interface. Três foram
pegas pelo agente; a quarta, pelo Rod. **Nenhuma delas foi falta de acesso** — o comando estava a
uma chamada de distância nas quatro. Ver `Docs/atrito-conversa-4.md`.

---

## 7. Decisões permanentes

- **D33** — as levas seguem a numeração das **fases** do roadmap, nunca a ordem de execução. A
  leva 04 rodou antes da 03; a 03 mantém o número e é a próxima.
- **D34** — nenhuma coluna booleana carrega padrão de banco. É `IsRequired()` e nada mais.
- **D35** — corrige o **motivo** da D32 e da D34 sem mudar o que elas fazem, e acrescenta a regra:
  depois de migration que acrescenta coluna com `defaultValue`, **ler `sys.default_constraints`** —
  o modelo não conta.
- **A costura de teste nasce em `tests/`, nunca em `src/`**, e um controle mede isso. Um desvio que
  a subida da aplicação possa ligar está a uma variável de ambiente de distância de uma
  administração aberta.
- **Antiforgery fica ligada na suíte**, e o token sai do HTML renderizado. Desligá-la deixaria a
  suíte verde sobre um formulário que não se posta em produção.
- **Toda asserção de ausência afirma uma presença no mesmo método**, e todo controle nasce com par
  de dois lados rodado a partir do arquivo gravado. Nesta conversa, das duas asserções de dois lados
  escritas, **uma pegou defeito real**.

---

## 8. Pendências

**Do Rod (o que a automação não alcança):**

1. **Push** — 12 commits à frente `[V]`.
2. **Q14, as baterias** — cinco pontos escritos em `Docs/open-questions.md`, e a spec da leva 03 não
   nasce antes deles.
3. **Corrigir *"Carrinho simples"* pela tela** — é conteúdo, é dele, e não precisa de código.
4. **A rodada de estética da administração**, que carrega junto os cinco itens não exercidos da
   conferência (anel de foco, `Shift+Tab`, o anúncio do leitor de tela, o produto nascendo oculto
   pela tela, e as faixas a 375 px) e os três achados de usabilidade A4, A5 e A6.
5. **Q13** — as etiquetas dos modelos. **Q12** — o WhatsApp; quando entrar, aposentar o C16 do
   `foundation.tsv` junto, nunca baixar o limiar.

**Backlog a arquivar pelo revisor, agora que a leva fechou:** `TurnaroundDays` e `SalesTaxRate` com
padrão de banco da mesma forma em coluna não booleana; a política que distinguiria `Admin` de
`Staff`; o `CatalogSeedData.cs` com o nome antigo do carrinho, que um banco criado do zero
reintroduziria.

**De conversas anteriores, ainda de pé:** Q3–Q7; os itens 7, 8 e 9 da `conferencia-leva-02.md`;
espanhol; caução como hold no cartão; `ForwardedHeaders` no App Service.

---

## 9. Próximas frentes candidatas

- **Leva 03 — reserva.** É o que transforma o site em negócio, e agora tem uma dependência a mais:
  a Q14. Precisa de Q3–Q6 respondidas antes da spec.
- **Rodada de estética da administração.** Não é leva: é CSS, texto e ordem de campo. Barata,
  reversível, e fecha cinco pendências de conferência de uma vez.
- **Imagens (D30).** Continua parada desde a conversa 3, e continua sendo o que mais muda a página
  pública por menos trabalho.

---

## 10. Abertura da próxima conversa

1. **Você — ação:** empurrar os 12 commits.
   ```
   git push origin main
   ```
2. **Você — ação:** abrir `https://localhost:7420/admin/products`, clicar em **Editar** na linha do
   carrinho simples, trocar o nome em português, salvar. Trinta segundos, e é a primeira correção de
   conteúdo que o sistema torna possível sem passar por commit.
3. **Você — decisão:** as cinco perguntas da **Q14** sobre baterias. São a única coisa que bloqueia
   a spec da leva 03.
4. **Cole no Claude (Cowork):** *"abrindo a conversa 5 do Orlando Up. Leva 04 fechada em `594ed39` /
   `a6f30bc`, conversa 4 fechada. Quero decidir a frente da conversa 5."* — e leve as respostas da
   Q14 junto, ou a conversa gasta a primeira rodada nelas.
5. **Cole no Claude Code:** nada antes de a spec da frente escolhida existir e de a linha nascer em
   `Docs/fila-cc.md`.

---

*A administração escreve, e cada escrita deixa rastro. O que a leva provou não foi só o CRUD: foi
que o portão pega o que o portão pega, que a conferência visual pega o que ele não pega, e que a
prosa de um projeto envelhece errado sem ninguém notar — três documentos deste repositório
explicavam ao contrário um mecanismo que nunca tinha mordido aqui. O que falta para o site ser um
negócio continua sendo a reserva; e ela agora sabe que precisa contar baterias.*
