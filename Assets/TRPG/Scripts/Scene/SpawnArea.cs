using UnityEngine;

namespace TRPG
{
    public class SpawnArea : MonoBehaviour
    {
        [SerializeField] private Transform[] pointList;

        private int currentPointIndex;

        public Transform GetPoint()
        {
            Transform _point = pointList[currentPointIndex];
            _point.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
            return _point;
        }

        public void IncreasePointIndex()
        {
            if (currentPointIndex >= pointList.Length - 1) return;

            currentPointIndex++;
        }
    }
}