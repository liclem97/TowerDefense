using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TeleportStraight : MonoBehaviour
{
    /** Anchor **/
    public Transform teleportCircleUI; // �ڷ���Ʈ�� ǥ���� UI
    LineRenderer lr; // ���� �׸� ������
    Vector3 originScale = Vector3.one * 0.02f; // ������ �ڷ���Ʈ UI ũ��
    /** End Anchor **/

    /** Warp **/
    public bool isWarp = false; // ���� ��� ����
    public float warpTime = 0.1f; // ������ �ɸ��� �ð�
    public PostProcessVolume post; // ����ϰ� �ִ� ����Ʈ���μ��� ���� ������Ʈ
    /** End Warp **/

    private void Start()
    {
        teleportCircleUI.gameObject.SetActive(false); // ���� �� UI ��Ȱ��ȭ
        TryGetComponent<LineRenderer>(out lr); // LineRenderer ������Ʈ ��������        
    }

    private void Update()
    {
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            lr.enabled = true; // ���� ��Ʈ�ѷ��� One ��ư�� ������ ���� ������ Ȱ��ȭ
        }
        else if (ARAVRInput.GetUp(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            lr.enabled = false; // ���� ��Ʈ�ѷ��� One ��ư�� ���� ���� ������ ��Ȱ��ȭ
            if (teleportCircleUI.gameObject.activeSelf)
            {   
                if (isWarp == false) // ���� ����� ��� ���� �ƴ� �� �����̵�
                {
                    GetComponent<CharacterController>().enabled = false; // ĳ���� ��Ʈ�ѷ� ��Ȱ��ȭ
                    transform.position = teleportCircleUI.position + Vector3.up; // �ڷ���Ʈ ��ġ�� �̵�
                    GetComponent<CharacterController>().enabled = true; // ĳ���� ��Ʈ�ѷ� �ٽ� Ȱ��ȭ
                }
                else
                {
                    StartCoroutine(nameof(Warp));
                }                
            }

            teleportCircleUI.gameObject.SetActive(false); // �ڷ���Ʈ UI ��Ȱ��ȭ
        }

        // ���� ��Ʈ�ѷ��� One��ư�� ������ ���� �� 
        if (ARAVRInput.Get(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            Ray ray = new Ray(ARAVRInput.LHandPosition, ARAVRInput.LHandDirection); // ���� ��Ʈ�ѷ��� ��ġ�� �������� ���� ����
            RaycastHit hitInfo;
            int layer = 1 << LayerMask.NameToLayer("Terrain"); // "Terrain" ���̾ �˻�
            if (Physics.Raycast(ray, out hitInfo, 200, layer))
            {
                // �ε��� ������ �ڷ���Ʈ UI ǥ��
                lr.SetPosition(0, ray.origin); // �������� ��Ʈ�ѷ� ��ġ
                lr.SetPosition(1, hitInfo.point); // ������ �ε��� ����

                // �ڷ���Ʈ UI Ȱ��ȭ �� ��ġ ����
                teleportCircleUI.gameObject.SetActive(true); // UI Ȱ��ȭ
                teleportCircleUI.position = hitInfo.point; // UI ��ġ�� �ε��� �������� ����
                teleportCircleUI.forward = hitInfo.normal; // UI�� ������ ������ �������� ����
                teleportCircleUI.localScale = originScale * Mathf.Max(1, hitInfo.distance); // UI ũ�⸦ �Ÿ��� ���� ����
            }
            else // Terrain�� �ε����� �ʾ��� ��
            {
                // ray �浹�� �߻����� ������, ���� �������� ������ 200���� ����
                lr.SetPosition(0, ray.origin); // �������� ��Ʈ�ѷ� ��ġ
                lr.SetPosition(1, ray.origin + ARAVRInput.LHandPosition * 200); // ������ ��Ʈ�ѷ� ��ġ���� �ָ� ������ ����
                teleportCircleUI.gameObject.SetActive(false); // UI ��Ȱ��ȭ
            }
        }
    }

    IEnumerator Warp()
    {
        MotionBlur blur; //= post.profile.GetSetting<MotionBlur>(); // ���� ������ ǥ���� ��Ǻ�
        Vector3 pos = transform.position; // ���� ���� ����
        Vector3 targetPos = teleportCircleUI.position + Vector3.up; // ������
        float currentTime = 0f; // ���� ��� �ð�
        post.profile.TryGetSettings<MotionBlur>(out blur); // ����Ʈ ���μ��̿��� ������� �������Ͽ��� ��Ǻ� ������
        blur.active = true; // ���� ���� �� �� Ȱ��ȭ
        GetComponent<CharacterController>().enabled = false; // ĳ������ �������� ����

        while (currentTime < warpTime)
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(pos, targetPos, currentTime / warpTime); 
            yield return null;
        }
        transform.position = teleportCircleUI.position + Vector3.up; // ���� �������� �̵�
        GetComponent<CharacterController>().enabled = true; // ĳ���� ������ Ȱ��ȭ
        blur.active = false; // �� ��Ȱ��ȭ
    }
}