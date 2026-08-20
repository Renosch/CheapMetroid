using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Threading;
using Weapon;

public class PlayerAttackScript : MonoBehaviour
{
    [SerializeField] InputActionReference StrgAction;
    [SerializeField] InputActionReference AttackAction;
    [SerializeField] InputActionReference ChargeAttackAction;
    [SerializeField] private WeaponFactory factory;
    [SerializeField] Transform AimStartPosition;
    [SerializeField] float shootOffset = 0.2f;
    [SerializeField] float aimTime = 2f;
    [SerializeField] bool clampAim = true;
    [SerializeField] BeamType beamType;
    [SerializeField] bool isCharged;
    [SerializeField] bool hasChargeBeamEnabled;
    private CollisionChecker _colisionChecker;
    private Player _player;
    private CancellationTokenSource aimHelperCancellation = new();
    private CancellationTokenSource aimTimeOutCancellation = new();
    Vector3 _direction = Vector2.left;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] float rayLength = 20;
    private void Awake()
    {
        _colisionChecker = GetComponent<CollisionChecker>();
        _player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        StrgAction.action.performed += HandleStrgInput;
        ChargeAttackAction.action.performed += HandleChargeAttack;
        AttackAction.action.performed += StartAiming;
        AttackAction.action.canceled += HandleAttack;
    }
    private void OnDisable()
    {
        StrgAction.action.performed -= HandleStrgInput;
        ChargeAttackAction.action.performed -= HandleChargeAttack;
        AttackAction.action.performed -= StartAiming;
        AttackAction.action.canceled -= HandleAttack;
    }
    private void StartAiming(InputAction.CallbackContext ctx)
    {
        if (_player.isMorph && _player.morphEnabled) return;

        lineRenderer.enabled = true;
        aimHelperCancellation.Cancel();
        aimHelperCancellation = new();
        _=AimHelper(aimHelperCancellation.Token);
    }
    async Task AimHelper( CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            _direction = GetDirection();

            //it Is Local Position
            Vector3 startPoint = Vector3.zero;
            Vector3 direction = new Vector3(_direction.x, _direction.y,0).normalized;

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, startPoint + direction.normalized * rayLength);

            await Task.Yield();
        }
    }
    async Task AimTimeOut(float duration,CancellationToken token)
    {
        await Task.Delay((int)(1000 * duration));
        if(!token.IsCancellationRequested)
        {
            lineRenderer.enabled = false;
            aimHelperCancellation.Cancel();
        }
    }
    private void HandleStrgInput(InputAction.CallbackContext ctx)
    {
        clampAim = !clampAim;
    }
    private void HandleChargeAttack(InputAction.CallbackContext _)
    {
        if(hasChargeBeamEnabled)
        {
            isCharged = true;
        }
    }
    private void HandleAttack(InputAction.CallbackContext ß)
    {
        if(_player.isMorph && _player.morphEnabled)
        {
            CreateBomb();
        }else
        {
            CreateBeam();
        }

        isCharged = false;
        beamType = beamType & ~BeamType.Charge;
        aimTimeOutCancellation.Cancel();
        aimTimeOutCancellation = new();
        _ = AimTimeOut(aimTime, aimTimeOutCancellation.Token);
    }
    private void CreateBomb()
    {
        factory.CreateBomb(this.transform);
    }
    private void CreateBeam()
    {
        _direction = GetDirection();
        if (isCharged && hasChargeBeamEnabled)
        {
            beamType = beamType | BeamType.Charge;
        }
        factory.CreateBeam(beamType, AimStartPosition, _direction, shootOffset);
    }
    private Vector3 GetDirection()
    {
        Vector3 mouseposition = Mouse.current.position.value;
        var worldPosition = Camera.main.ScreenToWorldPoint(mouseposition);
        worldPosition.z = 0;
        var direction = (worldPosition - AimStartPosition.position).normalized;
        if(clampAim)
        {
            direction.x = Mathf.RoundToInt(direction.x);
            direction.y = Mathf.RoundToInt(direction.y);
        }
        if(_colisionChecker.IsGrounded)
        {
            var angle = Vector3.Angle(Vector3.down, direction);
            if(angle<45)
            {
                var newdir = direction.x!=0? Mathf.Sign(direction.x):(int)_player.LookDirection;
                direction = new Vector3(newdir, -1);
            }
        }
        return direction.normalized;
    }
}
