//Declare relevant namespaces
using System.Collections.Generic;
using UnityEngine;

//Defines a Shape to be utilized and created with the necessary conditions for the effect to be accomplished
public class Shape : MonoBehaviour
{
    //Static collection which contains all the Shapes in the scene
    public static ICollection<Shape> shapeList = new List<Shape>{};
    //Collection which will contain the orientations of the planes which indicate this plane as partially inside the camera area
    public ICollection<PlaneOrientation> partialPlanesOrientation = new List<PlaneOrientation>{};
    //Array containing all the shape's vertices
    public Vector3[] vertices;
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
        vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;

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
    }

    /*public Shape(ICollection<Vector3> vertices)
    {
        this.vertices = vertices;
    }
    */
}
