using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BubbleChainFinder
{
    List<BubbleObject> bloomList = new List<BubbleObject>();
    List<BubbleObject> testList = new List<BubbleObject>();

    BubbleManager generater = null;

    HashSet<BubbleObject.BubbleTypes> allTypes = new HashSet<BubbleObject.BubbleTypes>();

    public List<BubbleObject> BloomList
    {
        get
        {
            return bloomList;
        }
    }

    public BubbleChainFinder(BubbleManager g)
    {
        generater = g;
        bloomList.Clear();
        testList.Clear();
        for(int i=0;i<(int)BubbleObject.BubbleTypes.BubbleType_Max;i++)
        {
            allTypes.Add((BubbleObject.BubbleTypes)i);
        }
    }

    public void Reset()
    {
        bloomList.Clear();
        testList.Clear();
    }

    public List<Vector2Int> FindEmptyNeighborsInHex(Vector2Int targetInHex)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        for (int q = -1; q <= 1; q++)
        {
            for (int r = -1; r <= 1; r++)
            {
                if (r == q)
                    continue;

                Vector2Int neighborHexIndex = targetInHex + new Vector2Int(q, r);
                bool isInMap = generater.IsInMap(neighborHexIndex);
                if (isInMap && generater.GetBubbleInMap(neighborHexIndex) == null)
                    neighbors.Add(neighborHexIndex);
            }
        }

        return neighbors;
    }

    public void FindBloomBubbles(BubbleObject current, HashSet<BubbleObject.BubbleTypes> types)
    {
        if (!bloomList.Contains(current))
        {
            bloomList.Add(current);
            //Debug.Log(bloomList);
        }

        var tempList = FindNeighborWithType(current, types);
        testList.AddRange(tempList);
        testList.Remove(current);
        for (int i = 0; i < testList.Count; i++)
        {
            var b = testList[i];
            FindBloomBubbles(b, types);
        }
    }

    List<BubbleObject> FindNeighborWithType
        (BubbleObject current, HashSet<BubbleObject.BubbleTypes> types)
    {
        List<BubbleObject> bubbles = new List<BubbleObject>();

        for (int q = -1; q <= 1; q++)
        {
            for (int r = -1; r <= 1; r++)
            {
                if (r == q)
                    continue;
                Vector2Int neighborHexIndex = current.GridPosition + new Vector2Int(q, r);

                if (generater.IsInMap(neighborHexIndex))
                {
                    var mapBubble = generater.GetBubbleInMap(neighborHexIndex);
                    if (mapBubble != null && types.Contains(mapBubble.BubbleType) && !bloomList.Contains(mapBubble) && !testList.Contains(mapBubble))
                    {
                        bubbles.Add(mapBubble);
                    }
                }

            }
        }
        return bubbles;
    }

    public List<BubbleObject> FindIsolatedBubble()
    {
        int r = 0;
        List<BubbleObject> tempList = new List<BubbleObject>();
        tempList.AddRange(generater.AllBubblesInMap);

        for (int q=0;q< generater.GenerateSize.x; q++)
        {
            Vector2Int indexHex = new Vector2Int(q, r);
            var mapBubble = generater.GetBubbleInMap(indexHex);
            if(mapBubble!=null)
            {
                FindBloomBubbles(mapBubble, allTypes);
                foreach (var b in bloomList)
                {
                    tempList.Remove(b);
                }
                bloomList.Clear();
            }
        }
        return tempList;
    }

}