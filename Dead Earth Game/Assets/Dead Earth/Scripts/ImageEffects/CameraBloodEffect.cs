using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour 
{
	[SerializeField]
	private Shader 		_shader = null;
	private Material	_material = null;

	void OnRenderImage( RenderTexture src, RenderTexture dest )
	{
		if (_shader==null) return;
		if (_material==null)
		{
			_material = new Material( _shader );
		}

		if (_material==null) return;

		Graphics.Blit( src, dest, _material);
	}

}
