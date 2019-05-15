/***************************************************
\file           LPK_BarDisplay.cs
\author        Christopher Onorati
\date   12/9/2018
\version   2.17

\brief
  This component controls the appearance of a display bar 
  such as a health or cooldown bar.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   //Access to Slider

/**
* \class LPK_BarDisplay
* \brief Create a display using a slider in the UI canvas.
**/
[RequireComponent(typeof(Slider))]
public class LPK_BarDisplay : LPK_LogicBase
{
    /**
    * \fn OnStart
    * \brief Initializes events.
    * 
    * 
    **/
    override protected void OnStart()
    {
        LPK_EventManager.OnLPK_DisplayUpdate += OnEvent;
    }

    /**
    * \fn OnEvent
    * \brief Changes the visible display of the bar.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Display Update");

        GetComponent<Slider>().value = data.m_flData[0] / data.m_flData[1];
    }
}
