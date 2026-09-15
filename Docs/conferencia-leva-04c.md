# Conferência visual — leva 04c (roteiro, a preencher)

**Este arquivo é o roteiro, escrito ANTES da conferência e commitado com a spec.** A coluna
**Resultado** nasce em branco e é você quem preenche — linha não preenchida fica visivelmente vazia,
e é assim que "vi tudo funcionando" deixa de ser uma resposta possível.

**Spec:** `Docs/spec-04c-zones-and-places.md` §10. **Quem preenche:** o Rod. **O agente não escreve
resultado neste arquivo** — conferência que o agente escreve é conferência que ninguém fez.

**Como responder:** `ok` quando fizer e vir o esperado; **o que viu**, quando divergir; e
**`não fiz`** quando não fizer — resposta legítima e melhor que verde inventado.

**Três categorias no resultado final, com o mesmo destaque:** passou; não alcançado, com o motivo;
alarme falso, com a remedição que o desfez.

**Os itens 6 a 9 não são só conferência: são o passo humano 2 da §0 da spec.** É neles que a zona
"outro hotel" entra no seu banco — o seed não alcança um banco já semeado.

---

## Pré-condições — em bloco, antes do primeiro item

1. **Recompilar e subir.** Seis telas novas e duas reescritas.
   ```
   dotnet run --project src/OrlandoUp.Web
   ```
2. **Nenhuma migration a aplicar.** Se algum passo pedir `dotnet ef`, isso é o achado — anote e pare.
3. **Entrar em `/admin`** com a conta semeada na leva 01, **com a administração em português** (o
   seletor de idioma no topo).
4. **Prova de build fresco, e é o item 0:** a barra de navegação da administração tem **nove**
   destinos, não sete. Se tiver sete, o binário é velho — derrube e suba de novo antes de conferir
   qualquer coisa.
5. **Estado do instrumento, para os itens de largura:** Chrome, `F12`, `Ctrl+Shift+M`, largura
   **375**, zoom em **100 %** — **não** em *Fit to window*, que escala o render e produz barra
   horizontal sozinho (custou uma rodada na leva 04, outra na 04b e uma anotação na 03).
6. **Datas dos itens:** use **26 e 27 de dezembro de 2026** (dois dias), que na leva 03 responderam
   *"Disponível nessas datas"* com Aluguel **$75.00** — assim uma recusa aqui é sobre o endereço, e
   nunca sobre a frota.
7. **Dado que os itens precisam:** as quatro zonas e os dez locais do seed da leva 01, e as três
   reservas que sobraram da conferência da leva 03. Nenhum item depende de reserva nova além das que
   ele mesmo cria.

**O que a tela diz, para você não procurar palavra que não existe:** os rótulos citados abaixo são
os que a §4 da spec manda escrever no arquivo de recursos. **Se um item citar uma palavra que você
não acha na tela, isso é o achado** — anote e siga.

---

| # | O que fazer | O que esperar | Resultado |
|---|---|---|---|
| 0 | Abrir `/admin` | A navegação tem **nove** destinos: Painel, Produtos, Frota, Baterias, **Zonas**, **Locais**, Reservas, Configurações, Registro | |
| 1 | Clicar em **Zonas** | Título **"Zonas de entrega"**, o botão **"Nova zona"** e **quatro** linhas, nesta ordem: Resorts do Walt Disney World, Resorts da Universal Orlando, Hotéis da International Drive e de Lake Buena Vista, Casas de temporada | |
| 2 | Na mesma lista | A coluna **"Taxa de entrega"** lê **$0.00** nas três primeiras e **$25.00** em Casas de temporada; a coluna **"Locais"** lê 6, 2, 2 e **0** | |
| 3 | Clicar em **Editar** na linha *Casas de temporada* | Campos: Código (**`vacation-homes`**, não editável), Tipo (**"Casa de temporada"**), Taxa de entrega (**25**), Como entregamos (**"Porta"**), Alíquota de imposto (**0**), Ordem (4), caixa **"Ativo"** marcada, e dois blocos de texto — **"Inglês"** e **"Português"** — cada um com **Nome** e **Instruções** preenchidos | |
| 4 | Trocar a taxa para **30**, salvar, voltar à lista, e desfazer voltando para **25** | A lista mostra **$30.00** e depois **$25.00** de novo; a tela confirma com **"Salvo."** nas duas vezes | |
| 5 | Abrir **Registro** (última aba) | As duas edições do item 4 aparecem, com o seu e-mail e a ação **"Atualizado"** | |
| 6 | Em **Zonas**, clicar **"Nova zona"** e preencher: Código `other-hotel`, Tipo **"Outro"**, Taxa de entrega **0**, Como entregamos **"Recepção"**, Alíquota **0**, Ordem **5**, **Ativo** marcado | O formulário aceita; ainda não salve | |
| 7 | No bloco **Inglês**: Nome `Any other hotel in Orlando or Kissimmee`; Instruções `If your hotel is not on the list, choose this option and type the address. We deliver to any hotel in the Orlando and Kissimmee area, and we leave the equipment with the front desk under the name on the booking unless we agree otherwise.` | Os dois campos aceitam o texto; o de instruções é alto o bastante para ler o parágrafo | |
| 8 | No bloco **Português**: Nome `Outro hotel em Orlando ou Kissimmee`; Instruções `Se o seu hotel não está na lista, escolha esta opção e digite o endereço. Atendemos qualquer hotel da região de Orlando e Kissimmee, e deixamos o equipamento na recepção no nome da reserva, salvo se combinarmos diferente.` — e **salvar** | **"Salvo."**; a lista passa a ter **cinco** zonas, a nova por último, com **$0.00** e **0** locais | |
| 9 | Tentar criar outra zona com o mesmo código `other-hotel` | Recusada com **"Já existe uma zona com este código."** e a lista continua com cinco | |
| 10 | Abrir **Locais** | Título **"Locais de entrega"**, o botão **"Novo local"**, e **dez** linhas agrupadas pela coluna **Zona**, na ordem das zonas | |
| 11 | **"Novo local"**: Zona *Hotéis da International Drive e de Lake Buena Vista*, Nome `Westgate Lakes Resort & Spa`, Endereço `10000 Turkey Lake Rd, Orlando, FL 32819`, Ordem 3, **Ativo** | **"Salvo."**; a lista passa a **onze**, com o novo sob a zona certa | |
| 12 | Tentar criar outro local com o **mesmo nome** na **mesma** zona | Recusado com **"Já existe um local com este nome nesta zona."**; a lista continua com onze | |
| 13 | Criar o mesmo nome em **outra** zona (Casas de temporada) e depois **desmarcar "Ativo"** nele | Aceito; a lista mostra doze, e o segundo aparece marcado como inativo | |
| 14 | Abrir `/admin/bookings/create` (**Reservas → Nova reserva**) | No seletor **"Local de entrega"**, o grupo *Hotéis da International Drive e de Lake Buena Vista* agora tem **três** opções, incluindo *Westgate Lakes Resort & Spa*; o local inativo do item 13 **não** aparece em lugar nenhum | |
| 15 | No mesmo seletor, procurar a zona nova | Existe a opção **"Outro hotel em Orlando ou Kissimmee"**, sozinha no grupo de mesmo nome | |
| 16 | Escolher essa opção, preencher um cliente fictício, 26/12 a 27/12, janelas 8h–10h e 18h–20h, **deixar Endereço em branco**, linha Drive Scout 4 com Quantidade **1**, e **Criar reserva** | Recusada com **"Informe o endereço de entrega."**; nada foi criado | |
| 17 | Preencher **Endereço** com `Westgate Vacation Villas, 7700 Westgate Blvd, Kissimmee, FL 34747` e enviar | Criada; a tela da reserva mostra o número no aviso **"Reserva OU-0000NN criada."**, e **Entrega $0.00** | |
| 18 | Voltar a **Nova reserva**, mesma coisa, mas digitar **25** no campo **"Taxa desta reserva (US$)"** | Criada; a tela da reserva mostra **Entrega $25.00**, e o Total é o do item 17 **mais 25** | |
| 19 | Na mesma reserva, olhar **Histórico** | A linha de criação **diz que a taxa de entrega foi definida à mão, com o valor** | |
| 20 | Uma terceira reserva igual, com **0** digitado no campo da taxa, contra a zona *Casas de temporada* (taxa 25) | Criada com **Entrega $0.00** — o zero digitado vale zero, e não "use a taxa da zona" | |
| 21 | Uma quarta, contra *Casas de temporada*, com o campo da taxa **em branco** | Criada com **Entrega $25.00** — em branco usa a taxa da zona | |
| 22 | Abrir `/pt/book`, escolher *Drive Scout 4*, 26/12 a 27/12, Quantos 1, e em **"Onde você vai ficar"** escolher **"Outro hotel em Orlando ou Kissimmee"**, deixando **"Endereço do hotel"** em branco | Erro **"Digite o endereço da entrega."** e nenhum preço na tela | |
| 23 | Preencher o endereço e enviar de novo | **"Disponível nessas datas"**, **"2 dias, de …"**, Aluguel **$75.00**, Entrega **$0.00**, **"Impostos incluídos"**, Total **$75.00** — e o endereço continua escrito no campo depois da resposta | |
| 24 | Olhar a barra de endereços do navegador | O endereço digitado aparece na URL da consulta (é assim que ele chega ao formulário de reserva da leva 03b) | |
| 25 | Na mesma página, escolher *Disney's Pop Century Resort* e enviar **sem** endereço | **"Disponível nessas datas"** normalmente — hotel da lista não pede endereço | |
| 26 | Abrir `/pt/delivery-areas` | Aparecem **cinco** áreas, a última **"Outro hotel em Orlando ou Kissimmee"**, com o parágrafo em português que você escreveu no item 8 | |
| 27 | Abrir `/delivery-areas` (inglês) | A mesma quinta área, com o parágrafo em inglês do item 7 | |
| 28 | Em **Zonas**, desmarcar **"Ativo"** da zona nova e salvar; recarregar `/pt/book` e `/pt/delivery-areas` | A opção some do seletor e a área some da página pública; a zona **continua na lista do `/admin`**, marcada como inativa. **Volte a marcar "Ativo" antes de seguir** | |
| 29 | Largura 375 (pré-condição 5): `/admin/bookings/create` | A lista de equipamentos aparece como **cartões, um por produto**, um abaixo do outro, com os rótulos Quantidade, Segundas baterias, Segunda bateria por dia e Extras dentro de cada cartão; **nenhuma tabela e nenhuma barra de rolagem horizontal** (`document.documentElement.scrollWidth` igual a `clientWidth` no console) | |
| 30 | Ainda a 375: os blocos **Cliente** e **Entrega** da mesma tela | Os campos ficam alinhados uns sob os outros, com a mesma largura; **Endereço**, **Observações de entrega** e **Observações internas** são caixas de várias linhas, não campos de uma linha só | |
| 31 | Voltar a 1440 e conferir a mesma tela | Os cartões de equipamento aparecem lado a lado, dois ou três por linha, e nada ficou cortado | |
| 32 | Teclado apenas, em `/admin/zones/create`: Tab por todos os campos e Enter no botão | Foco visível em cada campo; o Enter envia; a recusa ou o **"Salvo."** aparece | |

**Ao terminar:** o que ficou `não fiz` ou divergente entra no relatório de fechamento com o motivo,
e o alarme falso (item que parecia defeito e a remedição desfez) entra com a remedição escrita.

---

## Resultado (transcrito pelo Claude Web, a preencher)
