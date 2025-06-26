using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TeleportAnchor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image teleportSelectedUI;  // 플레이어가 앵커 선택 시 표시하는 이미지

    [Header("Item")]
    [SerializeField] private Transform itemSpawnPoint;  // 아이템이 스폰 되는 위치
    [SerializeField] private GameObject itemPrefab;           // 스폰할 아이템
    [SerializeField] private float itemSpawnTime = 10f;       // 시작 후 설정된 시간이 지나면 아이템 스폰

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

    // 시작 후 itemSpawnTime을 기다린 뒤 스폰 위치에 아이템 생성
    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(itemSpawnTime);                            // itemSpawnTime 만큼 대기
        Instantiate(itemPrefab, itemSpawnPoint.position, Quaternion.identity);     // 아이템 생성
        StopCoroutine(nameof(SpawnItem));                                          // 코루틴 종료
    }

    // 텔레포트 앵커가 플레이어에게 RayCast 당했을 때 실행
    public void OnAnchorAimmed()
    {
        if (teleportSelectedUI.enabled == false)
        {
            teleportSelectedUI.enabled = true;
            animator.enabled = true;
        }                
    }

    // 텔레포트 앵커가 플레이어의 RayCast에서 벗어났을 때 실행
    public void OnAnchorUnAimmed()
    {   
        if (teleportSelectedUI.enabled == true)
        {
            teleportSelectedUI.enabled = false;
            animator.enabled = false;
        }        
    }

    // 텔레포트 앵커가 플레이어에게 선택 되었을 때 실행
    public void OnAnchorSelected()
    {
        // 플레이어의 위치를 해당 앵커의 위로 텔레포트한다.
    }
}