using UnityEngine;
using UnityEngine.SceneManagement;

// Bridge target for React Native. The app sends:
//   postMessage("SceneLoader", "LoadScene", sceneId)
// so this component must live on a GameObject named exactly "SceneLoader".
// Place it once in the first scene in Build Settings (SAS-BaliPanjang);
// it survives every scene change via DontDestroyOnLoad.
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float userActivityReportInterval = 0.75f;

    private static SceneLoader instance;
    private float lastUserActivityReportTime = float.NegativeInfinity;

    private void Awake()
    {
        // Scene 0 contains this object, so guard against duplicates if it
        // ever gets reloaded.
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SendToRN("ready");
    }

    private void Update()
    {
        if (!HasUserInput()) return;

        if (Time.unscaledTime - lastUserActivityReportTime < userActivityReportInterval)
        {
            return;
        }

        lastUserActivityReportTime = Time.unscaledTime;
        SendToRN("user_activity");
    }

    // Called from React Native via UnitySendMessage.
    public void LoadScene(string sceneId)
    {
        Debug.Log($"[SceneLoader] Loading scene: {sceneId}");
        if (SceneManager.GetActiveScene().name == sceneId) return;
        SceneManager.LoadSceneAsync(sceneId);
    }

    private static bool HasUserInput()
    {
#if UNITY_EDITOR
        return Input.GetMouseButton(0) ||
               Input.GetMouseButton(1) ||
               Input.anyKeyDown ||
               Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0.001f;
#else
        return Input.touchCount > 0 || Input.anyKeyDown;
#endif
    }

    private static void SendToRN(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var proxy = new AndroidJavaClass("com.expounity.bridge.NativeCallProxy"))
        {
            proxy.CallStatic("sendMessageToMobileApp", message);
        }
#else
        Debug.Log($"[SceneLoader] (editor) would send to RN: {message}");
#endif
    }
}
