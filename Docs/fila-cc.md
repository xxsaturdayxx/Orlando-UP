# Fila de instruções — Claude Code

**Este arquivo é a fila viva.** Uma linha por instrução recebida do operador; a mais recente no
topo. Quando a fila passar de ~40 linhas, as concluídas migram verbatim para
`Docs/historico-fila-cc.md` (mesma regra do projeto irmão `ronatrip-website`: migrar não é
reescrever).

**Uma linha concluída nunca é reescrita** — o texto é permanente. A única célula que muda é o
**Estado** (e o **Commit**) da própria linha em andamento: `aguardando` → `concluido` com o hash
curto do commit de conteúdo, ou vazio se a instrução não gerou commit — nunca um hash chutado.
`cancelado` carrega o motivo em uma frase na descrição de uma linha nova.

**Instrução que emenda uma linha `aguardando` nasce linha nova**, nunca reescreve a emendada, e
declara qual das duas vence onde se contradizem.

**Nenhuma célula contém o caractere de barra vertical** — é o separador de coluna. Controle que
precisaria dele é expresso por coluna ("a coluna Commit da linha lê `bfbe6f3`").

**Backlog nunca entra aqui** — vai para `Docs/backlog-conhecido.md`. A fila é só instrução
recebida do operador.

**A frase de lançamento é constante:** *"leia `Docs/fila-cc.md` e execute a linha `aguardando`
cuja descrição começa com …"*. O agente lê a linha, abre a spec que ela aponta, mede o passo 0,
escreve o plano em `scratchpad/<leva>/plano.md` (não commitado) e **só começa a alterar arquivo
depois de o operador revisar o plano.**

| Data | Descrição | Estado | Commit |
|---|---|---|---|
| 2026-09-04 | LEVA 01 — APPLICATION FOUNDATION: executar `Docs/spec-01-foundation.md` INTEIRA, escrita pelo Claude Web em 04/09 na conversa 1. **Executor: Claude Code, modelo mais forte** — a leva cria o schema, o Identity e a localização que toda leva seguinte herda. **ESTA É A ÚNICA LINHA `aguardando` DESTA FILA.** **O QUE A LEVA FECHA:** o repositório tem 13 arquivos rastreados e nenhum código (`git ls-files` em `bfbe6f3`). **ESTADO DA ÁRVORE AO RECEBER, medido em 04/09:** HEAD é **descendente de `bfbe6f3`** (o commit de fechamento da conversa 1 vem depois dele e é o HEAD esperado); `git status --porcelain` **vazio** — tudo o que o Claude Web escreveu está commitado; remoto pode não existir ainda. Aviso de CRLF do Windows e índice desatualizado não são divergência; qualquer outro arquivo modificado ou não rastreado é parada com relato. **ARQUIVOS JÁ ESCRITOS PELO CLAUDE WEB (não alterar):** `CLAUDE.md`, `README.md`, `.gitattributes`, `.gitignore`, `.githooks/pre-commit`, `Docs/decisions.md`, `Docs/architecture.md`, `Docs/roadmap.md`, `Docs/market-notes.md`, `Docs/open-questions.md`, `Docs/backlog-conhecido.md`, `Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`, `Docs/spec-01-foundation.md`, `Docs/resumo-conversa-1.md`, `Docs/atrito-conversa-1.md`, e esta fila. **ARQUIVOS QUE A FRENTE PODE ALTERAR:** a lista fechada é a §9.1 da spec — `OrlandoUp.sln`, tudo sob `src/OrlandoUp.Web/` e `tests/OrlandoUp.Tests/`, `.github/workflows/ci.yml`, `Docs/controles/foundation.tsv`, `Docs/conferencia-leva-01.md`, `Docs/relatorio-leva-01-etapa-N.md`; e, só no que a §9.1 diz, `README.md`, `.gitignore` e as colunas Estado e Commit desta linha. Arquivo fora disso é parada, **sem cardinal**. **ONDE O PLANO NASCE:** `scratchpad/leva01/plano.md`, sem commit; a execução só começa depois da revisão do operador. **ONDE OS RELATOS DE PARADA NASCEM:** `Docs/relatorio-leva-01-etapa-N.md`, commitados antes de pedir aprovação. **PASSO 0 (spec §9.3):** `dotnet --list-sdks`, `dotnet ef --version`, `sqllocaldb info`; grep do radical `OrlandoUp` fora de `Docs/`, `README.md` e `CLAUDE.md` esperado 0; `ls Docs/controles` vazio (nenhum controle alheio a deslocar); proposta de `Docs/controles/foundation.tsv` com `medir` rodado no HEAD inicial; contradição entre spec e árvore vai no plano. **BANCO:** só o LocalDB `OrlandoUpDb` existe; a migration `InitialCreate` e os comandos `seed-catalog` e `seed-admin` rodam nele; não há outro banco e nenhuma connection string entra em arquivo commitado. **SEGREDOS:** os dois user-secrets (`ConnectionStrings:DefaultConnection`, `AdminSeed:Email` e `AdminSeed:Password`) são do operador — o agente imprime os comandos só com os nomes das chaves e espera. **PUSH: do operador, não do agente** — o remoto e as credenciais são dele e um commit empurrado não pode mais ser reescrito (o hash pode já estar gravado em artefato). **CONTROLES NEGATIVOS POR COLUNA DO DIFF:** os arquivos "já escritos pelo Claude Web" acima aparecem em zero linhas do `git diff --stat` da faixa de commits da leva, exceto `README.md` (seção "Running locally") e esta fila (colunas Estado e Commit desta linha). **ASSERÇÃO EM SALDO:** ao receber, esta fila tem **1** linha `aguardando`; ao fim, **0** `aguardando` e esta linha `concluido` com o hash do commit de conteúdo; o total de linhas da tabela **não muda** (nenhuma linha nova nasce nesta leva). **FECHAMENTO EM DOIS COMMITS:** o de conteúdo (código, testes, `foundation.tsv`, `conferencia-leva-01.md`, README) primeiro; depois o de fechamento, que grava o hash do primeiro na coluna Commit desta linha. **FIM DE SESSÃO:** `git status --short` inteiro e `git diff --stat`; `bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv` relatado, nunca silenciado. | aguardando | |
