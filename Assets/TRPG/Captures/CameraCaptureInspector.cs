using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(CameraCapturer))]
public class CameraCaptureInspector : Editor
{
    public CameraCapturer capturer;

    private void OnEnable()
    {
        capturer = (CameraCapturer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Capture"))
            capturer.Capture();
    }
}
#endif