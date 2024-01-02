using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ImageExplorer : MonoBehaviour
{
    [SerializeField]
    private RawImage _preview;
    private AspectRatioFitter _aspectRatio;

    [SerializeField]
    private Slider _zoomControl;

    private RawImage Preview => _preview ? _preview : _preview = GetComponent<RawImage>();

    public Texture Texture
    {
        get => Preview == null ? null : Preview.texture;
        set
        {
            if (Preview)
            {
                Preview.texture = value;

                if (value == null)
                    return;

                if (_aspectRatio == null)
                    _aspectRatio = Preview.GetComponent<AspectRatioFitter>();

                if (_aspectRatio != null)
                    _aspectRatio.aspectRatio = value.width / (float)value.height;
            }
        }
    }

    public Material Material => Preview.materialForRendering;

    void Awake()
    {
        _zoomControl.onValueChanged.AddListener((value) => Zoom = value);
        Zoom = _zoomControl.value;
    }

    public float Zoom
    {
        get => Preview ? Preview.transform.localScale.x : 1f;
        set
        {
            if (Preview)
                Preview.transform.localScale = Vector3.one * value;
        }
    }
}
