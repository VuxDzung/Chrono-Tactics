using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class BoneSnapController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private List<BoneSnapper> snapperList;

        private void Awake()
        {
            SnapParent();
        }

        private void SnapParent()
        {
            snapperList.ForEach(snapper => snapper.SnapParent(animator));
        }

        public void AlignBoneRefs()
        {
            snapperList.ForEach(b => b.AlignWorldPosition(animator));
        }
    }
}