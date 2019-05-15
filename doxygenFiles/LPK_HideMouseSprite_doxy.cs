/***************************************************
\file           LPK_HideMouseSprite.cs
\author        Christopher Onorati
\date   12/2/2018
\version   2.17

\brief
  This component hides the mouse from the screen.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System.Collections;

/**
* \class HideMouse
* \brief Hides the mouse from the screen.
**/
public class LPK_HideMouseSprite : LPK_LogicBase
{
    /**
    * \fn OnStart
    * \brief Hides the cursor automatically.
    * 
    * 
    **/
    override protected void OnStart()
    {
        Cursor.visible = false;
    }
}
