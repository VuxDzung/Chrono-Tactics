using GameKit.Dependencies.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace TRPG
{
    public class SceneCamera : M_Singleton<SceneCamera>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float lerpSpeed = 10f;
        private const float tolerance = 0.01f;
        public void ResetCameraTransform()
        {
            StartCoroutine(LerpTransformCoroutine(_camera.transform, Vector3.zero, Quaternion.identity, true));
        }

        public virtual void MoveTo(Vector3 position)
        {
            StartCoroutine(LerpTransformCoroutine(transform, position, Quaternion.identity, false));
        }

        private IEnumerator LerpTransformCoroutine(Transform targetTransform, Vector3 position, Quaternion quaternion, bool lerpLocal)
        {
            while (Vector3.Distance((lerpLocal ? targetTransform.localPosition : targetTransform.position), position) > tolerance ||
                   Quaternion.Angle((lerpLocal ? targetTransform.localRotation : targetTransform.rotation), quaternion) > tolerance)
            {
                // Lerp position towards zero
                if (lerpLocal)
                {
                    targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, position, Time.deltaTime * lerpSpeed);
                    targetTransform.localRotation = Quaternion.Slerp(targetTransform.localRotation, quaternion, Time.deltaTime * lerpSpeed);
                }
                else
                {
                    targetTransform.position = Vector3.Lerp(targetTransform.position, position, Time.deltaTime * lerpSpeed);
                    targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, quaternion, Time.deltaTime * lerpSpeed);

                }

                yield return null; // Wait for the next frame and continue the loop
            }
        }
    }
}