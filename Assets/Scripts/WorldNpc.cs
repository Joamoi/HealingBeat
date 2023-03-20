using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNpc : MonoBehaviour
{
    public float hp = 100f;
    public float damageTakenPerHit = 7f;
    public GameObject testCloud;

    public void TakeDamage()
    {
        hp -= damageTakenPerHit;

        if (hp <= 0)
        {
            hp = 0;
            testCloud.SetActive(false);
            gameObject.layer = LayerMask.NameToLayer("Obstacles");
        }
    }
}
