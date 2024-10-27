using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class UnitCombatBrain : CoreNetworkBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float range;

        private UnitController context;

        #region Server-Side fields [These fields cannot be read on client side]
        private List<HealthController> scannedEnemyList = new List<HealthController>();
        private int currentEnemyIndex;
        #endregion

        public virtual void Setup(UnitController context)
        {
            this.context = context;
        }

        [Server]
        public virtual void Scanning()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, enemyLayer);
            List<HealthController> enemyList = new List<HealthController>();
            foreach (Collider collider in colliders)
            {
                HealthController sceneUnit = collider.GetComponent<HealthController>();
                if (sceneUnit != null && sceneUnit.Owner != Owner)
                    enemyList.Add(sceneUnit);
            }
            scannedEnemyList = enemyList;

            //Aim to the first enemy.
            if (scannedEnemyList.Count > 0)
            {
                HealthController enemy = scannedEnemyList[currentEnemyIndex];
                transform.LookAt(enemy.transform.position);
            }
        }

        [Server]
        public virtual void ChangeToNextTarget()
        {
            currentEnemyIndex++;
            if (currentEnemyIndex >= scannedEnemyList.Count)
                currentEnemyIndex = 0;
            HealthController enemy = scannedEnemyList[currentEnemyIndex];
            transform.LookAt(enemy.transform.position);
        }
    }
}