using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AmmoItem : MonoBehaviour
{
    [Header("Game Ref")]
    [SerializeField] private Collider itemCollider;
    [SerializeField] private MeshRenderer itemMesh;

    [Header("Stat")]
    [SerializeField] private int amountOfAmmo = 30;         // 아이템 획득 시 추가되는 탄창의 수

    [Header("Respawn Time")]
    [SerializeField] private float itemRespawnTime = 30f;   // 아이템 획득 후 다시 재생성 되는 시간

    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 오브젝트가 플레이어고 gun을 갖고 있어야 함
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.gameObject.TryGetComponent<Gun>(out Gun gun))
        {
            //gun.Ammo += amountOfAmmo;                       // 플레이어 탄창 증가         
            //StartCoroutine(nameof(ItemRespawnProcess));     // 재생성 코루틴 시작
            //itemCollider.enabled = false;                   // 콜라이더 비활성화
            //itemMesh.enabled = false;                       // 아이템 매쉬 비활성화
        }
    }

    private IEnumerator ItemRespawnProcess()
    {
        yield return new WaitForSeconds(itemRespawnTime);   // 정해둔 시간 기다림
        itemCollider.enabled = true;                        // 콜라이더 활성화
        itemMesh.enabled = true;                            // 아이템 매쉬 활성화
        StopCoroutine(nameof(ItemRespawnProcess));          // 코루틴 종료
    }
}