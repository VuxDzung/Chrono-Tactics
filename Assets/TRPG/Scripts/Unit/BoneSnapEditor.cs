using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace TRPG.Unit
{
    [CustomEditor(typeof(BoneSnapController))]
    [CanEditMultipleObjects]
    public class BoneSnapEditor : Editor
    {
        public BoneSnapController controller;

        private void OnEnable()
        {
            controller = (BoneSnapController)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Align Bone Refs"))
                controller.AlignBoneRefs();
        }
    }
}
#endif