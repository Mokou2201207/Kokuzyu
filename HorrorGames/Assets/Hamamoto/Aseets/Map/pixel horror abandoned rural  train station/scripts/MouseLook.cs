using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace littleDog
{
    public class MouseLook : NetworkBehaviour
    {
        public float mouseSensitivity = 2f;
        public Transform PlayerBody;
        float Xrot = 0f;
        public static bool CanMove = true;
        
        void Start()
        {
            if (!isLocalPlayer) return; // 自分以外のカメラ設定は無視
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isLocalPlayer) return; // 自分が操作するキャラ以外は無視
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CanMove = !CanMove;
                if(CanMove == false)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    // Time.timeScale = 0;
                }else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    // Time.timeScale = 1;
                }
  
            }
            if (CanMove == false) return;
            float mousex = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            Xrot -= mouseY;
            Xrot = Mathf.Clamp(Xrot, -90, 90);
            transform.localRotation = Quaternion.Euler(Xrot, 0f, 0f);
            PlayerBody.Rotate(Vector3.up * mousex);
        }
    }
}
