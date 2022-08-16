using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer : MonoBehaviour
{
    public Transform target;
    public float height = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            transform.eulerAngles += new Vector3(0, 180 * Time.deltaTime, 0);
            transform.position = target.position + new Vector3(0, height, 0);
        }
    }
}
