/***************************************************
\file           LPK_SetInitialVelocity
\author        Christopher Onorati
\date   3/1/2019
\version   2018.3.4

\brief
  This component can be added to any object with a
  RigidBody to cause it to apply an initial velocity.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_SetInitialVelocity
* \brief This component can be added to any object with a RigidBody to cause it to spawn with a set velocity.
**/
[RequireComponent(typeof(Transform), typeof(Rigidbody2D))]
public class LPK_SetInitialVelocity : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether the velocity should ba applied every frame. Will only be applied on initialize otherwise.")]
    [Rename("Every Frame")]
    public bool m_bEveryFrame = false;

    [Tooltip("What direction to apply the velocity in.")]
    [Rename("Direction")]
    public Vector3 m_vecDir;

    [Tooltip("Variance to apply to Direction for randomized movement.")]
    [Rename("Variance")]
    public Vector3 m_vecVariance;

    [Tooltip("Force to be applied.")]
    [Rename("Force")]
    public float m_flSpeed = 5;

    /************************************************************************************/

    Rigidbody2D m_cRigidBody;

    /**
    * \fn OnStart
    * \brief Applies initial velocity and sets rigidbody component.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cRigidBody = GetComponent<Rigidbody2D>();

        if (!m_bEveryFrame)
        {
            ApplyVelocity();
            enabled = false;
        }
    }

    /**
    * \fn OnUpdate
    * \brief Applies ongoing velocity if appropriate.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        ApplyVelocity();
    }

    /**
    * \fn ApplyVelocity
    * \brief Manages velocity change on object with component.
    * 
    * 
    **/
    void ApplyVelocity()
    {
        Vector3 dir = m_vecDir;
        dir.x += Random.Range(-m_vecVariance.x, m_vecVariance.x);
        dir.y += Random.Range(-m_vecVariance.y, m_vecVariance.y);
        dir.z += Random.Range(-m_vecVariance.z, m_vecVariance.z);

        m_cRigidBody.velocity = dir * m_flSpeed;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Velocity Applied");
    }
}
