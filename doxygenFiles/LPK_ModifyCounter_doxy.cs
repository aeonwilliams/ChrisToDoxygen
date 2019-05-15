/***************************************************
\file           LPK_ModifyCounterOnEvent.cs
\author        Christopher Onorati
\date   12/10/2018
\version   2.17

\brief
  This component can be added to any object to cause it to 
  dispatch an event to modify a target counter upon receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ModifyCounter
* \brief Component used to modify the values of counters.
**/
public class LPK_ModifyCounter : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_CounterModifyMode
    {
        ADD,
        SET,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether to add or set health to specified value.")]
    [Rename("Mode")]
    public LPK_CounterModifyMode m_eMode = LPK_CounterModifyMode.ADD;

    [Tooltip("Value to add or set.")]
    [Rename("Value")]
    public int m_iValue = 0;

    [Tooltip("Number of seconds to wait until an event can trigger another instance of health change.")]
    [Rename("Cooldown")]
    public float m_flCooldown = 0.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver counters to be modified.")]
    public LPK_EventReceivers m_CounterReceiver;

    /************************************************************************************/

    //Whether this component is waiting its cooldown
    bool m_bOnCooldown = false;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for counter modifying.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Changes counter values on event receiving.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Spawn an object if not recharging and the max count hasnt been reached
        if (!m_bOnCooldown)
            ChangeCounter();
        else
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Counter modifier on cooldown.");
        }
    }

    /**
    * \fn ChangeCounter
    * \brief Changes counter values on event receiving.
    * 
    * 
    **/
    void ChangeCounter()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "LPK_CounterModify Event Dispatched");

        //Set recharging
        m_bOnCooldown = true;

        //Event dispatch.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CounterReceiver);

        if (m_eMode == LPK_CounterModifyMode.SET)
            data.m_bData.Add(true);
        else
            data.m_bData.Add(false);

        data.m_idata.Add(m_iValue);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterModify };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        StartCoroutine(DelayTimer());
    }

    /**
    * \fn DelayTimer
    * \brief Forces delay between counter modification event sends.
    * 
    * 
    **/
    IEnumerator DelayTimer()
    {
        yield return new WaitForSeconds(m_flCooldown);
        m_bOnCooldown = false;
    }
}
