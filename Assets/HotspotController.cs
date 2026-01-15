using UnityEngine;

public class HotspotController : MonoBehaviour
{
    public GameObject infoPanel;

    public void Toggle()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }
}
