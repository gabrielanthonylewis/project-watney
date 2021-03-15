using UnityEngine;
using Mirror;

public class Moveable : NetworkBehaviour
{
    public delegate void PickedUpDelegate(Transform target);
    private PickedUpDelegate pickedUpCallback;

    private Vector3 offset;
    private bool shouldFollowForward = false;
    private bool usePhysics = false;
    private Transform target = null;
    private new Rigidbody rigidbody = null;

    private void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    public void AddPickedUpCallback(PickedUpDelegate myCallback)
    {
        this.pickedUpCallback += myCallback;
    }

    public void RemovePickedUpCallback(PickedUpDelegate myCallback)
    {
        this.pickedUpCallback -= myCallback;
    }

    public void Follow(Transform targetObj, Vector3 offset, bool shouldFollowForward = false, bool usePhysics = false)
    {
        if (this.rigidbody != null)
        {
            this.rigidbody.velocity = Vector3.zero;
            this.rigidbody.isKinematic = !usePhysics;
        }

        this.usePhysics = usePhysics;
        this.shouldFollowForward = shouldFollowForward;
        this.offset = offset;
        this.target = targetObj;

        this.InvokePickedUpCallback(this.target);
    }

    [ClientRpc]
    public void RpcFollow(uint networkID, Vector3 offset, bool shouldFollowForward, bool usePhysics)
    {
        NetworkIdentity targetNetworkIdentity;
        if(!NetworkIdentity.spawned.TryGetValue(networkID, out targetNetworkIdentity))
        {
            Debug.LogError("Network Identity " + networkID + " wasn't found, returning..");
            return;
        }

        Transform obj;
        // this makes no sense, just a hack to get it working... refactor
        if (targetNetworkIdentity.transform.GetComponent<PlayerLook>() != null)
            obj = targetNetworkIdentity.transform.GetComponent<PlayerLook>().currentCamera.transform;
        else
            obj = targetNetworkIdentity.transform;

        if(obj == null)
        {
            Debug.LogError("Network Identity " + networkID + " has no transform, returning..");
            return;
        }

        if (this.rigidbody != null)
        {
            this.rigidbody.velocity = Vector3.zero;
            this.rigidbody.isKinematic = !usePhysics;
        }

        this.usePhysics = usePhysics;
        this.shouldFollowForward = shouldFollowForward;
        this.offset = offset;
        this.target = obj;

        this.InvokePickedUpCallback(this.target);
    }

    public void InvokePickedUpCallback(Transform target)
    {
        this.pickedUpCallback?.Invoke(target);
    }

    public void Drop()
    {
        this.target = null;
    }

    [ClientRpc]
    public void RpcDrop()
    {
        this.target = null;
    }

    private void Update()
    {
        if(this.target != null)
        {
            if (!this.usePhysics || this.rigidbody == null)
            {
                Vector3 newPosition = this.target.position + this.offset;
                if (shouldFollowForward)
                    newPosition += this.target.forward;

                this.transform.position = newPosition;
            }
        }
    }

    private void FixedUpdate()
    {
        if (this.target != null)
        {
            if (this.usePhysics && this.rigidbody != null)
            {
                Vector3 newPosition = this.target.position + this.offset;
                if (shouldFollowForward)
                    newPosition += this.target.forward;
                this.rigidbody.velocity = Vector3.zero;
                this.rigidbody.MovePosition(newPosition);
            }
        }
    }
}
