using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public static ICollection<Shape> shapeList = new List<Shape>{};
    public ICollection<PlaneOrientation> partialPlanesOrientation = new List<PlaneOrientation>{};
    public Vector3[] vertices;
    private new Renderer renderer;

    private void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
        shapeList.Add(this);
        vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;

        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }
    }

    public void SwitchMaterial(Material newMaterial)
    {
        renderer.material = newMaterial;
    }

    private void OnDrawGizmos()
    {
        foreach(Vector3 vertex in vertices)
        {
            Gizmos.DrawSphere(vertex, 0.1f);
        }
    }

    /*public Shape(ICollection<Vector3> vertices)
    {
        this.vertices = vertices;
    }
    */
}
