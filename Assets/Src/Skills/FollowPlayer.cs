using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject fromPlayer;
    public GameObject toPlayer;
    Vector3 position;
    float time = 0.4f;
    private float curtime = 0f;

    public int state = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        curtime += Time.deltaTime;
        if(state == 0) {
            float progress = curtime/time;
            this.transform.position = Vector3.Lerp(fromPlayer.transform.position, toPlayer.transform.position, EaseOutPoly(progress, 2));
            if(progress >= 1) state = 1;
        }
        else if(state == 1) {
            this.transform.position = toPlayer.transform.position;
            if(curtime >= 2) {
                GameObject.Destroy(this.gameObject);
            }
        }
    }

        //returns a quadratic function that eases the input from 0 -> 1 
    float EaseOutPoly(float x, float exp)
    {
        float y = 1 - Mathf.Pow(x - 1, exp);
        return y;
    }

    //returns a circle function that eases the input from 0 -> 1 
    //\sqrt{\left(1-\left(x-1\right)^{2}\right)}
    float EaseOutCircular(float x)
    {
        float y = Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
        return y;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     LocalPlayer col = other.gameObject.GetComponent<LocalPlayer>();
    //     if (col == player)
    //     {
    //         Destroy(gameObject);
    //     }
    // }
}
