using System;
using UnityEngine;

public class LevelProgressSystem : MonoBehaviour
{    
    public event Action OnLevelCompleat;
    private int _enemiesCount;

    public void Initialize(int enemiesCount)
    {
        _enemiesCount = enemiesCount;
    }

    public void DetectKill()
    { 
        _enemiesCount--;
        CheckIfCompleat();
    }

    private void CheckIfCompleat()
    {
        if (_enemiesCount <= 0)
        {
            OnLevelCompleat?.Invoke();
        }
    }
}