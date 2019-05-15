/***************************************************
\file           LPK_ChangeWindowResolution.cs
\author        Christopher Onorati
\date   12/15/2018
\version   2.17

\brief
  This component can be used to change the resolution
  of the game window during runtime.  This component is
  ideally used on an options menu.  UI only component.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   /*Text*/

/**
* \class LPK_ChangeWindowResolution
* \brief Component used to change resolution of game.
**/
public class LPK_ChangeWindowResolution : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Sets the resolution counter to wrap.  Example: If at the end of the array, and a positive event is received, go to the beggining of the array.")]
    [Rename("Wrap Counter")]
    public bool m_bWrap = true;

    [Tooltip("Object to change text of to reflect current resolution.")]
    [Rename("Text Object")]
    public GameObject m_pText;

    /************************************************************************************/

    //Position in the array of vectors to default on
    int m_iCounter;

    //List of all resolution values valid for monitor.
    Resolution[] m_aResolutions;

    /**
    * \fn OnStart
    * \brief Make sure counter is set to a legal value.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_aResolutions = Screen.resolutions;

        for (int i = 0; i < m_aResolutions.Length; i++)
        {
            if(Screen.currentResolution.height == m_aResolutions[i].height && Screen.currentResolution.width == m_aResolutions[i].width)
            {
                m_iCounter = i;
                break;
            }
        }

        SetText();
    }

    /**
    * \fn OnPositiveEvent
    * \brief Changes window resolution up in the array.
    * 
    * 
    **/
    public void OnPositiveEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintWarning(this, "POSITIVE options event received.");

        m_iCounter++;
        CheckBounds();

        Screen.SetResolution(m_aResolutions[m_iCounter].width, m_aResolutions[m_iCounter].height, Screen.fullScreen);

        SetText();
    }

    /**
    * \fn OnNegativeEvent
    * \brief Changes window resolution down in the array.
    * 
    * 
    **/
    public void OnNegativeEvent()
    {
        if (m_bPrintDebug)
            LPK_PrintWarning(this, "NEGATIVE options event received.");

        m_iCounter--;
        CheckBounds();

        Screen.SetResolution(m_aResolutions[m_iCounter].width, m_aResolutions[m_iCounter].height, Screen.fullScreen);

        SetText();
    }

    /**
    * \fn CheckBounds
    * \brief Keeps the counter in the legal range.
    * 
    * 
    **/
    void CheckBounds()
    {
        //Valid value.
        if (m_iCounter >= 0 && m_iCounter < m_aResolutions.Length)
            return;

        //Counter has gone below legal range.
        if (m_iCounter < 0 && m_bWrap)
            m_iCounter = m_aResolutions.Length - 1;
        else if (m_iCounter < 0 && !m_bWrap)
            m_iCounter = 0;

        //Counter has gone above legal range.
        if (m_iCounter >= m_aResolutions.Length && m_bWrap)
            m_iCounter = 0;
        else if (m_iCounter >= m_aResolutions.Length && !m_bWrap)
            m_iCounter = m_aResolutions.Length;
    }

    /**
    * \fn SetText
    * \brief Sets the text display.
    * 
    * 
    **/
    void SetText()
    {
        //Error - invalid object or missing component.
        if (m_pText == null || m_pText.GetComponent<Text>() == null)
        {
            if (m_bPrintDebug)
                LPK_PrintError(this, "Invalid text display set for resolution switcher!");

            return;
        }

        //Update the text display.
        m_pText.GetComponent<Text>().text = m_aResolutions[m_iCounter].width.ToString() + " x " + m_aResolutions[m_iCounter].height.ToString();
    }
}
