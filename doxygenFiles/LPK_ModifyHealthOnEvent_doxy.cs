/***************************************************
\file           LPK_ModifyHealthOnEvent.cs
\author        Christopher Onorati
\date   12/4/2018
\version   2.17

\brief
  This component can be added to any object to cause it to 
  dispatch an event to modify a target's health upon receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ModifyHealthOnEvent
* \brief Class which sends information out regarding modifying values.
**/
public class LPK_ModifyHealthOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_HealthModifyMode
    {
        ADD,
        SET,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether to add or set health to specified value.")]
    [Rename("Mode")]
    public LPK_HealthModifyMode m_eMode = LPK_HealthModifyMode.ADD;

    [Tooltip("Value to add or set.")]
    [Rename("Value")]
    public int m_iValue = 0;

    [Tooltip("Number of seconds to wait until an event can trigger another instance of health change.")]
    [Rename("Cooldown")]
    public float m_flCooldown = 0.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for health change events.")]
    public LPK_EventReceivers m_HealthModificationReceiver;

    /************************************************************************************/
    //Whether this component is waiting its cooldown
    bool m_bOnCooldown = false;

    /**
    * \fn OnStart
    * \brief Initializes event detection for health modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Manages health modification event sending.
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

        //Spawn an object if not recharging and the max count hasnt been reached
        if (!m_bOnCooldown)
            ChangeHealth();
    }

    /**
    * \fn ChangeHealth
    * \brief Changes the health of the target object.
    * 
    * 
    **/
    void ChangeHealth()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Health Change Event Dispatched");

        //Set recharging
        m_bOnCooldown = true;

        //Create and dispatch a LPK_HealthModify event
        //Gather event data.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_HealthModificationReceiver);

        if (m_eMode == LPK_HealthModifyMode.ADD)
            data.m_bData.Add(false);
        else if(m_eMode == LPK_HealthModifyMode.SET)
            data.m_bData.Add(true);

        data.m_idata.Add(m_iValue);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_HealthModified };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        StartCoroutine(DelayTimer());
    }

    /**
    * \fn DelayTimer
    * \brief Forces delay between health modification event sends.
    * 
    * 
    **/
    IEnumerator DelayTimer()
    {
        yield return new WaitForSeconds(m_flCooldown);
        m_bOnCooldown = false;
    }
}
