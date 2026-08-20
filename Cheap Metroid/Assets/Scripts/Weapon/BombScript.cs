using UnityEngine;
using System.Collections;

namespace Weapon
{

    public class BombScript : MonoBehaviour
    {
        [SerializeField] private BombData data;
    
        public void Inizialize(BombData bombData)
        {
            data = bombData;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(DetonationCouroutine());
        }
        IEnumerator DetonationCouroutine()
        {
            yield return new WaitForSeconds(data.DetonationTime);
            var colliders = Physics2D.OverlapCircleAll(transform.position, data.BlastRadius);
    
            foreach(var collider in colliders)
            {
                if(collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeBombDamage(this);
                }
                if(collider.TryGetComponent(out Player player))
                {
                    player.ApplyBombJumpForce(data);
                }
            }
                Destroy(this.gameObject);
        }
    }

}