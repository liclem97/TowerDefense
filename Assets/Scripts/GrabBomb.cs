using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// VR 폭탄 그랩 &amp; 투척 로직.
/// • 근거리/원거리 그랩 지원
/// • 투척 직전 예측 궤적(LineRenderer) 표시
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GrabBomb : MonoBehaviour
{
    #region === Inspector ===
    [Header("Grab Settings")]
    [SerializeField] private LayerMask grabbableLayer;      // 잡을 레이어
    [SerializeField] private float nearGrabRadius = 0.2f;
    [SerializeField] private bool enableRemoteGrab = true;
    [SerializeField] private float remoteGrabDist = 20f;

    [Header("Throw Settings")]
    [SerializeField] private float throwPower = 10f;        // 던질 힘 (m/s)

    [Header("Trajectory Settings")]
    [SerializeField] private int trajSegments = 50;     // 궤적 샘플 수
    [SerializeField] private float trajTimeStep = 0.02f;  // Δt
    [SerializeField] private LineRenderer trajLine;         // 궤적 라인 (optional)
    [SerializeField] private LayerMask terrainLayer;        // 라인이 닿을 때 멈출 레이어

    [Header("Visual Settings")]
    [SerializeField, Range(0f, 1f)] private float grabbedAlpha = 0.7f;

    [Header("Crosshair")]
    [SerializeField] private Transform crosshair;
    [SerializeField] private Gun gunScript;
    #endregion

    private bool isGrabbing;
    private GameObject grabbedObj;
    private Vector3 previousHandPos;
    private Vector3 handVelocity;

    private void Awake()
    {
        // 임의로 연결 안 했을 경우, 자신의 LineRenderer 사용
        if (trajLine == null)
            trajLine = GetComponent<LineRenderer>();
        trajLine.positionCount = 0;
        trajLine.enabled = false;
    }

    private void Update()
    {
        if (!isGrabbing)
        {
            TryGrab();
            //if (gunScript.shouldDrawCrosshair == false)
            //{
            //    gunScript.shouldDrawCrosshair = true;  // 잡지 않고 있을 때 총의 크로스헤어를 켬
            //}
        }
        else
        {
            ShowTrajectory();
            TryRelease();
            //if (gunScript.shouldDrawCrosshair == true)
            //{
            //    gunScript.shouldDrawCrosshair = false;   // 잡고 있을 때 총의 크로스헤어를 끔
            //}
        }
    }
    private void FixedUpdate()
    {
        Vector3 current = ARAVRInput.RHandPosition;
        handVelocity = (current - previousHandPos) / Time.fixedDeltaTime;
        previousHandPos = current;
    }

    #region === Grab Logic ===
    private void TryGrab()
    {
        if (!ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch))
            return;

        // 1) 원거리 그랩 --------------------------------------------------
        if (enableRemoteGrab)
        {
            var ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
            if (Physics.SphereCast(ray, 0.5f, out var hit, remoteGrabDist, grabbableLayer))
            {
                grabbedObj = hit.transform.gameObject;
                StartCoroutine(MoveToHandCoroutine());
                isGrabbing = true;
                return;
            }
        }

        // 2) 근거리 그랩 --------------------------------------------------
        var hits = Physics.OverlapSphere(ARAVRInput.RHandPosition, nearGrabRadius, grabbableLayer);
        if (hits.Length == 0) return;

        Transform closest = hits[0].transform;
        float minSqr = (closest.position - ARAVRInput.RHandPosition).sqrMagnitude;
        foreach (var h in hits)
        {
            float sqr = (h.transform.position - ARAVRInput.RHandPosition).sqrMagnitude;
            if (sqr < minSqr) { closest = h.transform; minSqr = sqr; }
        }

        grabbedObj = closest.gameObject;
        AttachToHandImmediate();
        isGrabbing = true;
    }

    private void TryRelease()
    {
        if (!ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch))
            return;

        isGrabbing = false;
        trajLine.enabled = false;
        trajLine.positionCount = 0;

        var rb = grabbedObj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = handVelocity + ARAVRInput.RHandDirection * throwPower;

        grabbedObj.transform.parent = null;
        grabbedObj = null;
    }
    #endregion

    #region === Trajectory Preview ===
    private void ShowTrajectory()
    {
        if (trajLine == null) return;

        trajLine.enabled = true;                                // 라인 활성화
        Vector3 pos = ARAVRInput.RHandPosition;                 // 목표 위치는 오른손 위치
        Vector3 vel = ARAVRInput.RHandDirection * throwPower;   // 방향은 오른손 방향 * 던지는 힘

        trajLine.positionCount = 0;
        int index = 0;

        for (int i = 0; i < trajSegments; ++i)
        {
            Vector3 next = pos + vel * trajTimeStep;
            if (Physics.Linecast(pos, next, out RaycastHit hit, terrainLayer))  // 라인이 Terrain에 닿으면 뒤의 라인을 더 이상 그리지 않음
            {
                trajLine.positionCount = index + 1;
                trajLine.SetPosition(index, hit.point);

                if (crosshair != null)
                {
                    crosshair.position = hit.point;
                    crosshair.rotation = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(0, 180f, 0);
                }
                break;
            }

            if (trajLine.positionCount <= index)
                trajLine.positionCount = index + 1;

            trajLine.SetPosition(index, pos);
            pos = next;
            vel += Physics.gravity * trajTimeStep;
            index++;
        }
    }
    #endregion

    #region === Attachment Helpers ===
    private void AttachToHandImmediate() // 근거리 그랩
    {
        var rb = grabbedObj.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        //grabbedObj.transform.SetParent(ARAVRInput.RHand, worldPositionStays: true);
        grabbedObj.transform.position = ARAVRInput.RHandPosition + ARAVRInput.RHandDirection * 0.1f;
        grabbedObj.transform.parent = ARAVRInput.RHand;
        SetAlpha(grabbedObj, grabbedAlpha);
    }

    private IEnumerator MoveToHandCoroutine() // 원거리 그랩
    {
        var rb = grabbedObj.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Vector3 startPos = grabbedObj.transform.position;
        Quaternion startRot = grabbedObj.transform.rotation;
        Vector3 targetPos = ARAVRInput.RHandPosition + ARAVRInput.RHandDirection * 0.1f;
        Quaternion targetRot = ARAVRInput.RHand.rotation;

        // 부모를 먼저 설정해야 이상한 위치로 가지 않음
        grabbedObj.transform.SetParent(ARAVRInput.RHand, false);

        const float DURATION = 0.2f;
        float t = 0f;
        while (t < DURATION)
        {
            t += Time.deltaTime;
            float lerp = t / DURATION;
            grabbedObj.transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            grabbedObj.transform.rotation = Quaternion.Slerp(startRot, targetRot, lerp);
            yield return null;           
        }
        SetAlpha(grabbedObj, grabbedAlpha);
    }
    #endregion

    #region === Utility ===
    private static void SetAlpha(GameObject obj, float alpha)
    {
        if (!obj.TryGetComponent<MeshRenderer>(out var rend))
            return;

        Material mat = rend.material; // 인스턴스
        Color c = mat.color; c.a = alpha; mat.color = c;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ARAVRInput.RHandPosition, nearGrabRadius);
    }
#endif
}
