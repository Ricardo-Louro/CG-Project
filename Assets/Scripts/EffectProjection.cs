//Declare relevant namespaces
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

//This class handles the projection phase of the effect
public class EffectProjection : MonoBehaviour
{
    //Declare PlayerController to access its triggers for actions
    [SerializeField] private PlayerController           playerController;
    private ICollection<Shape>                          partialShapes = new HashSet<Shape>();

    //Declare materials to replace on the shapes
    [SerializeField] private Material                   fullyMaterial;
    [SerializeField] private Material                   partlyMaterial;
    [SerializeField] private Material                   noneMaterial;

    //Declares variables to store the calculated camera vertices on
    private ICollection<Vector3>                        nearVertices = null;
    private Vector3                                     near_topLeft;
    private Vector3                                     near_topRight;
    private Vector3                                     near_bottomRight;
    private Vector3                                     near_bottomLeft;
    private ICollection<Vector3>                        farVertices = null;
    private Vector3                                     far_topLeft;
    private Vector3                                     far_topRight;
    private Vector3                                     far_bottomRight;
    private Vector3                                     far_bottomLeft;

    //Declare camera and a few of its measures as variables
    private Camera                                      cam;
    private float                                       zDistance;
    private float                                       yDistance;
    private float                                       xDistance;

    //Declare variables to store the camera's planes
    private ICollection<Plane>                          planeList;
    private Plane                                       topPlane;
    private Plane                                       botPlane;
    private Plane                                       leftPlane;
    private Plane                                       rightPlane;
    private Plane                                       closePlane;
    private Plane                                       farPlane;
    private Dictionary<PlaneOrientation, Plane[][]>     parallelPlanesDict;

    //Runs on the first start where this component is active
    private void Start()
    {
        //Assign the camera variables into their places so they can be referenced
        cam = gameObject.GetComponent<Camera>();
        zDistance = cam.farClipPlane;
        yDistance = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad /2) * zDistance;
        xDistance = Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect) * Mathf.Deg2Rad /2) * zDistance;
        
        //Assign each plane variable with a new instance of a plane which can then be modified
        topPlane = new Plane();
        botPlane = new Plane();
        leftPlane = new Plane();
        rightPlane = new Plane();
        closePlane = new Plane();
        farPlane = new Plane();

        //Initialize the parallel plane dictionary with the relevant values which will be used to test the shapes later on
        parallelPlanesDict = new Dictionary<PlaneOrientation, Plane[][]>
        {
            { PlaneOrientation.top_bottom, new Plane[2][]{new Plane[2] { leftPlane, rightPlane }, new Plane[2] { closePlane, farPlane }}},
            { PlaneOrientation.left_right, new Plane[2][]{new Plane[2] { topPlane, botPlane }, new Plane[2] { closePlane, farPlane }}},
            { PlaneOrientation.close_far, new Plane[2][]{new Plane[2] { leftPlane, rightPlane }, new Plane[2] { topPlane, botPlane }}},
        };
    }

    //Runs every single frame after the first one when this object is active
    private void Update()
    {
        //Calculates the world position for each vertex on the close plane of the cam
        nearVertices = CalculateCloseCamVertices();
        //Calculates the world position for each vertex on the far plane of the cam
        farVertices = CalculateFarCamVertices();
        //Calculate the planes based on the previously calculated vertices
        planeList = CalculatePlanes();

        //Iterate through every shape in the scene to test if they're inside the camera planes
        foreach(Shape shape in Shape.shapeList)
        {
            //Declare local variable which serves on the tests to see if the shape is completely inside the planes 
            bool fullyInside = true;
            //Declare local variable which serves on the tests to see if the shape is partially inside the planes
            bool partlyOutside = false;

            //Iterate through every plane of the camera (top, bot, left, right, close, far)
            foreach(Plane plane in planeList)
            {
                //See what the intersection state between the current plane and shape is (fully inside, fully outside or partially inside)
                IntersectionState state = plane.TestShape(shape, true);

                //If the shape is not fully inside with one of the planes...
                if(state != IntersectionState.Fully)
                {
                    //...set the local variable as false to symbolize that the shape cannot be considered fully inside
                    fullyInside = false;   
                }
                //If the shape is considered partially inside with one of the planes...
                if(state == IntersectionState.Partly)
                {
                    //...set the local variable as true to symbolize that the shape was considered partly inside by at least one of the planes
                    partlyOutside = true;
                }
            }

            //If at the end of testing every plane, the shape was considered fully inside by all of them...
            if(fullyInside)
            {
                //...switch the shape's material for the fully inside one.
                shape.SwitchMaterial(fullyMaterial);
            }
            //If at the end of testing every plane, at least one of them considered the shape to be only partly inside...
            else if(partlyOutside)
            {
                //...set a local variable to see if the object is considered to be inside the camera area (partly inside can be triggered for objects out of the camera area)
                bool inBounds = true;
                
                //Iterate through every orientation whose planes considered the shape to be partly inside
                foreach(Plane plane in shape.partialPlanes)
                {
                    //Test the shape to see if its not outside of the camera area
                    inBounds = CompareParallelPlanesTests(plane.orientation, shape);
                    //If it isn't...
                    if(!inBounds)
                    {
                        //Break out of the loop
                        break;
                    }
                }

                //If the shape was determined to have remained inside the camera's bounds for all the tests (and as such is a true partly inside result)
                if(inBounds)
                {
                    partialShapes.Add(shape);
                    //Replace the material of the shape with the partly inside material
                    shape.SwitchMaterial(partlyMaterial);

                }
                //If the shape was determined to be outside of the camera's bounds on a test (and as such is a false partly inside result)
                else
                {
                    //Replace the material of the shape with the fully outside material
                    shape.SwitchMaterial(noneMaterial);
                }
            }
            //If the shape was not considered to be fully inside nor partially inside (it is fully outside)
            else
            {
                //Replace the material of the shape with the fully outside material
                shape.SwitchMaterial(noneMaterial);
            }
        }
        foreach(Shape shape in partialShapes)
        {
            shape.CalculateIntersectionVertices();
            //Clear the orientation list after the tests so it can be reused on future cases
            shape.partialPlanes = new List<Plane>{};
        }
    }

    //Calculates the world position of the vertices on the camera's frustum closer to the player 
    private Vector3[] CalculateCloseCamVertices()
    {
        //Converts the point on the top left of the viewport into world coordinates
        near_topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        //Converts the point on the top right of the viewport into world coordinates
        near_topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        //Converts the point on the bottom right of the viewport into world coordinates
        near_bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        //Converts the point on the bottom left of the viewport into world coordinates
        near_bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        //Return all the converted points as an array of Vector3
        return new Vector3[4] {near_topLeft, near_topRight, near_bottomRight, near_bottomLeft};
    }

    //Calculates the world position of the vertices on the camera's far end
    private Vector3[] CalculateFarCamVertices()
    {
        //Transforms the coordinates of the near top left point into the far top left point
        far_topLeft = near_topLeft + (-xDistance * transform.right)
                                   + (yDistance * transform.up)
                                   + (zDistance * transform.forward);

        //Transforms the coordinates of the near top right point into the far top right point
        far_topRight = near_topRight + (xDistance * transform.right)
                                     + (yDistance * transform.up)
                                     + (zDistance * transform.forward);

        //Transforms the coordinates of the near bottom left point into the far bottom left point
        far_bottomRight = near_bottomRight + (xDistance * transform.right)
                                           + (-yDistance * transform.up)
                                           + (zDistance * transform.forward);

        //Transforms the coordinates of the near bottom left point into the far bottom left point
        far_bottomLeft = near_bottomLeft + (-xDistance * transform.right)
                                         + (-yDistance * transform.up)
                                         + (zDistance * transform.forward);

        //Return all the converted points as an array of Vector3
        return new Vector3[4] {far_topLeft, far_topRight, far_bottomRight, far_bottomLeft};
    }

    //Calculates each of the planes utilizing the previously calculated points
    private ICollection<Plane> CalculatePlanes()
    {
        //Calculates the top plane of the camera's pyramid and stores it in the relevant variable
        topPlane.UpdatePlane(far_topLeft - near_topLeft,
                             near_topRight - near_topLeft,
                             near_topLeft,
                             PlaneOrientation.top_bottom);

        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(near_topLeft, far_topLeft, Color.black);
        Debug.DrawLine(near_topRight, far_topRight, Color.black);
        Debug.DrawLine(near_topLeft, near_topRight, Color.black);
        Debug.DrawLine(far_topLeft, far_topRight, Color.black);

        //Calculates the bot plane of the camera's pyramid and stores it in the relevant variable
        botPlane.UpdatePlane(far_bottomRight - near_bottomRight,
                             near_bottomLeft - near_bottomRight,
                             far_bottomRight,
                             PlaneOrientation.top_bottom);

        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(near_bottomLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(near_bottomRight, far_bottomRight, Color.black);
        Debug.DrawLine(near_bottomLeft, near_bottomRight, Color.black);
        Debug.DrawLine(far_bottomLeft, far_bottomRight, Color.black);

        //Calculates the left plane of the camera's pyramid and stores it in the relevant variable
        leftPlane.UpdatePlane(far_bottomLeft - near_bottomLeft,
                              near_topLeft - near_bottomLeft,
                              near_bottomLeft,
                              PlaneOrientation.left_right);

        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(near_bottomLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(near_topLeft, far_topLeft, Color.black);
        Debug.DrawLine(near_bottomLeft, near_topLeft, Color.black);
        Debug.DrawLine(far_bottomLeft, far_topLeft, Color.black);

        //Calculates the right plane of the camera's pyramid and stores it in the relevant variable
        rightPlane.UpdatePlane(far_topRight - near_topRight,
                               near_bottomRight - near_topRight,
                               near_topRight,
                               PlaneOrientation.left_right);

        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(near_bottomRight, far_bottomRight, Color.black);
        Debug.DrawLine(near_topRight, far_topRight, Color.black);
        Debug.DrawLine(near_bottomRight, near_topRight, Color.black);
        Debug.DrawLine(far_bottomRight, far_topRight, Color.black);

        //Calculates the close plane of the camera's pyramid and stores it in the relevant variable
        closePlane.UpdatePlane(near_bottomRight - near_topRight,
                               near_topLeft - near_topRight,
                               near_bottomRight,
                               PlaneOrientation.close_far);

        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(near_topLeft, near_topRight, Color.black);
        Debug.DrawLine(near_bottomLeft, near_bottomRight, Color.black);
        Debug.DrawLine(near_topLeft, near_bottomLeft, Color.black);
        Debug.DrawLine(near_bottomRight, near_topRight, Color.black);
        
        //Calculates the far plane of the camera's pyramid and stores it in the relevant variable
        farPlane.UpdatePlane(far_bottomLeft - far_topLeft,
                             far_topRight - far_topLeft,
                             far_bottomLeft,
                             PlaneOrientation.close_far);
        
        //Debug code to draw the lines that cover the relevant area of the plane
        Debug.DrawLine(far_topLeft, far_topRight, Color.black);
        Debug.DrawLine(far_bottomLeft, far_bottomRight, Color.black);
        Debug.DrawLine(far_topLeft, far_bottomLeft, Color.black);
        Debug.DrawLine(far_bottomRight, far_topRight, Color.black);

        Shape.planes.Clear();
        Shape.planes.Add(topPlane);
        Shape.planes.Add(botPlane);
        Shape.planes.Add(leftPlane);
        Shape.planes.Add(rightPlane);
        Shape.planes.Add(closePlane);
        Shape.planes.Add(farPlane);

        //Returns all the calculated planes inside an array of the type Plane defined in Plane.cs 
        return new Plane[6] {topPlane, botPlane, leftPlane, rightPlane, closePlane, farPlane};
    }

    //Uses the planes parallel to the one who detected the shape to be partly in the camera area to test if it is truly in bounds or not 
    private bool CompareParallelPlanesTests(PlaneOrientation orientation, Shape shape)
    {
        //Iterate through each pair of parallel planes in the parallel plane dictionary based on the orientation of the plane who flagged the shape as partly in
        foreach(Plane[] planeArray in parallelPlanesDict[orientation])
        {
            //If one of the parallel planes considers the shape to be fully in and the other to be fully out (the plane is outside of the camera bounds)...
            if((planeArray[0].TestShape(shape, false) == IntersectionState.Fully &
                planeArray[1].TestShape(shape, false) == IntersectionState.None) |
               (planeArray[0].TestShape(shape, false) == IntersectionState.None &
                planeArray[1].TestShape(shape, false) == IntersectionState.Fully))
                {
                    //...indicate that the result was a false partly in
                    return false;
                }
        }
        //If no parallel plane pair considered the shape to be out of bounds, indicate that the result was a true partly in.
        return true;
    }

    //Draw Gizmos for debugging purpose
    private void OnDrawGizmos()
    {
        //If neither the collection of the close nor far vertices are null
        if(nearVertices != null && farVertices != null)
        {
            //Iterate through each vertex in the near vertices collection
            foreach(Vector3 vertex in nearVertices)
            {
                //Draw a small sphere at the position of each vertex
                Gizmos.DrawSphere(vertex, 0.01f);
            }
            //Iterate through each vertex in the far vertices collection
            foreach(Vector3 vertex in farVertices)
            {
                //Draw a small sphere at the position of each vertex
                Gizmos.DrawSphere(vertex, 50f);
            }
        }
    }
}