using UnityEngine;

namespace TRPG
{
    public class SceneSpawnAreaManager : MonoBehaviour
    {
        public static SceneSpawnAreaManager S;

        private void Awake()
        {
            S = this;
        }

        [SerializeField] private SpawnArea[] area;

        private int currentAreaIndex;

        public SpawnArea GetArea()
        {
            return area[currentAreaIndex];
        }

        public void IncreaseAreaIndex()
        {
            if (currentAreaIndex >= area.Length - 1) return;

            currentAreaIndex++;
        }
    }
}