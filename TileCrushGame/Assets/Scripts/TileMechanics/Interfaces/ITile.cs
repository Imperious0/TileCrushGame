using System;
using UnityEngine;
public interface ITile
{
    public event EventHandler<EventArgs> MovementDoneEvent;
    public event EventHandler<BubbleEventArgs> TileBubbleEvent;
    string getTileType();
    Vector2 getTilePos();
    void spawnAtPos(Vector2 index);
    void moveToPos(Vector2 index);
    void revertMovement();
    int getTilePoint();
    void Bubble();
    bool isBubbled();
}
