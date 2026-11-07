using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>
    /// A throwable pillow platform: the physics side of one <see cref="PillowSlot"/>.
    /// Also marks pillow surfaces for ground classification.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Pillow : MonoBehaviour, IPillowBody
    {
        private Rigidbody _rigidbody;

        // Lazy: the launcher stows its pillows in its own Awake, which may run before this one.
        private Rigidbody Body => _rigidbody != null ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        public Vector3 Position => this == null ? Vector3.zero : Body.position;

        public void Launch(Vector3 position, Quaternion rotation, Vector3 impulse)
        {
            if (this == null)
                return;

            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);

            Rigidbody body = Body;
            body.isKinematic = false;
            body.detectCollisions = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.AddForce(impulse, ForceMode.Impulse);
        }

        public void Freeze()
        {
            if (this == null)
                return;

            MakeKinematic();
        }

        public void BeginReturn(Vector3 startPosition)
        {
            if (this == null)
                return;

            MakeKinematic();
            Body.detectCollisions = false;
            Body.position = startPosition;
        }

        public void MoveTo(Vector3 position)
        {
            if (this == null)
                return;

            Body.position = position;
        }

        public void Stow()
        {
            if (this == null)
                return;

            MakeKinematic();
            transform.SetParent(null);
            gameObject.SetActive(false);
        }

        private void MakeKinematic()
        {
            Rigidbody body = Body;

            // Unity 6 rejects velocity writes on a kinematic body, so clear them only while it is still dynamic.
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = true;
        }
    }
}
