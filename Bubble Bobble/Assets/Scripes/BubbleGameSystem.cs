using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleGameSystem : MonoBehaviour
{

    [SerializeField]
    BubbleManager bubbleManager;

    [SerializeField]
    ShooterController _shooterController;

    [SerializeField]
    DownTrigger _downTrigger;

    BubbleChainFinder chainFinder;

    int fireCount = 0;

    [SerializeField]
    int _mapFalldownCycle = 3;

    [SerializeField]
    Text _gameoverMsg;

    [SerializeField]
    Text _winMsg;

    enum GamePlayStates
    {
        Initialize,
        Playing,
        GameOver,
        YouWin,
        PlayStateMax,
    }

    [SerializeField]
    GamePlayStates _gamePlayState;

    GamePlayStates GamePlayState
    {
        get { return _gamePlayState; }
        set
        {
            _gamePlayState = value;
            switch (_gamePlayState)
            {
                case GamePlayStates.Initialize:
                    CloseMsg(_gameoverMsg);
                    CloseMsg(_winMsg);
                    break;
                case GamePlayStates.Playing:
                    _shooterController.CanFire = true;
                    _shooterController.Reload();
                    CloseMsg(_gameoverMsg);
                    CloseMsg(_winMsg);
                    break;
                case GamePlayStates.GameOver:
                    fireCount = 0;
                    _shooterController.CanFire = false;
                    StartCoroutine(RestartGame());
                    OpenMsg(_gameoverMsg);
                    break;
                case GamePlayStates.YouWin:
                    fireCount = 0;
                    _shooterController.CanFire = false;
                    StartCoroutine(RestartGame());
                    OpenMsg(_winMsg);
                    break;
                case GamePlayStates.PlayStateMax:
                    break;
                default:
                    break;
            }
        }
    }

    private void OpenMsg(Text text)
    {
        Color c = text.color;
        c.a = 1;
        text.color = c;
    }

    private void CloseMsg(Text text)
    {
        Color c = text.color;
        c.a = 0;
        text.color = c;
    }

    private void Awake()
    {
        BubblePool.Instance.BubbleCreated += OnBubbleCreated;
        BubblePool.Instance.PoolInitialized += OnPoolInitialized;
        bubbleManager.MapGenerated += OnMapGenerated;
        _shooterController.BubbleFired += OnBubbleFired;
        //_downTrigger.BubbleTriggered += OnBubbleTriggered;
        GamePlayState = GamePlayStates.Initialize;
    }

    // Use this for initialization
    void Start()
    {
        chainFinder = new BubbleChainFinder(bubbleManager);
    }

    //private void OnBubbleTriggered(object sender, MessageEventArgs.ReadOnlyEventArgs<BubbleObject> e)
    //{
    //    //Gameover
    //    if (GamePlayState == GamePlayStates.Playing)
    //    {
    //        GamePlayState = GamePlayStates.GameOver;            
    //    }
    //}

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3.0f);
        bubbleManager.Reset();
        bubbleManager.GenerateMap();
    }

    private void OnBubbleFired(object sender, MessageEventArgs.ReadOnlyEventArgs<BubbleObject> e)
    {
        //BubbleObject bubble = e.Parameter;
        fireCount++;
        if (fireCount % _mapFalldownCycle == 0)
        {
            bubbleManager.FallDownMap();
            CheckGameOver();
        }
    }

    private void OnMapGenerated(object sender, System.EventArgs e)
    {
        GamePlayState = GamePlayStates.Playing;
    }

    private void OnPoolInitialized(object sender, System.EventArgs e)
    {
        _shooterController.Reload();
        bubbleManager.GenerateMap();
    }

    private void OnBubbleCreated(object sender, MessageEventArgs.ReadOnlyEventArgs<BubbleObject> e)
    {
        var bo = e.Parameter;
        bo.BubbleHit += OnBubbleHit;
    }

    private void OnBubbleHit(object sender, MessageEventArgs.ReadOnlyEventArgs<BubbleObject> e)
    {
        _shooterController.Reload();

        var thisBo = sender as BubbleObject;
        var otherBo = e.Parameter;

        Vector2Int gridPos = FindNearestNeighborPos(thisBo, otherBo);
        bubbleManager.AddBubble(thisBo, gridPos);

        #region chain destory
        chainFinder.Reset();

        HashSet<BubbleObject.BubbleTypes> bubbleTypes = new HashSet<BubbleObject.BubbleTypes>();
        bubbleTypes.Add(thisBo.BubbleType);

        chainFinder.FindBloomBubbles(thisBo, bubbleTypes);

        //Debug.Log(chainFinder.BloomList.Count);

        if (chainFinder.BloomList.Count >= 3)
        {
            foreach (var b in chainFinder.BloomList)
            {
                bubbleManager.RemoveBubble(b);
                BubblePool.Instance.ReleaseBubble(b);
            }
        }
        #endregion

        #region fall down isolated
        chainFinder.Reset();

        var IsoBubbles = chainFinder.FindIsolatedBubble();
        foreach (var b in IsoBubbles)
        {
            if (b.BubbleState == BubbleObject.BubbleStates.State_Bullet)
            {
                Debug.Log("!");
            }
            b.BubbleState = BubbleObject.BubbleStates.State_Fall;
            bubbleManager.RemoveBubble(b);

        }
        #endregion
        CheckWin();
        CheckGameOver();
    }

    void CheckGameOver()
    {
        var b= bubbleManager.GetLowestBubbleInMap();
        if(b!=null&& b.transform.position.y <= _downTrigger.transform.position.y)
        {
            if (GamePlayState == GamePlayStates.Playing)
            {
                GamePlayState = GamePlayStates.GameOver;
            }
        }
    }

    void CheckWin()
    {
        if (bubbleManager.AllBubblesInMap.Count==0)
        {
            if (GamePlayState == GamePlayStates.Playing)
            {
                GamePlayState = GamePlayStates.YouWin;
            }
        }

    }

    private Vector2Int FindNearestNeighborPos(BubbleObject thisBo, BubbleObject otherBo)
    {
        Vector2Int otherHexPos = otherBo.GridPosition;
        var neighbors = chainFinder.FindEmptyNeighborsInHex(otherHexPos);

        float currentNearest = float.MaxValue;
        Vector2Int gridPos = thisBo.GetGridPosition(thisBo.transform.position);
        foreach (var pos in neighbors)
        {
            var posW = otherBo.GetWorldPosition(pos);
            Vector2 thisPos = thisBo.transform.position;
            float dist = (thisPos - posW).SqrMagnitude();
            if (dist < currentNearest)
            {
                currentNearest = dist;
                gridPos = pos;
            }
        }

        return gridPos;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
