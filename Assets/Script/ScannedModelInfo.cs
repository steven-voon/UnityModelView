using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ScannedModelInfo", menuName = "ScriptableObjects/ScannedModelInfo")]
public class ScannedModelInfo : ScriptableObject
{
    public Info[] modelInfo;
}
[System.Serializable]
public class Info
{
    public string infoTitle;
    [TextArea]
    public string infoDescription;
}
