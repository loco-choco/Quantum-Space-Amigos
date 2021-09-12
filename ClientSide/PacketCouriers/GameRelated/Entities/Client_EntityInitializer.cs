﻿using System;
using System.Collections;
using System.Collections.Generic;
using ClientSide.PacketCouriers.Essentials;
using ClientSide.Sockets;
using ClientSide.Utils;
using UnityEngine;


namespace ClientSide.PacketCouriers.GameRelated.Entities
{
    static class InstantiadableGameObjectsPrefabHub
    {
        public static readonly Dictionary<string, GameObject> instantiadableGOPrefabs = new Dictionary<string, GameObject>();

        public static readonly Dictionary<int, NetworkedEntity> networkedEntities = new Dictionary<int, NetworkedEntity>();

        public static void AddPrefab(GameObject gameObject, string prefabName)
        {
            if (instantiadableGOPrefabs.ContainsKey(prefabName))
                throw new OperationCanceledException(string.Format("There is already a GameObject in {0}", prefabName));

            instantiadableGOPrefabs.Add(prefabName, gameObject);
        }

        public static List<int> GetAllEntitesIds()
        {
            return new List<int>(networkedEntities.Keys);
        }

        public static void AddGameObject(NetworkedEntity networkedEntity)
        {
            if (networkedEntities.ContainsKey(networkedEntity.id))
                throw new OperationCanceledException(string.Format("The id {0} is already being used!", networkedEntity.id));

            networkedEntities.Add(networkedEntity.id, networkedEntity);
        }

        public static bool RemoveGameObject(int id)
        {
            if (!networkedEntities.ContainsKey(id))
                return false;
            networkedEntities.Remove(id);
            return true;
        }

        /// <summary>
        /// Same as RemoveGameObject(int id), but if ForceValueRemove is set to true it will search for the key in the dictionary KeyValuesPairs if just using the id fails.
        /// </summary>
        /// <param name="networkedEntity"></param>
        /// <param name="ForceValueRemove"></param>
        /// <returns></returns>
        public static bool RemoveGameObject(NetworkedEntity networkedEntity, bool ForceValueRemove = false)
        {
            bool resultOfFirstAttempt = RemoveGameObject(networkedEntity.id);
            if (!ForceValueRemove || resultOfFirstAttempt)
                return resultOfFirstAttempt;

            if (!networkedEntities.ContainsValue(networkedEntity))
                return false;

            int key = -1;
            foreach (var keyPar in networkedEntities)
            {
                if (keyPar.Value == networkedEntity) { key = keyPar.Key; break; }
            }
            networkedEntities.Remove(key);
            return true;
        }

        public static void ResetInstantiadableGameObjectsPrefabHub()
        {
            instantiadableGOPrefabs.Clear();
            networkedEntities.Clear();
        }
    }

    class Client_EntityInitializer : MonoBehaviour
    {
        public static Client_EntityInitializer client_EntityInitializer;

        public Client_DynamicPacketIO DynamicPacketIO { get; private set; }
        const string EI_LOCALIZATION_STRING = "EntityInitializer";
        public int HeaderValue { get; private set; }

        public void Awake()
        {
            if (client_EntityInitializer != null)
            {
                Destroy(this);
                return;
            }
            client_EntityInitializer = this;

            Client_DynamicPacketCourierHandler handler = Client.GetClient().dynamicPacketCourierHandler;
            handler.SetPacketCourier(EI_LOCALIZATION_STRING, OnReceiveHeaderValue);
            DynamicPacketIO = handler.DynamicPacketIO;
        }
        private ReadPacketHolder.ReadPacket OnReceiveHeaderValue(int HeaderValue)
        {
            this.HeaderValue = HeaderValue;
            return ReadPacket;
        }

        public void OnDestroy()
        {
            if(client_EntityInitializer == this)
                InstantiadableGameObjectsPrefabHub.ResetInstantiadableGameObjectsPrefabHub();
        }

        public void AddGameObjectPrefab(string gameObjectName, GameObject gameObject)
        {
            InstantiadableGameObjectsPrefabHub.AddPrefab(gameObject, gameObjectName);
            gameObject.SetActive(false);
        }

        private void InstantiateEntity(string prefabName, int ID, Vector3 position, Quaternion rotation, params byte[][] data)
        {
            if (!InstantiadableGameObjectsPrefabHub.instantiadableGOPrefabs.TryGetValue(prefabName, out GameObject prefab))
                throw new OperationCanceledException(string.Format("There is no GameObject in {0}", prefabName));

            GameObject gameObject = (GameObject)Instantiate(prefab, position, rotation);
            NetworkedEntity networkedEntity = gameObject.GetAttachedNetworkedEntity();
            networkedEntity.SetInstantiateVariables(prefabName, ID, data);
            gameObject.SetActive(true);
            InstantiadableGameObjectsPrefabHub.AddGameObject(networkedEntity);
        }

        public void DestroyEntity(NetworkedEntity networkedEntity)
        {
            InstantiadableGameObjectsPrefabHub.RemoveGameObject(networkedEntity, true);
            Destroy(networkedEntity.gameObject);
        }
        
        private void ReceiveAddEntities(ref PacketReader reader)
        {
            int amount = reader.ReadInt32();
            for (int i = 0; i < amount; i++)
                ReadEntityInstantiateData(ref reader);
        }

        private void ReceiveRemoveEntities(ref PacketReader reader)
        {
            int[] ids = reader.ReadInt32Array();
            for(int i=0;i< ids.Length;i++)
                DestroyEntity(InstantiadableGameObjectsPrefabHub.networkedEntities[ids[i]]);
        }

        private void ReadEntityInstantiateData(ref PacketReader reader)
        {
            string prefabName = reader.ReadString();
            int id = reader.ReadInt32();
            byte[][] intantiateData = reader.ReadByteMatrix();
            Vector3 position = reader.ReadVector3();
            Quaternion rotation = reader.ReadQuaternion();

            InstantiateEntity(prefabName, id, position, rotation, intantiateData);
        }

        public void ReadPacket(byte[] data)
        {
            PacketReader reader = new PacketReader(data);
            switch ((EntityInitializerHeaders)reader.ReadByte())
            {
                case EntityInitializerHeaders.Instantiate:
                    ReceiveAddEntities(ref reader);
                    break;
                case EntityInitializerHeaders.Remove:
                    ReceiveRemoveEntities(ref reader);
                    break;
            }
        }

        enum EntityInitializerHeaders : byte
        {
            Instantiate,
            Remove
        }
    }
}

