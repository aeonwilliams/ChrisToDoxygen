/***************************************************
\file           LPK_ModifyGameObjectActiveState.cs
\author        Christopher Onorati
\date   12/26/2018
\version   2.17

\brief
  This component can be used to enable/disable the active
  state of gameobjects.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ModifyGameObjectActiveState
* \brief Component used to enable/disable game objects.
**/
public class LPK_ModifyGameObjectActiveState : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("How to change the active state of the declared gameobject(s).")]
    public LPK_ToggleType m_ToggleType;

    [Tooltip("Gameobject(s) to change the enabled state of.")]
    public GameObject[] m_ModifyGameObject;

    [Tooltip("Tag(s) to change the enabled state of.")]
    public string[] m_ModifyTag;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn Start
    * \brief Sets up what event to listen to for sprite and color modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Sets the active state of the gameobjects.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Debug search.
        for (int i = 0; i < m_ModifyGameObject.Length; i++)
        {
            if (m_ModifyGameObject[i] == null)
                continue;

            if (m_ToggleType == LPK_ToggleType.ON)
                m_ModifyGameObject[i].SetActive(true);
            else if (m_ToggleType == LPK_ToggleType.OFF)
                m_ModifyGameObject[i].SetActive(false);
            else if (m_ToggleType == LPK_ToggleType.TOGGLE)
            {
                if (!m_ModifyGameObject[i].activeSelf)
                    m_ModifyGameObject[i].SetActive(true);
                else if (m_ModifyGameObject[i].activeSelf)
                    m_ModifyGameObject[i].SetActive(false);
            }

            //Debug info.
            if (m_bPrintDebug && m_ModifyGameObject[i].activeSelf)
                LPK_PrintDebug(this, "Changing active state of " + m_ModifyGameObject[i] + " to ON.");
            else if (m_bPrintDebug && !m_ModifyGameObject[i].activeSelf)
                LPK_PrintDebug(this, "Changing active state of " + m_ModifyGameObject[i] + " to OFF.");
        }

        //Tag search.
        for (int i = 0; i < m_ModifyTag.Length; i++)
        {
            if (m_ModifyTag[i] == null)
                continue;

            GameObject[] objects = GameObject.FindGameObjectsWithTag(m_ModifyTag[i]);

            for (int j = 0; j < objects.Length; j++)
            {
                if (m_ToggleType == LPK_ToggleType.ON)
                    objects[j].SetActive(true);
                else if (m_ToggleType == LPK_ToggleType.OFF)
                    objects[j].SetActive(false);
                else if (m_ToggleType == LPK_ToggleType.TOGGLE)
                {
                    if (!objects[j].activeSelf)
                        objects[j].SetActive(true);
                    else if (objects[j].activeSelf)
                        objects[j].SetActive(false);
                }

                //Debug info.
                if (m_bPrintDebug && objects[j].activeSelf)
                    LPK_PrintDebug(this, "Changing active state of " + objects[j] + " to ON.");
                else if (m_bPrintDebug && !objects[j].activeSelf)
                    LPK_PrintDebug(this, "Changing active state of " + objects[j] + " to OFF.");
            }
        }
    }
}
