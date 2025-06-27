using System.Collections;
using UnityEngine;

// ������ ��Ʈ�ѷ����� ������ ����
public class RayVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public LineRenderer Ray;            // ���� ������
    public LayerMask HitRayMask;        // ������ ���̾� ����ũ
    public float Distance = 100f;       // ���� �ִ� �Ÿ�

    [Header("Reticle Point")]
    public GameObject ReticlePoint;     // ��Ʈ�� �׸� ����Ʈ
    public bool ShowReticle = true;

    private void Awake()
    {
        On();
    }

    public void On()
    {
        StopAllCoroutines();            // ��� �ڷ�ƾ�� ���߰�,
        StartCoroutine(Process());      // Process �ڷ�ƾ�� �����Ѵ�.
    }

    public void Off()
    {
        StopAllCoroutines();            // �ڷ�ƾ, ����, ReticlePoint�� ��� ��Ȱ��ȭ
        Ray.enabled = false;
        ReticlePoint.SetActive(false);
    }

    /*********************************************************************************************
    �Լ�: Process
    ���: �����տ��� �����ϴ� ���̸� �׸�
    *********************************************************************************************/
    private IEnumerator Process()
    {
        while (true)
        {
            // LineTrace: ���� ��ġ, ����, HitResult, �Ÿ�, Layer
            if (Physics.Raycast(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection, out RaycastHit HitInfo, Distance, HitRayMask))
            {
                Ray.SetPosition(1, transform.InverseTransformPoint(HitInfo.point)); // ������ �� ���� HitInfo.Point�� �����ϵ� ���� ��ǥ -> ���� ��ǥ��
                Ray.enabled = true;

                ReticlePoint.transform.position = HitInfo.point;
                ReticlePoint.SetActive(ShowReticle);                    // ���� ������ ũ�ν��� �׸�
                ReticlePoint.transform.LookAt(Camera.main.transform);   // ReticlePoint == ũ�ν��� �׻� ī�޶� �ٶ󺸵��� ��
            }
            else
            {
                Ray.enabled = false;
                ReticlePoint.SetActive(false);
            }

            yield return null;
        }
    }
}
