using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections;
using NaughtyAttributes;

// RN bridge: postMessage("LocalizationManager", "ChangeLanguage", localeCode)
public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instance;

    private bool isChanging = false;

    private void Awake()
    {
        // Only the dedicated bridge object persists; per-scene KerisHolder instances stay local.
        if (gameObject.name != "LocalizationManager") return;

        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Changes the game language to the specified locale code (e.g., "en", "ms").
    /// Called from React Native via UnitySendMessage.
    /// </summary>
    public void ChangeLanguage(string localeCode)
    {
        // Prevent spam clicking while a change is already in progress
        if (isChanging) return;

        StartCoroutine(SetLocaleRoutine(localeCode));
    }

    /// <summary>
    /// Toggles the language between English (en) and Malay (my).
    /// </summary>
    [Button]
    public void ToggleLanguage()
    {
        if (isChanging) return;

        // Check current language and swap to the other
        string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        string targetCode = (currentCode == "en") ? "ms" : "en";

        StartCoroutine(SetLocaleRoutine(targetCode));
    }

    private IEnumerator SetLocaleRoutine(string localeCode)
    {
        isChanging = true;

        // Wait for the localization system to fully initialize (crucial to prevent null refs)
        yield return LocalizationSettings.InitializationOperation;

        // Look for the requested locale in your project's available locales
        Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"Language successfully changed to: {localeCode}");
        }
        else
        {
            Debug.LogWarning($"Locale '{localeCode}' not found in Localization Settings!");
        }

        isChanging = false;
    }

    [Button]
    public void SetLocaleToBM()
    {
        ChangeLanguage("ms");
    }

    [Button]
    public void SetLocaleToEn()
    {
        ChangeLanguage("en");
    }
}
