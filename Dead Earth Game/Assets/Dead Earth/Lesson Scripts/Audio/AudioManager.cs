using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;


// -----------------------------------------------------------------------------------------
// CLASS	:	TrackInfo
// DESC		:	Wraps an AudioMixerGroup in Unity's AudioMixer. Contains the name of the
//				group (which is also its exposed volume paramater), the group itself
//				and an IEnumerator for doing track fades over time.
// -----------------------------------------------------------------------------------------
public class TrackInfo
{
	public string 			Name		=	string.Empty;
	public AudioMixerGroup	Group		=	null;
	public IEnumerator		TrackFader	=	null;
}

// ----------------------------------------------------------------------------------------
// CLASS	:	AudioManager
// DESC		: 	Provides pooled one-shot functionality with priority system and also
//				wraps the Unity Audio Mixer to make easier manipulation of audiogroup
//				volumes 
// ---------------------------------------------------------------------------------------- 
public class AudioManager : MonoBehaviour 
{
	// Statics
	private static AudioManager	_instance	=	null;
	public static AudioManager	instance
	{
		get
		{
			if (_instance==null)
				_instance = (AudioManager)FindObjectOfType( typeof(AudioManager));
			return _instance;
		}
	}


	// Inspector Assigned Variables
	[SerializeField] AudioMixer 	_mixer			=	null;


	// Private Variables
	Dictionary<string, TrackInfo> 		_tracks 		= new Dictionary<string, TrackInfo>();



	void Awake()
	{
		// This object must live for the entire application
		DontDestroyOnLoad(gameObject);

		// Return if we have no valid mixer reference
		if (!_mixer) return;

		// Fetch all the groups in the mixer - These will be our mixers tracks
		AudioMixerGroup[] groups = _mixer.FindMatchingGroups(string.Empty);

		// Create our mixer tracks based on group name (Track -> AudioGroup)
		foreach(AudioMixerGroup group in groups)
		{
			TrackInfo trackInfo = new TrackInfo();
			trackInfo.Name 			= group.name;
			trackInfo.Group			= group;
			trackInfo.TrackFader	=	null;
			_tracks[group.name] = trackInfo;
		}


	}


	void Update()
	{

	}

	// ------------------------------------------------------------------------------
	// Name	:	GetTrackVolume
	// Desc	:	Returns the volume of the AudioMixerGroup assign to the passed track.
	//			AudioMixerGroup MUST expose its volume variable to script for this to
	//			work and the variable MUST be the same as the name of the group
	// ------------------------------------------------------------------------------
	public float GetTrackVolume( string track )
	{
		TrackInfo trackInfo;
		if (_tracks.TryGetValue( track, out trackInfo ))
		{
			float volume;
			 _mixer.GetFloat( track, out volume );
			 return volume;
		}

		return float.MinValue;
	}

	public AudioMixerGroup GetAudioGroupFromTrackName(string name)
	{
		TrackInfo ti;
		if (_tracks.TryGetValue( name, out ti ))
		{
			return ti.Group;
		}

		return null;
	}

	// ------------------------------------------------------------------------------
	// Name	:	SetTrackVolume
	// Desc	:	Sets the volume of the AudioMixerGroup assigned to the passed track.
	//			AudioMixerGroup MUST expose its volume variable to script for this to
	//			work and the variable MUST be the same as the name of the group
	//			If a fade time is given a coroutine will be used to perform the fade
	// ------------------------------------------------------------------------------
	public void SetTrackVolume( string track, float volume, float fadeTime = 0.0f )
	{
		if (!_mixer) return;
		TrackInfo trackInfo;
		if (_tracks.TryGetValue( track, out trackInfo ))
		{
			// Stop any coroutine that might be in the middle of fading this track
			if (trackInfo.TrackFader!=null) StopCoroutine( trackInfo.TrackFader );

			if (fadeTime==0.0f)
				_mixer.SetFloat(track, volume );
			else 	
			{
				trackInfo.TrackFader = SetTrackVolumeInternal( track, volume, fadeTime );
				StartCoroutine( trackInfo.TrackFader );
			}
		}			

	}

	// -------------------------------------------------------------------------------
	// Name	:	SetTrackVolumeInternal - COROUTINE
	// Desc	:	Used by SetTrackVolume to implement a fade between volumes of a track
	//			over time.
	// -------------------------------------------------------------------------------
	protected IEnumerator SetTrackVolumeInternal( string track, float volume, float fadeTime )
	{
		float startVolume = 0.0f;
		float timer 	  = 0.0f;
		 _mixer.GetFloat( track, out startVolume );

		 while (timer<fadeTime)
		 {
		 	timer+=Time.unscaledDeltaTime;
			_mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer/fadeTime) );
			yield return null;
		 }

		_mixer.SetFloat(track, volume );
	}


}
