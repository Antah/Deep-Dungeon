using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Triangle object used in triangulation. Stores 3 edges.
public class DTTriangle
{

    private List<DTEdge> edgeList = new List<DTEdge>();

    private Color theDrawColor = new Color(255, 0, 0, 1);

    public DTTriangle(DTEdge _edg0, DTEdge _edg1, DTEdge _edg2)
    {
        edgeList.Add(_edg0);
        edgeList.Add(_edg1);
        edgeList.Add(_edg2);

        /*for (int i = 0; i < 3; i++){
			lineList[i] = new GameObject().AddComponent<LineRenderer>();
		}*/


    }

    public List<DTEdge> getEdges()
    {
        return edgeList;
    }

    public void drawTriangle()
    {
        foreach (DTEdge aEdge in edgeList)
        {
            aEdge.drawEdge();
        }
    }

    public void stopDraw()
    {
        foreach (DTEdge aEdge in edgeList)
        {
            aEdge.stopDraw();
        }
    }


    //Find if this triangle contains a vert)
    public bool containsVertex(DTNode _vert)
    {

        foreach (DTEdge aEdge in edgeList)
        {
            if (aEdge.getNode1() == _vert || aEdge.getNode2() == _vert)
            {
                return true;
            }
        }

        return false;
    }

    public void setDrawColor(Color _aColor)
    {
        theDrawColor = _aColor;
        foreach (DTEdge aEdge in edgeList)
        {
            aEdge.setDrawColor(_aColor);
        }
    }

    public bool checkTriangleShareEdge(DTTriangle _aTri)
    {

        foreach (DTEdge aEdge in _aTri.getEdges())
        {
            foreach (DTEdge myEdge in edgeList)
            {
                if (myEdge.checkSame(aEdge))
                {
                    return true;
                }
            }

        }

        return false;

    }

    public bool checkTriangleContainsEdge(DTEdge _aEdge)
    {
        foreach (DTEdge myEdge in edgeList)
        {
            if (myEdge.checkSame(_aEdge))
            {
                return true;
            }
        }

        return false;
    }

    public void setEdges(DTEdge _edge0, DTEdge _edge1, DTEdge _edge2)
    {

        foreach (DTEdge aEdge in edgeList)
        {
            aEdge.stopDraw();
        }
        edgeList.Clear();

        edgeList.Add(_edge0);
        edgeList.Add(_edge1);
        edgeList.Add(_edge2);


        //reset the drawing lines
        /*for (int i = 0; i < 3; i++){
			GameObject.Destroy(lineList[i]);
			lineList[i] = new GameObject().AddComponent<LineRenderer>();
		}*/
    }
}
