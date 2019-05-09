/***************************************************
unicorn
Authors:        Christopher Onorati
Last Updated:   12/9/2018
Last Version:   2.17

Description:
  This component controls the appearance of a display bar 
  such as a health or cooldown bar.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   //Access to Slider

/**
* CLASS NAME  : LPK_BarDisplay
* DESCRIPTION : Create a display using a slider in the UI canvas.
**/
[RequireComponent(typeof(Slider))]
public class LPK_BarDisplay : LPK_LogicBase
{
    /**
    * FUNCTION NAME: OnStart
    * DESCRIPTION  : Initializes events.
    * INPUTS       : None
    * OUTPUTS      : None
    **/
    override protected void OnStart()
    {
        LPK_EventManager.OnLPK_DisplayUpdate += OnEvent;
    }

    /**
    * FUNCTION NAME: OnEvent
    * DESCRIPTION  : Changes the visible display of the bar.
    * INPUTS       : data - Event data to parse for validation.
    * OUTPUTS      : None
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
