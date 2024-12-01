using DevOpsGuy.GUI;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class UnitCombatBrain : CoreNetworkBehaviour
    {
        public static readonly int FULL_COVER_DECS_PERCENT = 40;
        public static readonly int HALF_COVER_DECS_PERCENT = 20;

        [SerializeField] private Transform fireTransform;

        public Action<bool> OnScanningComplete;
        public Action<bool, bool> OnScanningCompleteCallback;

        protected UnitController context;

        private readonly SyncVar<int> fireCounter = new SyncVar<int>();
        private readonly SyncVar<float> currentHitChange = new SyncVar<float>();
        private readonly SyncVar<bool> hasEnemy = new SyncVar<bool>(new SyncTypeSettings(0.01f, FishNet.Transporting.Channel.Reliable));
        private readonly SyncVar<bool> IsInOverwatch = new SyncVar<bool>();
        public readonly SyncVar<bool> IsGrenadeAbility = new SyncVar<bool>();
        private readonly SyncVar<Vector3> grenadeEndPosition = new SyncVar<Vector3>(new SyncTypeSettings(0.01f, FishNet.Transporting.Channel.Reliable));

        #region Server-Side fields [These fields cannot be read on client side]
        private List<HealthController> scannedEnemyList = new List<HealthController>();
        private int currentEnemyIndex;
        private bool hasAttackedInOverwatch;
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
                aimHUD = UIManager.GetUIStatic<AimHUD>();
            }
        }

        public override void OnServerUpdate()
        {
            base.OnServerUpdate();

            if (IsInOverwatch.Value && !context.UnitOwner.Value.IsOwnerTurn)
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

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            if (IsOwner)
            {
                if (IsGrenadeAbility.Value && context.IsSelected)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        SetEndPosition(NetworkPlayer.GetMouseWorldPosition(MaskCategory.Ground));
                        context.AbilityController.ConfirmAbility();
                    }
                }
            }
        }

        [Server]
        public virtual void Scanning()
        {
            scannedEnemyList.Clear();

            Collider[] colliders = Physics.OverlapSphere(transform.position, context.Data.viewRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));

            List<HealthController> enemyList = new List<HealthController>();
            foreach (Collider collider in colliders)
            {
                
                HealthController sceneUnit = collider.GetComponent<HealthController>();
                if (sceneUnit != null && !sceneUnit.IsDead && sceneUnit.OwnerId != OwnerId)
                    enemyList.Add(sceneUnit);
            }

            Debug.Log($"Unit: {gameObject.name} | Colliders: {colliders.Length} | Actual Enemy: {enemyList.Count}");

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
            OnScanningComplete?.Invoke(hasEnemy.Value);
            ScanningCompletetCallback(hasEnemy.Value);
        }

        [ObserversRpc]
        private void ScanningCompletetCallback(bool hasEnemy)
        {
            OnScanningCompleteCallback?.Invoke(hasEnemy, IsOwner);
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
            currentHitChange.Value = CalculateHitChance(enemyUnit, context.WeaponManager.CurrentWeaponData.baseAccuracy);
            
            OnSelectTargetCallback(enemyUnit.Profile.unitName, currentHitChange.Value);
        }

        public float CalculateHitChance(UnitController target, float accuracy)
        {
            float baseHitChance = accuracy - target.Data.evasion;
            Vector3 modifiedEnemyPos = new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z);

            float distance = (modifiedEnemyPos - fireTransform.position).magnitude;
            CoverType cover = DetermineCoverType(modifiedEnemyPos);

            int decreaseDistancePercentage = 1;
            baseHitChance -= (int)(distance * decreaseDistancePercentage); // Decrease hit chance by 1% per unit distance [Dung]

            switch (cover)
            {
                case CoverType.None: // No adjustment if there's no cover [Dung].
                    break; 
                case CoverType.HalfCover:
                    baseHitChance -= FULL_COVER_DECS_PERCENT; // Reduce hit chance by 10% for half cover [Dung].
                    break;
                case CoverType.FullCover:
                    baseHitChance -= HALF_COVER_DECS_PERCENT; // Reduce hit chance by 20% for full cover [Dung].
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
        private bool IsHit(float hitChance)
        {
            int roll = UnityEngine.Random.Range(0, 100);
            Debug.Log($"Rolled Hit Change: {roll}");
            return roll <= hitChance;
        }

        [ServerRpc]
        private void SetEndPosition(Vector3 endPosition)
        {
            grenadeEndPosition.Value = endPosition;
            Debug.Log($"EndPosSync: {grenadeEndPosition.Value} | Param: {endPosition}");
        }

        [Server]
        public void RotateToTossDirection()
        {
            Vector3 direction = grenadeEndPosition.Value - transform.position;
            context.CC.RotateToDirection(direction);
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
        /// Detects and returns a list of all enemies which are still alive within the unit's field of view.
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
                if (IsEnemyInFOV(enemyTransform) && !enemyTransform.GetComponent<HealthController>().IsDead)
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

        public virtual void OnDamage(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.1f)
            {
                if (IsServerInitialized)
                    OnDamageServer();

                if (IsClientInitialized)
                    OnDamageClient();
            }
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
            Debug.Log($"GrenadeDesitnation: {grenadeEndPosition.Value}");
            netGrenade.Toss(context.BoneController.GetBoneRefByHandler(Handler.RightHand).transform.position, grenadeEndPosition.Value, grenadeData.baseDamage, grenadeData.grenadeSettings.blastRadius);
        }

        [Client]
        private void OnTossGrenadeClient()
        {

        }

        [Server]
        protected virtual void OnDamageServer()
        {
            HealthController enemy = scannedEnemyList[currentEnemyIndex];
            Debug.Log($"{gameObject.name}:OnDamageServer");
            if (!enemy.IsDead)
            {
                if (IsHit(currentHitChange.Value))
                {
                    enemy.TakeDamage(context.WeaponManager.CurrentWeaponData.baseDamage);
                }
            }
            else
            {
                Debug.Log("Enemy is dead!");
            }
        }

        [Client]
        protected virtual void OnDamageClient()
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
                if (currentWeaponData.rapidFire) context.AnimationController.WeakRecoilShotAnimation();

                else context.AnimationController.StrongRecoilShotAnimation();

                yield return new WaitForSeconds(currentWeaponData.delayBetweenStrike);

                fireCounter.Value++;
            }

            fireCounter.Value = 0;
        }
        #endregion
    }
}