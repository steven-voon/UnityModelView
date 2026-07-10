using UnityEngine;
using UnityEngine.SceneManagement;

// Bridge target for React Native. The app sends:
//   postMessage("SceneLoader", "LoadScene", sceneId)
// so this component must live on a GameObject named exactly "SceneLoader".
// Place it once in the first scene in Build Settings (SAS-BaliPanjang);
// it survives every scene change via DontDestroyOnLoad.
public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

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

    // Called from React Native via UnitySendMessage.
    public void LoadScene(string sceneId)
    {
        Debug.Log($"[SceneLoader] Loading scene: {sceneId}");
        if (SceneManager.GetActiveScene().name == sceneId) return;
        SceneManager.LoadSceneAsync(sceneId);
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
