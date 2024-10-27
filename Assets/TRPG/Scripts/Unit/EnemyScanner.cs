using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit {
    public class EnemyScanner : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;

        private float range;

        private UnitController context;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
        }

        public virtual List<UnitController> Scanning()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, enemyLayer);
            List<UnitController> enemyList = new List<UnitController>();
            foreach (Collider collider in colliders) 
                enemyList.Add(collider.GetComponent<UnitController>());
            return enemyList;
        }
    }
}