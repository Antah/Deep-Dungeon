using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RoutesController
{

    private Hashtable vertexTable = new Hashtable();

    private List<DTNode> allNodes;
    private List<DTEdge> allEdges;

    private List<DTEdge> connectionEdges = new List<DTEdge>();
    private List<DTNode> connectedNodes = new List<DTNode>();
    private System.Random pseudoRandom;

    public void Update()
    {
        foreach (DTEdge aEdge in connectionEdges)
        {
            aEdge.drawEdge("path");
        }
    }

    public void setUpPrims(List<DTNode> nodes, List<DTEdge> edges, System.Random random)
    {
        this.pseudoRandom = random;

        allNodes = nodes;
        allEdges = edges;

        foreach (DTEdge aEdge in edges)
        {

            if (!vertexTable.ContainsKey(aEdge.getNode1()))
            {
                List<DTNode> temp = new List<DTNode>();
                temp.Add(aEdge.getNode2());
                vertexTable.Add(aEdge.getNode1(), temp);
            }
            else
            {
                List<DTNode> temp = (List<DTNode>)vertexTable[aEdge.getNode1()];

                if (!temp.Contains(aEdge.getNode2()))
                {
                    temp.Add(aEdge.getNode2());
                    vertexTable[aEdge.getNode1()] = temp;
                }

            }

            if (!vertexTable.ContainsKey(aEdge.getNode2()))
            {
                List<DTNode> temp = new List<DTNode>();
                temp.Add(aEdge.getNode1());
                vertexTable.Add(aEdge.getNode2(), temp);
            }
            else
            {
                List<DTNode> temp = (List<DTNode>)vertexTable[aEdge.getNode2()];

                if (!temp.Contains(aEdge.getNode1()))
                {
                    temp.Add(aEdge.getNode1());
                    vertexTable[aEdge.getNode2()] = temp;
                }
            }

        }

        startPrims();
        
        List<DTEdge> poolList = new List<DTEdge>();

        foreach (DTEdge edges2 in allEdges)
        {
            if (!connectionEdges.Contains(edges2))
            {
                poolList.Add(edges2);
            }
        }

        int perc = (poolList.Count * 20) / 100;

        for (int i = 0; i < perc; i++)
        {
            int index = pseudoRandom.Next(0, poolList.Count);

            connectionEdges.Add(poolList[index]);
            poolList.RemoveAt(index);
        }
    }

    private void startPrims()
    {
        int count = pseudoRandom.Next(0, allNodes.Count);

        DTNode theNode = allNodes[count];
        connectedNodes.Add(theNode);
        findNext();
    }

    private void findNext()
    {

        DTNode oldNode = null;
        DTNode closestNode = null;
        float closestDistance = 0;


        foreach (DTNode aNode1 in connectedNodes)
        {

            List<DTNode> connectedNodes = (List<DTNode>)vertexTable[aNode1];

            foreach (DTNode aNode2 in connectedNodes)
            {
                if (!this.connectedNodes.Contains(aNode2))
                {
                    float tempDst = Vector2.Distance(aNode2.getParentCell().transform.position, aNode1.getParentCell().transform.position);
                    if (closestNode != null)
                    {
                        if (tempDst < closestDistance)
                        {
                            closestDistance = tempDst;
                            closestNode = aNode2;
                            oldNode = aNode1;
                        }
                    }
                    else
                    {
                        closestNode = aNode2;
                        closestDistance = tempDst;
                        oldNode = aNode1;
                    }
                }
            }
        }

        connectedNodes.Add(closestNode);

        foreach (DTEdge aEdge in allEdges)
        {
            if (aEdge.edgeContainsVertex(oldNode) && aEdge.edgeContainsVertex(closestNode))
            {
                aEdge.setDrawColor(new Color(50, 255, 0, 255));
                connectionEdges.Add(aEdge);
                break;
            }
        }

        if (connectedNodes.Count == allNodes.Count)
        {
            return;
        }
        else
        {
            findNext();
        }
    }

    public void stopEdgeDraw()
    {
        GameObject[] allLines = (GameObject[])GameObject.FindGameObjectsWithTag("Line");

        foreach (GameObject aLine in allLines)
        {
            GameObject.Destroy(aLine);
        }
    }

    public List<DTEdge> getConnections()
    {
        return connectionEdges;
    }
}
