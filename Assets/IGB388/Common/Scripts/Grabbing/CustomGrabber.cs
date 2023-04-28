/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif



/// <summary>
/// Allows grabbing and throwing of objects with the DistanceGrabbable component on them.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CustomGrabber : OVRGrabber
{


    public static List<OVRGrabber> Grabbers = new List<OVRGrabber>();

    [Header("Hand Visuals")]
    public GameObject handModel;
    public bool hideWhenGrabbing = true;

    [Header("Hand Grabbing")]
    public bool AllowHandGrabbing = true;

    [Header("Distance Grabbing")]
    public bool AllowDistanceGrabbing = true;
    public float maxGrabDistance = 5.0f;
    public float grabThreshold = 0.8f;

    

    //bool m_movingObjectToHand = false;

    [HideInInspector] public OVRGrabbable hoveredGrabbable;

    private void OnEnable()
    {
        CustomGrabber.Grabbers.Add(this);
    }

    private void OnDisable()
    {
        CustomGrabber.Grabbers.Remove(this);
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();

        handModel.SetActive(grabbedObject == null);

        // Can grab an object.
        if (grabbedObject == null)
        {
            //if (hoveredGrabbable != null)
            //    hoveredGrabbable.GetComponent<Renderer>().material.color = Color.white;
            
            hoveredGrabbable = FindClosestDistanceGrabbable();
            
            //if (hoveredGrabbable != null)
            //{
            //    hoveredGrabbable.GetComponent<Renderer>().material.color = Color.red;
            //}
        }
    }


    public OVRGrabbable FindClosestGrabbable()
    {
        float closestMagSq = float.MaxValue;
        OVRGrabbable closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (OVRGrabbable grabbable in m_grabCandidates.Keys)
        {
            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }
        return closestGrabbable;
    }


    public OVRGrabbable FindClosestDistanceGrabbable()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxGrabDistance);

        foreach (RaycastHit hit in hits)
        {
            foreach (OVRGrabbable grabbable in CustomGrabbable.DistanceGrabbables)
            {
                if (grabbable.grabPoints.Contains(hit.collider))
                {
                    return grabbable;
                }
            }
        }

        OVRGrabbable toGrab = null;
        float grabDot = 0;
        foreach (OVRGrabbable grabbable in CustomGrabbable.DistanceGrabbables)
        {
            float dist = 0;
            float dot = 0;

            if (grabbable.snapOffset == null)
            {
                dist = Vector3.Distance(transform.position, grabbable.transform.position);
                dot = Vector3.Dot(transform.forward, (grabbable.transform.position - transform.position).normalized);
            }
            else
            {
                dist = Vector3.Distance(transform.position, grabbable.snapOffset.transform.position);
                dot = Vector3.Dot(transform.forward, (grabbable.snapOffset.transform.position - transform.position).normalized);
            }

            if (dist < maxGrabDistance && dot > grabThreshold && dot > grabDot)
            {
                toGrab = grabbable;
                grabDot = dot;
            }
        }
        return toGrab;
    }

    [HideInInspector] public float lastGrabAt;
    [HideInInspector] public float pullTime = 1.0f;
    [HideInInspector] public Vector3 startPosition = Vector3.zero;
    [HideInInspector] public Quaternion startRotation = Quaternion.identity;
    [HideInInspector] public Vector3 endPosition = Vector3.zero;
    [HideInInspector] public Quaternion endRotation = Quaternion.identity;
    Vector3 newPosition = Vector3.zero;
    Quaternion newRotation = Quaternion.identity;

    // called when the grab button on the controller changes states - doesn't mean a grab is starting
    protected override void GrabBegin()
    {
        // Find the grabbable to manipulate (if one exists)
        lastGrabAt = Time.time;
        OVRGrabbable closestGrabbable = AllowHandGrabbing ? FindClosestGrabbable() : null;
        if (closestGrabbable == null && AllowDistanceGrabbing)
        {
            closestGrabbable = hoveredGrabbable;
        }

        // If no grabbable found - exit.
        if (closestGrabbable == null)
        {
            return;
        }
        Collider closestGrabbableCollider = closestGrabbable.GetComponentInChildren<Collider>();
        GrabVolumeEnable(false);

        if (closestGrabbable != null)
        {
            if (closestGrabbable.isGrabbed)
            {
                ((CustomGrabber)closestGrabbable.grabbedBy).OffhandGrabbed(closestGrabbable);
            }

            m_grabbedObj = closestGrabbable;
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);
            SetPlayerIgnoreCollision(m_grabbedObj.gameObject, true);

            //m_movingObjectToHand = true;
            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            // Set up offsets for grabbed object desired position relative to hand.
            m_grabbedObjectPosOff = Vector3.zero;// m_gripTransform.localPosition;

            
            if (m_grabbedObj.snapOffset != null)
            {
                Vector3 snapOffset = m_grabbedObj.snapOffset.localPosition;
                if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                m_grabbedObjectPosOff += snapOffset;
            }
            else
            {
                Vector3 snapOffset = m_grabbedObj.grabPoints[0].bounds.center;
                if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                m_grabbedObjectPosOff += snapOffset;
            }
            

            m_grabbedObjectRotOff = m_gripTransform.localRotation;
            if (m_grabbedObj.snapOffset)
            {
                m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
            }

            startPosition = m_grabbedObj.transform.position;
            startRotation = m_grabbedObj.transform.rotation;
            endPosition = Vector3.zero;
            endRotation = Quaternion.identity;

            if (m_grabbedObj.snapOffset)
            {
                //endRotation = Quaternion.Inverse(m_grabbedObj.snapOffset.localRotation);
                endRotation = Quaternion.Inverse(m_grabbedObj.snapOffset.localRotation);// * Quaternion.Euler(270, 0, 0);

                Vector3 pos = m_grabbedObj.snapOffset.localPosition;
                pos.x *= m_grabbedObj.snapOffset.transform.parent.lossyScale.x;
                pos.y *= m_grabbedObj.snapOffset.transform.parent.lossyScale.y;
                pos.z *= m_grabbedObj.snapOffset.transform.parent.lossyScale.z;

                endPosition = -(endRotation * pos);
            }
        }
    }


    protected override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (m_grabbedObj == null)
        {
            return;
        }

        Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
        Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

        float ratio = (Time.time - lastGrabAt) / pullTime;
        Vector3 modifiedEndPosition = transform.position + transform.rotation * endPosition;
        Quaternion modifiedEndRotation = transform.rotation * endRotation;
        newPosition = Vector3.Lerp(startPosition, modifiedEndPosition, ratio);
        newRotation = Quaternion.Lerp(startRotation, modifiedEndRotation, ratio);

        grabbedRigidbody.MovePosition(newPosition);
        grabbedRigidbody.MoveRotation(newRotation.normalized);
    }

    // Just here to allow calling of a protected member function.
    protected override void OffhandGrabbed(OVRGrabbable grabbable)
    {
        base.OffhandGrabbed(grabbable);
    }
}

