using UnityEngine;

public class DynamicGroundPlane : MonoBehaviour
{
    public GameObject groundPlane; // Assign the Plane in the inspector

    void Start()
    {
        AdjustGroundPlaneSize();
    }

    void AdjustGroundPlaneSize()
    {
        if (groundPlane == null)
        {
            Debug.LogError("Ground Plane is not assigned!");
            return;
        }

        // Get the screen dimensions
        float screenWidth = Camera.main.pixelWidth;
        float screenHeight = Camera.main.pixelHeight;

        // Convert screen dimensions to world units
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, Camera.main.nearClipPlane));

        float widthInWorldUnits = screenTopRight.x - screenBottomLeft.x + 20;
        float heightInWorldUnits = screenTopRight.z - screenBottomLeft.z + 20;

        // Adjust the ground plane size
        groundPlane.transform.localScale = new Vector3(widthInWorldUnits / 10, 1, heightInWorldUnits / 10); // Plane's default size is 10x10 units
    }
}