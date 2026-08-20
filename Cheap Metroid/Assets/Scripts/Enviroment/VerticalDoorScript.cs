using UnityEngine;

public class VerticalDoorScript : BaseDoorScript
{
    [SerializeField] RommTransitionData UpCamerabound;
    [SerializeField] RommTransitionData DownCamerabound;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if ((collision.transform.position - transform.position).y < 0)
            {
                RoomTransitioner.Instance.ChangeCameraBound(UpCamerabound);
                CloseDoor();
            }
            else
            {
                RoomTransitioner.Instance.ChangeCameraBound(DownCamerabound);
                CloseDoor();
            }
        }
    }
}
