﻿using UnityEngine;
using System.Collections;


class Cell : MonoBehaviour
{
    private float squareDistance;

    float xShift = 0;
    float yShift = 0;

    private float xMove = 0, yMove = 0;

    private bool hasStopped;
    private Vector2 oldPos = new Vector2();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] allCells = GameObject.FindGameObjectsWithTag("Cell");

        for (int i = 0; i < allCells.Length; i++)
        {
            if (allCells[i] != this.gameObject)
            {
                if (isOverlapping(allCells[i]))
                {
                    Vector3 direction = transform.position - allCells[i].transform.position;
                    direction.Normalize();
                    if (direction.x == 0 && direction.y == 0)
                        direction = new Vector3(1f, 1f, 0f);

                    xShift += direction.x;
                    yShift += direction.y;
                }
            }
        }

        transform.position = new Vector3(Mathf.Round(transform.position.x + xShift - xMove) + xMove, Mathf.Round(transform.position.y + yShift - yMove) + yMove, transform.position.z);
        xShift = 0;
        yShift = 0;


        if (transform.position.x == oldPos.x && transform.position.y == oldPos.y)
        {
            hasStopped = true;
        }
        else
        {
            hasStopped = false;
        }


        oldPos = new Vector2(transform.position.x, transform.position.y);
    }

    public void setup(bool xEven, bool yEven)
    {
        if (xEven)
            xMove = 0.5f;
        if (yEven)
            yMove = 0.5f;

        transform.position = new Vector3(Mathf.Round(transform.position.x) + xMove, Mathf.Round(transform.position.y) + yMove, transform.position.z);

        foreach (Transform aChild in transform)
        {
            aChild.gameObject.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
        }
    }

    private bool isOverlapping(GameObject aObj)
    {

        Vector2 mySize = new Vector2(GetComponent<Renderer>().bounds.size.x, GetComponent<Renderer>().bounds.size.y);

        if (pointInside(aObj, new Vector2(transform.position.x + 1 - mySize.x / 2, transform.position.y + 1 - mySize.y / 2)))
        {
            return true;
        }

        if (pointInside(aObj, new Vector2(transform.position.x - 1 + mySize.x / 2, transform.position.y + 1 - mySize.y / 2)))
        {
            return true;
        }

        if (pointInside(aObj, new Vector2(transform.position.x + 1 - mySize.x / 2, transform.position.y - 1 + mySize.y / 2)))
        {
            return true;
        }

        if (pointInside(aObj, new Vector2(transform.position.x - 1 + mySize.x / 2, transform.position.y - 1 + mySize.y / 2)))
        {
            return true;
        }

        return false;
    }


    private bool pointInside(GameObject aObj, Vector2 _point)
    {
        Vector2 objSize = new Vector2(aObj.GetComponent<Renderer>().bounds.size.x, aObj.GetComponent<Renderer>().bounds.size.y);
        if ((_point.x) >= (aObj.transform.position.x - objSize.x / 2) &&
             (_point.x) <= (aObj.transform.position.x + objSize.x / 2) &&
             (_point.y) >= (aObj.transform.position.y - objSize.y / 2) &&
             (_point.y) <= (aObj.transform.position.y + objSize.y / 2))
        {
            return true;
        }

        return false;
    }

    public bool getHasStopped()
    {
        return hasStopped;
    }
}
