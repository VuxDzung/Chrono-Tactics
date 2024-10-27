using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandInputManager : CoreNetworkBehaviour
{
    [SerializeField] private KeyCode leftMouseInput = KeyCode.Mouse0;
    [SerializeField] private KeyCode rightMouseInput = KeyCode.Mouse1;
    [SerializeField] private KeyCode tabInput = KeyCode.Tab;
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    public readonly SyncVar<bool> LockInput = new SyncVar<bool>();

    public bool LeftMouseDown => !LockInput.Value && Input.GetKeyDown(leftMouseInput);
    public bool RightMouseDown => !LockInput.Value && Input.GetKeyDown(rightMouseInput);
    public bool Tab => !LockInput.Value && Input.GetKeyDown(tabInput);

    public Vector2 MoveCameraInput { get; private set; }

    public override void OnClientUpdate()
    {
        base.OnClientUpdate();
        if (IsOwner)
        {
            if (LockInput.Value) return;

            Vector2 _moveCamInput = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));
            MoveCameraInput = _moveCamInput;
        }
    }
}