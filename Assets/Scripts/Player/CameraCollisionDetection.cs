using UnityEngine;

[System.Serializable]
public class CameraCollisionDetection
{
    public LayerMask collisionLayer; // objects that the camera can collide with

    [HideInInspector]
    public bool isColliding = false;

    [HideInInspector]
    public Vector3[] adjustedCameraClipPoints; //  the clip points that surround the camera current position 

    [HideInInspector]
    public Vector3[] desiredCameraClipPoints; // the clip points that surround the cameras expected position if it wasnt colliding with anything

    Camera camera;

    public void Initialise(Camera cam)
    {
        this.camera = cam;
        this.adjustedCameraClipPoints = new Vector3[5];
        this.desiredCameraClipPoints = new Vector3[5];
    }

    /**
     * Update camera clip points, do in update as camera is always moving. 
     */
    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion cameraRotation, ref Vector3[] intoArray)
    {
        if (!this.camera)
            return;
    
        // clear array (todo: dont need to do as setting anyways??)
        intoArray = new Vector3[5];

        float z = this.camera.nearClipPlane;
        float x = Mathf.Tan(this.camera.fieldOfView / 2.0f * Mathf.Deg2Rad) * z;
        float y = x / this.camera.aspect;

        // Find XYZ clip coords
        // Top left clip point
        intoArray[0] = (cameraRotation * new Vector3(-x, y, z)) + cameraPosition; // added and rotated the point relative to camera

        // Top right clip point
        intoArray[1] = (cameraRotation * new Vector3(x, y, z)) + cameraPosition;

        // Bottom left clip point
        intoArray[2] = (cameraRotation * new Vector3(-x, -y, z)) + cameraPosition;

        // Bottom right clip point
        intoArray[3] = (cameraRotation * new Vector3(x, -y, z)) + cameraPosition;

        // Camera's position
        const float safteyDistanceMultiplier = 0.75f; // so will detect collision sooner
        intoArray[4] = cameraPosition - this.camera.transform.forward * safteyDistanceMultiplier;
    }

    /**
     * Determines if an object has come inbetween any of the raycasts (from each clip point towards the player)
     * clipPoints: will only be using desiredCameraClipPoints but can do fancy things in future?
     * targetPosition: casting rays from target position to our clip points
     * */
    private bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 targetPosition)
    {
        for(int i = 0; i < clipPoints.Length; i++)
        {
            Vector3 dirTowardsClipPoint = clipPoints[i] - targetPosition;
            Ray ray = new Ray(targetPosition, dirTowardsClipPoint);
            float distance = Vector3.Distance(clipPoints[i], targetPosition);

            if(Physics.Raycast(ray, distance, this.collisionLayer))
                return true;
        }

        return false;
    }

    /**
     * Minimum distance our camera needs to move forward before stop running into an object (wall).
     * targetPosition: we want distance from our target
     * */
    //todo: Do I only need to use this no matter what in my function because it will be 0 or it will be the distance?
    public float GetAdjustedDistanceWithRayFrom(Vector3 targetPosition)
    {
        float distance = -1.0f;

        // Find shortest distance.
        for(int i = 0; i < this.desiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(targetPosition, this.desiredCameraClipPoints[i] - targetPosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                // Set as min distance if hasnt been set yet
                if (distance == -1.0f)
                    distance = hit.distance;
                // otherwise check if shortest distance
                else if (hit.distance < distance)
                    distance = hit.distance;
            }
        }

        return (distance == -1.0f) ? 0.0f : distance;
    }

    public void CheckColliding(Vector3 targetPosition)
    {
        this.isColliding = this.CollisionDetectedAtClipPoints(this.desiredCameraClipPoints, targetPosition);
    }
}
