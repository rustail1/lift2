using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _distance = 2.5f;
    [SerializeField] private LayerMask _interactionMask = ~0;

    [Header("Anti Flicker")]
    [Tooltip("Если включено, вместо тонкого Raycast используется SphereCast. Это убирает мигание интерактива из-за дыхания/покачивания камеры.")]
    [SerializeField] private bool _useSphereCast = true;

    [Tooltip("Радиус SphereCast. 0.05-0.12 обычно нормально для маленьких объектов.")]
    [SerializeField] private float _sphereCastRadius = 0.08f;

    [Tooltip("Сколько секунд держать последний найденный объект, если луч на долю секунды соскользнул из-за camera bob / breathing.")]
    [SerializeField] private float _lostTargetGraceTime = 0.15f;

    [Header("Debug")]
    [SerializeField] private string _currentInteractionText;
    [SerializeField] private bool _hasCurrent;
    [SerializeField] private bool _isUsingGraceTarget;

    private IInteractable _current;
    private IInteractable _lastValid;
    private float _lostTargetTimer;

    private void Reset()
    {
        if (_camera == null)
            _camera = Camera.main != null ? Camera.main : GetComponentInChildren<Camera>();
    }

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_camera == null)
            _camera = Camera.main != null ? Camera.main : GetComponentInChildren<Camera>();

        _current = null;
        _lastValid = null;
        _lostTargetTimer = 0f;
        _hasCurrent = false;
        _isUsingGraceTarget = false;
        _currentInteractionText = string.Empty;
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick()
    {
        UpdateCurrentInteractable();
        UpdateInteractionUI();
    }

    public void InteractDown()
    {
        if (_current == null)
            return;

        _current.InteractDown();
    }

    public void InteractUp()
    {
        _current?.InteractUp();
    }

    private void UpdateCurrentInteractable()
    {
        IInteractable found = FindInteractableInCenter();

        if (found != null)
        {
            _current = found;
            _lastValid = found;
            _lostTargetTimer = _lostTargetGraceTime;
            _isUsingGraceTarget = false;
        }
        else
        {
            // Если камера чуть качнулась и луч на 1-2 кадра потерял объект,
            // не выключаем UI сразу. Это убирает неприятное мигание при idle breathing.
            if (_lastValid != null && _lostTargetTimer > 0f)
            {
                _lostTargetTimer -= Time.deltaTime;
                _current = _lastValid;
                _isUsingGraceTarget = true;
            }
            else
            {
                _current = null;
                _lastValid = null;
                _lostTargetTimer = 0f;
                _isUsingGraceTarget = false;
            }
        }

        _hasCurrent = _current != null;
        _currentInteractionText = _current != null ? _current.GetInteractionText() : string.Empty;
    }

    private IInteractable FindInteractableInCenter()
    {
        if (_camera == null)
            return null;

        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

        RaycastHit hit;
        bool hasHit;

        if (_useSphereCast && _sphereCastRadius > 0f)
        {
            hasHit = Physics.SphereCast(
                ray,
                _sphereCastRadius,
                out hit,
                _distance,
                _interactionMask,
                QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            hasHit = Physics.Raycast(
                ray,
                out hit,
                _distance,
                _interactionMask,
                QueryTriggerInteraction.Ignore
            );
        }

        if (!hasHit)
            return null;

        return hit.collider.GetComponentInParent<IInteractable>();
    }

    private void UpdateInteractionUI()
    {
        InteractionUIService ui = InteractionUIService.Instance;
        if (ui == null)
            return;

        bool active = _current != null;
        ui.SetInteractionTarget(active, active ? _currentInteractionText : string.Empty);

        if (_current == null)
        {
            ui.SetHoldProgress(0f);
            ui.SetMashProgress(0f);
            return;
        }

        float progress = _current.GetInteractionProgress();
        InteractionType type = _current.GetInteractionType();

        ui.SetHoldProgress(type == InteractionType.Hold && _current.IsInteractionInProgress ? progress : 0f);
        ui.SetMashProgress(type == InteractionType.Mash ? progress : 0f);
    }
}
