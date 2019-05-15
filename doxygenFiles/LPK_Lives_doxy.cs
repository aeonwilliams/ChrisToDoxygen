/***************************************************
\file           LPK_Lives.cs
\author        Christopher Onorati
\date   2/22/2019
\version   2018.3.4

\brief
  This component can be added to any object to define its 
  amount of lives and dispatches a LPK_OutOfLives event 
  upon running out of them.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_Lives
* \brief Class which manages the lives of a character.
**/
public class LPK_Lives : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Current number of lives.")]
    [Rename("Lives")]
    public int m_iLives = 3;

    [Tooltip("Target object to display the lives value.")]
    public GameObject m_TargetDisplayObject;

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for displaying the timer.")]
    public LPK_EventReceivers m_DisplayUpdateReceivers;

    [Tooltip("Receiver Game Objects for character out of lives.")]
    public LPK_EventReceivers m_OutOfLivesReceivers;

    /************************************************************************************/

    int m_iStatingLives = 0;

    /**
    * \fn OnStart
    * \brief Initializes Death Event detection.
    * 
    * 
    **/
    override protected void OnStart()
    {
        LPK_EventList deathList = new LPK_EventList();
        deathList.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Death };

        InitializeEvent(deathList, OnEvent);

        m_iStatingLives = m_iLives;
        UpdateDisplay();
    }

    /**
    * \fn OnEvent
    * \brief Sets detecing death events.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        --m_iLives;

        UpdateDisplay();

        if (m_bPrintDebug)
        {
            LPK_PrintDebug(this, "Character Died");
            LPK_PrintDebug(this, "Current Lives: " + m_iLives);
        }

        //Dispatch out of lives
        if (m_iLives <= 0)
        {
            LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_OutOfLivesReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_OutOfLives };

            LPK_EventManager.InvokeEvent(sendEvent, newData);
        }
    }

    /**
    * \fn UpdateDisplay
    * \brief Invokes the UpdateDisplay events for objects that may be subscribed.
    * 
    * 
    **/
    void UpdateDisplay()
    {
        //Gather event data.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_DisplayUpdateReceivers);
        data.m_flData.Add(m_iLives);
        data.m_flData.Add(m_iStatingLives);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}
