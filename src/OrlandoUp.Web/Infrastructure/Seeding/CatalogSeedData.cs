using OrlandoUp.Application;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Seeding;

// =================================================================================================
// PLACEHOLDER DATA, AND THE ONLY PLACE THAT SAYS SO.
//
// Every number and every sentence below is a TYPICAL value for its class of equipment, written to
// give the site something true-shaped to show while the real fleet is unknown. None of it describes
// the fleet we actually own, and no price here was ever quoted to anyone.
//
// The open questions that close it are Q1 (what the fleet really is), Q2 (what the prices really
// are) and Q9 (the company data). When they are answered this file is replaced, and the rows it
// wrote are edited in the administration, never by a migration — which is exactly why the catalog
// is seeded by an explicit command instead of being declared in the model (D5/01).
// =================================================================================================

internal sealed record SeedText(string Culture, string Name, string Tagline, string Description, string[] Highlights);

internal sealed record SeedTier(int MinDays, int? MaxDays, TierMode Mode, decimal Amount);

internal sealed record SeedProduct(
    string Slug,
    ProductCategory Category,
    SeatConfiguration? Configuration,
    int? MaxRiderWeightLb,
    decimal WidthIn,
    decimal LengthIn,
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
        new("standard-scooter", ProductCategory.MobilityScooter, null, 300, 21m, 41m, 17m, 12m, 1,
            [new(1, 2, TierMode.FlatPerRental, 75m), new(3, 6, TierMode.PerDay, 32m), new(7, null, TierMode.PerDay, 27m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Standard mobility scooter", "The everyday scooter for a full park day",
                    "Our most rented model, and the one most people should start with. It carries a rider up to 300 lb, runs about 12 miles on a charge and comes apart in three pieces for a car trunk. At 21 by 41 inches it fits the Disney buses and the Skyliner, so it stays with you all day instead of waiting at the gate. It arrives at your hotel tested and charged; plug it in overnight in your room, and take the key out whenever you park it.",
                    ["Fits the Disney buses and the Skyliner", "Carries up to 300 lb", "About 12 miles on a full charge", "Delivered tested and charged"]),
                new(Pt, "Scooter de mobilidade padrão", "A scooter do dia a dia, para um dia inteiro de parque",
                    "É o modelo mais alugado, e por onde a maioria das pessoas deve começar. Suporta até 300 lb (cerca de 136 kg), faz umas 12 milhas com uma carga e se desmonta em três partes para caber no porta-malas. Com 21 por 41 polegadas, cabe nos ônibus da Disney e no Skyliner, então ela fica com você o dia todo em vez de esperar no portão. Chega ao seu hotel testada e carregada; carregue à noite no quarto e tire a chave sempre que estacionar.",
                    ["Cabe nos ônibus da Disney e no Skyliner", "Suporta até 300 lb (cerca de 136 kg)", "Cerca de 12 milhas por carga", "Entregue testada e carregada"]),
            ]),

        new("heavy-duty-scooter", ProductCategory.MobilityScooter, null, 400, 24m, 47m, 20m, 15m, 2,
            [new(1, 2, TierMode.FlatPerRental, 95m), new(3, 6, TierMode.PerDay, 38m), new(7, null, TierMode.PerDay, 33m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Heavy-duty mobility scooter", "More weight, more range, a wider seat",
                    "The model to pick when the standard scooter is not quite enough. It carries a rider up to 400 lb on a 20-inch seat and covers about 15 miles between charges, which is a full day at Epcot with room left over. At 24 by 47 inches it still fits the Disney buses and the Skyliner. It arrives tested and charged; charge it overnight in your room and remove the key whenever you leave it parked.",
                    ["Carries up to 400 lb", "20-inch seat", "About 15 miles on a full charge", "Fits the Disney buses and the Skyliner"]),
                new(Pt, "Scooter de mobilidade reforçada", "Mais peso, mais autonomia, assento mais largo",
                    "É o modelo para quando a scooter padrão não dá conta. Suporta até 400 lb (cerca de 181 kg) num assento de 20 polegadas e faz cerca de 15 milhas entre uma carga e outra, o que é um dia inteiro no Epcot com folga. Com 24 por 47 polegadas, ainda cabe nos ônibus da Disney e no Skyliner. Chega testada e carregada; carregue à noite no quarto e tire a chave sempre que deixar estacionada.",
                    ["Suporta até 400 lb (cerca de 181 kg)", "Assento de 20 polegadas", "Cerca de 15 milhas por carga", "Cabe nos ônibus da Disney e no Skyliner"]),
            ]),

        new("standard-wheelchair", ProductCategory.Wheelchair, null, 300, 25m, 42m, 18m, null, 3,
            [new(1, 2, TierMode.FlatPerRental, 40m), new(3, null, TierMode.PerDay, 12m)],
            ["cup-holder", "cane-holder", "rear-basket", "damage-waiver"],
            [
                new(En, "Standard wheelchair", "Light, foldable, and there when the legs give out",
                    "A folding transport wheelchair for someone who walks fine at home but not for ten miles of park. It seats a rider up to 300 lb on an 18-inch seat, folds flat in seconds and boards every park bus, boat and monorail. There is no battery to charge and nothing to plug in: push handles at the back, brakes on both wheels. It arrives clean and checked at your hotel.",
                    ["Carries up to 300 lb", "18-inch seat", "Folds flat in seconds", "No battery, nothing to charge"]),
                new(Pt, "Cadeira de rodas padrão", "Leve, dobrável, e ali quando as pernas cansam",
                    "Uma cadeira de transporte dobrável para quem anda bem em casa, mas não por quinze quilômetros de parque. Acomoda até 300 lb (cerca de 136 kg) num assento de 18 polegadas, dobra em segundos e entra em todo ônibus, barco e monotrilho dos parques. Não tem bateria para carregar nem nada para ligar na tomada: alças atrás e freios nas duas rodas. Chega limpa e revisada no seu hotel.",
                    ["Suporta até 300 lb (cerca de 136 kg)", "Assento de 18 polegadas", "Dobra em segundos", "Sem bateria e sem carregar"]),
            ]),

        new("single-stroller", ProductCategory.Stroller, SeatConfiguration.Single, null, 24m, 40m, null, null, 4,
            [new(1, 2, TierMode.FlatPerRental, 35m), new(3, null, TierMode.PerDay, 10m)],
            ["cup-holder", "sunshade", "rain-cover", "rear-basket"],
            [
                new(En, "Single stroller", "One child, one nap, all day",
                    "A full-size single stroller with a deep recline, a sun canopy and a basket that swallows a park bag. At 24 by 40 inches it sits well inside the 31 by 52 inch limit the Disney parks set for strollers, so nobody stops you at the gate. Wagons are not allowed inside the parks at all, which is why a stroller is what we rent. It arrives clean at your hotel and goes back the day you leave.",
                    ["Well inside the Disney 31 by 52 inch stroller limit", "Deep recline for naps", "Sun canopy and a park-bag basket", "Delivered clean to your hotel"]),
                new(Pt, "Carrinho simples", "Uma criança, um cochilo, o dia inteiro",
                    "Um carrinho simples de tamanho normal, com encosto que reclina bem, capota de sol e cesto que engole a bolsa do parque. Com 24 por 40 polegadas, fica bem dentro do limite de 31 por 52 polegadas que os parques da Disney exigem para carrinhos, então ninguém para você no portão. Wagons não são permitidos dentro dos parques, e é por isso que alugamos carrinho. Chega limpo no seu hotel e volta no dia da sua partida.",
                    ["Bem dentro do limite de 31 por 52 polegadas da Disney", "Encosto que reclina para o cochilo", "Capota de sol e cesto para a bolsa do parque", "Entregue limpo no seu hotel"]),
            ]),

        new("double-stroller", ProductCategory.Stroller, SeatConfiguration.Double, null, 30m, 48m, null, null, 5,
            [new(1, 2, TierMode.FlatPerRental, 45m), new(3, null, TierMode.PerDay, 13m)],
            ["cup-holder", "sunshade", "rain-cover", "rear-basket"],
            [
                new(En, "Double stroller", "Two children side by side",
                    "Two seats side by side, each reclining on its own, so one child naps through the afternoon while the other watches the parade. At 30 by 48 inches it is inside the 31 by 52 inch limit the Disney parks set, with a little room to spare. Wagons are not allowed inside the parks, so a double stroller is how two small children get through a full day. It arrives clean and folded at your hotel.",
                    ["Inside the Disney 31 by 52 inch stroller limit", "Two seats, each reclining on its own", "Canopy over both seats", "Delivered clean to your hotel"]),
                new(Pt, "Carrinho duplo", "Duas crianças, lado a lado",
                    "Dois assentos lado a lado, cada um reclinando por conta própria, então uma criança cochila a tarde inteira enquanto a outra assiste à parada. Com 30 por 48 polegadas, fica dentro do limite de 31 por 52 polegadas dos parques da Disney, com uma folga pequena. Wagons não são permitidos dentro dos parques, então o carrinho duplo é o jeito de duas crianças pequenas aguentarem um dia inteiro. Chega limpo e dobrado no seu hotel.",
                    ["Dentro do limite de 31 por 52 polegadas da Disney", "Dois assentos, cada um reclinando sozinho", "Capota sobre os dois assentos", "Entregue limpo no seu hotel"]),
            ]),

        new("triple-stroller", ProductCategory.Stroller, SeatConfiguration.Triple, null, 31m, 52m, null, null, 6,
            [new(1, 2, TierMode.FlatPerRental, 60m), new(3, null, TierMode.PerDay, 18m)],
            ["cup-holder", "sunshade", "rain-cover", "rear-basket"],
            [
                new(En, "Triple stroller", "Three children, one push",
                    "Three seats in one frame, for the family that would otherwise be pushing two strollers at once. At 31 by 52 inches it is exactly at the stroller limit the Disney parks set: allowed inside, but too wide for a Disney bus, so plan on driving or on the boats and the monorail. Wagons are not allowed in the parks at any size. It arrives clean at your hotel.",
                    ["Three seats in one frame", "At the Disney 31 by 52 inch stroller limit", "Too wide for the Disney buses", "Delivered clean to your hotel"]),
                new(Pt, "Carrinho triplo", "Três crianças, um empurrão só",
                    "Três assentos num quadro só, para a família que empurraria dois carrinhos ao mesmo tempo. Com 31 por 52 polegadas, ele está exatamente no limite de carrinho dos parques da Disney: entra nos parques, mas é largo demais para o ônibus da Disney, então conte com carro, com os barcos ou com o monotrilho. Wagons não são permitidos nos parques, de tamanho nenhum. Chega limpo no seu hotel.",
                    ["Três assentos num quadro só", "No limite de 31 por 52 polegadas da Disney", "Largo demais para os ônibus da Disney", "Entregue limpo no seu hotel"]),
            ]),

        new("infant-stroller", ProductCategory.Stroller, SeatConfiguration.Infant, null, 23m, 40m, null, null, 7,
            [new(1, 2, TierMode.FlatPerRental, 35m), new(3, null, TierMode.PerDay, 10m)],
            ["cup-holder", "sunshade", "rain-cover", "rear-basket"],
            [
                new(En, "Infant stroller", "Built around a car seat and a newborn",
                    "A stroller for the youngest travellers, with a flat recline and a frame that takes an infant car seat. At 23 by 40 inches it is the smallest frame we rent and well inside the Disney parks 31 by 52 inch stroller limit. Wagons are not allowed in the parks, and a newborn belongs lying flat in any case. It arrives clean at your hotel with the canopy already fitted.",
                    ["Flat recline for a newborn", "Takes an infant car seat", "The smallest frame we rent", "Well inside the Disney stroller limit"]),
                new(Pt, "Carrinho para bebê", "Feito para o bebê-conforto e o recém-nascido",
                    "Um carrinho para os viajantes mais novos, com encosto que deita totalmente e estrutura que recebe bebê-conforto. Com 23 por 40 polegadas, é a menor estrutura que alugamos e fica bem dentro do limite de 31 por 52 polegadas dos parques da Disney. Wagons não são permitidos nos parques, e um recém-nascido precisa mesmo ir deitado. Chega limpo no seu hotel, com a capota já montada.",
                    ["Encosto que deita totalmente para recém-nascido", "Recebe bebê-conforto", "A menor estrutura que alugamos", "Bem dentro do limite de carrinhos da Disney"]),
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
                    "We deliver to the door of the house. These addresses carry a delivery fee because they sit outside the resort corridor."),
                new(Pt, "Casas de temporada",
                    "Entregamos na porta da casa. Esses endereços têm taxa de entrega porque ficam fora do corredor dos resorts."),
            ],
            []),
    ];
}
