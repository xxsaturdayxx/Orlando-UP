# Atritos da frente: conversa 2 — execução da leva 01

Rodadas totais: 8 (abertura e revisão do plano; pedido de instrução clara; P2; P1; "CC parou em
P3" que não era P3; o relato colado do Claude Code; P3; fechamento) — rodadas de transporte: **3**
(as duas do falso P3 e a do pedido de clareza). Três em oito passa de um quinto: o problema não
está em nenhuma rodada, está em **onde a instrução para o agente vive**.

| Atrito | Vezes | Categoria | Solução |
|---|---|---|---|
| A resposta do Claude Web a uma parada (P1) existia só no chat; a sessão do agente caiu antes de recebê-la e o operador, em trânsito, leu "parou" como "terminou" — duas rodadas para descobrir que nada tinha acontecido | 1 (2 rodadas) | **artefato** | A revisão de cada parada nasce como seção **`## Revisão (Claude Web, data)`** no fim do próprio `Docs/relatorio-<leva>-etapa-N.md`, commitada pelo Claude Web. O operador cola no agente uma frase **constante**: *"leia o relatório da etapa N e execute a seção Revisão"*. Sobrevive a queda de sessão, funciona em sessão nova, e o operador não transporta texto |
| O operador pediu que as instruções para ele e para o agente fossem explícitas | 1 | emissão | Toda mensagem de parada fecha com dois blocos rotulados — **Você** (comando pronto) e **Cole no Claude Code** (texto exato) — e nada mais depois deles. Adotado da rodada 2 em diante; zero repetição depois |
| O relatório de P2 (90 mil caracteres, com a migration, o snapshot e o SQL em anexo) estourou o limite de leitura da ferramenta e exigiu leitura em pedaços | 1 | emissão | Anexos gerados por ferramenta vão para `scratchpad/<leva>/` (não commitado) ou para arquivo próprio; o relatório cita caminho e hash. O relatório é para ser lido, o anexo é para ser medido |
| Screenshot do site pelo Chrome falhou duas vezes (renderer sem responder); a conferência visual ficou só em texto | 2 | acesso | Máquina sob pressão de memória (o mesmo motivo da queda do agente). Antes da conferência visual: fechar a sessão do agente e o `dotnet test`; se repetir, o item 4/5 vai para o operador com o roteiro pronto |
| A senha do primeiro admin veio com 11 caracteres e o `seed-admin` recusou — uma rodada a mais em P3 | 1 | emissão | O comando entregue ao operador dizia "12+" em prosa ao lado; o template do comando passa a levar a exigência **dentro** do placeholder (`"SUA-SENHA-COM-12-OU-MAIS"`) — feito na rodada seguinte |
| O que foi produzido hoje e vai precisar de transporte manual amanhã: a resposta do Rod às Q1/Q2/Q9/Q11 e a direção de design existem só na cabeça dele | 1 (preditivo) | artefato | A abertura da conversa 3 pede as respostas **em texto corrido dentro do prompt de abertura**; o Claude Web as converte em decisões numeradas na primeira rodada |

Mudança de maior retorno: **a revisão de cada parada vira seção commitada no relatório da etapa,
e o operador cola uma frase constante no agente.** Elimina as duas rodadas do falso P3, torna a
queda de sessão do agente indolor (a sessão nova lê o repositório) e dispensa o operador de
transportar texto entre as duas pontas — que era o que ele estava fazendo em trânsito, pelo
celular. Custa: uma linha no `Docs/protocolo-conversa.md` (item 7) na conversa 3, e o hábito de o
Claude Web escrever no arquivo antes de escrever no chat.
Fica para depois: o controle permanente de BOM (o hook já pega no staged; o controle na árvore é
ajuste reversível de um commit) e o roteiro de conferência visual para o operador (só se o Chrome
voltar a falhar).
