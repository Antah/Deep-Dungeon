using UnityEngine;
using System.Collections;

//Node used in triangulation, stores corresponding Area and has its position.
public class DTNode
{
    private ArrayList connectionNodes = new ArrayList();

    private Vector2 nodePos;

    GameObject parentCell;

    public DTNode(float _x, float _y, GameObject _parentCell)
    {
        nodePos = new Vector2(_x, _y);
        parentCell = _parentCell;
    }

    public Vector2 getNodePosition()
    {
        return nodePos;
    }

    public void setNodes(DTNode room1, DTNode room2)
    {
        connectionNodes.Add(room1);
        connectionNodes.Add(room2);
    }

    public ArrayList getConnections()
    {
        return connectionNodes;
    }

    public GameObject getParentCell()
    {
        return parentCell;
    }
}