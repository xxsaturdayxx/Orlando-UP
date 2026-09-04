# Regras de controle

Doze regras, cada uma comprada com pelo menos um defeito real. Elas existem para fechar a mesma
classe de falha por caminhos diferentes: **um controle que fica verde sem medir nada**. Verde falso
é pior que vermelho, porque ninguém volta a olhar.

O portão só vale enquanto vermelho significar defeito. Controle que envelhece por construção
transforma o portão em ruído, e ruído é ignorado — por isso metade destas regras é sobre **o que
não pode ser controle**.

---

## O que o controle mede

**1. Alvo é identificador, nunca nome legível.** `Slug = "discovery-cove"` só existe na entrada
nova; `"Discovery Cove"` existe em qualquer parágrafo que cite o parque. Quatro controles de uma
frente dariam verde estando quebrados por isso.

**2. Comentário conta no controle, e pode quebrar o controle de outra frente.** `grep` não sabe o
que é comentário. Um comentário que citava o identificador de um campo morto fez o controle de uma
frente já fechada — um negativo que afirma *"nenhum código lê aquele campo"* — medir 2 onde afirma
0. **O controle estava certo e a prosa é que estava errada.** Comentário que precisa explicar por
que um identificador NÃO está ali escreve isso **sem escrever o identificador**.

**3. Comentário que explica um CONTROLE nunca transcreve a forma que o controle procura.** Uma
volta a mais que a regra 2: ali o comentário falava do campo; aqui ele explicava a escolha de um
lambda e transcrevia o literal que o controle conta — e o quebrou. O comentário nomeia o controle,
diz o motivo, e **descreve a forma sem escrevê-la**.

**4. Contagem absoluta de linhas de arquivo compartilhado não é controle, e contagem de ocorrência
também não — a forma à prova é a RELAÇÃO.** O defeito não é medir linha, é medir **tamanho**:
qualquer frente seguinte que toque o arquivo legitimamente derruba o número, e o vermelho não
significa defeito nenhum. Quando o invariante for *"todo X consulta Y"*, o controle é
`contagem(X) − contagem(Y)`, e **a diferença é o invariante** — *handlers menos guardas = 1*, onde
o 1 é a allowlist. Isso sobrevive a toda frente futura que acrescente handler.
**Corolário:** contagem de ponto de entrada ancora no **nome** (`OnPost…Async`), nunca no tipo de
retorno — ancorar no tipo repete no controle o buraco que a reflexão existe para fechar.

**5. Toda subtração nasce com asserção de ALCANCE — zero menos zero também dá zero.** Se os dois
operandos zerarem porque o padrão parou de casar ou o arquivo mudou de nome, a diferença continua
dando o valor esperado e o controle fica verde medindo nada. A asserção de alcance é um controle
irmão que afirma o operando maior.

**6. Asserção de alcance de controle PERMANENTE é limiar, nunca cardinal.** Num arquivo de
controles sem frente dona, o cardinal envelhece na primeira frente que acrescente um arquivo —
**inclusive a própria frente que o escreveu**. A forma à prova devolve `sim`/`nao` para um limiar
não trivial, com uma asserção provando que o limiar **sabe dizer `nao`**. Prova que a varredura
chega, sem afirmar quanto.

**7. Todo filtro dentro de um controle precisa de asserção provando que não zera o universo.** Um
filtro que exclui comentário, ou restringe a um subconjunto, pode apagar tudo — verde perfeito
medindo nada.

---

## Como o controle é escrito

**8. Todo controle nasce com par de asserção de dois lados — casa a forma proibida, não casa a
forma certa — rodado A PARTIR DO ARQUIVO GRAVADO, nunca da memória de quem o escreveu.** Três
falsos positivos de substring numa semana motivam: um identificador contido dentro de outro (o
controle mediria 1 *por o nome novo estar certo*), um metacaractere degenerado em quantificador de
nada, e um padrão de destrutivos casando uma palavra que aparece em migration puramente aditiva.

**9. Metacaractere literal vai em CLASSE DE CARACTERE, nunca em barra invertida.** Classe não
precisa de escape, então não há o que o transporte entre ferramentas comer. Uma interrogação
escapada que perdeu a barra vira quantificador: o negativo passa a casar exatamente a forma que o
positivo afirma.

**10. Operando de contagem vai com `; true` e valor-padrão na expansão, nunca com `echo 0`
pendurado no ou-lógico.** `grep -c` **imprime `0` e ainda assim sai com código 1** quando o padrão
não casa: o ramo de falha dispara em cima da saída que já existe, e a substituição devolve dois
zeros que quebram a subtração.

**11. Nenhum comando de controle usa `git grep`.** Ele varre só arquivo rastreado, e o portão de
fim de sessão roda **antes** do commit — todo arquivo que a frente cria é invisível para ele
naquele momento, e o controle mede a menos com cara de defeito da frente.

**12. Busca de AUSÊNCIA em documento de prosa é multilinha, nunca `grep` de uma linha.** Texto que
quebra em coluna faz uma expressão de duas palavras cair partida entre duas linhas: o comando
devolve **0 com a string presente** — verde por ausência que não existe. Junte as linhas antes de
procurar.

---

## Onde cada coisa mora

**A seção de controles de toda spec é saída do medidor, nunca digitada.** O arquivo de alvos é
`.tsv`; um modo gera a tabela e outro remede e sai com código de erro se qualquer controle
divergir. Quatro números de uma conversa estavam errados por terem sido escritos à mão.

**`medir` roda no HEAD em que a frente começa, antes de alterar qualquer arquivo.** Controle de
mudança cujo valor "hoje" já é o valor final pretendido é falso verde por construção: mede 3 antes
e 3 depois e passa sem nunca ter medido a mudança.

**Controle rodado antes da última alteração do alvo não é controle** — ele mede um estado que
deixou de existir, e passa por isso, não por acerto.

**Quando uma emenda muda o escopo, todo controle já proposto é remedido no HEAD antes de valer.**

**O `.tsv` carrega invariante ancorada em identificador que a própria frente move; asserção do
método de medição é da execução e mora no relatório.** O `.tsv` é remedido para sempre: uma
asserção ancorada em arquivo de outra frente fica vermelha no dia em que alguém mexer legitimamente
naquela listagem. No relatório ela está colada, datada, e prova o que precisa provar — que a
varredura discriminou **naquela medição**.

**Controle negativo de escopo é medido contra a FAIXA de commits, nunca contra a árvore** — a
árvore envelhece entre a medição e o commit.

**Controle negativo de escopo da EXECUÇÃO expira com a frente que o motivou.** Um controle cuja
descrição cita *"os arquivos que a linha da fila declara intocados"* morre quando aquela linha
morre. Distinguir do **permanente**, que não tem frente dona e declara a permanência no próprio
texto.

**Controle negativo que procura um literal dentro do arquivo onde a própria instrução mora conta a
si mesmo.** Expresse **por coluna** (*"a coluna Commit das 3 linhas lê vazio, `b3500ac`,
`b3500ac`"*), ou ancore no começo da linha.

**Estado congelado do banco, evidência de controle e fato medido que sustenta pedido futuro vão
colados no relatório COMMITADO, nunca só na resposta da sessão.** A sessão fecha, o transcript some,
e a afirmação sobrevive no repositório sem a prova — o que torna a reconferência impossível. Se um
fato foi medido e não gravado, ele será medido de novo ou lembrado errado.

---

## Duas ressalvas que invertem a hierarquia

**Quando duas formas produzem o mesmo número, nenhuma asserção de comportamento as distingue — a
barreira tem de ser o controle de FORMA.** Um filtro por presença trocado por coalescência deixa a
suíte inteira verde: item sem preço vale nulo ou zero, e zero não muda a soma. Nenhum teste de
valor pode distinguir as duas formas, porque o que muda é o **significado**, e significado não tem
asserção numérica. **Corolário:** todo controle de "nulo nunca vira zero" cobre as três formas de
coalescer no mesmo alvo — alvo estreito deixa passar justamente a forma que a suíte também não pega.

**Controle que estorva o desenho é para revisar, não desenho para dobrar.** Quando a contagem não
fecha, a primeira hipótese é o controle estar mal escrito: reescreva o alvo. Nunca afrouxe o número
e nunca mexa no código para caber na asserção.
