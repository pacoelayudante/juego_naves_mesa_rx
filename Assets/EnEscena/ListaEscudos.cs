using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ListaEscudos : ScriptableObject
{
    public int Count => escudosLevels == null ? 0 : escudosLevels.Count;

    [System.Serializable]
    public class EscudoLevel
    {
        public List<Vector2> Escudos = new();
    }

    public List<Vector2> this[int index] => escudosLevels == null || index >= escudosLevels.Count ? null : escudosLevels[index].Escudos;

    public List<EscudoLevel> escudosLevels = new();//x=porcentaje, y=tam
}
