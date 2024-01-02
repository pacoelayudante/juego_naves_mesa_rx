using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TomarFotoControl : MonoBehaviour
{
    [SerializeField]
    private Button _tomarImagen;
    [SerializeField]
    private BotonConOnRelease _verImagenActual;
    [SerializeField]
    private PreviewFoto _preview;

    void Awake()
    {
        _tomarImagen.onClick.AddListener(TomarFoto);
        _verImagenActual.onDown.AddListener(VerImagen);
        _verImagenActual.onUp.AddListener(EsconderImagen);
    }

    private void VerImagen()
    {
        _preview.gameObject.SetActive(true);
    }

    private void EsconderImagen()
    {
        _preview.gameObject.SetActive(false);
    }

    private void TomarFoto()
    {
        var permiso = NativeCamera.TakePicture(path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                var imagenRecuperada = NativeCamera.LoadImageAtPath(path, -1, markTextureNonReadable:false, generateMipmaps:false);
                if (imagenRecuperada != null)
                    CVManager.CambiarImagen(imagenRecuperada);
            }
        });

        if (permiso == NativeCamera.Permission.ShouldAsk)
        {
            if (NativeCamera.CanOpenSettings())
                NativeCamera.OpenSettings();
        }
    }
}
