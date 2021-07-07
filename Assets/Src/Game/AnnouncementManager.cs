using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;

public struct Announcement {
    public string content;
}

//RED team's flag has been captured!
//mooseliot has been caught!
//mooselliot has been freed (ally)

public class AnnouncementManager : NetworkBehaviour
{
    Queue<Announcement> announcements = new Queue<Announcement>();
    bool isShowingAnnouncement = false;    

    // Start is called before the first frame update
    void Start()
    {
        //server side announcement trigger
        if(!IsServer) return;
        GameManager.Instance.OnFlagCaptured += OnFlagCaptured;
        GameManager.Instance.OnPlayerScored += OnPlayerScored;
        GameManager.Instance.OnPlayerJailed += OnPlayerJailed;
        GameManager.Instance.OnPlayerFreed += OnPlayerFreed;
        // announcements.Enqueue(new Announcement{content="mooselliot has been captured!"});
        // announcements.Enqueue(new Announcement{content="Your team has captured the enemy flag!"});
        // announcements.Enqueue(new Announcement{content="Your flag has been captured!"});
        // announcements.Enqueue(new Announcement{content="An ally has been freed"});
    }

    void OnFlagCaptured(Player player) {
        if(!IsServer) return;        
        AnnounceFlagCapturedClientRpc(player.GetTeam());
    }    

    void OnPlayerScored(Player player) {
        if(!IsServer) return;        
        string teamString = (player.GetTeam() == Team.BLUE) ? "Blue" : "Red";
        AnnounceStringClientRpc($"{teamString} team scores!");
    }

    void OnPlayerJailed(Player player, Player caputredBy) {
        if(!IsServer) return;        
        AnnouncePlayerCapturedClientRpc(player.GetUser());
    }
    
    void OnPlayerFreed(Player player, Player freedBy) {
        if(!IsServer) return;        
        AnnouncePlayerFreedClientRpc(player.GetUser());
    }

    void OnAnnouncementFinish() {
        isShowingAnnouncement = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(announcements.Count != 0 && !isShowingAnnouncement) {
            Announcement announcement = announcements.Dequeue();
            DisplayAnnouncement(announcement);
            isShowingAnnouncement = true;
        }
    }

    [ClientRpc]
    void AnnounceStringClientRpc(string content) {        
        announcements.Enqueue(new Announcement{content=content});
    }

    [ClientRpc] 
    void AnnounceFlagCapturedClientRpc(Team playerTeam) {
        Player localPlayer = PlayerController.LocalInstance.GetPlayer().syncPlayer;
        bool isMyFlag = (playerTeam != localPlayer.GetTeam());
        announcements.Enqueue(new Announcement{content=isMyFlag ? "Your flag has been captured by the enemy!" : "Your team has captured the enemy flag!"});
    }
    
    [ClientRpc] 
    void AnnouncePlayerCapturedClientRpc(User user) {
        //my player
        Player localPlayer = PlayerController.LocalInstance.GetPlayer().syncPlayer;
        bool isMyTeammate = (user.team == localPlayer.team);
        bool isMe = (user.clientId == localPlayer.OwnerClientId);
        string playerString = isMe ? "You have been" : (isMyTeammate ? "Ally" : "Enemy");
        announcements.Enqueue(new Announcement{content=$"{playerString} put in jail!"});
    }
    
    [ClientRpc] 
    void AnnouncePlayerFreedClientRpc(User user) {
        //only announce to teammates
        if(user.team != PlayerController.LocalInstance.GetPlayer().team) return;
        Player localPlayer = PlayerController.LocalInstance.GetPlayer().syncPlayer;
        bool isMe = (user.clientId == localPlayer.OwnerClientId);
        string announcementString = (isMe ? "You have" : "Ally has") + "been freed!";
        announcements.Enqueue(new Announcement{content=announcementString});
    }

    void DisplayAnnouncement(Announcement announcement) {
        string content = announcement.content;
        GetComponent<TMPro.TMP_Text>().text = content;
        Animation animation = GetComponent<Animation>();
        animation.Rewind();
        animation.Play();
    }
}
