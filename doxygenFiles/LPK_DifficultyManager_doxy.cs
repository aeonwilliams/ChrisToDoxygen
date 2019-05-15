/***************************************************
\file           LPK_DifficultyManager.cs
\author        Christopher Onorati
\date   12/18/18
\version   2.17

\brief 
  This component manages the difficulty of the game.
  The difficutly level is serialized out and can be
  modified by the player via use of the ModifyDifficulty
  component.  This component should be added to every scene
  (preferablly the Main Camera), as well as on any UI
  canvas that wants to modify volume levels (like the
  options screen).

  Note that by default, the LPK does ==NOT== use the difficulty
  level for anything.  It is up to the user to decide how they
  want to implement difficulty modification, if at all.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;   /* Saved data. */
using System.IO;    /* File IO */

/**
* \class LPK_DifficultyManager
* \brief Manager to adjust gameplay difficulty.
**/
[DisallowMultipleComponent]
public class LPK_DifficultyManager : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_DifficultyLevel
    {
        EASY,
        MEDIUM,
        HARD,
    };

    /************************************************************************************/

    //NOTENOTE: Start out at MEDIUM to allow wiggle room up and down by default.
    public static LPK_DifficultyLevel m_eDifficultyLevel = LPK_DifficultyLevel.MEDIUM;

    /**
    * \fn OnEnable
    * \brief Restores difficulty settings from a past game session.
    * 
    * 
    **/
    public void OnEnable()
    {
        if (File.Exists(Application.persistentDataPath + "/difficulty_levels.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/difficulty_levels.dat", FileMode.Open);
            LPK_DifficultyData data = (LPK_DifficultyData)bf.Deserialize(file);
            file.Close();

            m_eDifficultyLevel = data.m_eDifficultyLevel;
        }
    }

    /**
    * \fn OnDestroy
    * \brief Saves difficulty settings for a future game session.
    * 
    * 
    **/
    public void OnDestroy()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/difficulty_levels.dat");

        LPK_DifficultyData data = new LPK_DifficultyData();

        data.m_eDifficultyLevel = m_eDifficultyLevel;

        bf.Serialize(file, data);
        file.Close();
    }

    /**
    * \fn IncreaseDifficulty
    * \brief Increase the difficulty level of the game.
    * 
    * 
    **/
    public void IncreaseDifficulty()
    {
        m_eDifficultyLevel++;

        DispatchEvent();
    }

    /**
    * \fn DecreaseDifficulty
    * \brief Decrease the dfficulty level of the game.
    * 
    * 
    **/
    public void DecreaseDifficulty()
    {
        m_eDifficultyLevel--;

        DispatchEvent();
    }

    /**
    * \fn SetDifficultyEasy
    * \brief Set the difficulty level of the game to be easy.
    * 
    * 
    **/
    public void SetDifficultyEasy()
    {
        m_eDifficultyLevel = LPK_DifficultyLevel.EASY;
        DispatchEvent();
    }

    /**
    * \fn SetDifficultyMedium
    * \brief Set the difficulty level of the game to be medium.
    * 
    * 
    **/
    public void SetDifficultyMedium()
    {
        m_eDifficultyLevel = LPK_DifficultyLevel.MEDIUM;
        DispatchEvent();
    }

    /**
* \fn SetDifficultyHard
* \brief Set the difficulty level of the game to be hard.
* 
* 
**/
    public void SetDifficultyHard()
    {
        m_eDifficultyLevel = LPK_DifficultyLevel.HARD;
        DispatchEvent();
    }

    /**
    * \fn DispatchEvent
    * \brief Dispatch difficulty levels adjusted event.
    * 
    * 
    **/
    void DispatchEvent()
    {
        //Event dispatch.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(null, null);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_OptionManagerEventTrigger = new LPK_EventList.LPK_OPTION_MANAGER_EVENTS[] { LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_DifficultyLevelAdjusted};

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}

/**
* \class LPK_DifficultyData
* \brief Saves difficulty data.
**/
[Serializable]
class LPK_DifficultyData
{
    public LPK_DifficultyManager.LPK_DifficultyLevel m_eDifficultyLevel = LPK_DifficultyManager.LPK_DifficultyLevel.MEDIUM;
}
