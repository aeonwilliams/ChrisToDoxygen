/***************************************************
\file           LPK_ModifyTextOnEvent.cs
\author        Christopher Onorati
\date   12/21/2018
\version   2.17

\brief
  This component can be added to any object to cause a 
  specified object's SpriteText component's its Visible, 
  Font, FontSize, VertexColor and Text properties to 
  change upon receiving a specified event

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; /* Text */

/**
* \class LPK_ModifySpriteOnEvent
* \brief Component used to modify the values of text.
**/
public class LPK_ModifyTextOnEvent : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_NonNumericModifyMode
    {
        NONE,
        SET,
        COPY,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Object to modify text of when the specified event is received.")]
    [Rename("Target Modify Object")]
    public GameObject m_pTargetModifyObject;

    [System.Serializable]
    public class VisibleProperties
    {
        [Tooltip("How to modify visibility upon receiving the event.")]
        [Rename("Visible Mode")]
        public LPK_NonNumericModifyMode m_eVisibleModifyMode;

        [Tooltip("New value to set visible flag to be.")]
        [Rename("Visible")]
        public bool m_bVisible;

        [Tooltip("Object whose visible flag will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Visible Copy Target")]
        public GameObject m_pVisibleCopyTarget;
    }

    public VisibleProperties m_VisibleProperties;

    [System.Serializable]
    public class FontProperties
    {
        [Tooltip("How to modify the font upon receiving the event.")]
        [Rename("Font Mode")]
        public LPK_NonNumericModifyMode m_eFontModifyMode;

        [Tooltip("New font to use on the text.")]
        [Rename("Font")]
        public Font m_Font;

        [Tooltip("Object whose font will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Font Copy Target")]
        public GameObject m_pFontCopyTarget;
    }

    public FontProperties m_FontProperties;

    [System.Serializable]
    public class FontSizeProperties
    {
        [Tooltip("How to modify the font size upon receiving the event.")]
        [Rename("Font Mode")]
        public LPK_NonNumericModifyMode m_eFontSizeModifyMode;

        [Tooltip("New font size for the text.")]
        [Rename("Font Size")]
        public int m_iFontSize;

        [Tooltip("Object whose font size will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Font Size Copy Target")]
        public GameObject m_pFontSizeCopyTarget;
    }

    public FontSizeProperties m_FontSizeProperties;

    [System.Serializable]
    public class ColorProperties
    {
        [Tooltip("How to modify color upon receiving the event.")]
        [Rename("Color Mode")]
        public LPK_NonNumericModifyMode m_eVertexColorModifyMode;

        [Tooltip("New value to set color to be.")]
        [Rename("Color Value")]
        public Color m_vecColorValue;

        [Tooltip("Object whose color will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Color Copy Target")]
        public GameObject m_pVertexColorCopyTarget;
    }

    public ColorProperties m_VertexColorProperties;

    [System.Serializable]
    public class TextProperties
    {
        [Tooltip("How to modify text upon receiving the event.")]
        [Rename("Text Mode")]
        public LPK_NonNumericModifyMode m_eTextModifyMode;

        [Tooltip("New text to display.")]
        [Rename("Text")]
        public string m_sText;

        [Tooltip("Object whose text will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Color Copy Target")]
        public GameObject m_pTextCopyTarget;
    }

    public TextProperties m_TextProperties;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for text modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnEvent
    * \brief Sets the new text values.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Verify whether the target is valid
        if (m_pTargetModifyObject == null || ( m_pTargetModifyObject.GetComponent<TextMesh>() == null
            && m_pTargetModifyObject.GetComponent<Text>() == null))
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Target not found");

            return;
        }

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");

        if (m_pTargetModifyObject.GetComponent<TextMesh>() != null)
            ModifyGameText();
        else if (m_pTargetModifyObject.GetComponent<Text>() != null)
            ModifyUIText();
    }

    /**
    * \fn ModifyGameText
    * \brief Modify text that is present in the scene.
    * 
    * 
    **/
    void ModifyGameText()
    {
        TextMesh modText = m_pTargetModifyObject.GetComponent<TextMesh>();

        //Modify the Visible property based on the mode selected
        if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.SET)
            m_pTargetModifyObject.GetComponent<MeshRenderer>().enabled = m_VisibleProperties.m_bVisible;
        else if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VisibleProperties.m_pVisibleCopyTarget != null && m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<MeshRenderer>() != null)
                m_pTargetModifyObject.GetComponent<MeshRenderer>().enabled = m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<MeshRenderer>().enabled;
        }

        //Modify the Font property based on the mode selected
        if (m_FontProperties.m_eFontModifyMode == LPK_NonNumericModifyMode.SET)
            modText.font = m_FontProperties.m_Font;
        else if (m_FontProperties.m_eFontModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_FontProperties.m_pFontCopyTarget != null && m_FontProperties.m_pFontCopyTarget.GetComponent<TextMesh>() != null)
                modText.font = m_FontProperties.m_pFontCopyTarget.GetComponent<TextMesh>().font;
        }

        //Modify the FontSize property based on the mode selected
        if (m_FontSizeProperties.m_eFontSizeModifyMode == LPK_NonNumericModifyMode.SET)
            modText.fontSize = m_FontSizeProperties.m_iFontSize;
        else if (m_FontSizeProperties.m_eFontSizeModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_FontSizeProperties.m_pFontSizeCopyTarget != null && m_FontSizeProperties.m_pFontSizeCopyTarget.GetComponent<TextMesh>() != null)
                modText.fontSize = m_FontSizeProperties.m_pFontSizeCopyTarget.GetComponent<TextMesh>().fontSize;
        }

        //Modify the Color property based on the mode selected
        if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.SET)
            modText.color = m_VertexColorProperties.m_vecColorValue;
        else if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VertexColorProperties.m_pVertexColorCopyTarget != null && m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<TextMesh>() != null)
                modText.color = m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<TextMesh>().color;
        }

        //Modify the Text property based on the mode selected
        if (m_TextProperties.m_eTextModifyMode == LPK_NonNumericModifyMode.SET)
            modText.text = m_TextProperties.m_sText;
        else if (m_TextProperties.m_eTextModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_TextProperties.m_pTextCopyTarget != null && m_TextProperties.m_pTextCopyTarget.GetComponent<TextMesh>() != null)
                modText.text = m_TextProperties.m_pTextCopyTarget.GetComponent<TextMesh>().text;
        }
    }

    /**
    * \fn ModifyUIText
    * \brief Modify text that is present in a canvas.
    * 
    * 
    **/
    void ModifyUIText()
    {
        Text modText = m_pTargetModifyObject.GetComponent<Text>();

        //Modify the Visible property based on the mode selected
        if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.SET)
            modText.enabled = m_VisibleProperties.m_bVisible;
        else if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_FontProperties.m_pFontCopyTarget != null && m_FontProperties.m_pFontCopyTarget.GetComponent<Text>() != null)
                modText.enabled = m_FontProperties.m_pFontCopyTarget.GetComponent<Text>().enabled;
        }

        //Modify the Font property based on the mode selected
        if (m_FontProperties.m_eFontModifyMode == LPK_NonNumericModifyMode.SET)
            modText.font = m_FontProperties.m_Font;
        else if (m_FontProperties.m_eFontModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_FontProperties.m_pFontCopyTarget != null && m_FontProperties.m_pFontCopyTarget.GetComponent<Text>() != null)
                modText.font = m_FontProperties.m_pFontCopyTarget.GetComponent<Text>().font;
        }

        //Modify the FontSize property based on the mode selected
        if (m_FontSizeProperties.m_eFontSizeModifyMode == LPK_NonNumericModifyMode.SET)
            modText.fontSize = m_FontSizeProperties.m_iFontSize;
        else if (m_FontSizeProperties.m_eFontSizeModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_FontSizeProperties.m_pFontSizeCopyTarget != null && m_FontSizeProperties.m_pFontSizeCopyTarget.GetComponent<Text>() != null)
                modText.fontSize = m_FontSizeProperties.m_pFontSizeCopyTarget.GetComponent<Text>().fontSize;
        }

        //Modify the Color property based on the mode selected
        if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.SET)
            modText.color = m_VertexColorProperties.m_vecColorValue;
        else if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VertexColorProperties.m_pVertexColorCopyTarget != null && m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<Text>() != null)
                modText.color = m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<Text>().color;
        }

        //Modify the Text property based on the mode selected
        if (m_TextProperties.m_eTextModifyMode == LPK_NonNumericModifyMode.SET)
            modText.text = m_TextProperties.m_sText;
        else if (m_TextProperties.m_eTextModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_TextProperties.m_pTextCopyTarget != null && m_TextProperties.m_pTextCopyTarget.GetComponent<Text>() != null)
                modText.text = m_TextProperties.m_pTextCopyTarget.GetComponent<Text>().text;
        }
    }
}
