﻿
namespace RtmpSharp.Messaging.Events
{
    class Acknowledgement : RtmpEvent
    {
        public int BytesRead { get;}

        public Acknowledgement(int bytesRead) : base(Net.MessageType.Acknowledgement)
        {
            BytesRead = bytesRead;
        }
    }
}
