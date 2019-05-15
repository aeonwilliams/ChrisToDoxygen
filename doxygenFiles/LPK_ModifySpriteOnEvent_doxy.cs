/***************************************************
\file           LPK_ModifySpriteOnEvent.cs
\author        Christopher Onorati
\date   12/21/2018
\version   2.17

\brief
  This component can be added to any object to cause
  it to modify an object's Sprite properties upon
  receiving an event.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; /* Image */

/**
* \class LPK_ModifySpriteOnEvent
* \brief Component used to modify the values of a sprite.
**/
public class LPK_ModifySpriteOnEvent : LPK_LogicBase
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

    [Tooltip("Object to modify sprite and color of when the specified event is received.  If not set, assume self.")]
    [Rename("Target Modify Object")]
    public GameObject m_pTargetModifyObject;

    [System.Serializable]
    public class VisibleProperties
    {
        [Tooltip("How to modify visibility upon recving the event.")]
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
    public class TextureProperties
    {
        [Tooltip("How to modify texture upon receiving the event.")]
        [Rename("Texture Mode")]
        public LPK_NonNumericModifyMode m_eTextureModifyMode;

        [Tooltip("New value to set visible flag to be.")]
        [Rename("New Texture")]
        public Sprite m_TextureValue;

        [Tooltip("Object whose texture will be copied to the recepient's property value. Only used if mode is set to copy.")]
        [Rename("Texture Copy Target")]
        public GameObject m_pTextureCopyTarget;
    }

    public TextureProperties m_TextureProperties;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /**
    * \fn Start
    * \brief Sets up what event to listen to for sprite and color modification.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_pTargetModifyObject == null)
            m_pTargetModifyObject = gameObject;
    }

    /**
    * \fn OnEvent
    * \brief Sets the new sprite values.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Incorrect object.
        if (!ShouldRespondToEvent(data))
            return;

        //Verify whether the target is valid
        if (m_pTargetModifyObject == null || (m_pTargetModifyObject.GetComponent<SpriteRenderer>() == null
            && m_pTargetModifyObject.GetComponent<Image>() == null))
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Target not found");

            return;
        }

        if (GetComponent<SpriteRenderer>() != null)
            ModifySprite();
        else if (GetComponent<Image>() != null)
            ModifyImage();
    }

    /**
    * \fn ModifySprite
    * \brief Modify the display of a sprite.
    * 
    * 
    **/
    void ModifySprite()
    {
        SpriteRenderer modSprite = m_pTargetModifyObject.GetComponent<SpriteRenderer>();

        //Modify the visible flag property based on the mode selected
        if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.enabled = m_VisibleProperties.m_bVisible;
        else if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VisibleProperties.m_pVisibleCopyTarget != null && m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<SpriteRenderer>() != null)
                modSprite.enabled = m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<SpriteRenderer>().enabled;
        }

        //Modify the Color property based on the mode selected
        if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.color = m_VertexColorProperties.m_vecColorValue;
        else if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VertexColorProperties.m_pVertexColorCopyTarget != null && m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<SpriteRenderer>() != null)
                modSprite.color = m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<SpriteRenderer>().color;
        }

        //Modify the texture property based on the mode selected
        if (m_TextureProperties.m_eTextureModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.sprite = m_TextureProperties.m_TextureValue;
        else if (m_TextureProperties.m_eTextureModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_TextureProperties.m_pTextureCopyTarget != null && m_TextureProperties.m_pTextureCopyTarget.GetComponent<SpriteRenderer>() != null)
                modSprite.sprite = m_TextureProperties.m_pTextureCopyTarget.GetComponent<SpriteRenderer>().sprite;
        }
    }

    /**
    * \fn ModifyImage
    * \brief Modify the display of a UI image.
    * 
    * 
    **/
    void ModifyImage()
    {
        Image modSprite = m_pTargetModifyObject.GetComponent<Image>();

        //Modify the visible flag property based on the mode selected
        if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.enabled = m_VisibleProperties.m_bVisible;
        else if (m_VisibleProperties.m_eVisibleModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VisibleProperties.m_pVisibleCopyTarget != null && m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<Image>() != null)
                modSprite.enabled = m_VisibleProperties.m_pVisibleCopyTarget.GetComponent<Image>().enabled;
        }

        //Modify the Color property based on the mode selected
        if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.color = m_VertexColorProperties.m_vecColorValue;
        else if (m_VertexColorProperties.m_eVertexColorModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_VertexColorProperties.m_pVertexColorCopyTarget != null && m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<Image>() != null)
                modSprite.color = m_VertexColorProperties.m_pVertexColorCopyTarget.GetComponent<Image>().color;
        }

        //Modify the texture property based on the mode selected
        if (m_TextureProperties.m_eTextureModifyMode == LPK_NonNumericModifyMode.SET)
            modSprite.sprite = m_TextureProperties.m_TextureValue;
        else if (m_TextureProperties.m_eTextureModifyMode == LPK_NonNumericModifyMode.COPY)
        {
            if (m_TextureProperties.m_pTextureCopyTarget != null && m_TextureProperties.m_pTextureCopyTarget.GetComponent<Image>() != null)
                modSprite.sprite = m_TextureProperties.m_pTextureCopyTarget.GetComponent<Image>().sprite;
        }
    }
}
