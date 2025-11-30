using UnityEngine;

public class LanguageSetting : MonoBehaviour
{
    public enum Languages
    {
        Brazilian,
        English
    }
    public static LanguageSetting Instance {get; private set;}
    private Languages _languageOfChoice;
    public Languages LanguageOfChoice => _languageOfChoice;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetLanguage(string language)
    {
        if (string.Equals(language, "BR"))
        {
            _languageOfChoice = Languages.Brazilian;
            return;
        }

        if (string.Equals(language, "EN"))
        {
            _languageOfChoice = Languages.English;
            return;
        }
    }
}
