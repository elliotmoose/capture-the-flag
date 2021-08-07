using MLAPI.Serialization;

public struct GameStat : INetworkSerializable {
    public User user;
    public ulong flagsScored;
    public ulong playersCaptured;
    public ulong playersFreed;
    public ulong timesInJail;
    public float timeInEnemyTerritory;
    public float timeWithFlag;
    public bool isMVP;

    public static GameStat operator +(GameStat x, GameStat y) {
        return new GameStat 
        {
            user = x.user,
            flagsScored = x.flagsScored + y.flagsScored,
            playersCaptured = x.playersCaptured + y.playersCaptured,
            playersFreed = x.playersFreed+ y.playersFreed,
            timesInJail = x.timesInJail + y.timesInJail,
            timeInEnemyTerritory = x.timeInEnemyTerritory+ y.timeInEnemyTerritory,
            timeWithFlag = x.timeWithFlag+ y.timeWithFlag,
            isMVP = y.isMVP || x.isMVP,
        };
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref user.clientId);
        serializer.Serialize(ref user.username);
        serializer.Serialize(ref user.team);
        serializer.Serialize(ref user.character);
        serializer.Serialize(ref flagsScored);
        serializer.Serialize(ref playersCaptured);
        serializer.Serialize(ref playersFreed);
        serializer.Serialize(ref timesInJail);
        serializer.Serialize(ref timeInEnemyTerritory);
        serializer.Serialize(ref timeWithFlag);
        serializer.Serialize(ref isMVP);
    }

    public float computedScore {
        get {
            return this.flagsScored * 10 + this.playersCaptured * 7 + this.playersFreed * 5 - this.timesInJail * 3;
        }
    }
}