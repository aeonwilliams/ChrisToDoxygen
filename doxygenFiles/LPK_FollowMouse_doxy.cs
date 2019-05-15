/***************************************************
\file           LPK_FollowMouse.cs
\author        Christopher Onorati
\date   2/7/2019
\version   2018.3.4

\brief
  This component can be added to any object to cause it to 
  follow the mouse position. This can be used to create a 
  custom mouse cursor.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_FollowMouse
* \brief Component to force an object to follow the mouse.
**/
[RequireComponent(typeof(Transform))]
public class LPK_FollowMouse : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Set to start the object following the mouse on spawn.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Whether to move object on mouse update. Will only move once if set to false.")]
    [Rename("Change On Update")]
    public bool m_bOnUpdate = true;

    [Tooltip("At what z depth should the object be relocated to.")]
    [Rename("Z Depth")]
    public float m_flZDepth = 0.0f;

    [Tooltip("Offset from final mouse position in world to be applied to the object.")]
    [Rename("Offset")]
    public Vector3 m_vecOffset;

    [Tooltip("What percentage of the distance between my current position and the target's should I move every frame.")]
    [Range(0, 1)]
    public float m_InterpolationFactor = 0.1f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    //Internal variable to handle single move
    public bool m_bMoved = false;

    /**
    * \fn OnStart
    * \brief Sets up event listening if necessary.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Sets following active.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_eToggleType == LPK_ToggleType.ON)
            m_bActive = true;
        else if (m_eToggleType == LPK_ToggleType.OFF)
            m_bActive = false;
        else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            m_bActive = !m_bActive;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event received.");
    }

    /**
    * \fn OnUpdate
    * \brief Manages movement of object to follow the mouse.
    * 
    * 
    **/
    void FixedUpdate()
    {
        //Move to match the mouse position
        if ((m_bOnUpdate || !m_bMoved) && m_bActive )
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = m_flZDepth;
            mousePos += m_vecOffset;

            transform.position = Vector3.Lerp(transform.position, mousePos, m_InterpolationFactor);

            m_bMoved = true;
        }
    }
}
