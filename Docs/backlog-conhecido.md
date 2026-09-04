# Backlog conhecido — Orlando Up

Conhecimento que **não foi pedido como instrução**. Nunca entra em `Docs/fila-cc.md` — a fila é só
instrução recebida do operador. Item daqui vira frente quando o operador pedir, com spec.

| Registrado | Item | Motivo de existir | Onde foi visto |
|---|---|---|---|
| 2026-09-04 | Terceiro idioma (es) | Mercado latino em Orlando; a arquitetura já suporta N culturas, custa um `.resx` e as traduções de catálogo | `Docs/decisions.md` D8 |
| 2026-09-04 | Caução por dano como hold no cartão (PaymentIntent manual capture) | Fora do v1 por decisão D7; concorrentes vendem "damage waiver" como add-on em vez de caução | `Docs/market-notes.md` |
| 2026-09-04 | WhatsApp Business API para confirmação e lembretes | v1 usa só click-to-chat; API tem custo e aprovação da Meta | `Docs/architecture.md` §6 |
| 2026-09-04 | Redirecionar `ronatrip.com/scooters` para `orlandoup.com` na fase 5 | Evita dois formulários concorrentes; a página da Ronatrip hoje é lead form sem preço | `Docs/market-notes.md` |
| 2026-09-04 | Ferramenta de postagem social (Ayrshare vs Meta Graph API direto) — pesquisar preço e aprovação de app antes da fase 7 | Decisão adiada em `Docs/roadmap.md` fase 7 | `Docs/roadmap.md` |
| 2026-09-04 | Apps nativos .NET MAUI — portão de decisão na fase 8 | D17 | `Docs/decisions.md` |
| 2026-09-04 | Dividir `OrlandoUp.Web` em projetos (Domain/Application/Infrastructure) se um segundo host (worker, MAUI) precisar referenciar o domínio | D11 previu a divisão sem renomear namespaces | `Docs/decisions.md` |
