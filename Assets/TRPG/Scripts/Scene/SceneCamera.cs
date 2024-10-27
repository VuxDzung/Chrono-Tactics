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
            StartCoroutine(LerpCameraCoroutine());
        }

        private IEnumerator LerpCameraCoroutine()
        {
            while (Vector3.Distance(_camera.transform.localPosition, Vector3.zero) > tolerance ||
                   Quaternion.Angle(_camera.transform.localRotation, Quaternion.identity) > tolerance)
            {
                // Lerp position towards zero
                _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, Vector3.zero, Time.deltaTime * lerpSpeed);
                // Slerp rotation towards identity
                _camera.transform.localRotation = Quaternion.Slerp(_camera.transform.localRotation, Quaternion.identity, Time.deltaTime * lerpSpeed);

                yield return null; // Wait for the next frame and continue the loop
            }
        }
    }
}