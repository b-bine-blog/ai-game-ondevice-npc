// 미니맵 켜기 / 끄기

using UnityEngine;

public class MinimapToggle : MonoBehaviour
{
    [Header("UI 오브젝트 연결")]
    public GameObject smallMapUI;
    public GameObject largeMapUI;
    public GameObject dialogueUI;
    public GameObject chatBackgroundUI; // 추가된 부분: ChatBackground 연결

    [Header("카메라 설정")]
    public Camera minimapCamera;
    public float smallZoom = 140f;
    public float largeZoom = 500f;

    private bool isMapOpen = false;

    void Start()
    {
        if (largeMapUI != null) largeMapUI.SetActive(false);
        if (smallMapUI != null) smallMapUI.SetActive(true);
        if (dialogueUI != null) dialogueUI.SetActive(true);
        if (chatBackgroundUI != null) chatBackgroundUI.SetActive(true); // 초기 상태 설정

        if (minimapCamera != null) minimapCamera.orthographicSize = smallZoom;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;

        // 1. 지도 UI 교체
        if (largeMapUI != null) largeMapUI.SetActive(isMapOpen);
        if (smallMapUI != null) smallMapUI.SetActive(!isMapOpen);

        // 2. 대화창 및 배경 숨기기/보이기
        if (dialogueUI != null) dialogueUI.SetActive(!isMapOpen);
        if (chatBackgroundUI != null) chatBackgroundUI.SetActive(!isMapOpen); // 추가된 부분: 상태 토글

        // 3. 카메라 줌 조절
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = isMapOpen ? largeZoom : smallZoom;
        }
    }
}
