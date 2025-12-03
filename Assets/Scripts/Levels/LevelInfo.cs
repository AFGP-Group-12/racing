using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public string musicPath;

    void Start()
    {
        if (!string.IsNullOrEmpty(musicPath) && MusicHandler.Instance != null)
        {
            MusicHandler.Instance.Play(musicPath);
        }
    }
}