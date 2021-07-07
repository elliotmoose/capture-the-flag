using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class JailManager : NetworkBehaviour
{
    public static JailManager Instance;

    void Awake() {
        Instance = this;
    }
    
    public Jail blueTeamJail;
    public Jail redTeamJail;

    public Jail JailForPlayerOfTeam(Team team) {
        return team == Team.BLUE ? redTeamJail : blueTeamJail;
    }
}
