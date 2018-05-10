using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Triangulation implementation
public class DTriangulation
{

    //All the triangles in the triangulation
    private List<DTTriangle> triangleList = new List<DTTriangle>();

    //Verticies that still need to be added to the triangulations
    private List<DTNode> toAddList = new List<DTNode>();

    //the current verticie that is being added to the triangulation
    private DTNode nextNode = null;

    //Edges that have become possibly unDelaunay due to the insertion of another verticie
    private List<DTEdge> dirtyEdges = new List<DTEdge>();

    //the omega triangle created at start of triangulation
    private DTTriangle rootTriangle;

    //the triangle the "nextNode" is inside of
    private DTTriangle inTriangle;

    private List<DTEdge> finalTriangulation = new List<DTEdge>();
    private System.Random pseudoRandom;

    //construvtor
    public DTriangulation()
    {

    }

    //Handles set up of triangulation
    public void setupTriangulation(List<DTNode> _roomList)
    {

        //puts all verticies into the toDo list
        foreach (DTNode aNode in _roomList)
        {
            toAddList.Add(aNode);
        }

        //creates three artificial verticies for the omega triangle
        DTNode node0 = new DTNode(0, 250, null);

        DTNode node1 = new DTNode(-250, -200, null);

        DTNode node2 = new DTNode(250, -200, null);

        //creates the omega triangle
        rootTriangle = new DTTriangle(new DTEdge(node0, node1), new DTEdge(node0, node2), new DTEdge(node1, node2));

        //adds the omega triangle to the triangle list
        triangleList.Add(rootTriangle);
    }


    internal void triangulate(List<DTNode> roomList, System.Random random)
    {
        this.pseudoRandom = random;
        setupTriangulation(roomList);

        while (toAddList.Count > 0)
        {
            addVertexToTriangulation();
        }

        
        //drawTriangles();
        constructFinal();
    }

    //Adds a verticies to the triangulation
    private void addVertexToTriangulation()
    {
        //Find a Random verticie from the todo list
        int choice = pseudoRandom.Next(0, toAddList.Count);

        //set next node to selected verticies
        nextNode = toAddList[choice];

        //remove selected verticies from todo list
        toAddList.Remove(nextNode);

        //stores triangles created during the loop to be appended to main list after loop
        List<DTTriangle> tempTriList = new List<DTTriangle>();

        //All edges are clean at this point. Remove any that may be left over from previous loop
        dirtyEdges.Clear();

        float count = -1;
        foreach (DTTriangle aTri in triangleList)
        {
            List<DTEdge> triEdges = aTri.getEdges();
            count++;
            //Find which triangle the current vertex being add is located within
            if (LineIntersector.PointInTraingle(nextNode.getNodePosition(), triEdges[0].getNode1().getNodePosition(),
                triEdges[0].getNode2().getNodePosition(), triEdges[1].getNode2().getNodePosition()))
            {

                //cache the triangle we are in so we can delete it after loop
                inTriangle = aTri;

                //create three new triangles from each edge of the triangle vertex is in to the new vertex
                foreach (DTEdge aEdge in aTri.getEdges())
                {
                    DTTriangle nTri1 = new DTTriangle(new DTEdge(nextNode, aEdge.getNode1()),
                                    new DTEdge(nextNode, aEdge.getNode2()),
                                    new DTEdge(aEdge.getNode2(), aEdge.getNode1()));

                    //cache created triangles so we can add to list after loop
                    tempTriList.Add(nTri1);

                    //mark the edges of the old triangle as dirty
                    dirtyEdges.Add(new DTEdge(aEdge.getNode1(), aEdge.getNode2()));

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
            inTriangle.stopDraw();
            inTriangle = null;
        }

        checkEdges(dirtyEdges);

    }

    private void checkEdges(List<DTEdge> _list)
    {
        //the current dirty edge
        if (_list.Count == 0)
        {
            if (toAddList.Count > 0)
            {
                addVertexToTriangulation();
            }
            return;
        }

        //get the next edge in the dirty list
        DTEdge currentEdge = _list[0];

        DTTriangle[] connectedTris = new DTTriangle[2];
        int index = 0;


        foreach (DTTriangle aTri in triangleList)
        {
            if (aTri.checkTriangleContainsEdge(currentEdge))
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
                foreach (DTEdge aEdge in connectedTris[i].getEdges())
                {
                    if (!currentEdge.edgeContainsVertex(aEdge.getNode1()))
                    {
                        uniqueNodes[index1] = aEdge.getNode1();
                        index1++;
                        break;
                    }

                    if (!currentEdge.edgeContainsVertex(aEdge.getNode2()))
                    {
                        uniqueNodes[index1] = aEdge.getNode2();
                        index1++;
                        break;
                    }
                }
            }


            //find the angles of the two unique verticies
            float angle0 = calculateVertexAngle(uniqueNodes[0].getNodePosition(),
                                                currentEdge.getNode1().getNodePosition(),
                                                currentEdge.getNode2().getNodePosition());

            float angle1 = calculateVertexAngle(uniqueNodes[1].getNodePosition(),
                                                currentEdge.getNode1().getNodePosition(),
                                                currentEdge.getNode2().getNodePosition());

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
                sharedNode0 = currentEdge.getNode1();
                sharedNode1 = currentEdge.getNode2();

                //construct a new triangle to update old triangle after flip
                firstTriEdges[0] = new DTEdge(uniqueNodes[0], sharedNode0);
                firstTriEdges[1] = new DTEdge(sharedNode0, uniqueNodes[1]);
                firstTriEdges[2] = flippedEdge;

                //construct a new triangle to update the other old triangle after flip
                secondTriEdges[0] = new DTEdge(uniqueNodes[1], sharedNode1);
                secondTriEdges[1] = new DTEdge(sharedNode1, uniqueNodes[0]);
                secondTriEdges[2] = flippedEdge;

                //update the edges of the triangles involved in the flip
                connectedTris[0].setEdges(firstTriEdges[0], firstTriEdges[1], firstTriEdges[2]);
                connectedTris[1].setEdges(secondTriEdges[0], secondTriEdges[1], secondTriEdges[2]);


                //Adds all edges to be potentially dirty. This is bad and should only add the edges that *could* be dirty
                foreach (DTEdge eEdge in connectedTris[0].getEdges())
                {
                    _list.Add(eEdge);
                }

                foreach (DTEdge eEdge in connectedTris[1].getEdges())
                {
                    _list.Add(eEdge);
                }

                //also add new edge to dirty list
                _list.Add(flippedEdge);
            }
        }

        //remove the current edge from the dirty list
        _list.Remove(currentEdge);

        checkEdges(_list);
    }

    //calculates the angle at vertex _target in triangle (_target _shared0 _shared1) in degrees
    private float calculateVertexAngle(Vector2 _target, Vector2 _shared0, Vector2 _shared1)
    {
        float length0 = Vector2.Distance(_target, _shared0);
        float length1 = Vector2.Distance(_shared0, _shared1);
        float length2 = Vector2.Distance(_shared1, _target);

        return Mathf.Acos(((length0 * length0) + (length2 * length2) - (length1 * length1)) / (2 * length0 * length2)) * Mathf.Rad2Deg;
    }

    public void drawTriangles()
    {
        foreach (DTEdge e in finalTriangulation)
        {
            e.drawEdge("final tri");
        }
    }


    //Construct a list of all the edges actually in the triangulation
    private void constructFinal()
    {
        foreach (DTTriangle aTriangle in triangleList)
        {
            foreach (DTEdge aEdge in aTriangle.getEdges())
            {
                //stop edges connecting to the omega triangle to be added to the final list
                if (aEdge.getNode1().getParentCell() != null && aEdge.getNode2().getParentCell() != null)
                {
                    bool tmp = true;
                    
                    foreach(DTEdge aEdge2 in finalTriangulation)
                    {
                        if (aEdge.checkSame(aEdge2))
                        {
                            tmp = false;
                        }
                    }
                    
                    if(tmp)
                        finalTriangulation.Add(aEdge);
                }
                aEdge.stopDraw();
            }
        }
    }

    public List<DTEdge> getTriangulation()
    {
        return finalTriangulation;
    }
}
