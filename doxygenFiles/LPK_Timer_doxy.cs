/***************************************************
\file           LPK_Timer.cs
\author        Christopher Onorati
\date   11/28/2018
\version   2.17

\brief
  This component can be added to an object to keep track
  of time starting when the object gets created. The timer 
  will tick up and send out an event when reaching the end.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_Timer
* \brief Class which manages basic timer functionality.
**/
public class LPK_Timer : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_TimerPolicy
    {
        RESET,
        STOP,
    };

    public enum LPK_CountType
    {
        COUNTUP,
        COUNTDOWN,
    }

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Active state of the timer.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How to behave when timer reaches MaxTime")]
    [Rename("Timer Policy")]
    public LPK_TimerPolicy m_eTimerPolicy = LPK_TimerPolicy.STOP;

    [Tooltip("Set to determine if the timer should count up or down.")]
    [Rename("Timer Type")]
    public LPK_CountType m_eCountType = LPK_CountType.COUNTUP;

    [Tooltip("Maximum time (in seconds) should the timer track. Will send a LPK_TimerCompleted event when done.")]
    [Rename("Duration")]
    public float m_flEndTime = 2.0f;

    [Tooltip("Allows a randomized variance to be applied to the goal time for each cycle of the timer.")]
    [Rename("Variance")]
    public float m_flVariance = 0.0f;

    [Tooltip("How long to wait until the timer begins (in seconds)")]
    [Rename("Start Delay")]
    public float m_flStartDelay = 0.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for displaying the timer.")]
    public LPK_EventReceivers m_DisplayUpdateReceiver;

    [Tooltip("Receiver Game Objects for timer completed.")]
    public LPK_EventReceivers m_TimerCompletedReceiver;

    /************************************************************************************/

    //Internal timer
    float m_flCurrentTime = 0.0f;
  
    //Whether the timer is currently paused
    bool m_bPaused = true;

    //Internal goal timer.
    float m_flCurrentGoalTime = 0.0f;

    /**
    * \fn OnStart
    * \brief Applies initial delay to timer if appropriate.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        SetTime();

        if (!m_bActive)
            return;

        StartCoroutine(DelayTimer());
	}

    /**
    * \fn OnEvent
    * \brief Become active once receiving an event.
    * \param data - Event info to parse.
    * 
    **/
    protected override void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Wrong event received.
        if (!ShouldRespondToEvent(data))
            return;

        StartCoroutine(DelayTimer());

        if (m_eCountType == LPK_CountType.COUNTDOWN)
            SetTime();
    }

    /**
    * \fn OnUpdate
    * \brief Manages behavior of the timer.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (m_bPaused)
            return;

        if (m_eCountType == LPK_CountType.COUNTUP)
            m_flCurrentTime += Time.deltaTime;
        else if (m_eCountType == LPK_CountType.COUNTDOWN)
            m_flCurrentTime -= Time.deltaTime;

        UpdateDisplay();

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Timer: " + m_flCurrentTime);


        if (m_eCountType == LPK_CountType.COUNTUP && m_flCurrentTime >= m_flCurrentGoalTime)
            CountUp();
        else if (m_eCountType == LPK_CountType.COUNTDOWN && m_flCurrentTime <= 0)
            CountDown();
    }

    /**
    * \fn CountUp
    * \brief Detects when timer has finished counting up.
    * 
    * 
    **/
    void CountUp()
    {
        //Send out event.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TimerCompletedReceiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TimerCompleted };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Timer Completed");

        if (m_eTimerPolicy == LPK_TimerPolicy.RESET)
        {
            m_flCurrentTime = 0.0f;
            SetTime();
        }
        else
            m_bPaused = true;
    }

    /**
    * \fn CountDown
    * \brief Detects when timer has finished counting down.
    * 
    * 
    **/
    void CountDown()
    {
        //Send out event.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TimerCompletedReceiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TimerCompleted };

        LPK_EventManager.InvokeEvent(sendEvent, data);
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Timer Completed");

        if (m_eTimerPolicy == LPK_TimerPolicy.RESET)
            SetTime();
        else
            m_bPaused = true;
    }

    /**
    * \fn UpdateDisplay
    * \brief Invokes the UpdateDisplay events for objects that may be subscribed.
    * 
    * 
    **/
    void UpdateDisplay()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_DisplayUpdateReceiver);

        data.m_flData.Add(m_flCurrentTime);
        data.m_flData.Add(m_flEndTime);


        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn SetTime
    * \brief Sets the timer to a random goal within a specified range.
    * 
    * 
    **/
    void SetTime()
    {
        if (m_eCountType == LPK_CountType.COUNTUP)
            m_flCurrentGoalTime = m_flEndTime + Random.Range(-m_flVariance, m_flVariance);
        else if (m_eCountType == LPK_CountType.COUNTDOWN)
            m_flCurrentTime = m_flEndTime + Random.Range(-m_flVariance, m_flVariance);
    }

    /**
    * \fn DelayTimer
    * \brief Forces initial delay before timer activates.
    * 
    * 
    **/
    IEnumerator DelayTimer ()
    {
        yield return new WaitForSeconds(m_flStartDelay);
        m_bPaused = false;
	}
}
