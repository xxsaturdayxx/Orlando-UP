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
| 0 | abrir `/admin` | a barra tem seis destinos; o painel mostra **cinco** contagens, com baterias e carregadores |OK |
| 1 | abrir a lista de baterias | doze linhas: seis de cada modelo, com etiqueta, modelo, tipo, autonomia e situação |OK |
| 2 | olhar os tipos | **onze Normal e uma de autonomia estendida**, e a estendida é de Drive Scout |OK |
| 3 | olhar as autonomias | 9 milhas nas Normal, 14 na estendida |OK |
| 4 | criar uma bateria com etiqueta que já existe | recusa com mensagem, não tela de erro |OK | "Outra bateria já carrega esta etiqueta."
| 5 | criar uma bateria e deixar a autonomia **em branco** | salva, e ao reabrir o campo volta **vazio** — nunca `0` |OK |
| 6 | tentar prender uma bateria a uma cadeira de rodas ou a um carrinho | recusa: bateria é de modelo de scooter | **não exercido pela tela, e por um motivo melhor** — o campo é um dropdown com só os dois scooters (Drive Scout e Spitfire), então o gesto é inalcançável. A recusa do servidor existe e está coberta pelo teste `A_battery_cannot_be_attached_to_a_product_that_is_not_a_scooter`, que faz o POST direto. Tela *fail-closed* vale mais que a mensagem de recusa.
| 7 | marcar uma bateria como **aposentada** (foi a que o cliente quebrou) | ela continua na lista, marcada, e a contagem de disponíveis cai em um | **ok — e a linha do roteiro é que estava errada.** O Rod olhou o Painel, que mostra **13**, e o Painel conta o TOTAL (`Batteries.Count()`), igual ao que já fazia com Unidades. A contagem de disponíveis vive na página Baterias, no rótulo *"Disponíveis agora:"*, e caiu: **12 disponíveis e 1 Baixada** (`Baixada` é o pt-BR de `Retired`). O roteiro não nomeou a tela; o defeito é do roteiro, não do código.
| 8 | abrir a tela de configurações | três valores: contagem de carregadores (**14**), valor da segunda bateria por dia, multa de carregador perdido (**30**) |OK |
| 9 | mudar a multa para outro valor e salvar; reabrir | o valor novo está lá |OK |
| 10 | tentar salvar contagem de carregadores negativa | recusa |OK |
| 11 | tentar salvar o valor da segunda bateria como **zero** | **aceita** — cortesia é zero, e zero é legítimo | OK|
| 12 | abrir o registro de auditoria | cada gesto dos itens acima aparece, com o seu e-mail, do mais recente ao mais antigo |OK |
| 13 | teclado na tela de bateria: só Tab, do topo ao botão salvar | todos os campos alcançados, **e o anel de foco visível em cada parada** |OK |
| 14 | `Shift+Tab` na mesma tela | volta na mesma ordem | OK|
| 15 | leitor de tela: com o Narrador ligado (`Ctrl+Win+Enter`), salvar uma bateria sem etiqueta | a mensagem de erro é **falada sozinha**, sem você clicar nela |ok |
| 16 | a 375 px (pré-condição 5), lista de baterias e tela de configurações | campos usáveis; a **página** não rola de lado — a tabela rolar dentro dela mesma é o esperado | **ok, medido e não olhado.** A resposta original (*"fizemos antes"*) foi retirada: o teste anterior foi na tela de Produtos da leva 04 e o simulador estava em *Fit to window*, que a pré-condição 5 proíbe. Refeito no Chrome do Rod pelo revisor, num viewport de **375 exatos** conferido por `documentElement.clientWidth`. **`/admin/batteries`:** `scrollWidth` 375 = `clientWidth` 375, a página **não** rola de lado; 87 elementos passam de 375 e **os 87 estão dentro de `div.table-scroll`** — nenhum escapa de um contêiner com `overflow-x`. **`/admin/settings`:** nenhum transbordo. Campos: todos com 327 px de largura e 50–52 px de altura, nada cortado, nada abaixo do alvo de toque.

---

## Depois de preencher

**O que não deu para conferir, e por quê** — escreva aqui, com a mesma clareza do que passou:

**Item 16** voltou como `não fiz` e foi refeito pelo revisor no Chrome do Rod, com o instrumento
declarado — viewport de 375 conferido por script, e o transbordo listado elemento a elemento em vez de
olhado. É a terceira vez que o *Fit to window* do simulador entra nesta conferência; ele não mede 375.

**Item 6** não foi exercido pela tela, e isso é o achado: a tela torna o gesto inalcançável.

**Todo o resto foi exercido.** Nenhuma linha ficou `não fiz` no fim.

**Achados que não são item do roteiro** — qualquer coisa que a tela fez e você não esperava,
inclusive *"não entendi o que essa tela queria de mim"*, que conta como achado:

**O rótulo do Painel conta o total, não o disponível** — e foi isso que confundiu o item 7. O número
é coerente com "Unidades", que também é um censo, então não é defeito hoje. Mas a leva 03 reserva contra
o **disponível**, e vale decidir ali se o Painel passa a mostrar os dois. Fica no backlog, não nesta leva.

**"Baixada" é o termo da tela para `Retired`** — o roteiro dizia "aposentada", que não existe na interface.
Mesma classe do "reservável" da leva 04: palavra do roteiro que a tela não usa.
