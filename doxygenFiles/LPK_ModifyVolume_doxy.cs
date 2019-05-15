/***************************************************
\file           LPK_ModifyVolume.cs
\author        Christopher Onorati
\date   12/17/2018
\version   2.17

\brief
  Implementation of a volume manager to adjust Audio Source
  volumes based on the type of FX they play at runtime.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ModifyVolume
* \brief Component to modify audio volumes at run time.
**/
[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class LPK_ModifyVolume : LPK_LogicBase
{
    [Header("Component Properties")]

    [Tooltip("Type of audio this gameobject will emit.  Note that each game object should only emit one audio file.")]
    [Rename("Audio Type")]
    public LPK_VolumeManager.LPK_AudioType m_eAudioType;

    /************************************************************************************/

    AudioSource m_cAudioSource;

    /**
    * \fn OnStart
    * \brief Connects to event listening.
    * 
    * 
    **/
    override protected void OnStart ()
    {
        m_cAudioSource = GetComponent<AudioSource>();

        //Set initial audio levels.
        SetAudioLevel();

        LPK_EventList audioLevelsList = new LPK_EventList();
        audioLevelsList.m_OptionManagerEventTrigger = new LPK_EventList.LPK_OPTION_MANAGER_EVENTS[] { LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_AudioLevelsAdjusted };

        InitializeEvent(audioLevelsList, OnAudioLevelsChange, false);
    }

    /**
    * \fn OnAudioLevelsChange
    * \brief Call the Audio Source's volume.
    * \param data - Event registration data (unused).
    * 
    **/
    void OnAudioLevelsChange(LPK_EventManager.LPK_EventData data)
    {
        SetAudioLevel();
    }

    /**
    * \fn SetAudioLevel
    * \brief Adjusts the Audio Source's volume.
    * 
    * 
    **/
    void SetAudioLevel()
    {
        if (m_eAudioType == LPK_VolumeManager.LPK_AudioType.MUSIC)
            m_cAudioSource.volume = LPK_VolumeManager.m_flMusicLevel * LPK_VolumeManager.m_flMasterLevel;
        else if (m_eAudioType == LPK_VolumeManager.LPK_AudioType.SFX)
            m_cAudioSource.volume = LPK_VolumeManager.m_flSFXLevel * LPK_VolumeManager.m_flMasterLevel;
        else if (m_eAudioType == LPK_VolumeManager.LPK_AudioType.VOICE)
            m_cAudioSource.volume = LPK_VolumeManager.m_flVoiceLevel * LPK_VolumeManager.m_flMasterLevel;
    }
}
