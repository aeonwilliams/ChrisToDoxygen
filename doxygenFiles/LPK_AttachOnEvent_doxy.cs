/***************************************************
\file           LPK_AttachOnEvent.cs
\author        Christopher Onorati
\date   2/21/2019
\version   2018.3.4

\brief
  This component causes and object to be attached 
  (parented) to another upon receiving a specified event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_AttachOnEvent
* \brief Component to enable parenting of objects on events.
**/
public class LPK_AttachOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Parent game object to attach the child to.  If not set and tag is not set, assume self.")]
    [Rename("Parent")]
    public GameObject m_pParentObject;

    [Tooltip("Tag to find the parent with.  Only used if Parent is set to null.  If not set and Parent is not set, assume self.")]
    [TagDropdown]
    public string m_ParentTag;

    [Tooltip("Child to attach to the parent.  If not set and tag is not set, assume self.")]
    [Rename("Child")]
    public GameObject m_pChildObject;

    [Tooltip("Tag to find the child with.  Only used if Child is set to null.  If not set and Child is not set, assume self.")]
    [TagDropdown]
    public string m_sChildTag;

    [Tooltip("Set to cause the attaching to occur as soon as this object is spawned.")]
    [Rename("Attach On Start")]
    public bool m_bAttachOnStart;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("receiver Game Object.")]
    [Rename("Attach Event receivers for when objects are parented to one another.")]
    public LPK_EventReceivers AttachEventReceivers;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object parenting.
    * 
    * 
    **/
    override protected void OnStart ()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_pParentObject == null && string.IsNullOrEmpty(m_ParentTag))
            m_pParentObject = gameObject;

        if (m_pChildObject == null && string.IsNullOrEmpty(m_sChildTag))
        {
            if (m_pParentObject == gameObject)
            {
                LPK_PrintError(this, "Attempted to set child and parent to the same object!  Game is now closing.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
                m_pChildObject = gameObject;
        }

        if (m_pParentObject == null && !string.IsNullOrEmpty(m_ParentTag))
            m_pParentObject = GameObject.FindGameObjectWithTag(m_ParentTag);

        if (m_pChildObject == null && !string.IsNullOrEmpty(m_sChildTag))
            m_pChildObject = GameObject.FindGameObjectWithTag(m_sChildTag);

        if (m_bAttachOnStart)
            Attach();
    }

    /**
    * \fn OnEvent
    * \brief Event validation.
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

        Attach();
    }

    /**
    * \fn Attach
    * \brief Attach child to parent.  Seperated from OnEvent for Start functionality.
    * \param data - Event data to parse for validation.
    * 
    **/
    void Attach()
    {
        if (m_pChildObject == null)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "No Child Object set for parenting.");

            return;
        }

        if (m_pParentObject == null)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "No Parent Object set for parenting.");

            return;
        }

        //Attach object if it isn't already attached to that object
        if (m_pChildObject.transform.parent != m_pParentObject.transform)
        {
            m_pChildObject.transform.SetParent(m_pParentObject.transform);

            //Send out event.
            LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, AttachEventReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Attached };

            LPK_EventManager.InvokeEvent(sendEvent, sendData);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Object Attached");
        }
    }
}
