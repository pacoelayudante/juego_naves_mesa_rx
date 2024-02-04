using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rect = UnityEngine.Rect;
using CvRect = OpenCvSharp.Rect;
using OpenCvSharp;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class PlasmarEscenario : ScriptableObject
{
    public List<Sprite> _imagenHits = new List<Sprite>();
    public Sprite _imagenHitEscudo;
    public Sprite _shotSource;

    public TokenDetector _tokenDetector;

    public float expectedImageSize = 930;

    // public TokenTemplates TokenTemplates => _tokenDetector._tokenTemplates;

    public float defaultDistLaserMiss = 800f;
    public Vector3 rootSceneScale = new Vector3(1f, -1f, 1f);

    public Gradient gradientLaser;
    public Material[] _mats;
    public Material _rayoMat;
    public Material _escudoMat;

    public int segmentosEscudos = 4;

    public GameObject PrepararEscenario(Texture2D input)
    {
        return PrepararEscenario(input, _tokenDetector);
    }

    public GameObject PrepararEscenario(Texture2D input, TokenDetector detector)
    {
        detector.Detectar(input, out TokenDetector.Resultados resultados);
        return PrepararEscenario(resultados, detector);
    }

    public GameObject PrepararEscenario(Mat hsvMat, TipoHue tipoHue, TokenDetector detector)
    {
        detector.Detectar(hsvMat, tipoHue, out TokenDetector.Resultados resultados);
        return PrepararEscenario(resultados, detector);
    }

    public GameObject PrepararEscenario(TokenDetector.Resultados resultados, TokenDetector detector)
    {
        float escalaAcomodarTam = expectedImageSize / Mathf.Max(CVManager.Imagen.width, CVManager.Imagen.height);

        var nuevoEscenario = new GameObject("Escenario Creado");
        if (!Application.isPlaying)
            nuevoEscenario.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

        Dictionary<TokenDetector.TokenEncontrado, NaveEnEscena> navesEnEscena = new();
        Dictionary<int, List<NaveEnEscena>> equipos = new();

        //foreach (var token in resultados.todosLosTokens)
        for (int i = 0; i < resultados.todosLosTokens.Count; i++)
        {
            var nuevoToken = new GameObject($"{resultados.todosLosTokens[i].TemplateMasPosible.Nombre} {i}");
            nuevoToken.transform.SetParent(nuevoEscenario.transform, worldPositionStays: false);
            if (!Application.isPlaying)
                nuevoToken.hideFlags = HideFlags.DontSave;
            var naveEnEscena = nuevoToken.AddComponent<NaveEnEscena>();
            naveEnEscena.distLaserMiss = defaultDistLaserMiss / escalaAcomodarTam;
            naveEnEscena.gradientLaser = gradientLaser;
            naveEnEscena.Inicializar(resultados.todosLosTokens[i], this);

            navesEnEscena[resultados.todosLosTokens[i]] = (naveEnEscena);

            if (!equipos.ContainsKey(naveEnEscena.equipo))
                equipos.Add(naveEnEscena.equipo, new());
            equipos[naveEnEscena.equipo].Add(naveEnEscena);
        }

        foreach (var disparo in resultados.tokensDisparadores)
        {
            var primerTokenValido = disparo.tokenConArmasCercanos.Find(el => !navesEnEscena[el].DisparoPreparado);
            if (primerTokenValido != null)
                navesEnEscena[primerTokenValido].ApuntarDisparo(disparo, this);
        }

        foreach (int equipo in equipos.Keys)
        {
            foreach (var nave in equipos[equipo])
                nave.ColliderEnabled = false;

            equipos[equipo].Sort((el, el2) => el.Orden.CompareTo(el2.Orden));
            var randomList = new List<NaveEnEscena>(equipos[equipo]);
            // foreach (var nave in equipos[equipo])
            while (randomList.Count > 0)
            {
                var nave = randomList[Random.Range(0, randomList.Count)];
                randomList.Remove(nave);
                nave.CalcularRayos(_rayoMat, _imagenHitEscudo);
            }

            foreach (var nave in equipos[equipo])
                nave.ColliderEnabled = true;
        }

        if (_imagenHits.Count > 0)
        {
            foreach (var nave in navesEnEscena.Values)
            {
                foreach (var disparo in nave._disparosRecibidos)
                {
                    if (disparo.receptor.transform != disparo.rayo.transform)
                        continue;

                    var newHitGO = new GameObject($"Disparo Recibido de {disparo.origen} contra {disparo.receptor}");
                    newHitGO.transform.parent = disparo.receptor.transform;
                    newHitGO.transform.position = (Vector3)disparo.rayo.point - Vector3.forward;
                    newHitGO.transform.localRotation = Quaternion.Euler(0, 0, Random.value * 360f);
                    newHitGO.transform.localScale = Vector3.one / escalaAcomodarTam;
                    var sr = newHitGO.gameObject.AddComponent<SpriteRenderer>();
                    int index = Mathf.Min(_imagenHits.Count - 1, disparo.origen.template.nivelArma);
                    sr.sprite = _imagenHits[index];
                }
            }
        }

        // el CreateMesh del collider tiene que suceder antes que el flip, asique...
        // mismo los rayos? como que andan mas o menos?
        nuevoEscenario.transform.localScale = rootSceneScale * escalaAcomodarTam;

        return nuevoEscenario;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlasmarEscenario))]
    public class PlasmarEscenarioEditor : Editor
    {
        RenderTexture _renderTexture;
        private Texture2D _texturaInput;

        void OnDisable()
        {
            if (_texturaInput != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_texturaInput)))
                DestroyImmediate(_texturaInput);
        }

        public override void OnInspectorGUI()
        {
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                _renderTexture = EditorGUILayout.ObjectField("Render Texture", _renderTexture, typeof(RenderTexture), allowSceneObjects: false) as RenderTexture;
                if (changed.changed && _renderTexture)
                {
                    if (_texturaInput != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_texturaInput)))
                        DestroyImmediate(_texturaInput);

                    _texturaInput = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false, true);
                    RenderTexture.active = _renderTexture;
                    _texturaInput.ReadPixels(new Rect(0, 0, _texturaInput.width, _texturaInput.height), 0, 0, false);
                    _texturaInput.Apply(false);
                }
            }
            _texturaInput = (Texture2D)EditorGUILayout.ObjectField("Templates Image", _texturaInput, typeof(Texture2D), allowSceneObjects: false);

            DrawDefaultInspector();
            if (GUILayout.Button("Generar Escena") && _texturaInput)
            {
                ((PlasmarEscenario)target).PrepararEscenario(_texturaInput);
            }
        }
    }
#endif
}
