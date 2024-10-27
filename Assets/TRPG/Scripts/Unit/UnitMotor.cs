using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace TRPG.Unit
{
    public class UnitMotor : CoreNetworkBehaviour
    {
        public Action OnMoveStartedServer;
        public Action OnMoveFinishedServer;

        public Action<bool> OnMoveStartedCallback;
        public Action<bool> OnMoveFinishedCallback;


        private readonly SyncVar<Vector3> syncPosition = new SyncVar<Vector3>();
        private readonly SyncVar<Quaternion> syncRotation = new SyncVar<Quaternion>();

        private NavMeshAgent navMeshAgent;
        private UnitController context;

        public float MoveMagnitude => navMeshAgent.velocity.magnitude / navMeshAgent.speed;

        private bool hasReachedDestination; //This field only runs on server.


        public virtual void Setup(UnitController context)
        {
            this.context = context;
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public override void OnServerUpdate()
        {
            base.OnServerUpdate();

            syncPosition.Value = transform.position;
            syncRotation.Value = transform.rotation;

            if (!context.IsSelected) return;

            if (!hasReachedDestination && HasReachedDestination())
            {
                hasReachedDestination = true;
                OnMoveFinishedServer?.Invoke();
                FinishMoveCallback();
            }
            else if (hasReachedDestination && !HasReachedDestination())
            {
                // Reset if the agent has a new destination
                hasReachedDestination = false;
            }
        }

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            transform.position = syncPosition.Value;
            syncRotation.Value = syncRotation.Value;
        }

        [Server]
        public virtual void MoveTo(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);
            OnMoveStartedServer?.Invoke();
            StartMoveCallback();
        }

        [ObserversRpc]
        private void StartMoveCallback()
        {
            OnMoveStartedCallback?.Invoke(IsOwner);
        }

        [ObserversRpc]
        private void FinishMoveCallback()
        {
            OnMoveFinishedCallback?.Invoke(IsOwner);
        }


        private bool HasReachedDestination()
        {
            // Check if the path calculation is complete and the agent is close enough to the destination
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // Ensure the agent is not still moving
                return !navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f;
            }
            return false;
        }
    }
}