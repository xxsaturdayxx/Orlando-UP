# Relatório — leva 02, etapa 2: as duas famílias de fonte (parada P2)

**Data:** 2026-09-06. **HEAD ao escrever:** `e9d537b`. **Spec:** `Docs/spec-02-public-site.md` com
as notas **`EMENDA-02-01`**, **`EMENDA-02-02`** e **`EMENDA-02-03`** no topo, que vencem o corpo
onde discordarem. **Linha da fila:** `2026-09-05`, "LEVA 02 — PUBLIC SITE", ainda `aguardando`.

**Etapas fechadas desde a P1:** E2 (migration aplicada) e E3 (fontes). **Nada foi empurrado.**

**O que esta parada pede:** conferir a procedência, a licença e a integridade dos quatro arquivos
de fonte antes de a E4 reescrever o `site.css` em cima deles — e ver a correção de ordem da §4,
que move o bloco destrutivo da spec para depois da E5.

---

## 1. E2 — a migration foi aplicada, e o que o banco diz

Executada depois do veredito "aplicar" da Revisão da P1. **Os metadados reais estão na §10 de
`Docs/relatorio-leva-02-etapa-1.md`**, acrescentada lá e commitada em `e9d537b`, acima da seção de
Revisão, como manda a `EMENDA-02-02` B6. Em uma linha cada:

- `WidthIn` e `LengthIn`: `IS_NULLABLE = YES`, `decimal(5,1)`, **sem** valor padrão;
- `IsBookable`: `bit NOT NULL`, padrão `(CONVERT([bit],(0)))`, restrição
  `DF__Products__IsBook__6E01572D`, **nomeada pelo sistema** e portanto diferente em cada banco
  (`EMENDA-02-03` C3);
- as sete linhas ficaram `IsBookable = 1` pelo `UPDATE` de preenchimento; Identity intocado;
- histórico: `InitialCreate` e `AddIsBookableAndOptionalDimensions`.

A suíte segue **69 passando** com o banco já migrado.

---

## 2. E3 — as quatro fontes

Todas de `fonts.gstatic.com`, que é de onde o CSS oficial de cada família as serve; os endereços
foram lidos do próprio `fonts.googleapis.com/css2` no passo 0, não adivinhados. Nenhuma passa a ser
buscada em tempo de execução: são arquivos do repositório, servidos pelo próprio site (D4/02).

| Arquivo | Bytes | Subconjunto | Origem |
|---|---:|---|---|
| `bricolage-grotesque-latin.woff2` | 76.888 | `latin` | `.../s/bricolagegrotesque/v9/3y9K6as8bTXq_nANBjzKo3IeZx8z6up5BeSl9D4dj_x9PpZBMlGIInE.woff2` |
| `bricolage-grotesque-latin-ext.woff2` | 30.736 | `latin-ext` | `.../s/bricolagegrotesque/v9/3y9K6as8bTXq_nANBjzKo3IeZx8z6up5BeSl9D4dj_x9PpZBMlGGInHEVA.woff2` |
| `manrope-latin.woff2` | 24.836 | `latin` | `.../s/manrope/v20/xn7gYHE41ni1AdIRggexSg.woff2` |
| `manrope-latin-ext.woff2` | 15.120 | `latin-ext` | `.../s/manrope/v20/xn7gYHE41ni1AdIRggmxSuXd.woff2` |

**São 147 KB somados** — contra 74,7 KB do Nunito, que era uma família só. O preço é a segunda
família; os quatro arquivos continuam carregando por `font-display: swap` e por subconjunto, então
uma página em inglês baixa só os dois `latin`, e é a portuguesa que puxa os acentuados.

**Ambas são fontes variáveis:** um arquivo por subconjunto carrega o eixo de peso inteiro
(500–800 no título, 400–700 no texto). Não há um arquivo por peso, como não havia no Nunito.

### 2.1 Integridade, conferida byte a byte e não pelo `curl` ter saído com 0

| Arquivo | Assinatura | `flavor` | Tabelas | Tamanho declarado no cabeçalho | Tamanho real |
|---|---|---|---:|---:|---:|
| `bricolage-grotesque-latin.woff2` | `wOF2` | `00010000` | 20 | 76.888 | 76.888 |
| `bricolage-grotesque-latin-ext.woff2` | `wOF2` | `00010000` | 20 | 30.736 | 30.736 |
| `manrope-latin.woff2` | `wOF2` | `00010000` | 19 | 24.836 | 24.836 |
| `manrope-latin-ext.woff2` | `wOF2` | `00010000` | 19 | 15.120 | 15.120 |

O campo de tamanho total do cabeçalho WOFF2 (offset 8, big-endian) bate com o tamanho do arquivo
nos quatro: **nenhum download foi truncado**. Nenhum dos quatro começa com HTML — é o modo comum de
um erro de CDN chegar como arquivo aparentemente válido e só falhar no navegador.

### 2.2 A licença

`wwwroot/fonts/OFL.txt` reescrito. O texto da SIL Open Font License 1.1 é **o mesmo** para as duas
famílias e continua reproduzido uma vez só, íntegro; o que mudou é o cabeçalho, agora com os dois
avisos de copyright, **copiados dos repositórios de origem** e não redigidos por mim:

```
Copyright 2022 The Bricolage Grotesque Project Authors (https://github.com/ateliertriay/bricolage)
Copyright 2018 The Manrope Project Authors (https://github.com/sharanda/manrope)
```

Lidos de `raw.githubusercontent.com/ateliertriay/bricolage/main/OFL.txt` e de
`raw.githubusercontent.com/google/fonts/main/ofl/manrope/OFL.txt` (o repositório do autor não serve
o arquivo de licença nesse caminho; o do Google Fonts serve, e é a mesma família). O cabeçalho
também lista os quatro arquivos e o que cada um cobre, para que a próxima pessoa não precise
adivinhar qual arquivo é de qual família.

O arquivo já **não nomeia a família aposentada**: `grep -ci nunito OFL.txt` = **0**.

---

## 3. O que os controles dizem agora

`bash Docs/medir-controles.sh medir scratchpad/leva02/public-site.tsv` — três se moveram nesta
etapa, e é a primeira vez que algum sai do valor inicial:

| Controle | No passo 0 | Agora | Alvo |
|---|---|---|---|
| C02 as quatro woff2 estão lá | as duas aposentadas | **as quatro novas + as duas aposentadas** | só as quatro |
| C07 a família aposentada não é mais nomeada | 8 | **6** | 0 |
| C08 ALCANCE — a varredura alcança a família de título | nao | **sim** | sim |

**As 6 ocorrências que restam estão todas em `wwwroot/css/site.css`** (4 `Nunito` nas regras
`@font-face` e na variável de tipografia, 2 `nunito` nos caminhos dos arquivos) — medido, não
suposto. Elas e os dois `.woff2` aposentados morrem na E4, que é a etapa que reescreve o estilo.
Nenhum dos outros doze controles mudou, o que é o esperado: nada nesta etapa tocou cor, marcação
ou endpoint.

---

## 4. Correção de ordem: o bloco destrutivo da §5.4 da spec sai da E2 e vai para depois da E5

Registrado também na §10 do relatório da etapa 1, e repetido aqui porque é uma mudança do plano
que você liberou.

O plano punha na E2 "aplicar a migration; bloco da §5.4 (apagar catálogo + `seed-catalog`);
contagens depois". **A segunda metade não podia rodar na E2.** O `CatalogSeedData.cs` ainda carrega
a frota placeholder da leva 01: apagar as sete linhas e rodar `seed-catalog` agora reinseriria
exatamente as mesmas sete, e as contagens que a §5.4 promete — 7 produtos e **10** unidades — só
podem existir depois de a E5 escrever a frota real (4 + 4 + 2 unidades). A ordem correta é:

> E5 escreve `CatalogSeedData.cs` e `CatalogSeeder.cs` → **então** roda o bloco da §5.4, com
> `SELECT DB_NAME()` antes, ordem de FK preservada e Identity intocado → contagens depois.

Entre a E2 e esse momento o site continua servindo o catálogo placeholder, que é o estado em que
ele já estava. Nada fica quebrado no meio.

---

## 5. O que está commitado, e o que ainda não

**Commitado nesta etapa:** os quatro `.woff2`, o `OFL.txt` reescrito e este relatório.

**Ainda não feito, de propósito:** os dois `nunito-*.woff2` continuam na pasta e o `site.css`
continua com os tokens e a tipografia da direção A. **Nada no site usa as fontes novas ainda** — um
navegador aberto agora renderiza exatamente o que renderizava antes. A troca é atômica na E4:
reescrever o estilo, redesenhar os três SVG e apagar os dois arquivos aposentados no mesmo passo,
para que não exista um estado intermediário em que o `@font-face` aponta para um arquivo que já não
está lá.

**Formas proibidas:** nada nesta etapa escreveu código. `?? 0`, `GetValueOrDefault(`,
`DateTime.Now`, `DateTime.Today`, `Migrate(` e `EnsureCreated(` seguem em 0 em `src/`; `UtcNow`
segue só em `Infrastructure/SystemClock.cs` e `using Markdig` só em `Application/RichText.cs`.

---

## 6. Uma decisão que preciso confirmar antes da E4

O `site.css` de hoje declara o Nunito com `font-weight: 400 700` e uma variável
`--font-heading` / `--font-body`. A `architecture.md` §12 pede **display de 84 px** no maior
tamanho de título. 84 px é uma escolha de canvas, e num telefone de 375 px de largura um título de
84 px quebra em duas ou três linhas com o `letter-spacing: -0.02em` pedido.

Vou implementar a escala do §12 como **escala de desktop**, com redução por `clamp()` no telefone —
o §12 diz "display 84/48/40/32/26/24 px **on desktop**", então leio isso como autorizado, e é o que
farei se você não disser o contrário. Estou registrando porque é a única leitura minha do §12 que
não é literal, e ela decide como o site aparece no aparelho em que a maior parte das pessoas vai
abri-lo.

---

## Revisão (Claude Web, )
