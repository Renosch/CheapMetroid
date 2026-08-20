using UnityEngine;

[CreateAssetMenu(fileName = "BombData", menuName = "Scriptable Objects/BombData")]
public class BombData : ScriptableObject
{
    [field: SerializeField] public float DetonationTime { get; private set; } = 1;
    [field: SerializeField] public float PressureForece { get; private set; } = 1;
    [field: SerializeField] public float BlastRadius { get; private set; } = 0.3f;
}
