using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bridge target for React Native. The app sends:
//   postMessage("SceneLoader", "LoadWeapon", JSON with sceneId and requestId)
// so this component must live on a GameObject named exactly "SceneLoader".
// Place it once in the first scene in Build Settings (SAS-BaliPanjang);
// it survives every scene change via DontDestroyOnLoad.
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float userActivityReportInterval = 0.75f;

    private static SceneLoader instance;
    private float lastUserActivityReportTime = float.NegativeInfinity;
    private WeaponRequest pendingRequest;
    private WeaponRequest latestRequest;
    private bool isLoading;

    [Serializable]
    private class WeaponRequest
    {
        public string sceneId;
        public string requestId;
    }

    [Serializable]
    private class WeaponMessage
    {
        public string type;
        public string sceneId;
        public string requestId;
        public string error;
    }

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

    // Keep the original entry point for callers using plain scene names.
    public void LoadScene(string sceneId)
    {
        QueueRequest(new WeaponRequest { sceneId = sceneId, requestId = "" });
    }

    // Called from React Native via UnitySendMessage.
    public void LoadWeapon(string payload)
    {
        WeaponRequest request;
        try
        {
            request = JsonUtility.FromJson<WeaponRequest>(payload);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SceneLoader] Invalid weapon request: {exception.Message}");
            return;
        }

        if (request == null || string.IsNullOrEmpty(request.requestId))
        {
            Debug.LogError("[SceneLoader] Weapon request is missing a request ID.");
            return;
        }

        QueueRequest(request);
    }

    private void QueueRequest(WeaponRequest request)
    {
        latestRequest = request;
        // An in-flight Unity scene load cannot be cancelled. Finish it, then
        // load only the newest pending selection without reporting stale ready.
        pendingRequest = request;
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(LoadPendingWeapons());
    }

    private IEnumerator LoadPendingWeapons()
    {
        while (pendingRequest != null)
        {
            WeaponRequest request = pendingRequest;
            pendingRequest = null;
            AsyncOperation operation = null;
            string error = null;

            try
            {
                if (string.IsNullOrEmpty(request.sceneId) ||
                    !Application.CanStreamedLevelBeLoaded(request.sceneId))
                {
                    error = $"Scene '{request.sceneId}' is not available in the build.";
                }
                else if (SceneManager.GetActiveScene().name != request.sceneId)
                {
                    Debug.Log($"[SceneLoader] Loading scene: {request.sceneId}");
                    operation = SceneManager.LoadSceneAsync(request.sceneId);
                    if (operation == null) error = "Unity could not start the scene load.";
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }

            if (error != null)
            {
                if (latestRequest == request) SendWeaponMessage("weapon_error", request, error);
                continue;
            }

            if (operation != null) yield return operation;
            if (latestRequest != request) continue;

            // Let Start() initialize the camera and interaction scripts, then
            // let a rendered frame reach the display before revealing Unity.
            // This also acknowledges a weapon whose scene is already active.
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return null;
            if (latestRequest != request) continue;

            if (SceneManager.GetActiveScene().name == request.sceneId)
            {
                SendWeaponMessage("weapon_ready", request);
            }
            else
            {
                SendWeaponMessage("weapon_error", request, "The requested scene did not become active.");
            }
        }

        isLoading = false;
    }

    private static void SendWeaponMessage(string type, WeaponRequest request, string error = null)
    {
        SendToRN(JsonUtility.ToJson(new WeaponMessage
        {
            type = type,
            sceneId = request.sceneId,
            requestId = request.requestId,
            error = error
        }));
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
