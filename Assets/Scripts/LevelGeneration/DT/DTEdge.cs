using UnityEngine;

//Edges used in triangulation. Methodes for drawing temporary lines area also present.
public class DTEdge
{

    private DTNode node1;
    private DTNode node2;

    private Color theDrawColor = new Color(255, 0, 0, 1);
    private LineRenderer theLine;

    public DTEdge(DTNode _n0, DTNode _n1)
    {
        node1 = _n0;
        node2 = _n1;
        theLine = new GameObject().AddComponent<LineRenderer>();
        theLine.material = new Material(Shader.Find("Particles/Additive"));
        theLine.name = "EdgeLine";
        theLine.tag = "Line";
        //theLine.renderer.material.color = theDrawColor;
    }

    public DTNode getNode1()
    {
        return node1;
    }

    public DTNode getNode2()
    {
        return node2;
    }

    public bool checkSame(DTEdge _aEdge)
    {
        if ((node1 == _aEdge.getNode1() || node1 == _aEdge.getNode2()) &&
              (node2 == _aEdge.getNode1() || node2 == _aEdge.getNode2()))
        {
            return true;
        }

        return false;
    }

    public bool edgeContainsVertex(DTNode _aNode)
    {
        if (node1 == _aNode || node2 == _aNode)
        {
            return true;
        }

        return false;
    }

    public void drawEdge(string name = "")
    {
        if (node1.getParentCell() != null && node2.getParentCell() != null)
        {
            if (theLine == null)
            {
                theLine = new GameObject().AddComponent<LineRenderer>();
                theLine.name = "EdgeLine";
                theLine.material = new Material(Shader.Find("Particles/Additive"));
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
                    Debug.Log(node1.getNodePosition().x + " " + node1.getNodePosition().y);
                    Debug.Log(node2.getNodePosition().x + " " + node2.getNodePosition().y);
                }
                theLine.material = new Material(Shader.Find("Particles/Additive"));
            }
            theLine.SetWidth(0.7f, 0.7f);
            //theLine.renderer.material.color = theDrawColor;
            theLine.SetColors(theDrawColor, theDrawColor);
            theLine.SetVertexCount(2);
            theLine.SetPosition(0, new Vector3(node1.getNodePosition().x, node1.getNodePosition().y, -3));
            theLine.SetPosition(1, new Vector3(node2.getNodePosition().x, node2.getNodePosition().y, -3));
        }
    }

    public void setDrawColor(Color _theColor)
    {
        theDrawColor = new Color(_theColor.r, _theColor.g, _theColor.b, 1);
        if (theLine != null)
        {
            theLine.material = new Material(Shader.Find("Particles/Additive"));
        }
    }

    public void stopDraw()
    {
        if (theLine != null)
        {
            GameObject.Destroy(theLine.gameObject);
        }
    }



}
