/***************************************************
\file           LPK_MagneticField.cs
\author        Christopher Onorati
\date   2/25/2019
\version   2.17

\brief
  This component causes objects within its radius to be pulled or pushed away.

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
* \brief Field which attracts or repels objects like a magnet.
**/
[RequireComponent(typeof(Transform))]
public class LPK_MagneticField : LPK_LogicBase
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

    [Tooltip("Radius of the field.")]
    [Rename("Radius")]
    public float m_flRadius = 10.0f;

    [Tooltip("Tags that the GameObjects must have to be affected.  Using this is much less expensive.  If not set, any GameObject with a Rigidbody will be affected.")]
            [TagDropdown]

    public string[] m_SearchTags;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    //NOTENOTE: How many objets to effect per frame.  The higher this is, the more expensive the effect.  Hardset to 128.
    const int m_GatherCount = 128;

    /**
    * \fn OnStart
    * \brief Sets up event listening.
    * 
    * 
    **/
    override protected void OnStart ()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnDrawGizmosSelected
    * \brief Visualizer for the radius of the magnet
    * 
    * 
    **/
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_flRadius);
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
    * \fn OnUpdate
    * \brief Gathers a list of objects to potentially push and then does the thing.
    * 
    * 
    **/
    override protected void OnUpdate ()
    {
        if (!m_bActive)
            return;

        List<GameObject> objects = new List<GameObject>();

        //No tags set.  Select all.
        if (m_SearchTags.Length == 0)
        {
            GetGameObjectsInRadius(objects, m_flRadius, m_GatherCount);

            for (int j = 0; j < objects.Count; j++)
            {
                if (objects[j].GetComponent<Rigidbody2D>() != null)
                    PushObject(objects[j]);
            }
        }

        else
        {
            for (int i = 0; i < m_SearchTags.Length; i++)
            {
                //NOTENOTE: Technically you could just use a sphere collider for this - but the designer may want to use a sphere collider
                // for something else relevant to the gameobject, so this we instead manually grab.  More expensive but worth the cost.
                GetGameObjectsInRadius(objects, m_flRadius, m_GatherCount, m_SearchTags[i]);

                for (int j = 0; j < objects.Count; j++)
                {
                    if (objects[j].GetComponent<Rigidbody2D>() != null)
                        PushObject(objects[j]);
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

        if (tarRigidBody == null)
            return;

        float distanceScalar = 1.0f;

        //Set distance scalar.
        if (!m_bConstantForce)
        {
            float distance = Vector3.Distance(target.transform.position, transform.position);
            distanceScalar = Mathf.Clamp(1.0f - (distance / m_flRadius), 0.0f, 1.0f);
        }

        Vector3 direction = target.transform.position - transform.position;
        direction = direction.normalized;

        tarRigidBody.AddForce(m_flMagnitude * direction * distanceScalar);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Pushing object:" + target.name);
    }
}
