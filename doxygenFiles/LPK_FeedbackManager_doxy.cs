/***************************************************
\file           LPK_FeedbackManager.cs
\author        Christopher Onorati
\date   2/28/2019
\version   2018.3.4

\brief
  This component can be used to create feedback loops for
  game events.  This component should be duplicated as needed,
  with student code (such as creating objects, modify colors, etc.)
  going in the lines commented below.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_FeedbackManager
* \brief Component used to track feedback loops on game events.
**/
[RequireComponent(typeof(TextMesh))]
public class LPK_FeedbackManager : LPK_LogicBase
{
    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Set to start the feedback loop instantly when this component is activated/spawned.")]
    [Rename("Start Feedback Instantly")]
    public bool m_bStartFeedbackInstantly = true;

    [Tooltip("Duration for the Signal portion of the feedback loop.")]
    [Rename("Signal Duration")]
    public float m_flSignalTime;

    [Tooltip("Duration for the Update portion of the feedback loop.")]
    [Rename("Update Duration")]
    public float m_flUpdateTime;

    [Tooltip("Duration for the Resolve portion of the feedback loop.")]
    [Rename("Resolve Duration")]
    public float m_flResolveTime;

    [Tooltip("Set to display the debug text on the object.  This is different than the print debug info flag.")]
    [Rename("Display Debug Text")]
    public bool m_bDisplayDebugText = true;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    /************************************************************************************/

    TextMesh m_cTextMesh;

    /**
    * \fn OnStart
    * \brief Sets up the text mesh and the feedback process.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cTextMesh = GetComponent<TextMesh>();

        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_bStartFeedbackInstantly)
            StartSignal();
    }

    /**
    * \fn OnEvent
    * \brief Activation of the feedback loop.
    * \param data - Event data to parse for validation.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Early out.
        if (!ShouldRespondToEvent(data))
            return;

        StartSignal();

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Feedback loop started via event trigger.");
    }

    /**
    * \fn StartSignal
    * \brief Starts the signal portion of the feedback loop.
    * 
    * 
    **/
    void StartSignal()
    {
        //NOTENOTE:  Could add a custom event here to allow students to hook into, but for educational purposes it may be more valuable
        //           for the student to write the calls themselves.  This also makes it easier for the instructot to track what is going on at each
        //           stage in the feedback loop.

        //STUDENT:  ADD SIGNAL FEEDBACK HERE (VISUAL, SFX, PARITCLES, ETC)

        if (m_bDisplayDebugText)
            m_cTextMesh.text = "S";

       StartCoroutine(DelayUpdate());
    }

    /**
    * \fn StartUpdate
    * \brief Starts the update portion of the feedback loop.
    * 
    * 
    **/
    void StartUpdate()
    {
        //NOTENOTE:  Could add a custom event here to allow students to hook into, but for educational purposes it may be more valuable
        //           for the student to write the calls themselves.  This also makes it easier for the instructot to track what is going on at each
        //           stage in the feedback loop.

        //STUDENT:  ADD UPDATE FEEDBACK HERE (VISUAL, SFX, PARITCLES, ETC)

        if (m_bDisplayDebugText)
            m_cTextMesh.text = "U";

        StartCoroutine(DelayResolve());
    }

    /**
     * \fn StartResolve
     * \brief Starts the resolve portion of the feedback loop.
     * 
     * 
     **/
    void StartResolve()
    {
        //NOTENOTE:  Could add a custom event here to allow students to hook into, but for educational purposes it may be more valuable
        //           for the student to write the calls themselves.  This also makes it easier for the instructot to track what is going on at each
        //           stage in the feedback loop.

        //STUDENT:  ADD RESOLVE FEEDBACK HERE (VISUAL, SFX, PARITCLES, ETC)

        if (m_bDisplayDebugText)
            m_cTextMesh.text = "R";

        StartCoroutine(StopResolve());
    }

    /**
    * \fn EndFeedbackLoop
    * \brief Ends the feedback loop.  Any last calls can be done here.
    * 
    * 
    **/
    void EndFeedbackLoop()
    {
        //NOTENOTE:  Could add a custom event here to allow students to hook into, but for educational purposes it may be more valuable
        //           for the student to write the calls themselves.  This also makes it easier for the instructot to track what is going on at each
        //           stage in the feedback loop.

        //STUDENT:  CLEAN UP ANY DANGLING FEEDBACK HERE.

        if (m_bDisplayDebugText)
            m_cTextMesh.text = "";
    }

    /**
    * \fn DelayUpdate
    * \brief Forces delay before starting the Update segment of the feedback loop.
    * 
    * 
    **/
    IEnumerator DelayUpdate()
    {
        yield return new WaitForSeconds(m_flSignalTime);
        StartUpdate();
    }

    /**
    * \fn DelayUpdate
    * \brief Forces delay before starting the Resolve segment of the feedback loop.
    * 
    * 
    **/
    IEnumerator DelayResolve()
    {
        yield return new WaitForSeconds(m_flUpdateTime);
        StartResolve();
    }

    /**
    * \fn StopResolve
    * \brief Forces delay before stopping the Resolve segment of the feedback loop.
    * 
    * 
    **/
    IEnumerator StopResolve()
    {
        yield return new WaitForSeconds(m_flResolveTime);
        EndFeedbackLoop();
    }
}
