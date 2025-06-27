using UnityEngine;

// 소지 중인 게임 오브젝트가 항상 플레이어 카메라를 바라보도록 한다
public class RotateToPlayer : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform; // 메인 카메라의 위치를 찾음
        }
        else
        {
            Debug.LogWarning("Main Camera가 설정되지 않았습니다. 'MainCamera' 태그를 확인하세요.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 카메라를 바라보되, Z축 회전은 유지 (UI 요소가 옆으로 눕지 않게)
        Vector3 direction = mainCamera.position - transform.position;
        direction.y = 0; // 수직 회전 방지 (필요 없으면 제거)
        transform.forward = -direction.normalized;
    }
}