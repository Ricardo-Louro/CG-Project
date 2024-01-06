//Declare relevant namespaces
using UnityEngine;

//Defines a Plane class to be utilized and created with the necessary conditions for the effect to be accomplished
public class Plane
{
    //Plane Orientation variable which is utilized to find this plane's parallel planes
    public PlaneOrientation orientation {get; private set;}
    //The normal vector of this plane utilized to calculate the position of any point relative to the plane
    public Vector3 normal {get; private set;}
    //Float value utilized for the comparison between a point and the plane
    public float d {get; private set;}

    //Method to update the plane depending on the new values it receives every frame
    public void UpdatePlane(Vector3 a, Vector3 b, Vector3 point, PlaneOrientation planeOrientation)
    {
        //Set the normal vector as the normalized cross between two specific points of the plane 
        normal = Vector3.Cross(a,b).normalized;

        //Calculate the d according to the relevant equation
        d = -(normal.x * point.x
            + normal.y * point.y
            + normal.z * point.z);
        
        //Set the plane's orientation
        orientation = planeOrientation; 
    }

    //Plane equation: (normal.X * x) + (normal.Y * y) + (normal.Z * z) + d = 0





    //Tests the relation between a shape and a plane to see whether it's fully in, partially in or fully out of the camera according to said plane
    public IntersectionState TestShape(Shape shape, bool storeOrientation)
    {
        //Initialize a flag on if the test was successful as false
        bool successfulTest = false;
        //Initialize a flag on if the test has failed as false
        bool failedTest = false;

        //Iterate through each vertex of the Shape
        foreach(Vector3 vertex in shape.vertices)
        {
            //Utilize the equation to compare said vertex to the plane
            float comparator = (normal.x * vertex.x)
                             + (normal.y * vertex.y)
                             + (normal.z * vertex.z)
                             + d;

            //If said vertex is within the desired area...
            if(comparator < 0)
            {
                //...set the successful test flag as true
                successfulTest = true;
            }
            //If said vertex is outside the desired area...
            else
            {
                //...set the failed test flag as true
                failedTest = true;
            }
        }

        //If both the successful test flag and the failed test flag are true...
        if(successfulTest && failedTest)
        {
            //...if the test had the indication to store the orientation within the shape...
            if(storeOrientation)
            {
                //...store the plane's orientation within the shape
                shape.partialPlanes.Add(this);
            }
            //...return the Intersection State between the plane and the shape are partly inside
            return IntersectionState.Partly;
        }
        //If only the successful test flag is true...
        else if(successfulTest)
        {
            //...return the Intersection State between the plane and the shape as fully inside
            return IntersectionState.Fully;
        }
        //If only the failed test flag is true...
        else
        {
            //...return the Intersection State between the plane and the shape as fully outside
            return IntersectionState.None;
        }
    }

    public bool TestVertex(Vector3 vertex)
    {
                    //Utilize the equation to compare said vertex to the plane
            float comparator = (normal.x * vertex.x)
                             + (normal.y * vertex.y)
                             + (normal.z * vertex.z)
                             - d;

            //If said vertex is within the desired area...
            if(comparator < 0)
            {
                //...return true
                return true;
            }
            //If said vertex is outside the desired area...
            else
            {
                //...return false
                return false;
            }
    }
}
