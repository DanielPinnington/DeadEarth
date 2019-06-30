using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField]
    private Texture2D _bloodTexture = null;

    [SerializeField]
    private Texture2D _bloodNormalMap = null;

    [SerializeField]
    private float _bloodAmount = 0.0f;

    [SerializeField]
    private float _minBloodAmount = 0.0f;

    [SerializeField]
    private float _distortion = 1.0f;

    [SerializeField]
    private bool _autoFade = true;

    [SerializeField]
    private float _fadeSpeed = 0.05f;

    [SerializeField]
    private Shader _shader = null;

    // Private
    private Material _material = null;

    // Properties
    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

    void Update()
    {
        if (_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_shader == null) return;
        if (_material == null)
        {
            _material = new Material(_shader);
        }

        if (_material == null) return;

        // Send data into Shader
        if (_bloodTexture != null)
            _material.SetTexture("_BloodTex", _bloodTexture);

        if (_bloodNormalMap != null)
            _material.SetTexture("_BloodBump", _bloodNormalMap);

        _material.SetFloat("_Distortion", _distortion);
        _material.SetFloat("_BloodAmount", _bloodAmount);

        // Perform Image Effect
        Graphics.Blit(src, dest, _material);
    }

}
