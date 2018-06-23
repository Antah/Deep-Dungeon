using UnityEngine;
using System.Collections;

//Node used in triangulation, stores corresponding Area and has its position.
public class DTNode
{
    private Vector2 nodePos;

    GameObject parentRoom;

    public DTNode(float x, float y, GameObject parentRoom)
    {
        nodePos = new Vector2(x, y);
        this.parentRoom = parentRoom;
    }

    public Vector2 getNodePosition()
    {
        return nodePos;
    }

    public GameObject getParentRoom()
    {
        return parentRoom;
    }
}