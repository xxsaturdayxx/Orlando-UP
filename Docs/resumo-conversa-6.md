# Resumo — conversa 6: o sistema aprende a prometer (leva 03, núcleo da reserva)

**Data:** 2026-09-12 a 15. **Par:** conversa 6 ↔ **leva 03**. **Continua** o
`Docs/resumo-conversa-5.md` (leva 04b, baterias e carregadores).

**Faixa de commits:** `6da3ff6..` — spec `7d0384b`, EMENDA-03-01 `70d6d68`, P1 `fb2439c` + revisão
`14e80e4`, P2 `0755e57` + revisão `1f504fe`, correções `3994f1a`, P3 `84dea21` + revisão `3359f89`,
correção final `05df7d2` *(verificado em `.git/logs/HEAD` lido pela ponte)*. **Os dois commits do
ritual — conteúdo (conferência preenchida + a correção do `{0}`) e fechamento (hash na fila) — vêm
DEPOIS deste resumo, pelo Claude Code, porque a máquina virtual da ponte parou de montar a pasta em
15/09 e o revisor ficou sem `git`.** A coluna Commit da linha "LEVA 03 — BOOKING CORE" de
`Docs/fila-cc.md` é a autoridade sobre o hash de conteúdo; este documento não o conhece.

**A conversa 5 fechou.** `git log` mostrou `6da3ff6 docs: resumo da conversa 5…` como HEAD com
árvore limpa na abertura *(verificado em 12/09)*.

---

## 1. O que a leva 03 fechou, e por que ela é meia fase

O sistema conhecia a frota inteira e **não podia prometer nada dela**. Agora: quatro tabelas
(`Bookings`, `BookingLines`, `BookingAddOns`, `BookingEvents`), uma regra de disponibilidade que
conta **unidades E baterias E carregadores por dia** com um dia de intervalo, uma cotação congelada
na reserva, a página pública `/book` (*"Ver disponibilidade e preço"*, no lugar do botão cinza), e
a reserva **lançada pela equipe** em `/admin/bookings` — que é como a reserva por WhatsApp entra
hoje. **286 testes** *(relato do agente na P3, herdado — o shell do revisor não tem `dotnet`)*.

Ela é meia fase por decisão (D39): Stripe, formulário público de reserva, e-mail, hold e reembolso
são a **leva 03b**, que trava na conta Stripe (Q6) e no texto de responsabilidade por bateria
(Q15). Nada da 03 esperava por isso; tudo da 03b espera.

**A regra que a leva carrega, em uma frase:** a disponibilidade é a peça mais escassa da
combinação. Com 4 Scouts e 6 baterias por modelo, o quarto cliente que quer segunda bateria é
recusado **pela bateria, não pela máquina** — e a conferência mediu exatamente isso (itens 8–10).

---

## 2. Decisões — D39 a D42, e as quatorze da spec

| Decisão | O que fixou | Por quê / cena |
|---|---|---|
| **D39** [operador] | fase 3 dividida: 03 núcleo, 03b pagamento | nada da 03 depende de Q6/Q15; precedente 04/04b |
| **D40** [operador] | turnaround **1 dia**, máquina e bateria | *SCT-02 volta terça 20h; quarta 9h outro cliente* → "precisa de um dia de folga" |
| **D41** [operador] | janelas fixas 8–10/10–12/14–16/18–20 e corte **18h** Orlando (coluna `NextDayCutoffHour`, editável) | *sexta 22h para sábado 9h* → recusado; corte só no público |
| **D42** [operador] | disponibilidade é **barreira** no site e **aviso** para o staff (`IsOverbooked`, caixa "Lançar acima da frota") | *quatro Scouts tomadas, quinto cliente* → "site bloqueia, staff pode passar" |

Da spec (D1–D14/03), as que valem além da leva: número `OU-000123` derivado do Id (SQLite de teste
não tem sequence — D6/03); **tudo na reserva é instantâneo** e só a cotação computa (D7/03);
baterias e carregadores são **números** na linha, a atribuição física é da frente seguinte (D8/03);
linha de configuração ausente **estoura**, nunca vira zero (D12/03); relógio falso em `tests/` por
delegação ao `SystemClock(Func)` (D13/03, forma escolhida pelo agente); 1–60 dias (D14/03).

**A Q3 ficou parcialmente respondida** (janelas e corte); faltam zonas, taxas e a rotina do
meet-and-greet. **Q15 e Q6 continuam abertas** e travam só a 03b.

---

## 3. As duas emendas e as três revisões — o que cada uma pegou

**EMENDA-03-01 (plano P0).** O plano do agente era o melhor da série: mediu tudo, achou dois defeitos
da própria spec antes de existirem (um operando que casaria a coluna-instantâneo; um `ERRO` que o
C07 gravaria como esperado) e propôs a fábrica no domínio. A revisão acrescentou o que ele não
listou: **o default constraint cai no mesmo `Up`** (a D35 já sabia que sobra) e **`Booking.Status`
com `private set`** — barreira de compilador, não disciplina.

**Revisão da P1.** Migration puramente aditiva, lida no SQL. Os três fatos `[H]` da spec caíram:
**13 baterias** (a `BSC-22` baixada — pool disponível continua 12), `LostChargerFee` = 32, e o
**carrinho simples reservável com preço e zero unidades** — edição do operador que a tela da leva
04 aceitou sem avisar (backlog). O revisor errou a contagem de índices (11/8; eram 10/7) e o agente
corrigiu na P2.

**Revisão da P2 — a que mais valeu.** Duas correções que os 251 testes verdes não viam:
1. **as linhas do MESMO pedido não se contavam** — Scout + Spitfire, cada uma com segunda bateria,
   conferidas uma a uma contra o diário, passavam juntas acima do limite de carregadores **sem o
   selo**. Saída: as outras linhas do pedido entram como `HoldingLine` na conferência de cada uma;
2. **ninguém abaixo da tela validava a linha** (quantidade zero, extras acima da quantidade,
   segunda bateria em cadeira de rodas — a cotação cobrava). Saída: três `QuoteProblem` no domínio;
   as telas traduzem, nunca revalidam.

**Revisão da P3.** Portão verde nos cinco `.tsv`, superfície do diff inteira na §11.1, catorze
frases do roteiro batendo com o `.resx`. Uma correção: **o controle moldou o código** — o agente
tinha tirado os dois `Record` do writer e posto nos handlers, para manter o C05 do `admin-catalog`
em 2; o evento `Created` passou a nascer **fora** da transação e o invariante foi morar na página,
onde a 03b (sem handler de admin) o esqueceria. Voltou para o writer; o C05 foi **emendado com o
irmão** C16 (`_timeline.Record(` no writer = 2) e o alcance C17.

**EMENDA-03-02** registrou o que veio da execução: a cena "segunda bateria indisponível" da §9.3
era **3 + 2, não 3 + 3** (3 + 3 esgota o pool); a cena do carregador é **inalcançável com a frota de
hoje** (dois pools de 6 limitam a 12 de 14 — os testes compram um estoque menor); e as correções da
P2 e da P3.

---

## 4. A descoberta de método desta conversa

**Um controle permanente que resiste ao desenho certo é emendado com o irmão que mede o que ele
deixou de medir — nunca contornado pelo código.** É o inverso do "rótulo nomeia classe, operando
conta membro" das conversas 4 e 5: lá o controle media menos do que prometia; aqui o controle media
certo e **o código se dobrou para caber nele**. Os dois têm a mesma cura: emendar a linha com o
irmão, e escrever no `.tsv` por que o irmão existe.

**A segunda:** fato `[H]` na spec é o que o agente reconfere primeiro, e três de três estavam
errados nesta leva. O carimbo funciona; o que não funciona é spec sem carimbo.

**A terceira, do roteiro:** o `ok` do item 5 passou por cima de um `{0}` cru — a célula tinha dez
expectativas. **Uma expectativa de texto por linha** (`Docs/atrito-conversa-6.md`).

---

## 5. A conferência visual

**27 itens, 27 `ok`, nenhum `não fiz`**, respondidos em uma rodada, com onze prints. Um defeito que
o roteiro pedia e o `ok` não viu: **"Reserva {0} criada."** — `Create` grava `FlashArgument`,
`Details` não lê. Corrigido no commit de conteúdo (uma linha e um teste de presença do número).

**Cinco achados de produto** (A1–A5, transcritos no `Docs/conferencia-leva-03.md` e no backlog):
hotel fora da lista precisa entrar por endereço (A1); campos de texto maiores (A2); alinhamento dos
blocos (A3); **editar a lista de hotéis** (A4); tabela de equipamentos mal apresentada a 375 px
(A5). Nenhum é defeito da leva; são a próxima frente.

---

## 6. Estado do portão

**Cinco arquivos, 69 controles** *(verificado pelo revisor em `84dea21`, antes da correção final;
`booking-core` 15 + 2 depois dela → 71)*:

| Arquivo | Controles | Estado |
|---|---|---|
| `foundation.tsv` | 18 | 16 no alvo; C14/C15 herdados verdes do agente |
| `public-site.tsv` | 17 | no alvo (C10 lê 24 e 24) |
| `admin-catalog.tsv` | 12 | no alvo; **C05 emendado** (conta também a escrita pelo writer) |
| `fleet-batteries.tsv` | 7 | no alvo; C06 rebaseado 5 → 6 (`IsOverbooked`, que não é bandeira de visibilidade — o plano diz isso nessas palavras) |
| `booking-core.tsv` | 17 | novo; sete irmãos de alcance foram de `nao` a `sim` |

---

## 7. Processo — a ponte caiu

A ponte de arquivos reconectou duas vezes (locks do `git` limpos à mão, permissão de exclusão
pedida de novo) e em 15/09 a máquina virtual **parou de montar a pasta** (atualização do Windows de
8/9, segundo a ferramenta). Leitura e escrita seguiram por `stage`/`commit` de arquivo; **`git`
não**. Por isso o fechamento desta conversa — conferência, correção do `{0}`, resumo, atrito,
backlog — sai pelo Claude Code, na sessão já aberta. Não é regra nova; é contingência registrada.

---

## Decisões permanentes

Registradas em `Docs/decisions.md`: **D39, D40, D41, D42**. Na spec e nas emendas: D1–D14/03.

Regras de método que valem além desta frente e ainda **não** estão em documento do repositório
(candidatas a `Docs/regras-de-controle.md` ou à skill `revisao-plano-agente`):

1. **Controle permanente que resiste ao desenho certo se emenda com o irmão, nunca se contorna pelo
   código.** (§4)
2. **Conferência de disponibilidade soma as linhas do próprio pedido**, não só o diário. (P2)
3. **Invariante de forma de linha mora no domínio, não na tela** — a segunda tela o esquece. (P2)
4. **Evento de escrita nasce na transação da escrita.** (P3)
5. **Uma expectativa de texto por linha do roteiro.** (§5)

---

## Pendências

**Do Rod — ação:**
- `git push` depois dos commits de fechamento;
- imprimir e trocar as etiquetas físicas das baterias (`BSC-`/`BSP-`) e aplicar `SCT-`/`SPT-`/`WCH-`
  às dez unidades pela tela *(herdado do resumo 5, não reconferido)*.

**Do Rod — decisão (travam a 03b):** **Q15** (dano/perda de bateria; existiu Spitfire XL?) e **Q6**
(conta Stripe). Da Q3, o que falta: zonas, taxas por zona, rotina do meet-and-greet — e a **taxa da
zona "outro hotel"** que o A1 vai precisar.

**Backlog consciente** (`Docs/backlog-conhecido.md`): produto reservável sem unidade não avisa;
unidade/bateria posta em manutenção depois de reserva futura não avisa; conferência e `INSERT` não
atômicos (obrigatório resolver no fluxo público da 03b); A1–A5; a rodada de estética (herdada); o
leitor de QR (D38, depois da 03b).

---

## Próximas frentes candidatas

1. **Leva 04c — zonas e locais: CRUD no `/admin` e "outro hotel (endereço)" nas duas telas** (A1 +
   A4). Sem migration: as tabelas existem desde a leva 01. Precisa de uma decisão do Rod: a taxa da
   zona "outro" (Q3). **Junto, como ajustes sem fila:** A2, A3, A5.
2. **Leva 03b — Stripe, formulário público, e-mail, hold.** Travada por Q6 e Q15.
3. **Rodada de estética** (herdada da conversa 4).

---

## Abertura da próxima conversa

1. **Cole no Claude Code** (fecha a leva 03 — três commits, nesta ordem):
   ```
   leia Docs/conferencia-leva-03.md inteiro, seção Resultado. Corrija o defeito do {0} (Details lê TempData["FlashArgument"] e formata; teste de presença do número no role="status"). Commit de conteúdo: conferência preenchida + correção. Commit de fechamento: coluna Commit da linha "LEVA 03 — BOOKING CORE" recebe o hash do commit de conteúdo, Estado concluido. Depois, commit só com Docs/resumo-conversa-6.md, Docs/atrito-conversa-6.md e Docs/backlog-conhecido.md, mensagem exata: docs: resumo da conversa 6
   ```
2. **Você — ação:** empurrar.
   ```
   git push
   ```
3. **Você — decisão:** a taxa de entrega para "outro hotel (endereço)" — zero como os hotéis da
   lista, ou 25 como casa de temporada? É o que a leva 04c precisa e a Q3 não tem.
4. **Cole no Claude Web (conversa 7):**
   ```
   Conversa 7 — abrir a partir de Docs/resumo-conversa-6.md. A leva 03 fechou (hash na fila). Confira se a conversa 6 fechou, decida a frente (candidata: leva 04c — zonas e locais + "outro hotel" — com os ajustes A2/A3/A5) e escreva a spec.
   ```
5. **Depois da spec revisada, e não antes — cole no Claude Code:** a linha nova de `Docs/fila-cc.md`,
   apontada **pela descrição**.

---

*O sistema agora promete: conta a peça mais escassa, congela o preço e deixa a equipe lançar acima
da frota sabendo que lançou. O que ele ainda não faz é receber dinheiro — e o que trava isso
continua não sendo código: é a Q6 e a Q15. A conversa 7 abre nas zonas e nos hotéis fora da lista.*
