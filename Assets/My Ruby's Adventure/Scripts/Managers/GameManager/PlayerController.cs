using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 当前刚体
    /// </summary>
    private Rigidbody2D _rigidbody2D;

    private Animator _animator;

    /// <summary>
    /// 头部碰撞体
    /// </summary>
    private BoxCollider2D _boxCollider2D;

    /// <summary>
    /// 跟着移动的相机
    /// </summary>
    [Header("跟着移动的相机")]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    /// <summary>
    /// 受伤后的无敌时间
    /// </summary>
    private float _invisibleTime = 0.7f;

    /// <summary>
    /// 可以受伤的时间
    /// </summary>
    private float _nextHurtTime;

    private static PlayerController _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _nextHurtTime = Time.time;
    }

    void Start()
    {
        if (!AudioManager.IsPlayingBgm())
        {
            AudioManager.PlayBgm();
        }
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        Run();
    }

    /// <summary>
    /// 跑步
    /// </summary>
    private void Run()
    {
        //横向移动
        var h = InputManager.GetAxis(MoveAxis.Horizontal);
        var v = InputManager.GetAxis(MoveAxis.Vertical);
        Vector2 moveDirection = new Vector2(h, v);
        if (h != 0 || v != 0)
        {
            //设定朝向
            _animator.SetFloat("Look X", moveDirection.x);
            _animator.SetFloat("Look Y", moveDirection.y);

            Vector3 position = _rigidbody2D.position;
            position.x += moveDirection.x * PlayerManager.Instance.PlayerAttribute.Speed * Time.fixedDeltaTime;
            position.y += moveDirection.y * PlayerManager.Instance.PlayerAttribute.Speed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(position);
        }
        //设定速度
        _animator.SetFloat("Speed", moveDirection.magnitude * PlayerManager.Instance.PlayerAttribute.Speed);
    }

    /// <summary>
    /// 玩家死亡动画
    /// </summary>
    internal static void DieAnim()
    {
        _instance._boxCollider2D.enabled = false;
        _instance._animator.SetBool("hurt", true);
    }


}
