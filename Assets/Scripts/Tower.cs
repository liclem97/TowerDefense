using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public static Tower Instance; //Tower�� �̱��� ��ü
    //������ ǥ���� UI
    public Transform damageUI; 
    public Image damageImage;
    public Text damageText;

    public int initialHP = 10; //�������, �ʵ�

    int _hp = 0;
    public int HP //������Ƽ 
    {
        get
        {
            return _hp;
        }
        set{
            _hp = value;
            StopAllCoroutines();            //���� ���� ���� �ڷ�ƾ ����
            StartCoroutine(DamageEvent());  //���ڰŸ��� ó���� �ڷ�ƾ �Լ� ȣ��
            if (_hp <= 0)
            {
                //Destroy(gameObject); // Ÿ���� ü���� 0�̵Ǹ� Ÿ��, �÷��̾�, ī�޶� ��� ����
            }
            damageText.text = _hp.ToString();
        }
    }
    public float damageTime = 0.1f;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this; //�̱��� ��ü �� �Ҵ�
        }
    }
    
    void Start()
    {
        _hp = initialHP;                            // Ÿ�� ü�� �ʱ�ȭ
        float z = Camera.main.nearClipPlane + 0.3f; // nearClipPlane �� ����

        damageUI.parent = Camera.main.transform;        // UI�� ī�޶� �ڽ����� ���
        damageUI.localPosition = new Vector3(0, 0, z);  // UI ��ġ�� ī�޶��� near������ ����
        damageUI.rotation = Camera.main.transform.rotation;
        damageImage.enabled = false;                    // ó������ ��Ȱ��ȭ

        damageText.text = initialHP.ToString();
    }
    IEnumerator DamageEvent()
    {
        damageImage.enabled = true;
        yield return new WaitForSeconds(damageTime);
        damageImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
