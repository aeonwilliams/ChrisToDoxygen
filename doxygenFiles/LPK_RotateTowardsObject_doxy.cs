/***************************************************
\file           LPK_RotateTowardsObject.cs
\author        Christopher Onorati
\date   2/19/2019
\version   2018.3.4

\brief
  This component can be added to any object to cause it to 
  rotate itself to face the position of a specified object 
  either every frame or just once.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_RotateTowardsObject
* \brief Component used to cause an object to always face another object.
**/
public class LPK_RotateTowardsObject : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Set to start rotating on spawn.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Whether to reorient towards mouse every time its position updates. Will only reorient once if set to false.")]
    [Rename("On Update")]
    public bool m_bOnUpdate = true;

    [Tooltip("Initial object to rotate towards.  If deleted or set to null, this script will try to find a tagged object to face.")]
    [Rename("Initial Transform Object")]
    public GameObject m_pTargetTransformObject;

    [Tooltip("Tag to search for to find an object to face.")]
    [Rename("Target Tag to Face")]
    public string m_sTargetFacingTag;

    [Tooltip("How fast to rotate the object.")]
    [Rename("Rotation Speed")]
    public float m_flRotationSpeed = 360.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    //Internal variable to handle single reorientation
    bool m_bReoriented = false;

    /**
    * \fn OnStart
    * \brief Stops rotating from starting by default if set.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (string.IsNullOrEmpty(m_sTargetFacingTag))
        {
            if (m_bPrintDebug)
                LPK_PrintError(this, "No string set as a follow object!");
        }
    }

    /**
    * \fn OnEvent
    * \brief Sets the rotating as active.
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
    * \brief Manages rotation of objects.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if (m_pTargetTransformObject == null)
            FindFacingObject();

        if (m_bOnUpdate || !m_bReoriented)
        {
            if (m_pTargetTransformObject != null && m_pTargetTransformObject.transform != null)
            {
                //Look at desired object.

                Vector3 diff = m_pTargetTransformObject.transform.position - transform.position;
                diff.Normalize();

                float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                Quaternion goalRotation = Quaternion.Euler(0f, 0f, rot_z - 90);
                transform.rotation = Quaternion.Lerp(transform.rotation, goalRotation, Time.deltaTime * m_flRotationSpeed);

                m_bReoriented = true;

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Rotation Applied");
            }
            else
            {
                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Invalid Transform Path");
            }
        }
    }

    /**
    * \fn FindFacingObject
    * \brief Sets the ideal object to face.  Will always be the first object with the tag found.  As such
    *                the tag used to find while following should only ever exist once in a scene.
    * 
    * 
    **/
    void FindFacingObject()
    {
        if (string.IsNullOrEmpty(m_sTargetFacingTag))
            return;

        m_pTargetTransformObject = GameObject.FindGameObjectWithTag(m_sTargetFacingTag);
    }
}
