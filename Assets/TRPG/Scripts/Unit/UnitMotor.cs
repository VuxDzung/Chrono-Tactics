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
        [SerializeField] private float lerpSpeed = 20f;
        public Action OnMoveStartedServer;
        public Action OnMoveFinishedServer;

        public Action<bool> OnMoveStartedCallback;
        public Action<bool> OnMoveFinishedCallback;


        private readonly SyncVar<Vector3> syncPosition = new SyncVar<Vector3>(new SyncTypeSettings(0.1f));
        private readonly SyncVar<Quaternion> syncRotation = new SyncVar<Quaternion>(new SyncTypeSettings(0.1f));
        private readonly SyncVar<float> moveMagnitude = new SyncVar<float>();
        private readonly SyncVar<bool> isMoving = new SyncVar<bool>();

        private NavMeshAgent navMeshAgent;
        private UnitController context;

        public float MoveMagnitude => moveMagnitude.Value;

        private bool hasReachedDestination; //This field only runs on server.

        public float AIMoveMagnitude => navMeshAgent.velocity.magnitude / navMeshAgent.speed;

        public bool IsMoving => isMoving.Value;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        [Server(Logging = FishNet.Managing.Logging.LoggingType.Off)]
        public override void OnServerUpdate()
        {
            base.OnServerUpdate();

            // Update only if there’s a significant change in position or rotation
            if (Vector3.Distance(syncPosition.Value, transform.position) > 0.1f)
            {
                syncPosition.Value = transform.position;
            }
            if (Quaternion.Angle(syncRotation.Value, transform.rotation) > 0.1f)
            {
                syncRotation.Value = transform.rotation;
            }

            if (!context.IsSelected) return;

            moveMagnitude.Value = navMeshAgent.velocity.magnitude / navMeshAgent.speed;

            if (!hasReachedDestination && HasReachedDestination())
            {
                hasReachedDestination = true;
                isMoving.Value = false;
                OnMoveFinishedServer?.Invoke();
                FinishMoveCallback();
            }
            else if (hasReachedDestination && !HasReachedDestination())
            {
                // Reset if the agent has a new destination
                hasReachedDestination = false;
            }
        }

        [Client(Logging = FishNet.Managing.Logging.LoggingType.Off)]
        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            // Interpolate position and rotation for smooth movement on the client side
            if (IsMoving)
            {
                transform.position = Vector3.Lerp(transform.position, syncPosition.Value, Time.deltaTime * lerpSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, syncRotation.Value, Time.deltaTime * lerpSpeed);
            }
        }

        [Server]
        public virtual void MoveTo(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);
            OnMoveStartedServer?.Invoke();
            isMoving.Value = true;
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
                return !navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.1f;
            }
            return false;
        }

        [Server]
        public void RotateToDirection(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = targetRotation;
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
        }
    }
}