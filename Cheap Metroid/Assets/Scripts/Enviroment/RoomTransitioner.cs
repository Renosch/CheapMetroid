using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class RommTransitionData
{
    [field: SerializeField] public PolygonCollider2D newBound { get; private set;}
    [field: SerializeField] public Transform spawnPoint { get; private set;}
}
[DefaultExecutionOrder(-100)]
public sealed class RoomTransitioner : MonoBehaviour
{
    private static RoomTransitioner _instance;
    public static RoomTransitioner Instance
    {
        get
        {
            return _instance;
        }
    }
    [SerializeField] CinemachineConfiner2D cameraBound;
    [SerializeField] Player player;
    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }else
        {
            Destroy(this.gameObject);
        }
    }
    public void ChangeCameraBound(RommTransitionData data)
    {
        cameraBound.BoundingShape2D = data.newBound;
        player.TransitionToNextRoom(data.spawnPoint);
    }
}
