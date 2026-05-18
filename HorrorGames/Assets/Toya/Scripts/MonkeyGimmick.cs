using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonkeyGimmick : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("プレイヤーのカメラ。指定しない場合はメインカメラが自動設定されます")]
    public Transform playerCamera;
    
    [Tooltip("カメラの前に召喚される猿のプレハブ")]
    public GameObject jumpscareMonkeyPrefab;

    [Header("設定")]
    [Tooltip("視線を外してからギミックが発動するまでに必要なカメラの回転角度")]
    public float requiredRotationAngle = 45f;
    
    [Tooltip("カメラを基準とした召喚位置のズレ（X:左右, Y:上下, Z:前後）")]
    public Vector3 spawnOffset = new Vector3(0, 0, 1.0f);
    
    [Tooltip("召喚された猿が消えるまでの時間（秒）")]
    public float disappearTime = 0.5f;

    [Tooltip("召喚された猿をカメラの子オブジェクトにして追従させるか")]
    public bool followCamera = true;

    // 内部状態
    private bool isPlayerInTrigger = false;
    private bool hasLookedAway = false;
    private bool isTriggered = false;
    private Quaternion rotationWhenLookedAway;
    private Camera cam;
    
    // デバッグ用
    private GameObject debugMonkeyInstance;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (playerCamera != null)
        {
            cam = playerCamera.GetComponent<Camera>();
        }

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
        // デバッグ用: Pキーで猿を表示/非表示を切り替える
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleDebugMonkey();
        }

        // デバッグ用の猿が表示されている間は、インスペクターの値変更に合わせて位置をリアルタイム更新する
        if (debugMonkeyInstance != null && playerCamera != null)
        {
            debugMonkeyInstance.transform.position = playerCamera.TransformPoint(spawnOffset);
            debugMonkeyInstance.transform.rotation = Quaternion.LookRotation(-playerCamera.forward);
        }

        // すでにギミックが発動済み、またはカメラが取得できていない場合は何もしない
        if (isTriggered || cam == null || playerCamera == null) return;

        if (isPlayerInTrigger)
        {
            if (!hasLookedAway)
            {
                // モンキー（このオブジェクト）が視界から外れたかチェック
                if (!IsMonkeyInView())
                {
                    hasLookedAway = true;
                    rotationWhenLookedAway = playerCamera.rotation;
                    Debug.Log("MonkeyGimmick: プレイヤーが視線を外しました。カメラの回転監視を開始します。");
                }
            }
            else
            {
                // 視線を外した後、カメラが指定角度以上回転したか判定
                float angle = Quaternion.Angle(rotationWhenLookedAway, playerCamera.rotation);
                if (angle >= requiredRotationAngle)
                {
                    Debug.Log($"MonkeyGimmick: プレイヤーが {angle:F1} 度傾きました（必要角度: {requiredRotationAngle}度）。");
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
        Debug.Log($"MonkeyGimmick: 【接触テスト】{other.name} がトリガーに触れました！");

        // プレイヤー（またはカメラ）が範囲内に入った
        if (other.CompareTag("Player") || other.GetComponentInChildren<Camera>() != null || other.GetComponentInParent<Camera>() != null)
        {
            isPlayerInTrigger = true;
            hasLookedAway = false; // 状態をリセット
            Debug.Log($"MonkeyGimmick: プレイヤー判定（{other.name}）が範囲内に入りました。");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 範囲外に出た
        if (other.CompareTag("Player") || other.GetComponentInChildren<Camera>() != null)
        {
            isPlayerInTrigger = false;
            hasLookedAway = false;
            Debug.Log("MonkeyGimmick: プレイヤーが範囲外に出ました。ギミックをリセットします。");
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
        else if (jumpscareMonkeyPrefab != null && playerCamera != null)
        {
            // 表示されていない場合は出現させる（時間経過で消えない）
            Vector3 spawnPosition = playerCamera.TransformPoint(spawnOffset);
            Quaternion spawnRotation = Quaternion.LookRotation(-playerCamera.forward);
            
            debugMonkeyInstance = Instantiate(jumpscareMonkeyPrefab, spawnPosition, spawnRotation);
            
            if (followCamera)
            {
                debugMonkeyInstance.transform.SetParent(playerCamera);
            }
            
            Debug.Log("MonkeyGimmick: 【デバッグ】猿を出現させました！（もう一度Pキーで消去）");
        }
    }

    private void TriggerJumpscare()
    {
        isTriggered = true; // 1回だけ発動するようにフラグを立てる

        if (jumpscareMonkeyPrefab != null && playerCamera != null)
        {
            // カメラを基準にした相対位置（オフセット）を計算
            Vector3 spawnPosition = playerCamera.TransformPoint(spawnOffset);
            
            // プレイヤーの方を向かせる（カメラの向きの逆）
            Quaternion spawnRotation = Quaternion.LookRotation(-playerCamera.forward);

            // 猿を召喚
            GameObject spawnedMonkey = Instantiate(jumpscareMonkeyPrefab, spawnPosition, spawnRotation);
            Debug.Log($"MonkeyGimmick: オフセット {spawnOffset} の位置に猿を召喚しました！");
            
            if (followCamera)
            {
                // カメラの子オブジェクトにして、カメラの動きに追従させる
                spawnedMonkey.transform.SetParent(playerCamera);
            }

            // 指定時間後に消去
            Destroy(spawnedMonkey, disappearTime);
        }
        else
        {
            Debug.LogWarning("MonkeyGimmick: JumpscareMonkeyPrefab が設定されていないか、カメラがありません。");
        }
    }

    /// <summary>
    /// シーンビューでのみ表示されるギズモ
    /// </summary>
    private void OnDrawGizmos()
    {
        // トリガーの範囲を可視化（緑色）
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // 半透明の緑
            
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

        // 召喚される位置の目安を描画（カメラがインスペクターにセットされている場合のみ）
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 targetPos = playerCamera.TransformPoint(spawnOffset);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
            Gizmos.DrawLine(playerCamera.position, targetPos);
        }
    }
}
