# Atrito — conversa 6 (leva 03, núcleo da reserva)

**Rodadas totais: 7 — rodadas de transporte: 1 (14 %).** Abaixo do quinto que a régua do projeto
usa como limiar, e abaixo dos 17 % da conversa 5. A conversa foi a mais longa da série em conteúdo
(spec de 1 045 linhas, quatro paradas, duas emendas) e a mais curta em rodadas, e as duas coisas têm
a mesma causa: **tudo o que atravessou as duas pontas atravessou como arquivo commitado** — plano,
relatório, emenda, roteiro. O chat carregou só hashes e uma frase.

Esta contagem não atribui culpa por rodada. Ela existe para achar a mudança que faz aquela classe de
rodada deixar de existir.

| Atrito | Vezes | Categoria | Solução |
|---|---|---|---|
| **A correção da P3 pediu "pare e espere a conferência"**, e a parada virou uma rodada inteira cujo conteúdo foi *"conferido; agora é sua a conferência"*. A instrução da conferência já estava pronta na rodada anterior | 1 | **emissão** | correção pequena e verificável por controle (`verificar` nos `.tsv`) vem com a instrução seguinte colada: *"corrija, rode o portão e siga para X"*. Parada só quando a correção muda o que vem depois |
| **O `ok` do item 5 do roteiro passou por cima de um `{0}` cru na tela** — o print anexado mostra *"Reserva {0} criada."* e a célula lê `OK`. A linha do item 5 tinha **dez** expectativas numa célula só | 1 (não é rodada; é defeito que quase fechou verde) | **artefato** | **uma expectativa por linha** quando a expectativa é texto exato que uma chave produz. Célula com dez expectativas vira "vi tudo", que é exatamente o que o roteiro em arquivo existia para impedir. O print salvou; o roteiro não |
| **A ponte de arquivos caiu duas vezes e depois a máquina virtual parou de montar a pasta** (atualização do Windows de 8/9, segundo a própria ferramenta). O `git` do revisor sumiu no meio da conversa; os commits das revisões 1 a 3 saíram, o fechamento não tem como sair pela ponte | 3 (nenhuma virou rodada; viraram minutos de espera e um caminho alternativo) | **acesso** | leitura e escrita continuam por `stage`/`commit` de arquivo; **o commit dos documentos de fechamento desta conversa é do Claude Code**, na sessão que já está aberta — custo zero, e registrado aqui para não virar regra por acidente. Volta ao normal quando a pasta voltar a montar |
| **Contagem errada do revisor** (11 índices e 8 FKs na revisão da P1; eram 10 e 7). O agente corrigiu na P2 e a correção teve de ser registrada em dois lugares | 1 (não é rodada) | **emissão** | número que sustenta parecer sai de `grep -c` no SQL, não de contar na tela — a regra da spec-skill, violada pelo próprio revisor |

---

## O que mudou desde a conversa 5

**A "mudança de maior retorno" da conversa 5 — o bloco para colar sair primeiro — foi aplicada em
todas as sete rodadas** e nenhuma instrução voltou como pergunta.

**A regra "rótulo nomeia classe, operando conta membro" voltou pela terceira vez**, e desta vez ao
contrário: o agente **moldou o código para caber no controle** (tirou o `Record` do writer para
manter o C05 do `admin-catalog` em 2). A revisão da P3 devolveu o código ao lugar certo e emendou o
controle com um irmão. A lição nova: **um controle permanente que resiste ao desenho certo é
emendado com o irmão que mede o que ele deixou de medir — nunca contornado pelo código.**

**Três fatos `[H]` da spec caíram na P1** (13 baterias e não 12; carrinho reservável sem unidade;
`LostChargerFee` editado) e um item humano ganhou um quarto produto por causa disso. O carimbo
`[H]` fez o que existe para fazer: o agente reconferiu exatamente o que estava marcado.

---

## Mudança de maior retorno

**Uma expectativa de texto por linha do roteiro de conferência.** É a única das quatro que quase
custou um defeito em produção com tudo verde, e custa uma linha a mais por chave. O item 5 vira
seis linhas (número, situação, origem, dias, cinco valores, histórico) — e cada uma tem um `ok`
que não pode significar outra coisa.

**Custa:** roteiro mais comprido. É barato: o Rod respondeu 27 itens em uma rodada.

**Fica para depois:**

- **o MCP de banco** — terceira conversa em que fica. Os números de banco desta conversa vieram do
  agente (`sqlcmd` com `SELECT DB_NAME()` colado), que é melhor do que herdar do relato, mas ainda
  não é o revisor medindo;
- **a ponte de arquivos** — depende de correção externa (Windows); enquanto durar, o commit dos
  documentos do Claude Web sai pelo Claude Code ou pelo operador.

---

## O que esta conversa produziu que vai precisar ser transportado à mão amanhã

**Nada que não esteja em arquivo.** As duas emendas (topo da spec), as três revisões (fim de cada
relatório), a conferência com resultado, este documento, o resumo e o backlog estão no repositório
— os três últimos no commit de fechamento que o Claude Code faz, porque a ponte não monta a pasta.
O que vive só no chat são os onze prints da conferência, e o único fato deles que importa (o
`{0}`) está transcrito no `Docs/conferencia-leva-03.md`.
