# Conferência visual — leva 03 (roteiro, a preencher)

**Este arquivo é o roteiro, escrito ANTES da conferência e commitado com a spec.** A coluna
**Resultado** nasce em branco e é você quem preenche — linha não preenchida fica visivelmente vazia,
e é assim que "vi tudo funcionando" deixa de ser uma resposta possível.

**Spec:** `Docs/spec-03-booking-core.md` §10. **Quem preenche:** o Rod. **O agente não escreve
resultado neste arquivo** — ele está na lista da §11.1 da spec só na coluna de resultado, e
conferência que o agente escreve é conferência que ninguém fez.

**Como responder:** `ok` quando fizer e vir o esperado; **o que viu**, quando divergir; e
**`não fiz`** quando não fizer — resposta legítima e melhor que verde inventado. O que ficar
`não fiz` entra no documento final como não alcançado, com o motivo.

**Três categorias no resultado final, com o mesmo destaque:** passou; não alcançado, com o
motivo; alarme falso, com a remedição que o desfez.

---

## Pré-condições — em bloco, antes do primeiro item

1. **Recompilar e subir.** O schema mudou, há página pública nova e três telas de administração novas.
   ```
   dotnet run --project src/OrlandoUp.Web
   ```
2. **A migration `AddBookings` já tem de estar aplicada** (passo humano 1 da §0 da spec). Sem ela,
   nenhum item abaixo do 0 abre.
3. **As doze baterias semeadas na leva 04b continuam no banco** e a linha de configuração existe
   (`/admin/settings` abre sem a frase "Não definido" nos campos).
4. **"Dias de intervalo" = 1 nos três produtos reserváveis** (passo humano 3 da §0 da spec) — o
   item 0b mede exatamente isso, e os itens 8 e 9 dependem dele.
5. **Entrar em `/admin`** com a conta semeada na leva 01, **com a administração em português** (o
   seletor de idioma no topo).
6. **Prova de build fresco, e é o item 0:** a barra de navegação da administração tem **sete**
   destinos, não seis. Se tiver seis, o binário é velho — derrube e suba de novo antes de conferir
   qualquer coisa.
7. **Estado do instrumento, para os itens de largura:** Chrome, `F12`, `Ctrl+Shift+M`, largura
   **375**, zoom em **100 %** — **não** em *Fit to window*, que escala o render e produz barra
   horizontal sozinho (custou uma rodada na leva 04 e outra na 04b, onde a primeira medição devolveu
   360 por causa da barra de rolagem do iframe).
8. **Datas dos itens:** use sempre **20 a 24 de dezembro de 2026** (cinco dias) salvo onde o item
   disser outra coisa, para que os números batam com os da spec §9.

**O que a tela diz, para você não procurar palavra que não existe:** os rótulos abaixo são os que a
interface mostra — em português no `/admin` e em `/pt/book`, em inglês em `/book`. Se um item citar
uma palavra que você não acha na tela, **isso é o achado** — anote e siga.

---

| # | O que fazer | O que esperar | Resultado |
|---|---|---|---|
| 0 | Abrir `/admin` | A navegação tem **sete** destinos: Painel, Produtos, Frota, Baterias, **Reservas**, Configurações, Registro | |
| 0b | Abrir `/admin/produtos`, editar *Drive Scout 4* | O campo **"Dias de intervalo"** mostra **1** (se mostrar 0, o passo humano 3 não foi feito — faça nos três produtos e anote aqui) | |
| 1 | No Painel | Um cartão novo, **"Reservas ativas"**, com **0** | |
| 2 | Abrir **Reservas** | Título **"Reservas"**, a frase **"Nenhuma reserva ainda."** e o botão **"Nova reserva"** | |
| 3 | Clicar **"Nova reserva"** | Formulário com: Nome, Sobrenome, E-mail, Telefone / WhatsApp, Idioma do cliente, Dia da entrega, Dia da retirada, Janela de entrega, Janela de retirada, Local de entrega, Endereço, Observações de entrega; uma tabela com **três** linhas (Drive Scout 4, Drive Spitfire EX, Cadeira de rodas Drive) e as colunas Quantidade, Segundas baterias, Segunda bateria por dia (US$), Extras; Observações internas; a caixa **"Lançar acima da frota (resolvo à mão)"**; o botão **"Criar reserva"** | |
| 4 | Enviar o formulário sem nenhuma quantidade | Erro **"Escolha pelo menos um equipamento."** e nada criado (a lista continua vazia) | |
| 5 | Preencher um cliente fictício (nome, sobrenome, e-mail, telefone), 20/12 a 24/12, janelas 8h–10h e 18h–20h, local *Disney's Pop Century Resort*, linha Drive Scout 4 com Quantidade **1**, Segundas baterias **1**, valor por dia **8**, extra *Porta-copos* marcado; **Criar reserva** | Vai para a tela da reserva com **"Reserva OU-000001 criada."** (o número pode ser outro se houver reserva anterior — anote o que apareceu); situação **"Confirmada"**; origem **"Lançada pela equipe"**; **5 dias**; Aluguel **$160.00**, Segunda bateria **$40.00**, Extras **$5.00**, Entrega **$0.00**, Impostos **$0.00**, Total **$205.00**; em **Histórico**, uma linha de criação com o seu e-mail | |
| 6 | Voltar a **Reservas** | A reserva aparece na lista com Número, Cliente, Datas, Entrega (*Disney's Pop Century Resort*), Situação **"Confirmada"** e Total **$205.00**; **sem** o selo "Acima da frota" | |
| 7 | No Painel | **"Reservas ativas"** mostra **1** | |
| 8 | Criar mais **duas** reservas iguais à do item 5 (clientes fictícios diferentes): cada uma 1 Scout + 1 segunda bateria | As duas são criadas. Agora há **3 Scouts e 6 baterias** tomadas de 20 a 24/12 — o pool de baterias da Scout está cheio, e sobra uma máquina | |
| 9 | Tentar uma **quarta** igual (1 Scout + 1 segunda bateria) | **Recusada:** a página volta com **"Não disponível: "** nomeando Drive Scout 4 — há máquina (a quarta), não há bateria. Anote o texto exato | |
| 9b | A mesma quarta reserva, agora **sem** segunda bateria | **Recusada também:** a quarta scooter sozinha pede a sétima bateria de um pool de seis. **"Não disponível: "** de novo | |
| 10 | Marcar **"Lançar acima da frota (resolvo à mão)"** e enviar de novo (sem segunda bateria) | Criada; na tela da reserva e na lista aparece o selo **"Acima da frota"**; no Histórico, a linha de criação diz que foi lançada acima da frota | |
| 11 | Abrir `/pt/rentals/drive-scout-4` | O botão cinza "Reservas em breve" **sumiu**; no lugar, o botão **"Ver disponibilidade e preço"** | |
| 12 | Clicar nele | Abre `/pt/book` com *Drive Scout 4* já escolhido; campos **Equipamento, Dia da entrega, Dia da retirada, Quantos, Segunda bateria (só scooters), Extras, Onde você vai ficar**; botão **"Ver disponibilidade e preço"** | |
| 13 | 20/12 a 24/12, Quantos **1**, segunda bateria **0**, Pop Century | **"Esgotado nessas datas"** — as quatro Scouts estão tomadas (três normais mais a lançada acima da frota) | |
| 14 | Mesma consulta com **26/12 a 27/12** | **"Disponível nessas datas"**, **"2 dias, de …"**, Aluguel **$75.00** (faixa fixa de 1–2 dias), na linha de impostos **"Impostos incluídos"**, Total **$75.00**, e no fim a frase que começa **"Pagamento e confirmação chegam na próxima entrega"** | |
| 15 | Mesma consulta com **25/12 a 26/12** | **"Esgotado nessas datas"** — o dia de intervalo (25/12) ainda segura as máquinas devolvidas no 24 | |
| 16 | Dia da entrega = **hoje** (ou amanhã, se já passou das 18h) | Erro **"O primeiro dia de entrega possível é …"** com a data certa pela regra do corte | |
| 17 | Escolher *Cadeira de rodas Drive* com segunda bateria **1** | Erro **"Só scooter leva segunda bateria."** | |
| 18 | Abrir `/book` (inglês) com a mesma consulta do item 14 | **"Available for these dates"**, **"Taxes included"**, **$75.00** | |
| 19 | No `/admin`, abrir a **primeira** reserva (a do item 5) e cancelar com um motivo | **"Reserva cancelada."**, situação **"Cancelada"**, uma linha nova no Histórico com o motivo; o formulário de cancelar some e a frase **"Esta reserva não pode ser cancelada na situação atual."** aparece no lugar | |
| 20 | Painel | **"Reservas ativas"** mostra **3** (quatro criadas, uma cancelada) | |
| 21 | Repetir o item 13 (`/pt/book`, 20 a 24/12, 1 Scout, segunda bateria 0) | **"Disponível nessas datas"**, Aluguel **$160.00**, Total **$160.00** — o cancelamento devolveu a máquina e as duas baterias ao pool | |
| 21b | O mesmo com segunda bateria **1** | **"A segunda bateria não está disponível nessas datas — o preço abaixo é sem ela."** e o Total **$160.00**: sobra uma máquina e sobra **uma** bateria (6 − 5), que é a da própria scooter | |
| 22 | `/admin/settings` | Campo novo **"Corte para o dia seguinte (hora, horário de Orlando)"** com **18**; salvar **24** → **"A hora tem de estar entre 0 e 23."**; salvar **17** → salvo, e o item 16 refeito entre 17h e 18h muda de resposta | |
| 23 | Largura 375 (pré-condição 7): `/pt/book` com resultado e `/admin/bookings/create` | A página **não** rola de lado (`document.documentElement.scrollWidth` igual a `clientWidth` no console); a tabela de equipamentos rola **dentro** do próprio bloco; nenhum campo cortado | |
| 24 | Teclado apenas, em `/pt/book`: Tab por todos os campos e Enter no botão | Foco visível em cada campo; o Enter envia; o resultado aparece | |

**Ao terminar:** o que ficou `não fiz` ou divergente entra no relatório de fechamento com o motivo, e
o alarme falso (item que parecia defeito e a remedição desfez) entra com a remedição escrita.
