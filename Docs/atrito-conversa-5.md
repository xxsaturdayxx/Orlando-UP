# Atrito — conversa 5 (leva 04b, baterias e carregadores)

**Rodadas totais: 18 — rodadas de transporte: 3 (17 %).** Abaixo do quinto que a régua do projeto
usa como limiar, e **abaixo dos 25 % da conversa 4**. As três continuam vindo do mesmo lado: do
revisor.

Esta contagem não atribui culpa por rodada. Ela existe para achar a mudança que faz aquela classe de
rodada deixar de existir.

| Atrito | Vezes | Categoria | Solução |
|---|---|---|---|
| **A instrução para o agente voltou como pergunta.** O texto para colar no Claude Code foi emitido no fim de uma mensagem que abria com o bloco de veredito e sete correções numeradas; o Rod voltou com *"pode me repedir então qual a instrução para CC?"* | 1 | **emissão** — a informação foi entregue, em formato que não sobreviveu ao uso | o bloco para colar sai **primeiro**, sozinho, antes do veredito e das correções. Quem revisa lê o parecer; quem executa precisa de uma linha. São dois leitores e a mensagem estava ordenada para o primeiro |
| **O relato do agente chegou cortado pelo canal** — veio a linha de abertura e nada mais, e o conteúdo só chegou na rodada seguinte | 1 | **artefato** | o relato **já existe** como arquivo commitado (`Docs/relatorio-leva-04b-etapa-N.md`), e é isso que o protocolo pede. Basta o Rod colar **o caminho e o hash**, não o texto: o revisor lê do repositório. Colar texto longo no chat é transporte que a ponte já faz melhor |
| **O site estava fechado quando o revisor foi medir.** O revisor pediu o site fechado ao fim da conferência (com razão: é o que destrava o portão), ofereceu medir o item 16 no navegador, e só descobriu o `ERR_CONNECTION_REFUSED` depois do "faça você" | 1 | **emissão** | a oferta de medir no navegador vem com a pré-condição colada: *"deixe o site de pé e me avise"*. Mesma classe do bloco acima — instrução emitida sem o que ela pressupõe |
| **A rota foi lida de uma captura de tela em vez da spec.** O revisor navegou para `/admin/baterias` porque o menu mostrava "Baterias"; a rota é `/admin/batteries`, e está escrita na §5 da spec | 1 (não é rodada) | **emissão** | é a reincidência da regra que a conversa 4 já escreveu: **rótulo de tela não é nome de rota, de coluna nem de arquivo**. Custou uma chamada, não uma rodada, porque o erro voltou em um 404 e não numa afirmação. A regra não muda; muda que desta vez o instrumento acusou sozinho |
| **A linha do roteiro de conferência não nomeou a tela.** O item 7 pedia "a contagem de disponíveis cai em um"; o Rod olhou o Painel, que conta o total, e a contagem de disponíveis está na página Baterias | 1 | **emissão** | **terceira reincidência da mesma classe**: a linha do roteiro tem de nomear **a tela** e usar **a palavra que a interface mostra**. A tela diz "Baixada"; o roteiro dizia "aposentada". Antes disso foi o "reservável" da leva 04, duas vezes |

---

## O que mudou desde a conversa 4, e é o resultado desta

O `Docs/atrito-conversa-4.md` registrou, na linha *"fica para depois"*:

> um caminho do revisor até o banco e até o `dotnet`, que é atrito de acesso real e persistente —
> todo número de build, de suíte e de banco desta conversa é herdado do relato do agente. Não entra
> agora porque **não há solução barata à vista**.

**Metade disso caiu, e caiu em uma rodada.** O acesso ao Chrome do Rod transformou o item 16 da
conferência de *olhado* em *medido*: viewport de 375 conferido por `documentElement.clientWidth`,
transbordo listado elemento a elemento, e cada elemento classificado por ter ou não um contêiner com
`overflow-x` acima dele.

E a medição **corrigiu a si mesma**: a primeira devolveu **360**, porque a barra de rolagem come
15 px de um iframe de 375. Um olho a 375 não vê essa diferença, e é por isso que ela é o argumento.

Era atrito de **acesso**, e foi resolvido por **canal** — como a Etapa 2 prevê. Nenhuma regra escrita
teria produzido esse número.

**A outra metade continua de pé:** `dotnet` e banco. Os controles C14 e C15 e toda contagem de linha
desta conversa são herdados do relato do agente. Um MCP de SQL Server apontado para o `OrlandoUpDb`
fecharia a parte do banco; foi oferecido nesta conversa e não decidido.

---

## Mudança de maior retorno

**O bloco para colar no agente sai primeiro na mensagem, sozinho, antes do parecer.**

Ela cobre **dois** dos cinco atritos da tabela — a instrução que voltou como pergunta e a oferta sem
pré-condição — e as duas são da mesma raiz: uma mensagem escrita para o leitor errado. O parecer da
revisão é para quem decide; o bloco é para quem executa. Quando os dois viajam juntos e o parecer vem
primeiro, o bloco some.

**Custa:** nada. É ordem de parágrafo.

**Fica para depois:**

- **o MCP de banco.** É a metade restante do atrito de acesso, e é a que produz os números que o
  revisor hoje herda. Não entra agora porque a troca de ferramenta não se faz no meio de uma frente
  aberta — entra no intervalo, e o intervalo é agora;
- **o roteiro de conferência gerado a partir das strings da interface.** A classe "palavra que a tela
  não usa" já reincidiu três vezes, e escrever a linha à mão é o que a produz. A saída real é a linha
  nascer com um `grep` no `.resx` colado ao lado, como a conversa 4 decidiu para as medições. Não
  entra agora porque muda o esqueleto da spec, e isso é frente própria.

---

## O que esta conversa produziu que vai precisar ser transportado à mão amanhã

**As cinco regras de método da §"Decisões permanentes" do resumo.** Elas estão no
`Docs/resumo-conversa-5.md`, que é lido na abertura da conversa 6 — mas nenhuma está em
`Docs/regras-de-controle.md` nem em `Docs/decisions.md`, que são os documentos que o agente relê. Foram
propostas como emenda à skill `revisao-plano-agente`, e **o estado dessa proposta não foi
reconferido**. Se a emenda não tiver sido salva, as cinco vivem só no resumo.

**A resposta da Q15.** Ela está escrita como pergunta aberta no repositório, com as duas metades
separadas — que é o formato que sobreviveu na Q14. A decisão que sair dela nasce na conversa 6 e
precisa chegar à spec da leva 03.

**Nada mais.** As quatro emendas, os dois relatórios de parada, a conferência preenchida, este
documento e o resumo estão todos commitados; nenhum fato desta conversa vive só no chat.
