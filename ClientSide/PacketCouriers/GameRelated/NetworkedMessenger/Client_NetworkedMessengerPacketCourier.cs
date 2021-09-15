﻿//using System;
//using System.Collections.Generic;
//using UnityEngine;

//using ClientSide.Sockets;


////TODO Refazer Server_NetworkedMessengerPacketCourier
//namespace ClientSide.PacketCouriers.NetworkedMessenger
//{
//    //Será Equivalente ao GlobalMessenger normal (só que não)[porém que sim]
//    public class Server_NetworkedMessengerPacketCourier : MonoBehaviour, IPacketCourier
//    {
//        //TODO trocar Client_DynamicPacketCourierHandler pelo "DynamicIO"
//        private Client_DynamicPacketCourierHandler dynamicPacketCourierHandler;
//        const string NM_LOCALIZATION_STRING = "NetworkedMessengerPacketCourier";
//        public int HeaderValue { get; private set; }

//        private Dictionary<long, Delegate> eventTable = new Dictionary<long, Delegate>();
        
//        public void Start()
//        {
//            dynamicPacketCourierHandler = GameObject.Find("QSAClient").GetComponent<ClientMod>()._clientSide.dynamicPacketCourierHandler;
//            dynamicPacketCourierHandler.SetPacketCourier(NM_LOCALIZATION_STRING, OnReceiveHeaderValue);
//        }
//        public ReadPacketHolder.ReadPacket OnReceiveHeaderValue(int HeaderValue)
//        {
//            this.HeaderValue = HeaderValue;
//            return Receive;
//        }

//        public void AddListener(string eventType, Callback handler)
//        {
//            long hash = GerarHash(eventType);

//            if (!eventTable.ContainsKey(hash))
//            {
//                eventTable.Add(hash, null);
//                PacketWriter packet = new PacketWriter();
//                packet.Write((byte)0);
//                packet.Write(hash);
//                dynamicPacketCourierHandler.DynamicPacketIO.SendPackedData((byte)HeaderValue, packet.GetBytes());
//            }

//            eventTable[hash] = (Callback)Delegate.Combine((Callback)eventTable[hash], handler);
//        }

//        public void RemoveListener(string eventType, Callback handler)
//        {
//            long hash = GerarHash(eventType);

//            if (eventTable.ContainsKey(hash))
//            {
//                eventTable[hash] = (Callback)Delegate.Remove((Callback)eventTable[hash], handler);

//                PacketWriter packet = new PacketWriter();
//                packet.Write((byte)1);
//                packet.Write(hash);
//                dynamicPacketCourierHandler.DynamicPacketIO.SendPackedData((byte)HeaderValue, packet.GetBytes());

//                if (eventTable[hash] == null)
//                    eventTable.Remove(hash);
//            }
//        }
        
//        public void FireEvent(long hash)
//        {
//            Delegate @delegate;
//            if (eventTable.TryGetValue(hash, out @delegate))
//                ((Callback)@delegate)?.Invoke();
//        }

//        public void Receive(int latency, DateTime sentPacketTime, byte[] data)
//        {
//            FireEvent(BitConverter.ToInt64(data, 0));
//        }

//    }
//}
