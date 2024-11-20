using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG
{
    public class BlastSphere : MonoBehaviour
    {
        private static BlastSphere instance;

        public Transform outterVisual;

        private void Awake()
        {
            instance = this;
            Deactivate();
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                // Follow the world mouse position.
                transform.position = NetworkPlayer.GetMouseWorldPosition(MaskCategory.Ground);
            }
        }

        public static void Activate(float blastRadius = 5)
        {
            instance.outterVisual.localScale = new Vector3(blastRadius, blastRadius, blastRadius);
            instance.gameObject.SetActive(true);
        }

        public static void Deactivate()
        {
            instance.gameObject.SetActive(false);
        }
    }
}