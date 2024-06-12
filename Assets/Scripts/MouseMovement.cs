using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
        GetComponent<CollisionObject>().SetSpeed(0);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }

}
