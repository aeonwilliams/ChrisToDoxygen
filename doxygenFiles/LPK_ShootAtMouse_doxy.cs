/***************************************************
\file           LPK_ShootAtMouse.cs
\author        Christopher Onorati
\date   2/19/2019
\version   2018.3.4

\brief
  This component can be used to cause an object to be shot
  from a transform, towards the mouse.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ShootAtMouse
* \brief Component to spawn an object and shoot it towards the mouse.
**/
public class LPK_ShootAtMouse : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_InputMode
    {
        PRESSED,
        RELEASED,
        HELD,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("What prefab to instantiate upon mouse event.")]
    [Rename("Prefab")]
    public GameObject m_pPrefabToSpawn;

    [Tooltip("Object whose position will be the spawn location")]
    [Rename("Spawner Object")]
    public GameObject m_pTargetSpawnTransformObj;

    [Tooltip("Total maximum number of instances this object is allowed to spawn. (0 means no limit).")]
    [Rename("Max Spawns")]
    public int m_iMaxTotalSpawnCount = 0;

    [Tooltip("Max amount of objects to have active at once.  (0 means no limit).")]
    [Rename("Max Alive")]
    public int m_iMaxAliveAtOnce = 0;

    [Tooltip("At what z depth should the object be shot towards.  Ideally the same z value as the Spawner Object.")]
    [Rename("Z Depth")]
    public float m_flZDepth = 0.0f;

    [Tooltip("Cooldown time between shots fired.  This is time in seconds.")]
    [Rename("Cooldown")]
    public float m_flCooldown = 0.0f;

    [Tooltip("Speed to shoot an object off at.")]
    [Rename("Bullet Speed")]
    public float m_flShootSpeed = 5.0f;

    [Tooltip("Which mouse button will trigger sending the event.  Note that Any does not detect scrolwheel.")]
    [Rename("Mouse Button")]
    public LPK_MouseButtons m_eMouseButton = LPK_MouseButtons.LEFT;

    [Tooltip("What mode should cause the event dispatch.")]
    [Rename("Input Mode")]
    public LPK_InputMode m_eInputMode = LPK_InputMode.PRESSED;

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Object for mouse input event.")]
    public LPK_EventReceivers m_InputReceiver;

    [Tooltip("Receiver Game Object for spawn event.  The object spawned is automatically added to this array.")]
    public LPK_EventReceivers m_SpawnObjectReceivers;

    /************************************************************************************/

    //Mouse press detection.
    int m_iMouseButton;

    //Cool down detection.
    bool m_bOnCooldown = false;

    //Number of objects spawned by this component so far
    int m_iSpawnCount = 0;

    List<GameObject> m_pActiveList = new List<GameObject>();

    /**
    * \fn OnStart
    * \brief Initializes m_iMouseButton;
    * 
    * 
    **/
    override protected void OnStart()
    {
        if (m_eMouseButton == LPK_MouseButtons.LEFT)
            m_iMouseButton = 0;
        else if (m_eMouseButton == LPK_MouseButtons.RIGHT)
            m_iMouseButton = 1;
        else
            m_iMouseButton = 2;

        //Assume self if set to null.
        if (m_pTargetSpawnTransformObj == null)
            m_pTargetSpawnTransformObj = gameObject;
    }

    /**
    * \fn OnUpdate
    * \brief Handles input checking for mice.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (m_bOnCooldown)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "On cooldown");

            return;
        }

        CleanUpList();

        //Pressed.
        if (m_eInputMode == LPK_InputMode.PRESSED)
        {
            if (Input.GetMouseButtonDown(m_iMouseButton) || m_eMouseButton == LPK_MouseButtons.ANY)
                SpawnObjectPrefab();
        }
        //Released.
        else if (m_eInputMode == LPK_InputMode.RELEASED || m_eMouseButton == LPK_MouseButtons.ANY)
        {
            if (Input.GetMouseButtonUp(m_iMouseButton) || m_eMouseButton == LPK_MouseButtons.ANY)
                SpawnObjectPrefab();
        }
        //Held.
        else if (m_eInputMode == LPK_InputMode.HELD || m_eMouseButton == LPK_MouseButtons.ANY)
        {
            if (Input.GetMouseButton(m_iMouseButton))
                SpawnObjectPrefab();
        }

        //Mouse scroll
        else
        {
            float mouseScrollDelta = Input.mouseScrollDelta.y;

            if (mouseScrollDelta < 0 && m_eMouseButton == LPK_MouseButtons.MIDDLE_SCROLL_DOWN)
                SpawnObjectPrefab();

            else if (mouseScrollDelta > 0 && m_eMouseButton == LPK_MouseButtons.MIDDLE_SCROLL_UP)
                SpawnObjectPrefab();
        }
    }

    /**
     * \fn SpawnObjectPrefab
     * \brief Spawns the prefab object and fires it towards mouse.
     * 
     * 
     **/
    void SpawnObjectPrefab()
    {
        if (m_iMaxTotalSpawnCount == 0 || m_iSpawnCount < m_iMaxTotalSpawnCount)
        {
            //Too many alive objects, but still count the spawn.
            //NOTENOTE: If you want failed spawns not to count, remove the increment line below.
            if (m_pActiveList.Count >= m_iMaxAliveAtOnce && m_iMaxAliveAtOnce != 0)
            {
                m_iSpawnCount++;
                return;
            }

            GameObject obj = Instantiate(m_pPrefabToSpawn, m_pTargetSpawnTransformObj.transform.position, Quaternion.identity);
            Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();

            //Debug info - failed due to no rigidbody.
            if (objRb == null)
            {
                if (m_bPrintDebug)
                    LPK_PrintError(this, "No rigidbody component on object to shoot!");
            }

            //Shoot.
            else
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = m_flZDepth;

                Vector3 direction = mousePos - m_pTargetSpawnTransformObj.transform.position;
                objRb.velocity = direction.normalized * m_flShootSpeed;
            }

            m_iSpawnCount++;
            m_pActiveList.Add(obj);

            DispatchLPKMouseInputEvent();
            DispatchLPKSpawnEvent(obj);

            m_bOnCooldown = true;
            StartCoroutine(DelayCoolDown());
        }
    }


    /**
    * \fn DelayCoolDown
    * \brief Creates a delay between spawns.
    * 
    * 
    **/
    IEnumerator DelayCoolDown()
    {
        yield return new WaitForSeconds(m_flCooldown);
        m_bOnCooldown = false;
    }

    /**
    * \fn CleanUpList
    * \brief Remove any nullified objects from the list.
    * 
    * 
    **/
    void CleanUpList()
    {
        for (int j = 0; j < m_pActiveList.Count; j++)
        {
            GameObject checkObj = m_pActiveList[j];

            if (checkObj == null)
                m_pActiveList.RemoveAt(j);
        }
    }

    /**
    * \fn DispatchLPKMouseInputEvent
    * \brief Dispatches the mouse event and prints debug info if set.
    * 
    * 
    **/
    void DispatchLPKMouseInputEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_InputReceiver);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_MouseInput };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Mouse Input dispatched");
    }

    /**
    * \fn DispatchLPKSpawnEvent
    * \brief Dispatches the spawn object event.
    * \param obj - Gameobject created to add to the array of event receivers.
    * 
    **/
    void DispatchLPKSpawnEvent(GameObject obj)
    {
        //Dispatch spawn event
        GameObject[] temp = new GameObject[m_SpawnObjectReceivers.m_GameObjectList.Length + 1];

        for (int i = 0; i < m_SpawnObjectReceivers.m_GameObjectList.Length; i++)
            temp[i] = m_SpawnObjectReceivers.m_GameObjectList[i];

        temp[temp.Length - 1] = obj;

        LPK_EventManager.LPK_EventData sendData = new LPK_EventManager.LPK_EventData(gameObject, m_SpawnObjectReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_ObjectSpawned };

        LPK_EventManager.InvokeEvent(sendEvent, sendData);
    }
}
