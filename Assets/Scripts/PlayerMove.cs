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
        if (!GameManager.Instance.isGameStarted) return; // ������ ���۵��� �ʾ����� ī�޶� �̵��� ����

        // �޼� x��ư, pc ���콺 ����
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch)) 
        {
            // ���� ī�޶�� �̵�
            currentCameraIndex = (currentCameraIndex + 1) % camerasToMove.Length;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move next camera, current camera index: {currentCameraIndex}");
        }
        // �޼� y��ư, pc �����̽���
        else if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch))
        {
            // ���� ī�޶�� �̵�
            currentCameraIndex = (currentCameraIndex - 1 + camerasToMove.Length) % camerasToMove.Length;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move previous camera, current camera index: {currentCameraIndex}");
        }
        // �޼� �ε��� Ʈ����, pc �ٹ�ư
        else if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch)) 
        {
            // Home Camera�� �̵�
            currentCameraIndex = 0;
            MoveToCamera(currentCameraIndex);
            //Debug.Log($"move home camera, current camera index: {currentCameraIndex}");
        }
    }


    /*********************************************************************************************
    �Լ�: MoveToCamera
    ���: �Ű������� ������ ī�޶��� ��ġ�� �÷��̾ �̵�
    �Ű�����
     - index: �̵��ϴ� ī�޶��� ��ȣ
    *********************************************************************************************/
    private void MoveToCamera(int index)
    {
        if (camerasToMove == null || camerasToMove.Length == 0 || camerasToMove[index] == null)
            return;

        transform.position = camerasToMove[index].transform.position;
        //transform.rotation = camerasToMove[index].transform.rotation;
    }
}