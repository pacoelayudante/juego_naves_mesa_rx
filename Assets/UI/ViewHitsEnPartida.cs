using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewHitsEnPartida : MonoBehaviour
{
    public Button _cerrarButton;
    public Camera _orthCam;
    public Slider _zoomSlider;

    public Button _sigNave;
    public Button _prevNave;
    public TMPro.TMP_Text _navesText;
    public List<NaveEnEscena> navesEnEscena = new();

    public ManagerDePartida managerDePartida;

    public event System.Action AlCerrar;

    int indiceMuestraActual = 0;
    // Start is called before the first frame update
    void Awake()
    {
        _cerrarButton.onClick.AddListener(() => AlCerrar?.Invoke());

        _zoomSlider.onValueChanged.AddListener(val =>
        {
            _orthCam.orthographicSize = val;
        });

        _sigNave.onClick.AddListener(() =>
        {
            if (navesEnEscena.Count > 0)
            {
                indiceMuestraActual = (indiceMuestraActual + 1) % navesEnEscena.Count;
                MostrarIndice(indiceMuestraActual);
            }
        });
        _prevNave.onClick.AddListener(() =>
        {
            if (navesEnEscena.Count > 0)
            {
                indiceMuestraActual = (indiceMuestraActual - 1 + navesEnEscena.Count) % navesEnEscena.Count;
                MostrarIndice(indiceMuestraActual);
            }
        });
    }

    void OnEnable()
    {
        if (managerDePartida.rootEscenaGenerada)
        {
            indiceMuestraActual = -1;
            managerDePartida.rootEscenaGenerada.GetComponentsInChildren<NaveEnEscena>(navesEnEscena);

            MostrarIndice(-1);
            navesEnEscena.RemoveAll(el => el._disparosRecibidos.Count == 0);
        }
    }

    void MostrarIndice(int mostrar)
    {
        indiceMuestraActual = mostrar;
        if (indiceMuestraActual >= 0 && indiceMuestraActual < navesEnEscena.Count)
        {
            var x = navesEnEscena[indiceMuestraActual].Centro.x * managerDePartida.escalaAcomodarTam;
            var y = -navesEnEscena[indiceMuestraActual].Centro.y * managerDePartida.escalaAcomodarTam;
            _orthCam.transform.position = new Vector3(x, y, _orthCam.transform.position.z);
            _navesText.text = $"{(indiceMuestraActual + 1)}/{navesEnEscena.Count}";
        }
        else
        {
            var x = CVManager.Imagen.width / (2 );
            var y = -CVManager.Imagen.height / (2 );
            _orthCam.transform.position = new Vector3(x, y, _orthCam.transform.position.z) * managerDePartida.escalaAcomodarTam;
            _orthCam.orthographicSize = CVManager.Imagen.height * managerDePartida.escalaAcomodarTam / 2;

            _navesText.text = $"Todo";
        }
    }
}
