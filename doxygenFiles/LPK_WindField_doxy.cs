/***************************************************
\file           LPK_WindField.cs
\author        Christopher Onorati
\date   2/25/2019
\version   2018.3.4

\brief
  This component causes objects within its collider to be
  blown away like the wind, or pulled in like a vortex.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_MagneticField
* \brief Wind field which pushes objects away.
**/
[RequireComponent(typeof(Transform), typeof(BoxCollider2D))]
public class LPK_WindField : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether this field should start active or not.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Set the field to apply a constant force, rather than be scaled based on distance.")]
    [Rename("Constant Force")]
    public bool m_bConstantForce = false;

    [Tooltip("Magnitude of the force.  Positive forces repel objects, negative forces pull objects.")]
    [Rename("Magnitude")]
    public float m_flMagnitude = 10.0f;

    [Tooltip("Tags that the GameObjects must have to be affected. Using this is much less expensive.  If not set, any GameObject with a Rigidbody will be affected.")]
    [TagDropdown]
    public string[] m_SearchTags;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    float m_flFieldSize;

    /**
    * \fn OnStart
    * \brief Sets up event listening.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        m_flFieldSize = GetComponent<BoxCollider2D>().size.y;

        //Ensure the collider is a trigger or this will not work.
        if(!GetComponent<BoxCollider2D>().isTrigger)
        {
            GetComponent<BoxCollider2D>().isTrigger = true;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "WindField was not set to have a trigger collider.  Switching automatically.  Please resolve in editor!");
        }
    }

    /**
    * \fn OnUpdate
    * \brief Gets the size of the box every frame in case something modifies it.  This is really sub-optimal but the only way to
    *                gaurantee this is getting set correclty with user-created events and scripts.
    * 
    * 
    **/
    protected override void OnUpdate()
    {
        m_flFieldSize = GetComponent<BoxCollider2D>().size.y;
    }

    /**
    * \fn OnEvent
    * \brief Changes active state of the field.
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
    * \fn LPK_OnTriggerStay2D
    * \brief Manages the pushing of detected objects.
    * \param target - Object to apply force to.
    * 
    **/
    override protected void LPK_OnTriggerStay2D(Collider2D col)
    {
        if (!m_bActive)
            return;

        if (col.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            if (m_SearchTags.Length == 0)
                PushObject(col.gameObject);
            else
            {
                for (int i = 0; i < m_SearchTags.Length; i++)
                {
                    if (col.gameObject.tag == m_SearchTags[i])
                        PushObject(col.gameObject);
                }
            }
        }
    }

    /**
    * \fn PushObject
    * \brief Manages the pushing of detected objects.
    * \param target - Object to apply force to.
    * 
    **/
    void PushObject(GameObject target)
    {
        Rigidbody2D tarRigidBody = target.GetComponent<Rigidbody2D>();

        //Get the right axis of the GameObject.
        Vector3 lineDir = transform.right.normalized;

        //Get direction from target to the game object.
        Vector3 direction = target.transform.position - transform.position;

        //Get the dot product of the direction onto the line.
        float dot = Vector3.Dot(direction, lineDir);

        //Get the nearest point from the target to the line.
        Vector3 nearestPoint = transform.position + lineDir * dot;

        //Correct direction to be between the target and the nearest point, instead of the target and the point of this GameObject.
        direction = target.transform.position - nearestPoint;

        float distanceScalar = 1.0f;

        if (!m_bConstantForce)
        {
            float distance = Vector3.Distance(target.transform.position, nearestPoint);
            distanceScalar = Mathf.Clamp(1.0f - (distance / m_flFieldSize), 0.0f, 1.0f);
        }

        tarRigidBody.AddForce(m_flMagnitude * direction.normalized * distanceScalar * (1/ Time.smoothDeltaTime));

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Pushing object:" + target.name);
    }
}
