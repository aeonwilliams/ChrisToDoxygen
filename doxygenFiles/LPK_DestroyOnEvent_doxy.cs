/***************************************************
\file           LPK_DestroyOnEvent.cs
\author        Christopher Onorati
\date   2/14/2019
\version   2018.3.4

\brief
  This component can be added to any object to cause 
  it to destroy specified target(s) upon receiving a
  specified event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DestroyOnEvent
* \brief Destroys an object on parsing user-specified event.
**/
public class LPK_DestroyOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Object to destroy on receiving event.  If this array and the tag array are set to length 0, assume self.")]
    public GameObject[] m_DestructionTargets;

    [Tooltip("Tagged objects to destroy on receiving event.  If this array and the game object array are set to length 0, assume self.")]
            [TagDropdown]

    public string[] m_DestructionTags;

    [Tooltip("How long until the object is destroyed in seconds once the event is received.")]
    [Rename("Destruction Delay")]
    public float m_flDelay;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Object for object destruction.")]
    public LPK_EventReceivers ObjectDeletedReceivers;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object destruction.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_DestructionTargets.Length == 0 && m_DestructionTags.Length == 0)
            m_DestructionTargets = new GameObject[] { gameObject };
    }

    /**
    * \fn OnEvent
    * \brief Destroys an object with an optiona delay.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Prevents odd frame delay.
        if (m_flDelay <= 0)
            DestroyTarget();
        else
            StartCoroutine(DestructionDelay());
    }

    /**
    * \fn DestructionDelay
    * \brief Forces delay before destroying set object.
    * 
    * 
    **/
    IEnumerator DestructionDelay()
    {
        yield return new WaitForSeconds(m_flDelay);
        DestroyTarget();
    }

    /**
    * \fn DestroyTarget
    * \brief Manages object destruction.
    * 
    * 
    **/
    public void DestroyTarget()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Destroying game objects...");

        //Destroy game objects.
        for (int i = 0; i < m_DestructionTargets.Length; i++)
            Destroy(m_DestructionTargets[i]);

        //Destroy tagged objects.
        for (int i = 0; i < m_DestructionTags.Length; i++)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(m_DestructionTags[i]);

            for (int j = 0; j < taggedObjects.Length; j++)
                Destroy(taggedObjects[j]);
        }

        DispatchDestructionEvent();
    }

    /**
    * \fn DispatchDestructionEvent
    * \brief Sends out the object destroyed event.
    * 
    * 
    **/
    void DispatchDestructionEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, ObjectDeletedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GameObjectDestroy };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}
