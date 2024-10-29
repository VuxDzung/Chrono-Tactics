using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

namespace TRPG
{
    public class HealthController : CoreNetworkBehaviour
    {
        public Action OnTakeDamage;
        public Action OnTakeDamageCallback;

        public Action OnDead;
        public Action OnDeadCallback;

        [SerializeField] private float maxHealth = 100; 

        private readonly SyncVar<float> syncCurrentHealth = new SyncVar<float>();

        public bool IsDead => syncCurrentHealth.Value <= 0;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                Setup();
            }
        }

        [ServerRpc]
        public virtual void Setup()
        {
            syncCurrentHealth.Value = maxHealth;
        }

        [Server]
        public virtual void TakeDamage(float damage)
        {
            if (syncCurrentHealth.Value <= 0) return;

            syncCurrentHealth.Value -= damage;
            Debug.Log($"{gameObject.name}.Health={syncCurrentHealth.Value}");

            if (syncCurrentHealth.Value <= 0)
            {
                OnDead?.Invoke();
                DeadCallback();
            }
            else
            {
                OnTakeDamage?.Invoke();
                TakeDamageCallback();
            }
        }

        [ObserversRpc]
        private void TakeDamageCallback()
        {
            OnTakeDamageCallback?.Invoke();
        }

        [ObserversRpc]
        private void DeadCallback()
        {
            OnDeadCallback?.Invoke();
        }
    }
}