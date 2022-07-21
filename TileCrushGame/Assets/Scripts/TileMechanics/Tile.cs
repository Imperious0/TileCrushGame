using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
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

    private Vector2 _prevIndex = Vector2.zero;
    private Vector2 _currentIndex = Vector2.zero;

    private Vector2 _prevPos = Vector2.zero;
    private Vector2 _currentPos = Vector2.zero;
    private Vector2 _nextPos = Vector2.zero;

    private bool _isBubbled = false;

    private void Awake()
    {
        sRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!_nextPos.Equals(_currentPos))
        {
            float timeLapse = Mathf.InverseLerp(0f, _animationDuration, _currentAnimationTime);
            Vector2 nextPos = Vector2.Lerp(_prevPos, _nextPos, timeLapse);
            transform.localPosition = nextPos;
            _currentPos = nextPos;
            _currentAnimationTime += Time.deltaTime * _animationSpeed;
            if(timeLapse >= 1)
            {
                _currentAnimationTime = 0f;
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
        transform.localPosition = new Vector3(_currentIndex.y * GameManager.Instance.TileOffset.x, -(_currentIndex.x * GameManager.Instance.TileOffset.y), 0);
        _currentPos = transform.localPosition;
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
        sRenderer.enabled = false;
        _isBubbled = true;
    }
    public bool isBubbled()
    {
        return _isBubbled;
    }

}
