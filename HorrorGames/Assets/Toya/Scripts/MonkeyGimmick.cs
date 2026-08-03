using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]
public class MonkeyGimmick : NetworkBehaviour
{
    [Header("参照")]
    [Tooltip("プレイヤーのカメラ。指定しない場合はメインカメラが自動設定されます")]
    public Transform playerCamera;
    
    [Tooltip("カメラの前に召喚される猿のプレハブ")]
    public GameObject jumpscareMonkeyPrefab;
    
    [Tooltip("猿が出現した時に鳴らす効果音（SE）")]
    public AudioClip jumpscareSE;

    [Header("設定")]
    [Tooltip("カメラを基準とした召喚位置のズレ（X:左右, Y:上下, Z:前後）")]
    public Vector3 spawnOffset = new Vector3(0, 0, 1.5f);
    
    [Tooltip("召喚された猿が消えるまでの時間（秒）")]
    public float disappearTime = 0.5f;

    [Tooltip("召喚された猿をカメラの子オブジェクトにして追従させるか")]
    public bool followCamera = true;

    // 内部状態
    private bool isPlayerInTrigger = false;
    private bool hasSeenMonkey = false;
    private bool isTriggered = false;
    private Camera cam;
    
    // デバッグ用
    private GameObject debugMonkeyInstance;

    /// <summary>
    /// カメラが未設定の場合にメインカメラから取得を試みる
    /// </summary>
    private bool TrySetupCamera()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (playerCamera != null && cam == null)
        {
            cam = playerCamera.GetComponent<Camera>();
        }

        return playerCamera != null && cam != null;
    }

    /// <summary>
    /// カメラのニアクリップ面とめり込まないように補正した召喚オフセットを取得する
    /// </summary>
    private Vector3 GetCorrectedSpawnOffset()
    {
        Vector3 corrected = spawnOffset;
        if (cam != null)
        {
            // ニアクリップ面の手前0.3m以内にスポーンしないようにする
            float minZ = cam.nearClipPlane + 0.3f;
            if (corrected.z < minZ)
            {
                corrected.z = minZ;
            }
        }
        return corrected;
    }

    void Start()
    {
        TrySetupCamera();

        // トリガーとして設定されているか確認
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("MonkeyGimmick: コライダーの IsTrigger がチェックされていません。自動的にチェックします。");
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // クライアントでなければ処理を行わない
        if (!isClient) return;

        // カメラが未設定ならメインカメラから取得を試みる
        if (playerCamera == null || cam == null)
        {
            TrySetupCamera();
        }

        // このオブジェクトがプレイヤーと顔を合わせないよう常に背を向ける
        if (playerCamera != null)
        {
            Vector3 awayDirection = transform.position - playerCamera.position;
            // 上下に傾いて不自然にならないよう高さを無視する
            awayDirection.y = 0;
            if (awayDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(awayDirection);
            }
        }

        // デバッグ用: Pキーで猿を表示/非表示を切り替える
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleDebugMonkey();
        }

        // デバッグ用の猿が表示されている間は、インスペクターの値変更に合わせて位置をリアルタイム更新する
        if (debugMonkeyInstance != null && playerCamera != null)
        {
            Vector3 correctedOffset = GetCorrectedSpawnOffset();
            debugMonkeyInstance.transform.position = playerCamera.TransformPoint(correctedOffset);
            debugMonkeyInstance.transform.rotation = Quaternion.LookRotation(-playerCamera.forward);
        }

        // すでにギミックが発動済み、またはカメラが取得できていない場合は何もしない
        if (isTriggered || cam == null || playerCamera == null) return;

        if (isPlayerInTrigger)
        {
            if (!hasSeenMonkey)
            {
                // まだ視界に入れていない場合、視界に入るのを待つ
                if (IsMonkeyInView())
                {
                    hasSeenMonkey = true;
                    Debug.Log("MonkeyGimmick: プレイヤーが猿を視界に入れました！目を離すと発動します。");
                }
            }
            else
            {
                // 一度視界に入れた後、画面から見えなくなったら即座に発動
                if (!IsMonkeyInView())
                {
                    Debug.Log("MonkeyGimmick: プレイヤーが猿から目を離しました！ギミック発動！");
                    TriggerJumpscare();
                }
            }
        }
    }

    /// <summary>
    /// オブジェクトがカメラの視界内にあるか判定する
    /// </summary>
    private bool IsMonkeyInView()
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        
        // z > 0 はカメラの前方にあることを意味する
        // x, y が 0~1 の範囲内であれば画面内に映っていると判定
        bool inViewport = viewportPoint.z > 0 &&
                          viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                          viewportPoint.y >= 0 && viewportPoint.y <= 1;

        return inViewport;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ローカルプレイヤーが範囲内に入った場合のみ処理する
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>() ?? other.GetComponentInParent<NetworkIdentity>();
        if (identity != null && identity.isLocalPlayer)
        {
            isPlayerInTrigger = true;
            // 状態をリセット
            hasSeenMonkey = false;
            Debug.Log($"MonkeyGimmick: ローカルプレイヤー（{other.name}）が範囲内に入りました。");

            // カメラが未設定ならトリガーに入ったオブジェクトからカメラの取得を試みる
            if (playerCamera == null || cam == null)
            {
                Camera foundCam = other.GetComponentInChildren<Camera>() ?? other.GetComponentInParent<Camera>();
                if (foundCam != null)
                {
                    playerCamera = foundCam.transform;
                    cam = foundCam;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ローカルプレイヤーが範囲外に出た場合のみ処理する
        NetworkIdentity identity = other.GetComponent<NetworkIdentity>() ?? other.GetComponentInParent<NetworkIdentity>();
        if (identity != null && identity.isLocalPlayer)
        {
            isPlayerInTrigger = false;
            hasSeenMonkey = false;
            Debug.Log("MonkeyGimmick: ローカルプレイヤーが範囲外に出ました。ギミックをリセットします。");
        }
    }

    private void ToggleDebugMonkey()
    {
        if (debugMonkeyInstance != null)
        {
            // 既に表示されている場合は消す
            Destroy(debugMonkeyInstance);
            Debug.Log("MonkeyGimmick: 【デバッグ】猿を消去しました。");
        }
        else if (jumpscareMonkeyPrefab != null)
        {
            // カメラが未設定ならメインカメラの取得を試みる
            if (playerCamera == null || cam == null)
            {
                TrySetupCamera();
            }

            if (playerCamera != null)
            {
                // 表示されていない場合は出現させる
                Vector3 correctedOffset = GetCorrectedSpawnOffset();
                Vector3 spawnPosition = playerCamera.TransformPoint(correctedOffset);
                Quaternion spawnRotation = Quaternion.LookRotation(-playerCamera.forward);
                
                debugMonkeyInstance = Instantiate(jumpscareMonkeyPrefab, spawnPosition, spawnRotation);
                
                if (followCamera)
                {
                    debugMonkeyInstance.transform.SetParent(playerCamera);
                }
                
                Debug.Log("MonkeyGimmick: 【デバッグ】猿を出現させました！（もう一度Pキーで消去）");
            }
        }
    }

    private void TriggerJumpscare()
    {
        // 1回だけ発動するようにフラグを立てる
        isTriggered = true;

        if (jumpscareMonkeyPrefab != null && playerCamera != null)
        {
            // カメラを基準にした相対位置を計算
            Vector3 correctedOffset = GetCorrectedSpawnOffset();
            Vector3 spawnPosition = playerCamera.TransformPoint(correctedOffset);
            
            // プレイヤーの方を向かせる
            Quaternion spawnRotation = Quaternion.LookRotation(-playerCamera.forward);

            // 猿を召喚
            GameObject spawnedMonkey = Instantiate(jumpscareMonkeyPrefab, spawnPosition, spawnRotation);
            Debug.Log($"MonkeyGimmick: オフセット {spawnOffset} の位置に猿を召喚しました！");
            
            // SEを再生
            if (jumpscareSE != null)
            {
                AudioSource.PlayClipAtPoint(jumpscareSE, playerCamera.position);
            }
            
            if (followCamera)
            {
                // カメラの子オブジェクトにして、カメラの動きに追従させる
                spawnedMonkey.transform.SetParent(playerCamera);
            }

            // 指定時間後に消去
            Destroy(spawnedMonkey, disappearTime);
            
            // サーバーにオブジェクトの削除を要求する
            CmdDestroyGimmick();
        }
        else
        {
            Debug.LogWarning("MonkeyGimmick: JumpscareMonkeyPrefab が設定されていないか、カメラがありません。");
        }
    }

    /// <summary>
    /// サーバー上でこのオブジェクトを削除するコマンド
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdDestroyGimmick()
    {
        // サーバー側でオブジェクトを削除する
        NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    /// シーンビューでのみ表示されるギズモ
    /// </summary>
    private void OnDrawGizmos()
    {
        // トリガーの範囲を可視化
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // 半透明の緑
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            // カプセルやメッシュコライダーなどは複雑になるため割愛
            
            // 境界のワイヤーフレームも描画して見やすくする
            Gizmos.color = Color.green;
            if (col is BoxCollider boxWire)
            {
                Gizmos.DrawWireCube(boxWire.center, boxWire.size);
            }
            else if (col is SphereCollider sphereWire)
            {
                Gizmos.DrawWireSphere(sphereWire.center, sphereWire.radius);
            }
            
            Gizmos.matrix = Matrix4x4.identity;
        }

        // 召喚される位置の目安を描画
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 correctedOffset = GetCorrectedSpawnOffset();
            Vector3 targetPos = playerCamera.TransformPoint(correctedOffset);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
            Gizmos.DrawLine(playerCamera.position, targetPos);
        }
    }
}
