using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.Trajectory
{
    public class PhysicsObjectsRegistry : MonoBehaviour
    {
        public class BodyData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;
        }

        private Dictionary<Rigidbody, BodyData> _bodiesData = new Dictionary<Rigidbody, BodyData>();
        private void Awake()
        {
            RegisterRigidbodiesOnScene();
        }

        private void RegisterRigidbodiesOnScene()
        {
            foreach (var rb in FindObjectsByType<Rigidbody>(0))
            {
                _bodiesData.Add(rb, new BodyData());
            }
        }

        public void RegisterNewRigidbody(Rigidbody rb)
        {
            _bodiesData.Add(rb, new BodyData());
        }

        public void DeleteRigidbody(Rigidbody rb)
        {
            _bodiesData.Remove(rb);
        }

        public void SaveRigitbodiesData()
        {
            foreach (var rb in _bodiesData)
            {
                rb.Value.position = rb.Key.transform.position;
                rb.Value.rotation = rb.Key.transform.rotation;
                rb.Value.velocity = rb.Key.linearVelocity;
                rb.Value.angularVelocity = rb.Key.angularVelocity;
            }
        }

        public void LoadRigitbodiesData()
        {
            foreach (var rb in _bodiesData)
            {
                rb.Key.transform.position = rb.Value.position;
                rb.Key.transform.rotation = rb.Value.rotation;
                rb.Key.linearVelocity = rb.Value.velocity;
                rb.Key.angularVelocity = rb.Value.angularVelocity;
            }
        }
    }
}
