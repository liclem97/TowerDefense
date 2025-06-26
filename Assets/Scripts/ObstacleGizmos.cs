using UnityEngine;
using UnityEngine.AI;

public class ObstacleGizmos : MonoBehaviour
{
    [Header("Game Ref")]
    [SerializeField] NavMeshObstacle obstacle;

    private void Reset()              // 컴포넌트 붙일 때 자동 참조
    {
        obstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnDrawGizmos()
    {
        if (obstacle == null || obstacle.shape != NavMeshObstacleShape.Box)
            return;

        // 기즈모 색상 설정
        Gizmos.color = Color.red;

        // 장애물의 위치·회전·스케일을 그대로 적용
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            obstacle.transform.position,
            obstacle.transform.rotation,
            obstacle.transform.lossyScale);

        // 중심과 크기를 기준으로 네모 그리기
        Gizmos.DrawCube(obstacle.center, obstacle.size);

        Gizmos.matrix = prevMatrix;   // 원래 행렬 복원
    }
}
