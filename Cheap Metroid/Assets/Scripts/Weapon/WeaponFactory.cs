using UnityEngine;
using System.Collections.Generic;


namespace Weapon
{
    [System.Flags]
    public enum BeamType
    {
        Base =0,
        Charge =1<<0,
        Ice = 1<<1,
        Wave = 1<<2,
        Spaiser = 1<<3,
        Plasma =1<<4
    }
    public class WeaponFactory : MonoBehaviour
    {
        [SerializeField] BeamData BeamData;
        [SerializeField] BombData BombData;
        [SerializeField] BeamScript beamPrefab;
        [SerializeField] BombScript bombPrefab;
        public void CreateBeam(BeamType type, Transform position,Vector3 direction,float shootOffset)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            var prefab = Instantiate(beamPrefab, position.position + direction * shootOffset, rotation,this.transform);
            prefab.Inizialize(type,BeamData);
        }
        public void CreateBomb(Transform position)
        {
            var prefab = Instantiate(bombPrefab, position.position, Quaternion.identity, this.transform);
            prefab.Inizialize(BombData);
        }
    }
}
