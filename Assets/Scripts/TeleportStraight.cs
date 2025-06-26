using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TeleportStraight : MonoBehaviour
{
    /** Anchor **/
    public Transform teleportCircleUI; // 텔레포트를 표시할 UI
    LineRenderer lr; // 선을 그릴 렌더러
    Vector3 originScale = Vector3.one * 0.02f; // 최초의 텔레포트 UI 크기
    /** End Anchor **/

    /** Warp **/
    public bool isWarp = false; // 워프 사용 여부
    public float warpTime = 0.1f; // 워프에 걸리는 시간
    public PostProcessVolume post; // 사용하고 있는 포스트프로세싱 볼륨 컴포넌트
    /** End Warp **/

    private void Start()
    {
        teleportCircleUI.gameObject.SetActive(false); // 시작 시 UI 비활성화
        TryGetComponent<LineRenderer>(out lr); // LineRenderer 컴포넌트 가져오기        
    }

    private void Update()
    {
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            lr.enabled = true; // 왼쪽 컨트롤러의 One 버튼을 누르면 라인 렌더러 활성화
        }
        else if (ARAVRInput.GetUp(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            lr.enabled = false; // 왼쪽 컨트롤러의 One 버튼을 떼면 라인 렌더러 비활성화
            if (teleportCircleUI.gameObject.activeSelf)
            {   
                if (isWarp == false) // 워프 기능이 사용 중이 아닐 때 순간이동
                {
                    GetComponent<CharacterController>().enabled = false; // 캐릭터 컨트롤러 비활성화
                    transform.position = teleportCircleUI.position + Vector3.up; // 텔레포트 위치로 이동
                    GetComponent<CharacterController>().enabled = true; // 캐릭터 컨트롤러 다시 활성화
                }
                else
                {
                    StartCoroutine(nameof(Warp));
                }                
            }

            teleportCircleUI.gameObject.SetActive(false); // 텔레포트 UI 비활성화
        }

        // 왼쪽 컨트롤러의 One버튼을 누르고 있을 때 
        if (ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            Ray ray = new Ray(ARAVRInput.LHandPosition, ARAVRInput.LHandDirection); // 왼쪽 컨트롤러의 위치와 방향으로 레이 생성
            RaycastHit hitInfo;
            int layer = 1 << LayerMask.NameToLayer("Terrain"); // "Terrain" 레이어만 검사
            if (Physics.Raycast(ray, out hitInfo, 200, layer))
            {
                // 부딪힌 지점에 텔레포트 UI 표시
                lr.SetPosition(0, ray.origin); // 시작점은 컨트롤러 위치
                lr.SetPosition(1, hitInfo.point); // 끝점은 부딪힌 지점

                // 텔레포트 UI 활성화 및 위치 설정
                teleportCircleUI.gameObject.SetActive(true); // UI 활성화
                teleportCircleUI.position = hitInfo.point; // UI 위치를 부딪힌 지점으로 설정
                teleportCircleUI.forward = hitInfo.normal; // UI의 방향을 지면의 법선으로 설정
                teleportCircleUI.localScale = originScale * Mathf.Max(1, hitInfo.distance); // UI 크기를 거리에 따라 조정
            }
            else // Terrain에 부딪히지 않았을 때
            {
                // ray 충돌이 발생하지 않으면, 라인 렌더러의 끝점을 200으로 설정
                lr.SetPosition(0, ray.origin); // 시작점은 컨트롤러 위치
                lr.SetPosition(1, ray.origin + ARAVRInput.LHandPosition * 200); // 끝점은 컨트롤러 위치에서 멀리 떨어진 지점
                teleportCircleUI.gameObject.SetActive(false); // UI 비활성화
            }
        }
    }

    IEnumerator Warp()
    {
        MotionBlur blur; //= post.profile.GetSetting<MotionBlur>(); // 워프 느낌을 표현할 모션블러
        Vector3 pos = transform.position; // 워프 시작 지점
        Vector3 targetPos = teleportCircleUI.position + Vector3.up; // 목적지
        float currentTime = 0f; // 워프 경과 시간
        post.profile.TryGetSettings<MotionBlur>(out blur); // 포스트 프로세싱에서 사용중인 프로파일에서 모션블러 가져옴
        blur.active = true; // 워프 시작 전 블러 활성화
        GetComponent<CharacterController>().enabled = false; // 캐릭터의 움직임을 막음

        while (currentTime < warpTime)
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(pos, targetPos, currentTime / warpTime); 
            yield return null;
        }
        transform.position = teleportCircleUI.position + Vector3.up; // 워프 목적지로 이동
        GetComponent<CharacterController>().enabled = true; // 캐릭터 움직임 활성화
        blur.active = false; // 블러 비활성화
    }
}