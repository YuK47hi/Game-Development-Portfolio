using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    public Transform cameraTransform;

    // 視点データの構造体
    private struct CameraView
    {
        public Vector3 position;
        public Vector3 rotation;
        public string name;

        public CameraView(Vector3 pos, Vector3 rot, string viewName)
        {
            position = pos;
            rotation = rot;
            name = viewName;
        }
    }

    private CameraView[] views;
    private int currentViewIndex = 0;

    void Start()
    {
        if (cameraTransform == null && GetComponentInChildren<Camera>() != null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }

        // ここにカメラの視点をいくつでも追加できます！
        views = new CameraView[]
        {
            new CameraView(new Vector3(0f, 2.5f, -6f), new Vector3(15f, 0f, 0f), "三人称（背後）"),
            new CameraView(new Vector3(0f, 4.5f, -9f), new Vector3(20f, 0f, 0f), "三人称（引き）"),
            new CameraView(new Vector3(0f, 0.5f, 0.5f), Vector3.zero, "一人称（コックピット）")
        };

        ApplyCurrentView();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) ||
            (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame))
        {
            // Cキーを押すたびに次の視点へ進む（最後の次は最初に戻る）
            currentViewIndex = (currentViewIndex + 1) % views.Length;
            ApplyCurrentView();
        }
    }

    void ApplyCurrentView()
    {
        if (cameraTransform == null) return;
        cameraTransform.localPosition = views[currentViewIndex].position;
        cameraTransform.localRotation = Quaternion.Euler(views[currentViewIndex].rotation);
    }
}