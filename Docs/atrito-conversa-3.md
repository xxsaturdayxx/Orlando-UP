# Atrito — conversa 3 (leva 02)

**Atritos da frente:** leva 02, site público — do P0 ao fechamento.
**Rodadas totais: 8 — rodadas de transporte: 1** (12%).

A proporção é a melhor das três conversas (`atrito-conversa-2.md` mediu 3 em 8), e a causa é
identificável: o **item 7 do protocolo**, registrado na primeira rodada desta conversa. A revisão
de cada parada passou a ser seção commitada no relatório da etapa, e o que o Rod transportou entre
as duas pontas foi uma frase constante em vez de um texto de revisão inteiro — seis vezes.

| Atrito | Vezes | Categoria | Solução |
|---|---|---|---|
| **`dotnet` não é alcançável pelo shell da ponte** — os controles de build e de suíte devolvem `127` para o revisor, e uma vez isso quase virou relato de portão vermelho | 6 (toda parada) | **acesso** | o agente grava a saída de `medir-controles.sh verificar` num arquivo do repositório a cada parada |
| Mensagem de parada repetida — a P3b chegou duas vezes e a segunda não produziu decisão | 1 | emissão | o revisor confere o estado da árvore antes de rever; feito, e custou uma chamada em vez de uma revisão |
| Arquivo binário que o shell não abre — os quatro `.woff2` precisaram de `device_stage_files` mais `fontTools` no container para responder "este subconjunto tem os acentos?" | 1 | acesso | skill irmã da `preparo-imagem-site` que dumpe `cmap` e `fvar` na origem e emita o `@font-face` com os eixos reais |
| Duas emendas do revisor escritas de raciocínio em vez de medição, e corrigidas pelo agente | 2 | emissão | a Etapa 5 da `revisao-plano-agente` ganha a lista do que se **recalcula** em vez de ler |
| Seis notas de emenda empilhadas no topo de uma spec — quem ler daqui a seis meses aplica seis correções de cabeça antes de chegar ao §0 | — (custo futuro) | artefato | ao fechar a leva, dobrar as emendas no corpo da spec com uma linha de histórico; **não feito, e é deliberado** — a spec é negativo da leva e reescrevê-la agora perderia o rastro de quem corrigiu o quê |

---

## Mudança de maior retorno

**O agente grava, em cada parada, a saída literal de
`bash Docs/medir-controles.sh verificar Docs/controles/*.tsv` num arquivo do repositório** — uma
seção do próprio relatório da etapa serve, ao lado da seção de Revisão que o item 7 já criou.

**Por que essa e não outra:** é o único atrito que apareceu em **todas as seis paradas**, e é o
único que deixa um buraco no produto e não só no processo. Hoje todo número de build e de suíte que
o revisor repassa é `[H]` — lido do relato, nunca conferido — e o revisor não tem como distinguir
"18 verdes" de "18 verdes segundo quem escreveu". Com a saída literal no arquivo, ele lê a
**medição**, não a afirmação sobre a medição; e o `127` do lado dele deixa de poder ser confundido
com vermelho, porque o número do lado certo está gravado.

**Custa:** uma linha no `CLAUDE.md` e um bloco de saída em cada relatório de parada. Nada de
ferramenta nova, nada de canal novo.

---

## Fica para depois

- **A skill de preparo de fonte.** Apareceu uma vez em três conversas; quando aparecer de novo,
  vale escrever. Antes disso é ferramenta procurando problema.
- **Dobrar as emendas no corpo da spec.** Só faz sentido depois que a leva fecha e o rastro já
  cumpriu a função — e o rastro **é** o valor nesta conversa, porque duas emendas foram derrubadas
  por medição e isso precisa continuar legível.
- **Atacar a mensagem de parada repetida.** Uma rodada em oito não paga mudança de fluxo.

---

## Nota de processo: o que funcionou e merece ser repetido de propósito

**O agente parou seis vezes e nenhuma delas foi desperdício.** Duas paradas devolveram correção ao
revisor — a ordem da alternância e o corte de `bin`/`obj` — o que só acontece quando o agente mede
em vez de obedecer. **Uma parada é entrega, não interrupção**, e o custo de uma contradição achada
antes da primeira linha continua sendo uma mensagem.

**O identificador de emenda com comando de prova de leitura funcionou.** `grep -c EMENDA-02-0N`
apareceu em todos os artefatos seguintes, sempre `>= 1`. "Ele não leu" deixou de ser palpite.

**Segurar o `.tsv` até os controles estarem no alvo foi a leitura certa** do que um arquivo de
controle é. Commitá-lo com dois vermelhos por trabalho que não tinha começado teria ensinado a
todos que vermelho no portão é normal — que é exatamente como um portão morre.
