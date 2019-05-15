/***************************************************
\file           LPK_ChangeWindowedState.cs
\author        Christopher Onorati
\date   12/15/2018
\version   2.17

\brief
  This component can be used to change the fullscreen mode
  of the game window during runtime.  This component is
  ideally used on an options menu.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_ChangeWindowedState
* \brief Component used to change windowed state of game.
**/
public class LPK_ChangeWindowedState : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_WindowToggleType
    {
        CHANGE_FULLSCREEN,
        CHANGE_WINDOWED,
        CHANGE_TOGGLE,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("How to change the windowed state when activated via event.")]
    [Rename("Toggle Type")]
    public LPK_WindowToggleType m_eWindowToggleType;

    /**
    * \fn SetWindowType
    * \brief Changes windowed state.  Moved to public so UI buttons can interact with this.
    * 
    * 
    **/
    public void SetWindowType()
    {
        if (m_eWindowToggleType == LPK_WindowToggleType.CHANGE_FULLSCREEN)
            Screen.fullScreen = true;
        else if (m_eWindowToggleType == LPK_WindowToggleType.CHANGE_WINDOWED)
            Screen.fullScreen = false;
        else
            Screen.fullScreen = !Screen.fullScreen;
    }
}
