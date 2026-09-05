# Conferência visual — leva 01 (spec `Docs/spec-01-foundation.md` §8)

**Data:** 2026-09-05. **Banco:** LocalDB `OrlandoUpDb`, único banco desta fase.
**Site:** `https://localhost:7420`. **Navegador:** Chrome, janela 1280×860 salvo onde o item diz
outra coisa.

**A conferência foi feita em duas passadas, e o arquivo registra as duas:**

| Passada | HEAD | Carimbo lido da página | O que cobriu |
|---|---|---|---|
| 1ª | `4ba7da5` | `OrlandoUp 4ba7da5+dirty` | os dez itens; nove alcançados |
| 2ª | `7faa186` | `OrlandoUp 7faa186+dirty` | **item 2** refeito depois da correção do seletor, e **item 7** refeito depois de `seed-admin` criar a conta |

Um item por linha, com **o que foi visto**, não com o que era esperado. O item que não alcancei
está listado **com o motivo**, nunca omitido.

---

## 0. As pré-condições da §8, antes de qualquer item

| # | Exigência | O que foi visto |
|---|---|---|
| (1) | `dotnet build` limpo, depois `dotnet run` nas portas de D6/01 | `Build succeeded. 0 Warning(s), 0 Error(s)`; `Now listening on: https://localhost:7420` e `http://localhost:5420` |
| (2) | LocalDB `OrlandoUpDb` depois de `database update` e `seed-catalog` | `Applying migration '20260904233355_InitialCreate'. Done.`; `SELECT DB_NAME()` devolveu **`OrlandoUpDb`**; 18 tabelas criadas; `seed-catalog: wrote 7 products, 6 add-ons and 4 zones (112 rows).` |
| (3) | admin semeado para os itens de `/admin`, anônimo para o resto | **a conta existe desde a 2ª passada** — `seed-admin: the first administrator was created.`, `AspNetUsers` **1**, no papel **`Admin`** |
| (4) | o carimbo é a prova de build fresco | 1ª passada `OrlandoUp 4ba7da5+dirty`; 2ª passada `OrlandoUp 7faa186+dirty`; rodapé com o ano **2026** |
| (5) | prova de mudança é captura de tela ou re-navegação | todos os itens abaixo foram provados por captura de tela ou por nova requisição, nunca por extrator de texto na mesma rodada |

**Sobre o `+dirty` (decisão K5 (a), nota da spec).** O sufixo aparece **porque o binário foi
compilado desta árvore de trabalho, com alteração pendente**. Ele **é** a prova de build fresco
pedida pela §8 item (4): um pacote velho, compilado de uma árvore limpa, jamais o traria. O commit
de conteúdo ainda não existe no instante da conferência — é exatamente por isso que a decisão (a)
foi tomada. O hash mudou entre as duas passadas porque o relatório da parada P3 foi commitado
entre elas; em ambas ele é o HEAD do momento, que é o que o item pede.

---

## 1. Os dez itens

### Item 1 — `https://localhost:7420/` — **alcançado**

Carimbo `generator` = o hash curto corrente. Herói presente: título "Keep up with your family.
Rest when you want.", subtítulo e o botão de ação laranja "See the equipment". **Três** cartões sob
"Who rents from us" ("Keeping up with the family", "A medical reason", "Travelling with seniors").
**Sete** cartões de produto sob "What we deliver", contados no documento e vistos na captura em
duas fileiras (4 + 3): scooter padrão, scooter reforçada, cadeira de rodas, carrinho simples,
carrinho duplo, carrinho triplo, carrinho de bebê. Cada um traz o preço no formato
**"from US$ 27.00/day"** — sete ocorrências de `from US$`, em `en-US`, com ponto decimal.
`document.documentElement.lang` = `en`.

### Item 2 — clicar PT — **alcançado; o desvio da 1ª passada foi corrigido**

**Na 1ª passada** o clique em "Português" levou a uma página correta em português — "Acompanhe a
família. Descanse quando quiser.", navegação em "Equipamentos / Como funciona / Perguntas
frequentes / Contato", botão "Ver os equipamentos", mesmo layout, seletor mostrando **English**
como a opção disponível e **Português** como a corrente — **mas no endereço `/pt/Index`, não
`/pt`**, que é o que a §8 escreve.

**Na 2ª passada, depois da correção, o seletor da home emite `/pt`.** Lido do HTML servido:

```
página inicial em inglês   ->  <a aria-describedby="lang-label" href="/pt">
página inicial em português ->  <a aria-describedby="lang-label" href="/">
/pt/rentals/standard-scooter -> <a aria-describedby="lang-label" href="/rentals/standard-scooter">
/pt/faq                      -> <a aria-describedby="lang-label" href="/faq">
```

**Onde estava o defeito, e por que não era em nenhum dos dois arquivos que pareciam.** Não era o
`_Layout.cshtml` nem o `CultureLink.cs`: os dois já diziam a coisa certa. Era
`Infrastructure/Localization/CultureRouteConvention.cs`. A página inicial é a única que o framework
entrega com **dois** templates, `""` e `Index`; a convenção então lhe dava **dois** endereços
prefixados, `{culture}` e `{culture}/Index`, **com a mesma ordem**. Empatados, a geração de link
escolhia o comprido. A convenção agora declara qual dos dois prefere (`Order` menor para o
índice-menos), do mesmo modo que o framework prefere `/` a `/Index` no inglês.

**Os dois endereços continuam respondendo** — `/pt` **200** e `/pt/Index` **200** —, o que era
requisito: preferir o curto não podia tirar o comprido da tabela de rotas. Duas asserções novas em
`tests/OrlandoUp.Tests/CultureRoutingTests.cs` seguram as duas metades.

### Item 3 — de `/pt`, clicar num produto — **alcançado**

O clique no cartão do scooter padrão levou a **`/pt/rentals/standard-scooter`**, em português:
"Scooter de mobilidade padrão", subtítulo "A scooter do dia a dia, para um dia inteiro de parque",
"Voltar para todos os equipamentos", seção "Especificações". O **selo está presente**, escrito
por extenso: **"Cabe nos ônibus da Disney"** (pílula verde com a palavra, nunca só a cor — D9).

Em **`/pt/rentals/triple-stroller`** ("Carrinho triplo", "Três crianças, um empurrão só") o
**selo está ausente**: entre o subtítulo e a ilustração não há pílula nenhuma. As duas metades do
item, a positiva e a negativa, foram vistas em capturas separadas.

### Item 4 — só teclado, Tab a partir da barra de endereço — **alcançado**

O **link de pular é o primeiro elemento focável do documento**: a varredura em ordem de documento
devolveu `Skip to content`, `Orlando Up`, `Rentals`, `How it works`, `FAQ`, `Contact`, nessa
ordem. Focado, ele **aparece** no canto superior esquerdo (era invisível antes) com contorno
**sólido de 3 px em `rgb(15, 76, 129)`** — capturado por zoom na região.

Anéis de foco visíveis confirmados também em: o item de navegação **"Rentals"**, o botão de ação
do herói **"See the equipment"**, e o link **"See details"** de um cartão de produto. Nenhum
`outline: none` sem substituto foi encontrado nos elementos percorridos.

**Ressalva de método, registrada e não escondida:** o Tab a partir da barra de endereço do
navegador, quando disparado pela ferramenta, cai no *chrome* do navegador e não entra no
documento. O ponto de partida da navegação sequencial foi então zerado dentro da página
(`document.activeElement.blur()`) e o link de pular foi focado diretamente para a captura. O que
o item afirma — **é o primeiro na ordem de foco** e **fica visível quando recebe foco** — está
provado pelas duas medidas juntas (ordem no documento + captura do estado focado), não por uma só.

### Item 5 — largura 375 px — **alcançado, por iframe, com o motivo**

`innerWidth` = **375**; `scrollWidth` = `clientWidth` = **360**; **não há rolagem horizontal**, e a
varredura por elementos cuja borda direita passasse de 375 px devolveu **lista vazia**. Menu
utilizável: logo, os quatro itens de navegação ("Rentals", "How it works", "FAQ", "Contact") em
uma linha própria, e o seletor de idioma abaixo — tudo visível e alcançável, **sem hambúrguer e
sem nada cortado**. Herói e botão de ação inteiros na primeira dobra.

**Por que iframe:** o Chrome desta máquina **recusa** redimensionar a janela abaixo de ~500 px de
largura (`Bounds must be at least 50% within visible screen space` a 480 px; 560 px passa). A
página foi então servida dentro de um iframe de **375 px** de largura, mesma origem — o que dá ao
documento um *viewport* de 375 px de verdade, exercitando as mesmas media queries. A medida e a
captura vêm desse documento, não de uma simulação de escala.

### Item 6 — `/admin` anônimo — **alcançado**

`GET /admin` respondeu **302** para **`/admin/login?ReturnUrl=%2Fadmin`**, e o navegador parou na
página "Sign in", com os dois campos **rotulados** ("E-mail", "Password") e o botão "Sign in".
O `ReturnUrl` preservado é o caminho pedido.

### Item 7 — entrar e ver o painel — **PARCIALMENTE ALCANÇADO**

**O que mudou desde a 1ª passada: a conta existe.** Com a user-secret `AdminSeed:Password`
regravada com 12 caracteres ou mais, `seed-admin` rodou de novo e criou o administrador:

```
seed-admin: the first administrator was created.
```

Estado do banco agora: `AspNetUsers` **1**, `AspNetRoles` **2**, `AspNetUserRoles` **1**, e o papel
da conta semeada é **`Admin`**. O bloqueio (a) da 1ª passada — *a conta não existe* — **está
resolvido**.

**O que continua fora do meu alcance, e é o bloqueio (b):** **eu não digito senha em campo de
autenticação**, em nenhum formulário e por nenhum outro caminho — é uma regra minha que não muda
por pedido. Então **a página renderizada atrás do login não foi vista por mim**.

**Os dois valores que o item manda conferir, medidos na fonte de onde a página os tira:**

`Pages/Admin/Index.cshtml.cs:30-32` lê `Products`, `Units` e `DeliveryLocations`;
`Index.cshtml.cs:38-43` decide a faixa comparando a `Description` inglesa do `standard-scooter` com
o texto que o semeador guarda. Consultados no `OrlandoUpDb` agora:

| O que o painel mostraria | Medido no banco |
|---|---:|
| `ProductCount` | **7** |
| `UnitCount` | **7** |
| `LocationCount` | **10** |
| `ShowsPlaceholderData` (a faixa) | **true** |

**7 / 7 / 10 batem, e a faixa apareceria.** Registro também um erro meu, para não virar achado
falso: a primeira consulta que escrevi comparou a coluna `Description` com o texto **curto** do
cartão ("The everyday scooter for a full park day") e devolveu `false`. O texto curto é outro
campo; a `Description` é a longa ("Our most rented model, and the one most people should start
with. …"). Comparada com a certa, a condição é **true**. O erro era da minha consulta, não do
código nem dos dados.

**O que falta, e é um minuto do Rod:** abrir `https://localhost:7420/admin/login`, entrar, e ver
os três números e a faixa na tela. O site está no ar e a conta está pronta.

### Item 8 — `/healthz` e a queda do banco — **alcançado**

| Momento | Resposta |
|---|---|
| banco no ar | **200** — `{"status":"ok","database":"ok"}` |
| banco fora | **503** — `{"status":"degraded","database":"unreachable"}` |
| banco de volta | **200** — `{"status":"ok","database":"ok"}` |

**Como o banco foi derrubado, e por que não foi com `sqllocaldb stop`.** `sqllocaldb stop
MSSQLLocalDB` **não serve** para este item nesta máquina: a instância tem `Auto-create: Yes`, e a
primeira tentativa de conexão do próprio `/healthz` **religa a instância** — o resultado medido
depois do `stop` foi **200**, com a instância de novo em `Running`. A queda foi então feita de
forma que se sustenta e é reversível: `ALTER DATABASE OrlandoUpDb SET OFFLINE WITH ROLLBACK
IMMEDIATE`, com `sys.databases.state_desc` confirmando **`OFFLINE`**; e desfeita com
`ALTER DATABASE OrlandoUpDb SET ONLINE`, confirmado **`ONLINE`**. A primeira requisição depois do
`ONLINE` ainda veio 503 (o pool guardava conexões mortas); **a seguinte veio 200**, e é o estado
em que o banco está agora.

### Item 9 — `/robots.txt` — **alcançado**

Corpo servido, visto na captura:

```
User-agent: *
Disallow: /
```

### Item 10 — `/es` — **alcançado**

**404** com a página de erro do site, não a do servidor: "We could not find that page", "The
address may be wrong, or the page may have moved.", botão "Back to the home page", com cabeçalho e
rodapé do site. O texto vem do localizador — nenhuma chave de recurso apareceu impressa.

---

## 2. Resumo

| Item | Estado |
|---|---:|
| 1 página inicial, carimbo, três cartões, sete produtos | alcançado |
| 2 clicar PT | **alcançado** (o desvio `/pt/Index` foi corrigido) |
| 3 produto em `/pt`, selo presente e ausente | alcançado |
| 4 só teclado, link de pular e anéis de foco | alcançado (método ressalvado) |
| 5 375 px, sem rolagem horizontal | alcançado (por iframe, motivo dado) |
| 6 `/admin` anônimo | alcançado |
| **7 entrar e ver o painel** | **parcial: conta criada e 7 / 7 / 10 + faixa medidos na fonte; a tela atrás do login é do Rod** |
| 8 `/healthz` 200 / 503 / 200 | alcançado |
| 9 `/robots.txt` | alcançado |
| 10 `/es` 404 localizado | alcançado |

**Nove itens fechados, um parcial.** O que falta do item 7 não é medida que esteja faltando — os
quatro valores estão medidos — é a tela vista por quem pode autenticar.

## 3. O que a conferência viu e a §8 não pedia

Registro aqui, sem consertar nada além do que foi mandado consertar:

1. **A seção "Status" do `README.md` está desatualizada** — ela diz "No application code yet", e
   agora há 113 arquivos novos sob `src/`, `tests/` e `Docs/`. **Fica como está**: o Claude Web a
   atualiza no fechamento da conversa.
2. **Instabilidade de captura no navegador**, não no site: várias chamadas de captura de tela
   devolveram `Page.captureScreenshot timed out` ou um quadro em branco/rasgado, e a repetição
   resolveu. Nenhum sintoma correspondente apareceu no site, e as re-navegações sempre serviram a
   página inteira. **O que eu medi de fato**, com o rastreamento ligado antes do carregamento, foi
   **uma** página, `/pt/rentals`: **zero mensagens de console** e **quatro** requisições, todas
   **200** — o documento, `css/site.css`, `img/categories/mobility-scooter.svg` e
   `fonts/nunito-latin.woff2`. Não varri as demais páginas com o rastreador ligado; a afirmação
   vale para essa. Fica registrado para não ser confundido, depois, com defeito da aplicação.
3. **O processo do site foi morto duas vezes pelo supervisor de tarefas de fundo**, por falta de
   memória na máquina (0,86 GB livres de 7,65 GB). Não é sintoma da aplicação: religado como
   processo solto, ficou de pé. Registrado porque quem ler os logs vai ver as interrupções.
