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
    [SerializeField] private float rotateSpeed = 60f; // ȸ�� �ӵ� (��/��)

    private AudioSource audioSource;
    public int currentCameraIndex = 0;

    private void Start()
    {
        // AudioSource ������Ʈ �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // ù ī�޶� ��ġ�� �̵�
        if (camerasToMove.Length > 0)
        {
            StartCoroutine(MoveToCamera(0));
        }
    }

    private void Update()
    {
        // �޼� �潺ƽ �¿� �Է� (Oculus ����: -1 ~ +1)
        float stickX = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.LTouch);

        if (Mathf.Abs(stickX) > 0.1f) // dead zone ó��
        {
            float rotateAmount = stickX * rotateSpeed * Time.deltaTime;
            transform.Rotate(0f, rotateAmount, 0f); // Y�� ȸ�� (���� ����)
        }

        if (!GameManager.Instance.isGameStarted) return; // ������ ���۵��� �ʾ����� ī�޶� �̵��� ����

        // �޼� X��ư or ���콺 ��Ŭ�� �� ���� ī�޶�
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = (currentCameraIndex + 1) % camerasToMove.Length;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
        // �޼� Y��ư or �����̽��� �� ���� ī�޶�
        else if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = (currentCameraIndex - 1 + camerasToMove.Length) % camerasToMove.Length;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
        // �޼� �ε��� Ʈ���� or �� Ŭ�� �� Ȩ ī�޶�
        else if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch))
        {
            currentCameraIndex = 0;
            StartCoroutine(MoveToCamera(currentCameraIndex));
        }
    }


    /*********************************************************************************************
    �Լ�: MoveToCamera
    ���: �Ű������� ������ ī�޶��� ��ġ�� �÷��̾ �̵�
    �Ű�����
     - index: �̵��ϴ� ī�޶��� ��ȣ
    *********************************************************************************************/
    private IEnumerator MoveToCamera(int index)
    {
        if (camerasToMove == null || camerasToMove.Length == 0 || camerasToMove[index] == null)
            yield break;

        // ������ ȿ�� ���
        if (noiseImage && noiseAudio && audioSource && GameManager.Instance.isGameStarted)
        {
            noiseImage.enabled = true;
            audioSource.clip = noiseAudio;
            audioSource.Play();

            yield return new WaitForSeconds(0.5f);

            noiseImage.enabled = false;
            audioSource.Stop();
        }

        // ī�޶� ��ġ�� �̵�
        transform.position = camerasToMove[index].transform.position;
        // �ʿ� �� ȸ�� ����
        // transform.rotation = camerasToMove[index].transform.rotation;
    }
}