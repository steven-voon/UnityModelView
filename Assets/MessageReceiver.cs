using UnityEngine;

public class MessageReceiver : MonoBehaviour
{
    public GameObject kerisAGO;
    public GameObject kerisBGO;


    public GameObject[] numberingGO;
    private bool isNumberShowing = true;

    // This name MUST match the second argument in postMessage
    public void ReceiveMessageFromRN(string message)
    {
        if (message.CompareTo("ToggleHotspot") == 0)
        {
            ToggleNumbering();
        }
        else if (message.CompareTo("ShowKerisA") == 0)
        {
            ShowKerisA();
        }
        else if (message.CompareTo("ShowKerisB") == 0)
        {
            ShowKerisB();
        }
    }

    private void ToggleNumbering()
    {
        isNumberShowing = !isNumberShowing;
        for (int i = 0; i < numberingGO.Length; i++)
        {
            numberingGO[i].SetActive(isNumberShowing);
        }
    }

    private void ShowKerisA()
    {
        kerisAGO.SetActive(true);
        kerisBGO.SetActive(false);
    }

    private void ShowKerisB()
    {
        kerisAGO.SetActive(false);
        kerisBGO.SetActive(true);
    }

}
