using UnityEngine;

public interface ITile
{
    string getTileType();
    void spawnAtPos(Vector2 index);
    void moveToPos(Vector2 index);
    void Bubble();
}
