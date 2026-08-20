using UnityEngine;
using System;

public class CollisionChecker : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    CapsuleCollider2D _collider;
    public bool IsGrounded { get; private set;}
    private const float _coyoteTime = 0.3f;
    private float _coyoteTimeLeft;
    public bool HasCoyoteTimeLeft => _coyoteTimeLeft > 0;
    //Events
    public event Action OnGrounded = delegate { };
    public event Action OnLeafGrounded = delegate { };

    public bool HasLeftwall = false;
    public bool HasRightwall = false;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider2D>();
    }
    private void OnEnable()
    {
        OnGrounded += ResetCoyoteTime;
    }
    private void OnDisable()
    {
        OnGrounded -= ResetCoyoteTime;
    }
    private void ResetCoyoteTime()
    {
        _coyoteTimeLeft = _coyoteTime;
    }
    private void CheckGround()
    {
        float _radiusPadding = -0.05f;
        float _positionPadding = -0.2f;

        var _radius = _collider.size.x / 2;
        var _point = new Vector2(_collider.bounds.center.x, _collider.bounds.min.y);


        var result = Physics2D.OverlapCircle(_point + Vector2.up * (_radius + _positionPadding), _radius + _radiusPadding, groundLayer) != null;

        if (result == true && !IsGrounded)
        {
            OnGrounded.Invoke();
        }
        if (result == false && IsGrounded)
        {
            OnLeafGrounded.Invoke();
        }

        IsGrounded = result;

        if (!IsGrounded && HasCoyoteTimeLeft)
        {
            _coyoteTimeLeft -= Time.deltaTime;
            if (!HasCoyoteTimeLeft)
            {
                _coyoteTimeLeft = 0;
            }
        }
    }
    public void CheckCanWallJump()
    {


        float sizeXPadding = 0.5f;
        float sizeY = 1f;

        var size = new Vector2(_collider.size.x + sizeXPadding, sizeY);

        var pointRight = new Vector2(_collider.bounds.center.x + (sizeXPadding / 2), _collider.bounds.center.y);
        var pointLeft = new Vector2(_collider.bounds.center.x - (sizeXPadding / 2), _collider.bounds.center.y);


        var resultRight = Physics2D.BoxCast(pointRight, size, 0, Vector2.zero, 0, groundLayer);
        var resultLeft = Physics2D.BoxCast(pointLeft, size, 0, Vector2.zero, 0, groundLayer);

        if (resultRight.normal == Vector2.left)
        {
            HasRightwall = true;
        }
        else
        {
            HasRightwall = false;
        }
        if (resultLeft.normal == Vector2.right)
        {
            HasLeftwall = true;
        }
        else
        {
            HasLeftwall = false;
        }
    }
    private void FixedUpdate()
    {
        CheckGround();
        CheckCanWallJump();
    }
}
