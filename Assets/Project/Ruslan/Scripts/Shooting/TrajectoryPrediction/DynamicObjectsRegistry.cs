using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectsRegistry : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> _dynamicBodies = new();
    public IReadOnlyList<Rigidbody> DynamicBodies => _dynamicBodies;

    [SerializeField] private int _batchSize = 50; // сколько объектов обрабатывать за кадр

    private void Start()
    {
        StartCoroutine(CollectAllCoroutine());
    }

    public IEnumerator CollectAllCoroutine()
    {
        _dynamicBodies.Clear();

        var bodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        int counter = 0;

        foreach (var rb in bodies)
        {
            if (!rb.gameObject.TryGetComponent(out PlayerContext _)
                && !rb.gameObject.TryGetComponent(out Bullet _)
                && !rb.gameObject.TryGetComponent(out Pillow _)
                && !rb.gameObject.TryGetComponent(out EnemyBullet _)
                && !rb.gameObject.TryGetComponent(out PlayerSafePosition _))
            {
                Register(rb);
            }

            counter++;
            if (counter >= _batchSize)
            {
                counter = 0;
                yield return null; // пропускаем кадр, чтобы сцена не зависла
            }
        }
    }

    public void Register(Rigidbody rb)
    {
        if (!_dynamicBodies.Contains(rb))
        {
            _dynamicBodies.Add(rb);
        }
        else
        {
            Debug.Log(rb);
        }
    }

    public void Unregister(Rigidbody rb)
    {
        _dynamicBodies.Remove(rb);
    }
}
