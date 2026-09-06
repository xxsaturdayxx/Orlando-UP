# Leva 01 executada — a aplicação existe e fala duas línguas (resumo da conversa 2)

**Data:** 2026-09-04 a 2026-09-05. **Par:** conversa 2 ↔ leva 01 (**executada e fechada**).
**Anterior:** `Docs/resumo-conversa-1.md` (fundação do repositório; escreveu a spec que esta
conversa executou).

Proveniência: `[V]` verificado nesta conversa (como, entre parênteses); `[H]` herdado sem
reconferência.

---

## 1. O que a conversa fez

A conversa 2 foi inteira de **revisão**: o Claude Code executou `Docs/spec-01-foundation.md` em
seis etapas com quatro paradas (P0 plano, P2 migration, P1 segredos, P3 pré-commit), e o Claude
Web revisou cada parada **contra o código, não contra o relatório**, commitando cada decisão como
nota datada na spec. Resultado: commit de conteúdo **`5d538ba`** (114 arquivos, 8.678 linhas) e
commit de fechamento **`f8933c6`** (só a linha da fila), `[V, git log; git show --stat]`.
`main` empurrado pelo Rod e igual a `origin/main` `[V, git status -sb]`.

O que existe agora, `[V, get_page_text em https://localhost:7420/ e /pt; git ls-files]`:
`OrlandoUp.sln`, `src/OrlandoUp.Web` (Razor Pages, .NET 10, EF Core 10.0.11, Identity),
`tests/OrlandoUp.Tests` (63 testes em 9 classes), `.github/workflows/ci.yml`, LocalDB
`OrlandoUpDb` com a migration `InitialCreate` aplicada e o catálogo semeado (7 produtos, 6
adicionais, 4 zonas, 10 locais, 112 linhas), sete páginas públicas em `/` e `/pt`, `/admin` com
login e painel read-only, `/healthz`, `robots.txt` fechado, `Docs/controles/foundation.tsv` com 18
controles verdes, `Docs/conferencia-leva-01.md` com os 10 itens da §8 (9 fechados, o 7 parcial —
ver §6).

## 2. Decisões tomadas nesta conversa (todas em nota datada no topo da spec)

| Decisão | Quem | Motivo |
|---|---|---|
| **K4 (a)** — laranja `#F26B1D` só como superfície (texto ink por cima, 4,85:1); laranja-como-texto usa o token derivado **`--color-action-text: #B84A0C`** (4,99:1) | Rod, recomendação do Claude Web | o laranja da marca dá 2,91:1 sobre o off-white e 3,05:1 sob branco; D9 exige 4,5:1. Registrado em `Docs/architecture.md` §12 |
| **K5 (a)** — o `meta generator` emite `OrlandoUp <hash>+dirty`; o `+dirty` **é** a prova de build fresco | Rod | o hash do commit de conteúdo não existe na hora da conferência; a alternativa era um terceiro commit contra a fila |
| **C17/C18 ficam** no `.tsv` (D15, preço nulo nunca coalesce para zero) | Rod | acréscimo do agente comprado pela decisão numerada |
| `SiteLocalizationOptions`, não `LocalizationOptions` | agente, aceito | o framework já é dono do nome |
| Páginas públicas filtram `IsActive`; produto escondido dá 404 igual a inexistente; tradução ausente cai para `en-US` | Claude Web (lacuna da spec) | não vazar o catálogo do que não está à venda; nunca 404 por falta de tradução |
| Chave de recurso estável (`Nav_Rentals`), não o texto inglês | agente | corrigir uma vírgula no inglês não pode derrubar a tradução |
| `nome-exato` no lugar de `presenca` para existência de arquivo | agente | o Windows ignora caixa; o Linux do App Service não |

## 3. Nove correções do plano, e onde cada uma acabou

| # | Correção | Onde está `[V, arquivo aberto]` |
|---|---|---|
| 1 | design-time factory lê user-secrets/ambiente, cai para `UseSqlServer()` só sem chave | `Infrastructure/Data/AppDbContextFactory.cs` |
| 2 | fail-fast da connection string contornável pelo host de teste | `Program.cs:32-40`; `tests/SiteFactory.cs:45-66` |
| 3 | C05 cobre `DateTimeOffset`; C06 nomeia o único arquivo que lê `UtcNow` | `Docs/controles/foundation.tsv` |
| 4 | C11 discriminante por caminho | idem |
| 5 | nenhum comentário em `src/` transcreve o que C05/C09/C17 procuram | os três medem 0 com 111 arquivos |
| 6 | ordem do middleware escrita com motivo | `Program.cs:124-161` |
| 7 | link para inglês sobrescreve o valor ambiente | `Pages/Shared/_Layout.cshtml:86`; `Pages/CultureLink.cs` |
| 8 | `dotnet dev-certs https --check` | já era confiável; medido no passo 0 |
| 9 | `IsActive` e fallback `en-US` | `Infrastructure/Data/CatalogQueries.cs`; `Application/Catalog/TranslationPicker.cs` |

Mais duas pedidas em P1 e feitas: asserção de alcance em
`SiteBehaviourTests.Nothing_in_this_release_can_send_a_message_to_anybody` (`RegisteredServiceNames`
não vazio) e em `RenderedTextTests.No_page_prints_a_resource_key` (≥ 20 chaves lidas).

## 4. Achados que contrariaram suposição

Tabela sintoma → causa → como se descobriu. São os itens que uma leva futura vai reencontrar.

| Sintoma | Causa | Como se descobriu |
|---|---|---|
| Toda página 200, título do catálogo literalmente `Rentals_Title` | `AssemblyName` `OrlandoUp.Web` ≠ `RootNamespace` `OrlandoUp`; o localizador monta o nome do recurso pelo assembly | a asserção do §7.3 exigia uma palavra portuguesa real; o teste de paridade dos `.resx` **não pega** (os dois arquivos estavam perfeitos). Corrigido com `[assembly: RootNamespace("OrlandoUp")]` + `RenderedTextTests` (145 chaves × 14 endereços) |
| Seis links de `/pt/rentals` escapavam para o inglês | valor ambiente de rota viaja para a **mesma** página, não para outra — a aposta do plano §5.3 estava errada | o teste §7.3 reprovou; todo link interno passou a declarar a cultura (`CultureLink`) |
| Dois provedores no mesmo `DbContext` no host de teste | remover `DbContextOptions<AppDbContext>` não basta neste EF Core; o provedor chega como registro de configuração de opções | `Only a single database provider can be registered`; a remoção varre os descritores por tipo |
| `dotnet ef migrations add` queria connection string | sem factory, o EF passa pelo `Program.cs` e bate no fail-fast | previsto na revisão do plano (correção 1); `UseSqlServer()` sem argumento gera migration sem conectar |
| `sqllocaldb stop` não derruba o `/healthz` | a instância tem `Auto-create: Yes` e a primeira conexão religa | o 503 do item 8 foi provado com `ALTER DATABASE … SET OFFLINE WITH ROLLBACK IMMEDIATE`, desfeito com `SET ONLINE` |
| `dotnet new sln` cria `.slnx` | padrão do SDK 10 | hook e controles nomeiam `OrlandoUp.sln`; `--format sln` (agora no `CLAUDE.md`) |
| BOM em 12 arquivos gerados, e de novo no `.sln` entre P1 e P3 | ferramentas do SDK escrevem UTF-8 com BOM; o hook recusa | varredura antes de cada commit (agora no `CLAUDE.md`) |
| `seed-admin` recusou a conta | a senha tinha 11 caracteres; o Identity exige 12 (D8/01) | o guarda funcionou como o teste afirma; senha regravada, conta criada em E6 |
| Sessão do Claude Code caiu depois de P1 sem receber a resposta | 33 min de sessão + `dotnet test` em segundo plano → "low on memory" + "Can't reach the API" | o disco mostrou HEAD parado em `4ba7da5`; a instrução foi reemitida autossuficiente para sessão nova (§8) |

## 5. Números da spec que envelheceram (registrados na nota, não corrigidos no corpo)

`Docs/decisions.md` mede **26** pelo comando da spec (`grep -c '^\*\*D[0-9]'`), não 24 — D25 e sua
nota `[V]`. `git ls-files` em `bfbe6f3` era 13; em `f8933c6` é **134** `[V]`. A regra continua: o
corpo da spec não é reescrito; a nota datada vence.

## 6. Pendências

**Fechadas pelo Rod em 2026-09-05, depois do resumo (capturas de tela na conversa):**
- **Item 7 da conferência — conferido:** painel `/admin` com **7 / 7 / 10** e a faixa "The catalog
  still carries placeholder data" `[V, captura do Rod]`. Os dez itens da §8 estão fechados.
- **CI verde no GitHub — conferido:** runs `ci #1` (`f8933c6`) e `ci #2` (`8850d97`) verdes em
  *Actions* `[V, captura do Rod]`. A fase 1 do roadmap está completa em todos os "done means".

**Validação humana (Rod), ainda aberta:**
- **Direção de design** (A, B, C ou mistura) — continua sem resposta explícita; a leva 01 executou
  os tokens da direção A por D7/01.
- **Q1, Q2, Q9, Q10, Q11** em `Docs/open-questions.md` destravam a leva 02 (frota, preços, dados
  da empresa, marca, fotos).

**Backlog consciente** (`Docs/backlog-conhecido.md`, inalterado): espanhol, caução por hold,
WhatsApp API, redirecionar `ronatrip.com/scooters`, postagem social, apps MAUI, divisão em
projetos. **Novo candidato**, não registrado lá: controle permanente de BOM/UTF-16 sobre a árvore
(hoje só o hook pega, e só no staged).

**Herdadas da conversa 1 e fechadas aqui:** SDK 10 instalado (10.0.400 `[V, plano]`), `dotnet-ef`
10.0.11, LocalDB presente, repositório no GitHub criado e empurrado.

## 7. Decisões permanentes

Nenhuma decisão numerada nova em `Docs/decisions.md` (D1–D25 inalterados; §9.1 da spec proibia
o agente e a conversa não precisou). Valem para toda leva seguinte e estão em nota na spec / no
`CLAUDE.md`: `--format sln`; strip de BOM antes de staged; `[assembly: RootNamespace]` quando
`AssemblyName` ≠ `RootNamespace`; todo link interno declara a cultura; `nome-exato` para
existência de arquivo; token `--color-action-text` (`architecture.md` §12).

## 8. Notas de processo

O que funcionou e deve ser repetido de propósito: revisar plano e relatórios **abrindo o arquivo**
— a revisão do plano pegou a factory que quebraria o `database update` e o fail-fast que mataria o
CI; a revisão de P1 pegou duas asserções vacuosas. Cada decisão do Rod virou nota datada commitada
**antes** de o agente seguir, e o agente respondeu às correções **por número, com arquivo e linha**.
Quando a sessão do agente caiu, a instrução seguinte foi reescrita **autossuficiente** ("leia
CLAUDE.md, a fila, a nota da spec e o relatório N; você está em P-N") e funcionou numa sessão nova —
os relatórios commitados são exatamente o que permite isso. O que custou está em
`Docs/atrito-conversa-2.md`.

## 9. Próximas frentes candidatas

1. **Leva 02 — site público com dados reais** (fase 2 do roadmap): precisa de Q1/Q2/Q9/Q11 e da
   direção de design; sem elas a leva só troca placeholder por placeholder.
2. **Ajustes reversíveis antes da leva 02** (sem spec, um commit cada): controle de BOM na árvore;
   `Docs/backlog-conhecido.md` ganhando o item acima.
3. **Leva 03 — reserva e pagamento** só depois da 02 e das respostas Q3–Q6 (roadmap).

## 9a. Emenda pós-fechamento (2026-09-05) — as respostas do Rod

Depois do resumo, o Rod respondeu na própria conversa às cinco perguntas que destravam a leva 02.
Viraram **D26–D30** em `Docs/decisions.md` e fecharam Q1, Q2, Q9, Q10 e Q11 em
`Docs/open-questions.md`. Em uma linha cada: frota real = ≈4 scooters Drive Medical + ≈4 Drive
Spitfire + 2 cadeiras Drive, carrinhos ainda por comprar (D26); preços = as faixas do seed da leva
01 vão ao ar, a prática atual (US$ 175/semana + 20/dia + 30 de entrega) fica registrada e a taxa de
entrega se decide com Q3 (D27); empresa = Ronatrip Tours & Travel, 7362 Futures Dr Ste 2, Orlando
FL 32819, WhatsApp a confirmar (D28); design = direção C retrabalhada em canvas moderno com a skill
de design antes da leva 02 (D29); imagens = geradas por IA como na Ronatrip, passando pela skill
`preparo-imagem-site` (D30). **O que ainda falta para a leva 02:** nomes exatos dos modelos e
contagem por modelo (etiqueta das unidades), o número do WhatsApp, e a aprovação do canvas novo.
O item 2 da abertura abaixo fica **substituído** por esses três.

## 10. Abertura da próxima conversa

Na ordem. Cada item nomeia quem executa.

1. **Você — ação (5 min):** abrir `https://localhost:7420/admin/login` (se o site não estiver
   rodando: `dotnet run --project src/OrlandoUp.Web` na pasta), entrar com o e-mail e a senha das
   user-secrets, conferir **7 / 7 / 10** e a faixa amarela de placeholder. Depois abrir
   `https://github.com/xxsaturdayxx/orlando-up/actions` e ver se o primeiro run está verde.
2. **Você — decisão (uma linha cada, a qualquer momento antes da conversa 3):** direção de design
   (A / B / C / mistura); e as respostas de Q1, Q2, Q9, Q11 de `Docs/open-questions.md` — pode ser
   em texto corrido, o Claude Web organiza.
3. **Cole no Claude Web** (conversa 3, com a pasta `Orlando-UP` conectada e permissão de exclusão
   concedida na abertura):
   ```
   Orlando-UP, conversa 3. Rode git log --oneline -15 e git status --short; leia
   Docs/resumo-conversa-2.md e Docs/open-questions.md. Minhas respostas: [direção de design,
   Q1, Q2, Q9, Q11 aqui, em texto corrido]. Escreva a spec da leva 02 e a linha da fila.
   ```
4. **Cole no Claude Code** — só depois de a spec 02 estar commitada pela conversa 3, não antes:
   ```
   Leia CLAUDE.md e Docs/fila-cc.md e execute a linha `aguardando` cuja descrição começa com
   "LEVA 02". Faça o passo 0 da spec e escreva o plano em scratchpad/leva02/plano.md. PARE
   para revisão antes de alterar qualquer arquivo.
   ```

---

*O Orlando Up deixou de ser documento: roda em `localhost:7420` em inglês e português a partir do
banco, com login de equipe, 63 testes e 18 controles verdes, tudo empurrado para o GitHub. Os
números que aparecem são placeholders declarados. A conversa 3 começa pelas respostas do Rod sobre
frota, preços e empresa — e escreve a spec da leva 02 em cima delas.*
