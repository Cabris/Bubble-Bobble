using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePool : Singleton<BubblePool>
{
    const int MAX_SIZE = 120;

    List<BubbleObject> bubblePool = new List<BubbleObject>();

    [SerializeField]
    BubbleObject bubblePrefab;

    [SerializeField]
    int poolSize = 0;

    public event EventHandler<MessageEventArgs.ReadOnlyEventArgs<BubbleObject>> BubbleCreated;
    public event EventHandler PoolInitialized;


    // Use this for initialization

    private void Awake()
    {

    }

    void Start()
    {
        GeneratePool();
    }

    private void GeneratePool()
    {
        for (int i = 0; i < MAX_SIZE; i++)
        {
            GameObject bubble = Instantiate(bubblePrefab.gameObject, transform);
            bubble.SetActive(false);
            var bubbleObject = bubble.GetComponent<BubbleObject>();
            bubblePool.Add(bubbleObject);

            if (BubbleCreated != null)
            {
                BubbleCreated(this, new MessageEventArgs.ReadOnlyEventArgs<BubbleObject>(bubbleObject));
            }
        }
        poolSize = bubblePool.Count;
        if (PoolInitialized != null)
            PoolInitialized(this, null);
    }

    public BubbleObject AcquireBubble(BubbleObject.BubbleTypes bubbleType)
    {
        if (poolSize > 0)
        {
            var bubble = bubblePool[poolSize - 1];
            bubblePool.Remove(bubble);
            poolSize--;
            bubble.gameObject.SetActive(true);
            bubble.GetComponent<SpriteRenderer>().color = Color.white;
            bubble.BubbleType = bubbleType;
            return bubble;
        }
        else
            return null;
    }

    public void ReleaseBubble(BubbleObject bubble)
    {
        bubble.gameObject.SetActive(false);
        bubble.transform.position = Vector3.zero;
        bubblePool.Add(bubble);
        poolSize++;
    }

    public int MaxObjectSize
    {
        get { return MAX_SIZE; }
    }

    public int PoolObjectSize
    {
        get { return poolSize; }
    }

}

