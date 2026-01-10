using UnityEngine;

public class PlayerIcon : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;

    private Vector3 _target;
    private bool _hasTarget;

    private void Awake()
    {
        _target = transform.position;
    }

    private void Update()
    {
        if (!_hasTarget) return;

        transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime * moveSpeed);

        if ((transform.position - _target).sqrMagnitude < 0.01f)
        {
            transform.position = _target;
            _hasTarget = false;
        }
    }

    public void MoveTo(Vector3 worldPosition)
    {
        _target = worldPosition;
        _hasTarget = true;
    }
}
