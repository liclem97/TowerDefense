using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TeleportAnchor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image teleportSelectedUI;  // �÷��̾ ��Ŀ ���� �� ǥ���ϴ� �̹���

    [Header("Item")]
    [SerializeField] private Transform itemSpawnPoint;  // �������� ���� �Ǵ� ��ġ
    [SerializeField] private GameObject itemPrefab;           // ������ ������
    [SerializeField] private float itemSpawnTime = 10f;       // ���� �� ������ �ð��� ������ ������ ����

    private Animator animator;

    private void Start()
    {
        teleportSelectedUI.enabled = false;
        animator = GetComponentInChildren<Animator>();
        if (animator != null )
        {
            animator.enabled = false;
        }

        StartCoroutine(nameof(SpawnItem));
    }

    // ���� �� itemSpawnTime�� ��ٸ� �� ���� ��ġ�� ������ ����
    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(itemSpawnTime);                            // itemSpawnTime ��ŭ ���
        Instantiate(itemPrefab, itemSpawnPoint.position, Quaternion.identity);     // ������ ����
        StopCoroutine(nameof(SpawnItem));                                          // �ڷ�ƾ ����
    }

    // �ڷ���Ʈ ��Ŀ�� �÷��̾�� RayCast ������ �� ����
    public void OnAnchorAimmed()
    {
        if (teleportSelectedUI.enabled == false)
        {
            teleportSelectedUI.enabled = true;
            animator.enabled = true;
        }                
    }

    // �ڷ���Ʈ ��Ŀ�� �÷��̾��� RayCast���� ����� �� ����
    public void OnAnchorUnAimmed()
    {   
        if (teleportSelectedUI.enabled == true)
        {
            teleportSelectedUI.enabled = false;
            animator.enabled = false;
        }        
    }

    // �ڷ���Ʈ ��Ŀ�� �÷��̾�� ���� �Ǿ��� �� ����
    public void OnAnchorSelected()
    {
        // �÷��̾��� ��ġ�� �ش� ��Ŀ�� ���� �ڷ���Ʈ�Ѵ�.
    }
}