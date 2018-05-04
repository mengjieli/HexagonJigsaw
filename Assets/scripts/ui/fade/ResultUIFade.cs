﻿using System;
using System.Collections;
using System.Collections.Generic;
using lib;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using hexjig;

public class ResultUIFade : UIFade {

    public RectTransform time;
    public RectTransform model;
    public RectTransform line1;
    public RectTransform line2;
    public RectTransform hex;
    public RectTransform quit;
    public RectTransform home;
    public RectTransform share;
    public RectTransform root;

    private ModuleName moduleName;

    override public void FadeOut(ModuleName name)
    {
        moduleName = name;
        MainData.Instance.showCutRoot.transform.DOMove(new Vector3(1.03f, -10, 100), 0.3f);
        MainData.Instance.showCutRoot.transform.DOLocalRotate(new Vector3(0, 0, -50), 0.3f);
        time.DOScaleX(0, 0.3f);
        model.DOScaleX(0, 0.3f);
        hex.DOScaleX(0, 0.3f);
        hex.DOScaleY(0, 0.3f);
        quit.DOScaleX(0, 0.3f).onComplete = TweenComplete;
    }

    private void TweenComplete()
    {
        Destroy(MainData.Instance.showCutRoot);
        MainData.Instance.showCutRoot = null;
        dispatcher.DispatchWith(lib.Event.COMPLETE);
    }

    override public void FadeIn(ModuleName name)
    {
        time.localScale = new Vector3(0, 1);
        model.localScale = new Vector3(0, 1);
        line1.localScale = new Vector3(0, 1);
        line2.localScale = new Vector3(0, 1);
        hex.localScale = new Vector3(0, 0);
        quit.localScale = new Vector3(0, 1);
        home.localScale = new Vector3(0, 1);
        share.localScale = new Vector3(0, 0);

        MainData.Instance.showCutRoot.transform.localPosition = new Vector3(MainData.Instance.showCutRoot.transform.localPosition.x, MainData.Instance.showCutRoot.transform.localPosition.y,100);
        MainData.Instance.showCutRoot.transform.DOMove(new Vector3(1.03f, -0.4f,100), 0.4f);
        MainData.Instance.showCutRoot.transform.DOLocalRotate(new Vector3(0, 0, -17), 0.4f).onComplete = FadeIn2;
    }

    private void FadeIn2()
    {
        time.DOScaleX(1, 0.2f);
        model.DOScaleX(1, 0.2f).onComplete = FadeIn3;
    }

    private void FadeIn3()
    {
        line1.DOScaleX(1, 0.2f);
        line2.DOScaleX(1, 0.2f).onComplete = FadeIn4;
    }

    private void FadeIn4()
    {
        time.DOScaleX(1, 0.2f);
        model.DOScaleX(1, 0.2f);
        hex.DOScaleX(1, 0.2f);
        hex.DOScaleY(1, 0.2f);
        quit.DOScaleX(1, 0.2f);
        home.DOScaleX(1, 0.2f).onComplete = FadeIn5;
    }

    private void FadeIn5()
    {
        share.localScale = new Vector3(5, 5);
        share.DOScale(new Vector3(1, 1, 1), 0.2f).onComplete = FadeIn6;
    }

    private void FadeIn6()
    {
        ResourceManager.PlaySound("sound/gaizhang", false, GameVO.Instance.soundVolumn.value / 100.0f);
        root.DOShakePosition(0.3f,30).onUpdate = FadeIn6Update;
        shootCutX = MainData.Instance.showCutRoot.transform.position.x;
        shootCutY = MainData.Instance.showCutRoot.transform.position.y;
    }

    private float shootCutX;
    private float shootCutY;
    private void FadeIn6Update()
    {
        MainData.Instance.showCutRoot.transform.position = new Vector3(shootCutX + root.position.x, shootCutY + root.position.y,100);
    }
}