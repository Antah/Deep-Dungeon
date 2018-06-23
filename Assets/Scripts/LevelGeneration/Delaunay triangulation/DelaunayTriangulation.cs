using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Triangulation implementation
public class DelaunayTriangulation
{

    //All the triangles in the triangulation
    private List<DTTriangle> triangleList = new List<DTTriangle>();

    //Verticies that still need to be added to the triangulations
    private List<DTNode> nodesToAddList = new List<DTNode>();

    //the current verticie that is being added to the triangulation
    private DTNode nextNode = null;

    //Edges that have become possibly unDelaunay due to the insertion of another verticie
    private List<DTEdge> edgesToCheck = new List<DTEdge>();

    //the omega triangle created at start of triangulation
    private DTTriangle bigTriangle;

    //the triangle the "nextNode" is inside of
    private DTTriangle inTriangle;

    private List<DTEdge> finalEdgesList = new List<DTEdge>();
    private System.Random pseudoRandom;

    //construvtor
    public DelaunayTriangulation()
    {

    }

    //Handles set up of triangulation
    public void SetupTriangulation(List<DTNode> roomList)
    {

        //puts all verticies into the toDo list
        foreach (DTNode n in roomList)
        {
            nodesToAddList.Add(n);
        }

        //creates three artificial verticies for the omega triangle
        DTNode nodeA = new DTNode(0, 250, null);

        DTNode nodeB = new DTNode(-250, -200, null);

        DTNode nodeC = new DTNode(250, -200, null);

        //creates the omega triangle
        bigTriangle = new DTTriangle(new DTEdge(nodeA, nodeB), new DTEdge(nodeA, nodeC), new DTEdge(nodeB, nodeC));

        //adds the omega triangle to the triangle list
        triangleList.Add(bigTriangle);
    }


    internal void Triangulate(List<DTNode> roomList, System.Random random)
    {
        this.pseudoRandom = random;
        SetupTriangulation(roomList);

        while (nodesToAddList.Count > 0)
        {
            AddVertexToTriangulation();
        }

        
        //drawTriangles();
        ConstructFinalEdgeList();
    }

    //Adds a verticies to the triangulation
    private void AddVertexToTriangulation()
    {
        //Find a Random verticie from the todo list
        int choice = pseudoRandom.Next(0, nodesToAddList.Count);

        //set next node to selected verticies
        nextNode = nodesToAddList[choice];

        //remove selected verticies from todo list
        nodesToAddList.Remove(nextNode);

        //stores triangles created during the loop to be appended to main list after loop
        List<DTTriangle> tempTriList = new List<DTTriangle>();

        //All edges are clean at this point. Remove any that may be left over from previous loop
        edgesToCheck.Clear();

        float count = -1;
        foreach (DTTriangle aTri in triangleList)
        {
            List<DTEdge> triEdges = aTri.GetEdges();
            count++;
            //Find which triangle the current vertex being add is located within
            if (LineIntersection.PointInTraingle(nextNode.getNodePosition(), triEdges[0].getNodeA().getNodePosition(),
                triEdges[0].getNodeB().getNodePosition(), triEdges[1].getNodeB().getNodePosition()))
            {

                //cache the triangle we are in so we can delete it after loop
                inTriangle = aTri;

                //create three new triangles from each edge of the triangle vertex is in to the new vertex
                foreach (DTEdge aEdge in aTri.GetEdges())
                {
                    DTTriangle nTri1 = new DTTriangle(new DTEdge(nextNode, aEdge.getNodeA()),
                                    new DTEdge(nextNode, aEdge.getNodeB()),
                                    new DTEdge(aEdge.getNodeB(), aEdge.getNodeA()));

                    //cache created triangles so we can add to list after loop
                    tempTriList.Add(nTri1);

                    //mark the edges of the old triangle as dirty
                    edgesToCheck.Add(new DTEdge(aEdge.getNodeA(), aEdge.getNodeB()));

                }

                break;
            }
        }

        //add the three new triangles to the triangle list
        foreach (DTTriangle aTri in tempTriList)
        {
            triangleList.Add(aTri);
        }

        //delete the old triangle that the vertex was inside of
        if (inTriangle != null)
        {
            triangleList.Remove(inTriangle);
            inTriangle.StopDraw();
            inTriangle = null;
        }

        CheckEdges(edgesToCheck);

    }

    private void CheckEdges(List<DTEdge> edgesList)
    {
        //the current dirty edge
        if (edgesList.Count == 0)
        {
            if (nodesToAddList.Count > 0)
            {
                AddVertexToTriangulation();
            }
            return;
        }

        //get the next edge in the dirty list
        DTEdge currentEdge = edgesList[0];

        DTTriangle[] connectedTris = new DTTriangle[2];
        int index = 0;


        foreach (DTTriangle aTri in triangleList)
        {
            if (aTri.ContainsEdge(currentEdge))
            {
                connectedTris[index] = aTri;
                index++;
            }
        }


        //in first case (omega triangle) this will = 1 so dont flip
        if (index == 2)
        {
            //stores the two verticies from both triangles that arnt on the shared edge
            DTNode[] uniqueNodes = new DTNode[2];
            int index1 = 0;

            //loop through the connected triangles and there edges. Checking for a vertex that isnt in the edge
            for (int i = 0; i < connectedTris.Length; i++)
            {
                foreach (DTEdge aEdge in connectedTris[i].GetEdges())
                {
                    if (!currentEdge.containsNode(aEdge.getNodeA()))
                    {
                        uniqueNodes[index1] = aEdge.getNodeA();
                        index1++;
                        break;
                    }

                    if (!currentEdge.containsNode(aEdge.getNodeB()))
                    {
                        uniqueNodes[index1] = aEdge.getNodeB();
                        index1++;
                        break;
                    }
                }
            }


            //find the angles of the two unique verticies
            float angle0 = CalculateVertexAngle(uniqueNodes[0].getNodePosition(),
                                                currentEdge.getNodeA().getNodePosition(),
                                                currentEdge.getNodeB().getNodePosition());

            float angle1 = CalculateVertexAngle(uniqueNodes[1].getNodePosition(),
                                                currentEdge.getNodeA().getNodePosition(),
                                                currentEdge.getNodeB().getNodePosition());

            //Check if the target Edge needs flipping
            if (angle0 + angle1 > 180)
            {
                //create the new edge after flipped
                DTEdge flippedEdge = new DTEdge(uniqueNodes[0], uniqueNodes[1]);

                //store the edges of both triangles in the Quad
                DTEdge[] firstTriEdges = new DTEdge[3];
                DTEdge[] secondTriEdges = new DTEdge[3];

                DTNode sharedNode0;
                DTNode sharedNode1;

                //set the shared nodes on the shared edge
                sharedNode0 = currentEdge.getNodeA();
                sharedNode1 = currentEdge.getNodeB();

                //construct a new triangle to update old triangle after flip
                firstTriEdges[0] = new DTEdge(uniqueNodes[0], sharedNode0);
                firstTriEdges[1] = new DTEdge(sharedNode0, uniqueNodes[1]);
                firstTriEdges[2] = flippedEdge;

                //construct a new triangle to update the other old triangle after flip
                secondTriEdges[0] = new DTEdge(uniqueNodes[1], sharedNode1);
                secondTriEdges[1] = new DTEdge(sharedNode1, uniqueNodes[0]);
                secondTriEdges[2] = flippedEdge;

                //update the edges of the triangles involved in the flip
                connectedTris[0].SetEdges(firstTriEdges[0], firstTriEdges[1], firstTriEdges[2]);
                connectedTris[1].SetEdges(secondTriEdges[0], secondTriEdges[1], secondTriEdges[2]);


                //Adds all edges to be potentially dirty. This is bad and should only add the edges that *could* be dirty
                foreach (DTEdge eEdge in connectedTris[0].GetEdges())
                {
                    edgesList.Add(eEdge);
                }

                foreach (DTEdge eEdge in connectedTris[1].GetEdges())
                {
                    edgesList.Add(eEdge);
                }

                //also add new edge to dirty list
                edgesList.Add(flippedEdge);
            }
        }

        //remove the current edge from the dirty list
        edgesList.Remove(currentEdge);

        CheckEdges(edgesList);
    }

    //calculates the angle at vertex _target in triangle (_target _shared0 _shared1) in degrees
    private float CalculateVertexAngle(Vector2 _target, Vector2 _shared0, Vector2 _shared1)
    {
        float length0 = Vector2.Distance(_target, _shared0);
        float length1 = Vector2.Distance(_shared0, _shared1);
        float length2 = Vector2.Distance(_shared1, _target);

        return Mathf.Acos(((length0 * length0) + (length2 * length2) - (length1 * length1)) / (2 * length0 * length2)) * Mathf.Rad2Deg;
    }

    public void drawTriangles()
    {
        foreach (DTEdge e in finalEdgesList)
        {
            e.drawEdge("final tri");
        }
    }


    //Construct a list of all the edges actually in the triangulation
    private void ConstructFinalEdgeList()
    {
        foreach (DTTriangle aTriangle in triangleList)
        {
            foreach (DTEdge aEdge in aTriangle.GetEdges())
            {
                //stop edges connecting to the omega triangle to be added to the final list
                if (aEdge.getNodeA().getParentRoom() != null && aEdge.getNodeB().getParentRoom() != null)
                {
                    bool tmp = true;
                    
                    foreach(DTEdge aEdge2 in finalEdgesList)
                    {
                        if (aEdge.checkSame(aEdge2))
                        {
                            tmp = false;
                        }
                    }
                    
                    if(tmp)
                        finalEdgesList.Add(aEdge);
                }
                aEdge.stopDraw();
            }
        }
    }

    public List<DTEdge> GetTriangulation()
    {
        return finalEdgesList;
    }
}
