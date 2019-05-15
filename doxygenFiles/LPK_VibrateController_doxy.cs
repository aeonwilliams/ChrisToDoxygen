/***************************************************
\file           LPK_VibrateController.cs
\author        Christopher Onorati
\date   12/23/18
\version   2.17

\brief
  This component is used to manage controller vibration.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

/**
* \class GamepadShakeStatus
* \brief Shake storage for gamepads.
**/
public class GamepadShakeStatus
{
    /************************************************************************************/

    public readonly PlayerIndex m_iID;

    /**
    * \fn Constructor
    * \brief Sets ID.
    * 
    * 
    **/
    public GamepadShakeStatus(PlayerIndex _ID)
    {
        m_iID = _ID;
    }

    /**
    * \fn Rumble
    * \brief Rumble the controller this class is in charge of tracking.
    * \param intensity     - Intensity of rumble.
    *                intensityMods - Modifiers to the intensity of the shake.
    * 
    **/
    public void Rumble(float intensity, Vector2 intensityMods)
    {
        GamePad.SetVibration(m_iID, intensity * intensityMods.x, intensity * intensityMods.y);
    }
}

/**
* \class LPK_VibrateController
* \brief Manages controller vibration.
**/
public class LPK_VibrateController : LPK_LogicBase
{
    /************************************************************************************/

    new public enum LPK_ControllerNumber
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        ALL,
    };

    public enum LPK_ShakeType
    {
        CONSTANT,
        FADE_IN,
        FADE_OUT,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Which gamepad to vibrate.")]
    [Rename("Gamepad Number")]
    public LPK_ControllerNumber m_eControllerNumber;

    [Tooltip("Which mode to use when shaking the gamepad.")]
    [Rename("Shake Type")]
    public LPK_ShakeType m_eShakeType;

    [Tooltip("Set the intensity of the shake on the left and right side of the controller.  Should be a value between 0 and 1.")]
    [Rename("Shake Modifiers")]
    public Vector2 m_vecLeftRightMods = new Vector2(1, 1);

    [Tooltip("Only activate this effect once, no matter how many times correct conditions are met.")]
    [Rename("Only Vibrate Once")]
    public bool m_bOnlyOnce;

    [Tooltip("How long to wait before allowing more vibrations to occur.  The delay starts AFTER a shake has finished.")]
    [Rename("Cool Down")]
    public float m_flCoolDown;

    [Tooltip("Duration of the controller vibration.")]
    [Rename("Vibration Duration")]
    public float m_flVibrateDuration = 1.5f;

    [Tooltip("Intensity of the controller vibration.")]
    public float m_VibrateIntensity = 1.0f;

    //NOTENOTE:  Currently this really only supports single player since there can only be one source.  If you wanted this to be
    //           more compatable with multiplayer, each controller should hold a Source object that represents its point in space, like
    //           a player, for example.  A workaround would be to add this component 4 times and set each controller and source differently.
    [System.Serializable]
    public class PointVibrationProperties
    {
        [Tooltip("Source of the vibration.  If left null, assume it is this object.")]
        [Rename("Vibrate Source")]
        public GameObject m_pVibrateSource;

        [Tooltip("Object that represents the point of receiving vibration, such as the player character.")]
        [Rename("Vibrate Receiver")]
        public GameObject m_pVibrateReceiver;

        [Tooltip("Distance from which the vibration can be felt.  Vibration strength will scale dynamically with distance.")]
        [Rename("Distance")]
        public float m_flDistance = 10.0f;

        [Tooltip("Flag to make vibration happen indefinently.  Useful for things such as a laser beam's contact point, for example.")]
        [Rename("Continual")]
        public bool m_bContinual;
    }

    public PointVibrationProperties m_PointVibrationProperties;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component's action")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for vibration starting.")]
    public LPK_EventReceivers m_VibrationStartReceivers;

    [Tooltip("Receiver Game Objects for vibration stopping.")]
    public LPK_EventReceivers m_VibrationStopReceivers;

    /************************************************************************************/

    //List of gamepads to shake.
    List<GamepadShakeStatus> m_pGamepads = new List<GamepadShakeStatus>();

    //Active state of the effect.
    bool m_bActive = false;

    //Flag to detect if the controller has vibrated once already.
    bool m_bHasVibrated = false;

    //Current intenisty of the shake.
    float m_flCurIntensity;

    //Current duration of the shake.
    float m_flCurDuration;

    /**
    * \fn OnStart
    * \brief Sets up what event to listen to for vibration starting.
    * 
    * 
    **/
    override protected void OnStart()
    {
        //Set a continual point shake in motion, if the user has specified.
        if (m_PointVibrationProperties.m_pVibrateReceiver != null && m_PointVibrationProperties.m_bContinual)
        {
            m_bActive = true;

            if (m_eShakeType != LPK_ShakeType.CONSTANT)
            {
                m_eShakeType = LPK_ShakeType.CONSTANT;

                if (m_bPrintDebug)
                    LPK_PrintDebug(this, "Changing shake type to be constant for a continual point shake.");
            }
        }

        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_PointVibrationProperties.m_pVibrateSource == null)
            m_PointVibrationProperties.m_pVibrateSource = gameObject;

        //Set controller list.
        if (m_eControllerNumber == LPK_ControllerNumber.ONE)
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.One));
        else if (m_eControllerNumber == LPK_ControllerNumber.TWO)
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Two));
        else if (m_eControllerNumber == LPK_ControllerNumber.THREE)
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Three));
        else if (m_eControllerNumber == LPK_ControllerNumber.FOUR)
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Four));
        else
        {
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.One));
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Two));
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Three));
            m_pGamepads.Add(new GamepadShakeStatus(PlayerIndex.Four));
        }
    }

    /**
    * \fn OnUpdate
    * \brief Manage whether or not to vibrate the controller.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (m_bActive && m_flCurDuration >= m_flVibrateDuration && !m_PointVibrationProperties.m_bContinual)
            StopVibration();
        else if (m_bActive)
            Vibrate();
    }

    /**
    * \fn Vibrate
    * \brief Set vibration state across appropriate controllers.
    * 
    * 
    **/
    void Vibrate()
    {
        m_flCurDuration += Time.deltaTime;

        float distanceScalar = 1.0f;

        //Set distance scalar if user desires.
        if(m_PointVibrationProperties.m_pVibrateReceiver !=  null)
        {
            float distance = Vector3.Distance(m_PointVibrationProperties.m_pVibrateReceiver.transform.position,
                m_PointVibrationProperties.m_pVibrateSource.transform.position);

            distanceScalar = Mathf.Clamp( 1.0f - ( distance / m_PointVibrationProperties.m_flDistance ), 0.0f, 1.0f );
                
        }

        if (m_eShakeType == LPK_ShakeType.FADE_IN)
            m_flCurIntensity = (m_flCurDuration / m_flVibrateDuration) * m_VibrateIntensity * distanceScalar;
        else if (m_eShakeType == LPK_ShakeType.FADE_OUT)
            m_flCurIntensity = ((m_flVibrateDuration - m_flCurDuration) / m_flVibrateDuration) * m_VibrateIntensity * distanceScalar;
        else if (m_eShakeType == LPK_ShakeType.CONSTANT)
            m_flCurIntensity = m_VibrateIntensity * distanceScalar;

        //Check vibration based on the controller number selected.
        for (int i = 0; i < m_pGamepads.Count; i++)
        {
            if ((int)m_pGamepads[i].m_iID != (int)m_eControllerNumber && m_eControllerNumber != LPK_ControllerNumber.ALL)
                continue;

            m_pGamepads[i].Rumble(m_flCurIntensity, m_vecLeftRightMods);
        }
    }

    /**
    * \fn StopVibration
    * \brief Stop vibration and send out appropriate event.  Also manages cooldown since a Corutine delay results
    *                in some unfortunate behavior.
    * 
    * 
    **/
    void StopVibration()
    {
        //Stop vibration based on the controller number selected.
        for (int i = 0; i < m_pGamepads.Count; i++)
        {
            if ((int)m_pGamepads[i].m_iID != (int)m_eControllerNumber && m_eControllerNumber != LPK_ControllerNumber.ALL)
                continue;

            m_pGamepads[i].Rumble(0.0f, m_vecLeftRightMods);
        }

        m_flCurDuration += Time.deltaTime;

        //Cool down time is over.
        if (m_flCurDuration >= m_flCoolDown + m_flVibrateDuration)
        {
            if (m_bPrintDebug)
                LPK_PrintDebug(this, "Stopping Controller Vibration");

            //Dispatch event.
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_VibrationStopReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStop };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            m_bActive = false;
        }
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

        //Already active.  If the cool down is less than or equal to 0, then just go ahead and create a new shake.
        if (m_bActive && m_flCoolDown > 0 || (m_bOnlyOnce && m_bHasVibrated))
            return;

        m_bActive = true;
        m_bHasVibrated = true;
        m_flCurDuration = 0.0f;

        //Set initial intensity.
        if (m_eShakeType == LPK_ShakeType.CONSTANT || m_eShakeType == LPK_ShakeType.FADE_OUT)
            m_flCurIntensity = m_VibrateIntensity;
        else
            m_flCurIntensity = 0.0f;

        //Dispatch event.
        LPK_EventManager.LPK_EventData newData = new LPK_EventManager.LPK_EventData(gameObject, m_VibrationStartReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_VibrationStart };

        LPK_EventManager.InvokeEvent(sendEvent, newData);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Starting Controller Vibration");
    }

    /**
    * \fn OnApplicationQuit
    * \brief Stop controller vibration when game is terminated.
    * 
    * 
    **/
    void OnApplicationQuit()
    {
        for (int i = 0; i < m_pGamepads.Count; i++)
            m_pGamepads[i].Rumble(0.0f, m_vecLeftRightMods);
    }
}
