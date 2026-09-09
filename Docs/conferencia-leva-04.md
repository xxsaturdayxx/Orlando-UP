# Conferência visual — leva 04

**Data:** 2026-09-09. **Spec:** `Docs/spec-04-admin-catalog.md` §9, com as notas `EMENDA-04-01` a
`EMENDA-04-07`. **Base:** `dotnet run --project src/OrlandoUp.Web`, LocalDB `OrlandoUpDb` **com a
migration já aplicada**, administrador semeado da leva 01, navegador em português.

**Prova de leitura exigida pelas duas notas:** ocorrências neste arquivo, contadas com
`grep -c` do arquivo gravado — `EMENDA-04-06`: **4**; `EMENDA-04-07`: **5**.

**Prova de build fresco:** a barra de navegação da D9/04, com os quatro destinos, visível em toda
tela da administração. Ela não existia no binário anterior, então vê-la é a prova de que o que
renderizou é esta leva.

**Como cada linha foi medida:** o Rod dirigiu o site de pé e relatou na conversa; o revisor
transcreveu. Onde a asserção é sobre **aparência** — anel de foco percebido, anúncio de leitor de
tela —, a linha diz **o que o controle garante** e marca o item como dele, porque nenhuma leitura de
arquivo enxerga isso. Onde a linha diz **não exercido**, ninguém a executou: é ausência declarada,
não verde presumido.

| # | O que | Esperado | Medido | |
|---|---|---|---|---|
| 1 | barra de navegação | quatro destinos alcançáveis de qualquer tela | leva aos quatro, de qualquer tela | ✅ |
| 1a | as três contagens do painel | produtos, unidades, locais | **não relatado** — a tela abriu, os números não foram conferidos | ⚠️ não exercido |
| 2 | `/admin/products` | link de editar por linha, botão de produto novo, aviso de somente-leitura sumido | os três, conferidos em captura de tela | ✅ |
| 3 | os quatro blocos do editor | produto, inglês, português, preços, adicionais | todos renderizam | ✅ |
| 4 | as duas traduções ao mesmo tempo | visíveis sem trocar o idioma da administração | visíveis; **empilhadas**, não lado a lado — ver achado A6 | ✅ invariante, ⚠️ forma |
| 5 | editar descrição e ver no site | o texto novo aparece em `/rentals` e `/pt/rentals` | aparece nas **duas** culturas, sem reiniciar nada | ✅ **é o caminho central da leva, provado de ponta a ponta** |
| 6 | limpar a largura da cadeira de rodas | salva ausente, volta **vazia**, nunca `0` | volta vazia | ✅ **D15 provada contra o banco real** |
| 7 | carrinho marcado "À venda" sem lista de preço | recusa nomeando o problema | *"Produto à venda precisa de ao menos uma faixa de preço"* — é o `PricingTierSetProblem.Empty` chegando em palavras | ✅ |
| 8 | produto criado pela tela nasce oculto (A3) | some de `/rentals`, 404 no endereço, listado em `/admin/products` | **não exercido** — nenhum produto foi criado. O teste `A_product_created_through_the_screen_is_born_hidden` cobre o comportamento | ⚠️ não exercido |
| 9 | `/admin/units` | lista com etiqueta, produto e situação; marcar em manutenção | funciona como descrito | ✅ |
| 10 | etiqueta duplicada | validação em palavras, não exceção de banco | *"Outra unidade já carrega esta etiqueta"* | ✅ |
| 11 | `/admin/audit` | os gestos do dia, do mais recente ao mais antigo, com o e-mail de quem fez | todos presentes, inclusive as edições de descrição | ✅ |
| 12 | teclado — alcance | Tab chega a todos os campos do editor | chega a todos | ✅ |
| 12a | teclado — anel de foco visível | ver onde o foco está em cada parada | **não relatado**. O mecanismo está garantido: controle C12 do `public-site.tsv` mede **0** ocorrências da forma que remove o contorno, e o C13 afirma que a folha define foco visível | ⚠️ mecanismo ✅, **o passeio é seu** |
| 12b | `Shift+Tab` volta na mesma ordem | — | **não exercido** | ⚠️ |
| 12c | mensagem de erro **anunciada** por leitor de tela | `role="alert"` dispara sozinho quando o erro aparece | **não exercido.** O Narrador foi aberto e lê a página **ao clicar numa linha**, que é navegação normal de leitor de tela e não testa o anúncio automático | ⚠️ |
| 13 | 375 px — a página não rola de lado | nenhum elemento passa da borda da janela | **medido no console:** a varredura de todos os elementos cujo lado direito ultrapassa `clientWidth` devolve **`[]`**. A página não rola | ✅ |
| 13a | 375 px — a tabela rola dentro de si | `.table-scroll` com `overflow-x: auto` contém a largura | arrastando, alcança as colunas da direita sem mover a página. É o comportamento pretendido | ✅ |
| 13b | 375 px — faixas de preço e adicionais usáveis | campos legíveis e clicáveis | **não relatado** | ⚠️ não exercido |

---

## Achados

**A1 — DEFEITO. O seletor de adicionais mostra o código interno em vez do nome traduzido.**
A tela renderiza `AddOn.Code`, e a consulta carrega os adicionais sem as traduções: o operador lê
`cup-holder`, `cane-holder`, `sunshade`, `rear-basket`, `rain-cover`, `damage-waiver` enquanto o
banco guarda *Porta-copos*, *Porta-bengala*, *Proteção de sol*, *Cesto traseiro*, *Capa de chuva* e
*Isenção de danos*. É a **única** tela da administração que põe identificador onde uma pessoa lê
texto. **Nenhum teste automatizado o pegaria** — uma asserção de que a caixa existe passa dos dois
jeitos —, e é exatamente para isso que a conferência visual existe. Conserto e forma dele:
`EMENDA-04-07` G1, no commit de conteúdo final.

> **CORRIGIDO neste commit de conteúdo, na forma que a G1 fixou.** A consulta passou a carregar as
> traduções (`.Include(addOn => addOn.Translations)`); o rótulo sai do `TranslationPicker`, que
> responde *a cultura pedida, senão o inglês, senão nada*; e a página recebe
> `CatalogWriter.AddOnChoice`, um `record` de duas casas declarado **ao lado do escritor** e não em
> `Application/Catalog/CatalogViews.cs`, que a D13/04 mantém fechado porque `SeoTests.cs:261-273`
> constrói `ProductDetail` com dezessete argumentos posicionais. O rótulo mostra **só o nome**.
> A cultura é decidida no page model, num arquivo `.cs`, e nunca na marcação — o controle C09 do
> `public-site.tsv` varre `Pages/` sem excluir `Admin` e continua no alvo.
>
> **O adicional sem tradução nenhuma cai para o código, e a queda tem teste**, como toda ausência
> desta leva: um rótulo em branco seria uma caixa de marcação sem significado algum.
>
> **Os dois testes eram vermelhos antes, e isso está medido e não suposto:**
> `git show 7b94393:src/OrlandoUp.Web/Pages/Admin/Products/Edit.cshtml` mostra, na linha 231,
> `<label for="addon-@addOn.Id">@addOn.Code</label>`. A asserção nova
> `Assert.DoesNotContain(">cup-holder<", html)` cai contra aquela linha, e a
> `Assert.Contains("Cup holder", html)` também.

**A2 — CONTEÚDO, e é do operador.** *"Carrinho simples"* se lê como *barato* em vez de *para uma
criança*. A string é o `Name` em `pt-BR` do produto, numa coluna que o editor desta leva escreve:
corrige-se pela tela, sem código e sem migration. O inglês *"Single stroller"* fica — é vocabulário
padrão de aluguel nos EUA e não carrega a ambiguidade (D31). **Consequência registrada, não
corrigida aqui:** o `CatalogSeedData.cs` ainda carrega o texto antigo, e um banco criado do zero
nasceria com ele de volta. `EMENDA-04-07` G2.

**A3 — Lacuna de vocabulário, e ela custou duas rodadas desta conferência.** A spec, a D32 e todos
os documentos dizem `IsBookable` / "reservável"; a tela diz **"À venda"** / *On sale*. A tela está
certa — é o que um operador entende. O que falta é o glossário dizer que são a mesma coisa. Duas
vezes o revisor mandou procurar uma palavra que não está escrita em lugar nenhum da interface.

**A4 — Descoberta: o editor não é alcançável navegando.** Ele fica atrás de um link de texto na
**última coluna** da tabela de produtos, e essa coluna está além da borda em janela estreita. Quem
não souber que o link existe não chega ao editor — foi o que aconteceu na primeira tentativa desta
conferência.

**A5 — `.table-scroll` não dá sinal de que há mais colunas à direita.** O conteúdo alcança-se
arrastando, mas nada na tela avisa que ele existe.

**A6 — As duas traduções aparecem empilhadas; a §5.1 da spec diz "lado a lado".** O invariante que
importa está cumprido (as duas visíveis de uma vez, sem ramificar por cultura na marcação — D3/04, e
o controle C09 do `public-site.tsv` continua no alvo). O texto da spec e a tela é que discordam.

**A7 — Alarme falso, registrado para ninguém o levantar de novo.** Um "a página rola
horizontalmente" foi relatado e, remedido, era duas outras coisas: a rolagem **interna** da tabela e
o modo *Fit to window* do simulador de dispositivo, que escala o render e produz barra sozinho. A
varredura do console devolveu `[]`. **Não há defeito de largura.**

**A8 — Baterias.** Durante a conferência o Rod percebeu que as baterias das scooters são removíveis,
identificadas individualmente, e que um aluguel sai com uma ou duas delas — fato que nenhum
documento do projeto carregava. Não afeta a leva 04, que fechou sem qualquer noção de bateria, e
isso agora é **ausência conhecida**. Registrado como **Q14** em `Docs/open-questions.md`, a responder
**antes da spec da leva 03**, porque muda o schema dela e não só as telas.

---

## Pendências, nomeadas em vez de escondidas

**Do Rod, e todas cabem na rodada de estética:**

1. O passeio de Tab olhando o **anel de foco** (12a) e o `Shift+Tab` (12b).
2. O anúncio da mensagem de erro por leitor de tela (12c) — com o Narrador ligado, salvar um produto
   com o nome em inglês em branco e ouvir se ele fala sem que ninguém clique na mensagem.
3. Criar um produto pela tela e ver que ele nasce oculto (8).
4. As faixas de preço e as caixas de adicional a 375 px (13b).
5. As três contagens do painel (1a).
6. A estética das telas de administração, adiada de propósito nesta conversa — e é onde A4, A5 e A6
   se resolvem juntos.

**De pé desde a leva 02:** os itens 7, 8 e 9 da `Docs/conferencia-leva-02.md`, que têm a metade
mecânica verde e a metade humana aberta. A acessibilidade é requisito (D9), então estes não
desaparecem por não terem sido feitos: ficam escritos até alguém os fazer.

---

## O que o commit de conteúdo final carrega

Pela `EMENDA-04-06` F3 passo 2, este documento e as duas coisas abaixo entram num commit só, e é o
hash **dele** que a linha da fila grava — a leva não está completa enquanto a conferência não
existir.

| O quê | De onde |
|---|---|
| o conserto do seletor de adicionais | `EMENDA-04-07` G1, achado A1 acima |
| os dois testes que o guardam | o mesmo item, que exige a queda para o código com teste |
| `Admin_BackToDashboard` fora dos dois `.resx` | `EMENDA-04-06` F2 |
| este documento | `EMENDA-04-06` F3 passo 2 |

**A F2 vem com um aviso para quem repetir a medição:** uma varredura literal por chaves `Admin_` sem
uso relata **catorze** órfãs e **treze são falsas** — `Admin_Tier…`, `Admin_Seat…`, `Admin_Status…`
e `Admin_Action…` são montadas interpolando o nome do membro do enum, e nenhum `grep` as vê usadas.
É a lacuna que o teste `Every_enum_member_a_screen_names_has_a_key_in_both_cultures` fecha, e a
varredura ingênua vai continuar relatando aquelas treze para sempre.

**A G2 não precisa de commit nenhum:** *"Carrinho simples"* se corrige pela tela, que é a leva se
provando. O `CatalogSeedData.cs` continua com o texto antigo e está na lista negativa desta frente;
o semeador só insere em tabela vazia (`CatalogSeeder.cs:31-37`), então ele nunca sobrescreve a
edição — mas um banco criado do zero nasceria com o nome antigo. O revisor arquiva isso como
backlog depois do fechamento.

**Portões deste commit:** `dotnet build` limpo com **0 avisos**; `dotnet test` **170 passando, 0
falhando** (eram 168 na P3, e as duas novas são as da G1); os três `.tsv` com **47 controles, 0 fora
do esperado**.

---

*A leva 04 entregou o que a spec dizia, e a conferência encontrou um defeito que nenhum teste
encontraria mais duas correções de conteúdo — uma delas feita pela própria tela que a leva criou,
que é a prova mais direta de que ela serve para o que foi construída.*
