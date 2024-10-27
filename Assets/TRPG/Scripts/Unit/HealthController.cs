using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Setup();
        }

        public virtual void Setup()
        {
            syncCurrentHealth.Value = maxHealth;
        }

        [Server]
        public virtual void TakeDamage(float damage)
        {
            if (syncCurrentHealth.Value <= 0) return;

            syncCurrentHealth.Value -= damage;

            if (syncCurrentHealth.Value <= 0)
            {
                OnTakeDamage?.Invoke();
                TakeDamageCallback();
            }
            else
            {
                OnDead?.Invoke();
                DeadCallback();
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