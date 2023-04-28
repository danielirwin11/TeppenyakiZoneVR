using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLaser : MonoBehaviour
{
    private LineRenderer laser;
    public GameObject uiHit;

    private void OnEnable()
    {
        if (laser == null)
            laser = GetComponent<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
        {
            laser.SetPosition(1, new Vector3(0, 0, hit.distance));
            uiHit.SetActive(hit.collider.gameObject.layer == LayerMask.NameToLayer("UI"));
            uiHit.transform.position = transform.position + transform.forward * hit.distance;
        }
        else
        {
            laser.SetPosition(1, new Vector3(0, 0, 5));
            uiHit.SetActive(false);
        }

    }
}
