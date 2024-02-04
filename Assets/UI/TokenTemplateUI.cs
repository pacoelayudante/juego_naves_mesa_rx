using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TokenTemplateUI : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown _tamDropdown;
    [SerializeField]
    TMP_Dropdown _ordenDropdown;
    [SerializeField]
    TMP_Dropdown _escudosDropdown;
    [SerializeField]
    TMP_Dropdown _armaPowerDropdown;

    [SerializeField]
    RawImage _rawImage;

    public void Set(TokenTemplates.TokenTemplate template)
    {
        _tamDropdown.value = (int)template.tipoTam;
        _ordenDropdown.value = template.ordenDeDisparo+1;
        _escudosDropdown.value = template.escudos.Count - 1;
        _armaPowerDropdown.value = template.nivelArma;
    }

    public void Apply(TokenTemplates.TokenTemplate target, ListaEscudos listaEscudos)
    {
        target.tipoTam = (TokenTemplates.TipoTam)_tamDropdown.value;
        target.ordenDeDisparo = _ordenDropdown.value-1;
        target.escudos = listaEscudos[_escudosDropdown.value];
        target.nivelArma = _armaPowerDropdown.value;
    }

    public void SetImage(Texture2D texture, Rect uvRect)
    {
        _rawImage.texture = texture;
        _rawImage.uvRect = uvRect;

        float matWidth = texture.width;
        float matHeight = texture.height;
        float matAspect = matHeight / matWidth;

        var escala = Vector3.one;
        escala.y = matAspect * _rawImage.uvRect.height / _rawImage.uvRect.width;
        escala.x = _rawImage.uvRect.width / (matAspect * _rawImage.uvRect.height);
        if (escala.y < escala.x)
            escala.x = 1f;
        else
            escala.y = 1f;
        _rawImage.transform.localScale = escala;
    }

    public void InitDropdowns(int ordenMax, int escudosMaxLevel, int armasMaxLevel)
    {
        _tamDropdown.options = OpcionesDesdeEnum<TokenTemplates.TipoTam>();
        _ordenDropdown.options = OpcionesDesdeRangoNumeros("Orden {0}", ordenMax);
        _escudosDropdown.options = OpcionesDesdeRangoNumeros("Escudo Lvl {0}", escudosMaxLevel);
        _armaPowerDropdown.options = OpcionesDesdeRangoNumeros("Arma Lvl {0}", armasMaxLevel);

        // extra orden -1 para que no se agregue el elemento
        _ordenDropdown.options.Insert(0, new TMP_Dropdown.OptionData(){ text = "No Usar" });
    }

    private List<TMP_Dropdown.OptionData> OpcionesDesdeEnum<T>() where T : System.Enum
    {
        var nombres = System.Enum.GetNames(typeof(T));
        List<TMP_Dropdown.OptionData> opciones = new();
        foreach (var nombre in nombres)
            opciones.Add(new TMP_Dropdown.OptionData() { text = nombre });

        return opciones;
    }

    private List<TMP_Dropdown.OptionData> OpcionesDesdeRangoNumeros(string opcionFormat, int max)
    {
        List<TMP_Dropdown.OptionData> opciones = new();
        for (int i = 0; i < max; i++)
            opciones.Add(new TMP_Dropdown.OptionData() { text = string.Format(opcionFormat, i + 1) });

        return opciones;
    }
}
