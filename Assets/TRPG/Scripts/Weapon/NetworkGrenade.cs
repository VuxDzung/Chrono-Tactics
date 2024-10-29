using FishNet.Object;
using TMPro;
using UnityEngine;

namespace TRPG
{
    public class NetworkGrenade : BaseWeapon
    {
        [SerializeField] private Rigidbody grenadeRb;
        [SerializeField] private float tossSpeed = 10f;

        [Server]
        public void Toss(Vector3 starterPosition, Vector3 endPosition)
        {
            float gravity = Mathf.Abs(Physics.gravity.y);
            float angle = 45f * Mathf.Deg2Rad;

            // Calculate distances
            float horizontalDistance = Vector3.Distance(new Vector3(starterPosition.x, 0, starterPosition.z),
                                                        new Vector3(endPosition.x, 0, endPosition.z));
            float heightDifference = endPosition.y - starterPosition.y;

            // Calculate initial speed
            float initialSpeed = Mathf.Sqrt(gravity * horizontalDistance * horizontalDistance /
                                            (2 * (heightDifference - horizontalDistance * Mathf.Tan(angle)) *
                                             Mathf.Pow(Mathf.Cos(angle), 2)));

            // Calculate velocity vector
            Vector3 horizontalDirection = (new Vector3(endPosition.x, 0, endPosition.z) -
                                           new Vector3(starterPosition.x, 0, starterPosition.z)).normalized;

            Vector3 velocity = horizontalDirection * initialSpeed * Mathf.Cos(angle);
            velocity.y = initialSpeed * Mathf.Sin(angle);

            // Apply velocity to grenade
            grenadeRb.velocity = velocity;
        }
    }
}