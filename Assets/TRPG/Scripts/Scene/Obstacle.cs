using UnityEngine;

namespace TRPG
{
    public enum CoverType
    {
        None,
        HalfCover,
        FullCover
    }

    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private CoverType type;
        [SerializeField] private Collider[] colliders;

        public CoverType CoverType => type;
    }
}