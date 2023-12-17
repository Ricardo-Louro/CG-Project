using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public class EffectProjection : MonoBehaviour
{

    [SerializeField] private Material fullyMaterial;
    [SerializeField] private Material partlyMaterial;
    [SerializeField] private Material noneMaterial;

    private ICollection<Vector3> nearVertices = null;
    private Vector3 near_topLeft;
    private Vector3 near_topRight;
    private Vector3 near_bottomRight;
    private Vector3 near_bottomLeft;
    private ICollection<Vector3> farVertices = null;
    private Vector3 far_topLeft;
    private Vector3 far_topRight;
    private Vector3 far_bottomRight;
    private Vector3 far_bottomLeft;
    private Camera cam;
    private float zDistance;
    private float yDistance;
    private float xDistance;
    private ICollection<Plane> planeList;
    private Plane topPlane;
    private Plane botPlane;
    private Plane leftPlane;
    private Plane rightPlane;
    private Plane closePlane;
    private Plane farPlane;
    private Dictionary<PlaneOrientation, Plane[][]> parallelPlanesDict;

    private void Start()
    {
        cam = gameObject.GetComponent<Camera>();
        //Gizmos.color = Color.yellow;

        zDistance = cam.farClipPlane;
        yDistance = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad /2) * zDistance;
        xDistance = Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect) * Mathf.Deg2Rad /2) * zDistance;
        
        topPlane = new Plane();
        botPlane = new Plane();
        leftPlane = new Plane();
        rightPlane = new Plane();
        closePlane = new Plane();
        farPlane = new Plane();

        parallelPlanesDict = new Dictionary<PlaneOrientation, Plane[][]>
        {
            { PlaneOrientation.top_bottom, new Plane[2][]{new Plane[2] { leftPlane, rightPlane }, new Plane[2] { closePlane, farPlane }}},
            { PlaneOrientation.left_right, new Plane[2][]{new Plane[2] { topPlane, botPlane }, new Plane[2] { closePlane, farPlane }}},
            { PlaneOrientation.close_far, new Plane[2][]{new Plane[2] { leftPlane, rightPlane }, new Plane[2] { topPlane, botPlane }}},
        };
    }

    private void Update()
    {
        CalculateCloseCamVertices();
        CalculateFarCamVertices();
        planeList = CalculatePlanes();

        foreach(Shape shape in Shape.shapeList)
        {
            bool fullyInside = true;
            bool partlyOutside = false;
            foreach(Plane plane in planeList)
            {
                IntersectionState state = plane.TestShape(shape, true);
                if(state != IntersectionState.Fully)
                    fullyInside = false;
                if(state == IntersectionState.Partly)
                    partlyOutside = true;
            }

            if(fullyInside)
            {
                shape.SwitchMaterial(fullyMaterial);
            }
            else if(partlyOutside)
            {
                bool inBounds = true;
                foreach(PlaneOrientation orientation in shape.partialPlanesOrientation)
                {
                    inBounds = CompareParallelPlanesTests(orientation, shape);
                    if(!inBounds)
                        break;
                }
                shape.partialPlanesOrientation = new List<PlaneOrientation>{};
                if(inBounds)
                    shape.SwitchMaterial(partlyMaterial);
                else
                    shape.SwitchMaterial(noneMaterial);
            }
            else
                shape.SwitchMaterial(noneMaterial);
        }
    }

    private void CalculateCloseCamVertices()
    {
        near_topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        near_topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        near_bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        near_bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        nearVertices = new Vector3[4] {near_topLeft, near_topRight, near_bottomRight, near_bottomLeft};
    }

    private void CalculateFarCamVertices()
    {
        far_topLeft = near_topLeft + (-xDistance * transform.right)
                                   + (yDistance * transform.up)
                                   + (zDistance * transform.forward);

        far_topRight = near_topRight + (xDistance * transform.right)
                                     + (yDistance * transform.up)
                                     + (zDistance * transform.forward);

        far_bottomRight = near_bottomRight + (xDistance * transform.right)
                                           + (-yDistance * transform.up)
                                           + (zDistance * transform.forward);

        far_bottomLeft = near_bottomLeft + (-xDistance * transform.right)
                                         + (-yDistance * transform.up)
                                         + (zDistance * transform.forward);

        farVertices = new Vector3[4] {far_topLeft, far_topRight, far_bottomRight, far_bottomLeft};
    }

    private ICollection<Plane> CalculatePlanes()
    {
        topPlane.UpdatePlane(far_topLeft - near_topLeft,
                             near_topRight - near_topLeft,
                             near_topLeft,
                             PlaneOrientation.top_bottom);

        Debug.DrawLine(near_topLeft, far_topLeft, Color.black);
        Debug.DrawLine(near_topRight, far_topRight, Color.black);
        Debug.DrawLine(near_topLeft, near_topRight, Color.black);
        Debug.DrawLine(far_topLeft, far_topRight, Color.black);

        botPlane.UpdatePlane(far_bottomRight - near_bottomRight,
                             near_bottomLeft - near_bottomRight,
                             far_bottomRight,
                             PlaneOrientation.top_bottom);

        Debug.DrawLine(near_bottomLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(near_bottomRight, far_bottomRight, Color.black);
        Debug.DrawLine(near_bottomLeft, near_bottomRight, Color.black);
        Debug.DrawLine(far_bottomLeft, far_bottomRight, Color.black);

        leftPlane.UpdatePlane(far_bottomLeft - near_bottomLeft,
                              near_topLeft - near_bottomLeft,
                              near_bottomLeft,
                              PlaneOrientation.left_right);

        Debug.DrawLine(near_bottomLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(near_topLeft, far_topLeft, Color.black);
        Debug.DrawLine(near_bottomLeft, near_topLeft, Color.black);
        Debug.DrawLine(far_bottomLeft, far_topLeft, Color.black);

        rightPlane.UpdatePlane(far_topRight - near_topRight,
                               near_bottomRight - near_topRight,
                               near_topRight,
                               PlaneOrientation.left_right);

        Debug.DrawLine(near_bottomRight, far_bottomRight, Color.black);
        Debug.DrawLine(near_topRight, far_topRight, Color.black);
        Debug.DrawLine(near_bottomRight, near_topRight, Color.black);
        Debug.DrawLine(far_bottomRight, far_topRight, Color.black);

        closePlane.UpdatePlane(near_bottomRight - near_topRight,
                               near_topLeft - near_topRight,
                               near_bottomRight,
                               PlaneOrientation.close_far);

        Debug.DrawLine(near_topLeft, near_topRight, Color.black);
        Debug.DrawLine(near_bottomLeft, near_bottomRight, Color.black);
        Debug.DrawLine(near_topLeft, near_bottomLeft, Color.black);
        Debug.DrawLine(near_bottomRight, near_topRight, Color.black);
        
        farPlane.UpdatePlane(far_bottomLeft - far_topLeft,
                             far_topRight - far_topLeft,
                             far_bottomLeft,
                             PlaneOrientation.close_far);
        
        Debug.DrawLine(far_topLeft, far_topRight, Color.black);
        Debug.DrawLine(far_bottomLeft, far_bottomRight, Color.black);
        Debug.DrawLine(far_topLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(far_bottomRight, far_topRight, Color.black);

        return new Plane[6] {topPlane, botPlane, leftPlane, rightPlane, closePlane, farPlane};
    }

    private bool CompareParallelPlanesTests(PlaneOrientation orientation, Shape shape)
    {
        foreach(Plane[] planeArray in parallelPlanesDict[orientation])
        {
            if((planeArray[0].TestShape(shape, false) == IntersectionState.Fully &
                planeArray[1].TestShape(shape, false) == IntersectionState.None) |
               (planeArray[0].TestShape(shape, false) == IntersectionState.None &
                planeArray[1].TestShape(shape, false) == IntersectionState.Fully))
                    return false;
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        if(nearVertices != null && farVertices != null)
        {
            foreach(Vector3 vertex in nearVertices)
                Gizmos.DrawSphere(vertex, 0.01f);

            foreach(Vector3 vertex in farVertices)
                Gizmos.DrawSphere(vertex, 50f);
        }
    }
}
