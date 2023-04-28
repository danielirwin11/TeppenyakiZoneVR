using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabOutlineController : MonoBehaviour
{
    public Color IsGrabbedColor = new Color(0, 0, 0, 0);
    public Color InteractableNotCloseByColor = new Color(0, 0, 0, 0);
    public Color InteractableCloseByColor = Color.white;
    public Color InteractableReadyToPickup = Color.yellow;
    public Outline.Mode OutlineMode = Outline.Mode.OutlineVisible;
    [Range(1.0f, 8.0f)]
    public float OutlineWidth = 3;
    public float HighlightRange = 3.0f;

    private Outline outline;
    private OVRGrabbable grabbable;
    private List<OVRGrabber> collidingGrabbers = new List<OVRGrabber>();

    private void OnEnable()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.OutlineMode = OutlineMode;
        outline.OutlineWidth = OutlineWidth;
        outline.OutlineColor = InteractableCloseByColor;
        outline.enabled = false;
        outline.enabled = true;
        grabbable = GetComponent<OVRGrabbable>();
    }

    private void Update()
    {
        bool highlighted = false;
        bool inRange = false;
        if (!grabbable.isGrabbed && collidingGrabbers.Count == 0)
        {
            foreach (OVRGrabber grabber in CustomGrabber.Grabbers)
            {
                if (grabber is CustomGrabber)
                {
                    var distGrabber = (CustomGrabber)grabber;
                    if (distGrabber.hoveredGrabbable == grabbable && distGrabber.grabbedObject == null)
                    {
                        highlighted = true;
                        break;
                    }
                    if (Vector3.Distance(grabber.transform.position, grabbable.transform.position) < HighlightRange)
                    {
                        inRange = true;
                    }
                }
            }
        }

        outline.OutlineMode = OutlineMode;
        outline.OutlineWidth = OutlineWidth;
        outline.OutlineColor = InteractableCloseByColor;

        if (grabbable.isGrabbed)
            outline.OutlineColor = IsGrabbedColor;
        else if (highlighted || collidingGrabbers.Count > 0)
            outline.OutlineColor = InteractableReadyToPickup;
        else if (inRange)
            outline.OutlineColor = InteractableCloseByColor;
        else
            outline.OutlineColor = InteractableNotCloseByColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        OVRGrabber grabber = other.GetComponent<OVRGrabber>();
        if (grabber != null && !collidingGrabbers.Contains(grabber))
        {
            collidingGrabbers.Add(grabber);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        OVRGrabber grabber = other.GetComponent<OVRGrabber>();
        if (grabber != null)
        {
            collidingGrabbers.Remove(grabber);
        }
    }
}
