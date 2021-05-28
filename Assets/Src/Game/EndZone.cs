using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndZone : MonoBehaviour
{
    public Team team = Team.BLUE;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider hit) {
        Flag flag = hit.gameObject.GetComponent<Flag>();
        if(flag && flag.team != team) {
            GameManager.Instance.ScorePoint(team);
        }
    }
}
