﻿using lib;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace hexjig
{
    public class Game : EventDispatcher
    {
        public Array<Piece> pieces = new Array<Piece>();

        public HaxgonCoord<Coord> coordSys = new HaxgonCoord<Coord>();

        private int maxx = 23;
        private int miny = -9;
        private int[] movesy = { 0,0,0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 2, 1, 2, 2, 2, 2,2,2 };

        public float offx;
        public float offy;
        public float offx1;
        public float offy1;

        private DateTime startTime;

        public GameObject root;

        public GameObject stageRoot;

        public static Game Instance;

        public Game(LevelConfig config)
        {
            Instance = this;
            //创建跟接点
            root = new GameObject();
            root.name = "GameRoot";
            root.layer = 8;

            MainData.Instance.dispatcher.AddListener(EventType.RESTART,OnRestart);
            MainData.Instance.dispatcher.AddListener(EventType.SHOW_TIP, OnShowTip);
            MainData.Instance.dispatcher.AddListener(EventType.HIDE_GAME, OnHideGame);
            MainData.Instance.dispatcher.AddListener(EventType.SHOW_START_EFFECT, ShowStartEffect);
            MainData.Instance.dispatcher.AddListener(EventType.SHOW_CUT, ShowCut);

            //外面有 23 x 9 的大小
            HaxgonCoord<Coord> sys = new HaxgonCoord<Coord>();
            for (int py = 0; py > miny; py--)
            {
                for (int x = 0; x < maxx; x++)
                {
                    int y = py - 3 + movesy[x];
                    sys.SetCoord(Point2D.Create(x, y), new Coord { type = 0 });
                }
            }
            //颜色信息
            int type = 1;
            //生成片信息
            for (int i = 0; i < config.pieces.Count; i++)
            {
                Piece piece = new Piece();
                piece.game = this;
                piece.isAnswer = true;
                pieces[i] = piece;
                for (int p = 0; p < config.pieces[i].coords.Count; p++)
                {
                    Coord coord = new Coord
                    {
                        x = config.pieces[i].coords[p].x,
                        y = config.pieces[i].coords[p].y,
                        piece = piece,
                        type = type
                    };
                    piece.coords.Add(coord);
                }
                type++;
                piece.Init();
            }
            for (int i = 0; i < config.pieces2.Count; i++)
            {
                Piece piece = new Piece();
                piece.game = this;
                piece.isAnswer = false;
                pieces.Add(piece);
                for (int p = 0; p < config.pieces2[i].coords.Count; p++)
                {
                    Coord coord = new Coord
                    {
                        x = config.pieces2[i].coords[p].x,
                        y = config.pieces2[i].coords[p].y,
                        piece = piece,
                        type = type
                    };
                    piece.coords.Add(coord);
                }
                type++;
                piece.Init();
            }

            //创建主坐标系
            for(int i = 0; i < pieces.length; i++)
            {
                Piece piece = pieces[i];
                piece.outCoord = AutoSetPiece(pieces[i], sys);
                if (piece.isAnswer)
                {
                    for(int n = 0; n < piece.coords.length; n++)
                    {
                        Coord coord = new Coord
                        {
                            x = piece.coords[n].x,
                            y = piece.coords[n].y,
                            type = 0
                        };
                        coordSys.SetCoord(Point2D.Create(piece.coords[n].x, piece.coords[n].y), coord);
                    }
                }
            }

            //生成显示相关内容
            CreateDisplay();

            //计时
            MainData.Instance.time.value = 0;
            startTime = System.DateTime.Now;
        }

        /// <summary>
        /// 显示截图
        /// </summary>
        /// <param name="e"></param>
        private void ShowCut(lib.Event e)
        {
            //0.2 1.07 -17
            stageRoot.transform.parent = root.transform.parent;
            GameObject image = ResourceManager.CreateImage("image/uiitem/rect");
            image.transform.parent = stageRoot.transform;
            image.transform.localPosition = new Vector3(-offx, -offy + GameVO.Instance.Height * 0.25f);
            float size = 13 * 0.6f;
            image.GetComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
            image.GetComponent<SpriteRenderer>().size = new Vector2(size, size);
            for (int i = 0; i < this.pieces.length; i++)
            {
                pieces[i].ShowCut();
            }
            Background bk = new Background();
            bk.Draw(1, 0, size - 0.4f);
            bk.container.transform.parent = stageRoot.transform;
            bk.container.transform.localPosition = new Vector3(-offx, -offy + GameVO.Instance.Height * 0.25f);
            //添加一个缩放层，达到以中心为缩放点缩放的效果
            MainData.Instance.showCutRoot = new GameObject();
            stageRoot.transform.localPosition = new Vector3(stageRoot.transform.localPosition.x, stageRoot.transform.localPosition.y - GameVO.Instance.Height * 0.25f, stageRoot.transform.localPosition.z);
            stageRoot.transform.parent = MainData.Instance.showCutRoot.transform;
            MainData.Instance.showCutRoot.transform.localPosition = new Vector3(0, GameVO.Instance.Height * 0.25f);
        }

        private void ShowStartEffect(lib.Event e)
        {
            root.SetActive(true);
        }

        private void OnHideGame(lib.Event e)
        {
            root.SetActive(false);
        }

        private void OnShowTip(lib.Event e)
        {
            for (int i = 0; i < pieces.length; i++)
            {
                if(pieces[i].isAnswer && pieces[i].hasShowTip == false)
                {
                    pieces[i].ShowTip();
                    return;
                }
            }
        }

        /// <summary>
        /// 重新开始关卡
        /// </summary>
        /// <param name="e"></param>
        private void OnRestart(lib.Event e)
        {
            //重置所有的片
            for(int i = 0; i < pieces.length; i++)
            {
                pieces[i].Reset();
            }

            //重置点阵信息
            foreach (var item in coordSys.coords)
            {
                Coord coord = item.Value;
                coord.piece = null;
                coord.type = 0;
            }
        }

        private void CreateDisplay()
        {
            //生成背景
            float minX = 1000;
            float maxX = -1000;
            float minY = 1000;
            float maxY = -1000;
            GameObject p = new GameObject();
            p.transform.parent = root.transform;
            foreach (var item in coordSys.coords)
            {
                Coord coord = item.Value;
                GameObject image = ResourceManager.CreateImage("image/grid/gridBg");
                Point2D position = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(coord.x, coord.y), 0.4f);
                image.transform.position = new Vector3(position.x, position.y, 100);
                image.transform.parent = p.transform;
                if(position.x < minX)
                {
                    minX = position.x;
                }
                if(position.x > maxX)
                {
                    maxX = position.x;
                }
                if(position.y < minY)
                {
                    minY = position.y;
                }
                if(position.y > maxY)
                {
                    maxY = position.y;
                }
            }
            MainData.Instance.levelWidth = maxX - minX + 1.5f;
            MainData.Instance.levelHeight = maxY - minY + 1.5f;
            offx = -((maxX - minX) * 0.5f + minX);
            offy = -((maxY - minY) * 0.5f + minY) + GameVO.Instance.Height * 0.25f;
            p.transform.position = new Vector3(offx, offy);
            stageRoot = p;

            GameObject outBackground = new GameObject();
            outBackground.transform.parent = root.transform;
            
            //生成背景
            minX = 1000;
            maxX = -1000;
            minY = 1000;
            maxY = -1000;
            GameObject p1 = new GameObject();
            p1.transform.parent = root.transform;
            for (int x = 0; x < this.maxx; x++)
            {
                for (int py = 0; py > this.miny; py--)
                {
                    int y = py - 3 + movesy[x];
                    Point2D position = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(x, y), 0.2f);

                    /*
                    GameObject image = ResourceManager.CreateImage("image/grid/gridBg");
                    image.transform.localScale = new Vector3(0.5f, 0.5f);
                    image.transform.position = new Vector3(position.x, position.y,100);
                    image.transform.parent = p1.transform;
                    //*/

                    if (position.x < minX)
                    {
                        minX = position.x;
                    }
                    if (position.x > maxX)
                    {
                        maxX = position.x;
                    }
                    if (position.y < minY)
                    {
                        minY = position.y;
                    }
                    if (position.y > maxY)
                    {
                        maxY = position.y;
                    }
                }
            }
            offx1 = -((maxX - minX) * 0.5f + minX);
            offy1 = -((maxY - minY) * 0.5f + minY) -2.4f;
            p1.transform.position = new Vector3(offx1, offy1);

            for (int i = 0; i < pieces.length; i++)
            {
                pieces[i].background = outBackground;
                pieces[i].CreateDisplay();
            }

            outBackground.transform.position = new Vector3(offx1, offy1);
        }

        private float lastClick = 0;
        private bool isDragMove = false;
        private Piece dragPiece;

        public void Update()
        {
            for(int i = 0; i < pieces.length; i++)
            {
                pieces[i].Update();
            }

            double time = System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
            MainData.Instance.time.value = (int)time;

            if (Input.GetAxis("Fire1") > 0 && lastClick == 0)
            {
                Vector3 pos = Input.mousePosition;
                pos.x = (pos.x / GameVO.Instance.PixelWidth - 0.5f) * GameVO.Instance.Width;
                pos.y = (pos.y / GameVO.Instance.PixelHeight - 0.5f) * GameVO.Instance.Height;
                //Point2D p = HaxgonCoord<Coord>.PositionToCoord(Point2D.Create(pos.x - offx1, pos.y - offy1), 0.2f);
                //Point2D p1 = HaxgonCoord<Coord>.PositionToCoord(Point2D.Create(pos.x - offx, pos.y - offy), 0.4f);
                for (int i = 0; i < pieces.length; i++)
                {
                    if(pieces[i].IsTouchIn(pos.x,pos.y))
                    {
                        pieces[i].StartDrag(pos.x, pos.y);
                        dragPiece = pieces[i];
                        break;
                    }
                }
                isDragMove = true;
            }
            else if(lastClick > 0 && Input.GetAxis("Fire1") == 0)
            {
                if(dragPiece != null)
                {
                    Vector3 pos = Input.mousePosition;
                    pos.x = (pos.x / GameVO.Instance.PixelWidth - 0.5f) * GameVO.Instance.Width;
                    pos.y = (pos.y / GameVO.Instance.PixelHeight - 0.5f) * GameVO.Instance.Height;
                    dragPiece.StopDrag(pos.x, pos.y);
                }
                dragPiece = null;
                isDragMove = false;
            }
            else if(isDragMove)
            {
                if (dragPiece != null)
                {
                    Vector3 pos = Input.mousePosition;
                    pos.x = (pos.x / GameVO.Instance.PixelWidth - 0.5f) * GameVO.Instance.Width;
                    pos.y = (pos.y / GameVO.Instance.PixelHeight - 0.5f) * GameVO.Instance.Height;
                    dragPiece.DragMove(pos.x, pos.y);
                }
            }
            lastClick = Input.GetAxis("Fire1");
        }

        private Point2D AutoSetPiece(Piece piece, HaxgonCoord<Coord> sys)
        {
            List<Point2D> list = new List<Point2D>();
            float minX = 1000;
            float maxX = -1000;
            float minY = 1000;
            float maxY = -1000;
            for (int py = 0; py > this.miny; py--)
            {
                for (int x = 0; x < this.maxx; x++)
                {
                    int y = py - 3 + movesy[x];
                    list.Add(Point2D.Create(x, y));
                    Point2D position = HaxgonCoord<Coord>.CoordToPosition(list[list.Count -1], 0.2f);
                    if (position.x < minX)
                    {
                        minX = position.x;
                    }
                    if (position.x > maxX)
                    {
                        maxX = position.x;
                    }
                    if (position.y < minY)
                    {
                        minY = position.y;
                    }
                    if (position.y > maxY)
                    {
                        maxY = position.y;
                    }
                }
            }
            Point2D center = HaxgonCoord<Coord>.PositionToCoord(Point2D.Create((minX + maxX) * 0.5f, (minY + maxY) * 0.5f), 0.2f);

            //从可选格子的正中心开始，以六边形的方式向外扩展找可以放的地方
            Dictionary<string, bool> findMap = new Dictionary<string, bool>();
            Dictionary<string, bool> findMap2 = new Dictionary<string, bool>();
            findMap2.Add(center.x + "," + center.y, true);
            List<Point2D> currentList = new List<Point2D>();
            currentList.Add(center);
            while (list.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    //在 list 中查找该点
                    bool findInList = false;
                    for (int l = 0; l < list.Count; l++)
                    {
                        if (list[l].x == currentList[i].x && list[l].y == currentList[i].y)
                        {
                            list.RemoveAt(l);
                            findInList = true;
                            break;
                        }
                    }
                    if (findInList)
                    {
                        //如果找到则查看该点是否可以放
                        Point2D result = TrySetPiece((int)currentList[i].x, (int)currentList[i].y, piece, sys);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                List<Point2D> newList = new List<Point2D>();
                //展开所有的点
                for (int i = 0; i < currentList.Count; i++)
                {
                    if (findMap.ContainsKey(currentList[i].x + "," + currentList[i].y))
                    {
                        continue;
                    }
                    //展开
                    List<Point2D> nextCoords = HaxgonCoord<Coord>.GetCoordsNextTo(currentList[i]);
                    for(int n = 0; n < nextCoords.Count; n++)
                    {
                        if(findMap2.ContainsKey(nextCoords[n].x + "," + nextCoords[n].y))
                        {
                            continue;
                        }
                        findMap2.Add(nextCoords[n].x + "," + nextCoords[n].y, true);
                        newList.Add(nextCoords[n]);
                    }
                }
                currentList = newList;
            }
            return null;
        }

        private Point2D TrySetPiece(int x,int y, Piece piece, HaxgonCoord<Coord> sys)
        {
            bool find = true;
            for (int i = 0; i < piece.coords.length; i++)
            {
                Point2D p = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(piece.coords[i].x, piece.coords[i].y), 0.2f);
                p.x += piece.offx * 0.5f;
                p.y += piece.offy * 0.5f;
                Point2D p2 = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(x, y), 0.2f);
                p2.x += p.x;
                p2.y += p.y;
                p = HaxgonCoord<Coord>.PositionToCoord(p2, 0.2f);
                Coord grid = sys.GetCoord(p);
                if (grid == null || grid.type != 0)
                {
                    find = false;
                    break;
                }
                else
                {
                    //获取周围的格子
                    List<Point2D> nextCoords = HaxgonCoord<Coord>.GetCoordsNextTo(p);
                    for (int n = 0; n < nextCoords.Count; n++)
                    {
                        Coord nextGrid = sys.GetCoord(Point2D.Create(nextCoords[n].x, nextCoords[n].y));
                        if (nextGrid != null && nextGrid.piece != grid.piece)
                        {
                            find = false;
                            break;
                        }
                    }
                }
            }
            if (find)
            {
                for (int i = 0; i < piece.coords.length; i++)
                {
                    Point2D p = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(piece.coords[i].x, piece.coords[i].y), 0.2f);
                    p.x += piece.offx * 0.5f;
                    p.y += piece.offy * 0.5f;
                    Point2D p2 = HaxgonCoord<Coord>.CoordToPosition(Point2D.Create(x, y), 0.2f);
                    p2.x += p.x;
                    p2.y += p.y;
                    p = HaxgonCoord<Coord>.PositionToCoord(p2, 0.2f);
                    sys.SetCoord(p, piece.coords[i]);
                }
                return Point2D.Create(x, y);
            }
            return null;
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public void CheckFinish()
        {
            bool finish = true;
            foreach(var item in coordSys.coords)
            {
                Coord coord = item.Value;
                if(coord.type == 0)
                {
                    finish = false;
                    break;
                }
            }
            if(finish)
            {
                MainData.Instance.dispatcher.DispatchWith(EventType.FINISH_LEVEL, MainData.Instance.time.value);
            }
        }

        public void Dispose()
        {
            MainData.Instance.dispatcher.RemoveListener(EventType.RESTART, OnRestart);
            MainData.Instance.dispatcher.RemoveListener(EventType.SHOW_TIP, OnShowTip);
            MainData.Instance.dispatcher.RemoveListener(EventType.HIDE_GAME, OnHideGame);
            MainData.Instance.dispatcher.RemoveListener(EventType.SHOW_START_EFFECT, ShowStartEffect);
            MainData.Instance.dispatcher.RemoveListener(EventType.SHOW_CUT, ShowCut);
        }
    }
}