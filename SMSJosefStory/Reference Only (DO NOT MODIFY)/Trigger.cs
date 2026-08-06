using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCreator.Runtime.VisualScripting;

[HelpURL("https://docs.gamecreator.io/gamecreator/visual-scripting/triggers")]
[AddComponentMenu("Game Creator/Visual Scripting/Trigger")]
[DefaultExecutionOrder(1)]
public class Trigger : BaseActions, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISignalReceiver
{
    [SerializeReference]
    protected Event m_TriggerEvent = new EventOnStart();

    [NonSerialized]
    private Args m_Args;

    [NonSerialized]
    private Rigidbody m_Rigidbody3D;

    [NonSerialized]
    private Rigidbody2D m_Rigidbody2D;

    [NonSerialized]
    private Collider m_Collider3D;

    [NonSerialized]
    private Collider2D m_Collider2D;

    [NonSerialized]
    private IInteractive m_Interactive;

    public bool IsExecuting { get; private set; }

    public event Action EventBeforeExecute;

    public event Action EventAfterExecute;

    public static void Reconfigure(Trigger trigger, Event triggerEvent, InstructionList instructions)
    {
        trigger.m_TriggerEvent = triggerEvent;
        trigger.m_Instructions = instructions;
    }

    public override void Invoke(GameObject self = null)
    {
        Args args = new Args((self != null) ? self : base.gameObject, base.gameObject);
        Execute(args);
    }

    public async Task Execute(Args args)
    {
        if (!IsExecuting)
        {
            IsExecuting = true;
            this.EventBeforeExecute?.Invoke();
            try
            {
                await ExecInstructions(args);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString(), this);
            }

            IsExecuting = false;
            this.EventAfterExecute?.Invoke();
        }
    }

    public async Task Execute(GameObject target)
    {
        if (!IsExecuting)
        {
            m_Args.ChangeTarget(target);
            await Execute(m_Args);
        }
    }

    public async Task Execute()
    {
        if (!IsExecuting)
        {
            m_Args.ChangeTarget(null);
            await Execute(m_Args);
        }
    }

    public void Cancel()
    {
        StopExecInstructions();
    }

    protected void Awake()
    {
        m_Args = new Args(this);
        m_TriggerEvent?.OnAwake(this);
    }

    protected void Start()
    {
        m_TriggerEvent?.OnStart(this);
    }

    protected void OnEnable()
    {
        m_TriggerEvent?.OnEnable(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Cancel();
        m_TriggerEvent?.OnDisable(this);
    }

    protected override void OnDestroy()
    {
        m_TriggerEvent?.OnDestroy(this);
        base.OnDestroy();
    }

    protected void OnBecameVisible()
    {
        m_TriggerEvent?.OnBecameVisible(this);
    }

    protected void OnBecameInvisible()
    {
        m_TriggerEvent?.OnBecameInvisible(this);
    }

    protected void Update()
    {
        m_TriggerEvent?.OnUpdate(this);
    }

    protected void LateUpdate()
    {
        m_TriggerEvent?.OnLateUpdate(this);
    }

    protected void FixedUpdate()
    {
        m_TriggerEvent?.OnFixedUpdate(this);
    }

    protected void OnApplicationFocus(bool f)
    {
        m_TriggerEvent?.OnApplicationFocus(this, f);
    }

    protected void OnApplicationPause(bool s)
    {
        m_TriggerEvent?.OnApplicationPause(this, s);
    }

    protected void OnApplicationQuit()
    {
        m_TriggerEvent?.OnApplicationQuit(this);
    }

    protected void OnCollisionEnter(Collision c)
    {
        m_TriggerEvent?.OnCollisionEnter3D(this, c);
    }

    protected void OnCollisionExit(Collision c)
    {
        m_TriggerEvent?.OnCollisionExit3D(this, c);
    }

    protected void OnCollisionStay(Collision c)
    {
        m_TriggerEvent?.OnCollisionStay3D(this, c);
    }

    protected void OnTriggerEnter(Collider c)
    {
        m_TriggerEvent?.OnTriggerEnter3D(this, c);
    }

    protected void OnTriggerExit(Collider c)
    {
        m_TriggerEvent?.OnTriggerExit3D(this, c);
    }

    protected void OnTriggerStay(Collider c)
    {
        m_TriggerEvent?.OnTriggerStay3D(this, c);
    }

    protected void OnJointBreak(float force)
    {
        m_TriggerEvent?.OnJointBreak3D(this, force);
    }

    protected void OnCollisionEnter2D(Collision2D c)
    {
        m_TriggerEvent?.OnCollisionEnter2D(this, c);
    }

    protected void OnCollisionExit2D(Collision2D c)
    {
        m_TriggerEvent?.OnCollisionExit2D(this, c);
    }

    protected void OnCollisionStay2D(Collision2D c)
    {
        m_TriggerEvent?.OnCollisionStay2D(this, c);
    }

    protected void OnTriggerEnter2D(Collider2D c)
    {
        m_TriggerEvent?.OnTriggerEnter2D(this, c);
    }

    protected void OnTriggerExit2D(Collider2D c)
    {
        m_TriggerEvent?.OnTriggerExit2D(this, c);
    }

    protected void OnTriggerStay2D(Collider2D c)
    {
        m_TriggerEvent?.OnTriggerStay2D(this, c);
    }

    protected void OnJointBreak2D(Joint2D joint)
    {
        m_TriggerEvent?.OnJointBreak2D(this, joint);
    }

    protected void OnMouseDown()
    {
        m_TriggerEvent?.OnMouseDown(this);
    }

    protected void OnMouseUp()
    {
        m_TriggerEvent?.OnMouseUp(this);
    }

    protected void OnMouseUpAsButton()
    {
        m_TriggerEvent?.OnMouseUpAsButton(this);
    }

    protected void OnMouseEnter()
    {
        m_TriggerEvent?.OnMouseEnter(this);
    }

    protected void OnMouseOver()
    {
        m_TriggerEvent?.OnMouseOver(this);
    }

    protected void OnMouseExit()
    {
        m_TriggerEvent?.OnMouseExit(this);
    }

    protected void OnMouseDrag()
    {
        m_TriggerEvent?.OnMouseDrag(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_TriggerEvent?.OnPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_TriggerEvent?.OnPointerExit(this);
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_TriggerEvent?.OnSelect(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        m_TriggerEvent?.OnDeselect(this);
    }

    protected virtual void OnDrawGizmos()
    {
        m_TriggerEvent?.OnDrawGizmos(this);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        m_TriggerEvent?.OnDrawGizmosSelected(this);
    }

    void ISignalReceiver.OnReceiveSignal(SignalArgs args)
    {
        m_TriggerEvent?.OnReceiveSignal(this, args);
    }

    public void OnReceiveCommand(CommandArgs args)
    {
        m_TriggerEvent?.OnReceiveCommand(this, args);
    }

    [Obsolete("Soon to deprecate. Use OnReceiveCommand(CommandArgs) instead")]
    public void OnReceiveCommand(PropertyName command)
    {
        m_TriggerEvent?.OnReceiveCommand(this, new CommandArgs(command));
    }

    public void RequireRigidbody()
    {
        if (m_Collider3D == null)
        {
            m_Collider3D = this.Get<Collider>();
        }

        if (m_Collider2D == null)
        {
            m_Collider2D = this.Get<Collider2D>();
        }

        if (m_Collider3D != null)
        {
            RequireRigidbody3D();
        }

        if (m_Collider2D != null)
        {
            RequireRigidbody2D();
        }
    }

    private void RequireRigidbody3D()
    {
        if (m_Rigidbody3D != null)
        {
            return;
        }

        m_Rigidbody3D = this.Get<Rigidbody>();
        if (m_Rigidbody3D != null)
        {
            return;
        }

        if (m_Collider3D == null)
        {
            m_Collider3D = this.Get<Collider>();
            if (m_Collider3D == null)
            {
                return;
            }
        }

        m_Rigidbody3D = this.Add<Rigidbody>();
        m_Rigidbody3D.isKinematic = true;
        m_Rigidbody3D.hideFlags = HideFlags.HideInInspector;
    }

    private void RequireRigidbody2D()
    {
        if (m_Rigidbody2D != null)
        {
            return;
        }

        m_Rigidbody2D = this.Get<Rigidbody2D>();
        if (m_Rigidbody2D != null)
        {
            return;
        }

        if (m_Collider2D == null)
        {
            m_Collider2D = this.Get<Collider2D>();
            if (m_Collider2D == null)
            {
                return;
            }
        }

        m_Rigidbody2D = this.Add<Rigidbody2D>();
        m_Rigidbody2D.isKinematic = true;
        m_Rigidbody2D.hideFlags = HideFlags.HideInInspector;
    }

    internal void RequireInteractionTracker()
    {
        InteractionTracker interactionTracker = (InteractionTracker)(m_Interactive = InteractionTracker.Require(base.gameObject));
        interactionTracker.EventInteract -= OnStartInteraction;
        interactionTracker.EventInteract += OnStartInteraction;
    }

    private void OnStartInteraction(Character character, IInteractive interactive)
    {
        EventAfterExecute -= OnStopInteraction;
        EventAfterExecute += OnStopInteraction;
        m_TriggerEvent?.OnInteract(this, character);
    }

    private void OnStopInteraction()
    {
        EventAfterExecute -= OnStopInteraction;
        m_Interactive?.Stop();
    }
}
