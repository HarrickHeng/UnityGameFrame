using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class CircleEffect : MonoBehaviour
{
 
    public Color color0;
    public Color color1;
    public Color colorBlank;
    public Renderer rendFill;

    private Vector3 _srcFillScale; 
    private Renderer _myRend;


    public void SetEffect(float time,float R) {
        var tf = gameObject.transform;
        tf.localScale = Vector3.one * R;
        _InitColor();
        _myRend.material.DOColor(color1, time);
        rendFill.material.DOColor(color1, time);
        tf.DOShakePosition(time, 0.05f).OnComplete(() =>
        {
            _SetColor(colorBlank);

            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(tf.DOScale(tf.localScale + new Vector3(0.2f, 0.2f, 0f), 0.3f));
            mySequence.Append(tf.DOScale(tf.localScale + Vector3.zero, 0.1f));
        });
    }

    public void ClearEffect()
    {
        _InitColor();

        gameObject.transform.DOKill();
        rendFill.material.DOKill();
        _myRend.material.DOKill();
    }

    public void SetFill(float fill = 1f)
    {
        rendFill.transform.localScale = new Vector3(_srcFillScale.x, _srcFillScale.y * fill, _srcFillScale.z);
    }

    void _InitColor() {
        _SetColor(color0);
    }

    void _SetColor(Color c)
    {
        _myRend.material.color = c;
        rendFill.material.color = c;
    }


    private void Awake()
    {
        _myRend = GetComponent<Renderer>();
    }


    void Start()
    {
        _srcFillScale = rendFill.transform.localScale;
        _InitColor();
        SetFill();
    }

}
