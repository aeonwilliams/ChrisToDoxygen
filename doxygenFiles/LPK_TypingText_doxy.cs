/***************************************************
\file           LPK_TypingText.cs
\author        Christopher Onorati
\date   12/21/2018
\version   2.17

\brief
  This component creates a typing text effect for an animated
  display.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; /* Text. */

/**
* \class LPK_TypingText
* \brief Replaces the standard static textfields with dynamic typing textfields.
**/
public class LPK_TypingText : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_TypeType
    {
        TYPE,
        CHARACTER_SCROLL,
    }

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Text to type out.")]
    [Rename("Text")]
    public string m_sText;

    [Tooltip("Start the typing effect on spawn of object.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("What type of animation to play for typing text out.")]
    [Rename("Type Mode")]
    public LPK_TypeType m_eTypeMode = LPK_TypeType.TYPE;

    [Tooltip("How fast each concurent character is typed.")]
    [Rename("Type Speed")]
    public float m_flSpeed = 0.15f;

    [Tooltip("How long to pause for commas.")]
    [Rename("Comma Pause Time")]
    public float m_flCommaPauseTime = 0.05f;

    [Tooltip("How long to pause for periods, question marks, colons, and exclamation points.")]
    [Rename("Punctuation Pause Time")]
    public float m_flPunctuationPauseTime = 0.15f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for when a new character is typed.")]
    public LPK_EventReceivers m_TypingUpdateReceivers;

    [Tooltip("Receiver Game Objects for typing completed.")]
    public LPK_EventReceivers m_TypingCompeltedReceivers;

    /************************************************************************************/

    //Variable to hold what we have already typed.
    string m_sPreviosulyTyped;
    //Array to hold the typed text in.
    List<string> m_aCharacters = new List<string>();
    //Used in the update loop
    int m_iCounter = 0;
    //Delay used for typing.
    float m_flDelay = 0;

    //Last attempted character to type.  Used for CHARACTER_SCROLL
    char m_sLastChar = '0';

    /************************************************************************************/
    TextMesh m_cTextMesh;
    Text m_cText;

    /**
    * \fn OnStart
    * \brief Splits up text into an indivitualized array, and set up
    *                event detection if appropriate.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cTextMesh = GetComponent<TextMesh>();
        m_cText = GetComponent<Text>();

        for (int i = 0; i < m_sText.Length; i++)
            m_aCharacters.Add(System.Convert.ToString(m_sText[i]));

        InitializeEvent(m_EventTrigger, OnEvent);
    }

    /**
    * \fn OnUpdate
    * \brief Manages selection of the typing animation.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (!m_bActive)
            return;

        if (m_eTypeMode == LPK_TypeType.TYPE)
        {
            if (m_cTextMesh != null)
                TypeText();
            else if (m_cText != null)
                TypeUIText();
        }
        else if (m_eTypeMode == LPK_TypeType.CHARACTER_SCROLL)
        {
            if (m_cTextMesh != null)
                CharacterScroll();
            else if (m_cText != null)
                UICharacterScroll();
        }
    }

    /**
    * \fn TypeText
    * \brief Plays typing animation for text display.
    * 
    * 
    **/
    void TypeText()
    {
        m_flDelay = m_flSpeed;

        //Extra delay for a comma.
        if (m_aCharacters[m_iCounter] == "," || m_aCharacters[m_iCounter] == ";")
            m_flDelay += m_flCommaPauseTime;

        //Extra delay for end of sentence quotations.
        else if (m_aCharacters[m_iCounter] == "." || m_aCharacters[m_iCounter] == "?" || m_aCharacters[m_iCounter] == "!" || m_aCharacters[m_iCounter] == ":")
            m_flDelay += m_flPunctuationPauseTime;

        //Notifying the owner that a letter has been typed.  Could be used for audio, for example.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TypingUpdateReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Typing effect update.");

        //Inputing the new text.
        m_cTextMesh.text = m_sPreviosulyTyped + m_aCharacters[m_iCounter];
        m_sPreviosulyTyped = m_cTextMesh.text;
        m_iCounter++;

        //Marks the text as finished.
        if (m_cTextMesh.text == m_sText)
            TypingFinished();

        //Typing the next letter.
        else
        {
            m_bActive = false;
            StartCoroutine(DelayType());
        }
    }

    /**
    * \fn CharacterScroll
    * \brief Plays typing animation for character scroll display.
    * 
    * 
    **/
    void CharacterScroll()
    {
        m_flDelay = m_flSpeed;

        //Extra delay for a comma.
        if (m_aCharacters[m_iCounter].ToCharArray()[0] >= ' ' && m_aCharacters[m_iCounter].ToCharArray()[0] <= '/')
            m_sLastChar = m_aCharacters[m_iCounter].ToCharArray()[0];

        //Inputing the new text.
        m_cTextMesh.text = m_sPreviosulyTyped + m_sLastChar;

        if (m_cTextMesh.text.Length > 0 && m_cTextMesh.text[m_cTextMesh.text.Length - 1] == m_aCharacters[m_iCounter].ToCharArray()[0])
        {
            //Notifying the owner that a letter has been typed.  Could be used for audio, for example.
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TypingUpdateReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Typing effect update.");

            m_iCounter++;
            m_sPreviosulyTyped = m_cTextMesh.text;
            m_sLastChar = '0';
        }

        //Marks the text as finished.
        if (m_cTextMesh.text == m_sText)
            TypingFinished();

        //Typing the next letter.
        else
        {
            m_bActive = false;
            m_sLastChar++;
            StartCoroutine(DelayType());
        }
    }

    /**
    * \fn TypeUIText
    * \brief Plays typing animation for UI text.
    * 
    * 
    **/
    void TypeUIText()
    {
        m_flDelay = m_flSpeed;

        //Extra delay for a comma.
        if (m_aCharacters[m_iCounter] == "," || m_aCharacters[m_iCounter] == ";")
            m_flDelay += m_flCommaPauseTime;

        //Extra delay for end of sentence quotations.
        else if (m_aCharacters[m_iCounter] == "." || m_aCharacters[m_iCounter] == "?" || m_aCharacters[m_iCounter] == "!" || m_aCharacters[m_iCounter] == ":")
            m_flDelay += m_flPunctuationPauseTime;

        //Notifying the owner that a letter has been typed.  Could be used for audio, for example.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TypingUpdateReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Typing effect update.");

        //Inputing the new text.
        m_cText.text = m_sPreviosulyTyped + m_aCharacters[m_iCounter];
        m_sPreviosulyTyped = m_cText.text;
        m_iCounter++;

        //Marks the text as finished.
        if (m_cText.text == m_sText)
            TypingFinished();

        //Typing the next letter.
        else
        {
            m_bActive = false;
            StartCoroutine(DelayType());
        }
    }

    /**
    * \fn UICharacterScroll
    * \brief Plays typing animation for UI character scroll display.
    * 
    * 
    **/
    void UICharacterScroll()
    {
        m_flDelay = m_flSpeed;

        //Extra delay for a comma.
        if (m_aCharacters[m_iCounter].ToCharArray()[0] >= ' ' && m_aCharacters[m_iCounter].ToCharArray()[0] <= '/')
            m_sLastChar = m_aCharacters[m_iCounter].ToCharArray()[0];

        //Inputing the new text.
        m_cText.text = m_sPreviosulyTyped + m_sLastChar;

        if (m_cText.text.Length > 0 && m_cText.text[m_cText.text.Length - 1] == m_aCharacters[m_iCounter].ToCharArray()[0])
        {
            //Notifying the owner that a letter has been typed.  Could be used for audio, for example.
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TypingUpdateReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextUpdate };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Typing effect update.");

            m_iCounter++;
            m_sPreviosulyTyped = m_cText.text;
            m_sLastChar = '0';
        }

        //Marks the text as finished.
        if (m_cText.text == m_sText)
            TypingFinished();

        //Typing the next letter.
        else
        {
            m_bActive = false;
            m_sLastChar++;
            StartCoroutine(DelayType());
        }
    }

    /**
    * \fn TypingFinished
    * \brief Disables effect and informs all receivers that this text is finished typing.
    * 
    * 
    **/
    void TypingFinished()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_TypingCompeltedReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_GameplayEventTrigger = new LPK_EventList.LPK_GAMEPLAY_EVENTS[] { LPK_EventList.LPK_GAMEPLAY_EVENTS.LPK_TypingTextComplete };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Typing effect finished.");

        m_bActive = false;
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
            LPK_PrintDebug(this, "Typing effect started.");

        m_bActive = true;
    }

    /**
    * \fn DelayTimer
    * \brief Forces delay between character typing.
    * 
    * 
    **/
    IEnumerator DelayType()
    {
        yield return new WaitForSeconds(m_flDelay);
        m_bActive = true;
    }
}
