using UnityEngine;

namespace TRPG.Unit
{
    public class BoneSnapper : MonoBehaviour
    {
        [SerializeField] private HumanBodyBones bone;

        public virtual void SnapParent(Animator animator)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            transform.SetParent(boneTransform, true);
        }

        public virtual void AlignWorldPosition(Animator animator)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            transform.position = boneTransform.position;
        }
    }
}