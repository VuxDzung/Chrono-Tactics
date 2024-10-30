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
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float rotateDuration = 1;
        [SerializeField] private Transform positionHandler;
        [SerializeField] private Transform rotationHandler;

        private const float tolerance = 0.01f;
        public void ResetCameraTransform()
        {
            StartCoroutine(LerpTransformCoroutine(_camera.transform, Vector3.zero, Quaternion.identity, true));
        }

        public virtual void Move(float horizontal, float vertical)
        {
            transform.Translate(new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime);
        }

        public virtual void MoveTo(Vector3 position, Quaternion rotation)
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

        public void Rotate(float yAngle, bool isLeftSide)
        {
            float yAxis = rotationHandler.eulerAngles.y;

            if (isLeftSide)
            {
                yAxis -= yAngle;  // Rotate 90 degrees counterclockwise
            }
            else
            {
                yAxis += yAngle;  // Rotate 90 degrees clockwise
            }

            // Apply the new rotation to the rotation handler

            StartCoroutine(LerpVector3(rotationHandler.rotation, new Vector3(
                rotationHandler.eulerAngles.x,
                yAxis,
                rotationHandler.eulerAngles.z
            )));
        }

        public Quaternion GetRotationHandler(float yAngle)
        {
            float yAxis = rotationHandler.eulerAngles.y;
            yAxis += yAngle;
            return Quaternion.Euler(rotationHandler.eulerAngles.x, yAxis, rotationHandler.eulerAngles.z);
        }

        private IEnumerator LerpVector3(Quaternion currentRotation, Vector3 target)
        {
            Quaternion startRotation = currentRotation;
            Quaternion endRotation = Quaternion.Euler(target);
            float elapsedTime = 0f;

            while (elapsedTime < rotateDuration)
            {
                currentRotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotateDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final rotation matches exactly
            currentRotation = endRotation;
        }
    }
}