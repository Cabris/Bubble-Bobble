using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleObject : MonoBehaviour {

    public enum BubbleStates
    {
        State_InMap,
        State_ReadyForFire,
        State_Bullet,
        State_Fall,
        State_Max
    }

    public enum BubbleTypes
    {
        Bubble_1,
        Bubble_2,
        Bubble_3,
        Bubble_4,
        Bubble_5,
        Bubble_6,
        Bubble_7,
        Bubble_8,
        Bubble_9,
        BubbleType_Max,
        Bubble_Block,
    }

    public bool _enablePhysic=false;
    public bool _enableGravity=false;

    public event EventHandler<MessageEventArgs.ReadOnlyEventArgs<BubbleObject>> BubbleHit;

    [SerializeField]
    TypeSpriteDictionary _typeSpriteDictionary;
 
    [SerializeField]
    BubbleTypes _bubbleType;

    public BubbleTypes BubbleType {
        get { return _bubbleType; }
        set
        {
            _bubbleType = value;
            if(_typeSpriteDictionary.ContainsKey(_bubbleType))
            {
                GetComponent<SpriteRenderer>().sprite = _typeSpriteDictionary[_bubbleType];
            }
        }
    }

    Vector2 BubbleSize
    {
        get { return GetComponent<SpriteRenderer>().bounds.size; }
    }

    public Vector2Int GridPosition { get; private set; }

    BubbleStates _bubbleState;
    public BubbleStates BubbleState {
        get { return _bubbleState; }
        set
        {
            _bubbleState = value;
            BubbleStateChanged(_bubbleState);
        }
    }

    void BubbleStateChanged(BubbleStates state) {
        switch (state)
        {
            case BubbleStates.State_InMap:
                _enablePhysic = true;
                _enableGravity = false;
                GetComponent<Rigidbody2D>().isKinematic = true;
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                gameObject.layer = LayerMask.NameToLayer("Bubble");
                break;
            case BubbleStates.State_Bullet:
                _enablePhysic = true;
                _enableGravity = false;
                GetComponent<Rigidbody2D>().isKinematic = false;
                gameObject.layer = LayerMask.NameToLayer("Bubble");
                break;
            case BubbleStates.State_Fall:
                _enablePhysic = true;
                GetComponent<Rigidbody2D>().isKinematic = false;
                _enableGravity = true;
                gameObject.layer = LayerMask.NameToLayer("NoInteraction");
                break;
            case BubbleStates.State_ReadyForFire:
                _enablePhysic = false;
                _enableGravity = false;
                GetComponent<Rigidbody2D>().isKinematic = true;
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                gameObject.layer = LayerMask.NameToLayer("NoInteraction");
                break;
            default:
                break;
        }
    }

    public void SetPositionInGrid(Vector2Int hex)
    {
        GridPosition = hex;
    }

    public Vector2 GetWorldPosition(Vector2Int hex)
    {
        var size = BubbleSize.x;
        var x = size * (hex.x + hex.y / 2f);
        var y = size * (Mathf.Sqrt(3) / 2f) * hex.y;
        var pos = new Vector2(x, -y) + BubbleManager.TopLeftPosition;
        return pos;
    }

    public Vector2Int GetGridPosition(Vector3 worldP)
    {
        var size = BubbleSize.x;
        Vector2 pos = (Vector2)worldP - BubbleManager.TopLeftPosition;
        pos.y = -pos.y;
        Vector2Int hex = Vector2Int.zero;
        hex.y = (int) (pos.y / (size * (Mathf.Sqrt(3) / 2f)));
        hex.x = (int)((pos.x / size) - (hex.y / 2f));

        return hex;
    }


    // Use this for initialization
    void Start () {
        BubbleType = _bubbleType;
    }
	
	// Update is called once per frame
	void Update () {
        GetComponent<Rigidbody2D>().simulated = _enablePhysic;
        GetComponent<Rigidbody2D>().gravityScale = _enableGravity ?1:0;


        Bounds safeBounds = new Bounds(Vector3.zero, new Vector3(BubbleSize.x*8, BubbleSize.y*15,1));
        if (!safeBounds.Contains(transform.position))
        {
            BubblePool.Instance.ReleaseBubble(this);
        }
        if(BubbleState==BubbleStates.State_InMap)
        {
            Vector2 pos = GetWorldPosition(GridPosition);
            transform.position = new Vector3(pos.x, pos.y, 0);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var bo=collision.gameObject.GetComponent<BubbleObject>();
        if (bo&&BubbleState==BubbleStates.State_Bullet&& bo.BubbleState==BubbleStates.State_InMap)
        {
            BubbleHit(this, new MessageEventArgs.ReadOnlyEventArgs<BubbleObject>(bo));
        }
    }

}
