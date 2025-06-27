using System.Collections;
using UnityEngine;

// 오른손 컨트롤러에서 나가는 레이
public class RayVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public LineRenderer Ray;            // 라인 렌더러
    public LayerMask HitRayMask;        // 반응할 레이어 마스크
    public float Distance = 100f;       // 라인 최대 거리

    [Header("Reticle Point")]
    public GameObject ReticlePoint;     // 히트시 그릴 포인트
    public bool ShowReticle = true;

    private void Awake()
    {
        On();
    }

    public void On()
    {
        StopAllCoroutines();            // 모든 코루틴을 멈추고,
        StartCoroutine(Process());      // Process 코루틴을 시작한다.
    }

    public void Off()
    {
        StopAllCoroutines();            // 코루틴, 레이, ReticlePoint를 모두 비활성화
        Ray.enabled = false;
        ReticlePoint.SetActive(false);
    }

    /*********************************************************************************************
    함수: Process
    기능: 오른손에서 시작하는 레이를 그림
    *********************************************************************************************/
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
                ReticlePoint.SetActive(ShowReticle);                    // 닿은 지점에 크로스헤어를 그림
                ReticlePoint.transform.LookAt(Camera.main.transform);   // ReticlePoint == 크로스헤어가 항상 카메라를 바라보도록 함
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
