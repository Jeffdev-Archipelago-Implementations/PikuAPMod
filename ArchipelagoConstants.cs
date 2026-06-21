using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PikunikuAPMod
{
    public static class Scenes
    {
        public const string TitleScreen   = "00_TITLESCREEN";
        public const string Prologue      = "66_PROLOGUE";
        public const string MountainVillage   = "02_MOUNTAINVILLAGE";
        public const string ValleyRoad        = "02.1_MOUNTAINVILLAGETOFOREST";
        public const string MountainTemple    = "02.2_MOUNTAIN_TEMPLE";
        public const string MountainBoss      = "02.3_MOUNTAIN_ROBOTBOSS";
        public const string MountainMetro     = "02.4_MOUNTAIN_METRO";
        public const string Forest            = "03_FOREST";
        public const string ForestDanceBattle = "03.1_FOREST_DANCEBATTLE";
        public const string ForestTemple      = "03.2_FOREST_TEMPLE";
        public const string ForestBoss        = "03.3_FOREST_ROBOTBOSS";
        public const string ForestMetro       = "03.4_FOREST_METRO";
        public const string ForestTartineZone = "03.5_FOREST_TARTINEZONE";
        public const string Lake              = "04.1_LAKE";
        public const string Mine              = "04_MINE";
        public const string LakeBoss          = "04.2_LAKE_ROBOTBOSS";
        public const string HQ                = "05_HQ";
        public const string HQFinalBoss       = "05.1_HQ_FINALBOSS";
        public const string Beach             = "07_BEACH";
    }

    public static class StorySegments
    {
        public const int INTRO_WAKEUP = 0;
        public const int INTRO_FINDWAYOUT = 1;
        public const int MOUNTAIN_WHEREAREYOU = 2;
        public const int MOUNTAIN_REPAIRBRIDGE = 3;
        public const int MOUNTAIN_BACKTOVILLAGE = 4;
        public const int MOUNTAIN_TALKTOPAINTER = 5;
        public const int MOUNTAIN_FINDPENCILHAT = 6;
        public const int MOUNTAIN_GIVEPENCILHATBACK = 7;
        public const int MOUNTAIN_DRAWSCARECROW = 8;
        public const int MOUNTAIN_PUTFACEONSCARECROW = 9;
        public const int MOUNTAIN_EXPLOREWORLD = 10;
        public const int MOUNTAIN_EXPLOREWORLD2 = 11;
        public const int FOREST_HELPVILLAGERSINTREE1 = 12;
        public const int FOREST_HELPVILLAGERSINTREE2_ROCK = 13;
        public const int FOREST_HELPVILLAGERSINTREE2 = 14;
        public const int FOREST_GOMEETPOINT = 15;
        public const int FOREST_GOTOCLUB = 16;
        public const int FOREST_GETSWAGGY = 17;
        public const int FOREST_ENTERCLUB = 18;
        public const int FOREST_CHALLENGEROBOT = 19;
        public const int FOREST_TALKTORESISTANCE = 20;
        public const int FOREST_FINDPURPOSECARD = 21;
        public const int FOREST_FOLLOWTHEREBELS = 22;
        public const int FOREST_BEATROBOT = 23;
        public const int FOREST_CELEBRATEVICTORY = 24;
        public const int FOREST_CELEBRATEVICTORY2 = 25;
        public const int MOUNTAIN_GETBACKMOUNTAIN = 26;
        public const int MOUNTAIN_BEATGIANTROBOT = 27;
        public const int FOREST_CELEBRATEVICTORYMOUNTAINBOSS = 28;
        public const int FOREST_FIXELECTRICITY = 29;
        public const int FOREST_GOTOTHELAKE = 30;
        public const int LAKE_INSPECT = 31;
        public const int LAKE_FINDKEY = 32;
        public const int LAKE_ENTERMINE = 33;
        public const int MINE_FOLLOWWORM = 34;
        public const int MINE_FOLLOWWORM2 = 35;
        public const int MINE_FINDERNIE = 36;
        public const int MINE_BRINGBACKERNIE = 37;
        public const int MINE_TALKTOREBELS = 38;
        public const int LAKE_FIGHTROBOT = 39;
        public const int HQ_OOPS = 40;
        public const int HQ_ARCHDPT1 = 41;
        public const int HQ_ARCHDPT2 = 42;
        public const int HQ_ESCAPE3 = 43;
        public const int HQ_ESCAPE_HELPVILLAGERS = 44;
        public const int HQ_BEATSUNSHINE1 = 45;
        public const int HQ_BEATSUNSHINE = 46;
    }

    public static class ArchipelagoConstants
    {
        // The AP game name - must match the apworld's game name exactly
        public const string GameName = "Pikuniku";

        // ========== ID CONSTANTS =========================================
        public const long BaseId = 100;

        // ========== DATA CONSTANTS =======================================

        // ========== LOCATION CONSTANTS (what you check in-game) ==========
        public static readonly Dictionary<string, long> Locations = new Dictionary<string, long>
        {
            { "Walking Piku Trophy", BaseId + 0 },
            { "The Hidden Rock Trophy", BaseId + 1 },
            { "Baskick Champion Trophy", BaseId + 2 },
            { "Sam The Slime Trophy", BaseId + 3 },
            { "The Resistance Trophy", BaseId + 4 },
            { "A Giant Robot Trophy", BaseId + 5 },
            { "Demonic Toast Trophy", BaseId + 6 },
            { "PikDug Trophy", BaseId + 7 },
            { "Piku at The Beach Trophy", BaseId + 8 },
            { "The Worms Trophy", BaseId + 9 },
            { "Ernie the Worm Trophy", BaseId + 10 },
            { "Sunshine Inc. Robot Trophy", BaseId + 11 },
            { "Mr. Sunshine Trophy", BaseId + 12 },
            { "Valley Dancing Bug", BaseId + 13 },
            { "Road to Forest Dancing Bug", BaseId + 14 },
            { "Forest Dancing Bug", BaseId + 15 },
            { "Cave Dancing Bug", BaseId + 16 },
            { "Sunshine HQ Dancing Bug", BaseId + 17 },
            { "Valley Plush Purchase", BaseId + 18 },
            { "Forest Sunglasses Purchase", BaseId + 19 },
            { "Forest Postcard Purchase", BaseId + 20 },
            { "Forest X-Ray Goggles Purchase", BaseId + 21 },
            { "Pencil Hat", BaseId + 22 },
            { "Apple 1", BaseId + 23 },
            { "Apple 2", BaseId + 24 },
            { "Apple 3", BaseId + 25 },
            { "Beast Mask", BaseId + 26 },
            { "Flower Hat", BaseId + 27 },
            { "Draw on the Tree", BaseId + 28 },
            { "Magnetic Card", BaseId + 29 },
            { "Defeat the First Giant Robot", BaseId + 30 },
            { "Water Hat", BaseId + 31 },
            { "Golden Tooth from the Silver Frog", BaseId + 32 },
            { "Some Arms", BaseId + 33 },
            { "Defeat the Second Giant Robot", BaseId + 34 },
            { "The Cabin Key", BaseId + 35 },
            { "A Video Game", BaseId + 36 },
            { "A Detonator", BaseId + 37 },
            { "Defeat the Third Giant Robot", BaseId + 38 },
            { "Piku & Niku I Trophy", BaseId + 39 },
            { "Piku & Niku II Trophy", BaseId + 40 },
            { "Piku & Niku III Trophy", BaseId + 41 },
            { "Piku & Niku IV Trophy", BaseId + 42 },
            { "Piku & Niku V Trophy", BaseId + 43 },
            { "Co-op Level 1 Complete", BaseId + 44 },
            { "Co-op Level 2 Complete", BaseId + 45 },
            { "Co-op Level 3 Complete", BaseId + 46 },
            { "Co-op Level 4 Complete", BaseId + 47 },
            { "Co-op Level 5 Complete", BaseId + 48 },
            { "Co-op Level 6 Complete", BaseId + 49 },
            { "Co-op Level 7 Complete", BaseId + 50 },
            { "Co-op Level 8 Complete", BaseId + 51 },
            { "Co-op Level 9 Complete", BaseId + 52 },
            { "Valley: Coin near windmill 1", BaseId + 53 },
            { "Valley: Coin near windmill 2", BaseId + 54 },
            { "Valley: Coin near windmill 3", BaseId + 55 },
            { "Valley: Coin above shop", BaseId + 56 },
            { "Valley: Coin above umbrella", BaseId + 57 },
            { "Valley: Coin above cloud 1", BaseId + 58 },
            { "Valley: Coin above cloud 2", BaseId + 59 },
            { "Valley: Coin above cloud 3", BaseId + 60 },
            { "Valley: Coin above flower house", BaseId + 61 },
            { "Valley: Coin left under moving bridge", BaseId + 62 },
            { "Valley: Coin right under moving bridge", BaseId + 63 },
            { "Valley: Coin above lower cornfield 1", BaseId + 64 },
            { "Valley: Coin above lower cornfield 2", BaseId + 65 },
            { "Valley: Coin above lower cornfield 3", BaseId + 66 },
            { "Valley: Coin above lower cornfield 4", BaseId + 67 },
            { "Valley: Coin in air between cornfields 1", BaseId + 68 },
            { "Valley: Coin in air between cornfields 2", BaseId + 69 },
            { "Valley: Coin above upper cornfield 1", BaseId + 70 },
            { "Valley: Coin above upper cornfield 2", BaseId + 71 },
            { "Valley: Coin above upper cornfield 3", BaseId + 72 },
            { "Valley: Coin above upper cornfield 4", BaseId + 73 },
            { "Apple Temple: Coin next to spring", BaseId + 74 },
            { "Apple Temple: Coin on spike trap", BaseId + 75 },
            { "Apple Temple: Coin on first platform between spikes", BaseId + 76 },
            { "Apple Temple: Coin on second platform between spikes", BaseId + 77 },
            { "Apple Temple: Coin near hidden room", BaseId + 78 },
            { "Apple Temple: Coin after breakable rock 1", BaseId + 79 },
            { "Apple Temple: Coin after breakable rock 2", BaseId + 80 },
            { "Apple Temple: Coin after breakable rock 3", BaseId + 81 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 1", BaseId + 82 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 2", BaseId + 83 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 3", BaseId + 84 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 4", BaseId + 85 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 5", BaseId + 86 },
            { "Apple Temple: Coin requiring 2 buttons puzzle 6", BaseId + 87 },
            { "Apple Temple: Coin between spike ceilings", BaseId + 88 },
            { "Apple Temple: Coin at start of bounce pad chain", BaseId + 89 },
            { "Apple Temple: Coin at end of temple 1", BaseId + 90 },
            { "Apple Temple: Coin at end of temple 2", BaseId + 91 },
            { "Apple Temple: Coin at end of temple 3", BaseId + 92 },
            { "Apple Temple: Coin at end of temple 4", BaseId + 93 },
            { "Valley Road: Coin at start", BaseId + 94 },
            { "Valley Road: Coin near hooks", BaseId + 95 },
            { "Valley Road: Coin on clouds above hooks", BaseId + 96 },
            { "Valley Road: Coin on clouds after boulders", BaseId + 97 },
            { "Valley Road: Coin in mushroom cave", BaseId + 98 },
            { "Valley Road: Coin on lower cave after door 1", BaseId + 99 },
            { "Valley Road: Coin on lower cave after door 2", BaseId + 100 },
            { "Valley Road: Coin on lower cave after door 3", BaseId + 101 },
            { "Valley Road: Coin on upper cloud near flower", BaseId + 102 },
            { "Valley Road: Coin on moving cloud 1", BaseId + 103 },
            { "Valley Road: Coin on moving cloud 2", BaseId + 104 },
            { "Valley Road: Coin on moving cloud 3", BaseId + 105 },
            { "Valley Road: Coin on moving cloud 4", BaseId + 106 },
            { "Valley Road: Coin on moving cloud 5", BaseId + 107 },
            { "Valley Road: Coin on moving cloud 6", BaseId + 108 },
            { "Valley Road: Coin on moving cloud 7", BaseId + 109 },
            { "Valley Road: Coin on moving cloud 8", BaseId + 110 },
            { "Valley Road: Coin on moving cloud 9", BaseId + 111 },
            { "Valley Road: Coin on moving cloud 10", BaseId + 112 },
            { "Valley Road: Coin on moving cloud 11", BaseId + 113 },
            { "Valley Road: Coin on moving cloud 12", BaseId + 114 },
            { "Valley Road: Coin on moving cloud 13", BaseId + 115 },
            { "Valley Road: Coin on moving cloud 14", BaseId + 116 },
            { "Valley Road: Coin at end of moving clouds", BaseId + 117 },
            { "Valley Road: Coin in hook upper hidden room 1", BaseId + 118 },
            { "Valley Road: Coin in hook upper hidden room 2", BaseId + 119 },
            { "Valley Road: Coin in hook upper hidden room 3", BaseId + 120 },
            { "Valley Road: Coin in hook upper hidden room 4", BaseId + 121 },
            { "Valley Road: Coin on zipline", BaseId + 122 },
            { "Valley Road: Coin on tree branch", BaseId + 123 },
            { "Valley Road: Coin near trees", BaseId + 124 },
            { "Valley Road: Coin in water hat area room 1", BaseId + 125 },
            { "Valley Road: Coin in water hat area room 2", BaseId + 126 },
            { "Valley Road: Coin in water hat area room 3", BaseId + 127 },
            { "Valley Road: Coin in water hat area room 4", BaseId + 128 },
            { "Forest: Coin on cut down log", BaseId + 129 },
            { "Forest: Coin on triple cut down logs 1", BaseId + 130 },
            { "Forest: Coin on triple cut down logs 2", BaseId + 131 },
            { "Forest: Coin on triple cut down logs 3", BaseId + 132 },
            { "Forest: Coin on ramp", BaseId + 133 },
            { "Forest: Coin on first log gear", BaseId + 134 },
            { "Forest: Coin on top of first log gear log", BaseId + 135 },
            { "Forest: Coin on second log gear", BaseId + 136 },
            { "Forest: Coin on fourth log", BaseId + 137 },
            { "Forest: Coin between logs", BaseId + 138 },
            { "Forest: Coin on fifth log gear", BaseId + 139 },
            { "Forest: Coin under sixth log gear", BaseId + 140 },
            { "Forest: Coin near owl on log", BaseId + 141 },
            { "Forest: Coin after logs", BaseId + 142 },
            { "Forest: Coin on town tree branch left", BaseId + 143 },
            { "Forest: Coin on town tree branch right", BaseId + 144 },
            { "Forest: Coin above community house", BaseId + 145 },
            { "Forest: Coin to the right of toastopia house", BaseId + 146 },
            { "Forest: Coin after rope bridge right of houses", BaseId + 147 },
            { "Forest: Coin atop last tree branch 1", BaseId + 148 },
            { "Forest: Coin atop last tree branch 2", BaseId + 149 },
            { "Forest: Coin on moving cloud 1", BaseId + 150 },
            { "Forest: Coin on moving cloud 2", BaseId + 151 },
            { "Forest: Coin on moving cloud 3", BaseId + 152 },
            { "Forest: Coin on moving cloud 4", BaseId + 153 },
            { "Forest: Coin on moving cloud 5", BaseId + 154 },
            { "Forest: Coin on moving cloud 6", BaseId + 155 },
            { "Forest: Coin on moving cloud 7", BaseId + 156 },
            { "Forest: Coin on moving cloud 8", BaseId + 157 },
            { "Forest: Coin on moving cloud 9", BaseId + 158 },
            { "Forest: Coin on moving cloud 10", BaseId + 159 },
            { "Forest: Coin on moving cloud 11", BaseId + 160 },
            { "Forest: Coin on moving cloud 12", BaseId + 161 },
            { "Forest: Coin on moving cloud 13", BaseId + 162 },
            { "Forest: Coin on moving cloud 14", BaseId + 163 },
            { "Forest: Coin on moving cloud 15", BaseId + 164 },
            { "Frog Temple: Coin left of entrance 1", BaseId + 165 },
            { "Frog Temple: Coin left of entrance 2", BaseId + 166 },
            { "Frog Temple: Coin left of entrance 3", BaseId + 167 },
            { "Frog Temple: Coin on spike trap", BaseId + 168 },
            { "Frog Temple: Coin before hook", BaseId + 169 },
            { "Frog Temple: Coin after 2nd checkpoint", BaseId + 170 },
            { "Frog Temple: Coin above arrow pit", BaseId + 171 },
            { "Frog Temple: Coin at symbol puzzle hint", BaseId + 172 },
            { "Frog Temple: Coin at nut bridge puzzle 1", BaseId + 173 },
            { "Frog Temple: Coin at nut bridge puzzle 2", BaseId + 174 },
            { "Frog Temple: Coin at nut bridge puzzle 3", BaseId + 175 },
            { "Cave: Coin in spring tunnel 1", BaseId + 176 },
            { "Cave: Coin in spring tunnel 2", BaseId + 177 },
            { "Cave: Coin in spring tunnel 3", BaseId + 178 },
            { "Cave: Coin in spring tunnel 4", BaseId + 179 },
            { "Cave: Coin after first pipe tunnel 1", BaseId + 180 },
            { "Cave: Coin after first pipe tunnel 2", BaseId + 181 },
            { "Cave: Coin before detonator tunnel 1", BaseId + 182 },
            { "Cave: Coin before detonator tunnel 2", BaseId + 183 },
            { "Cave: Coin before detonator tunnel 3", BaseId + 184 },
            { "Cave: Coin before detonator tunnel 4", BaseId + 185 },
            { "Cave: Coin near moving platforms", BaseId + 186 },
            { "Cave: Coin after first resistance puzzle 1", BaseId + 187 },
            { "Cave: Coin after first resistance puzzle 2", BaseId + 188 },
            { "Cave: Coin hidden near plant", BaseId + 189 },
            { "Cave: Coin after second resistance puzzle", BaseId + 190 },
            { "Cave: Coin above worm room 1", BaseId + 191 },
            { "Cave: Coin above worm room 2", BaseId + 192 },
            { "Cave: Coin under worm room", BaseId + 193 },
            { "Cave: Coin above ernie worm", BaseId + 194 },
            { "Cave: Coin above gray spinning cross 1", BaseId + 195 },
            { "Cave: Coin above gray spinning cross 2", BaseId + 196 },
        };

        // ========== ITEM CONSTANTS (what you receive in-game) ============
        // IDs mirror the apworld's item table exactly (BaseId == 100). Note ItemHandler maps
        // server items to in-game assets by NAME, not by these IDs, so this table is the
        // authoritative reference but is not what drives the actual grants.
        public static readonly Dictionary<string, long> Items = new Dictionary<string, long>
        {
            { "Pencil Hat", BaseId + 0 },
            { "Water Hat", BaseId + 1 },
            { "Sunglasses", BaseId + 2 },
            { "X-Ray Glasses", BaseId + 3 },
            { "Flower Hat", BaseId + 4 },
            { "Beast Mask", BaseId + 5 },
            { "Some Arms", BaseId + 6 },
            { "Magnetic Card", BaseId + 7 },
            { "The Cabin Key", BaseId + 8 },
            { "A Detonator", BaseId + 9 },
            { "Apple", BaseId + 10 },
            { "The Golden Tooth from the Silver Frog", BaseId + 11 },
            { "Co-op Level Access", BaseId + 12 },
            { "A Video Game", BaseId + 13 },
            { "Sam the Slime Trophy", BaseId + 14 },
            { "The Resistance Trophy", BaseId + 15 },
            { "A Giant Robot Trophy", BaseId + 16 },
            { "The Demonic Toast Trophy", BaseId + 17 },
            { "PikDug Trophy", BaseId + 18 },
            { "Piku at the Beach Trophy", BaseId + 19 },
            { "The Worms Trophy", BaseId + 20 },
            { "Ernie the Worm Trophy", BaseId + 21 },
            { "Sunshine Inc. Robot Trophy", BaseId + 22 },
            { "Mr. Sunshine Trophy", BaseId + 23 },
            { "Baskick Champion Trophy", BaseId + 24 },
            { "Walking Piku Trophy", BaseId + 25 },
            { "The Hidden Rock Trophy", BaseId + 26 },
            { "Piku & Niku I Trophy", BaseId + 27 },
            { "Piku & Niku II Trophy", BaseId + 28 },
            { "Piku & Niku III Trophy", BaseId + 29 },
            { "Piku & Niku IV Trophy", BaseId + 30 },
            { "Piku & Niku V Trophy", BaseId + 31 },
            { "A Scary Plush", BaseId + 32 },
            { "Forest Postcard", BaseId + 33 },
            { "5 Coins", BaseId + 34 },
            { "Joyful Whimsy", BaseId + 35 }
        };

        // ========== COINSANITY ===========================================
        // One coin's in-game Collectible.UniqueID + world position -> its AP location name
        // (resolved to an ID via Locations above). Harvested from the F7 dump / pickup logs,
        // per scene. UniqueID is NOT unique for every coin (e.g. every "The Valley Road" coin
        // shares 70776915), so the world position disambiguates collisions. Use
        // ResolveCoinLocation(id, x, y) to look one up.
        public readonly struct CoinLoc
        {
            public readonly int Id;
            public readonly float X;
            public readonly float Y;
            public readonly string Name;
            public CoinLoc(int id, float x, float y, string name) { Id = id; X = x; Y = y; Name = name; }
        }

        public static readonly CoinLoc[] CoinLocations =
        {
            // ---- The Valley ----
            new CoinLoc(1199576228, 290.5f, -81.1f,  "Valley: Coin near windmill 1"),
            new CoinLoc(889188242,  293.2f, -81.1f,  "Valley: Coin near windmill 2"),
            new CoinLoc(889144834,  295.7f, -81.0f,  "Valley: Coin near windmill 3"),
            new CoinLoc(889149006,  364.8f, -57.6f,  "Valley: Coin above shop"),
            new CoinLoc(889178494,  377.8f, -60.2f,  "Valley: Coin above umbrella"),
            new CoinLoc(889169866,  413.2f, -26.6f,  "Valley: Coin above cloud 1"),
            new CoinLoc(889134516,  415.8f, -26.6f,  "Valley: Coin above cloud 2"),
            new CoinLoc(889122018,  418.4f, -26.6f,  "Valley: Coin above cloud 3"),
            new CoinLoc(889153857,  432.0f, -50.7f,  "Valley: Coin above flower house"),
            new CoinLoc(889161219,  453.0f, -97.0f,  "Valley: Coin left under moving bridge"),
            new CoinLoc(889106873,  469.0f, -103.0f, "Valley: Coin right under moving bridge"),
            new CoinLoc(889191034,  175.0f, -89.0f,  "Valley: Coin above lower cornfield 1"),
            new CoinLoc(889123450,  177.0f, -89.0f,  "Valley: Coin above lower cornfield 2"),
            new CoinLoc(889121971,  179.0f, -89.0f,  "Valley: Coin above lower cornfield 3"),
            new CoinLoc(1199575527, 181.0f, -89.0f,  "Valley: Coin above lower cornfield 4"),
            new CoinLoc(889188847,  158.4f, -77.8f,  "Valley: Coin in air between cornfields 1"),
            new CoinLoc(889156023,  158.4f, -80.8f,  "Valley: Coin in air between cornfields 2"),
            new CoinLoc(889186242,  172.0f, -74.0f,  "Valley: Coin above upper cornfield 1"),
            new CoinLoc(889184443,  173.9f, -73.9f,  "Valley: Coin above upper cornfield 2"),
            new CoinLoc(889118349,  175.9f, -73.9f,  "Valley: Coin above upper cornfield 3"),
            new CoinLoc(889135973,  178.0f, -74.0f,  "Valley: Coin above upper cornfield 4"),
            

            // ---- Apple Temple ----
            new CoinLoc(51908967,   11.4f,  7.1f,    "Apple Temple: Coin next to spring"),
            new CoinLoc(51846333,   15.0f,  -17.0f,  "Apple Temple: Coin on spike trap"),
            new CoinLoc(51821243,   33.0f,  -21.0f,  "Apple Temple: Coin on first platform between spikes"),
            new CoinLoc(895346862,  44.0f,  -21.0f,  "Apple Temple: Coin on second platform between spikes"),
            new CoinLoc(51833320,   82.0f,  -5.0f,   "Apple Temple: Coin near hidden room"),
            new CoinLoc(51883161,   109.6f, -17.4f,  "Apple Temple: Coin after breakable rock 1"),
            new CoinLoc(51834740,   111.6f, -17.4f,  "Apple Temple: Coin after breakable rock 2"),
            new CoinLoc(51853567,   113.6f, -17.4f,  "Apple Temple: Coin after breakable rock 3"),
            new CoinLoc(51825124,   163.2f, -40.8f,  "Apple Temple: Coin requiring 2 buttons puzzle 1"),
            new CoinLoc(51828777,   165.2f, -40.8f,  "Apple Temple: Coin requiring 2 buttons puzzle 2"),
            new CoinLoc(51834651,   167.2f, -40.8f,  "Apple Temple: Coin requiring 2 buttons puzzle 3"),
            new CoinLoc(51847808,   163.2f, -43.1f,  "Apple Temple: Coin requiring 2 buttons puzzle 4"),
            new CoinLoc(51829431,   165.2f, -43.1f,  "Apple Temple: Coin requiring 2 buttons puzzle 5"),
            new CoinLoc(51893689,   167.2f, -43.1f,  "Apple Temple: Coin requiring 2 buttons puzzle 6"),
            new CoinLoc(1723174925, 215.0f, -46.9f,  "Apple Temple: Coin between spike ceilings"),
            new CoinLoc(51854706,   226.1f, -35.8f,  "Apple Temple: Coin at start of bounce pad chain"),
            new CoinLoc(51885771,   271.6f, -22.7f,  "Apple Temple: Coin at end of temple 1"),
            new CoinLoc(51838646,   268.6f, -25.5f,  "Apple Temple: Coin at end of temple 2"),
            new CoinLoc(51878197,   268.6f, -22.7f,  "Apple Temple: Coin at end of temple 3"),
            new CoinLoc(1723174131, 265.6f, -22.7f,  "Apple Temple: Coin at end of temple 4"),

            // ---- The Valley Road ----  (almost every coin shares UniqueID 70776915; position disambiguates)
            new CoinLoc(70776915,   22.8f,  -76.0f,  "Valley Road: Coin at start"),
            new CoinLoc(70776915,   85.0f,  -63.0f,  "Valley Road: Coin near hooks"),
            new CoinLoc(70776915,   69.0f,  -37.7f,  "Valley Road: Coin on clouds above hooks"),
            new CoinLoc(70776915,   147.6f, -53.8f,  "Valley Road: Coin on clouds after boulders"),
            new CoinLoc(70776915,   209.0f, -63.0f,  "Valley Road: Coin in mushroom cave"),
            new CoinLoc(70776915,   246.1f, -82.9f,  "Valley Road: Coin on lower cave after door 1"),
            new CoinLoc(70776915,   253.5f, -82.9f,  "Valley Road: Coin on lower cave after door 2"),
            new CoinLoc(70776915,   260.9f, -82.9f,  "Valley Road: Coin on lower cave after door 3"),
            new CoinLoc(70776915,   216.5f, -17.2f,  "Valley Road: Coin on upper cloud near flower"),
            new CoinLoc(70776915,   190.0f, 1.4f,    "Valley Road: Coin on moving cloud 1"),
            new CoinLoc(70776915,   191.8f, 1.4f,    "Valley Road: Coin on moving cloud 2"),
            new CoinLoc(70776915,   193.5f, 1.4f,    "Valley Road: Coin on moving cloud 3"),
            new CoinLoc(70776915,   195.6f, 1.4f,    "Valley Road: Coin on moving cloud 4"),
            new CoinLoc(70776915,   190.0f, -0.6f,   "Valley Road: Coin on moving cloud 5"),
            new CoinLoc(70776915,   191.8f, -0.6f,   "Valley Road: Coin on moving cloud 6"),
            new CoinLoc(70776915,   193.5f, -0.6f,   "Valley Road: Coin on moving cloud 7"),
            new CoinLoc(70776915,   195.6f, -0.6f,   "Valley Road: Coin on moving cloud 8"),
            new CoinLoc(70776915,   204.4f, 1.4f,    "Valley Road: Coin on moving cloud 9"),
            new CoinLoc(70776915,   206.2f, 1.4f,    "Valley Road: Coin on moving cloud 10"),
            new CoinLoc(70776915,   207.9f, 1.4f,    "Valley Road: Coin on moving cloud 11"),
            new CoinLoc(70776915,   204.4f, -0.6f,   "Valley Road: Coin on moving cloud 12"),
            new CoinLoc(70776915,   206.2f, -0.6f,   "Valley Road: Coin on moving cloud 13"),
            new CoinLoc(70776915,   207.9f, -0.6f,   "Valley Road: Coin on moving cloud 14"),
            new CoinLoc(70776915,   140.9f, -5.0f,   "Valley Road: Coin at end of moving clouds"),
            new CoinLoc(70776915,   125.2f, -15.5f,  "Valley Road: Coin in hook upper hidden room 1"),
            new CoinLoc(70776915,   127.0f, -15.5f,  "Valley Road: Coin in hook upper hidden room 2"),
            new CoinLoc(70776915,   125.2f, -13.5f,  "Valley Road: Coin in hook upper hidden room 3"),
            new CoinLoc(70776915,   127.0f, -13.5f,  "Valley Road: Coin in hook upper hidden room 4"),
            new CoinLoc(70776915,   305.3f, -16.2f,  "Valley Road: Coin on zipline"),
            new CoinLoc(70776915,   320.4f, -29.1f,  "Valley Road: Coin on tree branch"),
            new CoinLoc(62192953,   334.8f, -41.2f,  "Valley Road: Coin near trees"),
            new CoinLoc(2085386414, 144.0f, -98.0f,  "Valley Road: Coin in water hat area room 1"),
            new CoinLoc(2092971986, 138.0f, -98.0f,  "Valley Road: Coin in water hat area room 2"),
            new CoinLoc(649749052,  133.0f, -98.0f,  "Valley Road: Coin in water hat area room 3"),
            new CoinLoc(2093405322, 128.6f, -98.0f,  "Valley Road: Coin in water hat area room 4"),

            // ---- The Forest ----
            new CoinLoc(1549184843, 55.0f,  -195.0f, "Forest: Coin on cut down log"),
            new CoinLoc(1549224845, 141.0f, -206.0f, "Forest: Coin on triple cut down logs 1"),
            new CoinLoc(1549205227, 149.0f, -206.0f, "Forest: Coin on triple cut down logs 2"),
            new CoinLoc(1549228498, 157.0f, -204.0f, "Forest: Coin on triple cut down logs 3"),
            new CoinLoc(1549214684, 189.8f, -209.3f, "Forest: Coin on ramp"),
            new CoinLoc(1667825996, 219.2f, -194.6f, "Forest: Coin on first log gear"),
            new CoinLoc(1549200294, 219.1f, -189.0f, "Forest: Coin on top of first log gear log"),
            new CoinLoc(1549220811, 245.0f, -187.0f, "Forest: Coin on second log gear"),
            new CoinLoc(1549151259, 277.0f, -163.0f, "Forest: Coin on fourth log"),
            new CoinLoc(1702486725, 298.0f, -164.8f, "Forest: Coin between logs"),
            new CoinLoc(1703946260, 327.0f, -158.7f, "Forest: Coin on fifth log gear"),
            new CoinLoc(1549188468, 339.1f, -185.6f, "Forest: Coin under sixth log gear"),
            new CoinLoc(1548603409, 363.0f, -137.0f, "Forest: Coin near owl on log"),
            new CoinLoc(1549154939, 374.0f, -146.0f, "Forest: Coin after logs"),
            new CoinLoc(1839019233, 488.9f, -163.9f, "Forest: Coin on town tree branch left"),
            new CoinLoc(1839992568, 497.3f, -155.4f, "Forest: Coin on town tree branch right"),
            new CoinLoc(1834202660, 529.1f, -152.1f, "Forest: Coin above community house"),
            new CoinLoc(1837535892, 605.1f, -156.0f, "Forest: Coin to the right of toastopia house"),
            new CoinLoc(154918642,  653.0f, -169.0f, "Forest: Coin after rope bridge right of houses"),
            new CoinLoc(1549206017, 658.0f, -153.0f, "Forest: Coin atop last tree branch 1"),
            new CoinLoc(1549213430, 658.0f, -144.6f, "Forest: Coin atop last tree branch 2"),
            new CoinLoc(1549218646, 626.1f, -105.7f, "Forest: Coin on moving cloud 1"),
            new CoinLoc(1549221410, 629.1f, -105.7f, "Forest: Coin on moving cloud 2"),
            new CoinLoc(1549196638, 632.1f, -105.7f, "Forest: Coin on moving cloud 3"),
            new CoinLoc(1549176235, 635.1f, -105.7f, "Forest: Coin on moving cloud 4"),
            new CoinLoc(1549150881, 638.1f, -105.7f, "Forest: Coin on moving cloud 5"),
            new CoinLoc(1549213502, 629.1f, -108.0f, "Forest: Coin on moving cloud 6"),
            new CoinLoc(1549208573, 632.1f, -108.0f, "Forest: Coin on moving cloud 7"),
            new CoinLoc(1549200835, 635.1f, -108.0f, "Forest: Coin on moving cloud 8"),
            new CoinLoc(1549186031, 638.1f, -108.0f, "Forest: Coin on moving cloud 9"),
            new CoinLoc(1549163596, 649.1f, -105.7f, "Forest: Coin on moving cloud 10"),
            new CoinLoc(1549170179, 652.1f, -105.7f, "Forest: Coin on moving cloud 11"),
            new CoinLoc(1549163589, 655.1f, -105.7f, "Forest: Coin on moving cloud 12"),
            new CoinLoc(1549185406, 649.1f, -108.0f, "Forest: Coin on moving cloud 13"),
            new CoinLoc(1549232206, 652.1f, -108.0f, "Forest: Coin on moving cloud 14"),
            new CoinLoc(1549211934, 655.1f, -108.0f, "Forest: Coin on moving cloud 15"),

            // ---- Temple of the Silver Frog ----
            new CoinLoc(740310583,  8.0f,   10.0f,   "Frog Temple: Coin left of entrance 1"),
            new CoinLoc(740050088,  11.0f,  10.0f,   "Frog Temple: Coin left of entrance 2"),
            new CoinLoc(739750294,  14.0f,  10.0f,   "Frog Temple: Coin left of entrance 3"),
            new CoinLoc(1099654487, 70.1f,  9.6f,    "Frog Temple: Coin on spike trap"),
            new CoinLoc(1096215408, 97.8f,  5.6f,    "Frog Temple: Coin before hook"),
            new CoinLoc(1096215408, 130.1f, 8.6f,    "Frog Temple: Coin after 2nd checkpoint"),
            new CoinLoc(522213601,  236.2f, -9.7f,   "Frog Temple: Coin above arrow pit"),
            new CoinLoc(525698043,  304.0f, -14.0f,  "Frog Temple: Coin at symbol puzzle hint"),
            new CoinLoc(1706505722, 354.2f, -17.1f,  "Frog Temple: Coin at nut bridge puzzle 1"),
            new CoinLoc(1706215749, 356.7f, -17.1f,  "Frog Temple: Coin at nut bridge puzzle 2"),
            new CoinLoc(1706088097, 359.3f, -17.1f,  "Frog Temple: Coin at nut bridge puzzle 3"),

            // ---- The Cave ----
            new CoinLoc(73042658,   202.0f, -20.0f,  "Cave: Coin in spring tunnel 1"),
            new CoinLoc(1182784534, 205.6f, -20.0f,  "Cave: Coin in spring tunnel 2"),
            new CoinLoc(1182624852, 209.1f, -20.0f,  "Cave: Coin in spring tunnel 3"),
            new CoinLoc(1182375973, 212.6f, -20.0f,  "Cave: Coin in spring tunnel 4"),
            new CoinLoc(1248839813, 153.6f, -157.3f, "Cave: Coin after first pipe tunnel 1"),
            new CoinLoc(1248221666, 153.6f, -160.0f, "Cave: Coin after first pipe tunnel 2"),
            new CoinLoc(1249179375, 142.7f, -157.3f, "Cave: Coin before detonator tunnel 1"),
            new CoinLoc(1249379767, 145.5f, -157.3f, "Cave: Coin before detonator tunnel 2"),
            new CoinLoc(1248403122, 142.7f, -160.0f, "Cave: Coin before detonator tunnel 3"),
            new CoinLoc(1248666468, 145.5f, -160.0f, "Cave: Coin before detonator tunnel 4"),
            new CoinLoc(110171987,  218.0f, -146.0f, "Cave: Coin near moving platforms"),
            new CoinLoc(110514548,  242.0f, -179.0f, "Cave: Coin after first resistance puzzle 1"),
            new CoinLoc(109996565,  245.0f, -179.0f, "Cave: Coin after first resistance puzzle 2"),
            new CoinLoc(119674886,  268.0f, -163.0f, "Cave: Coin hidden near plant"),
            new CoinLoc(119361553,  380.5f, -173.2f, "Cave: Coin after second resistance puzzle"),
            new CoinLoc(120455579,  444.9f, -167.0f, "Cave: Coin above worm room 1"),
            new CoinLoc(120475059,  457.4f, -167.0f, "Cave: Coin above worm room 2"),
            new CoinLoc(179300603,  471.2f, -204.8f, "Cave: Coin under worm room"),
            new CoinLoc(180586320,  497.0f, -221.0f, "Cave: Coin above ernie worm"),
            new CoinLoc(182877868,  552.7f, -198.4f, "Cave: Coin above gray spinning cross 1"),
            new CoinLoc(448907562,  559.8f, -202.4f, "Cave: Coin above gray spinning cross 2"),
        };

        // Resolve a picked-up coin to its AP location name. Matches on UniqueID, then on the
        // nearest recorded world position (coins are not guaranteed a unique UniqueID, so the
        // position breaks ties). Returns null if no coin shares that UniqueID.
        public static string ResolveCoinLocation(int uniqueId, float x, float y)
        {
            string best = null;
            float bestDist = float.MaxValue;
            foreach (var c in CoinLocations)
            {
                if (c.Id != uniqueId) continue;
                float dx = c.X - x, dy = c.Y - y;
                float d = dx * dx + dy * dy;
                if (d < bestDist) { bestDist = d; best = c.Name; }
            }
            return best;
        }

        // ========== HELPER METHODS =======================================
    }
}
