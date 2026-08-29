using System;
using UnityEngine;

public class PlayerDetectorTrigger : MonoBehaviour
{
    public event Action OnPlayerTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out PlayerContext player))
        {
            OnPlayerTriggered?.Invoke();
        }
    }

}
