using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class BotonConOnRelease : Selectable
{
    // Event delegates triggered on up.
    [SerializeField]
    private UnityEvent m_OnDown = new();
    [SerializeField]
    private UnityEvent m_OnUp = new();

    public UnityEvent onDown
    {
        get { return m_OnDown; }
        set { m_OnDown = value; }
    }

    public UnityEvent onUp
    {
        get { return m_OnUp; }
        set { m_OnUp = value; }
    }

    int _pointerId = -1;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (_pointerId != -1)
            return;

        _pointerId = eventData.pointerId;
        m_OnDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == _pointerId)
        {
            _pointerId = -1;
            m_OnUp?.Invoke();
        }

        base.OnPointerUp(eventData);
    }
}
