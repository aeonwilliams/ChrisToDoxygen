/***************************************************
\file           LPK_DispatchOnButtonInput.cs
\author        Christopher Onorati
\date   12/8/18
\version   2.17

\brief
  This component can be added to any object to cause it to 
  dispatch a LPK_ButtonInput event on a specified 
  target upon a given unity button being pressed,
  released or held.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DispatchOnButtonInput
* \brief Component to manage user input responses via virtual buttons.
**/
public class LPK_DispatchOnButtonInput : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_InputMode
    {
        PRESSED,
        RELEASED,
        HELD,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Start the follower active on spawn.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("What virtual key will trigger the event dispatch.")]
    [Rename("Trigger Button")]
    public string m_sButton;

    [Tooltip("What mode should cause the event dispatch.")]
    [Rename("Input Mode")]
    public LPK_InputMode m_eInputMode = LPK_InputMode.PRESSED;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for when a virtual button is pressed.")]
    public LPK_EventReceivers m_VirtualButtonReceivers;

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
    * \fn OnUpdate
    * \brief Handles input checking
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if (m_eInputMode == LPK_InputMode.PRESSED && Input.GetButtonDown(m_sButton))
            DispatchButtonEvent();
        else if (m_eInputMode == LPK_InputMode.RELEASED && Input.GetButtonUp(m_sButton))
            DispatchButtonEvent();
        else if (m_eInputMode == LPK_InputMode.HELD && Input.GetButton(m_sButton))
            DispatchButtonEvent();
    }

    /**
    * \fn DispatchButtonEvent
    * \brief Send out event for virtual button input.
    * 
    * 
    **/
    void DispatchButtonEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_VirtualButtonReceivers);
        data.m_sPressedButton = m_sButton;

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_ButtonInput };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Button event dispatched");
    }
}
