using UnityEngine;

public class WorldSpaceConnector : MonoBehaviour
{
    public LineRenderer line;
    public Transform hotspot;    // Your white dot
    public Transform infoPanel;  // The UI Panel (or a pivot point on it)

    void Update()
    {
        if (line != null && hotspot != null && infoPanel != null)
        {
            // Set the number of points to 2
            line.positionCount = 2;

            // Point 0 is the hotspot
            line.SetPosition(0, hotspot.position);

            // Point 1 is the panel
            line.SetPosition(1, infoPanel.position);

            // Optional: Adjust thickness based on distance or scale
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
        }
    }
}
