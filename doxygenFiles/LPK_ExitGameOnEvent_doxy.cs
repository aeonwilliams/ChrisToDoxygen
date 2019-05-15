/***************************************************
\file           LPK_ExitGameOnEvent.cs
\author        Christoper Onorati
\date   12/1/2018
\version   2.17

\brief
  This component can be added to any object to cause it
  to exit the application upon receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ExitGameOnEvent
* \brief Closes the game on parsing user-specified event.
**/
public class LPK_ExitGameOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for game ending.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Exists out of the game.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        Quit();
    }

    /**
    * \fn Quit
    * \brief Calls quit command.  Seperate function so this is exposed to the unity
    *                UI system (buttons).
    * 
    * 
    **/
    public void Quit()
    {
        Application.Quit();
    }
}
