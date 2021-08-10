using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;

public struct Announcement {
    public string content;
    public AudioClip audioClip;
}

//RED team's flag has been captured!
//mooseliot has been caught!
//mooselliot has been freed (ally)

public class AnnouncementManager : NetworkBehaviour
{
    Queue<Announcement> announcements = new Queue<Announcement>();
    bool isShowingAnnouncement = false;    

    public AudioClip enemyCapturedAudio;
    public AudioClip allyCapturedAudio;
    public AudioClip youHaveBeenCapturedAudio;
    public AudioClip enemyFlagCapturedAudio;
    public AudioClip allyFlagCapturedAudio;
    public AudioClip redTeamWinAudio;
    public AudioClip blueTeamWinAudio;
    public AudioClip roundBeginAudio;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        //server side announcement trigger
        audioSource = GetComponent<AudioSource>();
        if(!IsServer) return;
        GameManager.Instance.OnFlagCaptured += OnFlagCaptured;
        GameManager.Instance.OnFlagScored += OnScored;
        GameManager.Instance.OnCaughtLastPlayer += OnScored;
        GameManager.Instance.OnPlayerJailed += OnPlayerJailed;
        GameManager.Instance.OnPlayerFreed += OnPlayerFreed;
        GameManager.Instance.OnRoundStart += OnRoundStart;
        // announcements.Enqueue(new Announcement{content="mooselliot has been captured!"});
        // announcements.Enqueue(new Announcement{content="Your team has captured the enemy flag!"});
        // announcements.Enqueue(new Announcement{content="Your flag has been captured!"});
        // announcements.Enqueue(new Announcement{content="An ally has been freed"});
    }

    void OnRoundStart() {
        if(!IsServer) return;
        AnnounceRoundStartClientRpc();
    }
    void OnFlagCaptured(Player player) {
        if(!IsServer) return;        
        AnnounceFlagCapturedClientRpc(player.GetTeam());
    }    

    void OnScored(Player player) {
        if(!IsServer) return;
        AnnounceRoundEndClientRpc(player.team);
    }

    void OnPlayerJailed(Player player, Player caputredBy) {
        if(!IsServer) return;        
        // AnnouncePlayerCapturedClientRpc(player.GetUser());
    }
    
    void OnPlayerFreed(Player player, Player freedBy) {
        if(!IsServer) return;        
        // AnnouncePlayerFreedClientRpc(player.GetUser());
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
            AudioAnnouncement(announcement);
            isShowingAnnouncement = true;
        }
    }

    [ClientRpc]
    void AnnounceRoundStartClientRpc() {
        announcements.Enqueue(new Announcement{content="", audioClip=roundBeginAudio});
    }
 

    [ClientRpc]
    void AnnounceStringClientRpc(string content, bool clearOld=false) {
        if(clearOld) {
            announcements.Clear();    
        }

        announcements.Enqueue(new Announcement{content=content});
    }
 
    [ClientRpc]
    void AnnounceRoundEndClientRpc(Team winningTeam) {
        announcements.Clear(); //overwrite old announcements
        LocalPlayer thisClientPlayer = PlayerController.LocalInstance.GetPlayer();
        AudioClip audioClip = (winningTeam == Team.RED ? redTeamWinAudio : blueTeamWinAudio);
        bool isMyTeam = (winningTeam == thisClientPlayer.team);
        announcements.Enqueue(new Announcement{content=isMyTeam ? "Round Won!" : "Round Lost", audioClip=audioClip});
    }

    [ClientRpc] 
    void AnnounceFlagCapturedClientRpc(Team playerTeam) {
        LocalPlayer thisClientPlayer = PlayerController.LocalInstance.GetPlayer();
        bool isMyFlag = (playerTeam != thisClientPlayer.team);
        AudioClip audioClip = (isMyFlag ? allyFlagCapturedAudio : enemyFlagCapturedAudio);
        announcements.Enqueue(new Announcement{content=isMyFlag ? "Your flag has been captured by the enemy!" : "Your team has captured the enemy flag!", audioClip=audioClip});
    }
    
    [ClientRpc] 
    void AnnouncePlayerCapturedClientRpc(User capturedUser) {
        //my player
        LocalPlayer thisClientPlayer = PlayerController.LocalInstance.GetPlayer();
        bool isMyTeammate = (capturedUser.team == thisClientPlayer.team);
        bool isMe = (capturedUser.clientId == thisClientPlayer.OwnerClientId);
        string playerString = isMe ? "You have been" : (isMyTeammate ? "Ally" : "Enemy");        
        AudioClip audioClip = (isMe ? youHaveBeenCapturedAudio : (isMyTeammate ? allyCapturedAudio : enemyFlagCapturedAudio));
        announcements.Enqueue(new Announcement{content=$"{playerString} put in jail!", audioClip=audioClip});
    }
    
    [ClientRpc] 
    void AnnouncePlayerFreedClientRpc(User freedUser) {
        //only announce to teammates
        LocalPlayer thisClientPlayer = PlayerController.LocalInstance.GetPlayer();
        if(freedUser.team != thisClientPlayer.team) return;
        bool isMe = (freedUser.clientId == thisClientPlayer.OwnerClientId);
        string announcementString = (isMe ? "You have" : "Ally has") + " been freed!";
        announcements.Enqueue(new Announcement{content=announcementString});
    }

    void DisplayAnnouncement(Announcement announcement) {
        string content = announcement.content;
        GetComponent<TMPro.TMP_Text>().text = content;
        Animation animation = GetComponent<Animation>();
        animation.Rewind();
        animation.Play();
    }

    void AudioAnnouncement(Announcement announcement) {
        if(announcement.audioClip != null) {
            audioSource.PlayOneShot(announcement.audioClip);
        }
    }
}
