using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;

namespace CosmicMachine.CSharpGalaxy
{
    public static class AbcModule
    {
        public static BitEncodedImage os = "os";
        public static BitEncodedImage ap = "`";
        public static BitEncodedImage mul = "*";
        public static BitEncodedImage power2 = "power2";
        public static BitEncodedImage coordinate = BitEncodeImage("#####|#    |#    |#    |#    ");
        public static BitEncodedImage matter = BitEncodeImage("#####|#####|#####|#####|#####");
        public static BitEncodedImage lengthSym = BitEncodeImage("###  |###  |#####|  ###|  ###");
        public static BitEncodedImage eq = "=";
        public static BitEncodedImage add = "+";
        public static BitEncodedImage ellipsis = "...";
        public static BitEncodedImage detonate = "detonate";
        public static BitEncodedImage burn = "burn";
        public static BitEncodedImage x = "x";
        public static BitEncodedImage y = "y";

        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        public static IEnumerable<long> humansRace = EncodeBitmap(0, 0, "Люди.png");
        public static IEnumerable<long> endoRace = EncodeBitmap(0, 0, "Эндо.png");
        public static IEnumerable<long> biolsRace = EncodeBitmap(0, 0, "Биолы.png");
        public static IEnumerable<long> virtulsRace = EncodeBitmap(0, 0, "Виртулы.png");
        public static IEnumerable<long> vrogsRace = EncodeBitmap(0, 0, "Вроги.png");
        public static IEnumerable<long> gzorgsRace = EncodeBitmap(0, 0, "Гзорги.png");
        public static IEnumerable<long> kronsRace = EncodeBitmap(0, 0, "Кроны.png");
        public static IEnumerable<long> kublesRace = EncodeBitmap(0, 0, "Кублы.png");
        public static IEnumerable<long> lizgiRace = EncodeBitmap(0, 0, "Лизги.png");
        public static IEnumerable<long> octosRace = EncodeBitmap(0, 0, "Окты.png");
        public static IEnumerable<long> oleodesRace = EncodeBitmap(0, 0, "Олеоды.png");
        public static IEnumerable<long> serpsRace = EncodeBitmap(0, 0, "Серпы.png");
        public static IEnumerable<long> sticksRace = EncodeBitmap(0, 0, "Стики.png");
        public static IEnumerable<long> stretchesRace = EncodeBitmap(0, 0, "Стречи.png");
        public static IEnumerable<long> tritsRace = EncodeBitmap(0, 0, "Триты.png");
        public static IEnumerable<long> khlostsRace = EncodeBitmap(0, 0, "Хлосты.png");
        public static IEnumerable<long> greyRace = EncodeBitmap(0, 0, "grey.png");

        public static IEnumerable<long> L_humansRace = EncodeBitmap(0, 0, "L-Люди.png");
        public static IEnumerable<long> L_endoRace = EncodeBitmap(0, 0, "L-Эндо.png");
        public static IEnumerable<long> L_biolsRace = EncodeBitmap(0, 0, "L-Биолы.png");
        public static IEnumerable<long> L_virtulsRace = EncodeBitmap(0, 0, "L-Виртулы.png");
        public static IEnumerable<long> L_vrogsRace = EncodeBitmap(0, 0, "L-Вроги.png");
        public static IEnumerable<long> L_gzorgsRace = EncodeBitmap(0, 0, "L-Гзорги.png");
        public static IEnumerable<long> L_kronsRace = EncodeBitmap(0, 0, "L-Кроны.png");
        public static IEnumerable<long> L_kublesRace = EncodeBitmap(0, 0, "L-Кублы.png");
        public static IEnumerable<long> L_lizgiRace = EncodeBitmap(0, 0, "L-Лизги.png");
        public static IEnumerable<long> L_octosRace = EncodeBitmap(0, 0, "L-Окты.png");
        public static IEnumerable<long> L_oleodesRace = EncodeBitmap(0, 0, "L-Олеоды.png");
        public static IEnumerable<long> L_serpsRace = EncodeBitmap(0, 0, "L-Серпы.png");
        public static IEnumerable<long> L_sticksRace = EncodeBitmap(0, 0, "L-Стики.png");
        public static IEnumerable<long> L_stretchesRace = EncodeBitmap(0, 0, "L-Стречи.png");
        public static IEnumerable<long> L_tritsRace = EncodeBitmap(0, 0, "L-Триты.png");
        public static IEnumerable<long> L_khlostsRace = EncodeBitmap(0, 0, "L-Хлосты.png");
        public static IEnumerable<long> L_grayRace = EncodeBitmap(0, 0, "arecibo.bmp");

        public static IEnumerable<long> TrueSymbol = EncodeBitmap(-33, -33, "True.png");
        public static IEnumerable<long> FalseSymbol = EncodeBitmap(-33, -33, "False.png");
        public static IEnumerable<long> Burn = EncodeBitmap(-33, -33, "Burn.png");
        public static IEnumerable<long> Temp = EncodeBitmap(-33, -33, "Temp.png");

        // ReSharper restore IdentifierTypo
        // ReSharper restore StringLiteralTypo

        public static IEnumerable<RaceInfo> races = List(
            new RaceInfo(0, Vec(14, -64), humansRace, L_humansRace, -1),
            new RaceInfo(1, Vec(-4, 94), endoRace, L_endoRace, 2),
            new RaceInfo(2, Vec(-78, -67), biolsRace, L_biolsRace, 1),
            new RaceInfo(3, Vec(-38, -46), virtulsRace, L_virtulsRace, -1),
            new RaceInfo(4, Vec(44, -34), vrogsRace, L_vrogsRace, -1),
            new RaceInfo(5, Vec(60, -30), gzorgsRace, L_gzorgsRace, 3),
            new RaceInfo(6, Vec(-81, 11), kronsRace, L_kronsRace, 0),
            new RaceInfo(7, Vec(-49, 34), kublesRace, L_kublesRace, -1),
            new RaceInfo(8, Vec(52, 27), lizgiRace, L_lizgiRace, -1),
            new RaceInfo(9, Vec(99, 15), octosRace, L_octosRace, -1),
            new RaceInfo(10, Vec(96, 35), oleodesRace, L_oleodesRace, -1),
            new RaceInfo(11, Vec(21, 92), greyRace, L_grayRace, -1)
            //new RaceInfo(12, Vec(87, -55), sticksRace, L_sticksRace, -1),
            //new RaceInfo(13, Vec(34, -73), stretchesRace, L_stretchesRace, -1),
            //new RaceInfo(14, Vec(0, 56), tritsRace, L_tritsRace, -1),
            //new RaceInfo(15, Vec(117, 33), khlostsRace, L_khlostsRace, -1)
        );

        //public static IEnumerable<BitEncodedImage> raceSymbols = List(race1, race2, race3, race4, race5, race6, race7, race8, race9, race10, race11, race12, race13, race14, race15, race16);
        //public static IEnumerable<IEnumerable<V>> raceImages = List(humansImage, humansImage, cronosImage, humansImage);
        public static BitEncodedImage shoot = "shoot";
        public static BitEncodedImage split = "split";
        public static BitEncodedImage battle = BitEncodeImage("#######|#     #|##### #|### ###|# #####|#     #|#######");
        public static BitEncodedImage attackShip0 = BitEncodeImage("     |     |  #  |     |     ");
        public static BitEncodedImage attackShip1 = BitEncodeImage("     | # # |  #  | # # |     ");
        public static BitEncodedImage attackShip2 = BitEncodeImage("#   #| ### | ### | ### |#   #");
        public static BitEncodedImage attackShip3 = BitEncodeImage("## ##|#####| ### |#####|## ##");
        public static BitEncodedImage defenseShip0 = BitEncodeImage("     |     |  #  |     |     ");
        public static BitEncodedImage defenseShip1 = BitEncodeImage("     |  #  | # # |  #  |     ");
        public static BitEncodedImage defenseShip2 = BitEncodeImage("  #  | ### |## ##| ### |  #  ");
        public static BitEncodedImage defenseShip3 = BitEncodeImage(" ### |#####|## ##|#####| ### ");

        public static BitEncodedImage[] AttackShipSymbols = new[]
        {
            attackShip0,
            attackShip1,
            attackShip2,
            attackShip3
        };

        public static BitEncodedImage[] DefenseShipSymbols = new[]
        {
            defenseShip0,
            defenseShip1,
            defenseShip2,
            defenseShip3
        };

        public static BitEncodedImage GetShipEncodedImage(ApiShip ship)
        {
            var symbols = ship.Role == ApiPlayerRole.Attacker ? AttackShipSymbols : DefenseShipSymbols;
            var totalMatterCount = ship.Matter.SumAll();
            var symbolIndex = totalMatterCount >= 256 ? 3 : totalMatterCount >= 16 ? 2 : totalMatterCount > 0 ? 1 : 0;
            return symbols.GetByIndex(symbolIndex);
        }


        public static BitEncodedImage applySplit = BitEncodeImage("#   #|#   #|## ##|#   #|#   #");
        public static BitEncodedImage selectDirection = BitEncodeImage("#  #  #|       |       |#     #|       |       |#  #  #");
        public static IEnumerable<V> planet8 = DrawFillRect(-8, -8, 17, 17);
        public static IEnumerable<V> planet16 = DrawFillRect(-16, -16, 33, 33);
        public static BitEncodedImage fuel = BitEncodeImage("# # #| # # |# # #| # # |# # #");
        public static BitEncodedImage lasers = BitEncodeImage("#####|     |#####|     |#####");
        public static BitEncodedImage radiators = BitEncodeImage("# # #|# # #|# # #|# # #|# # #");
        public static BitEncodedImage engines = BitEncodeImage("#####|#   #|# # #|#   #|#####");
        public static BitEncodedImage temp = BitEncodeImage(" ### | #   | ####| ### |#    ");
        public static IEnumerable<BitEncodedImage> matterSymbols = List(fuel, lasers, radiators, engines);

        //public static readonly IEnumerable<V> galaxy1 = DrawBitmap("Галактика 1.png").ShiftVectors(Vec(-128, -128));
        //public static readonly IEnumerable<V> galaxy0 = DrawBitmap("Галактика 0.png").ShiftVectors(Vec(-128, -128));
        //public static readonly IEnumerable<V> galaxy_1 = DrawBitmap("Галактика -1.png").ShiftVectors(Vec(-128, -128));
        public static readonly IEnumerable<long> galaxy1 = EncodeBitmap(-128, -128, "Галактика 1.png");
        public static readonly IEnumerable<long> galaxy0 = EncodeBitmap(-128, -128, "Галактика 0.png");
        public static readonly IEnumerable<long> galaxy_1 = EncodeBitmap(-128, -128, "Галактика -1.png");


        public static IEnumerable<V> DrawBurnBonus(V pos)
        {
            var bonusDescriptionItems = List(burn.BitDecodePixels(), DrawSymbolByName("="), DrawSymbolByName("1"), DrawSymbolByName("..."), DrawNumber(2));
            return DrawGlyphsWithSpacing(bonusDescriptionItems, 0, List(2, 2L, 0, 0, 0)).ShiftVectors(pos);
        }

        public static IEnumerable<V> DrawTempBonus(V pos)
        {
            var bonusDescriptionItems = List(temp.BitDecodePixels(), DrawSymbolByName("="), DrawSymbolByName("0"), DrawSymbolByName("..."), DrawNumber(64));
            return DrawGlyphsWithSpacing(bonusDescriptionItems, 0, List(2, 2L, 0, 0, 0)).ShiftVectors(pos);
        }

        public class RaceInfo
        {
            public RaceInfo(long raceId, V pos, IEnumerable<long> symbol, IEnumerable<long> image, long bonusId)
            {
                RaceId = raceId;
                Pos = pos;
                Symbol = symbol;
                Image = image;
                BonusId = bonusId;
            }

            public long RaceId;
            public V Pos;
            public IEnumerable<long> Symbol;
            public IEnumerable<long> Image;
            public long BonusId;
        }
    }
}