/***************************************************
\file           LPK_SpawnOnEvent.cs
\author        Christopher Onorati
\date   2/27/2019
\version   2018.3.4

\brief
  This component can be attached to any object to cause
  it to spawn an object upon receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_SpawnOnEvent
* \brief Component to spawn an object on events.
**/
public class LPK_SpawnOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("What prefab to instantiate upon receiving the event.")]
    [Rename("Prefab")]
    public GameObject m_pPrefabToSpawn;
  
    [Tooltip("Object whose position will be the spawn location.")]
    [Rename("Spawn Transform Object")]
    public GameObject m_pTargetSpawnTransformObj;

    [Tooltip("If set perform a spawn 'wave' on game object spawn.")]
    [Rename("Spawn On Start")]
    public bool m_bSpawnOnStart;

    [HideInInspector]
    [Tooltip("Time to delay spawn by in seconds.  Useful for respawning a dead character on the same frame it is destroyed or similar functionality.")]
    [Rename("Delay Time")]
    public float m_flDelayTime = 0.0f;

    [Tooltip("Whether the rotation of TargetSpawnTransformObj should be copied to the spawned object.")]
    [Rename("Copy Target Rotation")]
    public bool m_bCopyTargetRotation = false;

    [Tooltip("Assign this string to the objects spawned as their name.")]
    [Rename("Override Name")]
    public string m_sNewObjectName;

    [Tooltip("Parent the created prefab to the spawner object.")]
    [Rename("Assign Parent Spawner")]
    public bool m_bAttachToSpawner;

    [Tooltip("Parent the created prefab to the transform spawn location object.")]
    [Rename("Assign Parent Target")]
    public bool m_bAttachToSpawnTarget;

    [Tooltip("How many instances of the archetype to spawn everytime an event is received")]
    [Rename("Spawns Per Event")]
    public int m_iSpawnPerEventCount = 1;
  
    [Tooltip("Total maximum number of instances this object is allowed to spawn. (0 means no limit).")]
    [Rename("Max Spawns")]
    public int  m_iMaxTotalSpawnCount = 0;

    [Tooltip("Max amount of objects to have active at once.  (0 means no limit).")]
    [Rename("Max Alive")]
    public int m_iMaxAliveAtOnce = 0;

    [Tooltip("Offset from spawn position.")]
    [Rename("Offset")]
    public Vector3 m_vecOffset;

    [Tooltip("Random variance applied to the spawn position. A value of (2, 0, 0) will apply a random offset of -2 to 2 to the X value of the spawn position")]
    [Rename("Random Offset Variance")]
    public Vector3 m_vecRandomOffsetVariance;

    [Tooltip("Amount of time to wait (in seconds) until an event can trigger another spawn.")]
    [Rename("Cooldown")]
    public float m_flCooldown = 0.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("receiver Game Object for spawn event.  The object spawned is automatically added to this array.")]
    public GameObject[] m_SpawnObjectReceivers;

    /************************************************************************************/

    //Whether this component is waiting its cooldown
    bool m_bOnCooldown = false;
  
    //Number of objects spawned by this component so far
    int m_iSpawnCount = 0;

    List<GameObject> m_pActiveList = new List<GameObject>();

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object spawning.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_pTargetSpawnTransformObj == null)
        {
            m_pTargetSpawnTransformObj = gameObject;

            if(m_bPrintDebug)
                LPK_PrintDebug(this, "Target Transform not found.  Assigning to self.");
        }

        //NOTENOTE: Spawn object on start does not trigger the cooldown.
        if (m_bSpawnOnStart)
            SpawnObject();
    }

    /**
    * \fn OnEvent
    * \brief Manages spawning in of prefabs.
    * 
    * 
    **/
    override protected void OnEvent (LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bOnCooldown)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "On Cooldown");

            return;
        }

        m_bOnCooldown = true;

        CleanUpList();

        //HACKHACK: Fixes a bug where spawning and destroying an object with the same tag (for example destroing a player via tag player, and then spawning the player) on the same frame.
        //          causes both the dead player and the respawned player to be deleted.  This delays that respawned object from appearing for another frame.
        StartCoroutine(DelaySpawn());

        StartCoroutine(DelayCoolDown());
    }

    /**
    * \fn DelaySpawn
    * \brief Forces delay before spawning set object.
    * 
    * 
    **/
    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(m_flDelayTime);
        SpawnObject();
    }

    /**
    * \fn SpawnObject
    * \brief Spawn the desired object.  Public so the Unity UI system can interact with this function.
    * 
    * 
    **/
    public void SpawnObject()
    {
        //Spawn the objects
        for (int i = 0; i < m_iSpawnPerEventCount; ++i)
        {
            if (m_iMaxTotalSpawnCount == 0 || m_iSpawnCount < m_iMaxTotalSpawnCount)
            {
                //Too many alive objects, but still count the spawn.
                //NOTENOTE: If you want failed spawns not to count, remove the increment line below.
                if(m_pActiveList.Count >= m_iMaxAliveAtOnce && m_iMaxAliveAtOnce != 0)
                {
                    m_iSpawnCount++;
                    return;
                }

                float randX = Random.Range(m_vecOffset.x - m_vecRandomOffsetVariance.x, m_vecOffset.x + m_vecRandomOffsetVariance.x);
                float randY = Random.Range(m_vecOffset.y - m_vecRandomOffsetVariance.y, m_vecOffset.y + m_vecRandomOffsetVariance.y);
                float randZ = Random.Range(m_vecOffset.z - m_vecRandomOffsetVariance.z, m_vecOffset.z + m_vecRandomOffsetVariance.z);

                GameObject obj = (GameObject)Instantiate(m_pPrefabToSpawn, m_pTargetSpawnTransformObj.transform.position + new Vector3(randX, randY, randZ), Quaternion.identity);

                if (m_bCopyTargetRotation)
                    obj.transform.rotation = m_pTargetSpawnTransformObj.transform.rotation;

                if (m_bAttachToSpawnTarget)
                    obj.transform.SetParent(m_pTargetSpawnTransformObj.transform);

                if (m_bAttachToSpawner)
                    obj.transform.SetParent(gameObject.transform);

                if (!string.IsNullOrEmpty(m_sNewObjectName))
                    obj.name = m_sNewObjectName;

                //Dispatch spawn event
                GameObject[] temp = new GameObject[m_SpawnObjectReceivers.Length + 1];

                for (int j = 0; j < m_SpawnObjectReceivers.Length; j++)
                    temp[j] = m_SpawnObjectReceivers[j];

                temp[temp.Length - 1] = obj;

                //Dispatch spawn event
                LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, null);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_ObjectSpawned };

                LPK_EventManager.InvokeEvent(sendEvent, sendData);

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Object Spawned");

                m_iSpawnCount++;
                m_pActiveList.Add(obj);
            }
        }
    }


    /**
    * \fn CleanUpList
    * \brief Remove any nullified objects from the list.
    * 
    * 
    **/
    void CleanUpList()
    {
        for (int j = 0; j < m_pActiveList.Count; j++)
        {
            GameObject checkObj = m_pActiveList[j];

            if (checkObj == null)
                m_pActiveList.RemoveAt(j);
        }
    }

    /**
    * \fn DelayCoolDown
    * \brief Creates a delay between spawn waves.
    * 
    * 
    **/
    IEnumerator DelayCoolDown()
    {
        yield return new WaitForSeconds(m_flCooldown);
        m_bOnCooldown = false;
    }
}
