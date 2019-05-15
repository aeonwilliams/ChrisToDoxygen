/***************************************************
\file           LPK_VolumeIndicator.cs
\author        Christopher Onorati
\date   12/17/18
\version   2.17

\brief 
  This component is an indicator to display the current
  volume level of a sound type on a UI screen.  This should
  ideally be used on a canvas represting the options
  screen.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   /*Text */

/**
* \class LPK_VolumeIndicator
* \brief Indicator to show what level the volume is at.
**/
[RequireComponent(typeof(Text))]
public class LPK_VolumeIndicator : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_AudioDisplayType
    {
        SFX,
        MUSIC,
        VOICE,
        MASTER,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Type of audio this gameobject will emit.  Note that each game object should only emit one audio file.")]
    [Rename("Audio Type")]
    public LPK_AudioDisplayType m_eAudioType;

    /************************************************************************************/

    Text m_cText;

    /**
    * \fn OnStart
    * \brief Connects to event listening and initial display.
    * 
    * 
    **/
    override protected void OnStart ()
    {
        m_cText = gameObject.GetComponent<Text>();

        SetText();

        LPK_EventList audioLevelsList = new LPK_EventList();
        audioLevelsList.m_OptionManagerEventTrigger = new LPK_EventList.LPK_OPTION_MANAGER_EVENTS[] { LPK_EventList.LPK_OPTION_MANAGER_EVENTS.LPK_AudioLevelsAdjusted };

        InitializeEvent(audioLevelsList, OnAudioLevelsChange, false);
    }

    /**
    * \fn OnAudioLevelsChange
    * \brief Call the Set Text function.
    * \param data - Event registration data (unused).
    * 
    **/
    void OnAudioLevelsChange(LPK_EventManager.LPK_EventData data)
    {
        SetText();
    }

    /**
    * \fn SetText
    * \brief Set the text to display on screen.
    * 
    * 
    **/
    void SetText()
    {
        if (m_eAudioType == LPK_AudioDisplayType.MUSIC)
            m_cText.text = Mathf.RoundToInt(LPK_VolumeManager.m_flMusicLevel * 10).ToString();
        else if (m_eAudioType == LPK_AudioDisplayType.SFX)
            m_cText.text = Mathf.RoundToInt(LPK_VolumeManager.m_flSFXLevel * 10).ToString();
        else if (m_eAudioType == LPK_AudioDisplayType.VOICE)
            m_cText.text = Mathf.RoundToInt(LPK_VolumeManager.m_flVoiceLevel * 10).ToString();
        else if (m_eAudioType == LPK_AudioDisplayType.MASTER)
            m_cText.text = Mathf.RoundToInt(LPK_VolumeManager.m_flMasterLevel * 10).ToString();
    }

    /**
    * \fn OnDestroyed
    * \brief Remove event listening.
    * 
    * 
    **/
    protected override void OnDestroyed()
    {
        DetachFunction(OnAudioLevelsChange);
    }
}
