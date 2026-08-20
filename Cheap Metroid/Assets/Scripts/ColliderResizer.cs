using UnityEngine;

public class ColliderResizer : MonoBehaviour
{
    private CapsuleCollider2D _collider;
    [SerializeField] private Vector2 normalSize = new Vector2(1,2.75f);
    [SerializeField] private Vector2 crouchSize;
    [SerializeField] private Vector2 mophSize;
    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider2D>();
        normalSize = _collider.size;
    }
    public void ResizeToNormal()
    {
        Resize(normalSize);
    }
    public void ResizeColliderToCrouch()
    {
        Resize(crouchSize);
    }
    public void ResizeToMorph()
    {
        Resize(mophSize);
    }
    private void Resize(Vector2 target)
    {
        var currentSize = _collider.size;
        var diff = (target - currentSize) / 2;
        _collider.size = target;
        transform.localPosition += new Vector3(diff.x, diff.y - float.Epsilon); // floating point error correction
    }
}
