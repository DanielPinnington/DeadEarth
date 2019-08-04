using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipBack
{
    public List<AudioClip> clips = new List<AudioClip>();
}

[CreateAssetMenu(fileName = "New Audio Collection")]
public class AudioCollection : ScriptableObject
{
    //Inspector assigned variables

    [SerializeField] string _audioGroup = string.Empty;
    [SerializeField] [Range(0.0f, 1.0f)] float _volume = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _spatialBlend = 1.0f;
    [SerializeField] [Range(0, 256)] int _priority = 128;
    [SerializeField] List<ClipBack> _audioClipBanks = new List<ClipBack>();

    public string audioGroup { get { return _audioGroup; } }
    public float volume { get { return _volume; } }
    public float spatialBlend { get { return _spatialBlend; } }
    public int priority { get { return _priority; } }
    public int bankCount { get { return _audioClipBanks.Count; } }

    public AudioClip this[int i]
    {
        get
        {
            if (_audioClipBanks == null || _audioClipBanks.Count <= i) return null;
            if (_audioClipBanks[i].clips.Count == 0) return null;

            List<AudioClip> clipList = _audioClipBanks[i].clips;
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];
            return clip;
        }
    }

    public AudioClip audioClip
    {
        get
        {
            if (_audioClipBanks == null || _audioClipBanks.Count == 0) return null;
            if (_audioClipBanks[0].clips.Count == 0) return null;

            List<AudioClip> clipList = _audioClipBanks[0].clips;
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];
            return clip;
        }
    }

}
