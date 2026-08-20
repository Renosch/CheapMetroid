using UnityEngine;

[CreateAssetMenu(fileName = "BeamData", menuName = "Scriptable Objects/BeamData")]
public class BeamData : ScriptableObject
{
    [field: SerializeField] public float Speed { get; private set; } = 40;
    [field: SerializeField] public Sprite BaseSprite { get; private set; }
    [field: SerializeField] public Sprite ChargeSprite { get; private set; }
    [field: SerializeField] public Sprite WaveSprite { get; private set; }
    [field: SerializeField] public float WaveFrequence { get; private set; }
    [field: SerializeField] public AnimatorOverrideController Waveanimator;
    [field: SerializeField] public AnimatorOverrideController Normalanimator;
    [field: SerializeField] public Sprite IceSprite { get; private set; }
    [field: SerializeField] public Sprite SpaiserSprite { get; private set; }
    [field: SerializeField] public Sprite PlasmaSprite { get; private set; }
}
