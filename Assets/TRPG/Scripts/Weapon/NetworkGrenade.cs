using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TRPG
{
    public class NetworkGrenade : BaseWeapon
    {
        private readonly SyncVar<Vector3> networkPosition = new SyncVar<Vector3>();
        private readonly SyncVar<Quaternion> networkRotation = new SyncVar<Quaternion>();    

        [SerializeField] private Rigidbody grenadeRb;
        [SerializeField] private float explodeDelay = 5f;
        [SerializeField] private ParticleSystem explodeParticle;

        //[SerializeField] private float tossSpeed = 10f;
        private float damage;
        private float blastRadius;

        private Vector3 endPosition;

        [Server]
        public void Toss(Vector3 starterPosition, Vector3 endPosition, float damage, float blastRadius)
        {
            this.endPosition = endPosition;
            this.blastRadius = blastRadius;
            this.damage = damage;

            float gravity = -9.81f;
            float angle = 45f * Mathf.Deg2Rad;

            // Calculate distances
            float horizontalDistance = Vector3.Distance(new Vector3(starterPosition.x, 0, starterPosition.z),
                                                        new Vector3(endPosition.x, 0, endPosition.z));
            float heightDifference = endPosition.y - starterPosition.y;

            // Calculate initial speed
            float initialSpeed = Mathf.Sqrt(gravity * horizontalDistance * horizontalDistance /
                                            (2 * (heightDifference - horizontalDistance * Mathf.Tan(angle)) *
                                             Mathf.Pow(Mathf.Cos(angle), 2)));

            // Calculate velocity vector
            Vector3 horizontalDirection = (new Vector3(endPosition.x, 0, endPosition.z) -
                                           new Vector3(starterPosition.x, 0, starterPosition.z)).normalized;

            Vector3 velocity = horizontalDirection * initialSpeed * Mathf.Cos(angle);
            velocity.y = initialSpeed * Mathf.Sin(angle);

            // Apply velocity to grenade
            TossCallback(velocity);
            StartCoroutine(Explode());
        }

        [ObserversRpc]
        private void TossCallback(Vector3 velocity)
        {
            grenadeRb.velocity = velocity;
        }

        public override void OnServerUpdate()
        {
            base.OnServerUpdate();
            ControlGravity();

            networkPosition.Value = transform.position;
            networkRotation.Value = transform.rotation;
        }

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            //OnTransformChangedSync();
        }

        private void ControlGravity()
        {
            float distanceToDesitnation = Vector3.Distance(transform.position, endPosition);
            if (distanceToDesitnation <= 0.2f)
            {
                grenadeRb.velocity = Vector3.zero;
            }
            //else
            //{
            //    grenadeRb.useGravity = true;
            //}
        }

        private void OnTransformChangedSync()
        {
            transform.position = networkPosition.Value;
            transform.rotation = networkRotation.Value;
        }

        private IEnumerator Explode()
        {
            yield return new WaitForSeconds(explodeDelay);
            TakeDamageSurround();
            OnExplodeCallback();
        }

        [Server]
        private void TakeDamageSurround()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));
            List<HealthController> enemyList = new List<HealthController>();
            foreach (var _collider in colliders)
            {
                HealthController enemy = _collider.GetComponent<HealthController>();
                if (enemy != null && enemy.OwnerId != OwnerId)
                {
                    enemyList.Add(enemy);
                }
            }

            if (enemyList.Count > 0)
            {
                enemyList.ForEach(enemy => enemy.TakeDamage(damage));
            }
        }

        [ObserversRpc]
        private void OnExplodeCallback()
        {
            if (m_AudioSource && m_Clip != null) m_AudioSource.PlayOneShot(m_Clip);

            if (explodeParticle != null)
            {
                explodeParticle.gameObject.SetActive(true);
                explodeParticle.Play();
            }
        }
    }
}