using UnityEngine;
using System.Collections.Generic;

//Triangle object used in triangulation. Stores 3 edges.
public class DTTriangle
{

    private List<DTEdge> edgeList = new List<DTEdge>();

    private Color theDrawColor = new Color(255, 0, 0, 1);

    public DTTriangle(DTEdge edgeA, DTEdge edgeB, DTEdge edgeC)
    {
        edgeList.Add(edgeA);
        edgeList.Add(edgeB);
        edgeList.Add(edgeC);
    }

    public List<DTEdge> GetEdges()
    {
        return edgeList;
    }

    public void DrawTriangle()
    {
        foreach (DTEdge e in edgeList)
        {
            e.drawEdge();
        }
    }

    public void StopDraw()
    {
        foreach (DTEdge e in edgeList)
        {
            e.stopDraw();
        }
    }


    //Find if this triangle contains a vert)
    public bool ContainsNode(DTNode node)
    {

        foreach (DTEdge e in edgeList)
        {
            if (e.getNodeA() == node || e.getNodeB() == node)
            {
                return true;
            }
        }

        return false;
    }

    public void SetDrawColor(Color color)
    {
        theDrawColor = color;
        foreach (DTEdge e in edgeList)
        {
            e.setDrawColor(color);
        }
    }

    public bool CheckForSharedEdge(DTTriangle otherTriangle)
    {

        foreach (DTEdge e in otherTriangle.GetEdges())
        {
            foreach (DTEdge myEdge in edgeList)
            {
                if (myEdge.checkSame(e))
                {
                    return true;
                }
            }

        }

        return false;

    }

    public bool ContainsEdge(DTEdge edge)
    {
        foreach (DTEdge e in edgeList)
        {
            if (e.checkSame(edge))
            {
                return true;
            }
        }

        return false;
    }

    public void SetEdges(DTEdge edgeA, DTEdge edgeB, DTEdge edgeC)
    {

        foreach (DTEdge e in edgeList)
        {
            e.stopDraw();
        }
        edgeList.Clear();

        edgeList.Add(edgeA);
        edgeList.Add(edgeB);
        edgeList.Add(edgeC);


        //reset the drawing lines
        /*for (int i = 0; i < 3; i++){
			GameObject.Destroy(lineList[i]);
			lineList[i] = new GameObject().AddComponent<LineRenderer>();
		}*/
    }
}
