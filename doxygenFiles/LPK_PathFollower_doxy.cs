/***************************************************
\file           LPK_PathFollower.cs
\author        Christopher Onorati
\date   2/21/19
\version   2018.3.4

\brief
  This component causes its gameobject to follow a pre-determined
  path by the designer.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_PathFollower
* \brief Sets an object to follow nodes along a path.
**/
[RequireComponent(typeof(Transform))]
public class LPK_PathFollower : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_PathFollowerLoopType
    {
        SINGLE,
        LOOP,
        LOOP_TELEPORT,
        LOOP_BACKTRACK,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Start the follower active on spawn.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("How to handle path looping once the end of the path is hit.")]
    [Rename("Loop Type")]
    public LPK_PathFollowerLoopType m_eLoopType;

    [Tooltip("Speed (units per second) to move along the path.")]
    [Rename("Speed")]
    public float m_flSpeed = 5.0f;

    [Tooltip("Nodes that make up the path.")]
    public GameObject[] m_Nodes;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for reaching a node.")]
    public LPK_EventReceivers m_ReachNodeReceivers;

    [Tooltip("Receiver Game Objects for reaching final node.")]
    public LPK_EventReceivers m_ReachFinalNodeReceivers;

    /************************************************************************************/

    //Keep track of which object to move towards.
    int m_iCounter = 0;

    //Flag to detect when the path follower has reached a node.
    bool m_bReachedNode = false;

    //Flag used for LOOP_BACKTRACK type.  
    bool m_bGoingBackwards = false;

    /**
    * \fn OnStart
    * \brief Sets up event listening.
    * 
    * 
    **/
    override protected void OnStart ()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Changes active state of the path follower.
    * \param data - Event info to parse.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Invalid event
        if (!ShouldRespondToEvent(data))
            return;

        if (m_eToggleType == LPK_ToggleType.ON)
            m_bActive = true;
        else if (m_eToggleType == LPK_ToggleType.OFF)
            m_bActive = false;
        else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            m_bActive = !m_bActive;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");
    }

    /**
    * \fn FixedUpdate
    * \brief Manages object movement and looping.
    * 
    * 
    **/
    void FixedUpdate()
    {
        if (!m_bActive)
            return;

        MoveAlongPath();
        DetectReachNode();
    }

    /**
    * \fn OnEvent
    * \brief Manages object movement towards the next node in the path.
    * 
    * 
    **/
    void MoveAlongPath()
    {
        if (m_bReachedNode)
            return;

        transform.position = Vector3.MoveTowards(transform.position, m_Nodes[m_iCounter].transform.position, Time.deltaTime * m_flSpeed);

        if (transform.position == m_Nodes[m_iCounter].transform.position)
        {
            m_bReachedNode = true;

            //Dispatch event.
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_ReachNodeReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_AIEventTrigger = new LPK_EventList.LPK_AI_EVENTS[] { LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachNode };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Reached Node");
        }
    }

    /**
    * \fn DetectReachNode
    * \brief Manage behavior once reaching a node.
    * 
    * 
    **/
    void DetectReachNode()
    {
        if (!m_bReachedNode)
            return;

        if (!m_bGoingBackwards)
            m_iCounter++;
        else
            m_iCounter--;

        //Final node reached event sending.
        if (m_iCounter > m_Nodes.Length)
        {
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_ReachFinalNodeReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_AIEventTrigger = new LPK_EventList.LPK_AI_EVENTS[] { LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachFinalNode };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Reached end of path.");
        }

        //Move towards the start of the path manually.
        if (m_iCounter >= m_Nodes.Length && m_eLoopType == LPK_PathFollowerLoopType.LOOP)
            m_iCounter = 0;

        //Teleport to the beggining of the path and resume.
        else if (m_iCounter >= m_Nodes.Length && m_eLoopType == LPK_PathFollowerLoopType.LOOP_TELEPORT)
        {
            m_iCounter = 1;
            transform.position = m_Nodes[0].transform.position;
        }

        //Begin going backwards down the path.
        else if ( (m_iCounter >= m_Nodes.Length && m_eLoopType == LPK_PathFollowerLoopType.LOOP_BACKTRACK)
                 || (m_iCounter < 0 && m_bGoingBackwards))
        {
            if (!m_bGoingBackwards)
                m_iCounter = m_Nodes.Length - 2;
            else
            {
                //NOTENOTE: Technically the start of the path is now also an end of track, so we call the event here as well.
                LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_ReachFinalNodeReceivers);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_AIEventTrigger = new LPK_EventList.LPK_AI_EVENTS[] { LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachFinalNode };

                LPK_EventManager.InvokeEvent(sendEvent, data);

                m_iCounter = 1;

            }

            m_bGoingBackwards = !m_bGoingBackwards;
        }

        //Reset and move again.
        if (m_iCounter < m_Nodes.Length)
            m_bReachedNode = false;
    }
}
