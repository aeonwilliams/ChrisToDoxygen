/***************************************************
\file           LPK_Counter.cs
\author        Christopher Onorati
\date   3/1/2019
\version   2018.3.4

\brief
  This component can be added to any object to have
  it track a value relevant to the game. That value
  might be a collectible, currency, score, etc.
  This component can be connected to a display to
  show the value and forward events when updating
  or upon reaching a specified threshold.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_Counter
* \brief Class which acts as a counter.
**/
public class LPK_Counter : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_CounterThresholdMode
    {
        EQUAL_TO,
        NOT_EQUAL_TO,
        LESS_THAN,
        LESS_EQUAL,
        GREATER_THAN,
        GREATER_EQUAL,
        NONE,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("What value to start the counter at.")]
    [Rename("Value")]
    public int m_iValue = 0;

    [Tooltip("Maximum value permitted for the counter.")]
    [Rename("Max Value")]
    public int m_iMaxValue = 10;

    [Tooltip("Minimum value permitted for the counter.")]
    [Rename("Min Value")]
    public int m_iMinValue = -10;

    [Tooltip("What relation between the counter value and threshold value should trigger an event.")]
    [Rename("Threshold Mode")]
    public LPK_CounterThresholdMode m_eThresholdMode = LPK_CounterThresholdMode.NONE;

    [Tooltip("What value should be used as a threshold for sending an event.")]
    [Rename("Threshold Value")]
    public int m_iThresholdValue = 0;

    [Header("Event Sending Info")]

    [Tooltip("Send the Threshold event only once when the conditions are met.")]
    [Rename("Send Threshold Event Once")]
    public bool m_bOnlyOnce = false;

    [Tooltip("Receivers for when the value is past a certain threshold.")]
    public LPK_EventReceivers m_ThresholdEventReceiver;

    [Tooltip("Receivers for displaying the timer.")]
    public LPK_EventReceivers m_DisplayUpdateReceiver;

    [Tooltip("Receivers for displaying the timer.")]
    public LPK_EventReceivers m_CounterModifiedReceivers;

    [Tooltip("Receivers for when the value is increased.")]
    public LPK_EventReceivers m_CounterIncreasedReceivers;

    [Tooltip("Receivers for when the value is decreased.")]
    public LPK_EventReceivers m_CounterDecreasedReceivers;

    /************************************************************************************/

    //Check for if the counter has already dispatched a threshold event.
    bool m_bHasDispatched = false;

    /**
    * \fn OnStart
    * \brief Sets up initial value and event connections.
    * 
    * 
    **/
    override protected void OnStart()
    {
        LPK_EventManager.OnLPK_CounterModify += OnEvent;
    }

    /**
    * \fn Awake
    * \brief Update the display object(s) once everything is initialized.
    * 
    * 
    **/
    void Awake()
    {
        UpdateDisplay();
    }

    /**
    * \fn OnEvent
    * \brief Updates the counter value.
    * \param data - modify value.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;
        
        //Incorrect data sent.
        if(data.m_bData.Count < 1 || data.m_idata.Count < 1)
            return;

        int valToSet = 0;

        //Set the value to event value.
        if (data.m_bData[0])
            valToSet = data.m_idata[0];
    
        //Add the two values.
        else if(!data.m_bData[0])
          valToSet = m_iValue + data.m_idata[0];

        //Clamp the incoming counter value
        int finalVal = Mathf.Clamp(valToSet, m_iMinValue, m_iMaxValue);

        if (finalVal > m_iValue)
            DispatchIncreaseEvent();
        else if (finalVal < m_iValue)
            DispatchDecreaseEvent();

        if (finalVal != m_iValue)
            DispatchModifyEvent();
        
        m_iValue = finalVal;
        
        UpdateDisplay();

        //Dispatch threshold event
        if (m_eThresholdMode == LPK_CounterThresholdMode.EQUAL_TO && m_iValue == m_iThresholdValue)
            DispatchThresholdEvent();
        else if (m_eThresholdMode == LPK_CounterThresholdMode.NOT_EQUAL_TO && m_iValue != m_iThresholdValue)
            DispatchThresholdEvent();
        else if (m_eThresholdMode == LPK_CounterThresholdMode.GREATER_THAN && m_iValue > m_iThresholdValue)
            DispatchThresholdEvent();
        else if (m_eThresholdMode == LPK_CounterThresholdMode.LESS_THAN && m_iValue < m_iThresholdValue)
            DispatchThresholdEvent();
        else if (m_eThresholdMode == LPK_CounterThresholdMode.GREATER_EQUAL && m_iValue >= m_iThresholdValue)
            DispatchThresholdEvent();
        else if (m_eThresholdMode == LPK_CounterThresholdMode.LESS_EQUAL && m_iValue <= m_iThresholdValue)
            DispatchThresholdEvent();
        
        if(m_bPrintDebug)
            LPK_PrintDebug(this, "New Counter Value: " + m_iValue);
    }

    /**
    * \fn UpdateDisplay
    * \brief Sends display update change event.
    * 
    * 
    **/
    void UpdateDisplay()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_DisplayUpdateReceiver);
        data.m_flData.Add(m_iValue);
        data.m_flData.Add(m_iMaxValue);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn DispatchThresholdEvent
    * \brief Sends threshold event.
    * 
    * 
    **/
    void DispatchThresholdEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Counter Treshold Event Dispatched");

        if (m_bHasDispatched && m_bOnlyOnce)
            return;

        m_bHasDispatched = true;

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_ThresholdEventReceiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterThreshold };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn DispatchModifyEvent
    * \brief Sends counter modfiied event.
    * 
    * 
    **/
    void DispatchModifyEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CounterModifiedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterModify };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn DispatchIncreaseEvent
    * \brief Sends counter increased event.
    * 
    * 
    **/
    void DispatchIncreaseEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CounterIncreasedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterIncrease };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn DispatchDecreaseEvent
    * \brief Sends counter decreased event.
    * 
    * 
    **/
    void DispatchDecreaseEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CounterDecreasedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterDecrease };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}
