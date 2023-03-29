using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObject : MonoBehaviour
{
    private float moveSpeed;
    //public float yPosition;

    // Start is called before the first frame update
    void Start()
    {
        //transform.position += new Vector3(0f, yPosition, 0f);
        
        moveSpeed = Background.backgroundMoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= new Vector3(moveSpeed * Time.deltaTime, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DestroyPos")
        {
            Destroy(gameObject);
        }
    }
}
