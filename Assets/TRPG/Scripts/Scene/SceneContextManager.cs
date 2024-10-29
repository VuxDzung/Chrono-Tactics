using UnityEngine;

namespace TRPG
{
    public class SceneContextManager : MonoBehaviour
    {
        public static SceneContextManager S;

        [SerializeField] private Transform player1CameraStarterPos;
        [SerializeField] protected Transform player2CameraStarterPos;

        private void Awake()
        {
            S = this;
        }

        public Transform Player1CamPos => player1CameraStarterPos;
        public Transform Player2CamPos => player2CameraStarterPos;
    }
}