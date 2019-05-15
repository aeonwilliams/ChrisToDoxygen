/***************************************************
\file           LPK_FaceVelocity.cs
\author        Christopher Onorati
\date   2/15/2019
\version   2018.3.4

\brief
  This component causes an object to face its velocity
  either on start or every frame.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_FaceVelocity
* \brief Basic facing component.
**/
[RequireComponent(typeof(Rigidbody2D))]
public class LPK_FaceVelocity : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether to update the object to face velocity on start or every frame.")]
    [Rename("Every Frame")]
    public bool m_bEveryFrame = true;

    /************************************************************************************/

    Rigidbody2D m_cRigidBody;

    /**
     * \fn OnStart
     * \brief Checks to ensure proper components are on the object for facing.
     * 
     * 
     **/
    override protected void OnStart()
    {
        m_cRigidBody = GetComponent<Rigidbody2D>();

        if (!m_bEveryFrame)
        {
            Vector2 dir = m_cRigidBody.velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    /**
    * \fn OnUpdate
    * \brief Manages facing direction if user specified every frame updating.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bEveryFrame)
            return;

        Vector2 dir = m_cRigidBody.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
