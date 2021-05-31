using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Tutorial2
{
    public class Tutorial2MovementController : NetworkBehaviour
    {
        [SerializeField] float movementSpeed = 5f;
        [SerializeField] CharacterController controller = null;

        Vector2 previousInput;

        Tutorial2Controls controls;
        Tutorial2Controls Controls
        {
            get
            {
                if (controls != null) { return controls; }
                return controls = new Tutorial2Controls();
            }
        }

        public override void OnStartAuthority()
        {
            //base.OnStartAuthority();
            enabled = true;

            Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
            Controls.Player.Move.canceled += ctx => ResetMovement();
        }

        [ClientCallback]
        private void OnEnable() => Controls.Enable();
        [ClientCallback]
        private void OnDisable() => Controls.Disable();
        [ClientCallback]
        private void Update() => Move();

        [Client]
        void SetMovement(Vector2 movement) => previousInput = movement;
        [Client]
        void ResetMovement() => previousInput = Vector2.zero;
        [Client]
        void Move()
        {
            Vector3 right = controller.transform.right;
            Vector3 forward = controller.transform.forward;
            right.y = 0f;
            forward.y = 0f;

            Vector3 movement = right.normalized * previousInput.x + forward.normalized * previousInput.y;
            controller.Move(movement * movementSpeed * Time.deltaTime);
        }
    }

}

