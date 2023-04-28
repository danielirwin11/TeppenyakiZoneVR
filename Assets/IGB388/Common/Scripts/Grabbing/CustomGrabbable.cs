using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGrabbable : OVRGrabbable
{
    public static List<OVRGrabbable> Grabbables = new List<OVRGrabbable>();
    public static List<OVRGrabbable> DistanceGrabbables = new List<OVRGrabbable>();

    [Header("Distance Grab Settings")]
    public bool IsDistanceGrabbable;

    [Header("Events - Add Additional Actions Here")]
    [Space]
    public UnityEvent OnGrabStart = new UnityEvent();
    public UnityEvent OnGrabEnd = new UnityEvent();

    public virtual void OnEnable()
    {
        Grabbables.Add(this);
        if (IsDistanceGrabbable)
            DistanceGrabbables.Add(this);
    }

    public virtual void OnDisable()
    {
        Grabbables.Remove(this);
        if (IsDistanceGrabbable)
            DistanceGrabbables.Remove(this);
    }
}
