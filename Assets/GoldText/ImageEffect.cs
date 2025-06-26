using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageEffect : MonoBehaviour
{
    public Gradient gradient;

    [Range(0, 5)]
    public float time = 1; //정해진 시간마다 주기적으로 반복

    float gradientWaveTime;
    float curXNormalized;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();

    }

    private void Update()
    {
        gradientWaveTime += Time.deltaTime;

        curXNormalized = Mathf.PingPong(gradientWaveTime, time);

        Color c = gradient.Evaluate(curXNormalized);
        image.color = new Color(c.r, c.g, c.b);
    }
}
