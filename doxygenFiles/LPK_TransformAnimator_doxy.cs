/***************************************************
\file           LPK_TransformAnimator.cs
\author        Christopher Onorati
\date   2/19/2019
\version   2018.3.4

\brief
  This component can be used to animate the transform of
  a GameObject.  In a way, this is like the Zero Engine's
  Action animation system.  This is designed, however, to
  be usable without touching a single line of code.  For a
  more intricate system that is designed with
  self-programming in mind, refer to Joshua Bigg's U-EAT
  system on the Unity Asset Store.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_TransformAnimator
* \brief Component used to animate the transform of a GameObject.
**/
public class LPK_TransformAnimator : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_TransformMode
    {
        WORLD,
        LOCAL,
    };

    public enum LPK_LoopingStyle
    {
        PlAY_ONCE,
        LOOP,
        PINGPONG,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Start the activator active.")]
    [Rename("Active")]
    public bool m_bIsActive = true;

    [Tooltip("How to change the active state of the declared gameobject(s).")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("How the change in the transform component will be applied.")]
    [Rename("Transform Mode")]
    public LPK_TransformMode m_eTransformMode;

    [Tooltip("How to loop the animaiton once finished.")]
    [Rename("Looping Style")]
    public LPK_LoopingStyle m_eLoopingStyle;

    [Tooltip("Gameobject to change the enabled state of.  If not set assume self.")]
    [Rename("Modify Object")]
    public GameObject m_pModifyGameObject;

    [Tooltip("Keyframes to use for this transform animation.")]
    public LPK_TransformAnimationKeyFrame[] m_KeyFrames;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for when a keyframe is finished.  Note these receivers are sent events after EVERY keyframe.")]
    public LPK_EventReceivers m_KeyFrameFinishedReceivers;

    [Tooltip("Receiver Game Objects for when the sequence is finished.")]
    public LPK_EventReceivers m_SequenceFinishedReceivers;

    /************************************************************************************/

    //Stores initial properties of the previous keyframe for animation.
    LPK_TransformAnimationKeyFrame m_LastTransformValues = new LPK_TransformAnimationKeyFrame();

    //Stores the current properties of the desired keyframe for animation.
    LPK_TransformAnimationKeyFrame m_GoalTransformValues = new LPK_TransformAnimationKeyFrame();

    //Time passed from the previous animation frame to the current one.
    float m_flPassedTime = 0.0f;

    //Keeps track of the frame in the animation sequence.
    int m_iCounter = 0;

    //Counter modifier.  Only used in the PingPong looping style.
    int m_iCounterModifier = 1;

    /**
    * \fn Start
    * \brief Sets up what event to listen to for sprite and color modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        if (m_pModifyGameObject == null)
            m_pModifyGameObject = gameObject;

        if(m_KeyFrames.Length == 0)
        {
            m_bIsActive = false;

            if (m_bPrintDebug)
                LPK_PrintWarning(this, "No keyframes set in animator.  BUG THIS.");
            return;
        }

        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_eTransformMode == LPK_TransformMode.WORLD)
        {
            m_LastTransformValues.m_vecTranslate = m_pModifyGameObject.transform.position;
            m_LastTransformValues.m_vecRotate = m_pModifyGameObject.transform.eulerAngles;
            m_LastTransformValues.m_vecScale = m_pModifyGameObject.transform.localScale;
        }
        else
        {
            m_LastTransformValues.m_vecTranslate = m_pModifyGameObject.transform.localPosition ;
            m_LastTransformValues.m_vecRotate = m_pModifyGameObject.transform.localEulerAngles;
            m_LastTransformValues.m_vecScale = m_pModifyGameObject.transform.localScale;
        }

        m_GoalTransformValues = m_KeyFrames[0];
    }

    /**
    * \fn OnEvent
    * \brief Sets the active state of the animation.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        if (m_eToggleType == LPK_ToggleType.ON)
            m_bIsActive = true;
        else if (m_eToggleType == LPK_ToggleType.OFF)
            m_bIsActive = false;
        else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            m_bIsActive = !m_bIsActive;
    }

    /**
    * \fn OnUpdate
    * \brief Manages animation looping state and animate calling.
    * 
    * 
    **/
    protected override void OnUpdate()
    {
        if (!m_bIsActive)
            return;

        bool bShouldAnimate = true;

        //Animation sequence finished.  Less than zero check for the PINGPONG state.
        if (m_iCounter >= m_KeyFrames.Length || m_iCounter < 0)
            bShouldAnimate = AnimationFinished();

        if (bShouldAnimate)
            Animate();
    }

    /**
    * \fn Animate
    * \brief Manages animation of the transform.
    * 
    * 
    **/
    void Animate()
    {
        m_flPassedTime += Time.deltaTime;

        //Animation keyframe is done, set values.
        if (m_flPassedTime >= m_GoalTransformValues.m_flDuration)
        {
            if (m_eTransformMode == LPK_TransformMode.WORLD)
            {
                if(m_KeyFrames[m_iCounter].m_bModifyTranslation)
                    m_pModifyGameObject.transform.position = m_GoalTransformValues.m_vecTranslate;
                if(m_KeyFrames[m_iCounter].m_bModifyRotation)
                    m_pModifyGameObject.transform.eulerAngles = m_GoalTransformValues.m_vecRotate;
                if (m_KeyFrames[m_iCounter].m_bModifyScale)
                    m_pModifyGameObject.transform.localScale = m_GoalTransformValues.m_vecScale;
            }
            else
            {
                if (m_KeyFrames[m_iCounter].m_bModifyTranslation)
                    m_pModifyGameObject.transform.localPosition = m_GoalTransformValues.m_vecTranslate;
                if (m_KeyFrames[m_iCounter].m_bModifyRotation)
                    m_pModifyGameObject.transform.localEulerAngles = m_GoalTransformValues.m_vecRotate;
                if (m_KeyFrames[m_iCounter].m_bModifyScale)
                    m_pModifyGameObject.transform.localScale = m_GoalTransformValues.m_vecScale;
            }

            KeyFrameFinished();
            return;
        }

        //Play with set animation style.
        switch (m_GoalTransformValues.m_eAnimationStyle)
        {
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.LINEAR:
                LinearAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.QUAD_IN:
                QuadInAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.QUAD_OUT:
                QuadOutAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.QUAD_IN_OUT:
                QuadInOutAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.SIN_IN:
                SinInAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.SIN_OUT:
                SinOutAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.SIN_IN_OUT:
                SinInOutAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.EASE_IN:
                EaseInAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.EASE_OUT:
                EaseOutAnimate();
                break;
            case LPK_TransformAnimationKeyFrame.LPK_TransformAnimationStyle.EASE_IN_OUT:
                EaseInOutAnimate();
                break;
            default:
                LPK_PrintError(this, "Somehow a non-existant animation style was set.  Email c.onorati@digipen.edu if you did not modify this script.");
                break;
        }
    }

    /**
    * \fn LinearAnimate
    * \brief Animate the keyframe linearly.
    * 
    * 
    **/
    void LinearAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        vecTranslateVal += (m_GoalTransformValues.m_vecTranslate - vecTranslateVal) * m_flPassedTime/m_GoalTransformValues.m_flDuration;
        vecRotateVal += (m_GoalTransformValues.m_vecRotate - vecRotateVal) * m_flPassedTime / m_GoalTransformValues.m_flDuration;
        vecScaleVal += (m_GoalTransformValues.m_vecScale - vecScaleVal) * m_flPassedTime / m_GoalTransformValues.m_flDuration;

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn QuadInAnimate
    * \brief Animate the keyframe via QuadIn.
    * 
    * 
    **/
    void QuadInAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        float curTime = m_flPassedTime / m_GoalTransformValues.m_flDuration;

        vecTranslateVal += (m_GoalTransformValues.m_vecTranslate - vecTranslateVal) * curTime * curTime;
        vecRotateVal += (m_GoalTransformValues.m_vecRotate - vecRotateVal) * curTime * curTime;
        vecScaleVal += (m_GoalTransformValues.m_vecScale - vecScaleVal) * curTime * curTime;

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn QuadOutAnimate
    * \brief Animate the keyframe via QuadOut.
    * 
    * 
    **/
    void QuadOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        float curTime = m_flPassedTime / m_GoalTransformValues.m_flDuration;

        vecTranslateVal += (m_GoalTransformValues.m_vecTranslate - vecTranslateVal) * -1 * curTime * (curTime - 2);
        vecRotateVal += (m_GoalTransformValues.m_vecRotate - vecRotateVal) * -1 * curTime * (curTime - 2);
        vecScaleVal += (m_GoalTransformValues.m_vecScale - vecScaleVal) * -1 * curTime * (curTime - 2);

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn QuadInOutAnimate
    * \brief Animate the keyframe via QuadInOut.
    * 
    * 
    **/
    void QuadInOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        Vector3 vecTranslateChange = m_GoalTransformValues.m_vecTranslate - m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateChange = m_GoalTransformValues.m_vecRotate - m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleChange = m_GoalTransformValues.m_vecScale - m_LastTransformValues.m_vecScale;

        float flCurTime = m_flPassedTime / (m_GoalTransformValues.m_flDuration / 2.0f);

        if (flCurTime < 1)
        {
            vecTranslateVal += (vecTranslateChange / 2) * flCurTime * flCurTime;
            vecRotateVal += (vecRotateChange / 2) * flCurTime * flCurTime;
            vecScaleVal += (vecScaleChange / 2) * flCurTime * flCurTime;
        }
        else
        {
            flCurTime -= 1;

            vecTranslateVal += (vecTranslateChange * -1) / 2 * (flCurTime * (flCurTime * (flCurTime - 2)) - 1);
            vecRotateVal += (vecRotateChange * -1) / 2 * (flCurTime * (flCurTime * (flCurTime - 2)) - 1);
            vecScaleVal += (vecScaleChange * -1) / 2 * (flCurTime * (flCurTime * (flCurTime - 2)) - 1);
        }

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn SinInAnimate
    * \brief Animate the keyframe via SinIn.
    * 
    * 
    **/
    void SinInAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        float flCosPassedTime = Mathf.Cos(m_flPassedTime / m_GoalTransformValues.m_flDuration * (Mathf.PI / 2));

        vecTranslateVal += ((m_GoalTransformValues.m_vecTranslate - vecTranslateVal) * -1 * flCosPassedTime) + m_GoalTransformValues.m_vecTranslate - vecTranslateVal;
        vecRotateVal += ((m_GoalTransformValues.m_vecRotate - vecRotateVal) * -1 * flCosPassedTime) + m_GoalTransformValues.m_vecRotate - vecRotateVal;
        vecScaleVal += ((m_GoalTransformValues.m_vecScale - vecScaleVal) * -1 * flCosPassedTime) + m_GoalTransformValues.m_vecScale - vecScaleVal;

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn SinOutAnimate
    * \brief Animate the keyframe via SinOut.
    * 
    * 
    **/
    void SinOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        float flSinPassedTime = Mathf.Sin(m_flPassedTime / m_GoalTransformValues.m_flDuration * (Mathf.PI / 2));

        vecTranslateVal += ((m_GoalTransformValues.m_vecTranslate - vecTranslateVal) * flSinPassedTime);
        vecRotateVal += ((m_GoalTransformValues.m_vecRotate - vecRotateVal) * flSinPassedTime);
        vecScaleVal += ((m_GoalTransformValues.m_vecScale - vecScaleVal) * flSinPassedTime);

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn SinInOutAnimate
    * \brief Animate the keyframe via SinInOut.
    * 
    * 
    **/
    void SinInOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        Vector3 vecTranslateChange = (m_GoalTransformValues.m_vecTranslate - m_LastTransformValues.m_vecTranslate) * -0.5f;
        Vector3 vecRotateChange = (m_GoalTransformValues.m_vecRotate - m_LastTransformValues.m_vecRotate) * -0.5f;
        Vector3 vecScaleChange = (m_GoalTransformValues.m_vecScale - m_LastTransformValues.m_vecScale) * -0.5f;

        float flSinPassedTime = Mathf.Cos((Mathf.PI * m_flPassedTime) / m_GoalTransformValues.m_flDuration ) - 1;

        vecTranslateVal += flSinPassedTime * vecTranslateChange;
        vecRotateVal += flSinPassedTime * vecRotateChange;
        vecScaleVal += flSinPassedTime * vecScaleChange;

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn EaseInAnimate
    * \brief Animate the keyframe via exponential EaseIn.
    * 
    * 
    **/
    void EaseInAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        Vector3 vecTranslateChange = m_GoalTransformValues.m_vecTranslate - m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateChange = m_GoalTransformValues.m_vecRotate - m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleChange = m_GoalTransformValues.m_vecScale - m_LastTransformValues.m_vecScale;

        vecTranslateVal += (vecTranslateChange * Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) - 1 ));
        vecRotateVal += (vecRotateChange * Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) - 1));
        vecScaleVal += (vecScaleChange * Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) - 1));

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn EaseOutAnimate
    * \brief Animate the keyframe via exponential EaseOut.
    * 
    * 
    **/
    void EaseOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        Vector3 vecTranslateChange = m_GoalTransformValues.m_vecTranslate - m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateChange = m_GoalTransformValues.m_vecRotate - m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleChange = m_GoalTransformValues.m_vecScale - m_LastTransformValues.m_vecScale;

        vecTranslateVal += (vecTranslateChange * -Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) + 1));
        vecRotateVal += (vecRotateChange * -Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) + 1));
        vecScaleVal += (vecScaleChange * -Mathf.Pow(2, 10 * (m_flPassedTime / m_GoalTransformValues.m_flDuration) + 1));

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn EaseInOutAnimate
    * \brief Animate the keyframe via exponential EaseInOut.
    * 
    * 
    **/
    void EaseInOutAnimate()
    {
        Vector3 vecTranslateVal = m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateVal = m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleVal = m_LastTransformValues.m_vecScale;

        Vector3 vecTranslateChange = m_GoalTransformValues.m_vecTranslate - m_LastTransformValues.m_vecTranslate;
        Vector3 vecRotateChange = m_GoalTransformValues.m_vecRotate - m_LastTransformValues.m_vecRotate;
        Vector3 vecScaleChange = m_GoalTransformValues.m_vecScale - m_LastTransformValues.m_vecScale;

        float flCurTime = m_flPassedTime / (m_GoalTransformValues.m_flDuration / 2.0f);

        if (flCurTime < 1)
        {
            vecTranslateVal += (vecTranslateChange / 2 * Mathf.Pow(2, 10 * (flCurTime - 1)));
            vecRotateVal += (vecRotateChange / 2 * Mathf.Pow(2, 10 * (flCurTime - 1)));
            vecScaleVal += (vecScaleChange / 2 * Mathf.Pow(2, 10 * (flCurTime - 1)));
        }
        else
        {
            flCurTime -= 1;

            vecTranslateVal += (vecTranslateChange / 2 * -Mathf.Pow(2, 10 * (flCurTime + 2)));
            vecRotateVal += (vecRotateChange / 2 * -Mathf.Pow(2, 10 * (flCurTime + 2)));
            vecScaleVal += (vecScaleChange / 2 * -Mathf.Pow(2, 10 * (flCurTime + 2)));
        }

        SetTransformValues(vecTranslateVal, vecRotateVal, vecScaleVal);
    }

    /**
    * \fn SetTransformValues
    * \brief Set the values of the transform after doing interperlation for the frame.
    * \param translate - Translate value to set.
    *                rotate    - Rotation value to set.
    *                scale     - Scale value to set.
    * 
    **/
    void SetTransformValues(Vector3 translate, Vector3 rotate, Vector3 scale)
    {
        if (m_eTransformMode == LPK_TransformMode.WORLD)
        {
            if(m_KeyFrames[m_iCounter].m_bModifyTranslation)
                m_pModifyGameObject.transform.position = translate;
            if (m_KeyFrames[m_iCounter].m_bModifyTranslation)
                m_pModifyGameObject.transform.eulerAngles = rotate;
            if(m_KeyFrames[m_iCounter].m_bModifyScale)
                m_pModifyGameObject.transform.localScale = scale;
        }
        else
        {
            if (m_KeyFrames[m_iCounter].m_bModifyTranslation)
                m_pModifyGameObject.transform.localPosition = translate;
            if(m_KeyFrames[m_iCounter].m_bModifyRotation)
                m_pModifyGameObject.transform.localEulerAngles = rotate;
            if (m_KeyFrames[m_iCounter].m_bModifyScale)
                m_pModifyGameObject.transform.localScale = scale;
        }
    }

    /**
    * \fn KeyFrameFinished
    * \brief Updates what the last keyframe played in the animation was.
    * 
    * 
    **/
    void KeyFrameFinished()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Finished keyframe.");

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_KeyFrameFinishedReceivers);
        data.m_idata.Add(m_iCounter);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorKeyframeFinished };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        //Reset values.
        m_LastTransformValues = m_KeyFrames[m_iCounter];
        m_flPassedTime = 0.0f;
        m_iCounter += 1 * m_iCounterModifier;

        if (m_iCounter >= 0 && m_iCounter < m_KeyFrames.Length)
            m_GoalTransformValues = m_KeyFrames[m_iCounter];
    }

    /**
    * \fn AnimationFinished
    * \brief Manages how the animator responds if finished.
    * 
    * \return bool - True/False flag for whether or not the animator should animate again.
    **/
    bool AnimationFinished()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Finished animation sequence.");

        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_SequenceFinishedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TransformAnimatorSequenceFinished };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_eLoopingStyle == LPK_LoopingStyle.LOOP)
        {
            if (m_KeyFrames.Length < 2)
            {
                if (m_bPrintDebug)
                    LPK_PrintWarning(this, "Cannot have LOOPING animation if there is only one frame.  Changing to PLAY_ONCE.");

                m_eLoopingStyle = LPK_LoopingStyle.PlAY_ONCE;
                m_bIsActive = false;
                return false;
            }

            m_iCounter = 0;
            m_GoalTransformValues = m_KeyFrames[m_iCounter];
            return true;
        }
        else if (m_eLoopingStyle == LPK_LoopingStyle.PINGPONG)
        {
            if(m_KeyFrames.Length < 2)
            {
                if (m_bPrintDebug)
                    LPK_PrintWarning(this, "Cannot have PINGPONG animation if there is only one frame.  Changing to PLAY_ONCE.");

                m_eLoopingStyle = LPK_LoopingStyle.PlAY_ONCE;
                m_bIsActive = false;
                return false;
            }

            //Going backwards.
            if (m_iCounterModifier == 1)
                m_iCounter -= 1;
            //Going forwards.
            else
                m_iCounter += 1;

            m_iCounterModifier *= -1;

            m_LastTransformValues = m_KeyFrames[m_iCounter];

            //Set the next goal.
            m_iCounter += 1 * m_iCounterModifier;
            m_GoalTransformValues = m_KeyFrames[m_iCounter];

            return true;
        }
        else
            m_bIsActive = false;
            return false;
    }
}

/**
* \class LPK_TransformAnimationKeyFrame
* \brief Transform properties to modify.
**/
[System.Serializable]
public class LPK_TransformAnimationKeyFrame
{
    /************************************************************************************/

    public enum LPK_TransformAnimationStyle
    {
        LINEAR,
        QUAD_IN,
        QUAD_OUT,
        QUAD_IN_OUT,
        SIN_IN,
        SIN_OUT,
        SIN_IN_OUT,
        EASE_IN,
        EASE_OUT,
        EASE_IN_OUT,
    };

    /************************************************************************************/

    [Tooltip("Set to make this keyframe animate translation.")]
    [Rename("Animate Translation")]
    public bool m_bModifyTranslation = false;

    [Tooltip("Values to use when modifying the translation.")]
    [Rename("Translation Values")]
    public Vector3 m_vecTranslate;

    [Tooltip("Set to make this keyframe animate rotation.")]
    [Rename("Animate Rotation")]
    public bool m_bModifyRotation = false;

    [Tooltip("Values to use when modifying the rotation.")]
    [Rename("Rotation Values")]
    public Vector3 m_vecRotate;

    [Tooltip("Set to make this keyframe animate scale.")]
    [Rename("Animate Scale")]
    public bool m_bModifyScale = false;

    [Tooltip("Values to use when modifying the scale.")]
    [Rename("Scale Values")]
    public Vector3 m_vecScale;

    [Tooltip("How to animate the transform for this keyframe.")]
    [Rename("Animation Style")]
    public LPK_TransformAnimationStyle m_eAnimationStyle;

    [Tooltip("Duration of time (in seconds) to get to interperlate to the values in this keyframe.  As in, it will take X seconds to reach the values selected for this keyframe.")]
    [Rename("Duration")]
    public float m_flDuration = 1.0f;
}
