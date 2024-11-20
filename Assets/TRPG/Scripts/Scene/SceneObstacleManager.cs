using System.Collections;
using System.Collections.Generic;
using TRPG;
using UnityEngine;
using Utils;

namespace TRPG
{
    public class SceneObstacleManager : M_Singleton<SceneObstacleManager>
    {
        [SerializeField] private List<Obstacle> obstacleList = new List<Obstacle>();

        private void Start()
        {
            
        }

        public virtual void RegisterObstacle(Obstacle obstacle)
        {
            if (obstacleList.Contains(obstacle)) return;

            obstacleList.Add(obstacle);
        }

        public virtual Obstacle GetClosestObstacleToTarget(Vector3 targetPosition)
        {
            Obstacle closestObstacle = obstacleList[0];
            foreach (var  obstacle in obstacleList)
            {
                if (obstacle.HasAvailableSpot)
                {
                    if (Vector3.Distance(targetPosition, closestObstacle.transform.position) < Vector3.Distance(targetPosition, obstacle.transform.position))
                    {
                        closestObstacle = obstacle;
                    }
                }
            }
            return closestObstacle;
        }
    }
}