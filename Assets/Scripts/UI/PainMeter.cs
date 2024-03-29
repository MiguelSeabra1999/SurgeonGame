
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Slider))]
public class PainMeter : MonoBehaviour
{
    public float maxPain = 1;
    private Slider slider;
    public RawImage fillBar;
    public Color normalFillBarColor;
    public Color maxFillBarColor;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        float pain = GameManager.Instance.physicsSimulator.cachedErrorSum;
        float percent = Math.Min(pain/maxPain,1);
        slider.value = percent;
        fillBar.color = Color.Lerp(normalFillBarColor,maxFillBarColor,percent);
    }
}
