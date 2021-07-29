using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using System;
using MLAPI.Messaging;
using MLAPI.Serialization;

public class EventLogManager : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("subscribed");
        //server side announcement trigger
        if(!IsServer) return;
        GameManager.Instance.OnFlagCaptured += OnFlagCaptured;
        GameManager.Instance.OnFlagPassed += OnFlagPassed;
        GameManager.Instance.OnPlayerScored += OnPlayerScored;
        GameManager.Instance.OnPlayerJailed += OnPlayerJailed;
        GameManager.Instance.OnPlayerEvade += OnPlayerEvade;
        GameManager.Instance.OnPlayerFreed += OnPlayerFreed;
        GameManager.Instance.OnRoundStart += OnRoundStart;
        // announcements.Enqueue(new Announcement{content="mooselliot has been captured!"});
        // announcements.Enqueue(new Announcement{content="Your team has captured the enemy flag!"});
        // announcements.Enqueue(new Announcement{content="Your flag has been captured!"});
        // announcements.Enqueue(new Announcement{content="An ally has been freed"});
    }

    string HtmlColorForTeam(Team team) {
        return $"#{(team == Team.BLUE ? ColorUtility.ToHtmlStringRGB(UIManager.Instance.colors.textBlue) : ColorUtility.ToHtmlStringRGB(UIManager.Instance.colors.textRed))}";
    }
    string GetTime() {
        TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.gameTime);
        return string.Format("[{0:D2}:{1:D2}] ", timeSpan.Minutes, timeSpan.Seconds);
    }

    void OnRoundStart() {
        if(!IsServer) return;        
        int roundNumber = GameManager.Instance.currentRoundNumber;
        LogEventClientRpc(GetTime()+$"===== Round {roundNumber} =====");
    }

    void OnFlagCaptured(Player player) {
        if(!IsServer) return;        
        Team opponentTeam = (player.team == Team.RED ? Team.BLUE : Team.RED);
        LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(player.team)}>{player.username}</color> has captured the <color={HtmlColorForTeam(opponentTeam)}>flag</color>");
    }    
  
    void OnFlagPassed(Player player, Player by) {
        if(!IsServer) return;        
        LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(player.team)}>{player.username}</color> has passed the flag to <color={HtmlColorForTeam(by.team)}>{by.username}</color>");
    }    

    void OnPlayerScored(Player player) {
        if(!IsServer) return;        
        LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(player.team)}>{player.username}</color> has scored a point!");
    }

    void OnPlayerJailed(Player player, Player capturedBy) {
        if(!IsServer) return;
        LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(capturedBy.team)}>{capturedBy.username}</color> sent <color={HtmlColorForTeam(player.team)}>{player.username}</color> to jail");

        //if player has flag, mention flag returned
        if(GameManager.Instance.PlayerHasFlag(player.localPlayer)) {
            LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(capturedBy.team)}>{capturedBy.username} has returned the flag!</color>");
        }
    }
    
    void OnPlayerFreed(Player player, Player freedBy) {
        if(!IsServer) return;    
        LogEventClientRpc(GetTime()+$"(team only) <color={HtmlColorForTeam(player.team)}>{player.username}</color> has been freed by <color={HtmlColorForTeam(freedBy.team)}>{freedBy.username}</color>", true, player.team);
    }

    void OnPlayerEvade(Player player, Player catcher) {
        if(!IsServer) return;    
        LogEventClientRpc(GetTime()+$"<color={HtmlColorForTeam(player.team)}>{player.username}</color> has evaded <color={HtmlColorForTeam(catcher.team)}>{catcher.username}</color>");
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    [ClientRpc]
    void LogEventClientRpc(string content, bool teamOnly=false, Team team=Team.BLUE) {
        if(!teamOnly || (teamOnly && PlayerController.LocalInstance.GetUser().team == team)) {
            UIManager.Instance.LogEvent(content);
        }    
    }
}
