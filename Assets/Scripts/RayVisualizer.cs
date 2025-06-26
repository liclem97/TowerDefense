using System.Collections;
using UnityEngine;

public class RayVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public LineRenderer Ray;
    public LayerMask HitRayMask;
    public float Distance = 100f;

    [Header("Reticle Point")]
    public GameObject ReticlePoint;
    public bool ShowReticle = true;

    private void Awake()
    {
        On();
    }

    public void On()
    {
        StopAllCoroutines();
        StartCoroutine(Process());
    }

    public void Off()
    {
        StopAllCoroutines();
        Ray.enabled = false;
        ReticlePoint.SetActive(false);
    }

    private IEnumerator Process()
    {
        while (true)
        {
            // LineTrace: 시작 위치, 방향, HitResult, 거리, Layer
            if (Physics.Raycast(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection, out RaycastHit HitInfo, Distance, HitRayMask))
            {
                Ray.SetPosition(1, transform.InverseTransformPoint(HitInfo.point)); // 라인의 끝 점을 HitInfo.Point로 지정하되 월드 좌표 -> 로컬 좌표로
                Ray.enabled = true;

                ReticlePoint.transform.position = HitInfo.point;
                ReticlePoint.SetActive(ShowReticle); // 닿은 지점에 빨간 네모를 그림
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
