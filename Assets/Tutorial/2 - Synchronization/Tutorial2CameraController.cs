using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

namespace Tutorial2
{
    public class Tutorial2CameraController : NetworkBehaviour
    {
        [Header("Camra")]
        [SerializeField] Vector2 maxFollowOffset = new Vector2(-1f, 6f);//���󰡴� �Ÿ�
        [SerializeField] Vector2 cameraVelocity = new Vector2(4f, 0.25f);
        [SerializeField] Transform playerTransform = null;
        [SerializeField] Cinemachine.CinemachineVirtualCamera  virtualCamera= null;

        Tutorial2Controls controls;
        Tutorial2Controls Controls
        {
            get
            {
                if (controls != null) { return controls; }
                return controls = new Tutorial2Controls();
            }
        }//controls�� �������� ������� ����

        CinemachineTransposer transposer;

        public override void OnStartAuthority()
        {
            //base.OnStartAuthority();

            transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            virtualCamera.gameObject.SetActive(true);
            enabled = true;

            Controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
        }

        [ClientCallback]//Ŭ���̾�Ʈ�� ȣ��
        private void OnEnable() => Controls.Enable();
        [ClientCallback]
        private void OnDisable() => Controls.Disable();

        void Look(Vector2 lookAxis)
        {
            float deltaTime = Time.deltaTime;

            transposer.m_FollowOffset.y = Mathf.Clamp(
                transposer.m_FollowOffset.y - (lookAxis.y * cameraVelocity.y * deltaTime),
                maxFollowOffset.x,
                maxFollowOffset.y);

            playerTransform.Rotate(0f, lookAxis.x * cameraVelocity.x * deltaTime, 0f);
        }
    }
}
