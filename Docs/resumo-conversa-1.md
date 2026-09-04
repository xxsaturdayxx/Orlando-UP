# Fundação do repositório Orlando Up — resumo da conversa 1

**Data:** 2026-09-04. **Par:** conversa 1 ↔ leva 01 (spec escrita e enfileirada, **não executada**).
**Contexto:** primeira conversa do projeto; não havia repositório, memória nem documento anterior.
A pasta `C:\Users\danil\source\repos\Orlando-UP` estava vazia `[V, device_list_dir 04/09]`.

Proveniência: `[V]` verificado nesta conversa (como, entre parênteses); `[H]` herdado sem
reconferência — pendência, não fato.

---

## 1. O que o operador decidiu

Quatro perguntas, uma rodada, 04/09:

| Decisão | Resposta | Onde está registrada |
|---|---|---|
| Frota | Própria, operada pela Ronatrip — o site controla estoque por unidade e agenda de entrega | `Docs/decisions.md` D2 |
| Entidade | Ronatrip por enquanto, com intenção de LLC própria — tudo que nomeia a empresa vira configuração | D3 |
| Pagamento | Stripe, pagamento integral na reserva, Checkout hospedado | D7 |
| Referência | Leitura (só leitura) do repositório irmão `ronatrip-website` autorizada | §4 abaixo |

Tudo o mais foi decidido pelo Claude Web sob a autonomia pedida no briefing ("bastante autonomia
de decisão, em especial no início"), com o motivo escrito em `Docs/decisions.md` D1–D24, cada uma
marcada **[operator]** ou **[assistant]** — é ali que se discorda em uma linha.

## 2. Arquitetura e stack

.NET 10 LTS · ASP.NET Core Razor Pages (site e `/admin`) · Minimal APIs em `/api/v1` para o
futuro app · EF Core 10 + SQL Server (LocalDB local, Azure SQL na nuvem) · Identity só para
equipe · Stripe Checkout · Brevo SMTP atrás de `IEmailSender` · Azure App Service Linux em resource
group próprio · GitHub Actions. Um projeto web `src/OrlandoUp.Web` com camadas por pasta
(`Domain/`, `Application/`, `Infrastructure/`, `Pages/`, `Api/`) mais `tests/OrlandoUp.Tests` com
teste de arquitetura. Modelo de domínio, máquina de estados da reserva, cálculo de disponibilidade
e de preço, localização por prefixo de rota (`/` inglês, `/pt/` português, cultura de formatação
fixa em `en-US`), fluxo de pagamento por webhook idempotente, três ambientes — tudo em
`Docs/architecture.md`.

Duas escolhas que outras frentes herdam e por isso merecem o veto explícito do operador:
**Razor Pages em vez de Blazor** (D10 — o app nativo fica para uma fase com portão de decisão, D17)
e **um projeto web em vez de quatro** (D11 — a divisão fica possível sem renomear namespaces).

## 3. Mercado e regras dos parques

`Docs/market-notes.md`, tudo `[V, web 04/09]`, com fontes. Os três fatos que moldam o produto:
(a) a **ScooterBug é provedora exclusiva da Disney** — só ela deixa equipamento no Bell Services;
todo outro fornecedor faz meet-and-greet presencial, logo a entrega é uma **janela agendada**,
não um drop-off; (b) **30 × 48 in** é o limite que importa para ônibus e Skyliner da Disney e para
a Universal — vira selo de produto; (c) carrinhos até **31 × 52 in**, wagons proibidos. Preços de
concorrentes: 7 dias de scooter entre US$ 150 e US$ 245; escalões por peso do passageiro e
diária menor a partir de 3 dias são o padrão. Ninguém atende brasileiros em português com equipe
local — é a abertura do Orlando Up. A página atual `ronatrip.com/scooters` é um formulário de lead
sem preço, sem modelo e sem pagamento `[V, WebFetch 04/09]`.

## 4. O que veio da Ronatrip, e o que não veio

Lido em `ronatrip-website` `[V, device_bash 04/09]`: Razor Pages em `net8.0`, Identity, EF Core
SqlServer 8.0.8, MailKit (Brevo), QuestPDF, Azure Blob, **App Service Windows publicado por Web
Deploy do Visual Studio, sem CI, sem staging** (`CLAUDE.md` da Ronatrip dedica uma seção aos
acidentes disso). Reaproveitado: `Docs/medir-controles.sh` **copiado verbatim** (sem referência à
Ronatrip, `[V, grep]`; autoteste 41 casos, 0 falhas `[V]`), `Docs/regras-de-controle.md` copiado
da skill do operador (md5 `bb68305d…`), o ritual fila → spec → resumo → controles, e quatro
lições viradas decisão: cultura de formatação invariante (D20), data de calendário vs. instante
UTC (D16), sem `Migrate()` no boot e migration aditiva antes do deploy (D12), sem seed de admin
por configuração (D23). **Não copiado:** nenhuma linha de código, o multi-tenant, agência,
comissão, o glossário de domínio.

## 5. Processo neste projeto

`Docs/protocolo-conversa.md` aponta para o protocolo da Ronatrip (seções 1–4) e registra seis
diferenças: língua segue o leitor (D1), sem tenant, três ambientes, numeração conversa ↔ leva,
abertura por `git log`, e **o Claude Web commita os próprios documentos direto pela ponte** — o
que elimina a linha de FECHAMENTO para o agente. Pré-requisito medido: permissão de exclusão na
pasta (o `git` apaga `index.lock`; sem ela o primeiro commit trava — `Docs/atrito-conversa-1.md`).
Mudança reversível é ajuste, não frente (`CLAUDE.md`, "Process"). Hook `pre-commit` com quatro
checagens (segredo, UTF-16/BOM, teto de 200 linhas do `CLAUDE.md`, `dotnet build` quando houver
solution) — instalado por `git config core.hooksPath .githooks`, já feito neste clone `[V]`.

## 6. Design

Canvas publicado em https://claude.ai/code/artifact/83eb9846-8d54-48b3-a360-fde57494319f
(artefato "Orlando Up Website", 04/09): home desktop e home mobile em português na **direção A —
"ensolarada e amigável"** (fundo off-white quente, laranja para ação, azul profundo para
confiança, Nunito, tipo grande e contraste alto de propósito), página de produto com resumo de
reserva fixo, e duas direções alternativas em baixa fidelidade — **B "editorial calma"** (creme,
verde, serifa) e **C "energia de parque"** (navy, amarelo, display pesado). Preços entre
colchetes são placeholders; ilustrações em linha substituem fotos reais. Os tokens da direção A
já estão em `Docs/architecture.md` §12; a spec da leva 01 (D7/01) os consome.

## 7. Estado do repositório

Commit de conteúdo `bfbe6f3` (13 arquivos: `CLAUDE.md`, `README.md`, `.gitattributes`,
`.gitignore`, `.githooks/pre-commit`, `Docs/{decisions,architecture,roadmap,market-notes,
open-questions,backlog-conhecido,protocolo-conversa}.md`, `Docs/medir-controles.sh`) `[V, git log]`.
O commit de fechamento desta conversa acrescenta `Docs/regras-de-controle.md`,
`Docs/spec-01-foundation.md`, `Docs/fila-cc.md` (uma linha `aguardando`), este resumo,
`Docs/atrito-conversa-1.md` e as emendas em `Docs/protocolo-conversa.md` e
`Docs/open-questions.md`. Branch `main`. **Sem remoto** — criar e empurrar é ação do operador (§10).

---

## 8. Decisões permanentes

Todas em `Docs/decisions.md` (D1–D24), já registradas lá. As que valem além desta frente e
merecem repetição: D1 (língua segue o leitor), D3 (empresa como configuração), D9
(acessibilidade WCAG 2.2 AA é requisito), D14 (staging antes de pagamento ao vivo), D15/D16
(dinheiro e tempo), D20/D21 (cultura e URL), D22 (ritual leve), D24 (segredo nunca no repositório).

## 9. Pendências

**Validação humana (operador):**
- Instalar .NET 10 SDK e `dotnet-ef` na máquina; confirmar LocalDB (§10, item 1).
- Criar o repositório no GitHub e empurrar `main` (§10, item 2).
- Escolher a direção de design — A, B, C ou mistura — em uma linha (§10, item 3).
- Responder `Docs/open-questions.md` Q1–Q12 conforme cada fase pedir; **Q1, Q2, Q9, Q10 e Q11
  destravam a leva 02**, Q3–Q6 a leva 03, Q7–Q8 a fase 5. Nenhuma bloqueia a leva 01.

**Backlog consciente:** `Docs/backlog-conhecido.md` (espanhol, caução por hold, WhatsApp API,
redirecionar `ronatrip.com/scooters`, ferramenta de postagem social, apps MAUI, divisão em projetos).

**Herdadas de conversas anteriores:** nenhuma — esta é a primeira.

## 10. Abertura da próxima conversa

Na ordem. Cada item nomeia quem executa.

1. **Você — ação (antes de chamar o agente):** no PowerShell,
   `winget install Microsoft.DotNet.SDK.10` · `dotnet tool update --global dotnet-ef` ·
   `dotnet --list-sdks` (precisa mostrar uma linha `10.0.x`) · `sqllocaldb info` (precisa listar
   `MSSQLLocalDB`).
2. **Você — ação:** criar o repositório vazio `orlando-up` (privado) em
   https://github.com/new e, na pasta `Orlando-UP`:
   `git remote add origin https://github.com/xxsaturdayxx/orlando-up.git` ·
   `git push -u origin main`.
3. **Você — decisão (a qualquer momento antes da leva 02):** direção de design — "A", "B", "C"
   ou "A com fotos no estilo de B", por exemplo. Uma linha basta.
4. **Cole no Claude Code** (depois dos itens 1 e 2, não antes):
   ```
   Leia CLAUDE.md e Docs/fila-cc.md e execute a linha `aguardando` cuja descrição começa com
   "LEVA 01 — APPLICATION FOUNDATION". Faça o passo 0 da spec (§9.3) e escreva o plano em
   scratchpad/leva01/plano.md. PARE para revisão antes de alterar qualquer arquivo.
   ```
5. **Cole no Claude Web** (conversa 2, com a pasta `Orlando-UP` conectada e permissão de
   exclusão concedida na abertura):
   ```
   Orlando-UP, conversa 2. Rode git log --oneline -15 e git status --short; leia
   Docs/resumo-conversa-1.md, Docs/fila-cc.md e scratchpad/leva01/plano.md (se existir) e
   revise o plano do Claude Code contra Docs/spec-01-foundation.md antes de eu liberar a execução.
   ```
   Se o plano ainda não existir, a conversa 2 começa pelo item 4.

## 11. Notas de processo

O que funcionou e deve ser repetido de propósito: pesquisar mercado e ler o repositório irmão
**antes** de perguntar, para que as quatro perguntas viessem com a cena concreta e fossem
respondidas de primeira; decidir com motivo escrito e marcar de quem foi cada decisão; rodar o
hook a seco antes do primeiro commit (pegou o hook reprovando a si mesmo). O que custou:
`Docs/atrito-conversa-1.md` — dois prompts de canal, um evitável.

---

*O projeto tem repositório, decisões numeradas, arquitetura, roadmap, notas de mercado, perguntas
abertas com premissa em vigor, uma direção visual para reagir e a spec da leva 01 enfileirada. A
próxima conversa começa pela revisão do plano que o Claude Code escrever para a leva 01 — depois
de o operador instalar o SDK e empurrar o repositório para o GitHub.*
