using UnityEngine;

using Weapon;

public class HorizontalDoorScript : BaseDoorScript
{
    [SerializeField] RommTransitionData LeftCamerabound;
    [SerializeField] RommTransitionData RightCamerabound;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if((collision.transform.position-transform.position).x>0)
            {
                RoomTransitioner.Instance.ChangeCameraBound(LeftCamerabound);
                CloseDoor();
            }else
            {
                RoomTransitioner.Instance.ChangeCameraBound(RightCamerabound);
                CloseDoor();
            }
        }
    }
}
