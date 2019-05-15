/***************************************************
\file           LPK_Health.cs
\author        Christopher Onorati
\date   12/4/2018
\version   2.17

\brief
  This component can be added to any object to give it a 
  health mechanic. Entities with health can be "healed" or 
  "damaged" and will "die" if their health reaches 0.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_Health
* \brief Class which manages health of a character.
**/
public class LPK_Health : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Maximum health the character is allowed to have.")]
    [Rename("Max Health")]
    public int m_iMaxHealth = 10;

    [Tooltip("What health value should the character start at.  -1 is infinite health.")]
    [Rename("Starting Health")]
    public int m_iHealth = 10;

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for displaying the timer.")]
    public LPK_EventReceivers m_DisplayUpdateReceivers;

    [Tooltip("Receiver Game Objects for taking damage.")]
    public LPK_EventReceivers m_DamageReceivers;

    [Tooltip("Receiver Game Objects for healing.")]
    public LPK_EventReceivers m_HealingReceivers;

    [Tooltip("Receiver Game Objects for character death.")]
    public LPK_EventReceivers m_DeathReceivers;

    /************************************************************************************/

    /**
    * \fn OnStart
    * \brief Initializes Health Modification detection.
    * 
    * 
    **/
    override protected void OnStart()
    {
        LPK_EventList healthList = new LPK_EventList();
        healthList.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_HealthModified };

        InitializeEvent(healthList, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Sets detecing health modification events.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if(data.m_idata.Count < 1 || data.m_bData.Count < 1)
            return;

        //Infinite health.
        if (m_iHealth == -1)
            return;

        int valToSet = 0;

        //First bool false = set.
        if (data.m_bData[0])
            valToSet = data.m_idata[0];
    
        //First bool true = add.
        else if(!data.m_bData[0])
            valToSet = m_iHealth + data.m_idata[0];

        //Clamp the incoming health to max health
        int finalVal = Mathf.Clamp(valToSet, 0, m_iMaxHealth);
      
        //If the health is being set to a lower value, send a LPK_Damaged event
        if(m_iHealth > finalVal)
        {
            if (gameObject != null) //necessary check in case health is set at initialization
            {
                LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_DamageReceivers);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Damaged };

                LPK_EventManager.InvokeEvent(sendEvent, newData);
            }

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Character Damaged");
        }
      
        //If the health is being set to a higher value, send a LPK_Healed event
        else if(m_iHealth < finalVal)
        {
            if (gameObject != null)//necessary check in case health is set at initialization
            {
                LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_HealingReceivers);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Healed };

                LPK_EventManager.InvokeEvent(sendEvent, newData);
            }

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Character Healed");
        }
    
        //Set the new value
        m_iHealth = finalVal;
    
        //Update the display
        UpdateDisplay();
    
        if(m_bPrintDebug)
            LPK_PrintDebug(this, "Current Health:" + m_iHealth);
    
        //Dispatch LPK_Death when health reaches 0
        if(finalVal == 0)
        {
            LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_DeathReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Death };

            LPK_EventManager.InvokeEvent(sendEvent, newData);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Character Died");
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
        data.m_flData.Add(m_iHealth);
        data.m_flData.Add(m_iMaxHealth);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}
