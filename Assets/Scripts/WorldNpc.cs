using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNpc : MonoBehaviour
{
    public float hp = 100f;
    public float damageTakenPerHit = 7f;

    public void TakeDamage()
    {
        hp -= damageTakenPerHit;

        if (hp <= 0)
        {
            hp = 0;
            gameObject.SetActive(false);
        }
    }
}
