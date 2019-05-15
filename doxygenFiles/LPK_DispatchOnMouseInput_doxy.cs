/***************************************************
\file           LPK_DispatchOnMouseInput.cs
\author        Christopher Onorati
\date   11/30/18
\version   2.17

\brief
  This component can be added to any object to cause it to 
  dispatch a LPK_MouseInput event on a specified target 
  upon a given button being pressed, released or held.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DispatchOnMouseInput
* \brief Component to manage user input responses via mice.
**/
public class LPK_DispatchOnMouseInput : LPK_LogicBase
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

    [Tooltip("Which mouse button will trigger sending the event.  Note that Any does not detect scrolwheel.")]
    [Rename("Mouse Button")]
    public LPK_MouseButtons m_eMouseButton = LPK_MouseButtons.LEFT;
  
    [Tooltip("What mode should cause the event dispatch.")]
    [Rename("Input Mode")]
    public LPK_InputMode m_eInputMode = LPK_InputMode.PRESSED;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for mouse events.")]
    public LPK_EventReceivers m_MouseEventReceivers;

    /************************************************************************************/

    //Int for mouse press detection.
    private int m_iMouseButton;

    /**
    * \fn OnStart
    * \brief Initializes m_iMouseButton;
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_eMouseButton == LPK_MouseButtons.LEFT)
            m_iMouseButton = 0;
        else if (m_eMouseButton == LPK_MouseButtons.RIGHT)
            m_iMouseButton = 1;
        else
            m_iMouseButton = 2;
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
    * \fn Update
    * \brief Handles input checking for mice.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        //Pressed.
        if (m_eInputMode == LPK_InputMode.PRESSED)
        {
            if (Input.GetMouseButtonDown(m_iMouseButton) || m_eMouseButton == LPK_MouseButtons.ANY)
                DispatchLPKMouseInputEvent();
        }
        //Released.
        else if (m_eInputMode == LPK_InputMode.RELEASED || m_eMouseButton == LPK_MouseButtons.ANY)
        {
            if (Input.GetMouseButtonUp(m_iMouseButton) || m_eMouseButton == LPK_MouseButtons.ANY)
                DispatchLPKMouseInputEvent();       
        }
        //Held.
        else if (m_eInputMode == LPK_InputMode.HELD || m_eMouseButton == LPK_MouseButtons.ANY)
        { 
            if (Input.GetMouseButton(m_iMouseButton))
                DispatchLPKMouseInputEvent();
        }

        //Mouse scroll
        else
        {
            float mouseScrollDelta = Input.mouseScrollDelta.y;

            if(mouseScrollDelta < 0 && m_eMouseButton == LPK_MouseButtons.MIDDLE_SCROLL_DOWN)
                DispatchLPKMouseInputEvent();

            else if (mouseScrollDelta > 0 && m_eMouseButton == LPK_MouseButtons.MIDDLE_SCROLL_UP)
                DispatchLPKMouseInputEvent();
        }
    }

    /**
    * \fn DispatchLPKMouseInputEvent
    * \brief Dispatches the mouse event and prints debug info if set.
    * 
    * 
    **/
    void DispatchLPKMouseInputEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_MouseEventReceivers);
        data.m_PressedMouseButton = m_eMouseButton;

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_MouseInput };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Mouse Input dispatched");
    }
}
