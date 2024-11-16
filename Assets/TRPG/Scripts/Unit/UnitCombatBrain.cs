using DevOpsGuy.GUI;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class UnitCombatBrain : CoreNetworkBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private Transform fireTransform;

        private UnitController context;

        private readonly SyncVar<int> fireCounter = new SyncVar<int>();
        private readonly SyncVar<float> currentHitChange = new SyncVar<float>();
        private readonly SyncVar<bool> hasEnemy = new SyncVar<bool>();
        private readonly SyncVar<bool> IsInOverwatch = new SyncVar<bool>();

        #region Server-Side fields [These fields cannot be read on client side]
        private List<HealthController> scannedEnemyList = new List<HealthController>();
        private int currentEnemyIndex;
        private Vector3 endPosition;
        private bool hasAttackedInOverwatch;
        #endregion

        public bool HasEnemy => hasEnemy.Value;

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

        public override void OnServerUpdate()
        {
            base.OnServerUpdate();
            if (IsInOverwatch.Value)
            {
                List<Transform> enemyTransformList = DetectEnemiesInFOV();
                if (!hasAttackedInOverwatch && enemyTransformList.Count > 0)
                {
                    UnitController enemyController = enemyTransformList[0].GetComponent<UnitController>();
                    if (enemyController != null)
                    {
                        enemyController.Health.TakeDamage(context.WeaponManager.CurrentWeaponData.baseDamage);
                        hasAttackedInOverwatch = true;
                    }
                }
            }
        }

        [Server]
        public virtual void Scanning()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));
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
                hasEnemy.Value = true;
            }
            else
            {
                //Message: No enemy available!
                hasEnemy.Value = false;
            }
        }

        [Server]
        public virtual void ChangeToNextTarget()
        {
            if (scannedEnemyList.Count == 0) return;

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
            UnitController enemyUnit = enemy.GetComponent<UnitController>();
            currentHitChange.Value = CalculateHitChance(enemyUnit);
            
            OnSelectTargetCallback(enemyUnit.Data.unitName, currentHitChange.Value);
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

        [Server]
        public bool IsHit(float hitChance)
        {
            int roll = Random.Range(0, 100);
            return roll < hitChance;
        }

        [ServerRpc]
        private void SetEndPosition(Vector3 endPosition)
        {
            this.endPosition = endPosition;
        }

        #region Server-Logic [Overwatch] 
        [Server]
        public void StartOverwatch()
        {
            IsInOverwatch.Value = true;
        }

        [Server]
        public void ResetOverwatch()
        {
            IsInOverwatch.Value = false;
            hasAttackedInOverwatch = false;
        }

        /// <summary>
        /// Checks if a single enemy is within the unit's field of view.
        /// </summary>
        [Server]
        private bool IsEnemyInFOV(Transform enemy)
        {
            // Calculate direction and distance to the enemy
            Vector3 directionToEnemy = (enemy.position - transform.position);
            float distanceToEnemy = directionToEnemy.magnitude;

            // Check if within range
            if (distanceToEnemy > context.Data.viewRadius) return false;

            // Check if within field of view angle
            //float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);
            //if (angleToEnemy > viewAngle / 2) return false;

            // Optional: Check for line of sight with raycast
            if (Physics.Raycast(transform.position, directionToEnemy.normalized, out RaycastHit hit, context.Data.viewRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Obstacle)))
            {
                if (hit.transform != enemy) return false;  // Obstacle is blocking the view
            }

            // Enemy is within FOV and visible
            return true;
        }

        /// <summary>
        /// Detects and returns a list of all enemies within the unit's field of view.
        /// </summary>
        [Server]
        private List<Transform> DetectEnemiesInFOV()
        {
            List<Transform> enemiesInFOV = new List<Transform>();

            // Check for enemies within view range using a sphere overlap
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, context.Data.viewRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));

            foreach (Collider enemyCollider in enemiesInRange)
            {
                Transform enemyTransform = enemyCollider.transform;
                if (IsEnemyInFOV(enemyTransform))
                {
                    enemiesInFOV.Add(enemyTransform);
                }
            }

            return enemiesInFOV;
        }
        #endregion

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

        public void TossGrenade()
        {
            if (IsServerInitialized)
                OnTossGrenadeServer();

            if (IsClientInitialized)
                OnTossGrenadeClient();
        }


        [Server]
        private void OnTossGrenadeServer()
        {
            // - Spawn the grenade.
            WeaponData grenadeData = context.WeaponManager.Config.GetDataByType(WeaponType.Grenade);
            NetworkObject networkGrenadeObject = InstanceFinder.NetworkManager.GetPooledInstantiated(grenadeData.weaponPrefab.GetComponent<NetworkObject>(), context.BoneController.GetBoneRefByHandler(Handler.RightHand).transform.position, Quaternion.identity, true);
            ServerManager.Spawn(networkGrenadeObject, Owner);
            NetworkGrenade netGrenade = networkGrenadeObject.GetComponent<NetworkGrenade>();
            netGrenade.Toss(context.transform.position, endPosition);
        }

        [Client]
        private void OnTossGrenadeClient()
        {

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