using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PreviewFoto : MonoBehaviour
{
    private RawImage _preview;
    private AspectRatioFitter _aspectRatio;
    
    private RawImage Preview => _preview?_preview:_preview=GetComponent<RawImage>();

    void Awake()
    {
        CVManager.AlCambiarImagen(AlCambiarImagen);
    }

    private void AlCambiarImagen(Texture2D imagen)
    {
        if (!imagen)
            return;

        Preview.texture = imagen;

        if (_aspectRatio == null)
            _aspectRatio = Preview.GetComponent<AspectRatioFitter>();

        if (_aspectRatio != null)
            _aspectRatio.aspectRatio = imagen.width / (float)imagen.height;
    }
}
