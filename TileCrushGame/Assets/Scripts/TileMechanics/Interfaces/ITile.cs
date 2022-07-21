using System;
using UnityEngine;
public interface ITile
{
    public event EventHandler<EventArgs> MovementDoneEvent;
    string getTileType();
    Vector2 getTilePos();
    void spawnAtPos(Vector2 index);
    void moveToPos(Vector2 index);
    void revertMovement();
    void Bubble();
    bool isBubbled();
}
