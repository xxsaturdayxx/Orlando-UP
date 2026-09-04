# Protocolo de conversa — Orlando Up

**Escopo:** governa o Claude Web (Cowork) e o operador. Não governa o Claude Code — regra de
agente vai para `CLAUDE.md`.

Vale, por inteiro, o `Docs/protocolo-conversa.md` do repositório `ronatrip-website` (o irmão em
`C:\Users\danil\source\repos\ronatrip-website`), seções 1 a 4: estilo de resposta (conciso, sem
preâmbulo, registro denso no ARQUIVO e decisão na conversa), autonomia (decidir, executar,
informar; consultar antes só quando reverter custa: dado real, dinheiro, arquitetura herdada por
outras frentes), modelo por tarefa, e proveniência `[V]`/`[H]`.

O que muda aqui:

1. **Língua segue o leitor** (`Docs/decisions.md` D1): a conversa é em português; código, UI
   padrão, commits e documentos de engenharia em inglês; fila, resumos, atrito, backlog e
   controles em português.
2. **Não há multi-tenant, agência nem comissão.** Nada do glossário de domínio da Ronatrip se
   aplica; o glossário deste projeto é a tabela de entidades em `Docs/architecture.md` §3.
3. **Ambiente:** local, staging e produção são três (D14). Antes da fase 5 não existe produção;
   nenhum comando de dado roda fora do LocalDB sem `SELECT DB_NAME()` colado antes.
4. **Numeração:** conversa N ↔ leva M são séries diferentes. A conversa 1 (2026-09-04) escreveu a
   spec da leva 01. O par é declarado no cabeçalho de cada resumo.
5. **Abertura de conversa:** `git log --oneline -15` procurando a última linha
   `docs: resumo da conversa N`; trabalho acima dela sem resumo é conversa aberta (skill
   `resumo-frente-projeto`). Depois, `git status --short` e `Docs/fila-cc.md` inteira.
