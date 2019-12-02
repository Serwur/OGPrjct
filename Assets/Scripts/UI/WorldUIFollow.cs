using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIFollow : MonoBehaviour
{
    private Camera cam;

    public Transform LookAt { get; set; }
    public Vector3 Offset { get; set; }

    private void Awake()
    {
        transform.parent.GetComponent<UIHolder>().ClickEOverCollectableItem = gameObject;
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        
        cam = Camera.main;

    }

    void Update()
    {

        Vector3 pos = cam.WorldToScreenPoint(LookAt.position + Offset);

        if (transform.position != pos)
            transform.position = pos;
        
    }
}
