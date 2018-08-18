using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public event EventHandler MapGenerated;

    [SerializeField]
    BubbleGenPattern _generatePattern;

    [SerializeField]
    Vector2Int generateSize;

    [SerializeField]
    Vector2Int MapSize;

    [SerializeField]
    Transform _topLeftPlaceHolder;

    [SerializeField]
    public static BubbleGenPattern GeneratePattern;

    [SerializeField]
    float _falldownSize=0.5f;

    BubbleObject[,] _bubbleMap;

    List<BubbleObject> _tempBubbleInMap = new List<BubbleObject>();
    List<BubbleObject> _topBubbleWall = new List<BubbleObject>();

    Vector3 _initialPlaceHolderPos;

    private void Awake()
    {
        if (_generatePattern == null)
        {
            _generatePattern = gameObject.AddComponent<BubbleGenPattern>();
        }
        GeneratePattern = _generatePattern;
        _bubbleMap = new BubbleObject[MapSize.x, MapSize.y];
        _tempBubbleInMap.Clear();
    }

    // Use this for initialization
    void Start()
    {
        TopLeftPosition = _topLeftPlaceHolder.position;
        _initialPlaceHolderPos = TopLeftPosition;
    }

    public void Reset()
    {
        _topLeftPlaceHolder.position = _initialPlaceHolderPos;

        foreach(var b in _bubbleMap)
        {
            if (b != null)
            {
                RemoveBubble(b);
                BubblePool.Instance.ReleaseBubble(b);
            } 
        }
        foreach( var b in _topBubbleWall)
        {
            BubblePool.Instance.ReleaseBubble(b);
        }

        _bubbleMap = new BubbleObject[MapSize.x, MapSize.y];
        _tempBubbleInMap.Clear();
        _topBubbleWall.Clear();
    }

    public void FallDownMap()
    {
        _topLeftPlaceHolder.Translate(Vector3.down * _falldownSize);
    }

    public bool IsInMap(Vector2Int hexIndex)
    {
        var arrayIndex = Hex2Array(hexIndex);
        var ret = (arrayIndex.x >= 0 && arrayIndex.x < MapSize.x) && (arrayIndex.y >= 0 && arrayIndex.y < MapSize.y);
        return ret;
    }

    public BubbleObject GetBubbleInMap(Vector2Int hexIndex)
    {
        if (IsInMap(hexIndex))
        {
            var arrayIndex = Hex2Array(hexIndex);
            return _bubbleMap[arrayIndex.x, arrayIndex.y];
        }
        else
            return null;
    }

    public BubbleObject GetLowestBubbleInMap()
    {
        for(int y = MapSize.y - 1; y>=0;y--)
        {
            for(int x=0;x < MapSize.x;x++)
            {
                var b= _bubbleMap[x, y];
                if (b != null)
                    return b;
            }
        }
            return null;
    }

    public void GenerateMap()
    {
        for (int r = -1; r < GenerateSize.y; r++)
        {
            int offset = -Mathf.FloorToInt(((float)r) / 2f);
            int gridSizeX = (int)(r % 2 == 0 ? GenerateSize.x : GenerateSize.x - 1);

            for (int q = 0; q < gridSizeX; q++)
            {
                //int bubbleTypeInt = PerlinNoise(q, r, 0,1f, _generatePattern.BubbleTypes.Count, 1);
                int emptyFact = PerlinNoise(q - r, r, q, 0.1f, 100, 1);
                Vector2Int hex = new Vector2Int(q + offset, r);
                if (r < 0)//top compressor
                {
                    var bubble = BubblePool.Instance.AcquireBubble(BubbleObject.BubbleTypes.Bubble_Block);
                    bubble.SetPositionInGrid(hex);
                    bubble.BubbleState = BubbleObject.BubbleStates.State_InMap;

                    Color color = Color.white;
                    color.a = 0;
                    bubble.GetComponent<SpriteRenderer>().color = color;
                    _topBubbleWall.Add(bubble);
                }
                else
                {
                    if (emptyFact >= 60)
                    {
                        var p = Hex2Array(hex);
                        _bubbleMap[p.x, p.y] = null;
                    }
                    else
                    {
                        int bubbleTypeInt = UnityEngine.Random.Range(0, _generatePattern.BubbleTypes.Count);
                        BubbleObject.BubbleTypes bubbleType = _generatePattern.BubbleTypes[bubbleTypeInt];
                        var bubble = BubblePool.Instance.AcquireBubble(bubbleType);
                        if (bubble != null)
                        {
                            AddBubble(bubble, hex);
                        }
                    }
                }

            }
        }

        if (MapGenerated != null)
            MapGenerated(this, null);
    }

    public void AddBubble(BubbleObject bubble, Vector2Int hex)
    {
        bubble.SetPositionInGrid(hex);
        bubble.BubbleState = BubbleObject.BubbleStates.State_InMap;

        var p = Hex2Array(hex);
        _bubbleMap[p.x, p.y] = bubble;
        if (!_tempBubbleInMap.Contains(bubble))
            _tempBubbleInMap.Add(bubble);
    }

    public void RemoveBubble(BubbleObject bubble)
    {
        var hex = bubble.GridPosition;
        var p = Hex2Array(hex);
        _bubbleMap[p.x, p.y] = null;
        _tempBubbleInMap.Remove(bubble);
    }

    int PerlinNoise(int x, int y, int z, float scale, float height, float power)
    {
        float rValue;
        rValue = Noise.GetNoise(((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
        rValue *= height;

        if (power != 0)
        {
            rValue = Mathf.Pow(rValue, power);
        }

        return (int)rValue;
    }

    // Update is called once per frame
    void Update()
    {
        TopLeftPosition = _topLeftPlaceHolder.position;
    }

    public static Vector2 TopLeftPosition
    {
        get; set;
    }

    public Vector2Int GenerateSize
    {
        get
        {
            return generateSize;
        }
    }

    public List<BubbleObject> AllBubblesInMap
    {
        get
        {
            return _tempBubbleInMap;
        }
    }

    public static Vector2Int Hex2Array(Vector2Int hexIndex)
    {
        Vector2Int arrayIndex = Vector2Int.zero;
        arrayIndex.y = hexIndex.y;

        arrayIndex.x = hexIndex.x + (hexIndex.y / 2);

        return arrayIndex;
    }

}
