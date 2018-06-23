using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DTMinSpanningTree
{

    private Hashtable vertexTable = new Hashtable();

    private List<DTNode> allNodes;
    private List<DTEdge> allEdges;

    private List<DTEdge> connectionEdges = new List<DTEdge>();
    private List<DTNode> connectionNodes = new List<DTNode>();
    private System.Random pseudoRandom;

    public void Update()
    {
        foreach (DTEdge aEdge in connectionEdges)
        {
            aEdge.drawEdge("path");
        }
    }

    public void ChooseConnectionsFromEdges(List<DTNode> nodes, List<DTEdge> edges, System.Random random, int additionalConnectionsPercent)
    {
        pseudoRandom = random;
        allNodes = nodes;
        allEdges = edges;

        //Prepare hashtable - nodes are keys and edge lists are values
        PrepareHashtable();

        //Create minimum spanning tree by adding chosen edges to list connectionEdges
        CreateMinSpanningTree();

        //Add some edges not included in the minimum spanning tree
        AddOptionalConnections(additionalConnectionsPercent);
    }

    private void AddOptionalConnections(int additionalConnectionsPercent)
    {
        List<DTEdge> poolList = new List<DTEdge>();

        foreach (DTEdge e in allEdges)
        {
            if (!connectionEdges.Contains(e))
            {
                poolList.Add(e);
            }
        }

        int percent = (poolList.Count * additionalConnectionsPercent) / 100;

        for (int i = 0; i < percent; i++)
        {
            int index = pseudoRandom.Next(0, poolList.Count);

            connectionEdges.Add(poolList[index]);
            poolList.RemoveAt(index);
        }
    }

    private void PrepareHashtable()
    {
        foreach (DTEdge e in allEdges)
        {

            if (!vertexTable.ContainsKey(e.getNodeA()))
            {
                List<DTNode> temp = new List<DTNode>();
                temp.Add(e.getNodeB());
                vertexTable.Add(e.getNodeA(), temp);
            }
            else
            {
                List<DTNode> temp = (List<DTNode>)vertexTable[e.getNodeA()];

                if (!temp.Contains(e.getNodeB()))
                {
                    temp.Add(e.getNodeB());
                    vertexTable[e.getNodeA()] = temp;
                }

            }

            if (!vertexTable.ContainsKey(e.getNodeB()))
            {
                List<DTNode> temp = new List<DTNode>();
                temp.Add(e.getNodeA());
                vertexTable.Add(e.getNodeB(), temp);
            }
            else
            {
                List<DTNode> temp = (List<DTNode>)vertexTable[e.getNodeB()];

                if (!temp.Contains(e.getNodeA()))
                {
                    temp.Add(e.getNodeA());
                    vertexTable[e.getNodeB()] = temp;
                }
            }
        }
    }

    private void CreateMinSpanningTree()
    {
        int randomIndex = pseudoRandom.Next(0, allNodes.Count);

        DTNode theNode = allNodes[randomIndex];
        connectionNodes.Add(theNode);
        ConnectNextNode();
    }

    private void ConnectNextNode()
    {

        DTNode oldNode = null;
        DTNode closestNode = null;
        float closestDistance = 0;


        foreach (DTNode connectedNode in connectionNodes)
        {
            List<DTNode> nearbyNodes = (List<DTNode>)vertexTable[connectedNode];

            foreach (DTNode nearbyNode in nearbyNodes)
            {
                if (!connectionNodes.Contains(nearbyNode))
                {
                    float tempDst = Vector2.Distance(nearbyNode.getParentRoom().transform.position, connectedNode.getParentRoom().transform.position);
                    if (closestNode != null)
                    {
                        if (tempDst < closestDistance)
                        {
                            closestDistance = tempDst;
                            closestNode = nearbyNode;
                            oldNode = connectedNode;
                        }
                    }
                    else
                    {
                        closestNode = nearbyNode;
                        closestDistance = tempDst;
                        oldNode = connectedNode;
                    }
                }
            }
        }

        connectionNodes.Add(closestNode);

        foreach (DTEdge e in allEdges)
        {
            if (e.containsNode(oldNode) && e.containsNode(closestNode))
            {
                e.setDrawColor(new Color(50, 255, 0, 255));
                connectionEdges.Add(e);
                break;
            }
        }

        if (connectionNodes.Count == allNodes.Count)
        {
            return;
        }
        else
        {
            ConnectNextNode();
        }
    }

    public void StopEdgeDraw()
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
