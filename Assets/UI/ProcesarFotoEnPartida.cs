using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcesarFotoEnPartida : MonoBehaviour
{
    public ManagerDePartida _manager;
    public Button _procesar;
    public ImageExplorer _previewFoto;
    public ViewHitsEnPartida _viewHits;

    public Button _cerrar;
    // Start is called before the first frame update
    void Awake()
    {
        CVManager.AlCambiarImagen(AlCambiarImagen);

        _procesar.onClick.AddListener(ProcesarImagen);

        _viewHits.AlCerrar += () =>
        {
            gameObject.SetActive(true);
            _viewHits.gameObject.SetActive(false);
        };

        _cerrar.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        });
    }

    private void AlCambiarImagen(Texture2D imagen)
    {
        if (!imagen)
            return;

        _previewFoto.Texture = imagen;
    }

    private void ProcesarImagen()
    {
        if (CVManager.HsvMat != null)
        {
            _manager.ProcesarMat(CVManager.HsvMat);
            gameObject.SetActive(false);
            _viewHits.gameObject.SetActive(true);
        }
    }
}
