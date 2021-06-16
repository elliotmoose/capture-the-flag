using MLAPI.Serialization;

public struct User : INetworkSerializable {
    public ulong clientId;
    public string username;
    public Team team;
    public Character character;

    public User(ulong clientId, Team team, string username, Character character=Character.Warrior) {
        this.clientId = clientId;
        this.team = team;
        this.username = username;
        this.character = character;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref clientId);
        serializer.Serialize(ref username);
        serializer.Serialize(ref team);
        serializer.Serialize(ref character);
    }

    public bool IsNull() {
        return username == "" && clientId == 0;
    }
}