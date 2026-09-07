using OrlandoUp.Application;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Seeding;

// =================================================================================================
// THE REAL FLEET, AND EVERY PLACE WHERE A FACT IS STILL MISSING.
//
// This file used to hold typical values for classes of equipment. It now holds what Ronatrip owns
// (Docs/decisions.md D26) and the list prices D27 put on the market. The rule that governs every
// line below: a number that D26 does not carry is not written here. A dimension nobody measured is
// null, and null travels all the way to the page as an absent row - never as a zero, never as a
// typical value, and never as a badge claiming a machine fits somewhere (D32, and D15 before it).
//
// What is still unknown, and therefore absent rather than invented:
//   * the exact model names on the labels and the count per model - open question Q13; the names
//     here are the ones D26 records and the counts are its approximations. The public site shows
//     no count, so only /admin reads them.
//   * every dimension of the wheelchair - D26 gives none, and Rod chose (K2 b) to rent it with no
//     specification rather than to publish a measurement nobody took.
//   * the delivery fee - D27 left it inside Q3, so no page publishes one (K1 b). The zone fees
//     below are what the booking flow of leva 03 will read; they are not shown to a visitor.
//
// The four strollers are IsBookable = false: they are not bought yet (D26, Thanksgiving sales).
// They carry no pricing tier and no add-on, because a price nobody can pay is worse than no price.
//
// Park rules quoted in the copy come from Docs/market-notes.md and from nowhere else.
// =================================================================================================

internal sealed record SeedText(string Culture, string Name, string Tagline, string Description, string[] Highlights);

internal sealed record SeedTier(int MinDays, int? MaxDays, TierMode Mode, decimal Amount);

internal sealed record SeedProduct(
    string Slug,
    ProductCategory Category,
    SeatConfiguration? Configuration,
    bool IsBookable,
    int UnitCount,
    int? MaxRiderWeightLb,
    decimal? WidthIn,
    decimal? LengthIn,
    decimal? SeatWidthIn,
    decimal? RangeMiles,
    int SortOrder,
    SeedTier[] Tiers,
    string[] AddOnCodes,
    SeedText[] Texts);

internal sealed record SeedAddOnText(string Culture, string Name, string Description);

internal sealed record SeedAddOn(string Code, AddOnPricingMode PricingMode, decimal Amount, int SortOrder, SeedAddOnText[] Texts);

internal sealed record SeedZoneText(string Culture, string Name, string Instructions);

internal sealed record SeedZone(
    string Code,
    ZoneKind Kind,
    decimal DeliveryFee,
    HandoverMode HandoverMode,
    int SortOrder,
    SeedZoneText[] Texts,
    string[] LocationNames);

internal static class CatalogSeedData
{
    private const string En = SiteCultures.English;
    private const string Pt = SiteCultures.Portuguese;

    public static readonly SeedProduct[] Products =
    [
        // 300 lb, 42.3 by 20.5 in, 9 miles and 14 with the extended battery - every one of those
        // is D26, verified from the manufacturer's published specification on 2026-09-05.
        new("drive-scout-4", ProductCategory.MobilityScooter, null, true, 4, 300, 20.5m, 42.3m, null, 9m, 1,
            [new(1, 2, TierMode.FlatPerRental, 75m), new(3, 6, TierMode.PerDay, 32m), new(7, null, TierMode.PerDay, 27m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Drive Scout 4", "The four-wheel scooter for a full day on your feet — without being on your feet",
                    "This is the scooter most people should start with. It carries a rider up to 300 lb and covers about 9 miles on a charge, or about 14 with the extended battery. At 20.5 inches wide and 42.3 inches long it is inside the 30 by 48 inch limit the Disney buses and the Skyliner set, so it stays with you all day instead of waiting at a gate.\n\nIt arrives at your hotel tested and charged. Plug it in overnight in your room, and take the key out whenever you park it.",
                    ["Fits the Disney buses and the Skyliner", "Carries up to 300 lb", "About 9 miles on a charge, 14 with the extended battery", "Delivered tested and charged"]),
                new(Pt, "Drive Scout 4", "A scooter de quatro rodas para um dia inteiro em pé — sem ficar em pé",
                    "É a scooter por onde a maioria das pessoas deve começar. Suporta até 300 lb (cerca de 136 kg) e faz umas 9 milhas com uma carga, ou umas 14 com a bateria estendida. Com 20,5 polegadas de largura por 42,3 de comprimento, fica dentro do limite de 30 por 48 polegadas dos ônibus da Disney e do Skyliner, então ela fica com você o dia todo em vez de esperar num portão.\n\nChega ao seu hotel testada e carregada. Carregue à noite no quarto e tire a chave sempre que estacionar.",
                    ["Cabe nos ônibus da Disney e no Skyliner", "Suporta até 300 lb (cerca de 136 kg)", "Cerca de 9 milhas por carga, 14 com a bateria estendida", "Entregue testada e carregada"]),
            ]),

        // 300 lb, 39 by 19.5 in, 17 in seat, 9 miles and 15 with the 21 Ah pack - all D26.
        new("drive-spitfire-ex", ProductCategory.MobilityScooter, null, true, 4, 300, 19.5m, 39m, 17m, 9m, 2,
            [new(1, 2, TierMode.FlatPerRental, 75m), new(3, 6, TierMode.PerDay, 32m), new(7, null, TierMode.PerDay, 27m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Drive Spitfire EX", "The smaller travel scooter, for tight lifts and packed buses",
                    "The same 300 lb capacity in a smaller frame: 19.5 inches wide and 39 inches long, on a 17 inch seat. It covers about 9 miles on a charge, or about 15 with the 21 Ah battery. Being shorter than the Scout makes it easier in a crowded hotel lift and on a full bus, and it is comfortably inside the 30 by 48 inch limit the Disney buses and the Skyliner set.\n\nIt costs the same as the Scout 4 — pick this one for the size, not for the price.",
                    ["Fits the Disney buses and the Skyliner", "Carries up to 300 lb on a 17-inch seat", "About 9 miles on a charge, 15 with the 21 Ah battery", "The smaller of our two scooters"]),
                new(Pt, "Drive Spitfire EX", "A scooter de viagem menor, para elevador apertado e ônibus cheio",
                    "A mesma capacidade de 300 lb (cerca de 136 kg) numa estrutura menor: 19,5 polegadas de largura por 39 de comprimento, com assento de 17 polegadas. Faz umas 9 milhas com uma carga, ou umas 15 com a bateria de 21 Ah. Por ser mais curta que a Scout, entra melhor num elevador cheio de hotel e num ônibus lotado, e fica com folga dentro do limite de 30 por 48 polegadas dos ônibus da Disney e do Skyliner.\n\nCusta o mesmo que a Scout 4 — escolha esta pelo tamanho, não pelo preço.",
                    ["Cabe nos ônibus da Disney e no Skyliner", "Suporta até 300 lb (cerca de 136 kg), assento de 17 polegadas", "Cerca de 9 milhas por carga, 15 com a bateria de 21 Ah", "A menor das nossas duas scooters"]),
            ]),

        // Every dimension is null on purpose: D26 records two Drive wheelchairs and not one
        // measurement of them, and Rod chose to rent it with no specification rather than to
        // publish a number nobody took (K2 b). The specification list does not render at all.
        new("drive-wheelchair", ProductCategory.Wheelchair, null, true, 2, null, null, null, null, null, 3,
            [new(1, 2, TierMode.FlatPerRental, 40m), new(3, null, TierMode.PerDay, 12m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Drive wheelchair", "For the walker who is fine at home and not for ten miles of park",
                    "A manual wheelchair for somebody who has a pusher and does not need a motor. There is no battery, nothing to charge and nothing to switch on: brakes on both wheels, handles at the back, and that is the whole of it.\n\nIt is delivered clean and checked to your hotel, on the same trip as a scooter if you are renting both. We are confirming the exact model and will publish its measurements here as soon as we have them.",
                    ["No battery and nothing to charge", "Brakes on both wheels", "Delivered clean and checked", "Comes on the same delivery as a scooter"]),
                new(Pt, "Cadeira de rodas Drive", "Para quem anda bem em casa, mas não por quinze quilômetros de parque",
                    "Uma cadeira de rodas manual para quem tem alguém empurrando e não precisa de motor. Não tem bateria, não tem o que carregar e não tem o que ligar: freios nas duas rodas, alças atrás, e é isso.\n\nChega limpa e revisada no seu hotel, na mesma viagem da scooter se você alugar as duas. Estamos confirmando o modelo exato e publicamos as medidas aqui assim que tivermos.",
                    ["Sem bateria e sem nada para carregar", "Freios nas duas rodas", "Entregue limpa e revisada", "Vem na mesma entrega da scooter"]),
            ]),

        // The four below are IsBookable = false until the units exist (D26). No tier, no add-on, no
        // dimension: nothing about them is known yet except the class of equipment and the park
        // rules that apply to it, both of which are in Docs/market-notes.md.
        new("single-stroller", ProductCategory.Stroller, SeatConfiguration.Single, false, 0, null, null, null, null, null, 4,
            [],
            [],
            [
                new(En, "Single stroller", "One child, one nap, all day",
                    "A full-size single stroller for one child. The Disney parks allow strollers up to 31 by 52 inches and prohibit wagons and stroller-wagons of any size, which is why a stroller is what we will rent.\n\nWe are buying the stroller fleet for the winter season. Tell us your dates and we will let you know the moment they arrive.",
                    ["For one child", "Inside the Disney 31 by 52 inch stroller limit", "Wagons are not allowed in the parks at all"]),
                new(Pt, "Carrinho simples", "Uma criança, um cochilo, o dia inteiro",
                    "Um carrinho simples de tamanho normal, para uma criança. Os parques da Disney permitem carrinhos de até 31 por 52 polegadas e proíbem wagons e carrinhos-wagon de qualquer tamanho — é por isso que carrinho é o que vamos alugar.\n\nEstamos comprando a frota de carrinhos para a temporada de inverno. Diga suas datas e avisamos assim que chegarem.",
                    ["Para uma criança", "Dentro do limite de 31 por 52 polegadas da Disney", "Wagons não são permitidos nos parques"]),
            ]),

        new("double-stroller", ProductCategory.Stroller, SeatConfiguration.Double, false, 0, null, null, null, null, null, 5,
            [],
            [],
            [
                new(En, "Double stroller", "Two children side by side",
                    "Two seats in one frame, for two small children on a full park day. The Disney parks allow strollers up to 31 by 52 inches and prohibit wagons and stroller-wagons of any size.\n\nWe are buying the stroller fleet for the winter season. Tell us your dates and we will let you know the moment they arrive.",
                    ["Two seats in one frame", "Inside the Disney 31 by 52 inch stroller limit", "Wagons are not allowed in the parks at all"]),
                new(Pt, "Carrinho duplo", "Duas crianças, lado a lado",
                    "Dois assentos num quadro só, para duas crianças pequenas num dia inteiro de parque. Os parques da Disney permitem carrinhos de até 31 por 52 polegadas e proíbem wagons e carrinhos-wagon de qualquer tamanho.\n\nEstamos comprando a frota de carrinhos para a temporada de inverno. Diga suas datas e avisamos assim que chegarem.",
                    ["Dois assentos num quadro só", "Dentro do limite de 31 por 52 polegadas da Disney", "Wagons não são permitidos nos parques"]),
            ]),

        new("triple-stroller", ProductCategory.Stroller, SeatConfiguration.Triple, false, 0, null, null, null, null, null, 6,
            [],
            [],
            [
                new(En, "Triple stroller", "Three children, one push",
                    "Three seats in one frame, for the family that would otherwise be pushing two strollers at once. The Disney parks allow strollers up to 31 by 52 inches and prohibit wagons and stroller-wagons of any size; a triple frame sits at the top of that allowance, so we will publish its measurements before anyone books one.\n\nWe are buying the stroller fleet for the winter season. Tell us your dates and we will let you know the moment they arrive.",
                    ["Three seats in one frame", "Measurements published before booking opens", "Wagons are not allowed in the parks at all"]),
                new(Pt, "Carrinho triplo", "Três crianças, um empurrão só",
                    "Três assentos num quadro só, para a família que empurraria dois carrinhos ao mesmo tempo. Os parques da Disney permitem carrinhos de até 31 por 52 polegadas e proíbem wagons e carrinhos-wagon de qualquer tamanho; um quadro triplo fica no teto dessa permissão, então vamos publicar as medidas antes de alguém reservar.\n\nEstamos comprando a frota de carrinhos para a temporada de inverno. Diga suas datas e avisamos assim que chegarem.",
                    ["Três assentos num quadro só", "Medidas publicadas antes de abrir a reserva", "Wagons não são permitidos nos parques"]),
            ]),

        new("infant-stroller", ProductCategory.Stroller, SeatConfiguration.Infant, false, 0, null, null, null, null, null, 7,
            [],
            [],
            [
                new(En, "Infant stroller", "Built around a car seat and a newborn",
                    "A stroller for the youngest travellers, with a flat recline. The Disney parks allow strollers up to 31 by 52 inches and prohibit wagons and stroller-wagons of any size.\n\nWe are buying the stroller fleet for the winter season. Tell us your dates and we will let you know the moment they arrive.",
                    ["For a newborn or an infant", "Inside the Disney 31 by 52 inch stroller limit", "Wagons are not allowed in the parks at all"]),
                new(Pt, "Carrinho para bebê", "Feito para o bebê-conforto e o recém-nascido",
                    "Um carrinho para os viajantes mais novos, com encosto que deita totalmente. Os parques da Disney permitem carrinhos de até 31 por 52 polegadas e proíbem wagons e carrinhos-wagon de qualquer tamanho.\n\nEstamos comprando a frota de carrinhos para a temporada de inverno. Diga suas datas e avisamos assim que chegarem.",
                    ["Para recém-nascido ou bebê", "Dentro do limite de 31 por 52 polegadas da Disney", "Wagons não são permitidos nos parques"]),
            ]),
    ];

    public static readonly SeedAddOn[] AddOns =
    [
        new("cup-holder", AddOnPricingMode.PerRental, 5m, 1,
            [
                new(En, "Cup holder", "Holds a park mug or a water bottle within reach."),
                new(Pt, "Porta-copos", "Segura a caneca do parque ou a garrafa de água ao alcance da mão."),
            ]),
        new("cane-holder", AddOnPricingMode.PerRental, 5m, 2,
            [
                new(En, "Cane holder", "Keeps a cane or a crutch upright and away from the wheels."),
                new(Pt, "Porta-bengala", "Mantém a bengala ou a muleta em pé e longe das rodas."),
            ]),
        new("sunshade", AddOnPricingMode.PerDay, 3m, 3,
            [
                new(En, "Sun shade", "A clip-on shade for the Florida afternoon."),
                new(Pt, "Proteção de sol", "Uma sombra de encaixe para a tarde da Flórida."),
            ]),
        new("rear-basket", AddOnPricingMode.PerRental, 8m, 4,
            [
                new(En, "Rear basket", "Extra room behind the seat for bags and ponchos."),
                new(Pt, "Cesto traseiro", "Espaço extra atrás do assento para bolsas e capas de chuva."),
            ]),
        new("rain-cover", AddOnPricingMode.PerRental, 5m, 5,
            [
                new(En, "Rain cover", "A clear cover that keeps a stroller dry through an afternoon storm."),
                new(Pt, "Capa de chuva", "Uma capa transparente que mantém o carrinho seco na tempestade da tarde."),
            ]),
        new("damage-waiver", AddOnPricingMode.PerRental, 20m, 6,
            [
                new(En, "Damage waiver", "Covers accidental damage to the equipment during the rental."),
                new(Pt, "Isenção de danos", "Cobre danos acidentais ao equipamento durante o aluguel."),
            ]),
    ];

    // The fees below are what the booking flow of leva 03 will read. No page of leva 02 shows one:
    // D27 left the delivery fee inside open question Q3, and Rod chose not to publish a number
    // nobody has confirmed (K1 b).
    public static readonly SeedZone[] Zones =
    [
        new("disney-resorts", ZoneKind.DisneyResort, 0m, HandoverMode.MeetAndGreet, 1,
            [
                new(En, "Walt Disney World resorts",
                    "Disney allows only its featured provider to leave equipment with Bell Services, so we meet you in person at the resort at a time we agree with you. It takes about five minutes: we hand the equipment over, show you how it works and answer whatever you want to ask."),
                new(Pt, "Resorts do Walt Disney World",
                    "A Disney só permite que o fornecedor oficial dela deixe equipamento com o Bell Services, então encontramos você pessoalmente no resort, no horário que combinarmos. Leva uns cinco minutos: entregamos o equipamento, mostramos como funciona e respondemos o que você quiser perguntar."),
            ],
            [
                "Disney's Pop Century Resort",
                "Disney's Art of Animation Resort",
                "Disney's All-Star Movies Resort",
                "Disney's Caribbean Beach Resort",
                "Disney's Contemporary Resort",
                "Disney's Grand Floridian Resort & Spa",
            ]),

        new("universal-resorts", ZoneKind.UniversalResort, 0m, HandoverMode.MeetAndGreet, 2,
            [
                new(En, "Universal Orlando resorts",
                    "We meet you in person at the resort at a time we agree with you, hand the equipment over and show you how it works before we leave."),
                new(Pt, "Resorts da Universal Orlando",
                    "Encontramos você pessoalmente no resort, no horário que combinarmos, entregamos o equipamento e mostramos como funciona antes de ir embora."),
            ],
            [
                "Universal's Cabana Bay Beach Resort",
                "Universal's Endless Summer Resort - Surfside Inn",
            ]),

        new("idrive-lbv-hotels", ZoneKind.HotelOrResort, 0m, HandoverMode.FrontDesk, 3,
            [
                new(En, "International Drive and Lake Buena Vista hotels",
                    "We leave the equipment with the front desk under the name on the booking, so you can collect it whenever you arrive, however late the flight was."),
                new(Pt, "Hotéis da International Drive e de Lake Buena Vista",
                    "Deixamos o equipamento na recepção no nome da reserva, para você retirar na hora em que chegar, por mais atrasado que o voo tenha sido."),
            ],
            [
                "Hilton Orlando Buena Vista Palace",
                "Rosen Inn International Drive",
            ]),

        new("vacation-homes", ZoneKind.VacationHome, 25m, HandoverMode.Doorstep, 4,
            [
                new(En, "Vacation homes and rental houses",
                    "We deliver to the door of the house, at a time we agree with you."),
                new(Pt, "Casas de temporada",
                    "Entregamos na porta da casa, no horário que combinarmos."),
            ],
            []),
    ];
}
