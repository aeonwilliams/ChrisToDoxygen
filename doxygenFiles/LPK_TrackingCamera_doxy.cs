/***************************************************
\file           LPK_TrackingCamera.cs
\author        Christopher Onorati
\date   2/1/2019
\version   2.17

\brief
  This component allows for a dynamic tracking camera in a
  2D game.  This will work with perspective or orthogonal
  cameras.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_TrackingCamera
* \brief Implementation of a weighted tracking camera for 2D games.
**/
[RequireComponent(typeof(Camera), typeof(Transform))]
public class LPK_TrackingCamera : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Offset to add to the end of the calculation on where the camera should look.")]
    [Rename("Position Offset")]
    public Vector3 m_vecOffset;

    [Tooltip("Tags to follow, specifically.  If set to 0, then all tags are followed.")]
            [TagDropdown]

    public string[] m_FollowTags;

    [Tooltip("Lock X translation to disable movement of the camera in this axis.")]
    [Rename("Lock X Translation")]
    public bool m_bLockXTranslation = false;

    [Tooltip("Lock Y translation to disable movement of the camera in this axis.")]
    [Rename("Lock Y Translation")]
    public bool m_bLockYTranslation = false;

    [Tooltip("Maximum allowed movement in translation per frame.")]
    [Rename("Translation Movement")]
    public float m_flMaxTranslationChangeScalar = 0.2f;

    [Tooltip("Max change in camera size per frame.")]
    [Rename("Camera Size Scalar")]
    public float m_flMaxSizeScalar = 0.2f;

    [Tooltip("Minimum allowed size of the camera.")]
    [Rename("Min Camera Size")]
    public float m_flMinSize = 6.5f;

    [Tooltip("Maximum allowed size of the camera.")]
    [Rename("Max Camera Size")]
    public float m_flMaxSize = 10.0f;

    [Tooltip("Scalar for the alteration of camera size.")]
    [Rename("Size Scalar Point")]
    public float m_flSizeScalarPoint = 10.0f;

    /************************************************************************************/

    //Stores initial Z value of camera.
    float m_flCameraZ;

    //Stores the initial size of the camera.
    float m_flInitialSize;

    //Holds a list of all important objects.
    List<GameObject> m_aImportantObjects = new List<GameObject>();
    //Holds a list of each object's importance.
    List<float> m_aImportanceWeights = new List<float>();

    float m_flLockedXValue;
    float m_flLockedYValue;

    //Location of the camera on the last frame.
    Vector3 m_vecPreviousLocation;

    /************************************************************************************/

    Camera m_cCamera;

    /************************************************************************************/
    /*******************************EVENT_SENDING_INFO***********************************/
    /************************************************************************************/

    public delegate void TrackingCameraMove(TrackingCamera_MoveEvent data);
    public static event TrackingCameraMove OnTrackingCameraMove;

    /**
    * \class TrackingCameraMove_Event
    * \brief Event to communicate to objects when important objects move.
    **/
    public struct TrackingCamera_MoveEvent
    {
        public Vector3 m_vecCameraLocation;
        public float m_flLeftX;
        public float m_flRightX;
        public float m_flTopY;
        public float m_flBottomY;
    }

    /**
    * \fn Awake
    * \brief Sets up initial values for the camera to use. Awake
    *                is good to use in this case as we must make sure all
    *                objects are spawned before we send out distance events.
    * 
    * 
    **/
    void Awake ()
    {
        m_cCamera = GetComponent<Camera>();

        m_flCameraZ = transform.position.z;

        m_vecPreviousLocation = transform.position;

        if (m_cCamera.orthographic)
            m_flInitialSize = m_cCamera.orthographicSize;
        else
            m_flInitialSize = m_cCamera.fieldOfView;

        LPK_EventManager.OnLPK_TrackingCameraObjectAdd += AddImportantObject;
        LPK_EventManager.OnLPK_TrackingCameraObjectRemove += RemoveImportantObject;

        SendDistanceEvent();

        if (m_bLockXTranslation)
            m_flLockedXValue = transform.position.y;
        if (m_bLockYTranslation)
            m_flLockedYValue = transform.position.y;
    }

    /**
    * \fn FixedUpdate
    * \brief Manages camera movement.
    * 
    * 
    **/
    void FixedUpdate()
    {
        Vector3 pos = new Vector3();

        //Preventing camera updating if there are no objects to check for.
        if (m_aImportantObjects.Count <= 0)
            return;

        //Averaging the location of all important objects.
        for (int i = 0; i < m_aImportantObjects.Count; i++)
        {
            if (m_aImportantObjects[i] != null)
            {
                pos.x += m_aImportantObjects[i].transform.position.x * m_aImportanceWeights[i];
                pos.y += m_aImportantObjects[i].transform.position.y * m_aImportanceWeights[i];
            }

            //Cleaning the array.
            else
            {
                m_aImportantObjects.RemoveAt(i);
                m_aImportanceWeights.RemoveAt(i);
            }
        }

        float totalImportance = 0.0f;

        //Finding the total weight.
        if (m_aImportanceWeights.Count >= 1)
        {
            for (int i = 0; i < m_aImportanceWeights.Count; i++)
                totalImportance += m_aImportanceWeights[i];
        }

        pos = pos / totalImportance;

        if (m_bLockXTranslation)
            pos.x = m_flLockedXValue;
        if (m_bLockYTranslation)
            pos.y = m_flLockedYValue;

        pos += m_vecOffset;

        transform.position = Vector3.Lerp(transform.position, pos, m_flMaxTranslationChangeScalar);
        transform.position = new Vector3(transform.position.x, transform.position.y, m_flCameraZ);

        //Notifies all objects that the camera has moved.
        if (transform.position != m_vecPreviousLocation)
            SendDistanceEvent();

        m_vecPreviousLocation = transform.position;

        ScaleCameraView();
    }

    /**
    * \fn ScaleCameraView
    * \brief Scales the view of the camera based on the position of objects from the camera.
    * 
    * 
    **/
    void ScaleCameraView()
    {
        float maxdistance = 0.0f;
        Vector2 camLocation = new Vector2(transform.position.x, transform.position.y);

        //Determining the max distance any object is from the camera's position.
        for (int i = 0; i< m_aImportantObjects.Count; i += 1 )
        {
            if(m_aImportantObjects[i] != null)
            {
                Vector2 objLocation = new Vector2(m_aImportantObjects[i].transform.position.x, m_aImportantObjects[i].transform.position.y);
                float testDistance = Vector2.Distance(camLocation, objLocation);
                
                if(testDistance > maxdistance)
                    maxdistance = testDistance;
            }
            
            //Cleaning the array.
            else
            {
                m_aImportantObjects.RemoveAt(i);
                m_aImportanceWeights.RemoveAt(i);
            }
        }
        
        float scalar = (maxdistance / m_flSizeScalarPoint);
        float changeAmount = m_flInitialSize + scalar;

        if (m_cCamera.orthographic)
            UpdateCameraSize(scalar, changeAmount);
        else
            UpdateCameraFOV(scalar, changeAmount);
    }

    /**
    * \fn UpdateCameraSize
    * \brief Updates the size of the camera if orthographic.
    * \param scalar - Value to modify the size of the camera by.
    *                changeAmount - Amount of change to make in camera size.
    * 
    **/
    void UpdateCameraSize(float scalar, float changeAmount)
    {
        if (m_cCamera.orthographicSize != m_flInitialSize + scalar)
        {
            //Max cap for size change.
            if (changeAmount - m_cCamera.orthographicSize > m_flMaxSizeScalar)
                changeAmount = m_cCamera.orthographicSize + m_flMaxSizeScalar;

            //Min cap for size change.
            else if (changeAmount - m_cCamera.orthographicSize < -m_flMaxSizeScalar)
                changeAmount = m_cCamera.orthographicSize - m_flMaxSizeScalar;


            m_cCamera.orthographicSize = changeAmount;

            //Max cap.
            if (m_cCamera.orthographicSize > m_flMaxSize)
                m_cCamera.orthographicSize = m_flMaxSize;

            //Min cap.
            else if (m_cCamera.orthographicSize < m_flMinSize)
                m_cCamera.orthographicSize = m_flMinSize;
        }
    }

    /**
    * \fn UpdateCameraFOV
    * \brief Updates the FOV of the camera if perspective.
    * \param scalar - Value to modify the FOV of the camera by.
    *                changeAmount - Amount of change to make in camera FOV.
    * 
    **/
    void UpdateCameraFOV(float scalar, float changeAmount)
    {
        if (m_cCamera.fieldOfView != m_flInitialSize + scalar)
        {
            //Max cap for size change.
            if (changeAmount - m_cCamera.fieldOfView > m_flMaxSizeScalar)
                changeAmount = m_cCamera.fieldOfView + m_flMaxSizeScalar;

            //Min cap for size change.
            else if (changeAmount - m_cCamera.fieldOfView < -m_flMaxSizeScalar)
                changeAmount = m_cCamera.fieldOfView - m_flMaxSizeScalar;


            m_cCamera.fieldOfView = changeAmount;

            //Max cap.
            if (m_cCamera.fieldOfView > m_flMaxSize)
                m_cCamera.fieldOfView = m_flMaxSize;

            //Min cap.
            else if (m_cCamera.fieldOfView < m_flMinSize)
                m_cCamera.fieldOfView = m_flMinSize;
        }
    }

    /**
    * \fn SendDistanceEvent
    * \brief Notifies important objects as to the camera's location.
    * 
    * 
    **/
    void SendDistanceEvent()
    {
        //receiver can be null as we should only have one tracking camera at a time.
        TrackingCamera_MoveEvent data = new TrackingCamera_MoveEvent();

        //Camera position stored in vector 1 of event data.
        data.m_vecCameraLocation = transform.position;
        data.m_flLeftX = m_cCamera.ScreenToWorldPoint(Vector3.zero).x;
        data.m_flBottomY = m_cCamera.ScreenToWorldPoint(Vector3.zero).y;

        data.m_flRightX = m_cCamera.ScreenToViewportPoint(new Vector3(Screen.width, Screen.height, 0)).x;
        data.m_flTopY = m_cCamera.ScreenToViewportPoint(new Vector3(Screen.width, Screen.height, 0)).y;

        if (OnTrackingCameraMove != null)
            OnTrackingCameraMove(data);
    }

    /**
    * \fn AddImportantObject
    * \brief Adds an object to the list of important objects.
    * \param data - ImportantObjectAdd_Event
    * 
    **/
    void AddImportantObject(LPK_EventManager.LPK_EventData data) 
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Adding object to dynamic camera.");

        //Query tag info if desired.
        if(m_FollowTags.Length != 0)
        {
            for(int i = 0; i < m_FollowTags.Length; i++)
            {
                if (data.m_pSender.tag == m_FollowTags[i])
                    break;

                //No tags matched.  Stop early.
                if (i + 1 >= m_FollowTags.Length)
                    return;
            }
        }

        m_aImportantObjects.Add(data.m_pSender);
        m_aImportanceWeights.Add(data.m_flData[0]);
    }

    /**
    * \fn RemoveImportantObject
    * \brief Removes an object to the list of important objects.
    * \param data - ImportantObjectRemove_Event
    * 
    **/
    void RemoveImportantObject(LPK_EventManager.LPK_EventData data)
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Removing object to dynamic camera.");

        int location = -1;

        //Location of the important object.
        for (int i = 0; i < m_aImportantObjects.Count; i++)
        {
            if (m_aImportantObjects[i] == data.m_pSender)
            {
                location = i;
                break;
            }
        }

        //Unable to find the object.
        if (location == -1)
        {
            if (m_bPrintDebug)
                LPK_PrintError(this, "Unable to find object to destroy.  This message should never appear.");

            return;
        }

        m_aImportantObjects.RemoveAt(location);;
        m_aImportanceWeights.RemoveAt(location);
    }

    /**
    * \fn OnDestroy
    * \brief Used to remove the object add and remove events as they are not
    *                named OnEvent and therefore cannot be removed by the base class.
    *                
    *                This is a good exmaple for students to see how to make a complex
    *                component that requires a bit more interfacing with the LPK
    *                system.
    * 
    * 
    **/
    void OnDestroy()
    {
        LPK_EventManager.OnLPK_TrackingCameraObjectAdd -= AddImportantObject;
        LPK_EventManager.OnLPK_TrackingCameraObjectRemove -= RemoveImportantObject;
    }
}
