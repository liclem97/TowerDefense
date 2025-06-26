using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 왼손 트리거(버튼 One)를 누르는 동안 포물선을 시뮬레이션해
/// “Teleport” 레이어의 TeleportAnchor를 조준한다.
/// 버튼을 떼면 해당 지점으로 즉시 이동한다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CharacterController))]
public class TeleportCurve : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform teleportCircleUI;     // 목표 지점 표시
    private static readonly Vector3 ORIGIN_SCALE = Vector3.one * 0.02f;

    [Header("Curve")]
    [SerializeField] private int lineSmooth = 40;            // 샘플링 횟수
    [SerializeField] private float curveLength = 50f;        // 초기 속도 계수
    [SerializeField] private float gravity = -60f;           // 곡선 중력
    [SerializeField] private float simulateTime = 0.02f;     // 시뮬레이션 델타 타임

    private LineRenderer lr;
    private CharacterController cc;
    private readonly List<Vector3> points = new();

    private TeleportAnchor currentAnchor;                    // 현재 조준 중인 앵커

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        cc = GetComponent<CharacterController>();

        lr.enabled = false;
        lr.positionCount = 0;
        lr.startWidth = 0.0f;
        lr.endWidth = 0.2f;

        teleportCircleUI.gameObject.SetActive(false);
    }

    void Update()
    {
        bool down = ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);
        bool hold = ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);
        bool up = ARAVRInput.GetUp(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);

        if (down)
        {
            lr.enabled = true; //렌더러 활성화
        }
        else if (up)
        {
            lr.enabled = false; //렌더러 비활성화
            FinishTeleport();
        }
        else if (hold)
        {
            SimulateCurve();
        }
    }
    private void SimulateCurve()
    {
        points.Clear();

        Vector3 dir = ARAVRInput.LHandDirection * curveLength;
        Vector3 pos = ARAVRInput.LHandPosition;
        points.Add(pos);

        TeleportAnchor hitAnchor = null;

        for (int i = 0; i < lineSmooth; ++i)
        {
            Vector3 lastPos = pos;

            dir.y += gravity * simulateTime;   // v = v0 + a·t
            pos += dir * simulateTime;   // p = p0 + v·t

            if (SegmentHit(lastPos, ref pos, out hitAnchor))
            {
                points.Add(pos);
                break;
            }
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        HandleAnchorHighlight(hitAnchor);
    }

    /// <summary>한 구간을 레이캐스트해 충돌·앵커를 감지</summary>
    private bool SegmentHit(Vector3 from, ref Vector3 to, out TeleportAnchor anchor)
    {
        anchor = null;
        Vector3 dir = to - from;

        if (!Physics.Raycast(from, dir.normalized, out var hit, dir.magnitude))
            return false;                                     // 아무것도 안 맞음

        to = hit.point;                                      // 실제 충돌 지점으로 보정

        // Teleport 레이어가 아니면 단순히 막혔다고만 판단
        if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Teleport"))
            return true;

        anchor = hit.transform.GetComponent<TeleportAnchor>();
        if (anchor == null) return true;                     // 레이어만 맞고 앵커가 없음

        //--------------------------------------------------
        // ⬇️ UI 위치·크기 조정
        //--------------------------------------------------
        teleportCircleUI.gameObject.SetActive(true);
        teleportCircleUI.position = hit.point;
        teleportCircleUI.forward = hit.normal;

        float dist = Vector3.Distance(hit.point, ARAVRInput.LHandPosition);
        teleportCircleUI.localScale = ORIGIN_SCALE * Mathf.Max(1f, dist);

        return true;
    }

    /// <summary>조준 대상이 바뀌면 이전‧새 앵커에 알림</summary>
    private void HandleAnchorHighlight(TeleportAnchor newAnchor)
    {
        if (newAnchor != currentAnchor)
        {
            currentAnchor?.OnAnchorUnAimmed();
            currentAnchor = newAnchor;
        }

        if (currentAnchor != null)
            currentAnchor.OnAnchorAimmed();       // 계속 조준 중
        else
            teleportCircleUI.gameObject.SetActive(false);
    }

    private void FinishTeleport()
    {
        teleportCircleUI.gameObject.SetActive(false);

        if (currentAnchor == null) return;

        currentAnchor.OnAnchorUnAimmed();

        // CharacterController는 이동 전·후 잠시 껐다 켜서 충돌 계산 초기화
        cc.enabled = false;
        transform.position = currentAnchor.transform.position + new Vector3(0f, 1f, 0f); // 살짝 위로
        cc.enabled = true;

        currentAnchor = null;
    }
}