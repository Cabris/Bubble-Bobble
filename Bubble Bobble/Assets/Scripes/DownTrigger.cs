using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownTrigger : MonoBehaviour {

    public event EventHandler<MessageEventArgs.ReadOnlyEventArgs<BubbleObject>> BubbleTriggered;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var b = collision.GetComponent<BubbleObject>();
        if (b != null && b.BubbleState == BubbleObject.BubbleStates.State_InMap && BubbleTriggered != null)
        {
            BubbleTriggered(this, new MessageEventArgs.ReadOnlyEventArgs<BubbleObject>(b));
        }
    }

}
