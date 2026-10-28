// 마우스 방향으로 총알 발사

using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab; // 위에서 만든 Bullet 프리팹 연결
    public Transform firePoint;     // 총구 위치 (여기를 발사 지점으로 사용합니다)

    [Header("사운드 설정")]
    public AudioSource audioSource; // 소리를 낼 스피커
    public AudioClip bulletSound;   // 빵! 하는 총소리 오디오 파일

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 마우스 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 발사 지점 설정 (firePoint가 할당되어 있다면 그 위치를 쓰고, 없으면 기존처럼 플레이어 몸에서 발사)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        // 발사 지점으로부터 마우스 방향 계산
        Vector2 direction = ((Vector2)mousePos - (Vector2)spawnPosition).normalized;

        // 총알이 날아갈 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 수정된 부분: 플레이어 위치(transform.position) 대신 spawnPosition에서 생성
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, angle));

        // ⭐ 총알이 생성될 때 총소리 재생! ⭐
        if (audioSource != null && bulletSound != null)
        {
            audioSource.PlayOneShot(bulletSound);
        }
    }
}
