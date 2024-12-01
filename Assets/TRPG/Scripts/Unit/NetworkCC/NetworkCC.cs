using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using TRPG.Unit;
using UnityEngine;
using UnityEngine.AI;

namespace TheKistuneStudio
{
    public class NetworkCC : CoreNetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float lerpSpeed = 20f;
        [SerializeField] private CharacterController cc;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private bool useCC = true;

        public Action OnMoveStartedServer;
        public Action OnMoveFinishedServer;

        public Action<bool> OnMoveStartedCallback;
        public Action<bool> OnMoveFinishedCallback;

        private readonly SyncVar<Vector3> syncPosition = new SyncVar<Vector3>(new SyncTypeSettings(0.1f));
        private readonly SyncVar<Quaternion> syncRotation = new SyncVar<Quaternion>(new SyncTypeSettings(0.1f));
        private readonly SyncVar<float> moveMagnitude = new SyncVar<float>();
        private readonly SyncVar<bool> isMoving = new SyncVar<bool>();

        private Vector3 velocity;

        private UnitController context;

        public float MoveMagnitude => moveMagnitude.Value;

        private bool hasReachedDestination; //This field only runs on server.

        public bool IsMoving => isMoving.Value;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            navMeshAgent = GetComponent<NavMeshAgent>();
            cc = GetComponent<CharacterController>();

            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            syncPosition.Value = transform.position;
            syncRotation.Value = transform.rotation;
        }

        [Server(Logging = FishNet.Managing.Logging.LoggingType.Off)]
        public override void OnServerUpdate()
        {
            base.OnServerUpdate();

            if (Vector3.Distance(syncPosition.Value, transform.position) > 0.1f)
                syncPosition.Value = transform.position;
            if (Quaternion.Angle(syncRotation.Value, transform.rotation) > 0.1f)
                syncRotation.Value = transform.rotation;

            if (!context.IsSelected) return;

            if (useCC) MoveByCC();

            moveMagnitude.Value = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
            syncPosition.Value = transform.position;
            syncRotation.Value = transform.rotation;

            if (!hasReachedDestination && HasReachedDestination())
            {
                transform.position = navMeshAgent.destination;
                hasReachedDestination = true;
                isMoving.Value = false;
                OnMoveFinishedServer?.Invoke();
                FinishMoveCallback();
            }
            else if (hasReachedDestination && !HasReachedDestination())
            {
                // Reset if the agent has a new destination [Dung]
                hasReachedDestination = false;
            }
        }

        [Server]
        private void MoveByCC()
        {
            Vector3 direction;

            velocity = navMeshAgent.desiredVelocity;

            direction = (navMeshAgent.destination - transform.position);
            direction.y = 0;

            RotateToDirection(direction);
            cc.Move(velocity.normalized * moveSpeed * Time.deltaTime);

            navMeshAgent.velocity = cc.velocity;
        }

        [Client(Logging = FishNet.Managing.Logging.LoggingType.Off)]
        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            transform.position = Vector3.Lerp(transform.position, syncPosition.Value, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, syncRotation.Value, Time.deltaTime * lerpSpeed);
        }

        [Server]
        public virtual void SetDestination(Vector3 destination)
        {
            navMeshAgent.destination = (destination);

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
            // Check if the path calculation is complete and the agent is close enough to the destination [Dung]
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // Ensure the agent is not still moving [Dung]
                return !navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.1f;
            }
            return false;
        }

        [Server]
        public void RotateToDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > navMeshAgent.stoppingDistance)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
            }
        }
    }
}