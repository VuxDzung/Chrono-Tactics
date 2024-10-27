using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class UnitAnimationEventManager : CoreNetworkBehaviour
    {
        [Server]
        public void Fire()
        {

        }

        [Client]
        public void OnFootstep()
        {

        }
    }
}