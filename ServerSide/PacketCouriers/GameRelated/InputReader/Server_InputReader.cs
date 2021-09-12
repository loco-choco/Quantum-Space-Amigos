﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using ServerSide.Sockets.Servers;
using ServerSide.Sockets;

using ServerSide.PacketCouriers.Essentials;

namespace ServerSide.PacketCouriers.GameRelated.InputReader
{
    public class Server_InputReader : MonoBehaviour
    {
        public Server_DynamicPacketIO DynamicPacketIO { get; private set; }

        const string IR_LOCALIZATION_STRING = "InputReader";
        public int HeaderValue { get; private set; }

        private Dictionary<string, ClientInputChannels> ClientsInputChannels;

        public void Start()
        {
            Server_DynamicPacketCourierHandler handler = Server.GetServer().dynamicPacketCourierHandler;
            HeaderValue = handler.AddPacketCourier(IR_LOCALIZATION_STRING, ReadPacket);
            DynamicPacketIO = handler.DynamicPacketIO;

            ClientsInputChannels = new Dictionary<string, ClientInputChannels>();

            Server.GetServer().NewConnectionID += Server_InputReader_NewConnectionID;
            Server.GetServer().DisconnectionID += Server_InputReader_DisconnectionID;
        }
        public void OnDestroy()
        {
            Server.GetServer().NewConnectionID -= Server_InputReader_NewConnectionID;
            Server.GetServer().DisconnectionID -= Server_InputReader_DisconnectionID;
        }

        private void Server_InputReader_NewConnectionID(string clientID)
        {
            ClientsInputChannels.Add(clientID, new ClientInputChannels());
        }
        private void Server_InputReader_DisconnectionID(string clientID)
        {
            ClientsInputChannels.Remove(clientID);
        }

        public void Update()
        {
        }
        public void WriteInputChannelData(ref PacketWriter writer, InputChannel inputChannel)
        {
            writer.Write(inputChannel.GetAxis());
            writer.Write(inputChannel.GetAxisRaw());
            writer.Write(inputChannel.GetButton());
            writer.Write(inputChannel.GetButtonDown());
            writer.Write(inputChannel.GetButtonUp());
        }        
        public void ReadPacket(byte[] data, string ClientID)
        {
            PacketReader reader = new PacketReader(data);
            if (ClientsInputChannels.TryGetValue(ClientID, out ClientInputChannels inputChannels))
                inputChannels.ReadClienetInputChannelsData(ref reader);
        }
    }
}
