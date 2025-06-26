using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AmmoItem : MonoBehaviour
{
    [Header("Game Ref")]
    [SerializeField] private Collider itemCollider;
    [SerializeField] private MeshRenderer itemMesh;

    [Header("Stat")]
    [SerializeField] private int amountOfAmmo = 30;         // ������ ȹ�� �� �߰��Ǵ� źâ�� ��

    [Header("Respawn Time")]
    [SerializeField] private float itemRespawnTime = 30f;   // ������ ȹ�� �� �ٽ� ����� �Ǵ� �ð�

    private void OnTriggerEnter(Collider other)
    {
        // �ε��� ������Ʈ�� �÷��̾�� gun�� ���� �־�� ��
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.gameObject.TryGetComponent<Gun>(out Gun gun))
        {
            //gun.Ammo += amountOfAmmo;                       // �÷��̾� źâ ����         
            //StartCoroutine(nameof(ItemRespawnProcess));     // ����� �ڷ�ƾ ����
            //itemCollider.enabled = false;                   // �ݶ��̴� ��Ȱ��ȭ
            //itemMesh.enabled = false;                       // ������ �Ž� ��Ȱ��ȭ
        }
    }

    private IEnumerator ItemRespawnProcess()
    {
        yield return new WaitForSeconds(itemRespawnTime);   // ���ص� �ð� ��ٸ�
        itemCollider.enabled = true;                        // �ݶ��̴� Ȱ��ȭ
        itemMesh.enabled = true;                            // ������ �Ž� Ȱ��ȭ
        StopCoroutine(nameof(ItemRespawnProcess));          // �ڷ�ƾ ����
    }
}