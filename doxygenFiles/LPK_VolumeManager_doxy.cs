/***************************************************
\file           LPK_VolumeManager.cs
\author        Christopher Onorati
\date   12/17/18
\version   2.17

\brief 
  This component manages the volume levels of any LPK
  object that has a ModifyAudioVolume component on it.
  This component should be added to every scene
  (preferablly the Main Camera), as well as on any UI
  canvas that wants to modify volume levels (like the
  options screen).

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
* \class LPK_VolumeManager
* \brief Manager to adjust volume of audio.
**/
[DisallowMultipleComponent]
public class LPK_VolumeManager : MonoBehaviour
{
    /************************************************************************************/

    public enum LPK_AudioType
    {
        SFX,
        MUSIC,
        VOICE,
    };

    /************************************************************************************/

    //NOTENOTE: Start out at 0.7f to allow wiggle room up and down by default.
    public static float m_flSFXLevel = 0.7f;
    public static float m_flMusicLevel = 0.7f;
    public static float m_flVoiceLevel = 0.7f;
    public static float m_flMasterLevel = 0.7f;

    //NOTENOTE: Rate at which to change volume levels when functions are called.
    const float m_flAudioAdjustRate = 0.1f;

    /**
    * \fn OnEnable
    * \brief Restores sound settings from a past game session.
    * 
    * 
    **/
    public void OnEnable()
    {
        if (File.Exists(Application.persistentDataPath + "/volume_levels.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/volume_levels.dat", FileMode.Open);
            LPK_VolumeData data = (LPK_VolumeData)bf.Deserialize(file);
            file.Close();

            m_flMasterLevel = data.m_flMasterLevel;
            m_flSFXLevel = data.m_flSFXLevel;
            m_flMusicLevel = data.m_flMusicLevel;
            m_flVoiceLevel = data.m_flVoiceLevel;
        }
    }

    /**
    * \fn OnDestroy
    * \brief Saves sound settings for a future game session.
    * 
    * 
    **/
    public void OnDestroy()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/volume_levels.dat");

        LPK_VolumeData data = new LPK_VolumeData();

        //Record volume data.
        data.m_flMasterLevel = m_flMasterLevel;
        data.m_flMusicLevel = m_flMusicLevel;
        data.m_flSFXLevel = m_flSFXLevel;
        data.m_flVoiceLevel = m_flVoiceLevel;

        bf.Serialize(file, data);
        file.Close();
    }

    /**
    * \fn IncreaseSFXVolume
    * \brief Increase the SFX volume of the game.
    * 
    * 
    **/
    public void IncreaseSFXVolume()
    {
        m_flSFXLevel = Mathf.Clamp(m_flSFXLevel + m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn DecreaseSFXVolume
    * \brief Decrease the SFX volume of the game.
    * 
    * 
    **/
    public void DecreaseSFXVolume()
    {
        m_flSFXLevel = Mathf.Clamp(m_flSFXLevel - m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn IncreaseMusicVolume
    * \brief Increase the Music volume of the game.
    * 
    * 
    **/
    public void IncreaseMusicVolume()
    {
        m_flMasterLevel = Mathf.Clamp(m_flMasterLevel + m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn DecreaseMusicVolume
    * \brief Decrease the Music volume of the game.
    * 
    * 
    **/
    public void DecreaseMusicVolume()
    {
        m_flMasterLevel = Mathf.Clamp(m_flMasterLevel - m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn IncreaseVoiceVolume
    * \brief Increase the Voice volume of the game.
    * 
    * 
    **/
    public void IncreaseVoiceVolume()
    {
        m_flVoiceLevel = Mathf.Clamp(m_flVoiceLevel + m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn DecreaseVoiceVolume
    * \brief Decrease the Voice volume of the game.
    * 
    * 
    **/
    public void DecreaseVoiceVolume()
    {
        m_flVoiceLevel = Mathf.Clamp(m_flVoiceLevel - m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn IncreaseMasterVolume
    * \brief Increase the master volume of the game.
    * 
    * 
    **/
    public void IncreaseMasterVolume()
    {
        m_flMasterLevel = Mathf.Clamp(m_flMasterLevel + m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn DecreaseMasterVolume
    * \brief Decrease the master volume of the game.
    * 
    * 
    **/
    public void DecreaseMasterVolume()
    {
        m_flMasterLevel = Mathf.Clamp(m_flMasterLevel - m_flAudioAdjustRate, 0.0f, 1.0f);
        DispatchEvent();
    }

    /**
    * \fn DispatchEvent
    * \brief Dispatch audio levels adjusted event.
    * 
    * 
    **/
    void DispatchEvent()
    {
        //Event dispatch.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(null, null);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_OptionManagerEventTrigger = new LPK_EventList.LPK_OPTION_MANAGER_EVENTS[] { LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_AudioLevelsAdjusted };

        LPK_EventManager.InvokeEvent(sendEvent, data);
    }
}

/**
* \class LPK_VolumeData
* \brief Saves volume data.
**/
[Serializable]
class LPK_VolumeData
{
    public float m_flSFXLevel = 0.7f;
    public float m_flMusicLevel = 0.7f;
    public float m_flVoiceLevel = 0.7f;
    public float m_flMasterLevel = 0.7f;
}
