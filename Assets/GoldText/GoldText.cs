using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldText : MonoBehaviour
{
    TextMeshPro text;
    public float gold;
    Color alpha;

    Color32[] ul = new Color32[10];
    Color32[] ur = new Color32[10];
    Color32[] dl = new Color32[10];
    Color32[] dr = new Color32[10];
    // Start is called before the first frame update
    void Start()
    {
        // Bloody Mary #ff512f ¡æ #dd2476 // Red
        ul[0] = new Color32(255, 81, 47, 255);
        ur[0] = new Color32(221, 36, 118, 255);
        dl[0] = new Color32(255, 81, 47, 255);
        dr[0] = new Color32(221, 36, 118, 255);

        Destroy(this.gameObject, 1.0f);
        text = GetComponent<TextMeshPro>();

        text.text = string.Format("{0:#,##0.##}", gold);
        text.colorGradient = new VertexGradient(ul[2], ur[2], dl[2], dr[2]);

        alpha = text.color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(0, 2.0f * Time.deltaTime));
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * 1.0f);
        text.color = alpha;
    }
}
