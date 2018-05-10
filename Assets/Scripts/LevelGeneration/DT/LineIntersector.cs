using UnityEngine;

//A class to check some intersections and collisions between some shapes and points
public static class LineIntersector
{

    //Code retrieved from: http://forum.unity3d.com/threads/17384-Line-Intersection 
    public static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {

        Vector2 a = p2 - p1;
        Vector2 b = p3 - p4;
        Vector2 c = p1 - p3;


        float alphaNumerator = b.y * c.x - b.x * c.y;
        float alphaDenominator = a.y * b.x - a.x * b.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float betaDenominator = a.y * b.x - a.x * b.y;

        bool doIntersect = true;


        if (alphaDenominator == 0 || betaDenominator == 0)
        {
            doIntersect = false;
        }
        else
        {
            if (alphaDenominator > 0)
            {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                {
                    doIntersect = false;
                }

            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
            {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0)
            {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (betaNumerator > 0 || betaNumerator < betaDenominator)
            {
                doIntersect = false;
            }
        }


        return doIntersect;

    }

    //Method to check point inside triangle
    public static bool PointInTraingle(Vector2 _p, Vector2 _a, Vector2 _b, Vector2 _c)
    {
        if (SameSide(_p, _a, _b, _c) && SameSide(_p, _b, _a, _c) && SameSide(_p, _c, _a, _b))
        {
            return true;
        }

        return false;
    }

    private static bool SameSide(Vector2 _p1, Vector2 _p2, Vector2 _a, Vector2 _b)
    {
        Vector3 cp1 = Vector3.Cross(_b - _a, _p1 - _a);
        Vector3 cp2 = Vector3.Cross(_b - _a, _p2 - _a);

        //use Z because cross product x and Y always return zero
        Vector2 vP1 = new Vector2(cp1.x, cp1.z);
        Vector2 vP2 = new Vector2(cp2.x, cp2.z);

        if (Vector2.Dot(vP1, vP2) >= 0)
        {
            return true;
        }

        return false;
    }
}
