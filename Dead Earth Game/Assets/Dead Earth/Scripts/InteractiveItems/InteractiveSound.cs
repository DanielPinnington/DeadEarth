using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveSound : InteractiveItem
{
    //Inspector assigned
    [TextArea(3, 10)]
    [SerializeField] private string _infoText              = null;
    [TextArea(3, 10)]
    [SerializeField] private string _activatedText         = null;
    [SerializeField] private float  _activatedTextDuration = 3.0f;
    [SerializeField] private AudioCollection _audioCollection = null;
    [SerializeField] private int _bank = 0;

    //Private fields
    private IEnumerator _coroutine = null;
    private float _hideActivatedTextTime = 0.0f;


    public override string GetText()
    {
        if (_coroutine != null || Time.time < _hideActivatedTextTime)
            return _activatedText;
        else
            return _infoText;
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (_coroutine == null)
        {
            _hideActivatedTextTime = Time.time + _activatedTextDuration;
            _coroutine = DoActivation();
            StartCoroutine(_coroutine);
        }
    }

    private IEnumerator DoActivation()
    {
        //We need a valid collection and audio manager
        if (_audioCollection != null || AudioManager.instance == null) yield break;

        //Fetch clip from colleciton
        AudioClip clip = _audioCollection[_bank];
        if (clip == null) yield break;

        //Play it as one shot sound
        AudioManager.instance.PlayOneShotSound(_audioCollection.audioGroup, clip, transform.position, _audioCollection.volume, _audioCollection.spatialBlend, _audioCollection.priority);

        //Run while clip is playing
        yield return new WaitForSeconds(clip.length);

        //Unblock coroutine instantiation
        _coroutine = null;
    }
}
