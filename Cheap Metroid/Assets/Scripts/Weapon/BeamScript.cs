using UnityEngine;
using UnityEngine.Splines;

namespace Weapon
{

    public class BeamScript : MonoBehaviour
    {
        [field: SerializeField] public BeamType beamType { get; private set; } = BeamType.Base;
        [field: SerializeField] public BeamData data;
        [SerializeField] LayerMask EnviromentLayer;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] Animator animator;
        public void Inizialize(BeamType type,BeamData data)
        {
            beamType = type;
            this.data = data;
            _spriteRenderer.color = SetColor();
            _spriteRenderer.sprite = SetSprite();
            if((beamType & BeamType.Wave) > 0)
            {
                animator.runtimeAnimatorController = data.Waveanimator;
            }else
            {
                animator.runtimeAnimatorController = data.Normalanimator;

            }
        }
        private Color SetColor()
        {
            if ((beamType & BeamType.Ice) > 0)
            {
                return Color.blue;
            }
            if ((beamType & BeamType.Plasma) > 0)
            {
                return Color.green;
            }
            if ((beamType & BeamType.Wave) > 0)
            {
                return Color.violet;
            }
            if ((beamType & BeamType.Spaiser) > 0)
            {
                return Color.yellow;
            }
            return Color.orange;
        }
        private Sprite SetSprite()
        {
            if ((beamType & BeamType.Plasma) > 0)
            {
                return data.PlasmaSprite;
            }
            if ((beamType & BeamType.Wave) > 0)
            {
                return data.WaveSprite;
            }
            if ((beamType & BeamType.Ice) > 0)
            {
                return data.IceSprite;
            }
            if ((beamType & BeamType.Spaiser) > 0)
            {
                return data.SpaiserSprite;
            }
            if ((beamType & BeamType.Charge) > 0)
            {
                return data.ChargeSprite;
            }
            return data.BaseSprite;
        }
        void Update()
        {
            Move();
        }
        private void Move()
        {
            transform.Translate(Vector2.right* data.Speed * Time.deltaTime);
        }
        private void OnEnviromentHit()
        {
            if((beamType & BeamType.Spaiser)>0)
            {
                return;
            }
            Destroy(this.gameObject);
        }
        private void OnDamageableHit(IDamageable damageable)
        {
            if ((beamType & BeamType.Ice) > 0)
            {
                damageable.TakeDamageAndFreeze(this);
            }else
            {
                damageable.TakeDamage(this);
            }
            if ((beamType & BeamType.Spaiser) > 0)
            {
                return;
            }
            Destroy(this.gameObject);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            bool isEnviromentLayer = (EnviromentLayer.value & (1 << collision.gameObject.layer)) > 0;
            if (isEnviromentLayer)
            {
                OnEnviromentHit();
            }
            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                OnDamageableHit(damageable);
            }
        }
        private void OnBecameInvisible()
        {
            Destroy(this.gameObject);
        }
    }
}