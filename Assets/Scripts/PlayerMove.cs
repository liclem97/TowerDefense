using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerMove : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera[] camerasToMove;

    public int currentCameraIndex = 0;

    private void Start()
    {
        if (camerasToMove.Length > 0)
        {
            MoveToCamera(0);
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.isGameStarted) return; // 게임이 시작되지 않았으면 카메라 이동을 막음

        // 왼손 x버튼, pc 마우스 왼쪽
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch)) 
        {
            // 다음 카메라로 이동
            currentCameraIndex = (currentCameraIndex + 1) % camerasToMove.Length;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move next camera, current camera index: {currentCameraIndex}");
        }
        // 왼손 y버튼, pc 스페이스바
        else if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch))
        {
            // 이전 카메라로 이동
            currentCameraIndex = (currentCameraIndex - 1 + camerasToMove.Length) % camerasToMove.Length;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move previous camera, current camera index: {currentCameraIndex}");
        }
        // 왼손 인덱스 트리거, pc 휠버튼
        else if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch)) 
        {
            // Home Camera로 이동
            currentCameraIndex = 0;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move home camera, current camera index: {currentCameraIndex}");
        }
    }


    /*********************************************************************************************
    함수: MoveToCamera
    기능: 매개변수로 들어오는 카메라의 위치로 플레이어를 이동
    매개변수
     - index: 이동하는 카메라의 번호
    *********************************************************************************************/
    private void MoveToCamera(int index)
    {
        if (camerasToMove == null || camerasToMove.Length == 0 || camerasToMove[index] == null)
            return;

        transform.position = camerasToMove[index].transform.position;
        //transform.rotation = camerasToMove[index].transform.rotation;
    }
}