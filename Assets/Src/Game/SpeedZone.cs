using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    // Start is called before the first frame update
    const float speedIncrease = 12;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider) {
        LocalPlayer player = collider.gameObject.GetComponent<LocalPlayer>();
        if(player != null && player.IsOwner) {
            player.TakeEffect(new SpeedZoneEffect(player, speedIncrease));
        }
    }
    void OnTriggerExit(Collider collider) {
        LocalPlayer player = collider.gameObject.GetComponent<LocalPlayer>();
        if(player != null && player.IsOwner) {
            player.RemoveEffectWithName("SPEED_ZONE_EFFECT");
        }
    }
}
