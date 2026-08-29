using UnityEngine;

public class CollectableGun : Collectable
{
    [SerializeField] private PillowGun pillowGun;
    override protected void Awake()
    {
        base.Awake();

        if (pillowGun == null)
        {
            Debug.LogWarning($"{nameof(CollectableGun)} на '{name}': не назначен {nameof(pillowGun)}.", this);
            return;
        }

        pillowGun.Deactivate();
        pillowGun.enabled = false;
    }

    public override void Collect(Transform target)
    {
        base.Collect(target);

        if (pillowGun == null)
            return;

        pillowGun.Activate();
        pillowGun.enabled = true;
    }
}
