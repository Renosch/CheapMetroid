using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;
using System.Threading;

public class Player : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference shiftAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference morphAction;
    [SerializeField] private InputActionReference shineSparkAction;

    [SerializeField] public HorizontalDirection LookDirection= HorizontalDirection.None;

    [Header("Walk Settings")]
    [SerializeField] private float walkSpeed = 4;
    [SerializeField] private float sprintSpeed = 8;
    [SerializeField] private float acc = 0.9f;
    [SerializeField] private float decc = 0.9f;
    [Header("SpeedBooster Setting")]
    [SerializeField] private bool speedBoosterEnabled = true;
    [SerializeField] private float SpeedBoosterSpeed = 16;
    [SerializeField] private float shineSparkSpeed = 20;
    [SerializeField] private float SpeedBoosterChargeTime = 1;
    [SerializeField] private float ShineSparkStorTime = 5;
    [SerializeField] private float ShineSparkDirectionInputwindow = 2;
    [field: SerializeField] public bool IsShineSpark { get; private set; }
    [Header("Jump Setting")]
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private float horizontalWallJumpVelocity = 5;
    [SerializeField] private bool highJumpEnabled = true;
    [SerializeField] private float highjumpMultiplier = 2;
    [SerializeField] private float jumpFallDivider = 2;
    [Header("Morph Setting")]
    [field: SerializeField] public bool morphEnabled { get; private set; } = true;
    [SerializeField] bool morphSpringJumpEnabled = true;
    [field: SerializeField] public bool isMorph { get; private set;}
    [SerializeField] float morphSpeed =5;
    [Header("SpaceJump Setting")]
    [SerializeField] bool SpaceJumpEnable = true;

    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    [Header("Air Controll")]
    [SerializeField] private float airControll = 0.3f;
    [SerializeField] private float FallMultiplyer = 1.5f;

    //Animation
    private Animator _animator;
    private readonly int a_walkHash = Animator.StringToHash("Direction");
    private readonly int a_spinJumpHash = Animator.StringToHash("IsSpinJumping");
    private readonly int a_isWalk = Animator.StringToHash("IsWalk");
    private readonly int a_isSprint = Animator.StringToHash("IsSprint");
    private readonly int a_isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int a_isCrouchHash = Animator.StringToHash("IsCrouch");
    private readonly int a_MorphLayer = 1;
    private readonly int a_isMorphHash = Animator.StringToHash("IsMorph");

    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;
    private ColliderResizer _colliderResizer;
    private CollisionChecker collisionChecker;
    private Task _speedBoosterChargeCoroutine = Task.CompletedTask;
    private CancellationTokenSource _speedBoosterCancelToken = new();
    private CancellationTokenSource _ShineSparkStoreCancelToken = new();
    private Vector2 _moveInput;
    private Vector2 _shineSparkDirection;
    private HorizontalDirection _speedBoosterDirection;
    private bool _isSpeedBooster;
    private bool _hasShineSparkStored;
    private bool _isSprint { get => _animator.GetBool(a_isSprint); set => _animator.SetBool(a_isSprint, value); }
    private bool _setSurfaceAnchor =false;
    private bool _canWallJump => collisionChecker.HasLeftwall || collisionChecker.HasRightwall; 
    private bool _isCrouch => _moveInput.y == -1 && _hasCoyoteTimeLeft;
    private bool _hasCoyoteTimeLeft => collisionChecker.HasCoyoteTimeLeft;
    private bool _isSpinJumping { get => _animator.GetBool(a_spinJumpHash); set => _animator.SetBool(a_spinJumpHash, value); }

    public enum HorizontalDirection
    {
        None =0,
        Left =-1,
        Right =1,
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        _collider = GetComponent<CapsuleCollider2D>();
        _colliderResizer = GetComponent<ColliderResizer>();
        collisionChecker = GetComponent<CollisionChecker>();
        _animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        moveAction.action.performed += HandleWalkInput;
        moveAction.action.canceled += HandleWalkInput;

        shiftAction.action.performed += HandleShiftInput;
        shiftAction.action.canceled += HandleShiftInput;

        jumpAction.action.performed += HandleJumpInput;
        jumpAction.action.canceled += HandleJumpInput;

        morphAction.action.started += HandleMorphInput;

        shineSparkAction.action.started += HandleShineSparkInput;

        collisionChecker.OnGrounded += HandleOnGround;
        collisionChecker.OnLeafGrounded += HandleOnLeafGround;
    }
    private void OnDisable()
    {
        moveAction.action.performed -= HandleWalkInput;
        moveAction.action.canceled -= HandleWalkInput;

        shiftAction.action.performed -= HandleShiftInput;
        shiftAction.action.canceled -= HandleShiftInput;

        jumpAction.action.performed -= HandleJumpInput;
        jumpAction.action.canceled -= HandleJumpInput;

        morphAction.action.started -= HandleMorphInput;

        shineSparkAction.action.started -= HandleShineSparkInput;

        collisionChecker.OnGrounded -= HandleOnGround;
        collisionChecker.OnLeafGrounded -= HandleOnLeafGround;
    }

    private void HandleWalkInput(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
        if(_moveInput.x<0)
        {
            LookDirection = HorizontalDirection.Left;
        }
        if (_moveInput.x > 0)
        {
            LookDirection = HorizontalDirection.Right;
        }

    }
    private void HandleShiftInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _isSprint =true;
        }
        if(ctx.canceled)
        {
            _isSprint = false;
        }
    }
    private void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        if(IsShineSpark)
        {
            return;
        }
        if(ctx.performed)
        {
            if(isMorph && morphSpringJumpEnabled)
            {
                ApplyJumpForce();
                return;
            }
            if(collisionChecker.HasCoyoteTimeLeft || (SpaceJumpEnable && _isSpinJumping))
            {
                ApplyJumpForce();

                if(_moveInput.x !=0)
                {
                    _isSpinJumping = true;
                }else
                {
                    _isSpinJumping = false;
                }
            }

            collisionChecker.CheckCanWallJump();
            if (!collisionChecker.HasCoyoteTimeLeft && _isSpinJumping && !isMorph&& _canWallJump)
            {
                if(collisionChecker.HasLeftwall && _moveInput.x >0)
                {
                    _rb.linearVelocityX = horizontalWallJumpVelocity;
                    ApplyJumpForce();
                }
                if (collisionChecker.HasRightwall && _moveInput.x < 0)
                {
                    _rb.linearVelocityX = -horizontalWallJumpVelocity;
                    ApplyJumpForce();
                }
            }
        }
        if(ctx.canceled)
        {
            if (_rb.linearVelocityY > 0)
            {
                _rb.linearVelocityY /= jumpFallDivider;
                _rb.gravityScale = FallMultiplyer;
            }
        }
    }
    private void ApplyJumpForce()
    {
        _rb.gravityScale = 1;
        _setSurfaceAnchor = false;
        var gravity = Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);
        var height = highJumpEnabled ? highjumpMultiplier * jumpHeight : jumpHeight;

        var velocity = Mathf.Sqrt(2 * gravity * height);
        _rb.linearVelocityY = velocity;
    }
    private void HandleMorphInput(InputAction.CallbackContext ctx)
    {
            isMorph = !isMorph;
        _animator.SetBool(a_isMorphHash,isMorph);
        _animator.SetLayerWeight(a_MorphLayer, isMorph ? 1 : 0);
    }
    private void HandleShineSparkInput(InputAction.CallbackContext ctx)
    {
        if(_hasShineSparkStored)
        {
            _hasShineSparkStored = false;
            IsShineSpark = true;
            _=ShineSparkCoroutine();
        }
    }

    private void HandleOnGround()
    {
        ResetSpinJumpAnimation();
        _setSurfaceAnchor = true;
        _animator.SetBool(a_isGroundedHash, true);

    }
    private void HandleOnLeafGround()
    {
        _animator.SetBool(a_isGroundedHash, false);
    }

    private void ResetSpinJumpAnimation()
    {
        _animator.SetBool(a_spinJumpHash, false);
    }

    private void FixedUpdate()
    {
        if(!IsShineSpark)
        {
            HandleMovement();
        }
    }
    public void ApplyBombJumpForce(BombData data)
    {
        _rb.gravityScale = 1;
        _setSurfaceAnchor = false;
        var gravity = Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);
        var height = data.PressureForece;

        var velocity = Mathf.Sqrt(2 * gravity * height);
        _rb.linearVelocityY = velocity;
    }
    public void TransitionToNextRoom(Transform position)
    {
        _rb.position = position.position;
    }
    private void HandleMovement()
    {
        _animator.SetBool(a_isCrouchHash, _isCrouch);

        if(isMorph && morphEnabled)
        {
            _colliderResizer.ResizeToMorph();
            HandleMorph();
            CheckSpeedBooster();
            SetMovemntAnimation();
            return;
        }

        if (_isCrouch)
        {
            _colliderResizer.ResizeColliderToCrouch();
            CheckSpeedBooster();
            HandleCrouch();
            SetMovemntAnimation();
            return;
        }

        else
        {
            _colliderResizer.ResizeToNormal();
            ChargeSpeedBooster();
            CheckSpeedBooster();
            HandleLocomotion();
            SetMovemntAnimation();
        }
    }
    private void HandleMorph()
    {
        var speed = _isSpeedBooster ? SpeedBoosterSpeed : morphSpeed;
        var controll = collisionChecker.HasCoyoteTimeLeft ? 1 : airControll * Time.deltaTime;
        var change = _moveInput.x > 0 ? this.acc : decc;
        var resultingVelocity = Mathf.Lerp(_rb.linearVelocityX, speed * _moveInput.x, controll * change);
        _rb.linearVelocityX = resultingVelocity;
    }
    private void HandleCrouch()
    {
        _rb.linearVelocityX = 0;
    }
    private void HandleLocomotion()
    {
        var speed = _isSprint ? sprintSpeed : walkSpeed;
        speed = _isSpeedBooster ? SpeedBoosterSpeed : speed;

        var controll = collisionChecker.HasCoyoteTimeLeft ? 1 : airControll * Time.deltaTime;
        var change = _moveInput.x > 0 ? this.acc : decc;

        var resultingVelocity = Mathf.Lerp(_rb.linearVelocityX, speed * _moveInput.x, controll * change);
        if(_setSurfaceAnchor)
        {
            _rb.linearVelocityX = resultingVelocity/2;
            _rb.Slide(Vector2.right*resultingVelocity/2, Time.fixedDeltaTime, new() {surfaceAnchor=Vector2.down,gravity=Physics2D.gravity*_rb.gravityScale });
        }else
        {
            _rb.linearVelocityX = resultingVelocity;
        }
    }
    private void CheckSpeedBooster()
    {
        if (!_isSpeedBooster)
        {
            if (!_hasCoyoteTimeLeft)
            {
                _speedBoosterCancelToken.Cancel();
                return;
            }
            if (!_isSprint)
            {
                _speedBoosterCancelToken.Cancel();
                return;
            }
            if (_moveInput.x == 0)
            {
                _speedBoosterCancelToken.Cancel();
                return;
            }
            if (_isCrouch)
            {
                _speedBoosterCancelToken.Cancel();
                return;
            }
            if (isMorph)
            {
                _speedBoosterCancelToken.Cancel();
                return;
            }
        }
        if (_isSpeedBooster && _hasCoyoteTimeLeft)
        {
            if (_isCrouch)
            {
                _ShineSparkStoreCancelToken.Cancel();
                _isSpeedBooster = false;
                _hasShineSparkStored = true;
                _ShineSparkStoreCancelToken = new();
                _ = ShineSparkStorCoroutine(_ShineSparkStoreCancelToken.Token);
                return;
            }
            if (_moveInput.x == 0)
            {
                _isSpeedBooster = false;
                _speedBoosterCancelToken.Cancel();
                return;

            }
            if (_speedBoosterDirection != LookDirection)
            {
                _isSpeedBooster = false;
                _speedBoosterCancelToken.Cancel();
                return;
            }
        }
    }
    private void ChargeSpeedBooster()
    {
        if (collisionChecker.IsGrounded && _isSprint && _moveInput.x != 0 && _speedBoosterChargeCoroutine.IsCompleted && !_isSpeedBooster && !_hasShineSparkStored && speedBoosterEnabled && !isMorph && !_isCrouch)
        {
            _speedBoosterCancelToken.Cancel();
            _speedBoosterCancelToken = new();
            _speedBoosterChargeCoroutine = SpeedBoosterChargeCoroutine(_speedBoosterCancelToken.Token);
        }
    }
    private async Task SpeedBoosterChargeCoroutine(CancellationToken token)
    {
        await Task.Delay((int)(1000 * SpeedBoosterChargeTime));
        if(!token.IsCancellationRequested)
        {
            _isSpeedBooster = true;
            _speedBoosterDirection = LookDirection;
        }
    }
    private async Task ShineSparkStorCoroutine(CancellationToken token)
    {
        await Task.Delay((int)(1000 * ShineSparkStorTime),token);
        if(!token.IsCancellationRequested)
        {
            _hasShineSparkStored = false;
        }
    }
    private async Task ShineSparkCoroutine()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _rb.gravityScale = 0;
        if(_isCrouch)
        {
            _colliderResizer.ResizeToNormal();
            
        }
        await Task.Delay((int)(1000* ShineSparkDirectionInputwindow));

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        _shineSparkDirection = _moveInput != Vector2.zero? _moveInput : Vector2.up;
        bool isShinspakring = true;
        float hitmagin = 0.25f;
        float maxduration = 60;
        while(maxduration>=0 && isShinspakring)
        {
            _rb.linearVelocity = _shineSparkDirection * shineSparkSpeed;
            maxduration -= Time.deltaTime;

            var capsuleSize = new Vector2(_collider.size.x + hitmagin, _collider.size.y + hitmagin);

            var hits =Physics2D.CapsuleCastAll(this.transform.position,
                                                            capsuleSize,
                                                            CapsuleDirection2D.Vertical,
                                                            0,
                                                            Vector2.zero,
                                                            0,
                                                            groundLayer);
            foreach (var hit in hits)
            {
                float angle = Vector3.Angle(_shineSparkDirection, hit.normal);
                // Wall or Slope check
                if (angle >=100 )
                {
                    if(angle<140 && (_shineSparkDirection == Vector2.left || _shineSparkDirection == Vector2.right)) //Slope 
                    {
                        if(hit.normal.y > 0)
                        {
                            _speedBoosterDirection = (HorizontalDirection)(Math.Sign(_shineSparkDirection.x));
                            _isSpeedBooster = true;
                        }
                    }
                    isShinspakring = false;
                }
            }

            await Task.Yield();
        }
        _rb.gravityScale = 1;
        IsShineSpark = false;
    }
    private void SetMovemntAnimation()
    {
        if(_moveInput.x !=0)
        {
            _animator.SetFloat(a_walkHash, _moveInput.x);
            _animator.SetBool(a_isWalk, true);
        }else
        {
            _animator.SetBool(a_isWalk, false);
        }
    }
}
