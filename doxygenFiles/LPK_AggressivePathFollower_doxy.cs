/***************************************************
\file           LPK_AggressivePathFollower.cs
\author        Christopher Onorati
\date   12/21/2018
\version   2.17

\brief
  This component causes its gameobject to follow a pre-determined
  path by the designer.  This object will also be aggressive to
  a set GameObject or tag.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_AggressivePathFollower
* \brief Sets an object to follow nodes along a path, while also being territorial.
*               Note this gameobject moves via transform so will not be stopped by physics
*               objects or walls.
**/
[RequireComponent(typeof(Transform))]
public class LPK_AggressivePathFollower : LPK_LogicBase
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

    [System.Serializable]
    public class AggressionProperties
    {
        [Tooltip("Enemies to attack/run from if set.")]
        [Rename("Enemies")]
        public GameObject[] m_pEnemy;

        [Tooltip("Tag enemies will have.  Useful if this path follower should react to multiple different GameObjects.")]
        [TagDropdown]
        public string m_EnemyTag;

        [Tooltip("Distance at which the character becomes agressive.")]
        [Rename("Aggression Range")]
        public float m_flAggressionRange = 10.0f;

        [Tooltip("Speed (units per second) for the character while agressive.")]
        [Rename("Aggression Speed")]
        public float m_flAgressionSpeed = 5.0f;

        [Tooltip("Run from the enemy instead of running towards the enemy.")]
        [Rename("Is Coward")]
        public bool m_bIsCoward;

        [Tooltip("Once an enemy is found, do not forget about it.")]
        [Rename("Don't Forget")]
        public bool m_bDontForget;
    }

    public AggressionProperties m_AggressionProperties;

    [Tooltip("Speed (units per second) to move along the path.")]
    [Rename("Speed")]
    public float m_flSpeed = 5.0f;

    [Tooltip("Nodes that make up the path.")]
    public GameObject[] m_Nodes;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for findng an enemy.")]
    public LPK_EventReceivers m_NoticeEnemyReceivers;

    [Tooltip("Receiver Game Objects for loosing an enemy.")]
    public LPK_EventReceivers m_LostEnemyReceivers;

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

    //Enemy to move towards.
    GameObject m_pCurEnemy;

    /**
    * \fn OnStart
    * \brief Sets up event listening.
    * 
    * 
    **/
    override protected void OnStart()
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

        if ((m_pCurEnemy != null && m_AggressionProperties.m_bDontForget) || NoticeEnemy())
            HuntEnemy();
        else
        {
            if (m_pCurEnemy != null)
                DispatchLostEvent();

            m_pCurEnemy = null;
            MoveAlongPath();
            DetectReachNode();
        }
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

        if (m_Nodes.Length <= 0)
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
    * \fn NoticeEnemy
    * \brief Detect if an enemy has been noticed based on user specified detection paramaters.
    * 
    * \return bool - Detection state.
    **/
    bool NoticeEnemy()
    {
        //Hunt specific enemy.
        for (int i = 0; i < m_AggressionProperties.m_pEnemy.Length; i++)
        {
            if (m_AggressionProperties.m_pEnemy[i] != null
                && Vector3.Distance(gameObject.transform.position, m_AggressionProperties.m_pEnemy[i].transform.position) <= m_AggressionProperties.m_flAggressionRange)
            {
                //Already set, just stop.
                if (m_pCurEnemy == m_AggressionProperties.m_pEnemy[i])
                    return true;

                m_pCurEnemy = m_AggressionProperties.m_pEnemy[i];
                DispatchNoticeEvent();
                return true;
            }
        }

        //Hunt enemy tag.
        if(!string.IsNullOrEmpty(m_AggressionProperties.m_EnemyTag))
        {
            for(int i = 0; i < GameObject.FindGameObjectsWithTag(m_AggressionProperties.m_EnemyTag).Length; i++)
            {
                GameObject obj = GameObject.FindGameObjectsWithTag(m_AggressionProperties.m_EnemyTag)[i];

                if (Vector3.Distance(gameObject.transform.position, obj.transform.position) <= m_AggressionProperties.m_flAggressionRange)
                {
                    //Already set, just stop
                    if (m_pCurEnemy == obj)
                        return true;

                    m_pCurEnemy = obj;
                    DispatchNoticeEvent();
                    return true;
                }
            }
        }

        return false;
    }

    /**
    * \fn HuntEnemy
    * \brief Manages the character hunting its target.
    * 
    * 
    **/
    void HuntEnemy()
    {
        if(!m_AggressionProperties.m_bIsCoward)
            transform.position = Vector3.MoveTowards(transform.position, m_pCurEnemy.transform.position, Time.deltaTime * m_AggressionProperties.m_flAgressionSpeed);
        else
            transform.position = Vector3.MoveTowards(transform.position, m_pCurEnemy.transform.position, Time.deltaTime * -m_AggressionProperties.m_flAgressionSpeed);
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
        else if ((m_iCounter >= m_Nodes.Length && m_eLoopType == LPK_PathFollowerLoopType.LOOP_BACKTRACK)
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

    /**
    * \fn DispatchNoticeEvent
    * \brief Send out a notice event.
    * 
    * 
    **/
    void DispatchNoticeEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Enemy found");

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_NoticeEnemyReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_AIEventTrigger = new LPK_EventList.LPK_AI_EVENTS[] { LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerFindEnemy };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn DispatchLostEvent
    * \brief Send out a lost event.
    * 
    * 
    **/
    void DispatchLostEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Enemy lost");

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_LostEnemyReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_AIEventTrigger = new LPK_EventList.LPK_AI_EVENTS[] { LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerLostEnemy };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}
