using UnityEngine;
using UnityEngine.AI;

public class ObstacleGizmos : MonoBehaviour
{
    [Header("Game Ref")]
    [SerializeField] NavMeshObstacle obstacle;

    private void Reset()              // ������Ʈ ���� �� �ڵ� ����
    {
        obstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnDrawGizmos()
    {
        if (obstacle == null || obstacle.shape != NavMeshObstacleShape.Box)
            return;

        // ����� ���� ����
        Gizmos.color = Color.red;

        // ��ֹ��� ��ġ��ȸ������������ �״�� ����
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            obstacle.transform.position,
            obstacle.transform.rotation,
            obstacle.transform.lossyScale);

        // �߽ɰ� ũ�⸦ �������� �׸� �׸���
        Gizmos.DrawCube(obstacle.center, obstacle.size);

        Gizmos.matrix = prevMatrix;   // ���� ��� ����
    }
}
