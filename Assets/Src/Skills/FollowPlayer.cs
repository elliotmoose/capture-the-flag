using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public LocalPlayer player;
    Vector3 position;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        position = player.transform.position;
        Vector3 direction = position - this.transform.position;
        this.transform.position += direction.normalized * Time.deltaTime * 25;
        direction.y = 0;
        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        LocalPlayer col = other.gameObject.GetComponent<LocalPlayer>();
        if (col == player)
        {
            Destroy(gameObject);
        }
    }
}
