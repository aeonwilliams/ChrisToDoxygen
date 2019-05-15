/***************************************************
\file           LPK_DispatchOnKeyboardInput.cs
\author        Christopher Onorati
\date   11/30/18
\version   2.17

\brief
  This component can be added to any object to cause it to 
  dispatch a LPK_KeyboardInput event on a specified 
  target upon a given key being pressed, released or held.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DispatchOnKeyboardInput
* \brief Component to manage user input responses via keyboard.
**/
public class LPK_DispatchOnKeyboardInput : LPK_LogicBase
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

    [Tooltip("What key will trigger the event dispatch.")]
    [Rename("Trigger Key")]
    public KeyCode m_iKey;

    [Tooltip("Set any key to trigger event dispatch.  Overrides Trigger Key property.")]
    [Rename("Detect Any Key")]
    public bool m_bAnyKey;

    [Tooltip("What mode should cause the event dispatch.")]
    [Rename("Input Mode")]
    public LPK_InputMode m_eInputMode = LPK_InputMode.PRESSED;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for keyboard input detection.")]
    public LPK_EventReceivers KeyboardEventReceivers;

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

        //Dispatch event based on selected mode
        if (m_eInputMode == LPK_InputMode.PRESSED && (Input.GetKeyDown(m_iKey) || (m_bAnyKey && Input.anyKeyDown
            && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1) && !Input.GetKey(KeyCode.Mouse2) && !Input.GetKey(KeyCode.Mouse3) && !Input.GetKey(KeyCode.Mouse4) && !Input.GetKey(KeyCode.Mouse5) && !Input.GetKey(KeyCode.Mouse6))))
            DispatchKeyboardEvent();
        else if (m_eInputMode == LPK_InputMode.RELEASED && (Input.GetKeyUp(m_iKey) || (m_bAnyKey && Input.anyKeyDown
            && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1) && !Input.GetKey(KeyCode.Mouse2) && !Input.GetKey(KeyCode.Mouse3) && !Input.GetKey(KeyCode.Mouse4) && !Input.GetKey(KeyCode.Mouse5) && !Input.GetKey(KeyCode.Mouse6))))
            DispatchKeyboardEvent();
        else if (m_eInputMode == LPK_InputMode.HELD && (Input.GetKey(m_iKey) || (m_bAnyKey && Input.anyKeyDown
            && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1) && !Input.GetKey(KeyCode.Mouse2) && !Input.GetKey(KeyCode.Mouse3) && !Input.GetKey(KeyCode.Mouse4) && !Input.GetKey(KeyCode.Mouse5) && !Input.GetKey(KeyCode.Mouse6))))
            DispatchKeyboardEvent();
    }

    /**
    * \fn DispatchKeyboardEvent
    * \brief Sends event for keyboard input.
    * 
    * 
    **/
    void DispatchKeyboardEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, KeyboardEventReceivers);
        data.m_PressedKey = m_iKey;

        if (m_bAnyKey)
            data.m_PressedKey = KeyCode.None;

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_KeyboardInput };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Keyboard event dispatched");
    }
}
