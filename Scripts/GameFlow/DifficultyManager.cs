// 3지역 동적 난이도(DDA) 관리자
// - 호감도 구간에 따라 몹 수 · 플레이어 이동속도 · 장애물(벽) 생성을 바꿈

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using TMPro;

public class DifficultyManager : MonoBehaviour
{
    [Header("연결할 오브젝트")]
    public GameObject obstacleGroup;
    public Transform player;
    public Transform cameraTarget;

    [Header("시네머신 카메라")]
    public CinemachineCamera cinemachineCamera;

    [Header("UI 설정")]
    public TextMeshProUGUI firewallText;
    public float typingSpeed = 0.1f;

    [Header("우호도 -2 전용 고정 좌표 설정")]
    public Vector2 cinematicStartPos = new Vector2(-41f, 12f);

    [Header("연출 설정")]
    public float cinematicDuration = 3.5f;
    public float cameraReturnSpeed = 2.5f;

    [Header("★ 테스트 설정 (실시간 조절)")]
    public bool useTestScore = false;
    [Range(-5, 10)] public int testFriendship = 0;

    private bool isCinematicPlaying = false;
    private int lastTestFriendship = -999;

    IEnumerator TypingEffect(string text)
    {
        firewallText.text = "";
        foreach (char c in text)
        {
            firewallText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void Start()
    {
        if (obstacleGroup != null) obstacleGroup.SetActive(false);
        if (firewallText != null) firewallText.gameObject.SetActive(false);
        ApplyDifficulty();
    }

    void Update()
    {
        if (useTestScore)
        {
            if (testFriendship != lastTestFriendship)
            {
                lastTestFriendship = testFriendship;
                FriendshipSceneChanger.globalFriendship = testFriendship;
                ApplyDifficulty();
            }
        }
    }

    void LateUpdate()
    {
        if (!isCinematicPlaying && player != null && cameraTarget != null)
        {
            cameraTarget.position = new Vector3(player.position.x, player.position.y, cameraTarget.position.z);
        }
    }

    public void ApplyDifficulty()
    {
        int hostilityValue = FriendshipSceneChanger.globalFriendship;

        // 0. 플레이어 속도 변경 로직 (PlayerController 스크립트가 필요합니다)
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                if (hostilityValue <= -2)
                    controller.maxSpeed = 30f;    // -2 이하: 속도 감소
                else if (hostilityValue >= 6 && hostilityValue <= 9)
                    controller.maxSpeed = 70f;    // 6~9: 속도 증가
                else
                    controller.maxSpeed = 50f;    // 그 외: 기본 속도
            }
        }

        // 1. 몹 설정 및 연출 로직
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy").OrderBy(go => go.name).ToArray();
        foreach (GameObject enemy in allEnemies) enemy.SetActive(false);

        int activeCount = 0;

        if (hostilityValue <= -2)
        {
            activeCount = allEnemies.Length;
            if (obstacleGroup != null && !isCinematicPlaying)
            {
                StopAllCoroutines();
                StartCoroutine(ShowWallsWithCameraWork());
            }
        }
        else if (hostilityValue > -2 && hostilityValue <= 5)
        {
            activeCount = 4;
            if (obstacleGroup != null) obstacleGroup.SetActive(false);
            if (firewallText != null) firewallText.gameObject.SetActive(false);
        }
        else if (hostilityValue >= 6 && hostilityValue <= 9)
        {
            activeCount = 2;
        }
        else
        {
            activeCount = 0;
        }

        for (int i = 0; i < activeCount && i < allEnemies.Length; i++)
        {
            allEnemies[i].SetActive(true);
        }
    }

    // [참고] ShowWallsWithCameraWork 코루틴은 이전과 동일하게 유지됩니다.
    IEnumerator ShowWallsWithCameraWork()
    {
        isCinematicPlaying = true;
        if (firewallText != null)
        {
            firewallText.gameObject.SetActive(true);
            yield return StartCoroutine(TypingEffect("악성 파일 감지, 방화벽 가동"));
            yield return new WaitForSeconds(0.5f);
        }
        CinemachineConfiner2D confiner = null;
        if (cinemachineCamera != null)
        {
            confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null) confiner.enabled = false;
        }
        obstacleGroup.SetActive(true);
        List<Transform> walls = new List<Transform>();
        foreach (Transform child in obstacleGroup.transform)
        {
            child.gameObject.SetActive(false);
            walls.Add(child);
        }
        walls = walls.OrderBy(w => w.position.x).ToList();
        Vector3 startPos = new Vector3(cinematicStartPos.x, cinematicStartPos.y, cameraTarget != null ? cameraTarget.position.z : 0f);
        if (cameraTarget != null) cameraTarget.position = startPos;
        if (walls.Count > 0 && cameraTarget != null)
        {
            Vector3 targetWallPos = new Vector3(walls[0].position.x, walls[0].position.y, cameraTarget.position.z);
            float elapsedTime = 0f;
            while (elapsedTime < cinematicDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / cinematicDuration);
                cameraTarget.position = Vector3.Lerp(startPos, targetWallPos, progress);
                int wallsToActivate = Mathf.FloorToInt(progress * walls.Count);
                for (int i = 0; i < wallsToActivate; i++)
                {
                    int index = walls.Count - 1 - i;
                    if (index >= 0 && index < walls.Count)
                    {
                        walls[index].gameObject.SetActive(true);
                    }
                }
                yield return null;
            }
            foreach (var wall in walls) wall.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.5f);
        if (firewallText != null) firewallText.gameObject.SetActive(false);
        while (Vector3.Distance(cameraTarget.position, player.position) > 0.05f)
        {
            Vector3 playerPos = new Vector3(player.position.x, player.position.y, cameraTarget.position.z);
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, playerPos, Time.deltaTime * cameraReturnSpeed);
            yield return null;
        }
        if (player != null) cameraTarget.position = new Vector3(player.position.x, player.position.y, cameraTarget.position.z);
        if (confiner != null) confiner.enabled = true;
        isCinematicPlaying = false;
    }
}
