#!/usr/bin/env bash
# Docs/medir-controles.sh — gera e verifica a seção de controles de uma spec.
#
# Motivo: até a conversa 32 a tabela de controles de cada spec era DIGITADA. Quatro
# números errados numa única conversa (Epic Universe dado como 0 quando era 1; 2
# asteriscos quando eram 26; 9 imagens "OK" quando o casamento era 0 de 9). Nenhum
# deles sobreviveria a ter sido medido. Este script mede.
#
# Uso:
#   Docs/medir-controles.sh medir       Docs/controles/<frente>.tsv
#   Docs/medir-controles.sh verificar   Docs/controles/<frente>.tsv
#   Docs/medir-controles.sh quem-ancora <arquivo>...
#   Docs/medir-controles.sh proibidos   [<arquivo>]
#   Docs/medir-controles.sh autoteste
#
# DOIS GRUPOS DE MODO, e a diferença importa. Os dois primeiros MEDEM a árvore e
# comparam com a coluna Esperado; os três últimos CONSULTAM o acervo de .tsv e não
# medem nada — não abrem arquivo-alvo, não rodam comando de controle, e por isso
# saem sempre com 0 quando o acervo é legível. Só 'verificar' e 'autoteste' têm
# código de saída que significa reprovação.
#
# 'medir'       imprime a tabela markdown para colar na spec, e a versão do arquivo de
#               alvos com a coluna Esperado preenchida.
# 'verificar'   remede tudo e compara com a coluna Esperado. Sai com código 1 se
#               qualquer controle divergir — é isso que serve de portão no fim da frente.
# 'quem-ancora' para cada arquivo dado, lista quais controles de quais .tsv ancoram
#               nele. RODE ANTES de declarar escopo de .tsv numa spec: a afirmação
#               "nenhum .tsv existente muda" já foi escrita sem consultar nenhum, e o
#               passo 0 daquela frente derrubou oito controles de uma vez.
# 'proibidos'   o mesmo índice ao contrário: os padrões sob controle NEGATIVO, com
#               .tsv, alvo e rótulo. RODE ANTES de escrever comentário — comentário
#               casa grep, e isso já mordeu quatro vezes numa frente só. Com um
#               caminho como argumento, recorta para o que pode morder naquele arquivo.
# 'autoteste'   roda as barreiras de casamento de 'quem-ancora' e 'proibidos' contra
#               um acervo sintético, e sai com 1 se qualquer uma falhar. É o que
#               prende o comportamento desta ferramenta no portão, e o acervo é
#               gerado em diretório temporário — nenhum arquivo do repositório.
#
# Arquivo de alvos: TSV, uma linha por controle, campos separados por TAB.
#   tipo <TAB> alvo <TAB> padrao <TAB> rotulo [<TAB> esperado]
# Linha começando com # é comentário. Linha em branco é ignorada.
#
# TAB, e não barra vertical, porque barra vertical é separador de coluna em markdown
# e na fila de instruções — o mesmo motivo já registrado em Docs/fila-cc.md.
#
# Tipos:
#   linhas       wc -l do alvo. padrao ignorado (use -).
#   conta        nº de LINHAS que contêm o padrao (literal). = grep -cF
#   conta-re     idem, padrao é expressão regular estendida. = grep -cE
#   ocorr        nº de OCORRÊNCIAS do padrao (literal), inclusive várias na mesma
#                linha. = grep -oF | wc -l
#   presenca     sim/nao. É a FORMA IMUNE: use quando a própria spec insere o literal
#                contado, caso em que contagem bruta é controle inválido.
#   arquivos     nº de entradas no diretório alvo.
#   nome-exato   sim/nao: o padrao existe em `ls <alvo>` como nome EXATO, comparado
#                como texto. Use no lugar de test -e / [ -f ] / ls <caminho>, que dão
#                falso verde em sistema de arquivos que ignora caixa — o mount do
#                device_bash é um deles.
#   cmd          escotilha: roda o padrao como comando e usa a primeira linha da
#                saída. alvo é só rótulo de contexto (use -).
#   sem-comentario     como 'conta', mas contando SÓ NO CÓDIGO: as linhas de comentário
#                do alvo são removidas antes de contar. É a resposta à classe de defeito
#                "o controle estava certo e a prosa é que estava errada" — comentário
#                casa grep, e um comentário que cita um identificador desloca o controle
#                que conta aquele identificador, inclusive de frente alheia já fechada.
#   sem-comentario-re  idem, padrao é expressão regular estendida.
#
#                OPT-IN: nenhum controle existente muda de tipo, e 'conta'/'conta-re'
#                seguem contando o arquivo inteiro. Escolha este quando o alvo tiver
#                comentário que possa citar o que você conta.
#
#                EXTENSÕES COBERTAS, e a extensão do alvo é a ÚNICA coisa que decide:
#                  .cs .js      comentário de linha // e bloco /* */
#                  .css         bloco /* */
#                  .cshtml      linha // e blocos /* */, @* *@, <!-- -->
#                  .sh          linha #
#                  .tsv         linha # na COLUNA ZERO, como o parser deste script
#                QUALQUER OUTRA EXTENSÃO É RECUSA, e diretório também. É deliberado:
#                fingir cobertura devolveria o mesmo número que 'conta' — verde
#                idêntico, com a falsa impressão de estar protegido.
#
#                QUATRO VALORES DE RECUSA, e nenhum deles é um número:
#                  ALVO-AUSENTE          o alvo não está lá (igual aos outros tipos)
#                  EXTENSAO-NAO-COBERTA  o tipo não sabe o que é comentário ali
#                  FILTRO-ZEROU          o alvo tinha linhas e o filtro removeu todas
#                  BLOCO-ABERTO          o alvo termina dentro de um comentário de bloco
#                Os dois últimos existem para que um zero de contagem e um filtro que se
#                perdeu deixem de ser indistinguíveis.
#
#                LIMITE DECLARADO: comentário no FIM de uma linha de código NÃO é
#                removido — a linha conta inteira. Distinguir um marcador de comentário
#                de um marcador dentro de uma cadeia de texto exige analisador de
#                verdade, e heurística que erra em silêncio é pior que limite declarado.
#                Para esse resíduo, o modo 'proibidos' continua sendo a barreira.
#
# ---FIM-DO-USO---
# conta E ocorr NÃO SÃO A MESMA COISA, e a diferença já custou uma divergência: o
# controle "<strong> em Pages/Transfers.cshtml vai a 6" da conversa 32 dá 3 por 'conta'
# (são 3 linhas) e 6 por 'ocorr' (são 6 marcas). Escolher o tipo é declarar o que a
# spec quis dizer — em texto corrido essa escolha ficava implícita e ninguém conferia.

set -uo pipefail

modo="${1:-}"
alvos="${2:-}"

DIR_CONTROLES="${DIR_CONTROLES:-Docs/controles}"

# ⚠️ O TEXTO DE USO É DELIMITADO POR SENTINELA, NUNCA POR NÚMERO DE LINHA. Até
# 20/08/2026 isto era `sed -n '2,40p'`: um intervalo literal, que é contagem absoluta
# congelada de um recurso que qualquer edição do cabeçalho move — a mesma classe que o
# protocolo já expulsou dos controles. Acrescentar uma linha de ajuda passava a truncar
# a ajuda, EM SILÊNCIO, que é o pior sintoma possível num texto de socorro. E a função
# existe para o padrão morar num lugar só: a versão de rascunho desta frente chegou a
# ter três cópias do intervalo, uma por ramo do case abaixo.
uso() {
  sed -n '2,/---FIM-DO-USO---/p' "$0" | sed -e '/---FIM-DO-USO---/d' -e 's/^# \{0,1\}//'
  exit 2
}

case "$modo" in
  medir|verificar)
    [[ -z "$alvos" ]] && uso
    if [[ ! -f "$alvos" ]]; then
      echo "ERRO: arquivo de alvos não encontrado: $alvos" >&2
      exit 2
    fi
    ;;
  quem-ancora)
    [[ -z "$alvos" ]] && uso
    ;;
  proibidos|autoteste)
    ;;
  *)
    uso
    ;;
esac

if [[ ! -d .git ]]; then
  echo "ERRO: rode a partir da raiz do repositório (não achei .git)." >&2
  exit 2
fi

# --no-optional-locks: sem ele, `git status` cria .git/index.lock para atualizar o índice.
# Num mount onde o processo não tem permissão de unlink, o lock FICA e trava o commit
# seguinte do Claude Code. Descoberto em 13/08/2026, no primeiro teste deste script.
cabeca="$(git --no-optional-locks rev-parse --short HEAD)"
if [[ -n "$(git --no-optional-locks status --porcelain)" ]]; then
  arvore="COM ALTERAÇÕES NÃO COMMITADAS"
else
  arvore="limpa"
fi
hoje="$(date +%d/%m/%Y)"

# ============================================================================
# filtrar_sem_comentario — o filtro dos tipos 'sem-comentario' e 'sem-comentario-re'.
# Escreve o alvo SEM as linhas de comentário. Uma função só, consumida pelos dois
# tipos: duas cópias divergiriam, e a divergência seria silenciosa.
#
# A linha é DESCARTADA ou MANTIDA INTEIRA — nunca reescrita pela metade. É o que a
# D4/58 decidiu: comentário no fim de uma linha de código não é removido, e a linha
# continua contando inteira. Cirurgia dentro da linha exigiria saber o que é cadeia
# de texto, e heurística que erra em silêncio é pior que limite declarado.
#
# ⚠️ D8/58 — A PRECEDÊNCIA DENTRO DA LINHA, e ela inverte o resultado. O marcador de
# comentário de UMA LINHA é reconhecido PRIMEIRO: uma abertura de bloco que apareça
# depois dele, na mesma linha, NÃO abre bloco nenhum. Sem isto, TRÊS arquivos deste
# repositório (medidos em 26/08) derrubariam todo o resto do arquivo — os três citam
# um padrão de caminho ou um tipo de mídia dentro de um comentário de uma linha, e a
# barra seguida de asterisco vira abertura sem fechamento. O alvo não zera, só perde
# o fim: a recusa por zeramento NÃO pega, um controle afirmativo ficaria vermelho e
# um NEGATIVO ficaria VERDE afirmando ausência que não existe — a própria classe de
# falso verde que esta ferramenta existe para fechar.
#
# ⚠️ D9/58 — bloco ainda aberto no fim do alvo é RECUSA, nunca contagem. Se a máquina
# de estados chegou ao fim ainda dentro de um bloco, ela se perdeu, e o único desfecho
# seguro é dizer isso. Mesmo princípio da D5/58.
#
# ⚠️ D3/58 — a extensão é a ÚNICA coisa que decide, e extensão fora da tabela é RECUSA
# explícita. Diretório e arquivo sem extensão caem aqui também, e é deliberado: adivinhar
# a linguagem pela primeira linha acrescentaria superfície para atender três controles.
#
# ⚠️ R4/Nota 1 — para .tsv o comentário é reconhecido pela COLUNA ZERO, como o parser
# deste próprio script já faz. Sem isso a ferramenta discordaria de si mesma sobre o
# mesmo arquivo.
#
# Códigos de saída: 0 filtrou; 3 extensão não coberta; 4 bloco aberto ao fim.
filtrar_sem_comentario() {
  local alvo="$1" lc="" lc_col0="0" op="" cl=""
  # Diretório e qualquer coisa que não seja arquivo comum caem na recusa da D3/58: o
  # tipo não sabe ler aquilo, e dizer isso é melhor que devolver um número.
  [[ -f "$alvo" ]] || return 3
  case "$alvo" in
    *.cs|*.js) lc="//"; op="/*";         cl="*/"        ;;
    *.css)             op="/*";          cl="*/"        ;;
    *.cshtml)  lc="//"; op="/* @* <!--"; cl="*/ *@ -->" ;;
    *.sh)      lc="#"                                   ;;
    *.tsv)     lc="#"; lc_col0="1"                      ;;
    *)         return 3                                 ;;
  esac

  awk -v LC="$lc" -v LC0="$lc_col0" -v OP="$op" -v CL="$cl" '
    BEGIN { nop = split(OP, ab, " "); split(CL, fe, " ") }
    {
      linha = $0; manter = 0; cur = 1
      while (1) {
        resto = substr(linha, cur)
        if (dentro) {
          p = index(resto, fecha_atual)
          if (p == 0) break
          cur = cur + p - 1 + length(fecha_atual); dentro = 0
          continue
        }
        melhor = 0; especie = ""; qual = 0
        if (LC != "") {
          p = index(resto, LC)
          if (p > 0 && (LC0 != "1" || cur + p - 1 == 1)) { melhor = p; especie = "L" }
        }
        for (i = 1; i <= nop; i++) {
          p = index(resto, ab[i])
          if (p > 0 && (melhor == 0 || p < melhor)) { melhor = p; especie = "B"; qual = i }
        }
        if (melhor == 0) {
          # Nada de comentário daqui em diante. A linha fica se ainda houver texto,
          # ou se nunca passamos por comentário nenhum (linha em branco entra aqui).
          if (cur == 1 || resto ~ /[^ \t]/) manter = 1
          break
        }
        antes = substr(resto, 1, melhor - 1)
        if (antes ~ /[^ \t]/) manter = 1
        if (especie == "L") break
        dentro = 1; fecha_atual = fe[qual]
        cur = cur + melhor - 1 + length(ab[qual])
      }
      if (manter) print linha
    }
    END { if (dentro) exit 4 }
  ' "$alvo"
}

medir_um() {
  local tipo="$1" alvo="$2" padrao="$3" n
  case "$tipo" in
    linhas)
      [[ -f "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      wc -l < "$alvo" | tr -d ' '
      ;;
    conta)
      # grep -c IMPRIME 0 e SAI COM 1 quando não casa. Um `|| echo 0` aqui imprime
      # o zero duas vezes e a célula da tabela sai com duas linhas. Capturar, não encadear.
      [[ -f "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      n="$(grep -cF -- "$padrao" "$alvo" 2>/dev/null)" || true
      echo "${n:-0}"
      ;;
    conta-re)
      [[ -f "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      n="$(grep -cE -- "$padrao" "$alvo" 2>/dev/null)" || true
      echo "${n:-0}"
      ;;
    sem-comentario|sem-comentario-re)
      # Os dois tipos, um ramo só: eles diferem apenas em literal vs expressão regular,
      # e a diferença mora na escolha da opção do grep, não em duas cópias do filtro.
      #
      # QUATRO valores de recusa, e cada um diz uma coisa DIFERENTE. Nenhum deles é um
      # número, e é isso que os torna úteis: um zero de contagem e um filtro que se
      # perdeu deixam de ser indistinguíveis (D5/58, e o mesmo raciocínio na D9/58).
      #
      # ⚠️ -e E NÃO -f, e a diferença é o que separa duas recusas. Um DIRETÓRIO existe,
      # então chamá-lo de ausente seria mentira; ele cai na recusa da D3/58, que diz a
      # verdade — o tipo não sabe ler aquilo. Ausente é só o que não está lá.
      [[ -e "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      local filtrado rc opcao
      filtrado="$(filtrar_sem_comentario "$alvo")"; rc=$?
      case "$rc" in
        3) echo "EXTENSAO-NAO-COBERTA"; return ;;
        4) echo "BLOCO-ABERTO"; return ;;
      esac
      # D5/58: o alvo tinha linhas e o filtro removeu TODAS. Pode ser um arquivo
      # legitimamente todo em comentário, e pode ser o filtro quebrado — os dois pedem
      # olho humano, e nenhum dos dois é um zero de contagem.
      if [[ -s "$alvo" && -z "$filtrado" ]]; then echo "FILTRO-ZEROU"; return; fi
      if [[ "$tipo" == "sem-comentario" ]]; then opcao="-cF"; else opcao="-cE"; fi
      # Capturar, não encadear — mesmo motivo do 'conta' acima.
      n="$(printf '%s\n' "$filtrado" | grep "$opcao" -- "$padrao" 2>/dev/null)" || true
      echo "${n:-0}"
      ;;
    ocorr)
      [[ -f "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      grep -oF -- "$padrao" "$alvo" 2>/dev/null | wc -l | tr -d ' '
      ;;
    presenca)
      [[ -f "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      if grep -qF -- "$padrao" "$alvo" 2>/dev/null; then echo "sim"; else echo "nao"; fi
      ;;
    arquivos)
      [[ -d "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      ls -A "$alvo" | wc -l | tr -d ' '
      ;;
    nome-exato)
      [[ -d "$alvo" ]] || { echo "ALVO-AUSENTE"; return; }
      if ls -A "$alvo" | grep -qxF -- "$padrao"; then echo "sim"; else echo "nao"; fi
      ;;
    cmd)
      # Capturar, não encadear — mesmo motivo do 'conta' acima, e agravado pelo
      # `set -uo pipefail` do topo deste arquivo.
      #
      # A forma antiga era `eval "$padrao" 2>/dev/null | head -1 || echo "ERRO"`. Um
      # comando cuja convenção de saída é "1 = não achei" (`grep`, `git grep`, `diff`)
      # sai com 1 justamente quando o resultado é ZERO — que é precisamente o que um
      # controle NEGATIVO quer medir. O pipefail propagava esse 1, o `||` disparava, e
      # "ERRO" saía DEPOIS de o valor já ter sido impresso: a célula virava duas linhas
      # ("0" e "ERRO"), nunca casava com um esperado de uma linha, e a linha do arquivo
      # de alvos regenerado pelo 'medir' ganhava uma quebra no meio — TSV malformado.
      # Controle permanentemente vermelho é pior que controle nenhum: ensina a afrouxar.
      #
      # Regra agora, como o backlog pediu: HOUVE SAÍDA é resultado legítimo, qualquer que
      # tenha sido o código de saída; "ERRO" fica reservado para AUSÊNCIA de saída.
      # Mordeu duas frentes antes de ser consertado — a da parede (2026-08-15, contorno
      # `|| true` no .tsv) e a carteira de clientes (2026-08-16, mesmo contorno).
      n="$(eval "$padrao" 2>/dev/null | head -1)"
      echo "${n:-ERRO}"
      ;;
    *)
      echo "TIPO-DESCONHECIDO"
      ;;
  esac
}

comando_de() {
  local tipo="$1" alvo="$2" padrao="$3"
  case "$tipo" in
    linhas)     echo "wc -l $alvo" ;;
    conta)      echo "grep -cF '$padrao' $alvo" ;;
    conta-re)   echo "grep -cE '$padrao' $alvo" ;;
    sem-comentario)    echo "filtrar_sem_comentario $alvo | grep -cF '$padrao'" ;;
    sem-comentario-re) echo "filtrar_sem_comentario $alvo | grep -cE '$padrao'" ;;
    ocorr)      echo "grep -oF '$padrao' $alvo | wc -l" ;;
    presenca)   echo "grep -qF '$padrao' $alvo" ;;
    arquivos)   echo "ls -A $alvo | wc -l" ;;
    nome-exato) echo "ls -A $alvo | grep -qxF '$padrao'" ;;
    cmd)        echo "$padrao" ;;
    *)          echo "-" ;;
  esac
}

# Barra vertical dentro de célula parte a tabela markdown em colunas fantasma.
escapar() { printf '%s' "$1" | sed 's/|/\\|/g'; }

# ⚠️ CRLF. Este repositório é clonado no Windows e o .gitattributes não fixa `eol`
# para .tsv, então o git materializa o arquivo de alvos com CRLF. O `\r` fica grudado
# no ÚLTIMO campo lido — que é justamente `esperado` —, "1" deixa de ser igual a
# "1\r", e o 'verificar' reprova TODOS os controles de uma vez com a mensagem
# `esperado=1 hoje=1`. Ou seja: a saída acusa o alvo, quando o defeito é do arquivo,
# e a leitura natural é "os alvos estão todos errados".
#
# Não mordeu antes por acidente: os .tsv desta árvore foram escritos com LF e nunca
# tinham passado por um `git checkout`. Numa clonagem nova o portão triplo inteiro
# ficaria vermelho no primeiro dia. Descoberto em 16/08/2026, ao consertar o `cmd`.
descarnar_cr() {
  tipo="${tipo:-}";         tipo="${tipo%$'\r'}"
  alvo="${alvo:-}";         alvo="${alvo%$'\r'}"
  padrao="${padrao:-}";     padrao="${padrao%$'\r'}"
  rotulo="${rotulo:-}";     rotulo="${rotulo%$'\r'}"
  esperado="${esperado:-}"; esperado="${esperado%$'\r'}"
}

# ============================================================================
# MODOS DE CONSULTA — quem-ancora, proibidos e autoteste
#
# Origem: Docs/atrito-conversa-48.md §1 e Parecer (quem-ancora) e
# Docs/atrito-conversa-47.md §2 (proibidos). Os dois nasceram do mesmo diagnóstico —
# atrito de ACESSO, não de disciplina: a informação existe, espalhada por quinze .tsv
# e centenas de controles, e só aparece quando quebra.
#
# ⚠️ NENHUMA SUBSHELL DENTRO DO LAÇO, e isto é requisito, não estilo. A primeira
# versão desta frente chamava função por `$( )` uma vez por controle; com 454
# controles são ~1.000 forks, e fork em MSYS/Windows custa milissegundos: a consulta
# levava 28 SEGUNDOS, medidos. Ferramenta de consulta que demora meio minuto não é
# consultada — vira exatamente o atrito de acesso que ela existe para tirar. Tudo
# abaixo usa expansão de parâmetro e variável global, nunca captura de saída.
# Medido depois da reescrita: ~1,1 s.
# ============================================================================

# Caminho na forma canônica do repositório: relativo à raiz, barra normal, sem './'
# na frente e sem barra no fim.
#
# ⚠️ NÃO muda a caixa e NÃO resolve link. O mount do Windows ignora caixa, e casar
# 'pages/' com 'Pages/' esconderia exatamente o erro que o tipo 'nome-exato' existe
# para pegar — ver a descrição dele no cabeçalho.
CANON=""
canonizar_em() {
  CANON="${1//\\//}"
  while [[ "$CANON" == ./* ]]; do CANON="${CANON#./}"; done
  CANON="${CANON%/}"
}

# Devolve em KIND o tipo de ancoragem de um controle num arquivo, ou vazio:
#   alvo-exato      o campo alvo é o próprio arquivo
#   alvo-pasta      o campo alvo é uma pasta que contém o arquivo
#   comando-nomeia  o comando de um 'cmd' cita o caminho inteiro
#   comando-varre   o comando de um 'cmd' varre uma pasta que o contém
#
# ⚠️ AS QUATRO BARREIRAS CONTRA FALSO POSITIVO DE SUBSTRING, e cada uma mata um caso
# real do acervo. A semana de 18-20/08/2026 teve três falsos positivos de substring
# (ItemOrigemId casando dentro de PropostaItemOrigemId; um `??` comido pelo printf;
# `onDelete:` casando Delete), e ferramenta de medição com regex errada é pior que
# ferramenta nenhuma — ela dá confiança.
#
#   1. Linha de comentário não entra no índice (ver carregar_indice).
#   2. SEGMENTO, NUNCA PREFIXO: a barra é obrigatória. 'Pages/Cot' não contém
#      'Pages/Cotacoes/X.cs', e 'Pages/Cotacoes' não contém 'Pages/CotacoesAntigas/'.
#   3. O campo ROTULO nunca é consultado. Três controles do acervo citam TotalCotacao
#      no rótulo sem ancorar nele; se o rótulo entrasse, a ferramenta devolveria como
#      dependência a menção de precedente que ela existe para não confundir com uma.
#   4. 'nome-exato' sobre pasta exige que o BASENAME bata: ele olha a pasta mas
#      pergunta por UM nome só, então um arquivo vizinho não move aquele controle.
#
# As quatro são exercitadas pelo modo 'autoteste'.
KIND=""
ancoragem_em() {
  local arquivo="$1" tipo="$2" alvo="$3" padrao="$4"
  local a base tok varre="" limpo
  KIND=""

  a="${alvo//\\//}"; while [[ "$a" == ./* ]]; do a="${a#./}"; done; a="${a%/}"

  if [[ -n "$a" && "$a" != "-" ]]; then
    if [[ "$arquivo" == "$a" ]]; then KIND="alvo-exato"; return 0; fi
    if [[ "$arquivo" == "$a"/* ]]; then
      if [[ "$tipo" == "nome-exato" ]]; then
        base="${arquivo##*/}"
        if [[ "$base" == "$padrao" ]]; then KIND="alvo-pasta"; return 0; fi
      else
        KIND="alvo-pasta"; return 0
      fi
    fi
  fi

  if [[ "$tipo" == "cmd" && -n "$padrao" ]]; then
    # Troca todo caractere que NÃO pode compor caminho neste repositório por espaço.
    #
    # ⚠️ É HEURÍSTICA, E A SAÍDA DIZ ISSO. Comando é texto livre, e não há como saber
    # com certeza quais tokens dele são caminho. O desenho erra para o lado de
    # MOSTRAR — um falso positivo visível custa uma olhada, e uma ancoragem que não
    # aparece custa uma frente inteira — e por isso o casamento por comando sai em
    # grupo separado, com o comando ao lado, para quem lê julgar.
    limpo="${padrao//[!A-Za-z0-9._\/-]/ }"
    for tok in $limpo; do
      while [[ "$tok" == ./* ]]; do tok="${tok#./}"; done
      tok="${tok%/}"
      [[ -z "$tok" || "$tok" == "-" ]] && continue
      if [[ "$arquivo" == "$tok" ]]; then KIND="comando-nomeia"; return 0; fi
      [[ "$arquivo" == "$tok"/* ]] && varre="1"
    done
    if [[ -n "$varre" ]]; then KIND="comando-varre"; return 0; fi
  fi

  return 1
}

# Um controle é NEGATIVO? Esperado 0 nos tipos que contam, 'nao' nos dois que
# respondem sim/nao.
#
# ⚠️ 'nome-exato nao' entra JUNTO com 'presenca nao': os dois afirmam que algo NÃO
# existe, e deixar um de fora faria a lista mentir por omissão justamente na classe
# que ela expõe. Há exatamente um caso hoje — o nome minúsculo de disney-1.jpg em
# teste-conversa-32.tsv — e ele é proibição real.
eh_negativo() {
  case "$1" in
    presenca|nome-exato) [[ "$2" == "nao" ]] ;;
    *)                   [[ "$2" == "0" ]] ;;
  esac
}

CURTO=""
encurtar_em() {
  local t="$1" n="${2:-64}"
  if [[ "${#t}" -le "$n" ]]; then CURTO="$t"; else CURTO="${t:0:$((n - 1))}…"; fi
}

# Índice completo, uma linha por CONTROLE, carregado UMA vez num array:
#   tsv <TAB> linha <TAB> tipo <TAB> alvo <TAB> padrao <TAB> rotulo <TAB> esperado
#
# ⚠️ LINHA DE COMENTÁRIO NÃO ENTRA — primeira barreira. Um .tsv que MENCIONA um
# arquivo numa linha '#' não ancora nele.
IDX=()
N_TSV=0
carregar_indice() {
  local f n tipo alvo padrao rotulo esperado
  IDX=(); N_TSV=0
  for f in "$DIR_CONTROLES"/*.tsv; do
    [[ -f "$f" ]] || continue
    N_TSV=$((N_TSV + 1))
    n=0
    while IFS=$'\t' read -r tipo alvo padrao rotulo esperado; do
      n=$((n + 1))
      descarnar_cr
      [[ -z "${tipo:-}" ]] && continue
      [[ "${tipo:0:1}" == "#" ]] && continue
      IDX+=("$f"$'\t'"$n"$'\t'"$tipo"$'\t'"${alvo:-}"$'\t'"${padrao:-}"$'\t'"${rotulo:-}"$'\t'"${esperado:-}")
    done < "$f"
  done
  if [[ "$N_TSV" -eq 0 ]]; then
    echo "ERRO: nenhum .tsv em $DIR_CONTROLES — a consulta não mediria nada." >&2
    exit 2
  fi
}

modo_quem_ancora() {
  local arquivo alvo_bruto tsv linha tipo alvo padrao rotulo esperado
  local reg achados total_geral=0 grupo_cmd nome r

  carregar_indice
  echo "Consulta a $DIR_CONTROLES em $hoje, HEAD $cabeca, árvore $arvore."
  echo "Acervo: $N_TSV arquivos .tsv, ${#IDX[@]} controles."
  echo "Rótulo não é consultado, e linha de comentário não entra no índice."

  for alvo_bruto in "$@"; do
    canonizar_em "$alvo_bruto"; arquivo="$CANON"
    echo
    echo "== $arquivo =="
    [[ -e "$arquivo" ]] || echo "   (aviso: não existe na árvore; a consulta é sobre o texto dos .tsv, e segue)"

    achados=0; grupo_cmd=""
    for reg in "${IDX[@]}"; do
      IFS=$'\t' read -r tsv linha tipo alvo padrao rotulo esperado <<< "$reg"
      ancoragem_em "$arquivo" "$tipo" "$alvo" "$padrao" || continue
      achados=$((achados + 1))
      nome="${tsv##*/}"
      if [[ "$KIND" == comando-* ]]; then
        encurtar_em "$rotulo" 58; r="$CURTO"
        encurtar_em "$padrao" 92
        grupo_cmd+="   $(printf '%-38s %-14s esp=%-4s' "$nome:$linha" "$KIND" "${esperado:--}") $r"$'\n'
        grupo_cmd+="      $CURTO"$'\n'
      else
        encurtar_em "$rotulo" 58
        printf '   %-38s %-10s %-10s esp=%-4s %s\n' "$nome:$linha" "$tipo" "$KIND" "${esperado:--}" "$CURTO"
      fi
    done

    if [[ -n "$grupo_cmd" ]]; then
      echo "   ---- por comando: nomeia o arquivo, ou varre uma pasta que o contém ----"
      echo "        (casamento heurístico sobre texto de comando — confira o comando)"
      printf '%s' "$grupo_cmd"
    fi

    # ⚠️ O CASO VAZIO É IMPRESSO, e não silenciado: "nada ancora aqui" é a resposta
    # que quem escreve spec precisa, e silêncio é indistinguível de ferramenta que
    # não rodou. É a regra da asserção de controle aplicada à própria ferramenta.
    if [[ "$achados" -eq 0 ]]; then
      echo "   NENHUM CONTROLE ANCORA NESTE ARQUIVO."
    else
      echo "   $achados controle(s)."
    fi
    total_geral=$((total_geral + achados))
  done

  echo
  echo "$total_geral ancoragem(ns) em $# arquivo(s), $N_TSV .tsv varridos."
}

modo_proibidos() {
  local filtro="" arquivo="" reg tsv linha tipo alvo padrao rotulo esperado
  local n=0 tsv_atual="" grupo_cmd="" nome r

  carregar_indice
  if [[ $# -gt 0 ]]; then canonizar_em "$1"; arquivo="$CANON"; filtro="1"; fi

  echo "Identificadores sob controle NEGATIVO em $DIR_CONTROLES — $hoje, HEAD $cabeca."
  echo "Acervo: $N_TSV arquivos .tsv, ${#IDX[@]} controles."
  if [[ -n "$filtro" ]]; then
    echo "Recorte: só o que pode morder em $arquivo."
  else
    echo "Sem recorte. Passe um caminho para ver só o que pode morder nele."
  fi
  echo "Escrever qualquer um destes padrões no alvo — INCLUSIVE EM COMENTÁRIO — faz o"
  echo "controle medir mais que zero. Consulte antes de escrever, não no portão."

  for reg in "${IDX[@]}"; do
    IFS=$'\t' read -r tsv linha tipo alvo padrao rotulo esperado <<< "$reg"
    eh_negativo "$tipo" "$esperado" || continue
    if [[ -n "$filtro" ]]; then
      ancoragem_em "$arquivo" "$tipo" "$alvo" "$padrao" || continue
    fi
    nome="${tsv##*/}"
    if [[ "$tipo" == "cmd" ]]; then
      encurtar_em "$rotulo" 58; r="$CURTO"
      encurtar_em "$padrao" 92
      grupo_cmd+="   $(printf '%-38s' "$nome:$linha") $r"$'\n'
      grupo_cmd+="      $CURTO"$'\n'
      n=$((n + 1)); continue
    fi
    if [[ "$tsv" != "$tsv_atual" ]]; then
      tsv_atual="$tsv"; echo; echo "-- $nome"
    fi
    encurtar_em "$alvo" 42; printf '   %-9s %-42s %s\n' "$tipo" "$CURTO" "$padrao"
    encurtar_em "$rotulo" 86; printf '   %-9s %-42s └ %s\n' "" "" "$CURTO"
    n=$((n + 1))
  done

  if [[ -n "$grupo_cmd" ]]; then
    echo
    echo "-- por comando (o padrão é um comando inteiro, não um identificador) --"
    printf '%s' "$grupo_cmd"
  fi

  echo
  if [[ "$n" -eq 0 ]]; then
    echo "NENHUM controle negativo casa o recorte."
  else
    echo "$n controle(s) negativo(s), de ${#IDX[@]} controles em $N_TSV .tsv."
  fi
}

# ============================================================================
# autoteste — as barreiras são COMPORTAMENTO, e comportamento se prende por
# execução, não por padrão de texto que apodrece.
#
# ⚠️ O ACERVO SINTÉTICO É GERADO EM DIRETÓRIO TEMPORÁRIO, e nenhum arquivo de
# fixture nasce no repositório. A emenda de 20/08/2026 AUTORIZAVA fixtures sob
# Docs/controles/fixture/, e a opção não foi exercida por um motivo medido: o
# controle 'arquivos Docs/controles/fixture' de teste-conversa-32.tsv espera
# EXATAMENTE 1 entrada ali, e qualquer arquivo ou subpasta que esta frente
# criasse o deixaria vermelho — num .tsv que a mesma linha da fila proíbe tocar.
# Achado rodando 'quem-ancora Docs/controles/fixture' com o protótipo, que é
# precisamente o uso para o qual o modo foi pedido.
# ============================================================================
# ⚠️ R3/Nota 1 — O TOTAL DE CASOS É CONTADO, NUNCA DIGITADO. Até 26/08/2026 o número
# saía como literal em dois lugares e nada contava os casos: acrescentar um caso
# esquecendo um dos dois literais deixava o script anunciando um total FALSO com código
# de saída ZERO. É a regra do projeto — asserção de alcance permanente é limiar ou
# relação, nunca cardinal — aplicada ao próprio medidor.
FALHAS=0
CASOS=0
_af() { # _af <nome> <esperado> <obtido>
  CASOS=$((CASOS + 1))
  if [[ "$2" == "$3" ]]; then
    printf '  PASS  %-58s %s\n' "$1" "$3"
  else
    printf '  FALHA %-58s esperado=%s obtido=%s\n' "$1" "$2" "$3"
    FALHAS=$((FALHAS + 1))
  fi
}

modo_autoteste() {
  local tmp acervo saida alvo
  tmp="$(mktemp -d)" || { echo "ERRO: mktemp -d falhou." >&2; exit 2; }
  # ⚠️ ASPAS DUPLAS, para o caminho ser expandido AGORA e não na hora do EXIT: `tmp` é
  # local desta função, e quando o trap dispara a função já retornou — com `set -u`,
  # a forma de aspas simples morre com "tmp: unbound variable" DEPOIS de imprimir o
  # resultado, deixando o diretório temporário para trás. Pego rodando.
  trap "rm -rf \"$tmp\"" EXIT
  acervo="$tmp/acervo"; mkdir -p "$acervo"

  # O acervo sintético. Cada linha existe para exercitar UMA barreira, e o nome do
  # rótulo diz qual. Os Esperado são sintéticos e nunca são medidos por este modo —
  # o que se testa aqui é o CASAMENTO, não a medição.
  {
    printf '# comentario que cita Pages/Cotacoes/TotalCotacao.cs e NAO deve ancorar\n'
    printf 'conta\tPages/Cotacoes/TotalCotacao.cs\tFOO\tT1 alvo exato\t3\n'
    printf 'arquivos\tPages/Cotacoes\t-\tT2 alvo pasta, tipo que conta a pasta inteira\t9\n'
    printf 'nome-exato\tPages/Cotacoes\tTotalCotacao.cs\tT3 nome-exato perguntando por ESTE arquivo\tsim\n'
    printf 'nome-exato\tPages/Cotacoes\tVizinho.cs\tT4 nome-exato perguntando por VIZINHO, nao ancora\tsim\n'
    printf 'conta\tPages/CotacoesAntigas/X.cs\tFOO\tT5 pasta de prefixo comum, nao ancora\t1\n'
    printf 'conta\tPages/Cotacoes/Total.cs\tFOO\tT6 prefixo do nome, nao ancora\t1\n'
    printf 'conta\tDocs/algum.md\tFOO\tT7 rotulo cita Pages/Cotacoes/TotalCotacao.cs e nao deve ancorar\t0\n'
    printf 'cmd\t-\techo Pages/Cotacoes/TotalCotacao.cs\tT8 comando NOMEIA o arquivo\tx\n'
    printf 'cmd\t-\tgrep -r FOO Pages | wc -l\tT9 comando VARRE a pasta\t0\n'
    printf 'presenca\tDocs/algum.md\tBAR\tT10 presenca negativa\tnao\n'
    printf 'nome-exato\tDocs\tnaoexiste.md\tT11 nome-exato negativo\tnao\n'
  } > "$acervo/sintetico.tsv"

  echo "autoteste — acervo sintético em diretório temporário, nenhum arquivo do repositório."
  echo

  alvo="Pages/Cotacoes/TotalCotacao.cs"
  saida="$(DIR_CONTROLES="$acervo" modo_quem_ancora "$alvo")"

  _af "1  alvo-exato encontrado (T1)"                 1 "$(grep -c 'T1 alvo exato' <<< "$saida")"
  _af "2  alvo-pasta por tipo 'arquivos' (T2)"        1 "$(grep -c 'T2 alvo pasta' <<< "$saida")"
  _af "3  nome-exato com basename batendo (T3)"       1 "$(grep -c 'T3 nome-exato perguntando por ESTE' <<< "$saida")"
  _af "4  BARREIRA 4: vizinho de nome-exato ausente"  0 "$(grep -c 'T4 nome-exato perguntando por VIZINHO' <<< "$saida")"
  _af "5  BARREIRA 2: pasta de prefixo comum ausente" 0 "$(grep -c 'T5 pasta de prefixo comum' <<< "$saida")"
  _af "6  BARREIRA 2: prefixo de nome ausente"        0 "$(grep -c 'T6 prefixo do nome' <<< "$saida")"
  _af "7  BARREIRA 3: rotulo nao ancora (T7)"         0 "$(grep -c 'T7 rotulo cita' <<< "$saida")"
  _af "8  comando-nomeia encontrado (T8)"             1 "$(grep -c 'comando-nomeia' <<< "$saida")"
  _af "9  comando-varre encontrado (T9)"              1 "$(grep -c 'comando-varre' <<< "$saida")"
  _af "10 total de ancoragens e exatamente cinco"     "5 ancoragem(ns) em 1 arquivo(s), 1 .tsv varridos." \
      "$(tail -1 <<< "$saida")"

  # BARREIRA 1, o comentário: ele cita o arquivo e o índice tem de ignorá-lo. Se
  # entrasse, o total acima seria 6 — por isso o par 10/11 é a forma de dois lados.
  _af "11 BARREIRA 1: comentario fora do indice"      1 "$(grep -c '1 arquivos .tsv, 11 controles' <<< "$saida")"

  saida="$(DIR_CONTROLES="$acervo" modo_quem_ancora "pages/cotacoes/totalcotacao.cs")"
  _af "12 caixa NAO e normalizada"                    1 "$(grep -c 'NENHUM CONTROLE ANCORA' <<< "$saida")"

  saida="$(DIR_CONTROLES="$acervo" modo_quem_ancora "./Pages/Cotacoes/TotalCotacao.cs")"
  _af "13 './' normaliza para o mesmo resultado"      "5 ancoragem(ns) em 1 arquivo(s), 1 .tsv varridos." \
      "$(tail -1 <<< "$saida")"

  saida="$(DIR_CONTROLES="$acervo" modo_quem_ancora "Docs/nada/aqui.md")"
  _af "14 caso vazio diz que e vazio"                 1 "$(grep -c 'NENHUM CONTROLE ANCORA' <<< "$saida")"

  saida="$(DIR_CONTROLES="$acervo" modo_proibidos)"
  _af "15 negativos: os quatro sinteticos, e so eles" "4 controle(s) negativo(s), de 11 controles em 1 .tsv." \
      "$(tail -1 <<< "$saida")"
  _af "16 'nome-exato nao' entra como negativo (T11)" 1 "$(grep -c 'T11 nome-exato negativo' <<< "$saida")"

  saida="$(DIR_CONTROLES="$acervo" modo_proibidos "Pages/Cotacoes/TotalCotacao.cs")"
  _af "17 recorte por arquivo filtra de verdade"      1 "$(grep -c 'T9 comando VARRE' <<< "$saida")"
  _af "18 e exclui o que nao morde naquele arquivo"   0 "$(grep -c 'T10 presenca negativa' <<< "$saida")"

  # ==========================================================================
  # F — o filtro de comentário dos tipos 'sem-comentario' e 'sem-comentario-re'.
  #
  # ⚠️ CADA CASO AQUI É METADE DE UM PAR, e nenhuma metade sozinha prova nada. Um
  # filtro que apagasse o arquivo inteiro passaria em todo caso "remove"; uma recusa
  # que disparasse sempre passaria em todo caso "recusa". É o par que discrimina.
  # ==========================================================================
  local fonte="$tmp/fonte"; mkdir -p "$fonte"

  printf 'var a = ALVO; // ALVO no fim da linha\n// ALVO so em comentario\n/* ALVO em bloco */\nvar b = ALVO;\n' > "$fonte/mix.cs"
  _af "F1 remove a linha que e so comentario"        2 "$(medir_um sem-comentario "$fonte/mix.cs" ALVO)"
  _af "F2 PAR DE F1: o bruto ve as quatro"           4 "$(medir_um conta          "$fonte/mix.cs" ALVO)"
  _af "F3 PAR DE F1: preserva codigo (so em codigo)" 2 "$(medir_um sem-comentario "$fonte/mix.cs" 'var ')"
  _af "F4 PAR DE F3: o bruto ve o mesmo em codigo"   2 "$(medir_um conta          "$fonte/mix.cs" 'var ')"

  # D4/58: comentario no FIM de uma linha de codigo nao e removido, e a linha conta
  # inteira. Limite declarado, nao omissao — e o caso existe para que uma frente
  # futura nao o "conserte" sem decidir.
  _af "F5 D4 nao remove comentario de fim de linha"  1 "$(medir_um sem-comentario "$fonte/mix.cs" 'no fim da linha')"

  # ⚠️ D8/58 — A PRECEDENCIA, e e o caso que inverte o resultado. A abertura de bloco
  # esta DENTRO de um comentario de uma linha: ela nao abre bloco, e o codigo que vem
  # depois sobrevive. Sem a precedencia, este alvo perderia todo o resto.
  printf '// cita Pages/*.cshtml em prosa\nvar depois = ALVO;\nvar mais = ALVO;\n' > "$fonte/d8.cs"
  _af "F6 D8 abertura dentro de comentario de linha" 2 "$(medir_um sem-comentario "$fonte/d8.cs" ALVO)"
  printf 'var antes = ALVO;\n/* bloco de verdade\nvar ALVO dentro = 1;\n*/\nvar fim = ALVO;\n' > "$fonte/d8par.cs"
  _af "F7 PAR DE F6: bloco de verdade AINDA abre"    2 "$(medir_um sem-comentario "$fonte/d8par.cs" ALVO)"

  # ⚠️ D9/58 — bloco aberto ao fim do alvo e RECUSA, nunca contagem.
  printf '/* nunca fecha\nvar x = ALVO;\n' > "$fonte/aberto.cs"
  _af "F8 D9 bloco aberto ao fim recusa"             BLOCO-ABERTO "$(medir_um sem-comentario "$fonte/aberto.cs" ALVO)"
  _af "F9 PAR DE F8: bloco fechado nao recusa"       2 "$(medir_um sem-comentario "$fonte/d8par.cs" ALVO)"

  # D3/58 — a extensao e a unica coisa que decide, e fora da tabela e recusa explicita.
  printf 'ALVO em documento\n' > "$fonte/doc.md"
  _af "F10 D3 extensao nao coberta recusa"           EXTENSAO-NAO-COBERTA "$(medir_um sem-comentario "$fonte/doc.md" ALVO)"
  _af "F11 PAR DE F10: extensao coberta nao recusa"  2 "$(medir_um sem-comentario "$fonte/mix.cs" ALVO)"
  _af "F12 PAR DE F10: diretorio tambem recusa"      EXTENSAO-NAO-COBERTA "$(medir_um sem-comentario "$fonte" ALVO)"

  # D5/58 — o filtro zerou o universo. Zero legitimo e filtro quebrado deixam de ser
  # indistinguiveis, e nenhum dos dois e um zero de contagem.
  printf '// tudo comentario\n// mesmo\n' > "$fonte/todo.cs"
  _af "F13 D5 filtro que zera recusa"                FILTRO-ZEROU "$(medir_um sem-comentario "$fonte/todo.cs" ALVO)"
  printf '// quase tudo comentario\nvar sobra = 1;\n' > "$fonte/quase.cs"
  _af "F14 PAR DE F13: sobrando uma linha nao recusa" 0 "$(medir_um sem-comentario "$fonte/quase.cs" ALVO)"

  _af "F15 alvo ausente diz ausente"                 ALVO-AUSENTE "$(medir_um sem-comentario "$fonte/naoexiste.cs" ALVO)"

  # Uma forma de bloco por linguagem da tabela da §4.
  printf '@* ALVO em bloco razor *@\n<!-- ALVO em bloco html -->\n/* ALVO em bloco c */\n<p>ALVO</p>\n' > "$fonte/m.cshtml"
  _af "F16 cshtml: as tres formas de bloco saem"     1 "$(medir_um sem-comentario "$fonte/m.cshtml" ALVO)"
  _af "F17 PAR DE F16: o bruto ve as quatro"         4 "$(medir_um conta          "$fonte/m.cshtml" ALVO)"
  printf '/* ALVO em bloco */\n.c { color: ALVO; }\n' > "$fonte/e.css"
  _af "F18 css: bloco sai, regra fica"               1 "$(medir_um sem-comentario "$fonte/e.css" ALVO)"

  # ⚠️ R4/Nota 1 — em .tsv o comentario e a COLUNA ZERO, como o parser deste script.
  # Um marcador no meio da linha e DADO, e removê-la apagaria controle de verdade.
  printf '# ALVO em comentario de coluna zero\nconta\tx.cs\tALVO\trotulo com # no meio\t1\n' > "$fonte/t.tsv"
  _af "F19 R4 tsv: coluna zero e comentario"         1 "$(medir_um sem-comentario "$fonte/t.tsv" ALVO)"
  _af "F20 PAR DE F19: marcador no meio e dado"      1 "$(medir_um sem-comentario "$fonte/t.tsv" 'no meio')"

  # O irmao de comando_de() devolve comando LEGIVEL, nunca '-' — sem ele a tabela da
  # spec sai com um traco no lugar do comando e o controle nao teria o que citar.
  _af "F21 comando_de responde ao tipo literal"      0 "$(comando_de sem-comentario a.cs P | grep -cx -- -)"
  _af "F22 comando_de responde ao tipo regex"        0 "$(comando_de sem-comentario-re a.cs P | grep -cx -- -)"
  _af "F23 PAR DE F21/F22: tipo inexistente da '-'"  1 "$(comando_de tipo-que-nao-existe a.cs P | grep -cx -- -)"

  echo
  if [[ "$FALHAS" -eq 0 ]]; then
    echo "autoteste: $CASOS casos, 0 falha."
    return 0
  fi
  echo "autoteste: $CASOS casos, $FALHAS FALHA(S)."
  return 1
}

case "$modo" in
  quem-ancora) modo_quem_ancora "${@:2}"; exit 0 ;;
  proibidos)   modo_proibidos   "${@:2}"; exit 0 ;;
  autoteste)   modo_autoteste;            exit $? ;;
esac

total=0
divergentes=0

if [[ "$modo" == "medir" ]]; then
  echo "<!-- Gerado por Docs/medir-controles.sh em $hoje, HEAD $cabeca, árvore $arvore."
  echo "     Nenhuma linha desta tabela foi escrita à mão. -->"
  echo
  echo "**Controles medidos na árvore viva em $hoje, HEAD \`$cabeca\` (árvore $arvore):**"
  echo
  echo "| Controle | Comando | Hoje |"
  echo "|---|---|---:|"
fi

while IFS=$'\t' read -r tipo alvo padrao rotulo esperado; do
  descarnar_cr
  [[ -z "${tipo:-}" ]] && continue
  [[ "${tipo:0:1}" == "#" ]] && continue
  padrao="${padrao:-}"
  rotulo="${rotulo:-$tipo $alvo}"
  esperado="${esperado:-}"
  [[ "$padrao" == "-" ]] && padrao=""

  valor="$(medir_um "$tipo" "$alvo" "$padrao")"
  cmd="$(comando_de "$tipo" "$alvo" "$padrao")"
  total=$((total + 1))

  if [[ "$modo" == "medir" ]]; then
    printf '| %s | `%s` | **%s** |\n' "$(escapar "$rotulo")" "$(escapar "$cmd")" "$valor"
  else
    if [[ -z "$esperado" ]]; then
      printf 'SEM-ESPERADO  %-52s  hoje=%s\n' "$rotulo" "$valor"
      divergentes=$((divergentes + 1))
    elif [[ "$valor" == "$esperado" ]]; then
      printf 'OK            %-52s  %s\n' "$rotulo" "$valor"
    else
      printf 'DIVERGE       %-52s  esperado=%s  hoje=%s\n' "$rotulo" "$esperado" "$valor"
      divergentes=$((divergentes + 1))
    fi
  fi
done < "$alvos"

if [[ "$modo" == "medir" ]]; then
  echo
  echo "<!-- $total controles. Arquivo de alvos com a coluna Esperado preenchida,"
  echo "     para o 'verificar' do fim da frente: -->"
  echo
  while IFS=$'\t' read -r tipo alvo padrao rotulo esperado; do
    descarnar_cr
    [[ -z "${tipo:-}" ]] && continue
    if [[ "${tipo:0:1}" == "#" ]]; then echo "$tipo${alvo:+	$alvo}"; continue; fi
    padrao="${padrao:-}"; [[ "$padrao" == "-" ]] && padrao=""
    valor="$(medir_um "$tipo" "$alvo" "$padrao")"
    printf '%s\t%s\t%s\t%s\t%s\n' "$tipo" "$alvo" "${padrao:--}" "${rotulo:-}" "$valor"
  done < "$alvos"
else
  echo
  echo "$total controles, $divergentes fora do esperado, HEAD $cabeca, árvore $arvore." 
  [[ $divergentes -gt 0 ]] && exit 1
fi

exit 0
