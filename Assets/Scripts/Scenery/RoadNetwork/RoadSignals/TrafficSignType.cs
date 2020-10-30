namespace Scenery.RoadNetwork.RoadSignals {
    public enum TrafficSignType {
        Undefined = 0,
        MaximumSpeedLimit = 274,
        MinimumSpeedLimit = 275,
        EndOfMaximumSpeedLimit = 278,
        EndOfMinimumSpeedLimit = 279,
        EndOffAllSpeedLimitsAndOvertakingRestrictions = 282,
        TownBegin = 310,
        TownEnd = 311,
        Zone30Begin = 2741,                // 274.1
        Zone30End = 2742,                  // 274.2
        TrafficCalmedDistrictBegin = 3251, // 325.1
        TrafficCalmedDistrictEnd = 3252,   // 325.2
        EnvironmentalZoneBegin = 2701,     // 270.1
        EnvironmentalZoneEnd = 2702,       // 270.2
        OvertakingBanBegin = 276,
        OvertakingBanEnd = 280,
        OvertakingBanForTrucksBegin = 277,
        OvertakingBanForTrucksEnd = 281,
        RightOfWayBegin = 306,
        RightOfWayEnd = 307,
        RightOfWayNextIntersection = 301,
        Stop = 206,
        DoNotEnter = 267,
        HighWayBegin = 3301, // 330.1
        HighWayEnd = 3302,   // 330.2
        HighWayExit = 333,
        GiveWay = 205,
    }
}