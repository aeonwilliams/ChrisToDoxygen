/***************************************************
\file           LPK_ModifyTransformOnEvent.cs
\author        Christopher Onorati
\date   2/28/2019
\version   2018.3.4

\brief
  This component can be added to any object to cause
  it to modify an object's Transform properties upon
  receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**
* \class LPK_ModifyTransformOnEvent
* \brief Component used to modify the values of a transform.
**/
public class LPK_ModifyTransformOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    /************************************************************************************/

    public enum LPK_TransformMode
    {
        WORLD,
        LOCAL,
    };

    public enum LPK_NumericModifyMode
    {
        NONE,
        SET,
        ADD,
        MULTIPLY,
        DIVIDE,
        COPY,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Object to modify transform of when the specified event is received.")]
    [Rename("Target Modify Object")]
    public GameObject m_pTargetModifyObject;

    [Tooltip("Object to modify transform of when the specified event is received.  Note this will only affect the first object with the tag found.")]
    [Rename("Target Modify Tag")]
    public string m_sTargetModifyTag;

    [Tooltip("How the change in the transform component will be applied.")]
    [Rename("Transform Mode")]
    public LPK_TransformMode m_eTransformMode;

    [Tooltip("Set to cause this component to set its modification on an object's spawn.  This component will still react to set events.")]
    [Rename("Modify On Spawn")]
    public bool m_bModifyOnSpawn;

    [System.Serializable]
    public class TranslateProperties
    {
        [Tooltip("How to modify the property upon receiving the event.")]
        [Rename("Translation Mode")]
        public LPK_NumericModifyMode m_eTranslationModifyMode;

        [Tooltip("New value to be used in the operation upon receiving the event.  If set to copy, this is used as an offset.")]
        [Rename("Translation Value")]
        public Vector3 m_vecTranslationValue;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Translation Copy Target")]
        public GameObject m_pTranslationCopyTarget;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.  Note this will only affect the first object with the tag found.")]
        [Rename("Translation Copy Tag")]
        public string m_sTranslationCopyTag;
    }

    public TranslateProperties m_TranslateProperties;

    [System.Serializable]
    public class ScaleProperties
    {
        [Tooltip("How to modify the property upon receiving the event.")]
        [Rename("Scale Mode")]
        public LPK_NumericModifyMode m_eScaleModifyMode;

        [Tooltip("New value to be used in the operation upon receiving the event.  If set to copy, this is used as an offset.")]
        [Rename("Scale Value")]
        public Vector3 m_vecScaleValue;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Scale Copy Target")]
        public GameObject m_pScaleCopyTarget;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.  Note this will only affect the first object with the tag found.")]
        [Rename("Scale Copy Tag")]
        public string m_sScaleCopyTag;
    }

    public ScaleProperties m_ScaleProperties;

    [System.Serializable]
    public class RotationProperties
    {
        [Tooltip("How to modify the property upon receiving the event.")]
        [Rename("Rotation Mode")]
        public LPK_NumericModifyMode m_eRotateModifyMode;

        [Tooltip("New value to be used in the operation upon receiving the event.  If set to copy, this is used as an offset.")]
        [Rename("Rotation Value")]
        public Vector3 m_vecRotateValue;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Rotation Copy Target")]
        public GameObject m_pRotateCopyTarget;

        [Tooltip("Object whose property value will be copied to the recepient's property value. Only used if mode is set to copy.  Note this will only affect the first object with the tag found.")]
        [Rename("Rotation Copy Tag")]
        public string m_sRotationCopyTag;
    }

    public RotationProperties m_RotationProperties;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for object transformation modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_pTargetModifyObject == null)
        {
            if (!string.IsNullOrEmpty(m_sTargetModifyTag))
                m_pTargetModifyObject = GameObject.FindWithTag(m_sTargetModifyTag);
            else
                m_pTargetModifyObject = gameObject;
        }

        SetCopyTargets();

        //Modify on spawn functionality.
        if(m_bModifyOnSpawn)
        {
            if (m_eTransformMode == LPK_TransformMode.WORLD)
                ModifyTransformWorld();
            else if (m_eTransformMode == LPK_TransformMode.LOCAL)
                ModifyTransformLocal();
        }
    }

    /**
    * \fn OnEvent
    * \brief Preforms change on transform of desired object.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Verify whether the target is valid
        if (m_pTargetModifyObject.transform == null)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Target not found");

            return;
        }

        SetCopyTargets();

        if (m_eTransformMode == LPK_TransformMode.WORLD)
            ModifyTransformWorld();
        else if (m_eTransformMode == LPK_TransformMode.LOCAL)
            ModifyTransformLocal();

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");
    }

    /**
    * \fn ModifyTransformWorld
    * \brief Preforms change on transform of desired object in world space.
    * 
    * 
    **/
    void ModifyTransformWorld()
    {
        //Modify the Translation property based on the mode selected
            if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.position = m_TranslateProperties.m_vecTranslationValue;
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.position += m_TranslateProperties.m_vecTranslationValue;
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.position = Vector3.Scale(m_pTargetModifyObject.transform.position, m_TranslateProperties.m_vecTranslationValue);
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.position = Vector3.Scale(m_pTargetModifyObject.transform.position,
                                                                     new Vector3(1 / m_TranslateProperties.m_vecTranslationValue.x,
                                                                     1 / m_TranslateProperties.m_vecTranslationValue.y,
                                                                     1 / m_TranslateProperties.m_vecTranslationValue.z));
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_TranslateProperties.m_pTranslationCopyTarget != null && m_TranslateProperties.m_pTranslationCopyTarget.transform != null)
                m_pTargetModifyObject.transform.position = m_TranslateProperties.m_pTranslationCopyTarget.transform.position + m_TranslateProperties.m_vecTranslationValue;
        }

        //Modify the Scale property based on the mode selected
        if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.localScale = m_ScaleProperties.m_vecScaleValue;
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.localScale += m_ScaleProperties.m_vecScaleValue;
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.localScale = Vector3.Scale(m_pTargetModifyObject.transform.localScale, m_ScaleProperties.m_vecScaleValue);
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.localScale = Vector3.Scale(m_pTargetModifyObject.transform.localScale,
                                                                      new Vector3(1 / m_ScaleProperties.m_vecScaleValue.x,
                                                                      1 / m_ScaleProperties.m_vecScaleValue.y,
                                                                      1 / m_ScaleProperties.m_vecScaleValue.z));
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_ScaleProperties.m_pScaleCopyTarget != null && m_ScaleProperties.m_pScaleCopyTarget.transform != null)
                m_pTargetModifyObject.transform.localScale = m_ScaleProperties.m_pScaleCopyTarget.transform.localScale + m_ScaleProperties.m_vecScaleValue;
        }

        //Modify the Rotation property based on the mode selected
        if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.eulerAngles = m_RotationProperties.m_vecRotateValue;
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.eulerAngles += m_RotationProperties.m_vecRotateValue;
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.eulerAngles = Vector3.Scale(m_pTargetModifyObject.transform.eulerAngles, m_RotationProperties.m_vecRotateValue);
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.eulerAngles = Vector3.Scale(m_pTargetModifyObject.transform.eulerAngles,
                                                                      new Vector3(1 / m_RotationProperties.m_vecRotateValue.x,
                                                                      1 / m_RotationProperties.m_vecRotateValue.y,
                                                                      1 / m_RotationProperties.m_vecRotateValue.z));
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_RotationProperties.m_pRotateCopyTarget != null && m_RotationProperties.m_pRotateCopyTarget.transform != null)
                m_pTargetModifyObject.transform.eulerAngles = m_RotationProperties.m_pRotateCopyTarget.transform.eulerAngles + m_RotationProperties.m_vecRotateValue;
        }
    }

    /**
    * \fn ModifyTransformLocal
    * \brief Preforms change on transform of desired object in local space.
    * 
    * 
    **/
    void ModifyTransformLocal()
    {
        //Modify the Translation property based on the mode selected
        if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.localPosition = m_TranslateProperties.m_vecTranslationValue;
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.localPosition += m_TranslateProperties.m_vecTranslationValue;
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.localPosition = Vector3.Scale(m_pTargetModifyObject.transform.localPosition, m_TranslateProperties.m_vecTranslationValue);
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.localPosition = Vector3.Scale(m_pTargetModifyObject.transform.localPosition,
                                                                     new Vector3(1 / m_TranslateProperties.m_vecTranslationValue.x,
                                                                     1 / m_TranslateProperties.m_vecTranslationValue.y,
                                                                     1 / m_TranslateProperties.m_vecTranslationValue.z));
        else if (m_TranslateProperties.m_eTranslationModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_TranslateProperties.m_pTranslationCopyTarget != null && m_TranslateProperties.m_pTranslationCopyTarget.transform != null)
                m_pTargetModifyObject.transform.localPosition = m_TranslateProperties.m_pTranslationCopyTarget.transform.position + m_TranslateProperties.m_vecTranslationValue;

            if (m_bPrintDebug && m_TranslateProperties.m_pTranslationCopyTarget == null)
                LPK_PrintWarning(this, "Cannot find a transform copy target.");
        }

        //Modify the Scale property based on the mode selected
        if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.localScale = m_ScaleProperties.m_vecScaleValue;
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.localScale += m_ScaleProperties.m_vecScaleValue;
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.localScale = Vector3.Scale(m_pTargetModifyObject.transform.localScale, m_ScaleProperties.m_vecScaleValue);
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.localScale = Vector3.Scale(m_pTargetModifyObject.transform.localScale,
                                                                      new Vector3(1 / m_ScaleProperties.m_vecScaleValue.x,
                                                                      1 / m_ScaleProperties.m_vecScaleValue.y,
                                                                      1 / m_ScaleProperties.m_vecScaleValue.z));
        else if (m_ScaleProperties.m_eScaleModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_ScaleProperties.m_pScaleCopyTarget != null && m_ScaleProperties.m_pScaleCopyTarget.transform != null)
                m_pTargetModifyObject.transform.localScale = m_ScaleProperties.m_pScaleCopyTarget.transform.localScale + m_ScaleProperties.m_vecScaleValue;

            if (m_bPrintDebug && m_ScaleProperties.m_pScaleCopyTarget == null)
                LPK_PrintWarning(this, "Cannot find a scale copy target.");
        }

        //Modify the Rotation property based on the mode selected
        if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.SET)
            m_pTargetModifyObject.transform.localEulerAngles = m_RotationProperties.m_vecRotateValue;
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.ADD)
            m_pTargetModifyObject.transform.localEulerAngles += m_RotationProperties.m_vecRotateValue;
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.MULTIPLY)
            m_pTargetModifyObject.transform.localEulerAngles = Vector3.Scale(m_pTargetModifyObject.transform.localEulerAngles, m_RotationProperties.m_vecRotateValue);
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.DIVIDE)
            m_pTargetModifyObject.transform.localEulerAngles = Vector3.Scale(m_pTargetModifyObject.transform.localEulerAngles,
                                                                      new Vector3(1 / m_RotationProperties.m_vecRotateValue.x,
                                                                      1 / m_RotationProperties.m_vecRotateValue.y,
                                                                      1 / m_RotationProperties.m_vecRotateValue.z));
        else if (m_RotationProperties.m_eRotateModifyMode == LPK_NumericModifyMode.COPY)
        {
            if (m_RotationProperties.m_pRotateCopyTarget != null && m_RotationProperties.m_pRotateCopyTarget.transform != null)
                m_pTargetModifyObject.transform.localEulerAngles = m_RotationProperties.m_pRotateCopyTarget.transform.eulerAngles + m_RotationProperties.m_vecRotateValue;

            if (m_bPrintDebug && m_RotationProperties.m_pRotateCopyTarget == null)
                LPK_PrintWarning(this, "Cannot find a rotate copy target.");
        }
    }

    /**
    * \fn SetCopyTargets
    * \brief Sets targets to copy.
    * 
    * 
    **/
    void SetCopyTargets()
    {
        if (m_TranslateProperties.m_pTranslationCopyTarget == null)
        {
            if (!string.IsNullOrEmpty(m_TranslateProperties.m_sTranslationCopyTag))
                m_TranslateProperties.m_pTranslationCopyTarget = GameObject.FindWithTag(m_TranslateProperties.m_sTranslationCopyTag);
            else if (m_TranslateProperties.m_pTranslationCopyTarget == null)
                m_TranslateProperties.m_pTranslationCopyTarget = gameObject;
        }

        if (m_ScaleProperties.m_pScaleCopyTarget == null)
        {
            if (!string.IsNullOrEmpty(m_ScaleProperties.m_sScaleCopyTag))
                m_ScaleProperties.m_pScaleCopyTarget = GameObject.FindWithTag(m_ScaleProperties.m_sScaleCopyTag);
            else if (m_ScaleProperties.m_pScaleCopyTarget == null)
                m_ScaleProperties.m_pScaleCopyTarget = gameObject;
        }

        if (m_RotationProperties.m_pRotateCopyTarget == null)
        {
            if (!string.IsNullOrEmpty(m_RotationProperties.m_sRotationCopyTag))
                m_RotationProperties.m_pRotateCopyTarget = GameObject.FindWithTag(m_RotationProperties.m_sRotationCopyTag);
            else if (m_RotationProperties.m_pRotateCopyTarget == null)
                m_RotationProperties.m_pRotateCopyTarget = gameObject;
        }
    }
}
