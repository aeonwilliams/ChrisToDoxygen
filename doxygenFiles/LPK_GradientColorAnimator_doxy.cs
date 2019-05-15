/***************************************************
\file           LPK_GradientColorAnimator.cs
\author        Christopher Onorati
\date   1/24/2019
\version   2.17

\brief
  This component can be used to animate color based on a 
  given gradient.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   /* Image */

/**
* \class LPK_GradientColorAnimator
* \brief Component used to animate the color of a sprite or text.
**/
public class LPK_GradientColorAnimator : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_GradientAnimationPlayMode
    {
        PlAY_ONCE,
        LOOP,
        PINGPONG,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Set to start animating on spawn.")]
    [Rename("Active")]
    public bool m_bActive = true;

    [Tooltip("The gradient colors will be sampled from.")]
    [Rename("Gradient")]
    public Gradient m_Gradient;

    [System.Serializable]
    public class RendererProperties
    {
        [Tooltip("SpriteRenderer to modify the color of.  If left null, and this object has a SpriteRenderer component, assume self.")]
        [Rename("Modify SpriteRenderer")]
        public SpriteRenderer m_cRenderer;

        [Tooltip("TextMesh to modify the color of.  If left null, and this object has a TextMesh component, assume self.")]
        [Rename("Modify TextMesh")]
        public TextMesh m_cTextMesh;

        [Tooltip("UI Image to modify the color of.  If left null, and this object has an Image component, assume self.")]
        [Rename("Modify Image")]
        public Image m_cImage;
    }

    public RendererProperties m_RendererProperties;

    [Tooltip("What animation mode to use.")]
    [Rename("Mode")]
    public LPK_GradientAnimationPlayMode m_eMode;

    [Tooltip("If set, even if another event is received, the gradient will continue on as if nothing happened.")]
    [Rename("Never Restart")]
    public bool m_bNeverRestart = false;

    [Tooltip("How long is one animation cycle (in seconds).")]
    [Rename("Duration")]
    public float m_flDuration = 2.0f;

    [Tooltip("How long to wait until the animation begins.")]
    [Rename("Delay")]
    public float m_flDelay = 0.0f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Event receivers for when the gradient finishes its animation.")]
    public LPK_EventReceivers m_GradientFinishedReceivers = new LPK_EventReceivers();

    /************************************************************************************/

    //Internal Timer
    float m_flTimer = 0.0f;

    /**
    * \fn OnStart
    * \brief Begins intiial delay before animating.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (!m_RendererProperties.m_cRenderer && GetComponent<SpriteRenderer>())
        {
            m_RendererProperties.m_cRenderer = GetComponent<SpriteRenderer>();
        }

        if (!m_RendererProperties.m_cTextMesh && GetComponent<TextMesh>())
            m_RendererProperties.m_cTextMesh = GetComponent<TextMesh>();

        if (!m_RendererProperties.m_cImage && GetComponent<Image>())
            m_RendererProperties.m_cImage = GetComponent<Image>();

        if (!m_bActive)
            return;

        StartCoroutine(DelayTimer());
    }

    /**
    * \fn OnEvent
    * \brief Sets the gradient animation as active.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        m_bActive = true;

        if(!m_bNeverRestart)
            m_flTimer = 0.0f;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Color Gradient Active");
    }

    /**
    * \fn OnUpdate
    * \brief Manages color animation.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if(m_Gradient == null)
          return;
    
        //Increment timer
        m_flTimer += Time.deltaTime;
    
        //Reset Timer according to animation type
        if(m_flTimer >= m_flDuration)
        {
            if (m_eMode == LPK_GradientAnimationPlayMode.LOOP)
                m_flTimer = 0;
            else if (m_eMode == LPK_GradientAnimationPlayMode.PlAY_ONCE)
            {
                m_flTimer = m_flDuration;
                m_bActive = false;
            }
            else if (m_eMode == LPK_GradientAnimationPlayMode.PINGPONG)
                m_flTimer = -m_flDuration;

            DispatchGradientFinishedEvent();
        }

        //Set the color
        if (m_RendererProperties.m_cRenderer != null)
        {

            m_RendererProperties.m_cRenderer.color = m_Gradient.Evaluate(Mathf.Abs(m_flTimer) / m_flDuration);
                }
        else if (m_RendererProperties.m_cTextMesh != null)
            m_RendererProperties.m_cTextMesh.color = m_Gradient.Evaluate(Mathf.Abs(m_flTimer) / m_flDuration);
        else if (m_RendererProperties.m_cImage != null)
            m_RendererProperties.m_cImage.color = m_Gradient.Evaluate(Mathf.Abs(m_flTimer / m_flDuration));
    }

    /**
    * \fn DelayTimer
    * \brief Forces initial delay before animating.
    * 
    * 
    **/
    IEnumerator DelayTimer()
    {
        yield return new WaitForSeconds(m_flDelay);
        m_bActive = true;
    }

    /**
    * \fn DispatchGradientFinishedEvent
    * \brief Send out event when a gradient finishes an animation cycle.
    * 
    * 
    **/
    void DispatchGradientFinishedEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_GradientFinishedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_GradientAnimationFinished };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if(m_bPrintDebug)
            LPK_PrintDebug(this, "Event dispatched");
    }
}
