using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGenPattern : MonoBehaviour {

    public List<BubbleObject.BubbleTypes> BubbleTypes = new List<BubbleObject.BubbleTypes>();

    private void Awake()
    {
        if (BubbleTypes.Count == 0)
        {
            BubbleTypes.Add(BubbleObject.BubbleTypes.Bubble_1);
            BubbleTypes.Add(BubbleObject.BubbleTypes.Bubble_2);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
