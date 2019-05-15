/***************************************************
\file           LPK_TrackingCameraObject.cs
\author        Christopher Onorati
\date   12/8/2018
\version   2.17

\brief
  This component allows for an object to be added to a
  dynamic tracking system for a camera in 2D gameplay.
  This will work with perspective or orthogonal cameras.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_TrackingCameraObject
* \brief Used to communicate to the game that an important object exists.
**/
[RequireComponent(typeof(Transform))]
public class LPK_TrackingCameraObject : LPK_LogicBase
{
    /************************************************************************************/

    public enum ObjectTrackType
    {
        INSTANTANEOUS,
        DISTANCE,
        VIEWPORT,
    }

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Marks the different add styles for important objects.")]
    [Rename("Tracking Type")]
    public ObjectTrackType m_eTrackingType = ObjectTrackType.INSTANTANEOUS;

    [Tooltip("Weight of the object's importance towards the camera.")]
    [Rename("Importance Factor")]
    public float m_flImportanceWeight = 1.0f;

    [Tooltip("Distance at which to add objects to the camera.")]
    [Rename("Max Add Distance")]
    public float m_flMaxAddDistance;

    /************************************************************************************/

    bool m_bHasBeenAdded = false;

    /**
    * \fn OnStart
    * \brief Manage initial event hookup.
    * 
    * 
    **/
    override protected void OnStart()
    {
        //Adding the object to the Camera's array regardless of position.
        if (m_eTrackingType== ObjectTrackType.INSTANTANEOUS)
            AddObject();

        //Setting up logic to only add the object if it is within a certain distace.
        else if (m_eTrackingType == ObjectTrackType.DISTANCE)
            LPK_TrackingCamera.OnTrackingCameraMove += TrackingCameraMoved;

        //Setting up logic to add and remove the object based on if it visisble.
        else if (m_eTrackingType == ObjectTrackType.VIEWPORT)
            LPK_TrackingCamera.OnTrackingCameraMove += DetectVisibility;
    }

    /**
    * \fn TrackingCameraMove
    * \brief Checks the object's location relative to the camera's location for adding and removing objects.
    * 
    * 
    **/
    void TrackingCameraMoved(LPK_TrackingCamera.TrackingCamera_MoveEvent data)
    {
        Vector2 obj1 = new Vector2(transform.position.x, transform.position.y);
        Vector2 obj2 = new Vector2(data.m_vecCameraLocation.x, data.m_vecCameraLocation.y);
        
        //Adds the object to the imporant camera.
        if(Vector2.Distance(obj1, obj2) <= m_flMaxAddDistance && !m_bHasBeenAdded)
            AddObject();
        
        //Removes the object if it was added but is now out of distance.
        else if(Vector2.Distance(obj1, obj2) > m_flMaxAddDistance && m_bHasBeenAdded)
            RemoveObject();
    }

    /**
    * \fn DetectVisibility
    * \brief Adds and removes objects from the camera's list of objects to track based on visibility in the viewport.
    * 
    * 
    **/
    void DetectVisibility(LPK_TrackingCamera.TrackingCamera_MoveEvent data)
    {
        //Adding the object to the list of objects to track.
        if (GetComponent<Renderer>() && GetComponent<Renderer>().isVisible && !m_bHasBeenAdded)
            AddObject();

        //Removing the object from the camera's list of objects to track.
        else if(GetComponent<Renderer>() && !GetComponent<Renderer>().isVisible && m_bHasBeenAdded)
            RemoveObject();
    }

    /**
    * \fn AddObject
    * \brief Adds an object to the camera tracker.
    * 
    * 
    **/
    void AddObject() 
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Attempting to add object to dynamic camera.");

        m_bHasBeenAdded = true;
        LPK_EventReceivers sendinfo = new LPK_EventReceivers();
        sendinfo.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, sendinfo);
        data.m_flData.Add(m_flImportanceWeight);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CameraEventTrigger = new LPK_EventList.LPK_CAMERA_EVENTS[] { LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectAdd };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn RemoveObject
    * \brief Removes an object from the camera tracker.
    * 
    * 
    **/
    void RemoveObject()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Attempting to remove object to dynamic camera.");

        m_bHasBeenAdded = false;
        LPK_EventReceivers sendinfo = new LPK_EventReceivers();
        sendinfo.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, sendinfo);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CameraEventTrigger = new LPK_EventList.LPK_CAMERA_EVENTS[] { LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectRemove };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    /**
    * \fn OnDestroyed
    * \brief Used to remove the object add and remove events as they are not
    *                named OnEvent and therefore cannot be removed by the base class.
    *                
    *                This is a good exmaple for students to see how to make a complex
    *                component that requires a bit more interfacing with the LPK
    *                system.
    * 
    * 
    **/
    override protected void OnDestroyed()
    {
        LPK_TrackingCamera.OnTrackingCameraMove -= TrackingCameraMoved;
        LPK_TrackingCamera.OnTrackingCameraMove -= DetectVisibility;
    }
}
