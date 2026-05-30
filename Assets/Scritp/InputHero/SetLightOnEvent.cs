using UnityEngine;

public enum LightEventAction
{
    TurnOn,
    TurnOff,
    Toggle,
    SetIntensity,
    SetColor,
    SetIntensityAndColor
}

[DisallowMultipleComponent]
public class SetLightOnEvent : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private string _eventName = "PowerLost";

    [Header("Targets")]
    [SerializeField] private Light[] _lights;

    [Header("Action")]
    [SerializeField] private LightEventAction _action = LightEventAction.TurnOff;
    [SerializeField] private float _intensity = 1f;
    [SerializeField] private Color _color = Color.white;

    [Header("Debug")]
    [SerializeField] private bool _isInitialized;
    [SerializeField] private int _triggerCount;

    public void Initialize()
    {
        _isInitialized = true;
        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;
    }

    private void OnGameEvent(string eventId)
    {
        if (eventId != _eventName)
            return;

        Apply();
    }

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        _triggerCount++;

        if (_lights == null)
            return;

        for (int i = 0; i < _lights.Length; i++)
        {
            Light light = _lights[i];
            if (light == null)
                continue;

            switch (_action)
            {
                case LightEventAction.TurnOn:
                    light.enabled = true;
                    break;

                case LightEventAction.TurnOff:
                    light.enabled = false;
                    break;

                case LightEventAction.Toggle:
                    light.enabled = !light.enabled;
                    break;

                case LightEventAction.SetIntensity:
                    light.intensity = _intensity;
                    break;

                case LightEventAction.SetColor:
                    light.color = _color;
                    break;

                case LightEventAction.SetIntensityAndColor:
                    light.intensity = _intensity;
                    light.color = _color;
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
