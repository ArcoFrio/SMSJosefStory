
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Common.UnityUI;

[AddComponentMenu("Game Creator/UI/Toggle")]
public class TogglePropertyBool : Toggle
{
    [SerializeField]
    private bool m_SetFromSource;

    [SerializeField]
    private PropertySetBool m_OnChangeSet = new PropertySetBool();

    private Args m_Args;

    protected override void Start()
    {
        base.Start();
        if (Application.isPlaying)
        {
            m_Args = new Args(base.gameObject);
            if (m_SetFromSource)
            {
                SetValueFromProperty();
            }

            onValueChanged.AddListener(OnChangeValue);
        }
    }

    public void SetValueFromProperty()
    {
        base.isOn = m_OnChangeSet.Get(m_Args);
    }

    private void OnChangeValue(bool value)
    {
        m_OnChangeSet.Set(value, m_Args);
    }
}