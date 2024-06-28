using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using System;

public class TESTXRSettingsDebug : MonoBehaviour
{
    [SerializeField] private Text feedback;     /// <summary>Feedback.</summary>
    public void Awake()
    {
        StartCoroutine(SwitchToVR(() => {
            Debug.Log("Switched to VR Mode");
        }));

        //For disable VR Mode
        XRSettings.enabled = false;
    }

    IEnumerator SwitchToVR(Action callback)
    {
        // Device names are lowercase, as returned by `XRSettings.supportedDevices`.
        // Google original, makes you specify
        // string desiredDevice = "daydream"; // Or "cardboard".
        // XRSettings.LoadDeviceByName(desiredDevice);
        // this is slightly better;

        string[] Devices = new string[] { "daydream", "cardboard" };
        XRSettings.LoadDeviceByName(Devices);

        // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
        yield return null;

        // Now it's ok to enable VR mode.
        XRSettings.enabled = true;
        callback.Invoke();
    }
    private void Start()
    {
        feedback.text += XRSettings.enabled.ToString();
    }
}