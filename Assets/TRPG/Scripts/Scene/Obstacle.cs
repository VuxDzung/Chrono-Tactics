using System.Collections.Generic;
using UnityEngine;

namespace TRPG
{
    public enum CoverType
    {
        None,
        HalfCover,
        FullCover
    }

    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private CoverType type;
        [SerializeField] private List<ObstacleGridPosition> coverSpotList;

        public bool HasAvailableSpot
        {
            get
            {
                foreach (var spot in coverSpotList)
                {
                    if (!spot.HasBeenOccupied) return true;
                }
                return false;
            }
        }


        private void Start()
        {
            SceneObstacleManager.Singleton.RegisterObstacle(this);
        }

        public CoverType CoverType => type;

        public Vector3 GetAvailableCoverSpot()
        {
            Vector3 bestPosition = new Vector3(-1, 0, 0);

            foreach (var spot in coverSpotList)
            {
                if (!spot.HasBeenOccupied)
                {
                    bestPosition = spot.transform.position;
                    break;
                }
            }
            return bestPosition;
        }
    }
}