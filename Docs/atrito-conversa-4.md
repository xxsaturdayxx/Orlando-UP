# Atrito — conversa 4 (leva 04, administração do catálogo)

**Rodadas totais: 20 — rodadas de transporte: 5 (25 %).** Acima do quinto que a régua do projeto
usa como limiar, e todas as cinco vieram do mesmo lado: do revisor.

Esta contagem não atribui culpa por rodada. Ela existe para achar a mudança que faz aquela classe de
rodada deixar de existir.

| Atrito | Vezes | Categoria | Solução |
|---|---|---|---|
| **Parafrasear uma medição em vez de refazê-la.** O comando do controle de padrões booleanos diagnosticado sem ser lido (A6); o controle de alcance pedido sem a regra de varredura que o repositório já paga (A8); a frase sobre `Program.cs` que fundia dois fatos numa medição inexistente (E2); o nome de uma caixa de marcação que não está escrito em lugar nenhum da interface, mandado procurar **duas** vezes | 4 | **emissão** — o acesso existia nas quatro, a uma chamada de distância | toda afirmação sobre uma **string de tela, um comando ou um nome de coluna** nasce com o comando que a produziu colado ao lado. O formato força a medição; a regra escrita sozinha já falhou quatro vezes nesta conversa |
| **O roteiro de conferência viveu numa mensagem de chat.** Dez itens em prosa, num canal sem índice. Voltou como *"vi tudo, aparentemente funcionando"*, e foram precisas três rodadas para descobrir que o roteiro não tinha sido seguido | 1 (com 2 de rescaldo) | **artefato** | o roteiro vira **arquivo commitado antes da conferência**, uma linha por item, com coluna de resultado em branco para o operador preencher. O que ele não preencher fica visivelmente vazio, em vez de precisar de interrogatório |
| **O instrumento da conferência não foi declarado.** O simulador em *Fit to window* escala o render e produz barra horizontal sozinho; gastou-se uma rodada perseguindo um defeito que não existia | 1 | **emissão** | o mesmo arquivo acima abre com um bloco de pré-condições que inclui **o estado do instrumento** — zoom em 100 %, largura exata, e a página em que se mede |
| **O passo a passo para subir o projeto foi reemitido em prosa.** Ele já existe em `README.md`, seção *Running locally*, com portas e endereços | 1 | **artefato** — existe, não foi citado | citar o caminho e a seção, e só reemitir o que muda (nesta conversa, que a migration **já** estava aplicada e que os dois `seed-` recusariam) |
| **A permissão de exclusão na pasta caiu a cada reconexão da ponte**, e o `git` deixou `index.lock`, `HEAD.lock` e quatro `tmp_obj_*` para trás — que travariam o commit seguinte, inclusive o do Claude Code | 3 | **acesso** | pedir a permissão **na abertura** de toda conversa que vai commitar, e **de novo depois de cada reconexão**. Vai como frase nova no item 6 do `Docs/protocolo-conversa.md` |

---

## Mudança de maior retorno

**O roteiro da conferência visual passa a ser arquivo commitado antes da conferência, não mensagem
depois dela.** Uma linha por item, com a pré-condição do instrumento no topo e uma coluna de
resultado que o operador preenche.

Ela cobre, sozinha, **quatro** dos cinco atritos da tabela: o *"vi tudo funcionando"* deixa de ser
possível porque cada linha fica visivelmente vazia; a caixa que não se achava seria nomeada com a
string que a tela mostra, porque escrever a linha obriga a medi-la; o instrumento entra como
pré-condição; e o que não for exercido já nasce declarado, sem precisar de três rodadas de
interrogatório para virar honesto.

**Custa:** escrever o roteiro **antes** da parada P3, e não depois — o que significa que a spec
passa a gerar dois artefatos em vez de um. Meia hora por leva.

**Fica para depois:** um caminho do revisor até o banco e até o `dotnet`, que é atrito de acesso
real e persistente — todo número de build, de suíte e de banco desta conversa é herdado do relato do
agente. Não entra agora porque não há solução barata à vista, e porque a parada obrigatória com
medição colada no relatório já cobre a maior parte do risco.

---

## O que esta conversa produziu que vai precisar ser transportado à mão amanhã

**As respostas da Q14.** Elas estão numa pergunta aberta do repositório, mas as decisões que saírem
delas vão nascer na conversa 5 e precisam chegar à spec da leva 03. É o mesmo caminho que a D26 e a
D27 percorreram, e ali funcionou — a Q14 já está escrita com as cinco perguntas separadas, que é o
formato que sobrevive.

**Nada mais.** As sete emendas, os três relatórios de parada, a conferência e este resumo estão
todos commitados; nenhum fato desta conversa vive só no chat.
