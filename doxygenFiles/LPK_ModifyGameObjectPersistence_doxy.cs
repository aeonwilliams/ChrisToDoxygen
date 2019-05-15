/***************************************************
\file           LPK_ModifyGameObjectPersistence.cs
\author        Christopher Onorati
\date   2/7/2019
\version   2018.3.4

\brief
  This component modifies the persistence of gameobjects
  across scene loads.


This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

/**
* \class LPK_ModifyGameObjectPersistence
* \brief Component to modify the persistence of game objects across scenes.
**/
public class LPK_ModifyGameObjectPersistence : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether objects selected should have persistence modified on Start based on Toggle Type.")]
    [Rename("Activate On Start")]
    public bool m_bActive = true;

    [Tooltip("How to modify the persistent state of objects.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Game objects to mark as persistent.  If both this array, and the tag array are left at size 0, the script default to its owner game object.")]
    public GameObject[] m_GameObjects;

    [Tooltip("Tags to search for to mark objects as persistent.  If both this array, and the game object array are left at size 0, the script default to its owner game object.")]
    public string[] m_Tags;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn OnStart
    * \brief Sets up event listening and initial persistence.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_bActive)
        {
            if (m_eToggleType == LPK_ToggleType.ON)
                SetObjectPersistence();
            else if (m_eToggleType == LPK_ToggleType.OFF)
                SetObjectDestroyable();
            else if (m_eToggleType == LPK_ToggleType.TOGGLE)
                ToggleObjectPersistence();
        }
    }

    /**
    * \fn OnEvent
    * \brief Manages setting the destructable state of objects.
    * 
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Received event.");

        //NOTENOTE:  Technically these could be written as one function, and there is good argument for doing this.  I have seperated them out though, for easy
        //           of code readability.  The tradeoff of being able to read this code easily compared to having duplicate lines is okay here, I think.
        if (m_eToggleType == LPK_ToggleType.ON)
            SetObjectPersistence();
        else if (m_eToggleType == LPK_ToggleType.OFF)
            SetObjectDestroyable();
        else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            ToggleObjectPersistence();
    }

    /**
    * \fn SetObjectPersistence
    * \brief Marks specified objects to not be destroyed between scene loads.
    * 
    * 
    **/
    void SetObjectPersistence()
    {
        //Default to self.
        if(m_Tags.Length == 0 && m_GameObjects.Length == 0)
        {
            Object.DontDestroyOnLoad(gameObject);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Game object " + gameObject.name + " set to be persistent.");

            return;
        }

        for (int i = 0; i < m_GameObjects.Length; i++)
        {
            Object.DontDestroyOnLoad(m_GameObjects[i]);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Game object " + m_GameObjects[i].name + " set to be persistent.");
        }

        for (int i = 0; i < m_Tags.Length; i++)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(m_Tags[i]);

            for (int j = 0; j < taggedObjects.Length; j++)
            {
                Object.DontDestroyOnLoad(taggedObjects[j]);

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + taggedObjects[j].name + " set to be persistent.");
            }
        }
    }

    /**
    * \fn SetObjectDestroyable
    * \brief Marks specified objects to be destroyed on the next scene load.
    * 
    * 
    **/
    void SetObjectDestroyable()
    {
        //Default to self.
        if (m_Tags.Length == 0 && m_GameObjects.Length == 0)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Game object " + gameObject.name + " set to be destroyed on next scene load.");

            return;
        }

        for (int i = 0; i < m_GameObjects.Length; i++)
        {
            SceneManager.MoveGameObjectToScene(m_GameObjects[i], SceneManager.GetActiveScene());

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Game object " + m_GameObjects[i].name + " set to be destroyed on next scene load.");
        }

        for (int i = 0; i < m_Tags.Length; i++)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(m_Tags[i]);

            for (int j = 0; j < taggedObjects.Length; j++)
            {
                SceneManager.MoveGameObjectToScene(taggedObjects[j], SceneManager.GetActiveScene());

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + taggedObjects[j].name + " set to be destroyed on next scene load.");
            }
        }
    }

    /**
    * \fn ToggleObjectPersistence
    * \brief Toggle the persistence of objects between active and inactive.
    * 
    * 
    **/
    void ToggleObjectPersistence()
    {
        //Default to self.
        if (m_Tags.Length == 0 && m_GameObjects.Length == 0)
        {
            if (gameObject.scene.buildIndex == -1)
            {
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + gameObject.name + " set to be destroyed on next scene load.");
            }
            else
            {
                Object.DontDestroyOnLoad(gameObject);

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + gameObject.name + " set to be persistent.");
            }

            return;
        }


        for (int i = 0; i < m_GameObjects.Length; i++)
        {
            if (m_GameObjects[i].scene.buildIndex == -1)
            {
                SceneManager.MoveGameObjectToScene(m_GameObjects[i], SceneManager.GetActiveScene());

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + m_GameObjects[i].name + " set to be destroyed on next scene load.");
            }
            else
            {
                Object.DontDestroyOnLoad(m_GameObjects[i]);

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Game object " + m_GameObjects[i].name + " set to be persistent.");
            }
        }

        for (int i = 0; i < m_Tags.Length; i++)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(m_Tags[i]);

            for (int j = 0; j < taggedObjects.Length; j++)
            {
                if (taggedObjects[j].scene.buildIndex == -1)
                {
                    SceneManager.MoveGameObjectToScene(taggedObjects[j], SceneManager.GetActiveScene());

                    if (m_bPrintDebug)
                        LPK_PrintDebug(this, "Game object " + taggedObjects[j].name + " set to be destroyed on next scene load.");
                }
                else
                {
                    Object.DontDestroyOnLoad(taggedObjects[j]);

                    if (m_bPrintDebug)
                        LPK_PrintDebug(this, "Game object " + taggedObjects[j].name + " set to be persistent.");
                }
            }
        }
    }
}
