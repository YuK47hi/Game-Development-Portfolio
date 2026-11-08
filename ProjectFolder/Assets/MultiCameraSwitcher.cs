using UnityEngine;
using UnityEngine.InputSystem;

public class MultiCameraSwitcher : MonoBehaviour
{
    public Camera mainCamera;   // 一人称用 (これまでの Main Camera)
    public Camera subCamera2;   // 三人称用 (Main Camera2)

    // 三人称視点の位置バリエーション（ここで数を増やせます）
    private Vector3[] thirdPersonPositions = {
        new Vector3(0f, 2.5f, -6f), // 三人称パターン1（通常背後）
        new Vector3(0f, 4.5f, -9f)  // 三人称パターン2（少し引いた遠景）
    };
    private Quaternion[] thirdPersonRotations = {
        Quaternion.Euler(15f, 0f, 0f),
        Quaternion.Euler(20f, 0f, 0f)
    };

    private int cameraState = 0;
    // 0: Main Camera (一人称)
    // 1: Main Camera2 (三人称・パターン1)
    // 2: Main Camera2 (三人称・パターン2)

    void Start()
    {
        // 自動でカメラが見つからない場合の保険
        if (mainCamera == null)
        {
            GameObject mc = GameObject.Find("Main Camera");
            if (mc != null) mainCamera = mc.GetComponent<Camera>();
        }
        if (subCamera2 == null)
        {
            Transform t = transform.Find("Main Camera2");
            if (t != null) subCamera2 = t.GetComponent<Camera>();
        }

        ApplyCameraState();
    }

    void Update()
    {
        // Cキーを押すたびに状態を切り替える (0 -> 1 -> 2 -> 0 ...)
        if (Input.GetKeyDown(KeyCode.C) ||
            (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame))
        {
            cameraState = (cameraState + 1) % 3;
            ApplyCameraState();
        }
    }

    void ApplyCameraState()
    {
        if (mainCamera == null || subCamera2 == null) return;

        if (cameraState == 0)
        {
            // state 0: 一人称視点（Main Camera を有効、Main Camera2 を無効）
            mainCamera.gameObject.SetActive(true);
            subCamera2.gameObject.SetActive(false);
        }
        else
        {
            // state 1, 2: 三人称視点（Main Camera を無効、Main Camera2 を有効にして位置を変更）
            mainCamera.gameObject.SetActive(false);
            subCamera2.gameObject.SetActive(true);

            int index = cameraState - 1; // 1なら0番目の位置、2なら1番目の位置
            subCamera2.transform.localPosition = thirdPersonPositions[index];
            subCamera2.transform.localRotation = thirdPersonRotations[index];
        }
    }
}