using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterController : MonoBehaviour
{
    public event EventHandler<MessageEventArgs.ReadOnlyEventArgs<BubbleObject>> BubbleFired;


    [SerializeField]
    float _fireForce = 1;

    [SerializeField]
    Transform _fireTrans;

    [SerializeField]
    Transform _waitTrans;

    Queue<BubbleObject> _magzine = new Queue<BubbleObject>();

    public bool CanFire { get; set;}

    private void Awake()
    {
        CanFire = false;
    }

    // Use this for initialization
    void Start()
    {
    }

    public void Reload()
    {
        if (_magzine.Count == 0)
        {
            var bubble1 = AcquireBubbleFromPool();
            var bubble2 = AcquireBubbleFromPool();
            _magzine.Enqueue(bubble1);
            _magzine.Enqueue(bubble2);
        }
        if (_magzine.Count == 1)
        {
            var bubble2 = AcquireBubbleFromPool();
            _magzine.Enqueue(bubble2);
        }

        PlaceBubbles();
    }

    private void PlaceBubbles()
    {
        int index = 0;
        foreach (var b in _magzine)
        {
            if (index == 0)
                b.transform.position = _fireTrans.position;
            else
                b.transform.position = _waitTrans.position;
            index++;
        }
    }

    private BubbleObject AcquireBubbleFromPool()
    {
        int bubbleTypeInt = UnityEngine.Random.Range(0, BubbleManager.GeneratePattern.BubbleTypes.Count);
        BubbleObject.BubbleTypes bubbleType = BubbleManager.GeneratePattern.BubbleTypes[bubbleTypeInt];
        var b = BubblePool.Instance.AcquireBubble(bubbleType);
        b.BubbleState = BubbleObject.BubbleStates.State_ReadyForFire;
        return b;
    }

    void FireBubble()
    {
        if (_magzine.Count==2)
        {
            var currentBullet = _magzine.Dequeue();
            currentBullet.BubbleState = BubbleObject.BubbleStates.State_Bullet;
            Vector2 force = _fireTrans.up * _fireForce;
            currentBullet.GetComponent<Rigidbody2D>().AddForce(force);
            if (BubbleFired != null)
                BubbleFired(this, new MessageEventArgs.ReadOnlyEventArgs<BubbleObject>(currentBullet));
        }

        PlaceBubbles();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)&& CanFire)
        {
            FireBubble();
        }

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPoint2d = new Vector2(worldPoint.x, worldPoint.y);
        Vector2 fireOrig = _fireTrans.position;
        Vector2 direction = worldPoint2d - fireOrig;
        float rot = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

        rot = Mathf.Clamp(rot, -80, 80);
        _fireTrans.localRotation = Quaternion.Euler(0, 0, rot);

    }
}
