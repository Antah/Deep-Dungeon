using UnityEngine;

//Edges used in triangulation. Methodes for drawing temporary lines area also present.
public class DTEdge
{

    private DTNode nodeA;
    private DTNode nodeB;

    private Color theDrawColor = new Color(255, 0, 0, 1);
    private LineRenderer theLine;

    public DTEdge(DTNode nodeA, DTNode nodeB)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        theLine = new GameObject().AddComponent<LineRenderer>();
        theLine.name = "Edge";
        theLine.tag = "Line";
    }

    public DTNode getNodeA()
    {
        return nodeA;
    }

    public DTNode getNodeB()
    {
        return nodeB;
    }

    public bool checkSame(DTEdge otherEdge)
    {
        if ((nodeA == otherEdge.getNodeA() || nodeA == otherEdge.getNodeB()) &&
              (nodeB == otherEdge.getNodeA() || nodeB == otherEdge.getNodeB()))
        {
            return true;
        }

        return false;
    }

    public bool containsNode(DTNode _aNode)
    {
        if (nodeA == _aNode || nodeB == _aNode)
        {
            return true;
        }

        return false;
    }

    public void drawEdge(string name = "")
    {
        if (nodeA.getParentRoom() != null && nodeB.getParentRoom() != null)
        {
            if (theLine == null)
            {
                theLine = new GameObject().AddComponent<LineRenderer>();
                theLine.name = "EdgeLine";
                theLine.tag = "Line";
            }
            if (name != "")
            {
                theLine = new GameObject().AddComponent<LineRenderer>();
                theLine.name = name;

                if (name == "final tri") { 
                    theLine.startColor = new Color(255, 0, 0, 1);
                    theLine.endColor = new Color(255, 0, 0, 1);
                }
                if (name == "path")
                {
                    theLine.startColor = new Color(40, 255, 0, 1);
                    theLine.endColor = new Color(40, 255, 0, 1);
                    Debug.Log(theLine.name);
                    Debug.Log(nodeA.getNodePosition().x + " " + nodeA.getNodePosition().y);
                    Debug.Log(nodeB.getNodePosition().x + " " + nodeB.getNodePosition().y);
                }
            }
            theLine.startWidth = 0.7f;
            theLine.endWidth = 0.7f;
            //theLine.renderer.material.color = theDrawColor;
            theLine.startColor = theDrawColor;
            theLine.endColor = theDrawColor;
            theLine.positionCount = 2;
            theLine.SetPosition(0, new Vector3(nodeA.getNodePosition().x, nodeA.getNodePosition().y, -3));
            theLine.SetPosition(1, new Vector3(nodeB.getNodePosition().x, nodeB.getNodePosition().y, -3));
        }
    }

    public void setDrawColor(Color _theColor)
    {
        theDrawColor = new Color(_theColor.r, _theColor.g, _theColor.b, 1);
    }

    public void stopDraw()
    {
        if (theLine != null)
        {
            GameObject.Destroy(theLine.gameObject);
        }
    }



}
