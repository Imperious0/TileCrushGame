using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour, ITile
{
    [SerializeField]
    private string _tileType = "";

    private Vector2 _currentIndex = Vector2.zero;
    private Vector2 _currentPos = Vector2.zero;
    private Vector2 _nextPos = Vector2.zero;

    public string getTileType()
    {
        return _tileType;
    }
    public void spawnAtPos(Vector2 index)
    {
        _currentIndex = index;

        transform.localPosition = new Vector3(_currentIndex.y * GameManager.Instance.TileOffset.x, -(_currentIndex.x * GameManager.Instance.TileOffset.y), 0);
    }
    public void moveToPos(Vector2 index)
    {
        throw new System.NotImplementedException();
    }
    public void Bubble()
    {
        throw new System.NotImplementedException();
    }

}
