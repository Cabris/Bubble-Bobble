
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MessageEventArgs
{
    public class BubbleCreatedEventArgs : EventArgs
    {
        public BubbleCreatedEventArgs(BubbleObject bubbleObject)
        {
            this.BubbleObj = bubbleObject;
        }
        public BubbleObject BubbleObj { get; set; }
    }

    public class ReadOnlyEventArgs<T> : EventArgs
    {
        public T Parameter { get; private set; }

        public ReadOnlyEventArgs(T input)
        {
            Parameter = input;
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Parameter { get; set; }

        public EventArgs(T input)
        {
            Parameter = input;
        }
    }


}



