using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TRPG.Unit
{
    public class UnitMotor : CoreNetworkBehaviour
    {
        private readonly SyncVar<Vector3> syncPosition = new SyncVar<Vector3>();
        private readonly SyncVar<Quaternion> syncRotation = new SyncVar<Quaternion>();

        private NavMeshAgent navMeshAgent;
        private UnitController context;

        public float MoveMagnitude => navMeshAgent.velocity.magnitude / navMeshAgent.speed;


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
        }

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            transform.position = syncPosition.Value;
            syncRotation.Value = syncRotation.Value;
        }

        [ServerRpc]
        public virtual void MoveTo(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);
        }
    }
}