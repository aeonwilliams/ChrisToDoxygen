/***************************************************
\file           LPK_DebugStatisticsDisplay
\author        Christopher Onorati
\date   2/9/2019
\version   2018.3.4

\brief
  This component can be used to debug performance problems
  by getting statistics on LPK component and object usage.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DebugStatisticsDisplay
* \brief Debugger component to return data on LPK usage statistics.
**/
public class LPK_DebugStatisticsDisplay : MonoBehaviour
{
    /************************************************************************************/

    [Header("Object Statistics")]

    [Tooltip("Total amount of LPK objects in the game.")]
    [PreventEditing]
    public uint m_LPKObjectCount;

    [Tooltip("Total amount of LPK objects that use Update functions in the game.")]
    [PreventEditing]
    public uint m_LPKUpdatedObjectsCount;

    [Header("Component Statistics")]

    [Tooltip("Total amount of LPK components in the game.")]
    [PreventEditing]
    public uint m_LPKComponentCount;

    [Tooltip("Total amount of LPK components that use Update in the game.")]
    [PreventEditing]
    public uint m_LPKUpdatedComponentCount;

    /**
    * \fn Start
    * \brief Connects to event listening.
    * 
    * 
    **/
    void Start()
    {
        LPK_DebugStatistics.OnDebugStatisticsUpdated += UpdateDebugStatistics;
    }

    /**
    * \fn UpdateDebugStatistics
    * \brief Updates debug statistics when changes are made to objects and components in the game.
    * 
    * 
    **/
    void UpdateDebugStatistics()
    {
        m_LPKObjectCount = LPK_DebugStatistics.GetTotalObjectCount();
        m_LPKUpdatedObjectsCount = LPK_DebugStatistics.GetTotalUpdatedObjectCount();
        m_LPKComponentCount = LPK_DebugStatistics.GetTotalComponentCount();
        m_LPKUpdatedComponentCount = LPK_DebugStatistics.GetTotalUpdatedComponentCount();
    }

    /**
    * \fn OnDestroy
    * \brief Detaches from event listening.
    * 
    * 
    **/
    void OnDestroy()
    {
        LPK_DebugStatistics.OnDebugStatisticsUpdated -= UpdateDebugStatistics;
    }
}
