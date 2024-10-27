using FishNet.Object;
using FishNet.Object.Synchronizing;
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

        private readonly SyncVar<int> fireCounter = new SyncVar<int>();

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

        #region Animation Events

        public void OnDamage()
        {
            if (IsServerInitialized)
                OnDamageServer();

            if (IsClientInitialized)
                OnDamageClient();
        }

        [Server]
        private void OnDamageServer()
        {
            
        }

        [Client]
        private void OnDamageClient()
        {
            Debug.Log("OnDamage");
            context.WeaponManager.CurrentWeapon.OnDamageTarget(IsOwner);
        }

        [Server]
        public virtual void Fire()
        {
            StartCoroutine(FireCoroutine());
        }

        private IEnumerator FireCoroutine()
        {
            WeaponData currentWeaponData = context.WeaponManager.CurrentWeaponData;
            while (fireCounter.Value < currentWeaponData.strikeCount)
            {
                context.AnimationController.TriggerFireAnimation();

                yield return new WaitForSeconds(currentWeaponData.delayBetweenStrike);

                fireCounter.Value++;
            }

            fireCounter.Value = 0;
        }
        #endregion
    }
}