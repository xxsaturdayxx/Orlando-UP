# Conferência visual — leva 04b (roteiro, a preencher)

**Este arquivo é o roteiro, escrito ANTES da conferência e commitado com a spec.** A coluna
**Resultado** nasce em branco e é você quem preenche — linha não preenchida fica visivelmente vazia,
e é assim que "vi tudo funcionando" deixa de ser uma resposta possível.

**Spec:** `Docs/spec-04b-fleet-batteries.md` §9. **Quem preenche:** o Rod. **O agente não escreve
resultado neste arquivo** — ele está na lista negativa da §11.1 da spec, porque conferência que o
agente escreve é conferência que ninguém fez.

**Como responder:** `ok` quando fizer e vir o esperado; **o que viu**, quando divergir; e
**`não fiz`** quando não fizer — que é resposta legítima e melhor que verde inventado. O que ficar
`não fiz` entra no documento final como não alcançado, com o motivo.

---

## Pré-condições — em bloco, antes do primeiro item

1. **Recompilar e subir.** O schema mudou e as telas são novas.
   ```
   dotnet run --project src/OrlandoUp.Web
   ```
2. **A migration já tem de estar aplicada** (passo humano 1 da §0 da spec), e as doze baterias
   semeadas (passo 3). Sem elas, os itens 1 a 6 não têm o que mostrar.
3. **Entrar em `/admin`** com a conta semeada na leva 01.
4. **Prova de build fresco, e é o item 0:** a barra de navegação tem **seis** destinos, não quatro.
   Se tiver quatro, o binário é velho — derrube e suba de novo antes de conferir qualquer coisa.
5. **Estado do instrumento, para os itens de largura:** Chrome, `F12`, `Ctrl+Shift+M`, largura
   **375**, e o zoom em **100 %** — **não** em *Fit to window*, que escala o render e produz barra
   horizontal sozinho. Na conferência da leva 04 isso custou uma rodada perseguindo um defeito que
   não existia.

**O que a tela diz, para você não procurar palavra que não existe:** os rótulos desta conferência
são os que aparecem na interface em português, e não o nome da propriedade no código. Se algum item
abaixo citar uma palavra que você não acha na tela, **isso é o achado** — anote e siga.

---

| # | O que fazer | O que esperar | Resultado |
|---|---|---|---|
| 0 | abrir `/admin` | a barra tem seis destinos; o painel mostra **cinco** contagens, com baterias e carregadores | |
| 1 | abrir a lista de baterias | doze linhas: seis de cada modelo, com etiqueta, modelo, tipo, autonomia e situação | |
| 2 | olhar os tipos | **onze Normal e uma de autonomia estendida**, e a estendida é de Drive Scout | |
| 3 | olhar as autonomias | 9 milhas nas Normal, 14 na estendida | |
| 4 | criar uma bateria com etiqueta que já existe | recusa com mensagem, não tela de erro | |
| 5 | criar uma bateria e deixar a autonomia **em branco** | salva, e ao reabrir o campo volta **vazio** — nunca `0` | |
| 6 | tentar prender uma bateria a uma cadeira de rodas ou a um carrinho | recusa: bateria é de modelo de scooter | |
| 7 | marcar uma bateria como **aposentada** (foi a que o cliente quebrou) | ela continua na lista, marcada, e a contagem de disponíveis cai em um | |
| 8 | abrir a tela de configurações | três valores: contagem de carregadores (**14**), valor da segunda bateria por dia, multa de carregador perdido (**30**) | |
| 9 | mudar a multa para outro valor e salvar; reabrir | o valor novo está lá | |
| 10 | tentar salvar contagem de carregadores negativa | recusa | |
| 11 | tentar salvar o valor da segunda bateria como **zero** | **aceita** — cortesia é zero, e zero é legítimo | |
| 12 | abrir o registro de auditoria | cada gesto dos itens acima aparece, com o seu e-mail, do mais recente ao mais antigo | |
| 13 | teclado na tela de bateria: só Tab, do topo ao botão salvar | todos os campos alcançados, **e o anel de foco visível em cada parada** | |
| 14 | `Shift+Tab` na mesma tela | volta na mesma ordem | |
| 15 | leitor de tela: com o Narrador ligado (`Ctrl+Win+Enter`), salvar uma bateria sem etiqueta | a mensagem de erro é **falada sozinha**, sem você clicar nela | |
| 16 | a 375 px (pré-condição 5), lista de baterias e tela de configurações | campos usáveis; a **página** não rola de lado — a tabela rolar dentro dela mesma é o esperado | |

---

## Depois de preencher

**O que não deu para conferir, e por quê** — escreva aqui, com a mesma clareza do que passou:

*(a preencher)*

**Achados que não são item do roteiro** — qualquer coisa que a tela fez e você não esperava,
inclusive *"não entendi o que essa tela queria de mim"*, que conta como achado:

*(a preencher)*
