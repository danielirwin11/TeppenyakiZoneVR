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

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ActiveUIHand : MonoBehaviour
{
    OVRCameraRig m_CameraRig;
    OVRInputModule m_InputModule;

    public Transform left;
    public Transform right;

    private int resetFrames = 2;
    private int currentBuffer = 0;

    void Start()
    {
        m_CameraRig = FindObjectOfType<OVRCameraRig>();
        m_InputModule = FindObjectOfType<OVRInputModule>();
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.Any, OVRInput.Controller.LTouch))
        {
            SetActiveController(OVRInput.Controller.LTouch);
            currentBuffer = 0;
        }

        else if (OVRInput.Get(OVRInput.Button.Any, OVRInput.Controller.RTouch))
        {
            SetActiveController(OVRInput.Controller.RTouch);
            currentBuffer = 0;
        }

        else
        {
            currentBuffer++;
            if (currentBuffer >= resetFrames)
                SetActiveController(OVRInput.Controller.RTouch);
        }

    }

    void SetActiveController(OVRInput.Controller c)
    {
        Transform t;
        if (c == OVRInput.Controller.LTouch)
        {
            t = left;
        }
        else
        {
            t = right;
        }
        m_InputModule.rayTransform = t;
    }
}
