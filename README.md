# 1. The Effect
Analyzing the effect by playing the game, I have arrived at a few conclusions regarding how it works at a logical level.

I will describe the effect in a more detailed manner in the next section. It is divided into 3 phases: the capture phase, the destruction phase and the manifestation phase.

Please refer to the following 2D depiction of the logic behind the effect throughout the explanation:

![first_example](./report_images/first_example.png)

## 1.1. Capture Phase:
The player enters the camera mode and takes a snapshot of the world. This snapshot is of a square/rectangular shape and is the frustum of a pyramid based around the player’s camera.

Everything within this pyramid’s volume will be recorded and stored so that it can be instantiated during the manifestation phase. The base of the pyramid opposite to the player is then projected forward until it connects with the skybox. It then stores that section of the texture as well.

All of these textures and objects have a filter or shader applied to them so that when they are instantiated, they are distinguishable from all the objects originally contained in the scene.

The player is then provided with a photograph containing the texture of the taken snapshot 
and can be used to trigger the next phases of the effect.

![second_example](./report_images/second_example.png)

## 1.2. Destruction Phase:
With the picture provided, the player can then aim and rotate this picture around as seen below.

![third_example](./report_images/third_example.png)

Once they have decided on a position and rotation, they may click on the button to trigger the effect which will cause the first of the final two phases of the effect to begin.

Depending on the position and rotation of the picture, a pyramid with the same dimensions as the one used in the previous phase is projected from the frustum onwards.

Afterwards, everything within this pyramid’s volume will be deleted. This effect can be observed in the following picture where the picture did not contain any elements besides the skybox.

As such, the pyramid’s volume in empty and we can observe the effects of only the destruction phase:

![fourth_example](./report_images/fourth_example.png)

Once this phase has been completed and before the game can render the next frame, the final phase will occur.

## 1.3. Manifestation Phase:
Once the pyramid has been projected onto the scene, its volume is cleared of the existing contents in the scene.

When this has been completed, the previously saved contents during the first phase of this effect are instantiated into the scene, retaining their original position relative to the pyramid itself in order to account for their new positions and rotations in the world.

This can be observed in the following image where I have manifested the pyramid’s contents into the air so we can observe them without intersecting the elements already in the scene:

![fifth_example](./report_images/fifth_example.png)

Afterwards, the other end of the pyramid, its base is once again projected onwards until it reaches the skybox and that section of it is replaced with previously saved section of the original skybox as seen below:

![sixth_example](./report_images/sixth_example.png)

Only after this phase has concluded is the game allowed to render the scene so that the effect is seamless and complete.

--

# 2. The Implementation

This section will detail the ideas behind my version of this effect’s implementation. Although there were some issues to be detailed in this section which prevented me from completing the task, I will be detailing the theory and my ideas on the implementation whether they were successfully achieved or not.

## 2.1. Capture Phase
In order for the effect to work, I would need to know the relationship between each object in the scene and my camera area. As such, I would need to calculate the bounds of said area.

I started this by obtaining the vertices of the 6 sided polyhedra which makes up the camera area.

![vertices_diagram](./report_images/vertices_diagram.png)

The close vertices were obtained utilizing the ViewportToWorldPoint method from Unity’s camera class.

This method receives a Vector3 where the values (x,y,z) represent the following: x is a value in the x axis of the position which ranges from 0 to 1 where 0 references the leftmost edge of the viewport and 1 references the rightmost; y represents the same thing except for the y axis and z references the position in world units on the z axis from the camera.

Considering that we know the z distance we need is the camera’s nearClipPlane property, we can represent the vertices on the near end of the camera as follows:

- top_left_close: camera.ViewportToWorldPoint(new Vector3(0, 1, camera.nearClipPlane)

- top_right_close: camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane)

- bot_left_close: camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane)

- bot_right_close: camera.ViewportToWorldPoint(new Vector3(1, 0, camera.nearClipPlane)

Now that we have these vertices, we need to calculate the ones at the end of the camera area.

The easiest way to proceed with this would be to utilize this method again with the camera’s farClipPlane property, however, utilizing information provided by the teacher, I decided to try another method as it sounded very useful to learn how to utilize it.

Each point in Unity has its position represented as a Vector3(x,y,z), however, they also contain a sort of rotation inside them which points them in a certain direction. As such, the vertices we have calculated for the near plane of the camera point in the direction of the vertices on the far plane.

Utilizing this, we can reach these far vertices by transforming the position of the close ones by specific values on each of their components. The best thing about this method is that these values are constant so the way to calculate these points is always the same, independent of the changes which may have occurred to the close vertices.

Keeping this in mind, we were able to calculate the far plane vertices utilizing the values which will be referred to as xDistance, yDistance and zDistance.

Assuming that we are viewing the camera area on the xz plane we can visualize the xDistance to be as follows:

![top_view_breakdown](./report_images/top_view_breakdown.png)

Considering the rules of geometry, the tangent of the angle which we know to be the half the horizontal FOV angle will be equal to the xDistance divided by the View Distance.

As such, we can deduce the xDistance as follows:

tan(horizontalFOV2) = xDistanceViewDistance <=> xDistance = tan(horizontalFOV2) ViewDistance

This equation also allows us to calculate the yDistance by replacing the value of the horizontal FOV by the value of the vertical FOV.

As such, we were able to calculate these values as follows:

- zDistance = cam.farClipPlane - cam.nearClipPlane;

- yDistance = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad /2) * zDistance;

- xDistance = Mathf.Tan(Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect) * Mathf.Deg2Rad /2) * zDistance;

It is important to note that the Mathf.Tan function receives the degrees in radians while the fieldOfView properties provided by the camera are in degrees. As such, it was necessary to multiply them by Mathf.Deg2Rad to ensure that the equation functioned properly.
Now that we have these values, we were able to calculate the vertices on the far end of the camera area by transforming the vertices on the near end of it utilizing their previously referred orientation.

- far_topLeft = near_topLeft + (-xDistance * transform.right)
                             + (yDistance * transform.up)
                             + (zDistance * transform.forward);

- far_topRight = near_topRight + (xDistance * transform.right)
                               + (yDistance * transform.up)
                               + (zDistance * transform.forward);

- far_bottomRight = near_bottomRight + (xDistance * transform.right)
                                     + (-yDistance * transform.up)
                                     + (zDistance * transform.forward);

- far_bottomLeft = near_bottomLeft + (-xDistance * transform.right)
                                   + (-yDistance * transform.up)
                                   + (zDistance * transform.forward);

The values were utilized in their positive and negative versions in order to achieve the vertices we wanted as some were left (negative xDistance) or right (positive xDistance) of the original vertices as well as up (positive yDistance) or down (negative yDistance). 

Now that we have all the vertices of the camera area, I tried to write a simple checking of the coordinate values to see whether they were inside the camera area or not, however, as the planes horizontal camera planes were not parallel to the x, y or z axis, this did not prove to be as easy since for different values of z or y, the same value of x could be inside or outside the camera area and vice versa.

![same_values](./report_images/same_values.png)

However, I learned that utilizing the plane’s equation, I would be able to compare any point to it and know where it was by comparison.

Firstly, it was necessary to define the plane according to the following equation:

(a  x) + (b  y) + (c  z) + d  = 0 in which the normal vector of the plane is (a,b,c), the point (x,y,z) belongs to the plane and d is a real number.

To calculate this, we utilize two vectors contained within the plane as the cross product between them is the normal of the plane. At first, I calculated this using two vectors by random planes however, this did not yield the desired results.

I later learned that the order of the vectors for the cross product determines the direction of the normal according to the right hand rule as pictured below:

![right_hand_rule](./report_images/right_hand_rule.png)

--