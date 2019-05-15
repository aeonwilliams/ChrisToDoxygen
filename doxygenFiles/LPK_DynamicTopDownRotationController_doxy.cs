/***************************************************
\file           LPK_DynamicTopDownRotationController.cs
\author        Christopher Onorati
\date   2/25/2019
\version   2018.3.4

\brief
  This component replicates the player controls of a tank 
  or space ship using keys to move forward / backward 
  and rotating. This uses a dynamic RigidBody.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DynamicTopDownRotationController
* \brief Implementation of a vehicle top down character controller.
**/
[RequireComponent(typeof(Transform), typeof(Rigidbody2D))]
public class LPK_DynamicTopDownRotationController : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether this controller should be active or not.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("Virtual button used to move forward.")]
    [Rename("Forward Input")]
    public string m_MoveForwardButton = "MoveUp";

    [Tooltip("Virtual button used to move left.")]
    [Rename("Left Input")]
    public string m_RotateLeftButton = "MoveLeft";

    [Tooltip("Virtual button used to move right.")]
    [Rename("Backwards Input")]
    public string m_MoveBackwardsButton = "MoveDown";

    [Tooltip("Virtual button used to move backwards.")]
    [Rename("Right Input")]
    public string m_RotateRightButton = "MoveRight";

    [Tooltip("How fast the character will rotate.")]
    [Rename("Rotation Speed")]
    public float m_flRotationSpeed = 45.0f;

    [Tooltip("Speed the character will move at.")]
    [Rename("Acceleration Speed")]
    public float m_flAccelerationSpeed = 5.0f;

    [Tooltip("Maximum speed the character is allowed to move at.")]
    [Rename("Max Speed")]
    public float m_flMaxSpeed = 10.0f;

    [Tooltip("Drag factor to be applied to the character.  Higher values are less drag.")]
    [Rename("Drag Factor")]
    [Range(0, 1)]
    public float m_flDragFactor = .98f;

    /************************************************************************************/

    private Rigidbody2D m_cRigidBody;

    /**
    * \fn OnStart
    * \brief Checks to ensure proper components are on the object for movement.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cRigidBody = GetComponent<Rigidbody2D>();
    }

    /**
    * \fn OnUpdate
    * \brief Manages movement.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        //If Active is false, do nothing
        if (!m_bActive)
            return;

        //Handle forward movement
        if (!string.IsNullOrEmpty(m_MoveForwardButton) && Input.GetButton(m_MoveForwardButton))
        {
            m_cRigidBody.velocity += (Vector2)transform.up * m_flAccelerationSpeed;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move forward.");
        }

        //Handle backward movement
        if (!string.IsNullOrEmpty(m_MoveBackwardsButton) && Input.GetButton(m_MoveBackwardsButton))
        {
            m_cRigidBody.velocity -= (Vector2)transform.up * m_flAccelerationSpeed;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move backwards.");
        }

        //Handle left rotation
        if (!string.IsNullOrEmpty(m_RotateLeftButton) && Input.GetButton(m_RotateLeftButton))
        {
            transform.Rotate(new Vector3(0, 0, m_flRotationSpeed));

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Rotate left.");
        }

        //Handle right rotation
        if (!string.IsNullOrEmpty(m_RotateRightButton) && Input.GetButton(m_RotateRightButton))
        {
            transform.Rotate(new Vector3(0, 0, -m_flRotationSpeed));

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Rotate right.");
        }

        //Limit the maximum movement speed
        m_cRigidBody.velocity = Vector3.ClampMagnitude(m_cRigidBody.velocity, m_flMaxSpeed);
        m_cRigidBody.velocity *= m_flDragFactor;
    }
}
