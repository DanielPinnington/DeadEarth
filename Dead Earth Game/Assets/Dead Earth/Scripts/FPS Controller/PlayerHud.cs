using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScreenFadeType { FadeIn, FadeOut }

public class PlayerHud : MonoBehaviour
{
    // Inspector Assigned UI References
    [SerializeField] private GameObject _crosshair = null;
    [SerializeField] private Text _healthText = null;
    [SerializeField] private Text _staminaText = null;
    [SerializeField] private Text _interactionText = null;
    [SerializeField] private Image _screenFade = null;
    [SerializeField] private Text _missionText = null;
    [SerializeField] private float _missionTextDisplayTime = 3.0f;

    // Internals
    float _currentFadeLevel = 1.0f;
    IEnumerator _coroutine = null;

    public void Start()
    {
        if (_screenFade)
        {
            Color color = _screenFade.color;
            color.a = _currentFadeLevel;
            _screenFade.color = color;
        }

        if (_missionText)
        {
            Invoke("HideMissionText", _missionTextDisplayTime);
        }
    }

    // ---------------------------------------------------------------
    // Name : Invalidate
    // Desc : Refreshes the values of UI elements
    // ---------------------------------------------------------------
    public void Invalidate(CharacterManager charManager)
    {
        if (charManager == null) return;
        if (_healthText) _healthText.text = "Health " + ((int)charManager.health).ToString();
        if (_staminaText) _staminaText.text = "Stamina " + ((int)charManager.stamina).ToString();
    }


    // ---------------------------------------------------------------
    // Name	:	SetInteractionText
    // Desc	:	This function sets the text that is displayed at the
    //			bottom of the display area. It is called the
    //			InterationText because it is used to display messages
    //			relating to interacting with objects.
    // ---------------------------------------------------------------
    public void SetInteractionText(string text)
    {
        if (_interactionText)
        {
            if (text == null)
            {
                _interactionText.text = null;
                _interactionText.gameObject.SetActive(false);
            }
            else
            {
                _interactionText.text = text;
                _interactionText.gameObject.SetActive(true);
            }
        }
    }


    public void Fade(float seconds, ScreenFadeType direction)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        float targetFade = 0.0f; ;

        switch (direction)
        {
            case ScreenFadeType.FadeIn:
                targetFade = 0.0f;
                break;

            case ScreenFadeType.FadeOut:
                targetFade = 1.0f;
                break;
        }

        _coroutine = FadeInternal(seconds, targetFade);
        StartCoroutine(_coroutine);
    }


    IEnumerator FadeInternal(float seconds, float targetFade)
    {
        if (!_screenFade) yield break;

        float timer = 0;
        float srcFade = _currentFadeLevel;
        Color oldColor = _screenFade.color;
        if (seconds < 0.1f) seconds = 0.1f;

        while (timer < seconds)
        {
            timer += Time.deltaTime;
            _currentFadeLevel = Mathf.Lerp(srcFade, targetFade, timer / seconds);
            oldColor.a = _currentFadeLevel;
            _screenFade.color = oldColor;
            yield return null;
        }

        oldColor.a = _currentFadeLevel = targetFade;
        _screenFade.color = oldColor;
    }

    public void ShowMissionText(string text)
    {
        if (_missionText)
        {
            _missionText.text = text;
            _missionText.gameObject.SetActive(true);
        }
    }

    public void HideMissionText()
    {
        if (_missionText)
        {
            _missionText.gameObject.SetActive(false);
        }
    }
}
