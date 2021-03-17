using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerLook))]
public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float raycastDistance = 3.5f;

    private PlayerLook playerLook;
    private Moveable currentMoveable = null;

    private void Start()
    {
        this.playerLook = this.GetComponent<PlayerLook>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = new Ray(this.playerLook.currentCamera.transform.position, this.playerLook.currentCamera.transform.forward);
            if(Physics.Raycast(ray, out RaycastHit hit, this.raycastDistance, this.interactLayer))
            {
                if(NetworkClient.isConnected)
                    this.CmdTryUse(hit.transform.gameObject, this.gameObject);
                else
                    this.TryUse(hit.transform.gameObject, this.gameObject);

                if(this.currentMoveable == null)
                {
                    Moveable moveable = hit.transform.GetComponent<Moveable>();
                    if(moveable != null)
                    {
                        this.currentMoveable = moveable;

                        if(NetworkClient.isConnected)
                            this.CmdPickupMoveable(this.currentMoveable.gameObject, this.gameObject);
                        else
                            this.PickupMoveable(this.currentMoveable.gameObject, this.gameObject);     
                    }
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            if(this.currentMoveable != null)
            {
                if(NetworkClient.isConnected)
                    this.CmdDrop(this.currentMoveable.gameObject);
                else
                    this.Drop(this.currentMoveable.gameObject);

                this.currentMoveable = null;
            }
        }
    }

    private void TryUse(GameObject sceneObject, GameObject interactor)
    {
        Door door = sceneObject.transform.GetComponent<Door>();
        if(door != null)
            door.Interact();

        ButtonInteraction button = sceneObject.transform.GetComponent<ButtonInteraction>();
        if(button != null)
            button.Interact();
    }

    [Command]
    private void CmdTryUse(GameObject sceneObject, GameObject interactor)
    {
        this.RpcTryUse(sceneObject, interactor);
    }

    [ClientRpc]
    private void RpcTryUse(GameObject sceneObject, GameObject interactor)
    {
        this.TryUse(sceneObject, interactor);
    }

    private void PickupMoveable(GameObject moveableObj, GameObject interactor)
    {
        moveableObj.GetComponent<Moveable>().Pickup(interactor.GetComponent<PlayerLook>().currentCamera.transform,
            Vector3.zero, true, true, interactor.GetComponent<PlayerInteract>().OnMoveablePickedUpByOther);
    }

    [Command]
    private void CmdPickupMoveable(GameObject moveableObj, GameObject interactor)
    {
        this.RpcPickupMoveable(moveableObj, interactor);
    }

    [ClientRpc]
    private void RpcPickupMoveable(GameObject moveableObj, GameObject interactor)
    {
        this.PickupMoveable(moveableObj, interactor);
    }

    private void Drop(GameObject moveableObj)
    {
        moveableObj.transform.GetComponent<Moveable>().Drop();
    }

    [Command]
    void CmdDrop(GameObject moveableObj)
    {
        this.RpcDrop(moveableObj);
    }

    [ClientRpc]
    void RpcDrop(GameObject moveableObj)
    {
        this.Drop(moveableObj);
    }

    private void OnMoveablePickedUpByOther()
    {
        this.currentMoveable = null;
    }
}
