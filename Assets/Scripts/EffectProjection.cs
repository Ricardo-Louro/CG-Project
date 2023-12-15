using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectProjection : MonoBehaviour
{
    private Vector3[] closeCamVertices;
    private Camera cam;

    private void Start()
    {
        cam = gameObject.GetComponent<Camera>();
        Gizmos.color = Color.magenta;
    }

    private void Update()
    {
        closeCamVertices = CalculateCloseCamVertices();
    }

    private Vector3[] CalculateCloseCamVertices()
    {
        Vector3 near_topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        Vector3 near_topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        Vector3 near_bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        Vector3 near_bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        return new Vector3[4] {near_topLeft, near_topRight, near_bottomRight, near_bottomLeft};
    }

    /*private void OnDrawGizmos()
    {
        foreach(Vector3 vertex in closeCamVertices)
        {
            Gizmos.DrawSphere(vertex, 0.01f);
        }
    }
    */
}
