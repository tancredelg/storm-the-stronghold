[System.Serializable]
public class SettingsData
{
    private static SettingsData _instance;
    public static SettingsData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SettingsData();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    public float MusicVolume;
    public float SFXVolume;
    public int QualityLevel;

    public override string ToString()
    {
        return $"[SettingsData]\nMusicVolume: {MusicVolume}\nSFXVolume: {SFXVolume}\nGraphicsLevel: {QualityLevel}";
    }
}