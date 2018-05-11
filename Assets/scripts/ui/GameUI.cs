﻿using hexjig;
using lib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class GameUI : MonoBehaviour {

    public Text timeTxt;
    public TextExd modelTxt;
    public GameObject flush;

    public Text time2;
    public Text txt1;
    public Text txt2;
    public Text txt3;
    public Text txt4;

    public GameObject hex;

    public GameObject quit;
    public GameObject quitSelection;
    private bool quitOpen = false;

    //临时测试
    public Text levelTxt;

    public RectTransform hexTrans;
    public RectTransform sure;
    public RectTransform cancel;

    private void Awake()
    {
        UIFix.SetDistanceToTop(hexTrans);
        quitSelection.GetComponent<RectTransform>().sizeDelta = new Vector2(720, GameVO.Instance.PixelHeight);

        sure.localScale = new Vector3(0, 1);
        cancel.localScale = new Vector3(0, 1);

        quitSelection.SetActive(false);
        flush.SetActive(false);
        ButtonClick.dispatcher.AddListener("quitGame", OnQuit);
        ButtonClick.dispatcher.AddListener("restart", OnRestart);
        ButtonClick.dispatcher.AddListener("nextGame", OnNextGame);
        ButtonClick.dispatcher.AddListener("gameSure", OnQuitSure);
        ButtonClick.dispatcher.AddListener("gameCancel", OnQuitCancel);
        ButtonClick.dispatcher.AddListener("quitGameQuit", OnQuitCancel);
        ButtonClick.dispatcher.AddListener("tip", OnTip);
        MainData.Instance.dispatcher.AddListener(hexjig.EventType.FINISH_LEVEL, OnFinshLevel);
        MainData.Instance.dispatcher.AddListener(hexjig.EventType.SHOW_CUT_COMPLETE, ShowFlush);
        MainData.Instance.time.AddListener(lib.Event.CHANGE, OnTimeChange);
        MainData.Instance.dispatcher.AddListener(hexjig.EventType.SET_PIECE, OnSetPiece);
        MainData.Instance.dispatcher.AddListener(hexjig.EventType.SHOW_GAME_CHANGE_OUT_EFFECT_COMPLETE, OnFinshLevel2);
    }

    private void OnQuitCancel(lib.Event e)
    {
        quitOpen = false;
        GameObjectUtils.EnableComponentAllChildren<Shadow>(quit);
        sure.DOScaleX(0, 0.2f);
        cancel.DOScaleX(0, 0.2f);
        quitSelection.SetActive(false);
        Game.Instance.rootStage.transform.DOScale(1, 0.5f).SetEase(Ease.OutCirc);
    }

    private void OnQuitSure(lib.Event e)
    {
        quitOpen = false;
        GameObjectUtils.EnableComponentAllChildren<Shadow>(quit);
        sure.DOScaleX(0, 0.2f);
        cancel.DOScaleX(0, 0.2f);
        quitSelection.SetActive(false);
        Game.Instance.rootStage.transform.DOScale(1, 0.5f).SetEase(Ease.OutCirc);

        if (GameVO.Instance.model == GameModel.Freedom)
        {
            if(GameVO.Instance.modelCount < 10)
            {
                GameVO.Instance.bgmId.value = bgmId;
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                GameVO.Instance.ShowModule(ModuleName.Freedom);
            }
            else
            {
                ResourceManager.PlaySound("sound/camera", false, GameVO.Instance.soundVolumn.value / 100.0f);
                hex.SetActive(false);
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_CUT);
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                GameVO.Instance.ShowModule(ModuleName.Result, MainData.Instance.time.value);
            }
        }
        else if (GameVO.Instance.model == GameModel.Daily)
        {
            GameVO.Instance.totalTimeString.value = GameVO.Instance.daily.allTimeString.value;
            if (GameVO.Instance.daily.firstPassAll)
            {
                GameVO.Instance.daily.firstPassAll = false;
                ResourceManager.PlaySound("sound/camera", false, GameVO.Instance.soundVolumn.value / 100.0f);
                hex.SetActive(false);
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_CUT);
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                GameVO.Instance.ShowModule(ModuleName.Result, MainData.Instance.time.value);
            }
            else
            {
                GameVO.Instance.bgmId.value = bgmId;
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                GameVO.Instance.ShowModule(ModuleName.Daily);
            }
        }
        GameVO.Instance.modelCount = 0;
    }

    private void OnNextGame(lib.Event e)
    {
        if (GameVO.Instance.model == GameModel.Daily)
        {
            if (GameVO.Instance.daily.HasNextLevel(MainData.Instance.levelId.value))
            {
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_OUT_EFFECT);
            }
            else
            {
                GameVO.Instance.totalTimeString.value = GameVO.Instance.daily.allTimeString.value;
                if (GameVO.Instance.daily.firstPassAll)
                {
                    GameVO.Instance.daily.firstPassAll = false;
                    ResourceManager.PlaySound("sound/camera", false, GameVO.Instance.soundVolumn.value / 100.0f);
                    hex.SetActive(false);
                    MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_CUT);
                    MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                    GameVO.Instance.ShowModule(ModuleName.Result, e.Data);
                }
                else
                {
                    MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.QUIT_LEVEL);
                    GameVO.Instance.ShowModule(ModuleName.Daily);
                }
            }
        }
        else
        {
            MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_OUT_EFFECT);
        }
    }

    private void OnSetPiece(lib.Event e)
    {
        ResourceManager.PlaySound("sound/setpiece", false, GameVO.Instance.soundVolumn.value / 100.0f);
    }

    private void OnTip(lib.Event e)
    {
        MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_TIP);
    }

    private void OnTimeChange(lib.Event e)
    {
        timeTxt.text = StringUtils.TimeToMS(MainData.Instance.time.value);
        time2.text = StringUtils.TimeToMS(MainData.Instance.time.value);
    }

    /// <summary>
    /// 完成关卡，显示结果
    /// </summary>
    /// <param name="e"></param>
    private void OnFinshLevel(lib.Event e)
    {
        GameVO.Instance.modelCount++;
        GameVO.Instance.totalTime.value += MainData.Instance.time.value;
        GameVO.Instance.totalTimeString.value = StringUtils.TimeToMS(GameVO.Instance.totalTime.value);

        int time = (int)(MainData.Instance.time.value / 1000.0f);
        int modelId = 0;
        for(int i = 0; i < ModelConfig.Configs.Count; i++)
        {
            if(MainData.Instance.config.pieces.Count + MainData.Instance.config.pieces2.Count >= ModelConfig.Configs[i].min &&
                MainData.Instance.config.pieces.Count + MainData.Instance.config.pieces2.Count <= ModelConfig.Configs[i].max)
            {
                modelId = ModelConfig.Configs[i].id;
                break;
            }
        }
        for(int i = 0; i < PassScoreConfig.Configs.Count; i++)
        {
            if(modelId == PassScoreConfig.Configs[i].model.id && time >= PassScoreConfig.Configs[i].minTime && time <= PassScoreConfig.Configs[i].maxTime)
            {
                GameVO.Instance.passScore = PassScoreConfig.Configs[i];
                break;
            }
        }
        float score = GameVO.Instance.passScore.scoreMin + 1.0f * (GameVO.Instance.passScore.scoreMax - GameVO.Instance.passScore.scoreMin) * (time - GameVO.Instance.passScore.minTime) / (GameVO.Instance.passScore.maxTime - GameVO.Instance.passScore.minTime);
        txt3.text = ((int)score) + (score > 0 ? ((int)(UnityEngine.Random.Range(0, 1.0f) * 10))/10.0f : 0) + "%";

        txt1.GetComponent<TextExd>().languageId = GameVO.Instance.passScore.language.id;

        if (GameVO.Instance.model == GameModel.Daily)
        {
            //修改记录
            GameVO.Instance.daily.Finish(MainData.Instance.levelId.value,MainData.Instance.time.value);
            MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_OUT_EFFECT0);
        }
        else
        {
            //先播放之前关卡的退场动画
            MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_OUT_EFFECT0);
        }
    }


    private void OnFinshLevel2(lib.Event e)
    {
        if(GameVO.Instance.model == GameModel.Daily)
        {
            //修改记录
            GameVO.Instance.daily.Finish(MainData.Instance.levelId.value, MainData.Instance.time.value);
            if (GameVO.Instance.daily.HasNextLevel(MainData.Instance.levelId.value))
            {
                new StartGameCommand(GameVO.Instance.daily.GetNextLevel(MainData.Instance.levelId.value));
                levelTxt.text = GameVO.Instance.daily.GetNextLevel(MainData.Instance.levelId.value) + "";
                MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_IN_EFFECT);
            }
        }
        else
        {
            StartFreedomLevel();
            MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.SHOW_GAME_CHANGE_IN_EFFECT);
        }
    }

    private void OnRestart(lib.Event e)
    {
        //MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.RESTART);
        MainData.Instance.dispatcher.DispatchWith(hexjig.EventType.BACK_STEP);
    }

    private void OnQuit(lib.Event e)
    {
        if(quitOpen)
        {
            quitOpen = false;
            sure.DOScaleX(0, 0.2f);
            cancel.DOScaleX(0, 0.2f);
            GameObjectUtils.EnableComponentAllChildren<Shadow>(quit);
            quitSelection.SetActive(false);
            Game.Instance.rootStage.transform.DOScale(1, 0.5f).SetEase(Ease.OutCirc);
        }
        else
        {
            quitOpen = true;
            sure.DOScaleX(1, 0.2f);
            cancel.DOScaleX(1, 0.2f);
            GameObjectUtils.DisableComponentAllChildren<Shadow>(quit);
            quitSelection.SetActive(true);
            Game.Instance.rootStage.transform.DOScale(0.6f, 0.5f).SetEase(Ease.OutCirc);
        }
    }

    private void ShowFlush(lib.Event e)
    {
        hex.SetActive(true);
        flush.SetActive(true);
        flush.GetComponent<RectTransform>().sizeDelta = new Vector2(GameVO.Instance.PixelWidth, GameVO.Instance.PixelHeight);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(flush.GetComponent<Image>().DOColor(new Color(1, 1, 1, 1), 0.15f));
        mySequence.Append(flush.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 0.1f)).onComplete = FlushComplete;
    }

    private void FlushComplete()
    {
        flush.SetActive(false);
    }

    private int bgmId;
    private void OnEnable()
    {
        GameVO.Instance.modelCount = 0;
        GameVO.Instance.totalTime.value = 0;
        if (GameVO.Instance.model == GameModel.Daily)
        {
            int level = (int)GameVO.Instance.moduleData;
            new StartGameCommand(level);
            levelTxt.text = level + "";
        }
        else if (GameVO.Instance.model == GameModel.Freedom)
        {
            StartFreedomLevel();
        }
        modelTxt.languageId = GameVO.Instance.model == GameModel.Daily ? 10 : 9;
        bgmId = GameVO.Instance.bgmId.value;
        GameVO.Instance.bgmId.value = 1000;
    }

    private void StartFreedomLevel()
    {
        int level = 0;
        List<int> list = new List<int>();
        List<int> list2 = new List<int>();
        List<int> list3 = new List<int>();
        for (int i = 0; i < LevelConfig.Configs.Count; i++)
        {
            if (LevelConfig.Configs[i].pieces.Count + LevelConfig.Configs[i].pieces2.Count >= 4 && LevelConfig.Configs[i].pieces.Count + LevelConfig.Configs[i].pieces2.Count <= 6)
            {
                list.Add(LevelConfig.Configs[i].id);
            }
            if (LevelConfig.Configs[i].pieces.Count + LevelConfig.Configs[i].pieces2.Count >= 7 && LevelConfig.Configs[i].pieces.Count + LevelConfig.Configs[i].pieces2.Count <= 9)
            {
                list2.Add(LevelConfig.Configs[i].id);
            }
            else if (LevelConfig.Configs[i].pieces.Count + LevelConfig.Configs[i].pieces2.Count >= 10)
            {
                list3.Add(LevelConfig.Configs[i].id);
            }
        }
        if (GameVO.Instance.difficulty == DifficultyMode.Easy)
        {
            level = list[(int)(UnityEngine.Random.Range(0, 1.0f) * list.Count)];
        }
        else if (GameVO.Instance.difficulty == DifficultyMode.Normal)
        {
            level = list2[(int)(UnityEngine.Random.Range(0, 1.0f) * list2.Count)];
        }
        else if (GameVO.Instance.difficulty == DifficultyMode.Hard)
        {
            level = list3[(int)(UnityEngine.Random.Range(0, 1.0f) * list3.Count)];
        }
        new StartGameCommand(level);
        levelTxt.text = level + "";
    }

}