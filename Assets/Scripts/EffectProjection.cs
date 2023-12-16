using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public class EffectProjection : MonoBehaviour
{
    private Vector3[] closeCamVertices;
    private Vector3[] farCamVertices;
    private Camera cam;
    float zDistance;
    float yDistance;
    float xDistance;

    private void Start()
    {
        cam = gameObject.GetComponent<Camera>();
        Gizmos.color = Color.yellow;

        zDistance = cam.farClipPlane;
        yDistance = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad /2) * zDistance;
        xDistance = Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect) * Mathf.Deg2Rad /2) * zDistance;
    }

    private void Update()
    {
        closeCamVertices = CalculateCloseCamVertices();
        farCamVertices = CalculateFarCamVertices();
    }

    private Vector3[] CalculateCloseCamVertices()
    {
        Vector3 near_topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        Vector3 near_topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        Vector3 near_bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        Vector3 near_bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        return new Vector3[4] {near_topLeft, near_topRight, near_bottomRight, near_bottomLeft};
    }

    private Vector3[] CalculateFarCamVertices()
    {
        Vector3 far_topLeft = closeCamVertices[0] + (-xDistance * transform.right) + (yDistance * transform.up) + (zDistance * transform.forward);
        Vector3 far_topRight = closeCamVertices[0] + (xDistance * transform.right) + (yDistance * transform.up) + (zDistance * transform.forward);
        Vector3 far_bottomRight = closeCamVertices[0] + (xDistance * transform.right) + (-yDistance * transform.up) + (zDistance * transform.forward);
        Vector3 far_bottomLeft = closeCamVertices[0] + (-xDistance * transform.right) + (-yDistance * transform.up) + (zDistance * transform.forward);

        return new Vector3[4] {far_topLeft, far_topRight, far_bottomRight, far_bottomLeft};
    }

    private void OnDrawGizmos()
    {
        if(closeCamVertices != null && farCamVertices != null)
        {
            foreach(Vector3 vertex in closeCamVertices)
                Gizmos.DrawSphere(vertex, 0.01f);

            foreach(Vector3 vertex in farCamVertices)
                Gizmos.DrawSphere(vertex, 50f);
        }
    }
}
