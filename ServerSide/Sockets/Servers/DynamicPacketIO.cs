﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerSide.Sockets.Servers
{
    public class ReadPacketHolder
    {
        public const byte MAX_AMOUNT_OF_HEADER_VALUES = byte.MaxValue;
        public delegate void ReadPacket(PacketReader packet);

        public byte HeaderValue { get; private set; }
        public ReadPacket PacketRead { get; private set; }

        public ReadPacketHolder(ReadPacket readPacket)
        {
            HeaderValue = GetUniqueHeaderValue();
            PacketRead = readPacket;
        }

        private static byte lastHeaderValue = 0;
        private byte GetUniqueHeaderValue()
        {
            if (lastHeaderValue < MAX_AMOUNT_OF_HEADER_VALUES)
            {
                byte thisHeaderValue = lastHeaderValue;
                lastHeaderValue++;
                return thisHeaderValue;
            }
            else
                return MAX_AMOUNT_OF_HEADER_VALUES;
        }
    }
    //Teremos a classe ReadPacketHolder que guardara todos os dados para que possamos ter um IO dos pacotes
    //E teremos a classe DynamicPacketIO que cuidara dos valores que HeaderValue terão
    public class DynamicPacketIO
    {
        private ReadPacketHolder[] readPacketHolders = new ReadPacketHolder[ReadPacketHolder.MAX_AMOUNT_OF_HEADER_VALUES];
        private PacketWriter packetWriter;
        
        public byte[] GetAllData()
        {
            byte[] data = packetWriter.GetBytes();
            packetWriter = null;
            return data;
        }

        public ref PacketWriter GetPacketWriter(byte HeaderValue)
        {
            if(packetWriter == null)
                packetWriter = new PacketWriter();

            if (readPacketHolders[HeaderValue] != null)
                packetWriter.Write(HeaderValue);

            return ref packetWriter;
        }

        public void ReadReceivedPacket(PacketReader packetReader)
        {
            bool continueLoop = true;
            while (continueLoop)
            {
                try
                {
                    byte HeaderValue = packetReader.ReadByte();
                    if (readPacketHolders[HeaderValue] == null)
                        throw new OperationCanceledException(string.Format("This HeaderValue doesn't exist {0}", HeaderValue));

                    readPacketHolders[HeaderValue].PacketRead(packetReader);
                }
                catch (Exception ex)
                {
                    packetReader.Close();
                    continueLoop = false;
                    if (ex.GetType() != typeof(EndOfStreamException)) //Quando chegar no final da Stream, ele joga um EndOfStreamException, então sabemos que ela acabou sem outros erros
                        throw ex;
                }
            }
        }

        public int AddPacketCourier(ReadPacketHolder.ReadPacket readPacket)
        {
            ReadPacketHolder newReadPacketHolder = new ReadPacketHolder(readPacket);

            if (newReadPacketHolder.HeaderValue == ReadPacketHolder.MAX_AMOUNT_OF_HEADER_VALUES)
                return ReadPacketHolder.MAX_AMOUNT_OF_HEADER_VALUES;

            if (readPacketHolders[newReadPacketHolder.HeaderValue] != null)
                throw new OperationCanceledException(string.Format("This HeaderValue is already being used {0}", newReadPacketHolder.HeaderValue));

            readPacketHolders[newReadPacketHolder.HeaderValue] = newReadPacketHolder;
            return newReadPacketHolder.HeaderValue;
        }
    }
}
