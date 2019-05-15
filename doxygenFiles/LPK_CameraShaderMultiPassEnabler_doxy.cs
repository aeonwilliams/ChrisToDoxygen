/***************************************************
\file           LPK_CameraShaderMultiPassEnabler.cs
\author        Christopher Onorati
\date   1/25/2019
\version   2.17

\brief
  Allows the user to apply a effect on a camera renderer.
  The user can apply this effect multiple times before
  the render is made.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_CameraShaderMultiPassEnabler
* \brief Component to enable post processing effects from
*               a single camera with multiple draw passes.
**/
[RequireComponent(typeof(Camera))]
public class LPK_CameraShaderMultiPassEnabler : LPK_LogicBase
{
    /************************************************************************************/

    public enum ToggleType
    {
        ON,
        OFF,
        TOGGLE,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Start the camera with this shader effect active.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How this shader manager handles detecting input.")]
    [Rename("Toggle Type")]
    public ToggleType m_eToggleType = ToggleType.ON;

    [Tooltip("Shader to apply to the camera.  For multiple effects just add more of this component.")]
    [Rename("Shader Material")]
    public Material m_ShaderMat;

    [Tooltip("How many passes of this shader to make before rendering the screen.")]
    [Range(0, 10)]
    public int m_Iterations;

    [Tooltip("How many times to scale the resolution of the blurred image down.  This will help with performance.")]
    [Range(0, 4)]
    public int m_ResolutionScale;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for shader toggling.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnRenderImage
    * \brief Sets up which shaders to apply to a rendering camera.
    * \param src - Source image to modify before rendering.
    *                dst - Destination of the render (usually the screen).
    * 
    **/
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (m_bActive && m_ShaderMat != null)
        {
            //Downscale the camera resoluton.
            int width = src.width >> m_ResolutionScale;
            int height = src.height >> m_ResolutionScale;

            //Store each pass of the shader render here to apply after all iterations.
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(src, rt);

            //Perform the shader for however many passes specified.
            for (int i = 0; i < m_Iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(width, height);
                Graphics.Blit(rt, rt2, m_ShaderMat);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            Graphics.Blit(rt, dst);
            RenderTexture.ReleaseTemporary(rt);
        }
        else
            Graphics.Blit(src, dst);
    }

    /**
    * \fn OnEvent
    * \brief Manages active state of effect.
    * \param data - Event data to parse to determine triggering.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Shader effect enabled.");

        if (m_eToggleType == ToggleType.ON)
            m_bActive = true;
        else if (m_eToggleType == ToggleType.OFF)
            m_bActive = false;
        else if (m_eToggleType == ToggleType.TOGGLE)
            m_bActive = !m_bActive;

    }
}
