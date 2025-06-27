using UnityEngine;

// 게임 내 MainTarget 클래스
public class MainTarget : MonoBehaviour
{
    public float targetHP;

    private void Start()
    {
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";    // 시작 시 UI 갱신
    }


    /*********************************************************************************************
    함수: OnTargetDamaged
    기능: 메인 타겟의 체력을 감소하고 0 이하일 시 GameOver를 호출
    매개변수
        - damage: 받은 대미지의 양
    *********************************************************************************************/
    public void OnTargetDamaged(float damage)
    {
        targetHP -= damage;
        if (targetHP <= 0)
        {
            targetHP = 0;
            if (GameManager.Instance.isGameStarted == true)
            {
                GameManager.Instance.GameOver();
            }
        }
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";    // UI 업데이트
    }
}
