using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerLook))]
public class PlayerInteract : NetworkBehaviour
{
    [SerializeField]
    private LayerMask interactLayer;

    [SerializeField]
    private float raycastDistance = 3.5f;

    private PlayerLook _PlayerLook;

    private Moveable currentMoveable = null;

    private void Start()
    {
        _PlayerLook = this.GetComponent<PlayerLook>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = new Ray(this._PlayerLook.currentCamera.transform.position, this._PlayerLook.currentCamera.transform.forward);
            if(Physics.Raycast(ray, out RaycastHit hit, this.raycastDistance, this.interactLayer))
            {
                if (NetworkClient.isConnected)
                {
                    this.CmdUse(hit.transform.gameObject);
                    this.CmdPickup(hit.transform.gameObject, this.GetComponent<NetworkIdentity>().netId);
                }
                else
                {
                    this.TryUse(hit.transform.gameObject);
                    this.TryPickup(hit.transform.gameObject);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (this.currentMoveable)
            {
                if (NetworkClient.isConnected)
                    this.CmdDrop();
                else
                    this.Drop();
            }
        }
    }

    private void TryUse(GameObject sceneObject)
    {
        Door door = sceneObject.transform.GetComponent<Door>();
        if (door != null)
            door.Interact();

        ButtonInteraction button = sceneObject.transform.GetComponent<ButtonInteraction>();
        if (button != null)
            button.Interact();
    }

    [Command]
    void CmdUse(GameObject sceneObject)
    {
        this.TryUse(sceneObject);
    }

    private void Drop()
    {
        // Do i need this? I think so?
        this.currentMoveable.RemovePickedUpCallback(this.MoveablePickedUp);
        this.currentMoveable.Drop();
        this.currentMoveable = null;
    }

    [Command]
    void CmdDrop()
    {
        this.RpcDrop();
    }

    [ClientRpc]
    void RpcDrop()
    {
        this.Drop();
    }

    void TryPickup(GameObject moveableObject)
    {
        if (this.currentMoveable == null)
        {
            Moveable moveable = moveableObject.transform.GetComponent<Moveable>();
            if (moveable != null)
            {
                this.currentMoveable = moveable;
                this.currentMoveable.AddPickedUpCallback(this.MoveablePickedUp);
                this.currentMoveable.Follow(this.transform.GetComponent<PlayerLook>().currentCamera.transform
                    , Vector3.zero, true, true);
            }
        }
    }

    [Command]
    void CmdPickup(GameObject moveableObject, uint networkID)
    {
        this.RpcPickup(moveableObject, networkID);
    }

    [ClientRpc]
    void RpcPickup(GameObject moveableObject, uint networkID)
    {
        if (this.currentMoveable == null)
        {
            Moveable moveable = moveableObject.transform.GetComponent<Moveable>();
            if (moveable != null)
            {
                this.currentMoveable = moveable;
                this.currentMoveable.AddPickedUpCallback(this.MoveablePickedUp);
                this.currentMoveable.RpcFollow(networkID, Vector3.zero, true, true);
            }
        }
    }

    [Command]
    private void CmdFollowOther(GameObject moveableObject, uint networkID)
    {
        moveableObject.transform.GetComponent<Moveable>().RpcFollow(networkID, Vector3.zero, false, false);
    }

    private void FollowOther(GameObject moveableObject, Transform target)
    {
        moveableObject.transform.GetComponent<Moveable>().Follow(target, Vector3.zero, false, false);
    }

    private void MoveablePickedUp(Transform by)
    {
        if (by != this._PlayerLook.currentCamera.transform)
        {
            this.currentMoveable.RemovePickedUpCallback(this.MoveablePickedUp);

            if (NetworkClient.isConnected)
            {
                if (by.GetComponent<NetworkIdentity>())
                {
                    this.CmdFollowOther(this.currentMoveable.gameObject, by.GetComponent<NetworkIdentity>().netId);
                }
            }
            else
            {
                this.FollowOther(this.currentMoveable.gameObject, by);
            }

            this.currentMoveable = null;
        }
    }
}
