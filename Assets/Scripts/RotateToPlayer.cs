using UnityEngine;

// ���� ���� ���� ������Ʈ�� �׻� �÷��̾� ī�޶� �ٶ󺸵��� �Ѵ�
public class RotateToPlayer : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform; // ���� ī�޶��� ��ġ�� ã��
        }
        else
        {
            Debug.LogWarning("Main Camera�� �������� �ʾҽ��ϴ�. 'MainCamera' �±׸� Ȯ���ϼ���.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // ī�޶� �ٶ󺸵�, Z�� ȸ���� ���� (UI ��Ұ� ������ ���� �ʰ�)
        Vector3 direction = mainCamera.position - transform.position;
        direction.y = 0; // ���� ȸ�� ���� (�ʿ� ������ ����)
        transform.forward = -direction.normalized;
    }
}