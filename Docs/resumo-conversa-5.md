# Resumo — conversa 5: as baterias entram na frota (leva 04b)

**Data:** 2026-09-09 a 12. **Par:** conversa 5 ↔ **leva 04b**. **Continua** o
`Docs/resumo-conversa-4.md` (leva 04, administração do catálogo).

**Faixa de commits:** `4af89fb..bfaeac2`, **12 commits**, nenhum empurrado *(verificado:
`git rev-list --left-right --count origin/main...main` lê `0 12`)*. A leva fechou nos dois commits
do ritual — conteúdo `afd9f43`, fechamento `bfaeac2`.

**A conversa 4 fechou.** `git log` mostra `4af89fb docs: resumo da conversa 4` e todo o trabalho
acima dele pertence a esta conversa *(verificado)*.

---

## 1. O que a leva 04b fechou, e por que ela veio antes da leva 03

O sistema conhecia **dez máquinas e mais nada**. A operação tem **doze baterias e catorze
carregadores**, e a conta da D36 é a razão de tudo: com quatro scooters e seis baterias por modelo,
**a bateria acaba antes da scooter** — no máximo dois de quatro clientes podem levar a segunda. Um
fluxo de reserva escrito sobre o modelo antigo venderia uma segunda bateria que não existe.

A leva 03 (reserva) precisa contar baterias desde a primeira linha. Acrescentá-las durante ela seria
mudança de schema no meio de uma frente de reserva; acrescentá-las agora faz a leva 03 herdar uma
frota que já sabe contar.

**O que existe agora** *(verificado em `bfaeac2`)*: as tabelas `Batteries` e `OperationalSettings`,
quatro telas novas (`/admin/batteries`, `.../create`, `.../edit/{id}`, `/admin/settings`), o comando
`seed-batteries`, duas contagens novas no painel, e o arquivo `Docs/controles/fleet-batteries.tsv`.
`git ls-files` conta **208** arquivos, **143** sob `src/`, **16** sob `tests/`.

---

## 2. Decisões do domínio — D36, D37, D38

| Decisão | O que fixou | Por quê |
|---|---|---|
| **D36** | a bateria é inventário próprio, presa ao **modelo** (`Product`), não à unidade; doze baterias, catorze carregadores; cada bateria tem etiqueta | uma bateria não pertence a uma scooter específica — elas se trocam entre máquinas do mesmo modelo |
| **D37** | **a XL é um ATRIBUTO, nunca uma opção de catálogo**; um pool por modelo; a segunda bateria é precificada por reserva (US$ 5 a 10, ou cortesia); carregador é **fungível**, contado e não listado; a multa é **por carregador**, editável | o cliente não escolhe a XL e não precisa saber que ela existe; qualquer carregador serve para qualquer bateria dos dois modelos |
| **D38** | etiqueta **`LLL-NN`** para a frota inteira — três letras, traço, dois dígitos; **QR, não código de barras**; carga nua na etiqueta, nunca URL; o leitor é frente própria **depois** da leva 03 | prefixos distintos tornam a etiqueta única na frota de graça; o leitor é o primeiro JavaScript de verdade numa árvore que não tem nenhum |

**As doze etiquetas semeadas:** `BSC-01` a `BSC-06` (Drive Scout, a `BSC-06` é a Extended Range — a
sobrevivente das duas XL) e `BSP-01` a `BSP-06` (Drive Spitfire, todas Normal).

**A grade não está na etiqueta.** `BSC-06` não diz que é a XL; quem diz é a coluna `Kind`. Uma
etiqueta identifica um objeto, uma coluna o descreve, e nenhum código lê tipo a partir de texto de
etiqueta. Isso é D38 e é o que permite trocar uma bateria sem reimprimir nada.

**A Q14 fechou** com a D36 e a D37. **A Q15 abriu:** política de dano e perda de bateria — um cliente
quebrou uma Drive Scout XL e não há política, enquanto o carregador já tem (US$ 30, editável). O
adicional `damage-waiver`, semeado a US$ 20 por aluguel, hoje **não promete nada específico**.

---

## 3. As quatro emendas, e o que cada uma pegou

O ciclo desta leva foi: plano → emenda → execução → emenda. Nenhuma correção nasceu por reescrita do
corpo da spec; todas são notas datadas no topo, cada uma terminando com o comando que prova que o
agente a leu.

**`EMENDA-04B-01`** respondeu o ponto aberto K1 (as etiquetas) antes de o plano pedir.

**`EMENDA-04B-02`** — revisão do plano P0, sete correções. Duas invertiam resultado:

- **a linha de configuração no host de teste.** O agente parou no lugar certo e diagnosticou com
  exatidão — a suíte monta o schema com `EnsureCreatedAsync` e **nunca roda migration**, então a
  linha que a migration cria não existe lá. E então recomendou uma guarda dentro do `seed-batteries`,
  que o host de teste não executa. **Diagnóstico certo, saída que não tocava nele.** A resposta foi
  nem (a), nem (b), nem (c): o `AdminCrudTests.cs` arranja a linha, e o teste 7 se parte em dois
  instrumentos — a suíte prova que a tela edita e nunca cria, a P1 e o roteiro provam que a migration
  criou a linha;
- **um controle verde medindo a coisa errada.** O rótulo prometia *"nenhuma bandeira de visibilidade
  nova nasce no domínio"*; o operando contava o literal `public bool IsActive`. Contra um `Domain/`
  com `public bool IsVisible`, escrito à mão fora do repositório, ele devolvia **0** — passava
  exatamente na entrada que existia para recusar. E o quinto já estava na árvore: `Product.IsBookable`.

As outras cinco: o comentário do `Program.cs` que passou de *two* a *three* comandos de seed; um
controle que **duplicava o C16 do `public-site`** como subconjunto estrito e caiu; o operando do C06
do `admin-catalog` que sobe a **9** e não a 10 (achado do próprio agente, e ele tinha razão); o
cardinal `Assert.Equal(13, keys.Count)` que vira **15**; e dois números que o plano afirmou sem medir.

**`EMENDA-04B-03`** — revisão da P1. A migration foi aprovada e aplicada. O achado veio de **ler o
seeder, não o relatório**: ele resolve o modelo por **posição** ordenada por `SortOrder`, e
`SortOrder` é **editável pelo editor de produto que a leva 04 construiu** — logo, quem está na posição
0 é fato do banco e ninguém tinha medido. Dois produtos podem até carregar o mesmo `SortOrder`, e aí
a ordem é indefinida. Como isso decide em qual máquina física vão as etiquetas `BSC`, a saída foi
dupla: o operador lê a ordem em `/admin/produtos` antes de semear, e o seeder ganha
`.ThenBy(product => product.Id)`.

**`EMENDA-04B-04`** — revisão da etapa 2, e o achado é a regra fundadora do projeto quebrada no
único lugar onde ninguém olha. O painel projetava a contagem de carregadores num `int` **não
anulável** com `FirstOrDefaultAsync`: linha ausente virava **`0`**, e a tela dizia *"0 carregadores"*
ao lado de *"12 baterias"* — afirmação sobre a frota, não sobre uma linha que falta. O comentário
ao lado assumia a troca em voz alta (*"mostra zero em vez de estourar"*), e a troca estava errada:
a terceira saída é **mostrar a ausência**, e a convenção já existia uma tela ao lado
(`Admin_NotSet`, `class="todo"`).

---

## 4. A descoberta de método desta conversa

**O rótulo de um controle nomeia uma CLASSE; o operando conta um MEMBRO dela.** Apareceu duas vezes,
nas duas pontas do mesmo ciclo:

| Controle | O rótulo promete | O operando conta | O membro que escapa |
|---|---|---|---|
| C07 proposto (leva 04b) | nenhuma bandeira de visibilidade nova | `public bool IsActive` | `IsBookable`, e qualquer `IsVisible` futuro |
| **C17 de `foundation.tsv`** (permanente desde a leva 01) | a ausência nunca vira zero | `?? 0` e `GetValueOrDefault(` | **`FirstOrDefault` projetado em tipo-valor** |

É mais difícil de ver que "o controle casa por sorte", porque o controle **mede certo** uma coisa
mais estreita do que promete — e o número de hoje está correto, então nada chama atenção.

**Duas provas, as duas baratas:** conte hoje a **classe inteira** (se o cardinal da classe for maior
que o do operando, já existe na árvore um membro invisível — foi assim que o quinto `public bool
Is…` apareceu), e rode a fórmula contra **um membro novo da classe, escrito à mão fora do
repositório**.

**O C17 não foi alargado, e isso é decisão, não omissão.** Uma fórmula que separasse projeção em
tipo-valor de projeção em tipo-referência pela forma é o tipo de esperteza que acaba verde-falsa:
medido, **onze dos doze** `FirstOrDefault*` do `src/` caem em entidade ou string e estão certos. O
instrumento virou **teste de tipo** — `Assert.Equal(typeof(int?), …PropertyType)` — porque um tipo é
barreira e um hábito não é.

---

## 5. A conferência visual, e o que ela custou de verdade

Dezessete itens, **nenhum `não fiz`**. Três precisaram de adjudicação, e **dois dos três achados são
defeito do roteiro, não do código**:

| Item | O que apareceu | Veredito |
|---|---|---|
| 6 | o campo é um dropdown com só os dois scooters | **não exercido pela tela, e por um motivo melhor** — a tela torna o gesto inalcançável; a recusa do servidor existe e está no teste |
| 7 | o Painel mostra 13 e não caiu quando uma bateria foi baixada | **ok** — o Painel conta o **total**, e a contagem de disponíveis está na página Baterias, onde caiu para 12. **O roteiro não nomeou a tela** |
| 16 | veio como *"fizemos antes"* | **retirado e refeito**, pelo revisor, medido |

**O item 7 é a mesma classe do "reservável" da leva 04**, e a terceira vez: a tela diz **"Baixada"**,
o roteiro dizia "aposentada" — palavra que a interface não usa.

---

## 6. O acesso ao navegador, que é o resultado de processo desta conversa

O `Docs/atrito-conversa-4.md` registrou, em *"fica para depois"*: **um caminho do revisor até o banco
e até o `dotnet`, que é atrito de acesso real e persistente — não entra agora porque não há solução
barata à vista.**

Metade dele caiu nesta conversa, e caiu em uma rodada. O item 16 foi medido no Chrome do Rod, com o
instrumento declarado:

```
/admin/batteries   scrollWidth 375 = clientWidth 375   a página NÃO rola de lado
                   87 elementos passam de 375 — os 87 dentro de div.table-scroll, nenhum escapa
/admin/settings    nenhum transbordo
campos             327 px de largura, 50–52 px de altura, nada cortado
```

**O viewport real foi conferido, não presumido:** a primeira medição devolveu **360**, porque a barra
de rolagem come 15 px de um iframe de 375. Foi corrigida e remedida. É exatamente o que "olhar a
375 px" não pega — e é a terceira conversa em que o *Fit to window* do simulador entra na conferência
prometendo uma largura que ele não entrega.

O instrumento foi um iframe de 375 na própria aba do operador, então nada na janela dele foi mexido.

**O que continua fora de alcance:** o `dotnet` e o banco. Toda contagem de linha e os controles C14 e
C15 desta conversa são **herdados do relato do agente**, não medidos pelo revisor. Um MCP de SQL
Server apontado para o `OrlandoUpDb` fecharia a outra metade — foi oferecido e não decidido.

---

## 7. Estado do portão

**54 controles em quatro arquivos** *(verificado em `bfaeac2`; os três primeiros remedidos pelo
revisor, o `foundation` com os dois de sempre fora por falta de `dotnet` no shell da ponte)*:

| Arquivo | Controles | Estado |
|---|---|---|
| `foundation.tsv` | 18 | 16 no alvo; C14 e C15 respondem 127 ao revisor — **herdados como verdes do agente** |
| `public-site.tsv` | 17 | no alvo |
| `admin-catalog.tsv` | 12 | no alvo, com o operando do C06 movido para **9** na linha existente |
| `fleet-batteries.tsv` | 7 | no alvo; **C02 e C04 saíram de `nao` para `sim`** — as metades de alcance realmente se moveram |

---

## Decisões permanentes

Já registradas em `Docs/decisions.md`: **D36**, **D37**, **D38**.

Regras de método que valem além desta frente, e ainda **não** estão em documento do repositório:

1. **Diagnóstico correto não garante saída correta.** Avalie separadamente a causa que o agente
   descreveu e o remédio que ele propôs.
2. **Rótulo que nomeia uma classe com operando que conta um membro** é verde-falso invisível; as duas
   provas estão na §4.
3. **Enunciado de teste com duas metades pede dois instrumentos.** A suíte não vê migration; o que
   depende de migration se prova na parada e no roteiro.
4. **Controle novo que é subconjunto estrito de um permanente** não acrescenta nada e dobra o arquivo.
5. **Linha do roteiro de conferência nomeia a TELA e usa a palavra que a interface mostra.** Terceira
   reincidência.

*(Já propostos como emenda à skill `revisao-plano-agente`; a proposta foi mostrada ao Rod e o estado
dela não foi reconferido — herdado.)*

---

## Pendências

**Do Rod — ação:**

- **empurrar os 12 commits** *(verificado: `0 12`)*;
- **aplicar as etiquetas `SCT-`, `SPT-` e `WCH-`** às dez unidades, à mão, pela tela da leva 04. Dez
  linhas, sem migration e sem código;
- imprimir e trocar as etiquetas físicas das doze baterias pelas `BSC-`/`BSP-` semeadas.

**Do Rod — decisão, e as três travam a leva 03:**

- **Q15** — o que custa uma bateria quebrada ou não devolvida, e se o `damage-waiver` de US$ 20 cobre
  isso. E, menor: existiu alguma Drive Spitfire XL?
- **Q6** — conta Stripe (chaves de teste destravam a fase 3);
- **Q3** — *(herdado do resumo 4, não reconferido nesta conversa)*.

**Backlog consciente:**

- **o Painel conta o total, não o disponível.** Coerente com "Unidades", que também é censo, então não
  é defeito hoje — mas a leva 03 reserva contra o **disponível**, e é lá que se decide se o Painel
  mostra os dois números;
- **a rodada de estética**, carregando os cinco itens não exercidos da conferência da leva 04 e os
  achados A4/A5/A6 *(herdado do resumo 4)*;
- **o leitor de QR**, frente própria depois da leva 03 (D38);
- **as etiquetas dos carregadores** (`CHG-01` a `CHG-14` na D38) existem no objeto físico mas **não**
  no sistema, que os conta (D3/04b). Não é esquecimento: nunca entraram, de propósito.

---

## Próximas frentes candidatas

1. **Leva 03 — reserva e disponibilidade.** É a frente natural: a frota agora sabe contar a peça que
   limita o dia. **Travada pela Q15 e pela Q6.**
2. **Rodada de estética.** Não depende de nenhuma pergunta aberta e limpa uma dívida de duas levas.
3. **Leitor de QR.** Depois da leva 03, por D38.

---

## Abertura da próxima conversa

1. **Você — ação:** empurrar.
   ```
   git push
   ```
2. **Você — decisão:** responder a **Q15** (`Docs/open-questions.md`), que é o que trava a leva 03.
   As duas perguntas estão escritas separadas lá, no formato que sobrevive.
3. **Você — decisão:** dizer se a **Q6** (Stripe) já tem conta, ou se ela vira ação sua antes da
   leva 03.
4. **Cole no Claude Web (conversa 6):**
   ```
   Conversa 6 — abrir a partir de Docs/resumo-conversa-5.md. A leva 04b fechou em
   afd9f43 e bfaeac2. Confira se a conversa 5 fechou, decida a frente e escreva a spec.
   ```
5. **Depois da spec revisada, e não antes — cole no Claude Code:** a linha nova de
   `Docs/fila-cc.md`, apontada **pela descrição**, nunca por "a linha aguardando".

---

*O projeto está com a frota inteira modelada — dez máquinas, doze baterias, catorze carregadores — e
com quatro telas de administração para mantê-la. O portão tem 54 controles em quatro arquivos e nada
fora do alvo que o revisor alcance. A próxima conversa abre na leva 03, e o que a trava não é código:
é a Q15.*
