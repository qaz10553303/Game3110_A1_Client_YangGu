using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 position=new Vector3(0,0,0);
    private float speed = 10;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.position = position;
        if (Input.GetKey(KeyCode.A)){
            position -= new Vector3(speed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            position += new Vector3(speed * Time.deltaTime,0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            position += new Vector3(0,speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            position -= new Vector3(0, speed * Time.deltaTime, 0);
        }
    }
}
