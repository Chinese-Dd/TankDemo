﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;
    public CameraFollow cameraFollow;
    private int enemyNo;//敌人编号

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }
    int i = 0;
    bool b = false;
    // Update is called once per frame
    void Update()
    {
        if(!net_game.net.GetStart()){
            Debug.Log("等待游戏初始化" + i++);//替换为等待场景
        }else if(!b){
            Debug.Log("游戏开始初始化");
            enemyNo = 0;
            net_game.net.GameMsgInit();//初始化场景
            CONFIG.Config_Value config_value = net_game.net.GetGameinit();
            long num = config_value.num;

            for (long i=0; i<num; i++){
                long id = config_value.ids[i];
                config.Add(id);
                config.xyz_Add(id);//统一位置
                if(id == net_game.net.GetPlayerId()){
                    SpawnPlayer(id, config_value.values[i].x, config_value.values[i].y);
                    cameraFollow.player = GameObject.FindGameObjectWithTag("Player").transform;
                }
                else {
                    SpawnEnemy(id, config_value.values[i].x, config_value.values[i].y);
                    Debug.Log("生成敌人"+id);
                }
            }
            Debug.Log("游戏初始化完成");
            net_game.net.GameStart();

            Thread thread = new Thread(new ThreadStart(net_game.net.SetMoveValueByNet));//接受移动参数数据
            thread.Start();

            b = true;
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemy(1, -15, 20);
        }
    }

    void FixedUpdate()
    {
        if(net_game.net.GetGameing()){
            net_game.net.Operate();//获取帧同步操作
        }


    }

    /// <summary>
    /// tank生成
    /// </summary>
    /// <param name="TankNo">坦克的编号</param>
    void SpawnPlayer(long TankNo, float x, float y)//生成玩家
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;
        go.transform.tag = "Player";
        go.transform.name = TankNo.ToString();        
        Vector3 value = new Vector3(x,0,y);
        config.ID_Add(go.transform.name);
        config.Set_ID(go.transform.name, TankNo);
        config.Name_Add(TankNo);
        config.Set_Name(TankNo, go.transform.name);
        go.transform.position = value;
    }

    void SpawnEnemy(long TankNo, float x, float y)//生成敌人
    {
        GameObject go = Instantiate(enemyPrefabs[enemyNo]) as GameObject;
        go.transform.tag = "Enemy";
        go.transform.name = TankNo.ToString();
        Vector3 value = new Vector3(x,0,y);
        config.ID_Add(go.transform.name);
        config.Set_ID(go.transform.name, TankNo);

        Quaternion q = new Quaternion(0,0,0,0);
        config.xyz_Set(TankNo, value, q);

        config.Name_Add(TankNo);
        config.Set_Name(TankNo, go.transform.name);

        go.transform.position = value;
        enemyNo++;
    }
}
