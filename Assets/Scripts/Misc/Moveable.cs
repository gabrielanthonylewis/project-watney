using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Moveable : NetworkBehaviour
{
    private Vector3 offset = Vector3.zero;
    private bool shouldOffsetForward = false;
    private bool usePhysics = false;
    private Transform interactor = null;
    private new Rigidbody rigidbody = null;
    private UnityEvent pickedUpByOtherCallback = new UnityEvent();

    private void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(this.interactor == null)
            return;

        // If not using physics (or can't) set position directly.
        if(!this.usePhysics || this.rigidbody == null)
            this.transform.position = this.GetPositionRelativeToTarget();
    }

    private void FixedUpdate()
    {
        if(this.interactor == null)
            return;

        // If using physics and can, use rigibody.
        if(this.usePhysics && this.rigidbody != null)
        {
            this.rigidbody.velocity = Vector3.zero;
            this.rigidbody.MovePosition(this.GetPositionRelativeToTarget());
        }
    }

    private Vector3 GetPositionRelativeToTarget()
    {
        Vector3 newPosition = this.interactor.position + this.offset;
        if(this.shouldOffsetForward)
            newPosition += this.interactor.forward;

        return newPosition;
    }

    public void Pickup(Transform interactor, Vector3 offset, bool shouldOffsetForward = false,
        bool usePhysics = false, UnityAction callback = null)
    {
        if(interactor == null)
            return;

        if(this.interactor != interactor && 
            this.pickedUpByOtherCallback != null)
        {
            this.pickedUpByOtherCallback.Invoke();
            this.pickedUpByOtherCallback = new UnityEvent();
        }

        this.interactor = interactor;
        this.offset = offset;
        this.shouldOffsetForward = shouldOffsetForward;
        this.usePhysics = usePhysics;

        if(this.rigidbody != null)
        {
            this.rigidbody.velocity = Vector3.zero;
            this.rigidbody.isKinematic = !this.usePhysics;
        }

        if(callback != null)
            this.pickedUpByOtherCallback.AddListener(callback);
    }

    public void Drop()
    {
        this.interactor = null;
        this.pickedUpByOtherCallback = new UnityEvent();
    } 
}
