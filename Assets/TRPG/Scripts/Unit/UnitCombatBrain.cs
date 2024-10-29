using DevOpsGuy.GUI;
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
        [SerializeField] private Transform fireTransform;

        private UnitController context;

        private readonly SyncVar<int> fireCounter = new SyncVar<int>();
        private readonly SyncVar<float> currentHitChange = new SyncVar<float>();

        #region Server-Side fields [These fields cannot be read on client side]
        private List<HealthController> scannedEnemyList = new List<HealthController>();
        private int currentEnemyIndex;
        #endregion

        #region Client-Side fields
        private AimHUD aimHUD;
        #endregion

        public virtual void Setup(UnitController context)
        {
            this.context = context;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                aimHUD = UIManager.GetUI<AimHUD>();
            }
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
                OnSelectTarget(enemy);
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
            OnSelectTarget(enemy);
        }

        [Server]
        private void OnSelectTarget(HealthController enemy)
        {
            currentHitChange.Value = CalculateHitChance(enemy.GetComponent<UnitController>());
            
            OnSelectTargetCallback("", currentHitChange.Value);
        }

        [Server]
        private void OnDealingDamage()
        {
            if (!scannedEnemyList[currentEnemyIndex].IsDead)
            {
                if (IsHit(currentHitChange.Value))
                    scannedEnemyList[currentEnemyIndex].TakeDamage(context.WeaponManager.CurrentWeaponData.baseDamage);
            }  
        }

        [Server]
        private float CalculateHitChance(UnitController target)
        {
            float baseHitChance = context.WeaponManager.CurrentWeaponData.baseAccuracy - target.Data.evasion;
            Vector3 modifiedEnemyPos = new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z);

            float distance = (modifiedEnemyPos - fireTransform.position).magnitude;
            CoverType cover = DetermineCoverType(modifiedEnemyPos);

            baseHitChance -= (int)(distance * 2); // Decrease hit chance by 2% per unit distance

            // Adjust hit chance based on cover
            switch (cover)
            {
                case CoverType.None:
                    break; // No adjustment if there's no cover
                case CoverType.HalfCover:
                    baseHitChance -= 20; // Reduce hit chance by 20% for half cover
                    break;
                case CoverType.FullCover:
                    baseHitChance -= 40; // Reduce hit chance by 40% for full cover
                    break;
            }

            return Mathf.Clamp(baseHitChance, 0, 100);
        }

        [Server]
        private CoverType DetermineCoverType(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - fireTransform.position).normalized;
            RaycastHit hit;

            if (Physics.Raycast(targetPosition, -directionToTarget, out hit))
            {
                if (hit.collider.CompareTag("Cover"))
                {
                    Obstacle cover = hit.collider.GetComponent<Obstacle>();
                    return cover != null ? cover.CoverType : CoverType.None;
                }
            }
            return CoverType.None; // No cover if raycast doesn't hit anything
        }

        public bool IsHit(float hitChance)
        {
            int roll = Random.Range(0, 100);
            return roll < hitChance;
        }

        #region Callback
        [ObserversRpc]
        private void OnSelectTargetCallback(string enemyName, float hitChance)
        {
            if (IsOwner)
            {
                aimHUD.SetEnemyName(enemyName);
                aimHUD.SetHitRate(hitChance);
            }
        }
        #endregion

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
            OnDealingDamage();
        }

        [Client]
        private void OnDamageClient()
        {
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