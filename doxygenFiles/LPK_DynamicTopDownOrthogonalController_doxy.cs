/***************************************************
\file           LPK_DynamicTopDownOrthogonalController.cs
\author        Christopher Onorati
\date   2/25/2019
\version   2018.3.4

\brief
  This component replicates the movement functionality of 
  top-down characters moving orthogonally at constant 
  velocity. This uses a dynamic RigidBody.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DynamicTopDownOrthogonalController
* \brief Implementation of a basic top down character controller.
**/
[RequireComponent(typeof(Transform), typeof(Rigidbody2D))]
public class LPK_DynamicTopDownOrthogonalController : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_FaceVelocityModes
    {
        DONT_FACE,
        ROTATE_TO_FACE,
        SNAP_TO_FACE,
    };

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
    public string m_MoveLeftButton = "MoveLeft";

    [Tooltip("Virtual button used to move backwards.")]
    [Rename("Backwards Input")]
    public string m_MoveBackwardsButton = "MoveDown";

    [Tooltip("Virtual button used to move right.")]
    [Rename("Right Input")]
    public string m_MoveRightButton = "MoveRight";

    [Tooltip("Speed at which the object will move.")]
    [Rename("Move Speed")]
    public float m_flMoveSpeed = 8.0f;

    [Tooltip("Speed at which the object will rotate to face the right direction if the flag below is set.")]
    [Rename("Rotation Speed")]
    public float m_flRotationSpeed = 8.0f;

    [Tooltip("Whether this object should rotate to face the direction of movement.")]
    [Rename("Face Velocity")]
    public LPK_FaceVelocityModes m_eFaceVelocity = LPK_FaceVelocityModes.SNAP_TO_FACE;

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
    * \brief Manages movement of the object.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        //If Axtive is false, do nothing
        if (!m_bActive)
            return;

        bool bDidMove = false;

        //Variable to determine the movement direction
        Vector3 dir = Vector3.zero;

        //Handle absolute movement type (always orthogonal in the X and Y axis)
        if (!string.IsNullOrEmpty(m_MoveForwardButton) && Input.GetButton(m_MoveForwardButton))
        {
            dir += Vector3.up;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move forward.");
        }

        if (!string.IsNullOrEmpty(m_MoveLeftButton) && Input.GetButton(m_MoveLeftButton))
        {
            dir -= Vector3.right;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move left.");
        }

        if (!string.IsNullOrEmpty(m_MoveBackwardsButton) && Input.GetButton(m_MoveBackwardsButton))
        {
            dir -= Vector3.up;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move down.");
        }

        if (!string.IsNullOrEmpty(m_MoveRightButton) && Input.GetButton(m_MoveRightButton))
        {
            dir += Vector3.right;

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Move right.");
        }

        if (dir != Vector3.zero)
            bDidMove = true;

        if (bDidMove)
        {
            if (m_eFaceVelocity == LPK_FaceVelocityModes.ROTATE_TO_FACE)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, dir.normalized), Time.fixedDeltaTime * m_flRotationSpeed);
            else if (m_eFaceVelocity == LPK_FaceVelocityModes.SNAP_TO_FACE)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, dir.normalized);
        }

        //Apply velocity
        m_cRigidBody.velocity = dir.normalized * m_flMoveSpeed;
    }
}
