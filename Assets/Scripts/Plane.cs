using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane
{
    public PlaneOrientation orientation {get; private set;}
    private Vector3 normal;
    private float d;

    public void UpdatePlane(Vector3 a, Vector3 b, Vector3 point, PlaneOrientation planeOrientation)
    {
        normal = Vector3.Cross(a,b).normalized;

        d = normal.x * point.x
          + normal.y * point.y
          + normal.z * point.z;
        
       orientation = planeOrientation; 
    }

    public IntersectionState TestShape(Shape shape, bool storeOrientation)
    {
        bool successfulTest = false;
        bool failedTest = false;
        foreach(Vector3 vertex in shape.vertices)
        {
            float comparator = (normal.x * vertex.x)
                             + (normal.y * vertex.y)
                             + (normal.z * vertex.z)
                             - d;

            if(comparator < 0)
                successfulTest = true;
            else
                failedTest = true;
        }

        if(successfulTest && failedTest)
        {
            if(storeOrientation)
                shape.partialPlanesOrientation.Add(orientation);
            return IntersectionState.Partly;
        }
        else if(successfulTest)
            return IntersectionState.Fully;
        else
            return IntersectionState.None;
    }
}
