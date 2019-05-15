/***************************************************
\file           LPK_VelocityTowardsGameObject
\author        Christopher Onorati
\date   2/25/2019
\version   2018.3.4

\brief
  This component can be added to any object with a
  RigidBody to cause it to apply a velocity force towards
  another object.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_VelocityTowardsGameObject
* \brief This component can be added to any game object with a RigidBody to cause it to move towards a game object.
**/
[RequireComponent(typeof(Transform), typeof(Rigidbody2D))]
public class LPK_VelocityTowardsGameObject : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether the velocity should ba applied every frame. Will only be applied once otherwise.")]
    [Rename("Only Once")]
    public bool m_bOnlyOnce = false;

    [Tooltip("Game object to move towards.  If set to null, try to find the first object with the specified tag.")]
    [Rename("Target Game Object")]
    public GameObject m_pTargetGameObject;

    [Tooltip("Tags to move the object towards.  Will start at the top trying to find the first object of the tag.")]
    [TagDropdown]
    public string[] m_TargetTags;

    [Tooltip("Max distance used to search for game objects.  If set to 0, detect objects anywhere.")]
    [Rename("Detect Radius")]
    public float m_flRadius = 10.0f;

    [Tooltip("Force to be applied.")]
    [Rename("Force")]
    public float m_flSpeed = 5;

    /************************************************************************************/

    bool m_bHasAppliedVelocity;

    /************************************************************************************/

    Rigidbody2D m_cRigidBody;

    /**
    * \fn OnStart
    * \brief Sets rigidbody component.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cRigidBody = GetComponent<Rigidbody2D>();
    }

    /**
    * \fn OnUpdate
    * \brief Applies ongoing velocity if appropriate.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if(m_bOnlyOnce && m_bHasAppliedVelocity)
            return;
        
        ApplyVelocity();
    }

    /**
     * \fn ApplyVelocity
     * \brief Applies velocity to object.
     * 
     * 
     **/
    void ApplyVelocity()
    {
        if (m_pTargetGameObject == null)
            if (!FindGameObject())
                return;

        //Velocity application.
        m_cRigidBody.velocity = (m_pTargetGameObject.transform.position - transform.position).normalized * m_flSpeed;

        m_bHasAppliedVelocity = true;
    }

    /**
     * \fn FindGameObject
     * \brief Applies ongoing velocity if appropriate.
     * 
     * \return bool - True/false of if a game object was found and set.
     **/
    bool FindGameObject()
    {
        for (int i = 0; i < m_TargetTags.Length; i++)
        {
            List<GameObject> objects = new List<GameObject>();
            GetGameObjectsInRadius(objects, m_flRadius, 1, m_TargetTags[i]);

            if(objects[0] != null)
            {
                m_pTargetGameObject = objects[0];
                return true;
            }
        }

        return false;
    }
}
