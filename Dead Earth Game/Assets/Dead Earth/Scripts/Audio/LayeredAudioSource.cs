using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLayer
{
	public AudioClip			Clip		=	null;
	public AudioCollection		Collection	=	null;
	public int					Bank		=	0;
	public bool					Looping		=	true;
	public float				Time		=	0.0f;
	public float				Duration	=	0.0f;
	public bool					Muted		=	false;
}

public interface ILayeredAudioSource
{
	bool Play ( AudioCollection pool, int bank, int layer, bool looping = true );
	void Stop ( int layerIndex );
	void Mute (int layerIndex, bool mute);
	void Mute ( bool mute );
}


public class LayeredAudioSource : ILayeredAudioSource 
{
	// Public Properties
	public AudioSource audioSource{ get{ return _audioSource;}}

	// Private
	AudioSource			_audioSource	=	null;
	List<AudioLayer>	_audioLayers	=	new List<AudioLayer>();
	int 				_activeLayer	=	-1;
	// ----------------------------------------------------------------------------
	// Name	:	Contructor
	// Desc	:	Allocates the layer stack
	// ----------------------------------------------------------------------------
	public LayeredAudioSource( AudioSource source, int layers )
	{
		if (source!=null && layers>0)
		{
			// Assign audio source to this layer stack
			_audioSource = source;

			// Create the requested number of layers
			for(int i=0; i<layers; i++)
			{
				// Create new layer
				AudioLayer newLayer = new AudioLayer();
				newLayer.Collection	=	null;
				newLayer.Duration	=	0.0f;
				newLayer.Time		=	0.0f;
				newLayer.Looping	=	false;
				newLayer.Bank		=	0;
				newLayer.Muted		=	false;
				newLayer.Clip		=	null;

				// Add Layer to stack
				_audioLayers.Add( newLayer );
			}		
		}
	}

	// ------------------------------------------------------------------------
	// Name	:	Play
	// Desc	:	
	// ------------------------------------------------------------------------
	public bool Play ( AudioCollection collection, int bank, int layer, bool looping = true )
	{	
		/// Layer must be in range
		if (layer>=_audioLayers.Count) return false;

		// Fetch the layer we wish to configure
		AudioLayer audioLayer = _audioLayers[layer];


		// Already doing what we want then just return true
		if (audioLayer.Collection == collection && audioLayer.Looping == looping && bank==audioLayer.Bank ) return true;

		audioLayer.Collection 	= 	collection;
		audioLayer.Bank			=	bank;
		audioLayer.Looping 		= 	looping;
		audioLayer.Time			=	0.0f;
		audioLayer.Duration		=	0.0f;
		audioLayer.Muted		=	false;
		audioLayer.Clip		=	null;	

		return true;
	}

	public void Stop ( int layerIndex )
	{
		if (layerIndex>= _audioLayers.Count) return;
		AudioLayer layer = _audioLayers[layerIndex];
		if (layer!=null)
		{
			layer.Looping = false;
			layer.Time = layer.Duration;
		}

	}


	public void Mute (int layerIndex, bool mute)
	{
		if (layerIndex>= _audioLayers.Count) return;
		AudioLayer layer = _audioLayers[layerIndex];
		if (layer!=null)
		{
			layer.Muted = mute;
		}
	}

	public void Mute ( bool mute )
	{
		for (int i=0; i<_audioLayers.Count; i++)
		{
			Mute( i, mute );
		}
	}

	// --------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Updates the time of all layered clips and makes sure that the audio source
	//			is playing the clip on the highest layer.
	// --------------------------------------------------------------------------------------
	public void Update(  )
	{
		// Used to record the highest layer with a clip assigned and still playing
		int	 newActiveLayer	=	-1;
		bool refreshAudioSource = false;	

		// Update the stack each frame by iterating the layers (Working backwards)
		for(int i=_audioLayers.Count-1; i>=0; i-- )
		{
			// Layer being processed
			AudioLayer layer = _audioLayers[i];

			// Ignore unassigned layers
			if (layer.Collection==null) continue;

			// Update the internal playhead of the layer		
			layer.Time+=Time.deltaTime;

			// If it has exceeded its duration then we need to take action
			if (layer.Time>layer.Duration)
			{
				// If its a looping sound OR the first time we have set up this layer
				// we need to assign a new clip from the pool assigned to this layer
				if (layer.Looping || layer.Clip==null)
				{
					
					// Fetch a new clip from the pool
					AudioClip clip 	= layer.Collection[layer.Bank];

					// Calculate the play position based on the time of the layer and store duration
					if (clip==layer.Clip)
						layer.Time = layer.Time%layer.Clip.length;
					else
						layer.Time		= 0.0f;

					layer.Duration	= clip.length;
					layer.Clip=clip;

					// This is a layer that has focus so we need to chose and play
					// a new clip from the pool
					if (newActiveLayer<i)
					{
						// This is the active layer index
						newActiveLayer = i;
						// We need to issue a play command to the audio source
						refreshAudioSource=	true;
					}
				}
				else
				{
					// The clip assigned to this layer has finished playing and is not set to loop
					// so clear the later and reset its status ready for reuse in the future
					layer.Clip	=	null;
					layer.Collection = null;
					layer.Duration = 0.0f;
					layer.Bank = 0;
					layer.Looping = false;
					layer.Time = 0.0f;
				}	
			}
			// Else this layer is playing
			else
			{
				// If this is the highest layer then record that....its the clip currently playing
				if (newActiveLayer<i) newActiveLayer = i;
			}
		}

		// If we found a new active layer (or none)
		if (newActiveLayer!=_activeLayer || refreshAudioSource)
		{
			// Previous layer expired and no new layer so stop audio source - there are no active layers
			if (newActiveLayer==-1)
			{
				_audioSource.Stop();
				_audioSource.clip = null;
			}
			// We found an active layer but its different than the previous update so its time to switch
			// the audio source to play the clip on the new layer
			else
			{
				// Get the layer
				AudioLayer layer = _audioLayers[newActiveLayer];

				_audioSource.clip 			= layer.Clip;
				_audioSource.volume 		= layer.Muted?0.0f:layer.Collection.volume;
				_audioSource.spatialBlend	= layer.Collection.spatialBlend;
				_audioSource.time			= layer.Time;
				_audioSource.loop			= false;
				_audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName( layer.Collection.audioGroup);
				_audioSource.Play();
			}
		}

		// Remember the currently active layer for the next update check
		_activeLayer = newActiveLayer;

		if (_activeLayer!=-1 && _audioSource)
		{
			AudioLayer audioLayer = _audioLayers[_activeLayer]; 
			if (audioLayer.Muted) _audioSource.volume = 0.0f;
			else 				  _audioSource.volume = audioLayer.Collection.volume;
		}
	}
}
