﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Tank : MonoBehaviour
{
    public int expValue = 0; //游戏中的经验值
    public int levelValue = 1; //默认一级
    public string nickName;//游戏呢称

    //坦克移动控制
    public float speed = 2;
    public float angulaSpeed = 5;
    public float playerNum = 1;    //玩家编号,用于区分不同的控制
    private Rigidbody body;
    public Transform gameManagerTransform;

    public AudioClip idleAudio;
    public AudioClip drivingAudio;
    private AudioSource audioSource;//坦克移动声音

    public int Hp = 100;//坦克默认的血量
    public GameObject tankExplosion;
    public AudioClip tankExplosionAudio;
    public Slider hpSlider;//血条

    public GameObject shellPrefab;
    public KeyCode fireKey = KeyCode.Space;//空格键开火
    private Transform firePosition;
    public float shellSpeed = 15;
    public AudioClip shotClip;
    public AudioSource audioSourceFire;

    private float _X = 0;
    private float _Y = 0;

    int i = 0;
    float time = 0;

    void Start()
    {
        body = this.GetComponent<Rigidbody>();
        audioSource = this.GetComponent<AudioSource>();
        gameManagerTransform = GameObject.Find("GameManager").transform;
        hpSlider = GameObject.Find("HpSlider").GetComponent<Slider>();
        InitOperation();
    }

    //初始化操作
    void InitOperation()
    {
        //炮弹起始位置
        firePosition = transform.Find("FirePosition");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(fireKey)&& this.transform.tag == "Player")
        {
            //AudioSource.PlayClipAtPoint(shotClip, transform.position, 1.0f);//开火的声音
            audioSourceFire.clip = shotClip;
            if (audioSource.isPlaying == false)
                audioSource.Play();
            GameObject go = GameObject.Instantiate(shellPrefab, firePosition.position, firePosition.rotation) as GameObject;
            //设置炮弹的父物体
            go.transform.parent = gameManagerTransform;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * shellSpeed;//炮弹速度
        }
    }

    void FixedUpdate()
    {
        if(net_game.net.GetRooming()&&this.transform.tag=="Player"){
            GameObject player = this.gameObject;
            if(player == null){
                Debug.LogError("this.GameObject错误");
                return ;
            }
            long ID = config.Get_id_by_Tag(player.name);
            //移动数值
            _X = config.Get_xy_by_id(ID).x;
            _Y = config.Get_xy_by_id(ID).y;         

            /* 
            //旋转 
            if(_X != 0){
                Quaternion targetAngels =  Quaternion.Euler(0, player.transform.localEulerAngles.y + _X * 90, 0);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetAngels, speed * Time.deltaTime);
            }

            //移动
            Vector3 oldpos = player.transform.position;
            Vector3 tarPos = oldpos + transform.forward *_Y;
            player.transform.position  = Vector3.Lerp(oldpos, tarPos,Time.deltaTime * angulaSpeed );
            */

            if (Mathf.Abs(_X) > 0.1 || Mathf.Abs(_Y) > 0.1)  //坦克行走时播放的声音
            {
                audioSource.clip = drivingAudio;
                if (audioSource.isPlaying == false)
                    audioSource.Play();
            }
            else //坦克停止时播放的声音                                                  
            {
                audioSource.clip = idleAudio;
                if (audioSource.isPlaying == false)
                    audioSource.Play();
            }     
        }
    }

    //Tank伤害计算
    void TakeDamage()
    {
        //如果血量已小于0,直接结束
        if (Hp <= 0)
            return;
        //如果血量大于0,血量减少,伤害在10-20之间
        Hp -= Random.Range(10, 20);
        hpSlider.value = Hp / 100.0f;
        //收到伤害之后 血量为0 控制死亡效果
        if (Hp <= 0)
        {
            AudioSource.PlayClipAtPoint(tankExplosionAudio, transform.position);
            GameObject.Instantiate(tankExplosion, transform.position + Vector3.up, transform.rotation);//实例化tankExplosion
            GameObject.Destroy(this.gameObject);
        }
    }
}
