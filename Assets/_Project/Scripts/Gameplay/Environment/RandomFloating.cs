using System.Collections;
using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    public class RandomFloating : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _radius = 5f;
        [SerializeField] private float _minSpeed = 2f;
        [SerializeField] private float _maxSpeed = 4f;

        [Header("Height Limit")]
        [SerializeField] private float minY = -50f;
        [SerializeField] private float maxY = 50f;

        [Header("Pause")]
        [SerializeField] private float stopTime = 2f;

        private Vector3 targetPoint;
        private float _speed;

        private void Start()
        {
            _speed = Random.Range(_minSpeed, _maxSpeed);
            PickNewPoint();
            StartCoroutine(MoveLoop());
        }

        private IEnumerator MoveLoop()
        {
            while (true)
            {
                while (Vector3.Distance(transform.position, targetPoint) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetPoint,
                        _speed * Time.deltaTime);

                    yield return null;
                }

                yield return new WaitForSeconds(stopTime);

                PickNewPoint();
            }
        }

        private void PickNewPoint()
        {
            Vector3 randomOffset = Random.insideUnitSphere * _radius;
            targetPoint = transform.position + randomOffset;
            targetPoint.y = Mathf.Clamp(targetPoint.y, minY, maxY);
        }
    }
}
