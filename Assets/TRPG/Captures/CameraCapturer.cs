using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraCapturer : MonoBehaviour
{
    [SerializeField] private string savePath;

    public void Capture()
    {
        ScreenCapture.CaptureScreenshot($"{savePath}{GUID.Generate()}.png");
    }
}
