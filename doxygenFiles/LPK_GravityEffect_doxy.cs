/***************************************************
\file           LPK_GravityEffect.cs
\author        Christopher Onorati
\date   1/9/2019
\version   2.17

\brief
  This component can be added to an object to modify how gravity
  affects the object.  Note that for 2D objects, it is possible
  to just use the RigidBody2D's gravity scale property to
  achieve the same effect.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_GravityEffect
* \brief Modify the gravity scale on an object.  Useful for 3D objects.
**/
public class LPK_GravityEffect : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether the velocity effect should start active or not.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Scalar to modify gravity on the object by.")]
    [Rename("Gravity Scale")]
    public Vector2 m_vecGravityScale = new Vector2(1, 1);

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    Rigidbody m_cRigidbody;
    Rigidbody2D m_cRigidbody2D;

    /**
    * \fn OnStart
    * \brief Sets up components.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cRigidbody = GetComponent<Rigidbody>();
        m_cRigidbody2D = GetComponent<Rigidbody2D>();

        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_cRigidbody == null && m_cRigidbody2D == null && m_bPrintDebug)
            LPK_PrintDebug(this, "No Rigibody component found for gameobject " + gameObject.name + ".");
    }

    /**
    * \fn OnEvent
    * \brief Changes active state of the gravity effect.
    * \param data - Event info to parse.
    * 
    **/
    protected override void OnEvent(LPK_EventManager.LPK_EventData data)
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
            LPK_PrintDebug(this, "Event received.");
    }

    /**
    * \fn OnUpdate
    * \brief Apply gravity effect if active.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if(m_cRigidbody)
        {
           // Vector2 gravity = Physics.gravity * m_vecGravityScale;
          //  m_cRigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        else if (m_cRigidbody2D)
        {
           // Vector2 gravity = Physics.gravity * m_vecGravityScale;
           // m_cRigidbody2D.AddForce(gravity, ForceMode2D.Force);
        }
    }
}
