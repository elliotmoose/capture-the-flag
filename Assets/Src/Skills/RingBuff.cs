using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuff : MonoBehaviour
{

    float radius = 10;
    LocalPlayer caster;
    float buff_factor = 1;

    // Start is called before the first frame update
    void Start()
    {
        caster = transform.GetComponentInParent<LocalPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, radius);

        foreach (Collider c in hitColliders)
        {
            LocalPlayer target = c.gameObject.GetComponent<LocalPlayer>();
            if (target != null)
            {
                if (caster.team != target.team)
                {
                    target.buffStamina(-buff_factor);
                }
                else
                {
                    target.buffStamina(buff_factor);
                }
            }
            
        }
    }
}
