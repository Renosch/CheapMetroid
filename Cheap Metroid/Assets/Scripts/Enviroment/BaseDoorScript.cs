using UnityEngine;

using Weapon;

public abstract class BaseDoorScript : MonoBehaviour, IDamageable
{
    public enum DoorType
    {
        normal,
        missule,
        supermissule,
        superBomb,
        Plasma
    }

    [SerializeField] DoorType doorType;
    [SerializeField] Collider2D doorCollider;

    protected void OpenDoor()
    {
        doorCollider.enabled = false;
    }

    protected void TryOpenDoor(DoorType type)
    {
        if (type == doorType)
        {
            OpenDoor();
        }
    }

    protected void CloseDoor()
    {
        doorCollider.enabled = true;
    }

    public void TakeBombDamage(BombScript data)
    {
        //Boom
        TryOpenDoor(DoorType.normal);
    }

    public void TakeDamage(BeamScript data)
    {
        TryOpenDoor(DoorType.normal);

        if ((data.beamType & BeamType.Plasma) > 0)
        {
            TryOpenDoor(DoorType.Plasma);
        }
    }

    public void TakeDamageAndFreeze(BeamScript data)
    {
        TryOpenDoor(DoorType.normal);
    }
}
