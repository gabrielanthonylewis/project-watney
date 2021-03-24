using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerLook))]
public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float firstPersonRayDistance = 3.5f;
    [SerializeField] private float thirdPersonRayDistance = 5.0f;

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
            Transform cameraTransform = this.playerLook.GetCurrentCamera().transform;
            float rayDistance = (this.playerLook.currentView == PlayerLook.View.FirstPerson)
                ? this.firstPersonRayDistance : this.thirdPersonRayDistance;

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if(Physics.Raycast(ray, out RaycastHit hit, rayDistance, this.interactLayer))
            {
                // Interact.
                if(hit.transform.gameObject.GetComponent<Door>() != null ||
                    hit.transform.gameObject.GetComponent<ButtonInteraction>() != null)
                {
                    if(NetworkClient.isConnected)
                        this.CmdInteract(hit.transform.gameObject, this.gameObject);
                    else
                        this.Interact(hit.transform.gameObject, this.gameObject);
                }

                // Pickup.
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

        // Drop.
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

    #region Interact
    private void Interact(GameObject sceneObject, GameObject interactor)
    {
        Door door = sceneObject.GetComponent<Door>();
        if(door != null)
            door.Interact();

        ButtonInteraction button = sceneObject.GetComponent<ButtonInteraction>();
        if(button != null)
            button.Interact();
    }

    [Command]
    private void CmdInteract(GameObject sceneObject, GameObject interactor)
    {
        this.RpcInteract(sceneObject, interactor);
    }

    [ClientRpc]
    private void RpcInteract(GameObject sceneObject, GameObject interactor)
    {
        this.Interact(sceneObject, interactor);
    }
    #endregion

    #region Pickup
    private void PickupMoveable(GameObject moveableObj, GameObject interactor)
    {
        moveableObj.GetComponent<Moveable>().Pickup(interactor.GetComponent<PlayerLook>().GetCurrentCamera().transform,
            Vector3.zero, true, false, interactor.GetComponent<PlayerInteract>().OnMoveablePickedUpByOther);
    }

    private void OnMoveablePickedUpByOther()
    {
        this.currentMoveable = null;
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
    #endregion

    #region Drop
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
    #endregion
}
