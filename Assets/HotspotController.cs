using UnityEngine;

public class HotspotController : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject closeInstruction;
    public GameObject leadingLine;

    public void Toggle()
    {
        closeInstruction.SetActive(!closeInstruction.activeSelf);
        infoPanel.SetActive(!infoPanel.activeSelf);
        leadingLine.SetActive(!leadingLine.activeSelf);
    }

    public void Close()
    {
        closeInstruction.SetActive(false);
        infoPanel.SetActive(false);
        leadingLine.SetActive(false);
    }
}
