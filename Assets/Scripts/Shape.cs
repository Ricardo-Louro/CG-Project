//Declare relevant namespaces
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

//Defines a Shape to be utilized and created with the necessary conditions for the effect to be accomplished
public class Shape : MonoBehaviour
{
    public static ICollection<Plane> planes = new List<Plane>();
    //Static collection which contains all the Shapes in the scene
    public static ICollection<Shape> shapeList = new List<Shape>();
    //Collection which will contain the orientations of the planes which indicate this plane as partially inside the camera area
    public ICollection<Plane> partialPlanes = new List<Plane>();
    //Collection with no duplicates which will contain the new vertices which are the intersection between each line segment and the plane
    public ICollection<Vector3> intersectionVertices = new HashSet<Vector3>();
    //Array containing all the shape's vertices
    public Vector3[] vertices;
    //Collection with no duplicates which will contain the vertices considered to be inside the camera area
    public ICollection<Vector3> insideVertices = new HashSet<Vector3>();
    //Collection with no duplicates which will contain the vertices considered to be inside the camera area
    public ICollection<Vector3> outsideVertices = new HashSet<Vector3>();
    //Variable containing the renderer so that it can be referenced to
    private new Renderer renderer;

    //Runs the first frame when this object is active
    private void Start()
    {
        //Assign the variable with the renderer attached to this object
        renderer = gameObject.GetComponent<Renderer>();
        //Add this shape to the static list of all the shapes in the scene 
        shapeList.Add(this);
        //Collect all the vertices from the mesh and store them in the relevant variable
        UpdateVerticesList();

        //Iterate through each vertex in the vertices array...
        for(int i = 0; i < vertices.Length; i++)
        {
            //...and transform it from the local coordinates to the world coordinates
            vertices[i] = transform.TransformPoint(vertices[i]);
        }
    }

    //Switches the material of this object with another
    public void SwitchMaterial(Material newMaterial)
    {
        //Sets the material of the object with the new one
        renderer.material = newMaterial;
    }

    //Draw Gizmos utilized for debugging
    private void OnDrawGizmos()
    {
        //Iterate through each vertex in the vertices list...
        foreach(Vector3 vertex in vertices)
        {
            //...and draw a small sphere at that position 
            Gizmos.DrawSphere(vertex, 0.1f);
        }

        if(intersectionVertices.Count != 0)
        {
            foreach(Vector3 vertex in intersectionVertices)
            {
                Gizmos.DrawSphere(vertex, 0.1f);
            }
        }
    }

    public void CalculateIntersectionVertices()
    {
        foreach(Vector3 vertex in vertices)
        {
            bool outsideVertexFlag = false;
            foreach(Plane plane in planes)
            {
                if(!plane.TestVertex(vertex))
                {
                    outsideVertexFlag = true;
                }
            }
            if(!outsideVertexFlag)
            {
                insideVertices.Add(vertex);
            }
        }

        foreach(Vector3 insideVertex in insideVertices)
        {
            foreach(Vector3 vertex in vertices)
            {
                if(!insideVertices.Contains(vertex))
                {
                    Vector3 lineSegment = new Vector3(insideVertex.x - vertex.x,
                                                      insideVertex.y - vertex.y,
                                                      insideVertex.z - vertex.z);

                    foreach(Plane plane in partialPlanes)
                    {
                        /*(plane.normal.x * (vertex.x + lineSegment.x * t)) +
                          (plane.normal.y * (vertex.y + lineSegment.y * t)) +
                          (plane.normal.y * (vertex.y + lineSegment.y * t)) + d = 0*/

                        /*plane.normal.x * vertex.x +
                          plane.normal.y * vertex.y +
                          plane.normal.z * vertex.z + d +
                          t*(plane.normal.x * lineSegment.x +
                             plane.normal.y * lineSegment.y +
                             plane.normal.z * lineSegment.z) = 0 */

                        float t = - ((plane.normal.x * vertex.x +
                                    plane.normal.y * vertex.y +
                                    plane.normal.z * vertex.z + plane.d) 
                                    /
                                    (plane.normal.x * lineSegment.x +
                                    plane.normal.y * lineSegment.y +
                                    plane.normal.z * lineSegment.z));
                        
                        if(t >= 0 && t <= 1)
                        {
                            float x = vertex.x + lineSegment.x * t;
                            float y = vertex.y + lineSegment.y * t;
                            float z = vertex.z + lineSegment.z * t;

                            intersectionVertices.Add(new Vector3(x,y,z));
                        }
                    }
                }
            }
        }
        insideVertices.Clear();
    }

    //Collect all the vertices from the mesh and store them in the relevant variable
    private void UpdateVerticesList()
    {
        vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
    }

    /*public Shape(ICollection<Vector3> vertices)
    {
        this.vertices = vertices;
    }
    */
}
