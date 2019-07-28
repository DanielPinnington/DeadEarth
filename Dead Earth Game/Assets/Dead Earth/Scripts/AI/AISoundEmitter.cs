using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    //Inspector Assigned

    [SerializeField] private float _decayRate = 1.0f;

    //Internal
    private SphereCollider _collider = null;
    private float _srsRadius = 0.0f;
    private float _tgtRadius = 0.0f;
    private float _interpolator = 0.0f;
    private float _interpolatorSpeed = 0.0f;

    void Start()
    {
        //Cache collider reference
        _collider = GetComponent<SphereCollider>();
        if (!_collider) return;

        //set radius values
        _srsRadius = _tgtRadius = _collider.radius;

        //set interpolator
        _interpolator = 0.0f;
        if (_decayRate > 0.0f)
            _interpolatorSpeed = 1.0f / _decayRate;
        else
            _interpolatorSpeed = 0.0f;
    }

    void FixedUpdate()
    {
        if (!_collider) return;
        _interpolator = Mathf.Clamp01(_interpolator + Time.deltaTime * _interpolatorSpeed);
        _collider.radius = Mathf.Lerp(_srsRadius, _tgtRadius, _interpolator);

        if (_collider.radius < Mathf.Epsilon)

            _collider.enabled = false;
        else
            _collider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantResize = false)
    {
        if (!_collider || newRadius == _tgtRadius) return;
        _srsRadius = (instantResize || newRadius > _collider.radius)?newRadius:_collider.radius;
        _tgtRadius = newRadius;
        _interpolator = 0.0f;
    }
}
