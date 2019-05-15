/***************************************************
\file           LPK_SetInitialAngularVelocity
\author        Christopher Onorati
\date   3/1/19
\version   2018.3.4

\brief
  This component can be added to any object with a
  RigidBody to cause it to apply an initial angular velocity.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_SetInitialAngularVelocity
* \brief This component can be added to any object with a RigidBody to cause it to spawn with a set angular velocity.
**/
[RequireComponent(typeof(Transform), typeof(Rigidbody2D))]
public class LPK_SetInitialAngularVelocity : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether the velocity should ba applied every frame. Will only be applied on initialize otherwise.")]
    [Rename("Every Frame")]
    public bool m_bEveryFrame = false;

    [Tooltip("Angular force to be applied.")]
    [Rename("Force")]
    public float m_flAngularForce = 5;

    [Tooltip("Variance to apply to Force for randomized movement.")]
    [Rename("Variance")]
    public float m_flVariance;

    /************************************************************************************/
    private Rigidbody2D m_cRigidBody;

    /**
    * \fn OnStart
    * \brief Applies initial angular velocity and sets rigidbody component.
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
        if (m_bEveryFrame)
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
        float frameForce = m_flAngularForce + Random.Range(-m_flVariance, m_flVariance);

        m_cRigidBody.angularVelocity = frameForce;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Angular Velocity Applied");
    }
}
