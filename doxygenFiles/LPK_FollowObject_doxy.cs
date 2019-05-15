/***************************************************
\file           LPK_FollowObject.cs
\author        Christopher Onorati
\date   2/19/2019
\version   2018.3.4

\brief
  This component can be added to any object to cause it to 
  follow another object's position.  Note this does so
  without any parenting.
  

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_FollowObject
* \brief Component to force an object to follow another object's movements (pseudo parenting).
**/
[RequireComponent(typeof(Transform))]
public class LPK_FollowObject : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_FollowType
    {
        ANCHOR_POINT,
        BEHIND,
        IN_FRONT,
        LEFT,
        RIGHT,
    }

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Set to start the object following on spawn.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("Toggle type of event receiving.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Initial object to folow.  If deleted or set to null, this script will try to find a tagged object to follow.")]
    [Rename("Initial Follow Object")]
    public GameObject m_pCurFollowObj;

    [Tooltip("Tag to search for to find an object to follow.")]
    [TagDropdown]
    public string m_TargetFollowTag;

    [Tooltip("How this object will behave when following its target.")]
    [Rename("Follow Type")]
    public LPK_FollowType m_eFollowType;

    [Tooltip("What offset to keep from the followed object.  Used for ANCHOR_POINT")]
    [Rename("Anchor Offset")]
    public Vector3 m_vecOffset;

    [Tooltip("What offset to keep from the followed object.  Used for every follow type ==BUT== ANCHOR_POINT")]
    [Rename("Directional Offset")]
    public float m_flOffset = 4;

    [Tooltip("What percentage of the distance between my current position and the target's should I move every frame.")]
    [Range(0, 1)]
    public float m_InterpolationFactor = 0.1f;

    [Tooltip("Allows the following object to switch targets after it has already started following an object.")]
    [Rename("Can Switch Targets")]
    public bool m_bCanBeStolen = true;

    [Tooltip("Set the object to become a child of whatever it is following.")]
    [Rename("Become Child")]
    public bool m_bBecomeChild = false;

    [Tooltip("Set the object to follow the last child of the target object, rather than the target object itself.")]
    [Rename("Follow Child")]
    public bool m_bFollowChild = false;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    /**
    * \fn OnStart
    * \brief Sets up event listening if necessary.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if(string.IsNullOrEmpty(m_TargetFollowTag))
        {
            if (m_bPrintDebug)
                LPK_PrintError(this, "No string set as a follow object!");
        }
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

        //Set active state.
        if (m_eToggleType == LPK_ToggleType.ON)
            m_bActive = true;
        else if (m_eToggleType == LPK_ToggleType.OFF)
            m_bActive = false;
        else
            m_bActive = !m_bActive;

        if (m_bCanBeStolen && data.m_pSender != null)
        {
            m_pCurFollowObj = data.m_pSender;

            if (m_bBecomeChild)
                transform.SetParent(m_pCurFollowObj.transform);
        }

        //Set so all children are under one parent and not each other.
        GameObject initialFollow = m_pCurFollowObj;

        if (m_bFollowChild && m_pCurFollowObj.transform.childCount > 0)
            m_pCurFollowObj = m_pCurFollowObj.transform.GetChild(m_pCurFollowObj.transform.childCount - 1).gameObject;

        if (m_bBecomeChild)
            transform.SetParent(initialFollow.transform);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Object Following Active");
    }

    /**
    * \fn FixedUpdate
    * \brief Manages movement of object following.
    * 
    * 
    **/
    void FixedUpdate()
    {
        if (m_pCurFollowObj == null)
            FindFollowObject();

        //No object was able to be found.
        if (m_pCurFollowObj == null)
            return;

        //If the target object doesnt exist or doesnt have a tranform, do nothing.
        if (m_pCurFollowObj == null || m_pCurFollowObj.transform == null)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "No object set to follow.  Set in inspector or use a different event activator.");

            return;
        }

        if (!m_bActive)
            return;

        Vector3 targetPos = new Vector3();

        if (m_eFollowType == LPK_FollowType.ANCHOR_POINT)
            targetPos = m_pCurFollowObj.transform.position + m_vecOffset;
        else if(m_eFollowType == LPK_FollowType.IN_FRONT)
            targetPos = m_pCurFollowObj.transform.position + m_pCurFollowObj.transform.up.normalized * m_flOffset;
        else if (m_eFollowType == LPK_FollowType.BEHIND)
            targetPos = m_pCurFollowObj.transform.position + -m_pCurFollowObj.transform.up.normalized * m_flOffset;
        else if (m_eFollowType == LPK_FollowType.LEFT)
            targetPos = m_pCurFollowObj.transform.position + -m_pCurFollowObj.transform.right.normalized * m_flOffset;
        else if (m_eFollowType == LPK_FollowType.RIGHT)
            targetPos = m_pCurFollowObj.transform.position + m_pCurFollowObj.transform.right.normalized * m_flOffset;

        //Interpolate from current position to target object's position
        transform.position = Vector3.Lerp(transform.position, targetPos, m_InterpolationFactor);
    }

    /**
    * \fn FindFollowObject
    * \brief Sets the ideal object to follow.  Will always be the first object with the tag found.  As such
    *                the tag used to find while following should only ever exist once in a scene.
    * 
    * 
    **/
    void FindFollowObject()
    {
        if (string.IsNullOrEmpty(m_TargetFollowTag))
            return;

        m_pCurFollowObj = GameObject.FindGameObjectWithTag(m_TargetFollowTag);

        if (m_bBecomeChild)
            transform.SetParent(m_pCurFollowObj.transform);
    }
}
