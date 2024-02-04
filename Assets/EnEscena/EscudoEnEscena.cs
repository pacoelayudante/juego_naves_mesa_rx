using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscudoEnEscena : MonoBehaviour
{
    public NaveEnEscena _nave;

    public Vector2 escudo;
    public Vector2 centro;
    public float radio;

    public List<Collider2D> _colliders = new();
    [System.NonSerialized]
    public LineRenderer linea;

    public float escalaAcomodarTam = 1;

    public void Hit(Disparo disparo, Sprite escudoHit)
    {
        var newHitGO = new GameObject($"Disparo Recibido de {disparo.origen} contra escudo de {disparo.receptor}");
        newHitGO.transform.parent = transform;
        newHitGO.transform.position = (Vector3)disparo.rayo.point - Vector3.forward;
        newHitGO.transform.localRotation = Quaternion.Euler(0, 0, Random.value * 360f);
        newHitGO.transform.localScale = Vector3.one / escalaAcomodarTam;
        var sr = newHitGO.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = escudoHit;

        var linea = GetComponent<LineRenderer>();
        linea.startColor = linea.endColor = Color.Lerp(linea.startColor, Color.black, 0.5f);

        foreach (var collider in _colliders)
            collider.enabled = false;
    }

    public static EscudoEnEscena Inicializar(NaveEnEscena nave, Vector2 centro, float radioBase, int segmentosEscudos, Vector2 escudoPow, Material matEscudo)
    {
        var escudoGO = new GameObject($"Escudo de {nave} ({escudoPow.x})");
        escudoGO.transform.parent = nave.transform;
        escudoGO.transform.localPosition = centro;
        
        // hardcoded
        escudoGO.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        var escudo = escudoGO.AddComponent<EscudoEnEscena>();

        escudo._nave = nave;
        escudo.centro = centro;
        escudo.radio = radioBase * escudoPow.y;

        escudo.linea = escudoGO.AddComponent<LineRenderer>();
        escudo.linea.sharedMaterial = matEscudo;
        escudo.linea.loop = true;
        escudo.linea.useWorldSpace = false;
        escudo.linea.positionCount = 36;
        float rotVal = 360f / escudo.linea.positionCount;
        for (int i = 0; i < escudo.linea.positionCount; i++)
        {
            escudo.linea.SetPosition(i, Quaternion.Euler(0, 0, i * rotVal) * Vector3.right * escudo.radio);
        }

        if (escudoPow.x >= 1f)
        {
            var colliderCircular = escudoGO.AddComponent<CircleCollider2D>();
            // colliderCircular.offset = centro;
            colliderCircular.radius = escudo.radio;
            escudo._colliders.Add(colliderCircular);
        }
        else
        {
            var curvaList = new List<Keyframe>();
            for (int i = 0; i < segmentosEscudos; i++)
            {
                escudo.AgregarArcoCollider(i * 360f / segmentosEscudos, (i + escudoPow.x) * 360f / segmentosEscudos, rotVal);

                curvaList.Add(new Keyframe()
                {
                    // weightedMode = WeightedMode.None,
                    inTangent = float.PositiveInfinity,
                    outTangent = float.PositiveInfinity,
                    time = i * (1f / segmentosEscudos),
                    value = 1,
                });
                curvaList.Add(new Keyframe()
                {
                    // weightedMode = WeightedMode.None,
                    inTangent = float.PositiveInfinity,
                    outTangent = float.PositiveInfinity,
                    time = (i + escudoPow.x) * (1f / segmentosEscudos),
                    value = 0,
                });
            }
            escudo.linea.widthCurve = new AnimationCurve(curvaList.ToArray());
        }

        return escudo;
    }

    void AgregarArcoCollider(float minA, float maxA, float rotVal)
    {
        var colliderArco = gameObject.AddComponent<EdgeCollider2D>();
        // colliderArco.offset = centro;

        var pos = new List<Vector2>();
        for (float a = minA; a < maxA; a += rotVal)
        {
            pos.Add(Quaternion.Euler(0, 0, a) * Vector3.right * radio);
        }
        pos.Add(Quaternion.Euler(0, 0, maxA) * Vector3.right * radio);
        colliderArco.SetPoints(pos);

        _colliders.Add(colliderArco);
    }
}
