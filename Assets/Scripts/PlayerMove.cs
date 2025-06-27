using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera[] camerasToMove;
    [SerializeField] private Image noiseImage;
    [SerializeField] private AudioClip noiseAudio;
    [SerializeField] private float rotateSpeed = 60f; // 회전 속도 (도/초)

    private AudioSource audioSource;
    public int currentCameraIndex = 0;

    private void Start()
    {
        // AudioSource 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 첫 카메라 위치로 이동
        if (camerasToMove.Length > 0)
        {
            StartCoroutine(MoveToCamera(0));
        }
    }

    private void Update()
    {
        // 왼손 썸스틱 좌우 입력 (Oculus 기준: -1 ~ +1)
        float stickX = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.LTouch);

        if (Mathf.Abs(stickX) > 0.1f) // dead zone 처리
        {
            float rotateAmount = stickX * rotateSpeed * Time.deltaTime;
            transform.Rotate(0f, rotateAmount, 0f); // Y축 회전 (월드 기준)
        }

        if (!GameManager.Instance.isGameStarted) return; // 게임이 시작되지 않았으면 카메라 이동을 막음

        // 왼손 X버튼 or 마우스 좌클릭 → 다음 카메라
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = (currentCameraIndex + 1) % camerasToMove.Length;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
        // 왼손 Y버튼 or 스페이스바 → 이전 카메라
        else if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = (currentCameraIndex - 1 + camerasToMove.Length) % camerasToMove.Length;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
        // 왼손 인덱스 트리거 or 휠 클릭 → 홈 카메라
        else if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = 0;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
    }


    /*********************************************************************************************
    함수: MoveToCamera
    기능: 매개변수로 들어오는 카메라의 위치로 플레이어를 이동
    매개변수
     - index: 이동하는 카메라의 번호
    *********************************************************************************************/
    private IEnumerator MoveToCamera(int index)
    {
        if (camerasToMove == null || camerasToMove.Length == 0 || camerasToMove[index] == null)
            yield break;

        // 노이즈 효과 재생
        if (noiseImage && noiseAudio && audioSource && GameManager.Instance.isGameStarted)
        {
            noiseImage.enabled = true;
            audioSource.clip = noiseAudio;
            audioSource.Play();

            yield return new WaitForSeconds(0.5f);

            noiseImage.enabled = false;
            audioSource.Stop();
        }

        // 카메라 위치로 이동
        transform.position = camerasToMove[index].transform.position;
        // 필요 시 회전 적용
        // transform.rotation = camerasToMove[index].transform.rotation;
    }
}