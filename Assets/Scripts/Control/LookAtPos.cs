using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPos : MonoBehaviour
{
    public GameObject target;
    public float speed = 10;
    public bool isLookActive = false;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        if (isLookActive)
        {
            float singleStep = speed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, target.transform.localPosition, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            //transform.LookAt(new Vector3(transform.position.x, transform.position.y, mainCam.transform.position.z));
        }
    }
}
