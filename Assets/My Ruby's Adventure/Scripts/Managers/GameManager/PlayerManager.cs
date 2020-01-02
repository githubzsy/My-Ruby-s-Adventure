using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 玩家管理类
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// 跟着移动的相机
    /// </summary>
    [Header("跟着移动的相机")]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    internal PlayerAttribute PlayerAttribute;

    /// <summary>
    /// 玩家本地化数据存储位置
    /// </summary>
    internal static string PlayerAttributeJson = "PlayerAttribute.json";

    internal static PlayerManager Instance;

    /// <summary>
    /// 是否从死亡中重置的
    /// </summary>
    private static bool _resetFromDead;

    /// <summary>
    /// 是否切换场景
    /// </summary>
    private static bool _changeScene;

    /// <summary>
    /// 下一个场景的位置
    /// </summary>
    private static Vector3 _nextScenePosition;

    /// <summary>
    /// 玩家是否死了
    /// </summary>
    private bool _isDead;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadPlayer();
    }

    void Start()
    {
        AttachToOthers();
        if (!AudioManager.IsPlayingBgm())
        {
            AudioManager.PlayBgm();
        }
        UIManager.RefreshHp(PlayerAttribute.Hp);
        UIManager.RefreshCherryCount(PlayerAttribute.CherryCount);
    }

    /// <summary>
    /// 读取玩家信息
    /// </summary>
    static void LoadPlayer()
    {
        Instance.PlayerAttribute = SaveManager.ReadFormFile<PlayerAttribute>(PlayerAttributeJson);
        // 如果玩家在本场景有存档，则载入存档位置
        if (SceneManager.GetActiveScene().buildIndex == Instance.PlayerAttribute.SaveSceneIndex)
        {
            Instance.transform.position = Instance.PlayerAttribute.SavePosition;
        }
        else if (_changeScene)
        {
            Instance.transform.position = _nextScenePosition;
            _changeScene = false;
        }

        //如果玩家是从死亡中恢复,则Hp回满
        if (_resetFromDead)
        {
            Instance.PlayerAttribute.Hp = Instance.PlayerAttribute.MaxHp;
            _resetFromDead = false;
        }
        SaveManager.SaveGame();
    }

    /// <summary>
    /// 保存玩家信息
    /// </summary>
    internal static void SavePlayer()
    {
        Instance.PlayerAttribute.SaveSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Instance.PlayerAttribute.SavePosition = Instance.transform.position;
        Instance.PlayerAttribute.SaveToFile(PlayerAttributeJson);
    }

    /// <summary>
    /// 将玩家添加到其它脚本引用上
    /// </summary>
    void AttachToOthers()
    {
        _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (_cinemachineVirtualCamera != null)
        {
            _cinemachineVirtualCamera.Follow = transform;
        }

        DeadLine deadLine = FindObjectOfType<DeadLine>();
        if (deadLine != null)
        {
            deadLine.PlayerController = this;
        }

    }

    /// <summary>
    /// 拾取樱桃
    /// </summary>
    /// <param name="cherry"></param>
    internal static void PickCherry(GameObject cherry)
    {
        Destroy(cherry);
        Instance.PlayerAttribute.CherryCount++;
        if (Instance.PlayerAttribute.CherryCount % 10 == 0)
        {
            Instance.PlayerAttribute.MaxHp++;
            Instance.PlayerAttribute.Hp++;
            UIManager.RefreshHp(Instance.PlayerAttribute.Hp);
            AudioManager.MaxHpIncreaseAudio();
        }
        else AudioManager.CherryAudio();

        UIManager.RefreshCherryCount(Instance.PlayerAttribute.CherryCount);
    }

    /// <summary>
    /// 获取到了技能
    /// </summary>
    /// <param name="skill"></param>
    internal static void PickSkill(GameObject skill)
    {
        AudioManager.SkillAudio();
        Destroy(skill);
    }

    /// <summary>
    /// 增加一次额外跳跃的能力
    /// </summary>
    internal void ExtraJumpIncrease()
    {
        Instance.PlayerAttribute.ExtraJumpCount++;
    }

    /// <summary>
    /// 设定下一个场景的所在位置
    /// </summary>
    /// <param name="position"></param>
    internal static void SetNextScenePosition(Vector3 position)
    {
        _changeScene = true;
        _nextScenePosition = position;
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    private void Reset()
    {
        //重新加载当前场景
        LevelManager.LoadSceneAsync(SceneManager.GetActiveScene().name, Reset_completed);
    }

    /// <summary>
    /// 场景加载完成后打上死亡恢复标签
    /// </summary>
    /// <param name="obj"></param>
    private void Reset_completed(AsyncOperation obj)
    {
        //打上死亡恢复标签，让玩家的Hp填满
        _resetFromDead = true;
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    internal static void PlayerDie()
    {
        //保证玩家死亡效果不会重复触发
        if (Instance != null && Instance._isDead == false)
        {
            Instance._isDead = true;
            if (Instance._cinemachineVirtualCamera != null)
            {
                //相机不再移动
                Instance._cinemachineVirtualCamera.enabled = false;
            }
            PlayerController.DieAnim();
            AudioManager.DeathAudio();
            AudioManager.StopBgm();
            Instance.Invoke("Reset", 2f);
            Instance = null;
        }
    }
}
