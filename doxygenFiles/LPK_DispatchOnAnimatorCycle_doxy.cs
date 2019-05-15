/***************************************************
\file           LPK_DispatchOnAnimatorCycle.cs
\author        Christopher Onorati
\date   2/6/2019
\version   2.17

\brief
  This component detects when an animator has finished a
  cycle for event dispatching.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DispatchOnAnimatorCycle
* \brief Dispatcher for animator cycles.
**/
public class LPK_DispatchOnAnimatorCycle : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether this field should start active or not.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Animator to detect cycles finishing on.")]
    [Rename("Animator")]
    public Animator m_cAnimator;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for animator cycle finished.")]
    public LPK_EventReceivers m_AnimatorCycleFinishedReceivers;

    /************************************************************************************/

    //Used for nonlooping animations.
    bool m_bHasFired = false;

    /**
    * \fn OnStart
    * \brief Sets up which animator to listen for.
    * 
    * 
    **/
    override protected void OnStart()
    {
        if(m_cAnimator == null)
        {
            if (GetComponent<Animator>())
                m_cAnimator = GetComponent<Animator>();
        }

        if (m_bPrintDebug && m_cAnimator == null)
            LPK_PrintWarning(this, "No animator found to use for event dispatching.");

        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnUpdate
    * \brief Checks state of animator being listend to.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if (m_cAnimator == null)
            return;

        if (m_cAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            m_bHasFired = false;

        if (m_cAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !m_bHasFired)
        {
            DispatchAnimationCycleEvent();

            if (m_cAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.isLooping)
                m_cAnimator.PlayInFixedTime(0, -1, m_cAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - 1.0f);
            else
                m_bHasFired = true;
        }
    }

    /**
    * \fn OnEvent
    * \brief Changes active state of the dispatcher.
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
            LPK_PrintDebug(this, "Event received.");
    }

    /**
    * \fn DispatchAnimationCycleEvent
    * \brief Sends event out for a cycle of the animation being finished.
    * 
    * 
    **/
    void DispatchAnimationCycleEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Animation cycle event dispatched.");

        LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_AnimatorCycleFinishedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_AnimationCycleFinished };

        LPK_EventManager.InvokeEvent(sendEvent, newData);
    }
}
