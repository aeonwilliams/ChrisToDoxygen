/***************************************************
\file           LPK_ShakeOnEvent.cs
\author        Christopher Onorati
\date   12/3/2018
\version   2.17

\brief
  This component causes the specified object to shake when
  the specified event is received.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ShakeOnEvent
* \brief Component to enable shaking of objects.
**/
[RequireComponent(typeof(Transform))]
public class LPK_ShakeOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Object to shake when the specified event is received.")]
    [Rename("Target Shake Object")]
    public GameObject m_pTargetShakeObject;

    [Tooltip("Flag to check if the object shaking is normally static in translation (should never move).")]
    [Rename("Static Translation")]
    public bool m_bIsStaticTranslation = true;

    [Tooltip("Flag to check if the object shaking is normally static in rotation (never changes angle values).")]
    [Rename("Static Rotation")]
    public bool m_bIsStaticRotation = true;

    [Tooltip("How much intensity should be added per shake")]
    [Range(0, 1)]
    [Rename("Intensity")]
    public float m_flIntensity = 0.25f;

    [Tooltip("Set a curve for the shake.  High values are larger curves.")]
    [Range(1, 10)]
    [Rename("Intensity Exponent")]
    public float m_flIntensityExponent = 2.0f;
  
    [Tooltip("The maximum distance that the object should displace itself at maximum intensity")]
    [Rename("Translational Magnitude")]
    public Vector3 m_vecTranslationalMagnitude = new Vector3(1, 1, 0);

    [Tooltip("The maximum Euler angles (in degrees) that the object should rotate itself at maximum intensity")]
    [Rename("Rotational Magnitude")]
    public Vector3 m_vecRotationalMagnitude = new Vector3(0, 0, 15);

    [Tooltip("The rate (in intensity amount per second) at which the current shake intensity should decay.")]
    [Rename("Decay Rate")]
    public float m_flDecayRate = 2.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    // Every frame that this value is above 0, the shake target will shake. This value constantly
    // undergoes linear decay at a rate defined by DecayRate, above. This value is capped at 1.
    private float m_flCurrentIntensity = 0.0f;

    private bool m_bActive = false;

    private Vector3 m_vecInitialPosition;
    private Vector3 m_vecInitialAngles;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object spawning.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Begins the shaking of the specified object.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");

        m_bActive = true;

        // When the event comes through, if there's already some CurrentIntensity present, then the
        // object is already shaking, so there's no need to BeginShaking
        if (m_flCurrentIntensity <= 0)
            BeginShaking();

        // Add this component's defined Intensity onto the CurrentIntensity, but don't exceed 1
        m_flCurrentIntensity = Mathf.Clamp(m_flCurrentIntensity + m_flIntensity, 0, 1);
    }

    /**
    * \fn BeginShaking
    * \brief Begins the shaking of the specified object.
    * 
    * 
    **/
    void BeginShaking()
    {
        m_vecInitialPosition = transform.position;
        m_vecInitialAngles = transform.eulerAngles;
        m_bActive = true;
    }

    /**
    * \fn EndShaking
    * \brief Ends the shaking of the specified object.
    * 
    * 
    **/
    void EndShaking()
    {
        if (m_bIsStaticTranslation)
            m_pTargetShakeObject.transform.position = m_vecInitialPosition;
        if (m_bIsStaticRotation)
            m_pTargetShakeObject.transform.eulerAngles = m_vecInitialAngles;

        m_bActive = false;
    }

    /**
    * \fn FixedUpdate
    * \brief Manages actual camera movement.
    * 
    * 
    **/
    void FixedUpdate()
    {
        if (!m_bActive)
            return;

        if (m_bIsStaticTranslation)
            m_pTargetShakeObject.transform.position = m_vecInitialPosition;
        if(m_bIsStaticRotation)
            m_pTargetShakeObject.transform.eulerAngles = m_vecInitialAngles;

        // Shake intensity is more interesting when it's curved, so we raise the CurrentIntensity
        // to the power set by the user. A higher exponent makes for a sharper curve
        var perceivedIntensity = Mathf.Pow(m_flCurrentIntensity, m_flIntensityExponent);

        // Determine the point in the ellipsoid (as described above)
        Vector3 pointOnEllipsoid = Vector3.Scale(Random.insideUnitSphere, m_vecTranslationalMagnitude);
        var r = Random.Range(0, perceivedIntensity);
        var pos = transform.position + r * pointOnEllipsoid;

        // Determine the random angles (as described above)
        float xAngle = Random.Range(0, m_vecRotationalMagnitude.x);
        float yAngle = Random.Range(0, m_vecRotationalMagnitude.y);
        float zAngle = Random.Range(0, m_vecRotationalMagnitude.z);
        Vector3 angles =  new Vector3(xAngle, yAngle, zAngle) * m_flCurrentIntensity;

        Shake(pos, angles);

        // Reduce the CurrentIntensity by the user-defined DecayRate
        m_flCurrentIntensity -= m_flDecayRate * Time.deltaTime;
    
        // If the CurrentIntensity drops to zero (or past it), the shaking is complete
        if (m_flCurrentIntensity <= 0)
        {
            m_flCurrentIntensity = 0;
            EndShaking();
        }
    }

    /**
    * \fn Shake
    * \brief Set the angle and position of the object.
    * 
    * 
    **/
    void Shake(Vector3 posOffset, Vector3 angles)
    {
        // The specified shake target is moved to the calculated offset and rotated
        // to the calculated Euler angles. All shakes are done in world space
        m_pTargetShakeObject.transform.position = posOffset;
        m_pTargetShakeObject.transform.eulerAngles = angles;
    }
}
