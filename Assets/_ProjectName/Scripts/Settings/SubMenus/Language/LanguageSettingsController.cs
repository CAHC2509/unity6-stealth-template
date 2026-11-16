using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSettingsController : SettingsSubMenuControllerBase<LanguageSettingsView>
{
    [SerializeField] private LanguageSettingsData languageSettingsData;

    private readonly List<Locale> availableLocales = new();
    private int defaultLanguageIndex;
    private CoroutineHandle localizationReadyCoroutine;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        languageSettingsData = new LanguageSettingsData();

        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            OnLocalizationReady();
            return;
        }

        localizationReadyCoroutine = Timing.RunCoroutine(WaitForLocalization().CancelWith(gameObject));
    }

    public override void Conclude()
    {
        if (localizationReadyCoroutine.IsRunning)
            Timing.KillCoroutines(localizationReadyCoroutine);

        base.Conclude();
    }

    public override void SaveSettings()
    {
        SaveSystem.Save(Constants.LANGUAGE_SETTINGS_SAVE_KEY, languageSettingsData);
    }

    public override void LoadSettings()
    {
        if (!SaveSystem.TryLoad(Constants.LANGUAGE_SETTINGS_SAVE_KEY, out languageSettingsData))
        {
            languageSettingsData = new LanguageSettingsData();
            languageSettingsData.LanguageIndex = defaultLanguageIndex;
        }
    }

    public override void RestoreDefaultSettings()
    {
        languageSettingsData = new LanguageSettingsData();
        languageSettingsData.LanguageIndex = defaultLanguageIndex;

        ApplySettings();
        View.SetData(languageSettingsData);

        SaveSettings();
    }

    public void UpdateLanguage(int index)
    {
        languageSettingsData.LanguageIndex = index;

        ApplyLanguage();
    }

    private IEnumerator<float> WaitForLocalization()
    {
        while (!LocalizationSettings.InitializationOperation.IsDone)
            yield return Timing.WaitForOneFrame;

        OnLocalizationReady();
    }

    private void OnLocalizationReady()
    {
        SetupLocales();
        LoadSettings();
        ApplySettings();

        View.SetData(languageSettingsData);
    }

    private void SetupLocales()
    {
        availableLocales.Clear();
        defaultLanguageIndex = 0;

        List<string> options = new();

        if (LocalizationSettings.AvailableLocales == null || LocalizationSettings.AvailableLocales.Locales == null)
        {
            View.SetLanguageOptions(options);
            return;
        }

        List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;

        for (int i = 0; i < locales.Count; i++)
        {
            Locale locale = locales[i];

            availableLocales.Add(locale);

            string nativeName = locale.Identifier.CultureInfo.NativeName;
            nativeName = char.ToUpper(nativeName[0]) + nativeName[1..];
            options.Add(nativeName);
        }

        defaultLanguageIndex = GetDefaultLanguageIndex();
        View.SetLanguageOptions(options);
    }

    private void ApplySettings()
    {
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        if (languageSettingsData.LanguageIndex < 0 || languageSettingsData.LanguageIndex >= availableLocales.Count)
            return;

        LocalizationSettings.SelectedLocale = availableLocales[languageSettingsData.LanguageIndex];
    }

    private int GetDefaultLanguageIndex()
    {
        if (LocalizationSettings.AvailableLocales == null)
            return 0;

        Locale systemLocale = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);

        if (systemLocale == null)
            return 0;

        for (int i = 0; i < availableLocales.Count; i++)
        {
            if (availableLocales[i].Identifier == systemLocale.Identifier)
                return i;
        }

        return 0;
    }
}
