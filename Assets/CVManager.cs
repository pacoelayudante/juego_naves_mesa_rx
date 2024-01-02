using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Rect = UnityEngine.Rect;
using CvRect = OpenCvSharp.Rect;

public static class CVManager
{
    public static Texture2D Imagen { get; private set; }
    public static Texture2D ImagenHSV { get; private set; }

    public static Mat OriginalMat { get; private set; }
    public static Mat HsvMat { get; private set; }

    private static event System.Action<Texture2D> _onImagenCambiada;
    private static event System.Action<Mat> _onHSVMatGenerado;

    public static TipoHue TipoHue => TipoHue.HSV;

    public static void CambiarImagen(Texture2D imagen)
    {
        if (Imagen)
            Object.Destroy(Imagen);

        Imagen = imagen;
        bool hsvGenerado = false;

        if (Imagen)
        {
            if (OriginalMat != null && !OriginalMat.IsDisposed)
                OriginalMat.Dispose();

            OriginalMat = OpenCvSharp.Unity.TextureToMat(imagen);

            if (HsvMat == null)
                HsvMat = new Mat();

            Cv2.CvtColor(OriginalMat, HsvMat, TipoHue == TipoHue.HSV ? ColorConversionCodes.BGR2HSV : ColorConversionCodes.BGR2HLS);

            Cv2.MinMaxLoc(HsvMat.ExtractChannel(0), out double min, out double max);
            hsvGenerado = true;
        }

        _onImagenCambiada?.Invoke(imagen);

        if (hsvGenerado)
            _onHSVMatGenerado?.Invoke(HsvMat);
    }

    public static void AlCambiarImagen(System.Action<Texture2D> evento, bool ejecutarImediato = true)
    {
        evento?.Invoke(Imagen);
        if (ejecutarImediato)
            _onImagenCambiada += evento;
    }

    public static void AlGenerarHSVMat(System.Action<Mat> evento, bool ejecutarImediato = true)
    {
        evento?.Invoke(HsvMat);
        if (ejecutarImediato)
            _onHSVMatGenerado += evento;
    }

    public static Rect ConvertirBBoxAUVRect(CvRect cvRect, float width, float height, Vector4 margenes = default(Vector4))
    {
        margenes.Scale(new Vector4(-1f / width, -1f / height, 2f / width, 2f / height));
        return new Rect(cvRect.Left / width + margenes.x, (height - cvRect.Bottom) / height + margenes.y,
            cvRect.Width / width + margenes.z, cvRect.Height / height + margenes.w);
    }
}
