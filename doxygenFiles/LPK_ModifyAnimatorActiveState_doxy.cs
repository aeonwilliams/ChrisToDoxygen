/***************************************************
\file           LPK_ModifyAnimatorActiveState.cs
\author        Christopher Onorati
\date   2/7/2019
\version   2018.3.4

\brief
  This component can be used to enable/disable the active
  state of animators.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ModifyAnimatorActiveState
* \brief Component used to enable/disable animators.
**/
public class LPK_ModifyAnimatorActiveState : LPK_LogicBase
{
    /************************************************************************************/
    [Header("Component Properties")]

    [Tooltip("How to change the active state of the declared script(s).")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Animator(s) to change the enabled state of.  Default to an animator on this script's owner, if one is found, should the array be empty.")]
    public Animator[] m_ModifyAnimators;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn Start
    * \brief Sets up what event to listen to for sprite and color modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        //Attempt to default to this gameobject's animator.
        if(m_ModifyAnimators.Length == 0)
        {
            if (GetComponent<Animator>() != null)
                m_ModifyAnimators = new Animator[] { GetComponent<Animator>() };
        }

        if (m_bPrintDebug && m_ModifyAnimators.Length == 0)
            LPK_PrintWarning(this, "No animators were found to modify the active state of.");
    }

    /**
    * \fn OnEvent
    * \brief Sets the actibe state of the animator(s).
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        for (int i = 0; i < m_ModifyAnimators.Length; i++)
        {
            if (m_ModifyAnimators[i] == null)
                continue;

            if (m_eToggleType == LPK_ToggleType.ON)
                m_ModifyAnimators[i].enabled = true;
            else if (m_eToggleType == LPK_ToggleType.OFF)
                m_ModifyAnimators[i].enabled = false;
            else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            {
                if (!m_ModifyAnimators[i].enabled)
                    m_ModifyAnimators[i].enabled = true;
                else if (m_ModifyAnimators[i].enabled)
                    m_ModifyAnimators[i].enabled = false;
            }

            //Debug info.
            if (m_bPrintDebug && m_ModifyAnimators[i].enabled)
                LPK_PrintDebug(this, "Changing active state of " + m_ModifyAnimators[i] + " to ON.");
            else if (m_bPrintDebug && !m_ModifyAnimators[i].enabled)
                LPK_PrintDebug(this, "Changing active state of " + m_ModifyAnimators[i] + " to OFF.");
        }
    }
}
