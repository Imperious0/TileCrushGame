using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Collider2D))]
public class Tile : MonoBehaviour, ITile
{
    [SerializeField]
    private string _tileType = "";

    [Range(0.1f, 10f), SerializeField]
    private float _animationSpeed = 1f;
    [SerializeField]
    private float _animationDuration = 2f;

    private float _currentAnimationTime = 0f;

    public event EventHandler<EventArgs> MovementDoneEvent;

    private SpriteRenderer sRenderer;
    private Collider2D sCollider;
    private Transform sTransform;

    private Vector2 _prevIndex = Vector2.zero;
    private Vector2 _currentIndex = Vector2.zero;

    private Vector2 _prevPos = Vector2.zero;
    private Vector2 _currentPos = Vector2.zero;
    private Vector2 _nextPos = Vector2.zero;

    private bool _isBubbled = false;

    private void Awake()
    {
        sRenderer = GetComponent<SpriteRenderer>();
        sCollider = GetComponent<Collider2D>();
        sTransform = transform;
    }

    private void Update()
    {
        if (!_nextPos.Equals(_currentPos))
        {
            float timeLapse = Mathf.InverseLerp(0f, _animationDuration, _currentAnimationTime);
            Vector2 nextPos = Vector2.Lerp(_prevPos, _nextPos, timeLapse);
            sTransform.localPosition = nextPos;
            _currentPos = nextPos;
            _currentAnimationTime += Time.deltaTime * _animationSpeed;
            if(timeLapse >= 1)
            {
                _currentAnimationTime = 0f;
                _currentPos = _nextPos;
                MovementDoneEvent?.Invoke(this, new EventArgs());
            }
        }
    }
    public Vector2 getTilePos()
    {
        return _currentIndex;
    }
    public string getTileType()
    {
        return _tileType;
    }
    public void spawnAtPos(Vector2 index)
    {
        _prevIndex = index;
        _currentIndex = index;
        sTransform.localPosition = new Vector3(_currentIndex.y * GameManager.Instance.TileOffset.x, -(_currentIndex.x * GameManager.Instance.TileOffset.y), 0);
        _currentPos = sTransform.localPosition;
        _nextPos = _currentPos;
    }
    public void moveToPos(Vector2 index)
    {
        _prevIndex = _currentIndex;
        _currentIndex = index;
        _prevPos = _currentPos;
        _nextPos = new Vector3(_currentIndex.y * GameManager.Instance.TileOffset.x, -(_currentIndex.x * GameManager.Instance.TileOffset.y), 0);
    }
    public void revertMovement()
    {
        moveToPos(_prevIndex);
    }
    public void Bubble()
    {
        if (_isBubbled)
            return;

        sRenderer.enabled = false;
        sCollider.enabled = false;
        _isBubbled = true;
        Destroy(gameObject);
    }
    public bool isBubbled()
    {
        return _isBubbled;
    }

}
