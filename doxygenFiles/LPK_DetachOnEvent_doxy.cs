/***************************************************
\file           LPK_DetachOnEvent.cs
\author        Christopher Onorati
\date   2/21/19
\version   2018.3.4
\brief
  This component can be added to any object to have it 
  detach an object from its parent upon receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DetachOnEvent
* \brief Component to unparent an object on events.
**/
public class LPK_DetachOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Game object to detach from parent.  If not set and tag is not set, assume self.")]
    [Rename("Detach Object")]
    public GameObject m_pDetachObject;

    [Tooltip("Tag to detach from parent.  Only used if Detach Object is set to null.  If not set and detach object is not set, assume self.")]
    [TagDropdown]
    public string m_DetachTag;

    [Tooltip("Set to cause the detaching to occur as soon as this object is spawned.")]
    [Rename("Detach On Start")]
    public bool m_bDetachOnStart;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for object detaching from a parent.")]
    public LPK_EventReceivers m_DetachEventReceivers;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object parenting.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_pDetachObject == null && string.IsNullOrEmpty(m_DetachTag))
            m_pDetachObject = gameObject;

        if (m_pDetachObject == null && !string.IsNullOrEmpty(m_DetachTag))
            m_pDetachObject = GameObject.FindGameObjectWithTag(m_DetachTag);

        if (m_bDetachOnStart)
            Detach();
    }

    /**
    * \fn OnEvent
    * \brief Event responding.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");

        Detach();
    }

    /**
    * \fn Detach
    * \brief Detaches two objects together on an event occurance.  Seperated from OnEvent for Start functionality.
    * 
    * 
    **/
    void Detach()
    {
        //Detach object
        if (m_pDetachObject.transform.parent != null)
        {
            m_pDetachObject.transform.parent = null;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Object Detached");

            //Send out event.
            LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, m_DetachEventReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Detached };

            LPK_EventManager.InvokeEvent(sendEvent, sendData);
        }
    }
}
