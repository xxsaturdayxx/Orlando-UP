# Conferência visual — leva 02

**Data:** 2026-09-07. **Spec:** `Docs/spec-02-public-site.md` §10, com as emendas `EMENDA-02-01` a
`EMENDA-02-05`. **Base:** `dotnet run --project src/OrlandoUp.Web`, LocalDB `OrlandoUpDb` depois do
bloco da §5.4, visitante anônimo salvo onde a linha diz o contrário.

**Prova de build fresco:** `<meta name="generator" content="OrlandoUp 926d6a6+dirty">`. O `+dirty`
**é** a prova (K5/01): a leva ainda não tem commit de conteúdo, então o hash é o do último commit e
o marcador diz que o que renderizou veio da árvore de trabalho.

**Como cada linha foi medida:** requisição ao site de pé e leitura do que voltou. Onde a asserção é
sobre aparência e não sobre conteúdo — cor percebida, tipografia, o encaixe a 375 px — a linha diz
**o que a folha de estilo garante** e marca o item como **seu**, porque um extrator de texto não vê
layout e a spec proíbe usar um no lugar de olhar.

| # | O que | Esperado | Medido | |
|---|---|---|---|---|
| 1 | `/` abre | cabeçalho navy, botão sol com texto navy, títulos Bricolage; nada laranja | a laranja aposentada aparece **0** vezes na home e na folha; `--color-navy`, `--color-sun` e a família de título estão na folha | ✅ |
| 2 | rolar a home | três cartões com preço, quatro "em breve" sem preço | **3** ocorrências de `from US$`, **4** pastilhas `badge--soon` | ✅ |
| 3 | clicar PT | português, e a banda de equipe diz o que a inglesa não diz | `equipe brasileira` aparece **2×** em `/pt` e **0×** em `/` | ✅ |
| 4 | `/rentals/drive-scout-4` | lista de especificações presente, badge de ônibus presente | `class="specs"` **1**, `badge--transport` **1** | ✅ |
| 4a | a ilustração aparece (`EMENDA-02-04` D5 / A15) | o SVG de categoria carrega e está na paleta v1 | `/img/categories/wheelchair.svg` **HTTP 200**, traço `#0B1F3F`, superfície `#FFC72C` | ✅ |
| 4b | `ImagePath` resolve quando existir | hoje nenhuma linha tem imagem, então a arte de categoria é o caminho exercido | `ImagePath` nulo nas 7 linhas; o outro ramo continua sem exercício até a D3/02 trazer imagem | ⚠️ parcial, e a razão está na spec |
| 5 | `/rentals/single-stroller` | pastilha, sem preço, sem tabela, sem adicionais, sem botão | `badge--soon` **1**, `US$` **0**, `button--disabled` **0** | ✅ |
| 6 | `/delivery-areas` e `/pt/delivery-areas` | quatro zonas, Disney primeiro, texto do banco | **4** blocos de zona em cada uma; a instrução de entrega em mãos renderiza (teste afirma 60 caracteres dela) | ✅ |
| 7 | teclado a partir da barra de endereço | skip link primeiro, anel de foco visível nos dois fundos | `skip-link` presente; `:focus-visible` definido; `outline: none` **0** vezes; o anel é token e vira sol dentro de painel escuro | ✅ mecanismo; **o passeio de Tab é seu** |
| 8 | largura 375 px | sem rolagem horizontal, menu usável, tabela rolando dentro do próprio contêiner | `clamp()` em **5** tamanhos; `auto-fit` em **4** grades; `.table-scroll` com `overflow-x: auto`; `--tap: 44px` usado em **4** lugares | ✅ mecanismo; **o telefone é seu** |
| 9 | `/admin` depois do login | contagens **7 / 10 / 10**, faixa de placeholder **sumida** | `/admin` anônimo → **302** para o login; a faixa não existe mais em lugar nenhum de `src/`; o banco tem 7 produtos, 10 unidades, 10 locais | ✅ dados; **o login é seu** |
| 10 | fonte de `/rentals/drive-scout-4` | um bloco `application/ld+json`, `Product` sem `offers`, nenhum `TODO-` | **1** bloco, `offers` **0**, `TODO-` **0**; o tipo sai literal, sem entidade | ✅ |
| 11 | `/sitemap.xml` | XML válido, duas culturas por página, produtos presentes, nada de `/admin` | **30** `<loc>` = 8 páginas × 2 + 7 produtos × 2; `admin` **0**; declara `encoding="utf-8"`, que é o que os bytes são | ✅ |
| 12 | `/robots.txt` | ainda fechado | `User-agent: *` / `Disallow: /` | ✅ |
| 13 | **Rod:** Lighthouse, categoria acessibilidade, em `/` e `/rentals` | ≥ 95 (portão da fase 2 do roadmap) | **não rodado** — precisa do Chrome DevTools; peso das páginas medido para contexto: `/` 11,8 kB, `/rentals` 10,0 kB, fonte de título 76,9 kB | ⬜ **seu** |
| 14 | `/es` | 404 localizado | **404** | ✅ |

## O que eu não consegui alcançar, e por quê

- **O item 13 é seu.** Lighthouse roda dentro do Chrome; daqui eu meço o que a página contém, não o
  que uma ferramenta de auditoria pontua. É o único portão do roadmap que ainda não tem número.
- **Os itens 7, 8 e 9 têm a metade mecânica verde e a metade humana aberta.** Eu provo que o anel de
  foco existe e nunca é removido, que a escala encolhe e que o alvo de toque é 44 px; nada disso
  prova que o Tab passa na ordem que faz sentido nem que o menu cabe na sua mão. Vale abrir o site
  no telefone e dar um passeio de Tab no desktop.
- **O item 4b fica parcial de propósito.** A D3/02 diz que nenhuma imagem entra nesta leva, então o
  ramo do `ImagePath` preenchido não tem como ser exercido aqui. O ramo do caminho vazio — a arte de
  categoria — está exercido e verde.
