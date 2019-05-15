/***************************************************
\file           LPK_DebugDraw.cs
\author        Christopher Onorati
\date   12/15/2018
\version   2.17

\brief
  Holds the parent class for all debug drawing components.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DebugBase
* \brief Base class for any debug drawing component.
**/
[ExecuteInEditMode]
public class LPK_DebugBase : MonoBehaviour
{
    /************************************************************************************/

    [Header("Base Debug Properties")]

    [Tooltip("Sets the debug draw to appear in game view.")]
    [Rename("Draw In Game")]
    public bool m_bDrawInGame = true;

    [Tooltip("Sets the debug draw to appear in editor view.")]
    [Rename("Draw In Editor")]
    public bool m_bDrawInEditor = true;

    [Tooltip("Whether this object's children should also be drawn.")]
    [Rename("Draw Hierarchy")]
    public bool m_bDrawHierarchy = false;

    /**
    * \fn Update
    * \brief Manages debug drawing.
    * 
    * 
    **/
    void Update()
    {
        if (!Application.isEditor && m_bDrawInGame)
        {
            if (m_bDrawHierarchy)
                DrawRecursive(gameObject);
            else
                Draw(gameObject);

        }
        else if (Application.isEditor && m_bDrawInEditor)
        {
            if (m_bDrawHierarchy)
                DrawRecursive(gameObject);
            else
                Draw(gameObject);
        }
        else if(!m_bDrawInEditor && !m_bDrawInGame)
        {
            if (m_bDrawHierarchy)
                UndrawRecursive(gameObject);
            else
                Undraw(gameObject);
        }

        OnUpdate();
    }

    /**
    * \fn OnUpdate
    * \brief Update support class to be used by parents.
    * 
    * 
    **/
    protected virtual void OnUpdate()
    {
        //Implemented in child class.
    }

    /**
    * \fn DrawRecursive
    * \brief Draw debug info for all children attached to the owner gameobject.
    * \param obj - Game object to find children of.
    * 
    **/
    void DrawRecursive(GameObject obj)
    {
        //Draw current object.
        Draw(obj);

        for (int i = 0; i < transform.childCount; i++)
            DrawRecursive(transform.GetChild(i).gameObject);
    }

    /**
    * \fn Draw
    * \brief Draw debug info for the gameobject.
    * \param obj - Game object to draw debug info for.
    * 
    **/
    protected virtual void Draw(GameObject obj)
    {
        //Implemented in inhereted class.
    }

    /**
    * \fn UndrawRecursive
    * \brief Remove debug info for all children attached to the owner gameobject.
    * \param obj - Game object to find children of.
    * 
    **/
    void UndrawRecursive(GameObject obj)
    {
        //Undraw debug info for current object.
        Undraw(obj);

        for (int i = 0; i < transform.childCount; i++)
            UndrawRecursive(transform.GetChild(i).gameObject);
    }

    /**
    * \fn Undraw
    * \brief Removes any debug info for the gameobject.
    * \param obj - Game object to remove debug info for.
    * 
    **/
    protected virtual void Undraw(GameObject obj)
    {
        //Implemented in inhereted class.
    }

    /**
    * \fn OnDisable
    * \brief Remnove debug info when the component is destroyed.
    * 
    * 
    **/
    protected void OnDisable()
    {
        if (m_bDrawHierarchy)
            UndrawRecursive(gameObject);
        else
            Undraw(gameObject);
    }
}

/**
* \class LPK_DebugLineDrawer
* \brief Debugging class to draw a line in game.
**/
public class LPK_DebugLineDrawer
{
    /************************************************************************************/

    //Reference to the line renderer component.
    LineRenderer m_cLineRenderer;

    //Color for the line to draw.
    public Color m_vecLineColor;

    //Game object created by class.
    public GameObject m_pGameObject;

    /**
    * \fn Constructor
    * \brief Set up the line renderer and color.
    * \param lineColor - color of the line.
    * 
    **/
    public LPK_DebugLineDrawer(Color lineColor, GameObject parent)
    {
        m_pGameObject = new GameObject("LPK_DebugLineObj");
        m_pGameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild
                                  | HideFlags.DontSaveInEditor | HideFlags.HideInInspector
                                  | HideFlags.HideInHierarchy;

        m_cLineRenderer = m_pGameObject.AddComponent<LineRenderer>();
        m_cLineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

        m_pGameObject.GetComponent<Transform>().SetParent(parent.transform);

        m_vecLineColor = lineColor;
    }

    /**
    * \fn DrawLineInGameView
    * \brief Draw the line.
    * \param startPos - start position of the line.
    *                endPos   - end position of the line.
    * 
    **/
    public void DrawLineInGameView(Vector3 startPos, Vector3 endPos)
    {
        //Not enough memory to create the line renderer component - ABORT.
        if (m_cLineRenderer == null)
            return;

        //Set color
        m_cLineRenderer.startColor = m_vecLineColor;
        m_cLineRenderer.endColor = m_vecLineColor;

        //Set width
        m_cLineRenderer.startWidth = 0.05f;
        m_cLineRenderer.endWidth = 0.05f;

        //Set line count which is 2
        m_cLineRenderer.positionCount = 2;

        //Set the postion of both two lines
        m_cLineRenderer.SetPosition(0, startPos);
        m_cLineRenderer.SetPosition(1, endPos);
    }

    /**
    * \fn OnDisable 
    * \brief Remove the line renderer component.
    * 
    * 
    **/
    public void OnDisable()
    {
        if (m_cLineRenderer != null)
            Object.Destroy(m_cLineRenderer.gameObject);
    }
}
