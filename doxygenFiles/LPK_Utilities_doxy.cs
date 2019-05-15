/***************************************************
\file           LPK_Utilities.cs
\author        Christopher Onorati
\date   2/27/2019
\version   2018.3.4

\brief
  This script contains the event manager and base class
  all LPK objects inheret from.  Please read through
  all documentation carefully before modifying any aspect
  of this script.
  
  Note to add a new event to the system...
  
  1)  Add the event to the LPK_EventList class under the correct enum.
  2)  Add the public static event to the LPK_EventManager class.  Search
      for "EVENT DECLERATIONS" to find the right place in this file.
  3)  Add the call to invoke the event in the InvokeEvent function 
      found in the LPK_EventManager class, within the switch statement.
  4)  Add the ability for an LPK object to hook up to the event in
      InitializeEvent on the LPK_LogicBase class.
  5)  Add the ability for an LPK object to detach from the event in
      the LPK_LogicBase class function DetachFunction.
      

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_LogicBase
* \brief Base object for all LPK objects to inherited off of.
**/
public class LPK_LogicBase : MonoBehaviour
{
    /************************************************************************************/

    public enum LPK_ToggleType
    {
        ON,
        OFF,
        TOGGLE,
    };

    [System.Flags]
    enum CollisionCheckType
    {
        COLLISION_NONE = 0,
        COLLISION_ENTER = 1,
        COLLISION_EXIT = 2,
        COLLISION_STAY = 4,
        COLLISION_TRIGGER_ENTER = 8,
        COLLISION_TRIGGER_EXIT = 16,
        COLLISION_TRIGGER_STAY = 32,
    };

    [System.Flags]
    protected enum VisibilityCheckType
    {
        VISIBLITY_NONE = 0,
        VISIBLITY_ENTER_SCREEN = 1,
        VISIBLITY_EXIT_SCREEN = 2,
        VISIBLITY_PERSIST_ON_SCREEN = 4,
        VISIBLITY_PERSIST_OFF_SCREEN = 8,
    };

    [System.Flags]
    protected enum MouseCheckType
    {
        MOUSE_NONE = 0,
        MOUSE_ENTER = 1,
        MOUSE_EXIT = 2,
        MOUSE_STAY = 4,
        MOUSE_DOWN = 8,
        MOUSE_UP = 16,
        MOUSE_UP_BUTTON = 32,
    };

    /************************************************************************************/
    //**************************ENUMS USED FOR INPUT VALIDATION**************************/
    /************************************************************************************/

    public enum LPK_MouseButtons
    {
        LEFT,
        RIGHT,
        MIDDLE_CLICK,
        MIDDLE_SCROLL_UP,
        MIDDLE_SCROLL_DOWN,
        ANY,
    };

    public enum LPK_ControllerButtons
    {
        A,
        B,
        X,
        Y,
        START,
        BACK,
        GUIDE,
        LEFT_SHOULDER,
        RIGHT_SHOULDER,
        LEFT_STICK,
        RIGHT_STICK,
        LEFT_JOYSTICK_UP,
        LEFT_JOYSTICK_DOWN,
        LEFT_JOYSTICK_LEFT,
        LEFT_JOYSTICK_RIGHT,
        RIGHT_JOYSTICK_UP,
        RIGHT_JOYSTICK_DOWN,
        RIGHT_JOYSTICK_LEFT,
        RIGHT_JOYSTICK_RIGHT,
        LEFT_TRIGGER,
        RIGHT_TRIGGER,
        DPAD_UP,
        DPAD_DOWN,
        DPAD_LEFT,
        DPAD_RIGHT,
        ANY,
    };

    public enum LPK_ControllerNumber
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        ANY,
    };

    /************************************************************************************/

    [Header("Base Properties")]

    [Tooltip("Toggle console debug messages.")]
    [Rename("Print Debug Info")]
    public bool m_bPrintDebug = false;

    [System.Serializable]
    public class CollisionEventInfo
    {

        [Tooltip("Objects to activate collisions.  If both properties are set to null any object will activate the event.  Note this is an OR search with the Activator Tags.")]
        public GameObject[] m_Activators;

        [Tooltip("Tags to activate collisions.  If both properties are set to null any object will activate the event.  Note this is an OR search with the Activator Objects")]
        [TagDropdown]
        public string[] m_ActivatorTags;
    }

    [Tooltip("Event info ONLY used for LPK collision events.")]
    public CollisionEventInfo m_CollisionEventInfo;

    [System.Serializable]
    public class InputEventInfo
    {
        [Tooltip("Specify virutal buttons here that will be validated for button event parsing.  If left empty, all viritual buttons are accepted.")]
        public string[] m_SpecifiedVirtualButtons;

        [Tooltip("Specify keys here that will be validated for keyboard event parsing.  If left empty, all keys are accepted.")]
        public KeyCode[] m_SpecifiedKeys;

        [Tooltip("Specify mouse buttons here that will be validated for mouse event parsing.  If left empty, all keys are accepted.")]
        public LPK_MouseButtons[] m_SpecifiedMouseButtons;

        [Tooltip("Specify gamepad buttons here that will be validated for gamepad event parsing.  If left empty, all buttons are accepted.")]
        public LPK_ControllerButtons[] m_SpecifiedGamepadButtons;

        [Tooltip("Specify gamepad numbers here that will be validated for gamepad event parsing.  If left empty, all numbers are accepted.")]
        public LPK_ControllerNumber[] m_SpecifiedGamepadNumbers;
    }

    [Tooltip("Event info ONLY used for LPK input events.")]
    public InputEventInfo m_InputEventInfo;

    /************************************************************************************/

    CollisionCheckType m_eCollisionCheck = CollisionCheckType.COLLISION_NONE;
    VisibilityCheckType m_eVisibilityCheck = VisibilityCheckType.VISIBLITY_NONE;
    MouseCheckType m_eMouseCheck = MouseCheckType.MOUSE_NONE;

    //Used to detect if a game object is on screen.
    bool m_bIsVisible = false;

    //Used to detect pause state of game.
    bool m_bGamePaused = false;

    //Used to determine if the Update function is in use.
    bool m_bUsesUpdate = true;

    //Check to see if the object has already been counted in statistics.
    bool m_bHasBeenCounted = false;

    /************************************************************************************/

    /**
    * \fn Start
    * \brief Sets up pause event listening and calls OnStart.
    * 
    * 
    **/
    void Start()
    {
        OnStart();

        LPK_EventList pauseList = new LPK_EventList();
        pauseList.m_PauseEventTrigger = new LPK_EventList.LPK_PAUSE_EVENTS[] { LPK_EventList.LPK_PAUSE_EVENTS.LPK_GamePaused };

        LPK_EventList resumeList = new LPK_EventList();
        pauseList.m_PauseEventTrigger = new LPK_EventList.LPK_PAUSE_EVENTS[] { LPK_EventList.LPK_PAUSE_EVENTS.LPK_GameUnpaused };

        InitializeEvent(pauseList, OnPauseEvent, false);
        InitializeEvent(resumeList, OnUnpauseEvent, false);
    }

    /**
    * \fn OnStart
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void OnStart()
    {
        //Implemented by child classes so start will always be called.
    }

    /**
    * \fn GetPauseStatus
    * \brief Get the pause status of the object.  Useful for input functions.
    * 
    * 
    **/
    public bool GetPauseStatus()
    {
        return m_bGamePaused;
    }

    /**
    * \fn Update
    * \brief Prevent game action from proceeding of paused, alongside
                     calling utility functions.
    * 
    * 
    **/
    void Update()
    {
        //Do nothing if paused.
        if (GetPauseStatus())
            return;

        CheckVis();
        OnUpdate();

        //Count component into statistics only once.
        if(!m_bHasBeenCounted)
        {
            LPK_DebugStatistics.AddObjectCount(this);
            m_bHasBeenCounted = true;
        }
    }

    /**
    * \fn OnUpdate
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void OnUpdate()
    {
        //Implemented by child classes so Update will always be called.

        //NOTENOTE: If this flag is flipped, then we know that this component is not making use of Update outside of base LPK functionality.
        //          This data is tracked by LPK_DebugStatistics.
        m_bUsesUpdate = false;
    }

    /**
    * \fn UsesUpdate
    * \brief Used by DebugStatistics to track perfermance of objects.
    * 
    * \return bool - True/false value of if Update is used by the component outside of the LogicBase
    **/
    public bool UsesUpdate()
    {
        return m_bUsesUpdate;
    }

    /**
    * \fn CheckVis
    * \brief Object visibility detection.
    * 
    * 
    **/
    void CheckVis()
    {
        //Game object now on screen.
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().isVisible)
        {
            if (!m_bIsVisible && (m_eVisibilityCheck & VisibilityCheckType.VISIBLITY_ENTER_SCREEN) != 0)
            {
                LPK_EventReceivers receiver = new LPK_EventReceivers();
                receiver.m_GameObjectList = new GameObject[]{ gameObject };

                LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_VisibilityEventTrigger = new LPK_EventList.LPK_VISIBILITY_EVENTS[] { LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreen };

                LPK_EventManager.InvokeEvent(sendEvent, sendData);
            }
            else if ((m_eVisibilityCheck & VisibilityCheckType.VISIBLITY_PERSIST_ON_SCREEN) != 0)
            {
                LPK_EventReceivers receiver = new LPK_EventReceivers();
                receiver.m_GameObjectList = new GameObject[] { gameObject };

                LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_VisibilityEventTrigger = new LPK_EventList.LPK_VISIBILITY_EVENTS[] { LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreenPersist };

                LPK_EventManager.InvokeEvent(sendEvent, sendData);
            }

            m_bIsVisible = true;
        }

        //Game object now off screen.
        else if (GetComponent<Renderer>() && !GetComponent<Renderer>().isVisible)
        {
            if( m_bIsVisible && (m_eVisibilityCheck & VisibilityCheckType.VISIBLITY_EXIT_SCREEN) != 0)
            {
                LPK_EventReceivers receiver = new LPK_EventReceivers();
                receiver.m_GameObjectList = new GameObject[] { gameObject };

                LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_VisibilityEventTrigger = new LPK_EventList.LPK_VISIBILITY_EVENTS[] { LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreen };

                LPK_EventManager.InvokeEvent(sendEvent, sendData);
            }
            else if ((m_eVisibilityCheck & VisibilityCheckType.VISIBLITY_PERSIST_OFF_SCREEN) != 0)
            {
                LPK_EventReceivers receiver = new LPK_EventReceivers();
                receiver.m_GameObjectList = new GameObject[] { gameObject };

                LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_VisibilityEventTrigger = new LPK_EventList.LPK_VISIBILITY_EVENTS[] { LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreenPersist };

                LPK_EventManager.InvokeEvent(sendEvent, sendData);
            }

            m_bIsVisible = false;
        }
    }

    /**
    * \fn GetGameObjectsInRadius
    * \brief Fills a list with gameobjects found within a certain radius of the owner gameobject.
    * \param objects     - List to return objects in.
    *                radius      - Max distance an object can be to be valid.
    *                objectCount - How many objects to find.  -1 is as many as possible.
    *                tagName     - Specify to search for a specific tag.  This is much less expensive.
    * 
    **/
    public void GetGameObjectsInRadius(List<GameObject> objects, float radius, int objectCount = -1, string tagName = "")
    {
        GameObject[] allObjects;

        if (string.IsNullOrEmpty(tagName))
            allObjects = FindObjectsOfType<GameObject>();
        else
            allObjects = GameObject.FindGameObjectsWithTag(tagName);

        for (int i = 0; i < allObjects.Length; i++)
        {
            //Do not detect yourself.
            if (allObjects[i] == gameObject)
                continue;

            if(Vector3.Distance(allObjects[i].transform.position, transform.position) <= radius)
            {
                objects.Add(allObjects[i]);
                objectCount--;

                if (objectCount == 0)
                    break;
            }
        }
    }

    /**
    * \fn OnPauseEvent
    * \brief Detects game pause state.
    * \param data - Activating trigger.
    * 
    **/
    void OnPauseEvent(LPK_EventManager.LPK_EventData data)
    {
        //We do not need to validate this event.  If we are paused, just pause us.
        m_bGamePaused = true;
    }

    /**
    * \fn OnUnpauseEvent
    * \brief Detects game unpause state.
    * \param data - Activating trigger.
    * 
    **/
    void OnUnpauseEvent(LPK_EventManager.LPK_EventData data)
    {
        //We do not need to validate this event.  If we are paused, just pause us.
        m_bGamePaused = false;
    }

    /************************************************************************************/
    /********************************3D Collision Funcs**********************************/
    /************************************************************************************/

    /**
    * \fn OnCollisionEnter
    * \brief Sends an event on colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionEnter(Collision col)
    {
        LPK_OnCollisionEnter(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_ENTER) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject};

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionEnter };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionEnter
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionEnter(Collision col)
    {
        //Implemented by child classes so OnCollisionEnter will always be called.
    }

    /**
    * \fn OnCollisionStay
    * \brief Sends an event on colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionStay(Collision col)
    {
        LPK_OnCollisionStay(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_STAY) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionStay };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionStay
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionStay(Collision col)
    {
        //Implemented by child classes so OnCollisionStay will always be called.
    }

    /**
    * \fn OnCollisionExit
    * \brief Sends an event on stop colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionExit(Collision col)
    {
        LPK_OnCollisionExit(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_EXIT) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionExit };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionExit
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionExit(Collision col)
    {
        //Implemented by child classes so OnCollisionExit will always be called.
    }

    /**
    * \fn OnTriggerEnter
    * \brief Sends an event on colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerEnter( Collider col)
    {
        LPK_OnTriggerEnter(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_ENTER) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerEnter };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerEnter
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerEnter(Collider col)
    {
        //Implemented by child classes so OnTriggerEnter will always be called.
    }

    /**
    * \fn OnTriggerStay
    * \brief Sends an event on colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerStay(Collider col)
    {
        LPK_OnTriggerStay(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_STAY) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerStay };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerStay
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerStay(Collider col)
    {
        //Implemented by child classes so OnTriggerStay will always be called.
    }

    /**
    * \fn OnTriggerExit
    * \brief Sends an event on stop colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerExit(Collider col)
    {
        LPK_OnTriggerExit(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_EXIT) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerExit };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerExit
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerExit(Collider col)
    {
        //Implemented by child classes so OnTriggerExit will always be called.
    }

    /************************************************************************************/
    /********************************2D Collision Funcs**********************************/
    /************************************************************************************/

    /**
    * \fn OnCollisionEnter2D
    * \brief Sends an event on colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionEnter2D(Collision2D col)
    {
        LPK_OnCollisionEnter2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_ENTER) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionEnter };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionEnter2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionEnter2D(Collision2D col)
    {
        //Implemented by child classes so OnCollisionEnter2D will always be called.
    }

    /**
    * \fn OnCollisionStay2D
    * \brief Sends an event on colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionStay2D(Collision2D col)
    {
        LPK_OnCollisionStay2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_STAY) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionStay };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionStay2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionStay2D(Collision2D col)
    {
        //Implemented by child classes so OnCollisionStay2D will always be called.
    }

    /**
    * \fn OnCollisionExit2D
    * \brief Sends an event on stop colliding with another object if applicable.
    * \param col - Collider information.  Not used for event sending.
    * 
    **/
    void OnCollisionExit2D(Collision2D col)
    {
        LPK_OnCollisionExit2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_EXIT) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionExit };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnCollisionExit2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnCollisionExit2D(Collision2D col)
    {
        //Implemented by child classes so OnCollisionExit2D will always be called.
    }

    /**
    * \fn OnTriggerEnter2D
    * \brief Sends an event on colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerEnter2D(Collider2D col)
    {
        LPK_OnTriggerEnter2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_ENTER) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerEnter };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerEnter2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerEnter2D(Collider2D col)
    {
        //Implemented by child classes so OnTriggerEnter2D will always be called.
    }

    /**
    * \fn OnTriggerStay2D
    * \brief Sends an event on colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerStay2D(Collider2D col)
    {
        LPK_OnTriggerStay2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_STAY) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(col.gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerStay };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerStay2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerStay2D(Collider2D col)
    {
        //Implemented by child classes so OnTriggerStay2D will always be called.
    }

    /**
    * \fn OnTriggerExit2D
    * \brief Sends an event on stop colldiing with a trigger object if applicable.
    * \param col - Trigger collided with.
    * 
    **/
    void OnTriggerExit2D(Collider2D col)
    {
        LPK_OnTriggerExit2D(col);

        if ((m_eCollisionCheck & CollisionCheckType.COLLISION_TRIGGER_EXIT) == 0)
            return;

        if (!CheckValidCollision(col.gameObject))
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CollisionEventTrigger = new LPK_EventList.LPK_COLLISION_EVENTS[] { LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerExit };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn LPK_OnTriggerExit2D
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void LPK_OnTriggerExit2D(Collider2D col)
    {
        //Implemented by child classes so OnTriggerExit2D will always be called.
    }

    /**
    * \fn ChecKValidCollision
    * \brief Determines if a collision should result in event sending.
    * \param obj - Object to check for collision validation.
    * \return bool - Result of validation check
    **/
    bool CheckValidCollision(GameObject obj)
    {
        //Not the right game object.
        for (int i = 0; i < m_CollisionEventInfo.m_Activators.Length; i++)
        {
            if (obj == m_CollisionEventInfo.m_Activators[i])
                return true;
        }

        //Tag test.
        for (int i = 0; i < m_CollisionEventInfo.m_ActivatorTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(m_CollisionEventInfo.m_ActivatorTags[i]) && obj.tag == m_CollisionEventInfo.m_ActivatorTags[i])
                return true;
        }

        //If the game object marked as the receiver is the game object this component is on, accept no matter what.
        for (int i = 0; i < m_CollisionEventInfo.m_Activators.Length; i++)
        {
            if (m_CollisionEventInfo.m_Activators[i] != null)
                return false;
        }

        //If the game object marked as the receiver is the game object this component is on, accept no matter what.
        for (int i = 0; i < m_CollisionEventInfo.m_ActivatorTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(m_CollisionEventInfo.m_ActivatorTags[i]))
                return false;
        }

        return true;
    }

    /**
    * \fn OnMouseEnter
    * \brief Detects when the mouse starts hovering over this object.
    * 
    * 
    **/
    void OnMouseEnter()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_ENTER) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseEnterThisObject };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn OnMouseExit
    * \brief Detects when the mouse stops hovering over this object.
    * 
    * 
    **/
    void OnMouseExit()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_EXIT) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseExitThisObject };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn OnMouseOver
    * \brief Detects when the mouse is hovering over this object.
    * 
    * 
    **/
    void OnMouseOver()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_STAY) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseOverThisObject };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn OnMouseDown
    * \brief Detects when the mouse has clicked this object.
    * 
    * 
    **/
    void OnMouseDown()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_DOWN) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseClickThisObject };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn OnMouseUp
    * \brief Detects when the mouse has released anywhere.
    * 
    * 
    **/
    void OnMouseUp()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_UP) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseAnywhere };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn OnMouseUp
    * \brief Detects when the mouse has released over the same object it clicked on.
    * 
    * 
    **/
    void OnMouseUpAsButton()
    {
        if ((m_eMouseCheck & MouseCheckType.MOUSE_UP_BUTTON) == 0)
            return;

        LPK_EventReceivers receiver = new LPK_EventReceivers();
        receiver.m_GameObjectList = new GameObject[]{ gameObject };

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, receiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_MouseEventTrigger = new LPK_EventList.LPK_MOUSE_EVENTS[] { LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseThisObject };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }

    /**
    * \fn InitializeEvent
    * \brief Set up a function to respond to a certain event.
    * \param eventTrigger  - Event for the function to respond to.
    *                eventFunction - Function to hook up to respond to a certain event.
    * 
    **/
    protected void InitializeEvent(LPK_EventList eventTrigger, LPK_EventManager.LPK_EventType eventFunction,
                                   bool displayWarnings = true /* Used to inform user they are connecting to a event not intended for gameplay. */)
    {        
        //Initialize all collision events.
        for (int i = 0; i < eventTrigger.m_CollisionEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_CollisionEventTrigger[i])
            {
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionEnter:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_ENTER;
                    LPK_EventManager.OnLPK_CollisionEnter += eventFunction;
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionStay:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_STAY;
                    LPK_EventManager.OnLPK_CollisionStay += eventFunction;
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionExit:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_EXIT;
                    LPK_EventManager.OnLPK_CollisionExit += eventFunction;
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerEnter:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_TRIGGER_ENTER;
                    LPK_EventManager.OnLPK_TriggerEnter += eventFunction;
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerStay:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_TRIGGER_STAY;
                    LPK_EventManager.OnLPK_TriggerStay += eventFunction;
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerExit:
                    m_eCollisionCheck |= CollisionCheckType.COLLISION_TRIGGER_EXIT;
                    LPK_EventManager.OnLPK_TriggerExit += eventFunction;
                    break;
            }
        }

        //Initialize all visibility events.
        for (int i = 0; i < eventTrigger.m_VisibilityEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_VisibilityEventTrigger[i])
            {
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreen:
                    m_eVisibilityCheck |= VisibilityCheckType.VISIBLITY_ENTER_SCREEN;
                    LPK_EventManager.OnLPK_VisibilityEnterScreen += eventFunction;
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreen:
                    m_eVisibilityCheck |= VisibilityCheckType.VISIBLITY_EXIT_SCREEN;
                    LPK_EventManager.OnLPK_VisibilityExitScreen += eventFunction;
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreenPersist:
                    m_eVisibilityCheck |= VisibilityCheckType.VISIBLITY_PERSIST_ON_SCREEN;
                    LPK_EventManager.OnLPK_VisibilityPersistOnScreen += eventFunction;
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreenPersist:
                    m_eVisibilityCheck |= VisibilityCheckType.VISIBLITY_PERSIST_OFF_SCREEN;
                    LPK_EventManager.OnLPK_VisibilityPersistOffScreen += eventFunction;
                    break;
            }
        }

        //Initialize all mouse events.
        for (int i = 0; i < eventTrigger.m_MouseEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_MouseEventTrigger[i])
            {
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseEnterThisObject:
                    m_eMouseCheck |= MouseCheckType.MOUSE_ENTER;
                    LPK_EventManager.OnLPK_MouseEnter += eventFunction;
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseExitThisObject:
                    m_eMouseCheck |= MouseCheckType.MOUSE_EXIT;
                    LPK_EventManager.OnLPK_MouseExit += eventFunction;
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseOverThisObject:
                    m_eMouseCheck |= MouseCheckType.MOUSE_STAY;
                    LPK_EventManager.OnLPK_MouseStay += eventFunction;
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseClickThisObject:
                    m_eMouseCheck |= MouseCheckType.MOUSE_DOWN;
                    LPK_EventManager.OnLPK_MouseDown += eventFunction;
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseAnywhere:
                    m_eMouseCheck |= MouseCheckType.MOUSE_UP_BUTTON;
                    LPK_EventManager.OnLPK_MouseUp += eventFunction;
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseThisObject:
                    m_eMouseCheck |= MouseCheckType.MOUSE_UP;
                    LPK_EventManager.OnLPK_MouseUpButton += eventFunction;
                    break;
            }
        }

        //Initialize all input events.
        for (int i = 0; i < eventTrigger.m_InputEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_InputEventTrigger[i])
            {
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_ButtonInput:
                    LPK_EventManager.OnLPK_ButtonInput += eventFunction;
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_KeyboardInput:
                    LPK_EventManager.OnLPK_KeyboardInput += eventFunction;
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_MouseInput:
                    LPK_EventManager.OnLPK_MouseInput += eventFunction;
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_GamepadInput:
                    LPK_EventManager.OnLPK_GamepadInput += eventFunction;
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStart:
                    LPK_EventManager.OnLPK_VibrationStart += eventFunction;
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStop:
                    LPK_EventManager.OnLPK_VibrationStop += eventFunction;
                    break;
            }
        }

        //Initialize all camera events.
        for (int i = 0; i < eventTrigger.m_CameraEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_CameraEventTrigger[i])
            {
                case LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectAdd:
                    LPK_EventManager.OnLPK_TrackingCameraObjectAdd += eventFunction;
                    break;
                case LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectRemove:
                    LPK_EventManager.OnLPK_TrackingCameraObjectRemove += eventFunction;
                    break;
            }
        }

        //Initialize all character events.
        for (int i = 0; i < eventTrigger.m_CharacterEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_CharacterEventTrigger[i])
            {
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_HealthModified:
                    LPK_EventManager.OnLPK_HealthModified += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Damaged:
                    LPK_EventManager.OnLPK_Damaged += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Healed:
                    LPK_EventManager.OnLPK_Healed += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Death:
                    LPK_EventManager.OnLPK_Death += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_OutOfLives:
                    LPK_EventManager.OnLPK_OutOfLives += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterJump:
                    LPK_EventManager.OnLPK_CharacterJump += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterLand:
                    LPK_EventManager.OnLPK_CharacterLand += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_AllyCollision:
                    LPK_EventManager.OnLPK_AllyCollision += eventFunction;
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_EnemyCollision:
                    LPK_EventManager.OnLPK_EnemyCollision += eventFunction;
                    break;
            }
        }

        //Initialize all AI events.
        for (int i = 0; i < eventTrigger.m_AIEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_AIEventTrigger[i])
            {
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachNode:
                    LPK_EventManager.OnLPK_PathFollowerReachNode += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachFinalNode:
                    LPK_EventManager.OnLPK_PathFollowerReachFinalNode += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerFindEnemy:
                    LPK_EventManager.OnLPK_PathFollowerFindEnemy += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerLostEnemy:
                    LPK_EventManager.OnLPK_PathFollowerLostEnemy += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightEstablished:
                    LPK_EventManager.OnLPK_LineOfSightEstablished += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightMaintained:
                    LPK_EventManager.OnLPK_LineOfSightMaintained += eventFunction;
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightLost:
                    LPK_EventManager.OnLPK_LineOfSightLost += eventFunction;
                    break;
            }
        }

        //Initialize all gameplay events.
        for (int i = 0; i < eventTrigger.m_GameplayEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_GameplayEventTrigger[i])
            {
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GameObjectDestroy:
                    LPK_EventManager.OnLPK_GameObjectDesroyed += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_ObjectSpawned:
                    LPK_EventManager.OnLPK_ObjectSpawned += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TimerCompleted:
                    LPK_EventManager.OnLPK_TimerCompleted += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate:
                    LPK_EventManager.OnLPK_DisplayUpdate += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterModify:
                    LPK_EventManager.OnLPK_CounterModify += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterIncrease:
                    LPK_EventManager.OnLPK_CounterIncrease += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterDecrease:
                    LPK_EventManager.OnLPK_CounterDecrease += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterThreshold:
                    LPK_EventManager.OnLPK_CounterThresholdChange += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Attached:
                    LPK_EventManager.OnLPK_Attached += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Detached:
                    LPK_EventManager.OnLPK_Detached += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GradientAnimationFinished:
                    LPK_EventManager.OnLPK_GradientAnimationFinished += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorKeyframeFinished:
                    LPK_EventManager.OnLPK_TransformAnimatorKeyframeFinished += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorSequenceFinished:
                    LPK_EventManager.OnLPK_TransformAnimatorSequenceFinished += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate:
                    LPK_EventManager.OnLPK_TypingTextUpdate += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextComplete:
                    LPK_EventManager.OnLPK_TypingTextCompleted += eventFunction;
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_AnimationCycleFinished:
                    LPK_EventManager.OnLPK_AnimationCycleFinished += eventFunction;
                    break;
            }
        }

        //Initialize all pause menu events.
        for (int i = 0; i < eventTrigger.m_PauseEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_PauseEventTrigger[i])
            {
                case LPK_EventList.LPK_PAUSE_EVENTS.LPK_GamePaused:
                    if (displayWarnings)
                        LPK_PrintWarning(this, "You should not connect objects to the Game Paused event.");
                    LPK_EventManager.OnLPK_GamePaused += eventFunction;
                    break;
                case LPK_EventList.LPK_PAUSE_EVENTS.LPK_GameUnpaused:
                    if (displayWarnings)
                        LPK_PrintWarning(this, "You should not connect objects to the Game Unpaused event.");
                    LPK_EventManager.OnLPK_GameUnpaused += eventFunction;
                    break;
            }
        }

        //Initialize all option mangager events.
        for (int i = 0; i < eventTrigger.m_OptionManagerEventTrigger.Length; i++)
        {
            switch (eventTrigger.m_OptionManagerEventTrigger[i])
            {

                case LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_AudioLevelsAdjusted:
                    if (displayWarnings)
                        LPK_PrintWarning(this, "You should not connect objects to the Audio Levels Adjusted event.");
                    LPK_EventManager.OnLPK_AudioLevelsAdjusted += eventFunction;
                    break;
                case LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_DifficultyLevelAdjusted:
                    if (displayWarnings)
                        LPK_PrintWarning(this, "You should not connect objects to the Difficulty Levels Adjusted event.");
                    LPK_EventManager.OnLPK_DifficultyLevelAdjusted += eventFunction;
                    break;
            }
        }
    }

    /**
    * \fn ShouldRespondToEvent
    * \brief Determine whether or not to respond to an event.
    * \param data - Event data to parse for recivers.
    * \return bool - True or false whether or not to respond to event.
    **/
    protected bool ShouldRespondToEvent(LPK_EventManager.LPK_EventData data)
    {
        //NOTENOTE: For some reason this is needed at least for mouse events.
        if (!this.isActiveAndEnabled)
            return false;

        //Do nothing if paused.
        if (m_bGamePaused)
            return false;

        bool noObjectsSet = data.m_pReceiver.m_GameObjectList.Length == 0;
        bool noTagsSet = data.m_pReceiver.m_Tags.Length == 0;
        bool validInput = IsInputValid(data);
        bool validCollision = CheckValidCollision(data.m_pSender);

        //No lengths means broadcast to all.
        if (noObjectsSet && noTagsSet && validInput)
            return true;

        bool allNullObjects = true;
        bool allNullTags = true;

        //If the game object marked as the receiver is the game object this component is on, accept no matter what.
        for (int i = 0; i < data.m_pReceiver.m_GameObjectList.Length; i++)
        {
            if (data.m_pReceiver.m_GameObjectList[i] == gameObject)
                return true && validInput && validCollision;
            else if (data.m_pReceiver.m_GameObjectList[i] != null)
                allNullObjects = false;     //NOTENOTE: Do not break as there could be only 1 null object in the middle of the array.
        }

        //If the tag marked as the receiver is on the game object this component is on, accept no matter what.
        for (int i = 0; i < data.m_pReceiver.m_Tags.Length; i++)
        {
            if (!string.IsNullOrEmpty(data.m_pReceiver.m_Tags[i]) && data.m_pReceiver.m_Tags[i] == gameObject.tag)
                return true && validInput && validCollision;
            else if (!string.IsNullOrEmpty(data.m_pReceiver.m_Tags[i]))
                allNullTags = false;    //NOTENOTE: Do not break as there could be only 1 null string in the middle of the array.
        }

        //If everything was null and the sender is this game object, respod.
        if (allNullTags && allNullObjects && data.m_pSender == gameObject && validInput && validCollision)
            return true;

        return false;
    }

    /**
    * \fn IsInputValid
    * \brief Determine whether or not to respond to an input event based on pressed data.
    * \param data - Event data to parse input on.
    * \return bool - True or false whether or not to respond to event.
    **/
    bool IsInputValid(LPK_EventManager.LPK_EventData data)
    {
        bool returnVal = true;

        //There are buttons to detect input on.
        if (!string.IsNullOrEmpty(data.m_sPressedButton) && m_InputEventInfo.m_SpecifiedVirtualButtons.Length > 0)
        {
            for (int i = 0; i < m_InputEventInfo.m_SpecifiedVirtualButtons.Length; i++)
            {
                if (m_InputEventInfo.m_SpecifiedVirtualButtons[i] == data.m_sPressedButton)
                    returnVal = true;
                else
                    returnVal = false;
            }
        }

        //There are keys to detect input on.
        if (data.m_PressedKey != KeyCode.None && m_InputEventInfo.m_SpecifiedKeys.Length > 0)
        {
            for (int i = 0; i < m_InputEventInfo.m_SpecifiedKeys.Length; i++)
            {
                if (m_InputEventInfo.m_SpecifiedKeys[i] == data.m_PressedKey)
                    returnVal = true;
                else
                    returnVal = false;
            }
        }

        //There are mouse buttons to detect input on.
        else if (data.m_PressedMouseButton != LPK_MouseButtons.ANY && m_InputEventInfo.m_SpecifiedMouseButtons.Length > 0)
        {
            for (int i = 0; i < m_InputEventInfo.m_SpecifiedMouseButtons.Length; i++)
            {
                if (m_InputEventInfo.m_SpecifiedMouseButtons[i] == data.m_PressedMouseButton)
                    returnVal = true;
                else
                    returnVal = false;
            }
        }

        //There are gamepad buttons to detect input on.
        else if (data.m_PressedGamepadButton != LPK_ControllerButtons.ANY && m_InputEventInfo.m_SpecifiedGamepadButtons.Length > 0)
        {
            for (int i = 0; i < m_InputEventInfo.m_SpecifiedGamepadButtons.Length; i++)
            {
                if (m_InputEventInfo.m_SpecifiedGamepadButtons[i] == data.m_PressedGamepadButton)
                    returnVal = true;
                else
                    returnVal = false;
            }
        }

        //There are gamepad numbers to detect input on.
        if (data.m_PressedGamepadNumber != LPK_ControllerNumber.ANY && m_InputEventInfo.m_SpecifiedGamepadNumbers.Length > 0 && returnVal)
        {
            for (int i = 0; i < m_InputEventInfo.m_SpecifiedGamepadNumbers.Length; i++)
            {
                if (m_InputEventInfo.m_SpecifiedGamepadNumbers[i] == data.m_PressedGamepadNumber)
                    returnVal = true;
                else
                    returnVal = false;
            }
        }

        return returnVal;
    }

    /**
    * \fn LPK_PrintDebug
    * \brief Prints a formatted debug message.
    * \param caller  - Unity object (component) that invoked this function.
    *                message - Additional message to print along with caller data.
    * 
    **/
    protected void LPK_PrintDebug(Object caller, string message)
    {
        Debug.Log(message + " | " + caller);
    }

    /**
    * \fn LPK_PrintWarning
    * \brief Prints a formatted debug warning.
    * \param caller  - Unity object (component) that invoked this function.
    *                message - Additional message to print along with caller data.
    * 
    **/
    protected void LPK_PrintWarning(Object caller, string message)
    {
        Debug.LogWarning(message + " | " + caller);
    }

    /**
    * \fn LPK_PrintError
    * \brief Prints a formatted debug error.
    * \param caller  - Unity object (component) that invoked this function.
    *                message - Additional message to print along with caller data.
    * 
    **/
    protected void LPK_PrintError(Object caller, string message)
    {
        Debug.LogError(message + " | " + caller);
    }

    /**
    * \fn OnEvent
    * \brief Empty function overriden to handle script action implementation.
    *                Detaching this function from the event system will be done by
    *                default.  Any other functions the user makes in their own components
    *                to hook up with the event system must be removed manually through the
    *                class's OnDestroyed function (not to be confused with OnDestroy).
    * 
    * 
    **/
    protected virtual void OnEvent( LPK_EventManager.LPK_EventData data)
    {
        //Implemented in each indivitual script.
    }

    /**
    * \fn OnDestroy
    * \brief Handle object destruction.
    * 
    * 
    **/
    void OnDestroy()
    {
        LPK_DebugStatistics.RemoveObjectCount(this);

        //Detach linked function.
        DetachFunction(OnEvent);

        //Pause functions.
        LPK_EventManager.OnLPK_GamePaused -= OnPauseEvent;
        LPK_EventManager.OnLPK_GameUnpaused -= OnUnpauseEvent;

        //Call to children.
        OnDestroyed();
    }

    /**
    * \fn OnDestroyed
    * \brief Implemented by child classes.
    * 
    * 
    **/
    protected virtual void OnDestroyed()
    {
        //Implemented by child classes.
    }

    /**
    * \fn DetachFunction
    * \brief Detach a function from every possible handler.
    * \param eventFunction - Function to detach.
    * 
    **/
    protected void DetachFunction(LPK_EventManager.LPK_EventType eventFunction)
    {
        //Colllision events.
        LPK_EventManager.OnLPK_CollisionEnter -= eventFunction;
        LPK_EventManager.OnLPK_CollisionStay -= eventFunction;
        LPK_EventManager.OnLPK_CollisionExit -= eventFunction;
        LPK_EventManager.OnLPK_TriggerEnter -= eventFunction;
        LPK_EventManager.OnLPK_TriggerStay -= eventFunction;
        LPK_EventManager.OnLPK_TriggerExit -= eventFunction;

        //Visibility events.
        LPK_EventManager.OnLPK_VisibilityEnterScreen -= eventFunction;
        LPK_EventManager.OnLPK_VisibilityPersistOnScreen -= eventFunction;
        LPK_EventManager.OnLPK_VisibilityExitScreen -= eventFunction;
        LPK_EventManager.OnLPK_VisibilityPersistOffScreen -= eventFunction;

        //Mouse events.
        LPK_EventManager.OnLPK_MouseEnter -= eventFunction;
        LPK_EventManager.OnLPK_MouseExit -= eventFunction;
        LPK_EventManager.OnLPK_MouseStay -= eventFunction;
        LPK_EventManager.OnLPK_MouseDown -= eventFunction;
        LPK_EventManager.OnLPK_MouseUp -= eventFunction;
        LPK_EventManager.OnLPK_MouseUpButton -= eventFunction;

        LPK_EventManager.OnLPK_GameObjectDesroyed -= eventFunction;
        LPK_EventManager.OnLPK_AllyCollision -= eventFunction;
        LPK_EventManager.OnLPK_EnemyCollision -= eventFunction;
        LPK_EventManager.OnLPK_ObjectSpawned -= eventFunction;
        LPK_EventManager.OnLPK_TimerCompleted -= eventFunction;
        LPK_EventManager.OnLPK_DisplayUpdate -= eventFunction;
        LPK_EventManager.OnLPK_ButtonInput -= eventFunction;
        LPK_EventManager.OnLPK_KeyboardInput -= eventFunction;
        LPK_EventManager.OnLPK_MouseInput -= eventFunction;
        LPK_EventManager.OnLPK_GamepadInput -= eventFunction;
        LPK_EventManager.OnLPK_VibrationStart -= eventFunction;
        LPK_EventManager.OnLPK_VibrationStop -= eventFunction;
        LPK_EventManager.OnLPK_HealthModified -= eventFunction;
        LPK_EventManager.OnLPK_Damaged -= eventFunction;
        LPK_EventManager.OnLPK_Healed -= eventFunction;
        LPK_EventManager.OnLPK_Death -= eventFunction;
        LPK_EventManager.OnLPK_OutOfLives -= eventFunction;
        LPK_EventManager.OnLPK_CounterModify -= eventFunction;
        LPK_EventManager.OnLPK_CounterIncrease -= eventFunction;
        LPK_EventManager.OnLPK_CounterDecrease -= eventFunction;
        LPK_EventManager.OnLPK_CounterThresholdChange -= eventFunction;
        LPK_EventManager.OnLPK_CharacterJump -= eventFunction;
        LPK_EventManager.OnLPK_CharacterLand -= eventFunction;
        LPK_EventManager.OnLPK_Attached -= eventFunction;
        LPK_EventManager.OnLPK_Detached -= eventFunction;
        LPK_EventManager.OnLPK_TrackingCameraObjectAdd -= eventFunction;
        LPK_EventManager.OnLPK_TrackingCameraObjectRemove -= eventFunction;
        LPK_EventManager.OnLPK_TypingTextUpdate -= eventFunction;
        LPK_EventManager.OnLPK_TypingTextCompleted -= eventFunction;
        LPK_EventManager.OnLPK_PathFollowerReachNode -= eventFunction;
        LPK_EventManager.OnLPK_PathFollowerReachFinalNode -= eventFunction;
        LPK_EventManager.OnLPK_PathFollowerFindEnemy -= eventFunction;
        LPK_EventManager.OnLPK_PathFollowerLostEnemy -= eventFunction;
        LPK_EventManager.OnLPK_TransformAnimatorKeyframeFinished -= eventFunction;
        LPK_EventManager.OnLPK_TransformAnimatorSequenceFinished -= eventFunction;
        LPK_EventManager.OnLPK_LineOfSightEstablished -= eventFunction;
        LPK_EventManager.OnLPK_LineOfSightMaintained -= eventFunction;
        LPK_EventManager.OnLPK_LineOfSightLost -= eventFunction;
        LPK_EventManager.OnLPK_GradientAnimationFinished -= eventFunction;
        LPK_EventManager.OnLPK_AnimationCycleFinished -= eventFunction;

        //Pause menu evnets.

        LPK_EventManager.OnLPK_GamePaused -= eventFunction;
        LPK_EventManager.OnLPK_GameUnpaused -= eventFunction;

        LPK_EventManager.OnLPK_AudioLevelsAdjusted -= eventFunction;
        LPK_EventManager.OnLPK_DifficultyLevelAdjusted -= eventFunction;
    }
}

/**
* \class LPK_EventList
* \brief Formated event list for view in the Unity Editor.
**/
[System.Serializable]
public class LPK_EventList
{
    public enum LPK_INPUT_EVENTS
    {
        LPK_ButtonInput,
        LPK_KeyboardInput,
        LPK_MouseInput,
        LPK_GamepadInput,
        LPK_VibrationStart,
        LPK_VibrationStop,
    };

    public enum LPK_MOUSE_EVENTS
    {
        LPK_MouseEnterThisObject,
        LPK_MouseExitThisObject,
        LPK_MouseOverThisObject,
        LPK_MouseClickThisObject,
        LPK_MouseReleaseThisObject,
        LPK_MouseReleaseAnywhere,
    };

    public enum LPK_COLLISION_EVENTS
    {
        LPK_CollisionEnter,
        LPK_CollisionExit,
        LPK_CollisionStay,
        LPK_TriggerEnter,
        LPK_TriggerExit,
        LPK_TriggerStay,
    };

    public enum LPK_VISIBILITY_EVENTS
    {
        LPK_VisibilityEnterScreen,
        LPK_VisibilityExitScreen,
        LPK_VisibilityEnterScreenPersist,
        LPK_VisibilityExitScreenPersist,
    };

    public enum LPK_CAMERA_EVENTS
    {
        LPK_TrackingCameraObjectAdd,     //NOTENOTE: Sender and reviever are always the gameobject the component is on.
        LPK_TrackingCameraObjectRemove,  //NOTENOTE: Sender and reviever are always the gameobject the component is on.
    };

    public enum LPK_CHARACTER_EVENTS
    {
        LPK_CharacterJump,
        LPK_CharacterLand,
        LPK_AllyCollision,
        LPK_EnemyCollision,
        LPK_HealthModified,
        LPK_Damaged,
        LPK_Healed,
        LPK_Death,
        LPK_OutOfLives,
    };

    public enum LPK_AI_EVENTS
    {
        LPK_PathFollowerReachNode,
        LPK_PathFollowerReachFinalNode,
        LPK_PathFollowerLostEnemy,
        LPK_PathFollowerFindEnemy,
        LPK_LineOfSightEstablished,
        LPK_LineOfSightMaintained,
        LPK_LineOfSightLost,
    };

    public enum LPK_GAMEPLAY_EVENTS
    {
        LPK_Attached,
        LPK_Detached,
        LPK_GameObjectDestroy,
        LPK_GradientAnimationFinished,
        LPK_ObjectSpawned,
        LPK_TimerCompleted,
        LPK_DisplayUpdate,
        LPK_CounterModify,
        LPK_CounterIncrease,
        LPK_CounterDecrease,
        LPK_CounterThreshold,
        LPK_TypingTextUpdate,
        LPK_TypingTextComplete,
        LPK_TransformAnimatorKeyframeFinished,
        LPK_TransformAnimatorSequenceFinished,
        LPK_AnimationCycleFinished,
    };

    public enum LPK_PAUSE_EVENTS
    {
        LPK_GamePaused,
        LPK_GameUnpaused,
    };

    public enum LPK_OPTION_MANAGER_EVENTS
    {
        LPK_AudioLevelsAdjusted,
        LPK_DifficultyLevelAdjusted,
    };

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_INPUT_EVENTS[] m_InputEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_MOUSE_EVENTS[] m_MouseEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_COLLISION_EVENTS[] m_CollisionEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    [Rename("Visibility Event Triggers")]
    public LPK_VISIBILITY_EVENTS[] m_VisibilityEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_CAMERA_EVENTS[] m_CameraEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_CHARACTER_EVENTS[] m_CharacterEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_AI_EVENTS[] m_AIEventTrigger;

    [Tooltip("Which event will trigger this component's action.")]
    public LPK_GAMEPLAY_EVENTS[] m_GameplayEventTrigger;

    [Tooltip("Which event will trigger this component's action.  This should not be used for most gameplay.")]
    public LPK_PAUSE_EVENTS[] m_PauseEventTrigger;

    [Tooltip("Which event will trigger this component's action.  This should not be used for most gameplay.")]
    public LPK_OPTION_MANAGER_EVENTS[] m_OptionManagerEventTrigger;

    /**
    * \fn Constructor
    * \brief Initialize all arrays to not be null.
    * 
    * 
    **/
    public LPK_EventList()
    {
        m_InputEventTrigger = new LPK_INPUT_EVENTS[] { };
        m_MouseEventTrigger = new LPK_MOUSE_EVENTS[] { };
        m_CollisionEventTrigger = new LPK_COLLISION_EVENTS[] { };
        m_VisibilityEventTrigger = new LPK_VISIBILITY_EVENTS[] { };
        m_CameraEventTrigger = new LPK_CAMERA_EVENTS[] { };
        m_CharacterEventTrigger = new LPK_CHARACTER_EVENTS[] { };
        m_AIEventTrigger = new LPK_AI_EVENTS[] { };
        m_GameplayEventTrigger = new LPK_GAMEPLAY_EVENTS[] { };
        m_PauseEventTrigger = new LPK_PAUSE_EVENTS[] { };
        m_OptionManagerEventTrigger = new LPK_OPTION_MANAGER_EVENTS[] { };
    }
}

/**
* \class LPK_EventReceivers
* \brief Serialized class to specify search targets for event dispatching.
**/
[System.Serializable]
public class LPK_EventReceivers
{
    [Tooltip("Game objects that will receive the event.  If this is set to only contain NULL objects, only the game object this component is on will receive the event.  If this array's length is set to zero alongside the tag array, all game objects will receive the event.  Note this is an OR search with the tag specifier array.")]
    public GameObject[] m_GameObjectList;

    [Tooltip("Tags that will receive the event.  If this array's length is set to zero alongside the game object array, all game objects will receive the event.  Note this is an OR search with the game object specifier array.")]
    [TagDropdown]
    public string[] m_Tags;

    /**
    * \fn Constructor
    * \brief Constructor for array initialization.
    * 
    * 
    **/
    public LPK_EventReceivers()
    {
        //By default only send to yourself.
        m_GameObjectList = new GameObject[] { null };
        m_Tags = new string[] { };
    }
}


/**
* \class LPK_EventManager
* \brief Messaging system implementation.
**/
public static class LPK_EventManager
{
    /************************************************************************************/
    /*********************************EVENT DATA STRUCT**********************************/
    /************************************************************************************/

    public struct LPK_EventData
    {
        /**
        * \fn Constructor
        * \brief Constructor for data paramater only taking sender and
        *                receiver information.
        * \param sender   - Game Object sending the event.
        *                receiver - Game Object receiving the event.
        * 
        **/
        public LPK_EventData(GameObject sender = null, LPK_EventReceivers receiver = null)
        {
            m_pReceiver = receiver;
            m_pSender = sender;

            m_idata = new List<int>();
            m_bData = new List<bool>();
            m_flData = new List<float>();
            m_dData = new List<double>();
            m_sData = new List<string>();
            m_vecData = new List<Vector3>();

            //Input event validation.
            m_sPressedButton = "";
            m_PressedKey = KeyCode.None;
            m_PressedMouseButton = LPK_LogicBase.LPK_MouseButtons.ANY;
            m_PressedGamepadButton = LPK_LogicBase.LPK_ControllerButtons.ANY;
            m_PressedGamepadNumber = LPK_LogicBase.LPK_ControllerNumber.ANY;
    }

        public LPK_EventReceivers m_pReceiver;
        public GameObject m_pSender;

        //NOTENOTE: Lists so you can cram a bunch of data in if necessary.  You can add more lists as needed.

        public List<int> m_idata;
        public List<bool> m_bData;
        public List<float> m_flData;
        public List<double> m_dData;
        public List<string> m_sData;
        public List<Vector3> m_vecData;

        //Used for input event validation.
        public string m_sPressedButton;
        public KeyCode m_PressedKey;
        public LPK_LogicBase.LPK_MouseButtons m_PressedMouseButton;
        public LPK_LogicBase.LPK_ControllerButtons m_PressedGamepadButton;
        public LPK_LogicBase.LPK_ControllerNumber m_PressedGamepadNumber;
    };

    /************************************************************************************/
    /********************************EVENT DECLERATIONS**********************************/
    /************************************************************************************/

    public delegate void LPK_EventType(LPK_EventData data);

    //Colllision events.
    public static event LPK_EventType OnLPK_CollisionEnter;
    public static event LPK_EventType OnLPK_CollisionStay;
    public static event LPK_EventType OnLPK_CollisionExit;
    public static event LPK_EventType OnLPK_TriggerEnter;
    public static event LPK_EventType OnLPK_TriggerStay;
    public static event LPK_EventType OnLPK_TriggerExit;

    //Visibility events.
    public static event LPK_EventType OnLPK_VisibilityEnterScreen;
    public static event LPK_EventType OnLPK_VisibilityPersistOnScreen;
    public static event LPK_EventType OnLPK_VisibilityExitScreen;
    public static event LPK_EventType OnLPK_VisibilityPersistOffScreen;

    //Mouse events.
    public static event LPK_EventType OnLPK_MouseEnter;
    public static event LPK_EventType OnLPK_MouseExit;
    public static event LPK_EventType OnLPK_MouseStay;
    public static event LPK_EventType OnLPK_MouseDown;
    public static event LPK_EventType OnLPK_MouseUp;
    public static event LPK_EventType OnLPK_MouseUpButton;

    public static event LPK_EventType OnLPK_GameObjectDesroyed;
    public static event LPK_EventType OnLPK_AllyCollision;
    public static event LPK_EventType OnLPK_EnemyCollision;
    public static event LPK_EventType OnLPK_ObjectSpawned;
    public static event LPK_EventType OnLPK_TimerCompleted;
    public static event LPK_EventType OnLPK_DisplayUpdate;
    public static event LPK_EventType OnLPK_ButtonInput;
    public static event LPK_EventType OnLPK_KeyboardInput;
    public static event LPK_EventType OnLPK_MouseInput;
    public static event LPK_EventType OnLPK_GamepadInput;
    public static event LPK_EventType OnLPK_HealthModified;
    public static event LPK_EventType OnLPK_Damaged;
    public static event LPK_EventType OnLPK_Healed;
    public static event LPK_EventType OnLPK_Death;
    public static event LPK_EventType OnLPK_OutOfLives;
    public static event LPK_EventType OnLPK_CounterModify;
    public static event LPK_EventType OnLPK_CounterIncrease;
    public static event LPK_EventType OnLPK_CounterDecrease;
    public static event LPK_EventType OnLPK_CounterThresholdChange;
    public static event LPK_EventType OnLPK_CharacterJump;
    public static event LPK_EventType OnLPK_CharacterLand;
    public static event LPK_EventType OnLPK_Attached;
    public static event LPK_EventType OnLPK_Detached;
    public static event LPK_EventType OnLPK_GradientAnimationFinished;
    public static event LPK_EventType OnLPK_TrackingCameraObjectAdd;
    public static event LPK_EventType OnLPK_TrackingCameraObjectRemove;
    public static event LPK_EventType OnLPK_TypingTextUpdate;
    public static event LPK_EventType OnLPK_TypingTextCompleted;
    public static event LPK_EventType OnLPK_VibrationStart;
    public static event LPK_EventType OnLPK_VibrationStop;
    public static event LPK_EventType OnLPK_PathFollowerReachNode;
    public static event LPK_EventType OnLPK_PathFollowerReachFinalNode;
    public static event LPK_EventType OnLPK_PathFollowerFindEnemy;
    public static event LPK_EventType OnLPK_PathFollowerLostEnemy;
    public static event LPK_EventType OnLPK_TransformAnimatorKeyframeFinished;
    public static event LPK_EventType OnLPK_TransformAnimatorSequenceFinished;
    public static event LPK_EventType OnLPK_AnimationCycleFinished;
    public static event LPK_EventType OnLPK_LineOfSightEstablished;
    public static event LPK_EventType OnLPK_LineOfSightMaintained;
    public static event LPK_EventType OnLPK_LineOfSightLost;

    //Managers
    public static event LPK_EventType OnLPK_AudioLevelsAdjusted;
    public static event LPK_EventType OnLPK_DifficultyLevelAdjusted;

    //Pause functions.
    public static event LPK_EventType OnLPK_GamePaused;
    public static event LPK_EventType OnLPK_GameUnpaused;

    /**
    * \fn InvokeEvent
    * \brief Invokes a specified event.
    * \param eventCalled - Which type of event to invoke.
    *                data        - Optional event data to be passed through.
    * 
    **/
    public static void InvokeEvent(LPK_EventList eventCalled, LPK_EventData data)
    {
        //Input event invoking.
        for (int i = 0; i < eventCalled.m_InputEventTrigger.Length; i++)
        {
            switch (eventCalled.m_InputEventTrigger[i])
            {
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_ButtonInput:
                    if (OnLPK_ButtonInput != null)
                        OnLPK_ButtonInput(data);
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_KeyboardInput:
                    if (OnLPK_KeyboardInput != null)
                        OnLPK_KeyboardInput(data);
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_MouseInput:
                    if (OnLPK_MouseInput != null)
                        OnLPK_MouseInput(data);
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_GamepadInput:
                    if (OnLPK_GamepadInput != null)
                        OnLPK_GamepadInput(data);
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStart:
                    if (OnLPK_VibrationStart != null)
                        OnLPK_VibrationStart(data);
                    break;
                case LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStop:
                    if (OnLPK_VibrationStop != null)
                        OnLPK_VibrationStop(data);
                    break;
            }
        }

        //Mouse event invoking.
        for (int i = 0; i < eventCalled.m_MouseEventTrigger.Length; i++)
        {
            switch (eventCalled.m_MouseEventTrigger[i])
            {
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseEnterThisObject:
                    if (OnLPK_MouseEnter != null)
                        OnLPK_MouseEnter(data);
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseExitThisObject:
                    if (OnLPK_MouseExit != null)
                        OnLPK_MouseExit(data);
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseOverThisObject:
                    if (OnLPK_MouseStay != null)
                        OnLPK_MouseStay(data);
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseClickThisObject:
                    if (OnLPK_MouseDown != null)
                        OnLPK_MouseDown(data);
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseThisObject:
                    if (OnLPK_MouseUpButton != null)
                        OnLPK_MouseUpButton(data);
                    break;
                case LPK_EventList.LPK_MOUSE_EVENTS.LPK_MouseReleaseAnywhere:
                    if (OnLPK_MouseUp != null)
                        OnLPK_MouseUp(data);
                    break;
            }
        }

        //Collision event invoking.
        for (int i = 0; i < eventCalled.m_CollisionEventTrigger.Length; i++)
        {
            switch (eventCalled.m_CollisionEventTrigger[i])
            {
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionEnter:
                    if (OnLPK_CollisionEnter != null)
                        OnLPK_CollisionEnter(data);
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionStay:
                    if (OnLPK_CollisionStay != null)
                        OnLPK_CollisionStay(data);
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_CollisionExit:
                    if (OnLPK_CollisionExit != null)
                        OnLPK_CollisionExit(data);
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerEnter:
                    if (OnLPK_TriggerEnter != null)
                        OnLPK_TriggerEnter(data);
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerStay:
                    if (OnLPK_TriggerStay != null)
                        OnLPK_TriggerStay(data);
                    break;
                case LPK_EventList.LPK_COLLISION_EVENTS.LPK_TriggerExit:
                    if (OnLPK_TriggerExit != null)
                        OnLPK_TriggerExit(data);
                    break;
            }
        }

        //Visibility event invoking
        for (int i = 0; i < eventCalled.m_VisibilityEventTrigger.Length; i++)
        {
            switch (eventCalled.m_VisibilityEventTrigger[i])
            {
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreen:
                    if (OnLPK_VisibilityEnterScreen != null)
                        OnLPK_VisibilityEnterScreen(data);
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreen:
                    if (OnLPK_VisibilityExitScreen != null)
                        OnLPK_VisibilityExitScreen(data);
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityEnterScreenPersist:
                    if (OnLPK_VisibilityPersistOnScreen != null)
                        OnLPK_VisibilityPersistOnScreen(data);
                    break;
                case LPK_EventList.LPK_VISIBILITY_EVENTS.LPK_VisibilityExitScreenPersist:
                    if (OnLPK_VisibilityPersistOffScreen != null)
                        OnLPK_VisibilityPersistOffScreen(data);
                    break;
            }
        }

        //Camera event invoking.
        for (int i = 0; i < eventCalled.m_CameraEventTrigger.Length; i++)
        {
            switch (eventCalled.m_CameraEventTrigger[i])
            {
                case LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectAdd:
                    if (OnLPK_TrackingCameraObjectAdd != null)
                        OnLPK_TrackingCameraObjectAdd(data);
                    break;
                case LPK_EventList.LPK_CAMERA_EVENTS.LPK_TrackingCameraObjectRemove:
                    if (OnLPK_TrackingCameraObjectRemove != null)
                        OnLPK_TrackingCameraObjectRemove(data);
                    break;
            }
        }

        //Character event invoking.
        for (int i = 0; i < eventCalled.m_CharacterEventTrigger.Length; i++)
        {
            switch (eventCalled.m_CharacterEventTrigger[i])
            {
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_HealthModified:
                    if (OnLPK_HealthModified != null)
                        OnLPK_HealthModified(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Damaged:
                    if (OnLPK_Damaged != null)
                        OnLPK_Damaged(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Healed:
                    if (OnLPK_Healed != null)
                        OnLPK_Healed(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_Death:
                    if (OnLPK_Death != null)
                        OnLPK_Death(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_OutOfLives:
                    if (OnLPK_OutOfLives != null)
                        OnLPK_OutOfLives(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterJump:
                    if (OnLPK_CharacterJump != null)
                        OnLPK_CharacterJump(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterLand:
                    if (OnLPK_CharacterLand != null)
                        OnLPK_CharacterLand(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_AllyCollision:
                    if (OnLPK_AllyCollision != null)
                        OnLPK_AllyCollision(data);
                    break;
                case LPK_EventList.LPK_CHARACTER_EVENTS.LPK_EnemyCollision:
                    if (OnLPK_EnemyCollision != null)
                        OnLPK_EnemyCollision(data);
                    break;
            }
        }

        //AI event invoking.
        for (int i = 0; i < eventCalled.m_AIEventTrigger.Length; i++)
        {
            switch (eventCalled.m_AIEventTrigger[i])
            {
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachNode:
                    if (OnLPK_PathFollowerReachNode != null)
                        OnLPK_PathFollowerReachNode(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerReachFinalNode:
                    if (OnLPK_PathFollowerReachFinalNode != null)
                        OnLPK_PathFollowerReachFinalNode(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerFindEnemy:
                    if (OnLPK_PathFollowerFindEnemy != null)
                        OnLPK_PathFollowerFindEnemy(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_PathFollowerLostEnemy:
                    if (OnLPK_PathFollowerLostEnemy != null)
                        OnLPK_PathFollowerLostEnemy(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightEstablished:
                    if (OnLPK_LineOfSightEstablished != null)
                        OnLPK_LineOfSightEstablished(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightMaintained:
                    if (OnLPK_LineOfSightMaintained != null)
                        OnLPK_LineOfSightMaintained(data);
                    break;
                case LPK_EventList.LPK_AI_EVENTS.LPK_LineOfSightLost:
                    if (OnLPK_LineOfSightLost != null)
                        OnLPK_LineOfSightLost(data);
                    break;
            }
        }

        //Gameplay event invoking.
        for (int i = 0; i < eventCalled.m_GameplayEventTrigger.Length; i++)
        {
            switch (eventCalled.m_GameplayEventTrigger[i])
            {
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GameObjectDestroy:
                    if (OnLPK_GameObjectDesroyed != null)
                        OnLPK_GameObjectDesroyed(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_ObjectSpawned:
                    if (OnLPK_ObjectSpawned != null)
                        OnLPK_ObjectSpawned(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TimerCompleted:
                    if (OnLPK_TimerCompleted != null)
                        OnLPK_TimerCompleted(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_DisplayUpdate:
                    if (OnLPK_DisplayUpdate != null)
                        OnLPK_DisplayUpdate(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterModify:
                    if (OnLPK_CounterModify != null)
                        OnLPK_CounterModify(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterIncrease:
                    if (OnLPK_CounterIncrease != null)
                        OnLPK_CounterIncrease(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterDecrease:
                    if (OnLPK_CounterDecrease != null)
                        OnLPK_CounterDecrease(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_CounterThreshold:
                    if (OnLPK_CounterThresholdChange != null)
                        OnLPK_CounterThresholdChange(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Attached:
                    if (OnLPK_Attached != null)
                        OnLPK_Attached(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_Detached:
                    if (OnLPK_Detached != null)
                        OnLPK_Detached(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorKeyframeFinished:
                    if (OnLPK_TransformAnimatorKeyframeFinished != null)
                        OnLPK_TransformAnimatorKeyframeFinished(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorSequenceFinished:
                    if (OnLPK_TransformAnimatorSequenceFinished != null)
                        OnLPK_TransformAnimatorSequenceFinished(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate:
                    if (OnLPK_TypingTextUpdate != null)
                        OnLPK_TypingTextUpdate(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextComplete:
                    if (OnLPK_TypingTextCompleted != null)
                        OnLPK_TypingTextCompleted(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GradientAnimationFinished:
                    if (OnLPK_GradientAnimationFinished != null)
                        OnLPK_GradientAnimationFinished(data);
                    break;
                case LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_AnimationCycleFinished:
                    if (OnLPK_AnimationCycleFinished != null)
                        OnLPK_AnimationCycleFinished(data);
                    break;
            }
        }

        //Pause event invoking.
        for (int i = 0; i < eventCalled.m_PauseEventTrigger.Length; i++)
        {
            switch (eventCalled.m_PauseEventTrigger[i])
            {
                case LPK_EventList.LPK_PAUSE_EVENTS.LPK_GamePaused:
                    if (OnLPK_GamePaused != null)
                        OnLPK_GamePaused(data);
                    break;
                case LPK_EventList.LPK_PAUSE_EVENTS.LPK_GameUnpaused:
                    if (OnLPK_GameUnpaused != null)
                        OnLPK_GameUnpaused(data);
                    break;
            }
        }

        //Option manager event invoking.
        for (int i = 0; i < eventCalled.m_OptionManagerEventTrigger.Length; i++)
        {
            switch (eventCalled.m_OptionManagerEventTrigger[i])
            {
                case LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_AudioLevelsAdjusted:
                    if (OnLPK_AudioLevelsAdjusted != null)
                        OnLPK_AudioLevelsAdjusted(data);
                    break;
                case LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_DifficultyLevelAdjusted:
                    if (OnLPK_DifficultyLevelAdjusted != null)
                        OnLPK_DifficultyLevelAdjusted(data);
                    break;
            }
        }
    }
}

/**
* \class LPK_DebugStatistics
* \brief Keeps track of stats regarding LPK usage for perfermance debugging.
*               This class is placed in the Utilities file to prevent against the need
*               to import LPK_DebugObjectStatistics.cs to use the LPK.
**/
static class LPK_DebugStatistics
{
    /************************************************************************************/

    //How many LPK objects are currently in the game.
    static uint m_iObjectCount;

    //How many LPK objects that use Update or OnUpdate are currently in the game.
    static uint m_iUpdatedObjectCount;

    //How many LPK components are currently in the game.
    static uint m_iComponentCount;

    //How many LPK components that make use of an Update function are currently in use.
    static uint m_iUpdatedComponentCount;

    //List of all LPK gameobjects currently added to the list.
    static List<GameObject> m_pObjectList = new List<GameObject>();

    /************************************************************************************/

    public delegate void DebugStatisticsUpdated();
    public static event DebugStatisticsUpdated OnDebugStatisticsUpdated;

    /**
    * \fn AddObjectCount
    * \brief Notifies statistics of a potential new object add.
    * \param _newObject - Object to be counted in the stats.
    * 
    **/
    public static void AddObjectCount(LPK_LogicBase _newObject)
    {
        //Always add to the component count.
        m_iComponentCount++;
        AddUpdatedObjectCount(_newObject);

        if (!m_pObjectList.Contains(_newObject.gameObject))
        {
            m_pObjectList.Add(_newObject.gameObject);
            m_iObjectCount++;
        }

        //Check to add to the updated object list.

        if (OnDebugStatisticsUpdated != null)
            OnDebugStatisticsUpdated();
    }

    /**
    * \fn RemoveObjectCount
    * \brief Notifies statistics of a potential object to remove.
    * \param _removeObject - Object to be removed in the stats.
    * 
    **/
    public static void RemoveObjectCount(LPK_LogicBase _removeObject)
    {
        //Always remove from the component count.
        m_iComponentCount--;
        RemoveUpdatedObjectCount(_removeObject);

        if (m_pObjectList.Contains(_removeObject.gameObject))
        {
            m_pObjectList.Remove(_removeObject.gameObject);
            m_iObjectCount--;
        }

        if (OnDebugStatisticsUpdated != null)
            OnDebugStatisticsUpdated();
    }

    /**
    * \fn AddUpdatedObjectCount
    * \brief Notifies statistics of a potential updated object to add.
    * \param _removeObject - Object to be added in the stats.
    * 
    **/
    static void AddUpdatedObjectCount(LPK_LogicBase _newObject)
    {
        if (_newObject.UsesUpdate())
        {
            m_iUpdatedComponentCount++;

            if (!m_pObjectList.Contains(_newObject.gameObject))
                m_iUpdatedObjectCount++;
        }
    }

    /**
    * \fn RemoveUpdatedObjectCount
    * \brief Notifies statistics of a potential updated object to remove.
    * \param _removeObject - Object to be removed from the stats.
    * 
    **/
    static void RemoveUpdatedObjectCount(LPK_LogicBase _removeObject)
    {
        if (_removeObject.UsesUpdate())
        {
            m_iUpdatedComponentCount--;

            if (m_pObjectList.Contains(_removeObject.gameObject))
                m_iUpdatedObjectCount--;
        }
    }

    /**
    * \fn GetTotalObjectCount
    * \brief Returns the total count of LPK objects.
    * 
    * \return uint - Total count of LPK objects in the game.
    **/
    public static uint GetTotalObjectCount()
    {
        return m_iObjectCount;
    }

    /**
    * \fn GetTotalUpdatedObjectCount
    * \brief Returns the total count of LPK objects that use Update.
    * 
    * \return uint - Total count of LPK objects in the game that use update.
    **/
    public static uint GetTotalUpdatedObjectCount()
    {
        return m_iUpdatedObjectCount;
    }

    /**
    * \fn GetTotalComponentCount
    * \brief Returns the total count of LPK components currently in use.
    * 
    * \return uint - Total count of LPK components in the game.
    **/
    public static uint GetTotalComponentCount()
    {
        return m_iComponentCount;
    }

    /**
    * \fn GetTotalUpdatedComponentCount
    * \brief Returns the total count of LPK components that use Update.
    * 
    * \return uint - Total count of LPK components that use Update in the game.
    **/
    public static uint GetTotalUpdatedComponentCount()
    {
        return m_iUpdatedComponentCount;
    }
}
