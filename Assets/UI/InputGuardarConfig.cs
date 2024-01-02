using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class InputGuardarConfig : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _textInput;
    [SerializeField]
    Button _saveButton;

    public event System.Action<string> OnSave;

    public string Text
    {
        get => _textInput.text;
        set => _textInput.text = value;
    }

    void Awake()
    {
        _saveButton.onClick.AddListener(() => OnSave?.Invoke(Text));
    }
}
