using UnityEngine;
public class PortalActivator: MonoBehaviour
{
    [SerializeField] Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void ActivatePortal()
    {
        _animator.SetTrigger("start");
    }
}