/***************************************************
\file           LPK_DispatchOnGamepadInput.cs
\author        Christopher Onorati
\date   2/13/19
\version   2018.3.4

\brief
  This component can be added to any object to cause it to 
  dispatch a LPK_GamepadInput event upon a given button being
  pressed, released or held.

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
* \class GamepadInputStatus
* \brief Input storage for gamepads.
**/
public class GamepadInputStatus
{
    /************************************************************************************/

    public enum LPK_ReturnInput
    {
        NONE,   //Just used for setup.
        PRESSED,
        RELEASED,
        HELD,
    }

    /************************************************************************************/

    public readonly PlayerIndex m_iID;
    public GamePadState m_prevState;
    public GamePadState m_currentState;

    /************************************************************************************/

    //Input dictionary.
    Dictionary<LPK_DispatchOnGamepadInput.LPK_ControllerButtons, LPK_ReturnInput> m_dInputList
           = new Dictionary<LPK_DispatchOnGamepadInput.LPK_ControllerButtons, LPK_ReturnInput>();

    //Dead zone
    readonly float m_flDeadZone;


    /**
    * \fn Constructor
    * \brief Sets ID and creates intiail dictionary of inputs.
    * 
    * 
    **/
    public GamepadInputStatus(PlayerIndex _ID, float _DeadZone)
    {
        m_iID = _ID;
        m_flDeadZone = _DeadZone;
        
        //Set up each value in the dictionary up to ANY.
        for(int i = 0; i < (int)LPK_DispatchOnGamepadInput.LPK_ControllerButtons.ANY; i++)
            CreateDictionary( (LPK_DispatchOnGamepadInput.LPK_ControllerButtons)i );
    }

    /**
    * \fn CreateDictionary
    * \brief Creates all entries into the dictionary of inputs.
    * 
    * 
    **/
    void CreateDictionary(LPK_DispatchOnGamepadInput.LPK_ControllerButtons input)
    {
        m_dInputList.Add(input, LPK_ReturnInput.NONE);
    }

    /**
    * \fn UpdateDictionary
    * \brief Update the input state of every item in the dictionary.
    * 
    * 
    **/
    public void UpdateDictionary()
    {
        //NOTENOTE: This would be cleaner to do in a loop.  For now, this works, but this should be revisited and refactored later,
        //          if only for readability sake.

        //Update the buttons.
        UpdateDictionaryButton(m_prevState.Buttons.A, m_currentState.Buttons.A, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.A);
        UpdateDictionaryButton(m_prevState.Buttons.B, m_currentState.Buttons.B, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.B);
        UpdateDictionaryButton(m_prevState.Buttons.X, m_currentState.Buttons.X, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.X);
        UpdateDictionaryButton(m_prevState.Buttons.Y, m_currentState.Buttons.Y, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.Y);
        UpdateDictionaryButton(m_prevState.Buttons.Start, m_currentState.Buttons.Start, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.START);
        UpdateDictionaryButton(m_prevState.Buttons.Back, m_currentState.Buttons.Back, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.BACK);
        UpdateDictionaryButton(m_prevState.Buttons.Guide, m_currentState.Buttons.Guide, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.GUIDE);
        UpdateDictionaryButton(m_prevState.Buttons.LeftShoulder, m_currentState.Buttons.LeftShoulder, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_SHOULDER);
        UpdateDictionaryButton(m_prevState.Buttons.RightShoulder, m_currentState.Buttons.RightShoulder, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_SHOULDER);
        UpdateDictionaryButton(m_prevState.Buttons.LeftStick, m_currentState.Buttons.LeftStick, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_STICK);
        UpdateDictionaryButton(m_prevState.Buttons.RightStick, m_currentState.Buttons.RightStick, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_STICK);

        //Update the DPADs.
        UpdateDictionaryButton(m_prevState.DPad.Up, m_currentState.DPad.Up, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.DPAD_UP);
        UpdateDictionaryButton(m_prevState.DPad.Down, m_currentState.DPad.Down, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.DPAD_DOWN);
        UpdateDictionaryButton(m_prevState.DPad.Left, m_currentState.DPad.Left, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.DPAD_LEFT);
        UpdateDictionaryButton(m_prevState.DPad.Right, m_currentState.DPad.Right, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.DPAD_RIGHT);

        //Trigggers
        UpdateDictionaryTrigger(m_prevState.Triggers.Left, m_currentState.Triggers.Left, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_TRIGGER);
        UpdateDictionaryTrigger(m_prevState.Triggers.Right, m_currentState.Triggers.Right, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_TRIGGER);

        //Update left joystick.
        UpdateDictionaryJoystick(m_prevState.ThumbSticks.Left.X, m_currentState.ThumbSticks.Left.X, m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_JOYSTICK_RIGHT);
        UpdateDictionaryJoystick(m_prevState.ThumbSticks.Left.Y, m_currentState.ThumbSticks.Left.Y, m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_JOYSTICK_UP);
        UpdateDictionaryJoystickReverse(m_prevState.ThumbSticks.Left.X, m_currentState.ThumbSticks.Left.X, -m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_JOYSTICK_LEFT);
        UpdateDictionaryJoystickReverse(m_prevState.ThumbSticks.Left.Y, m_currentState.ThumbSticks.Left.Y, -m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.LEFT_JOYSTICK_DOWN);

        //Update the right joystick.
        UpdateDictionaryJoystick(m_prevState.ThumbSticks.Right.X, m_currentState.ThumbSticks.Right.X, m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_JOYSTICK_RIGHT);
        UpdateDictionaryJoystick(m_prevState.ThumbSticks.Right.Y, m_currentState.ThumbSticks.Right.Y, m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_JOYSTICK_UP);
        UpdateDictionaryJoystickReverse(m_prevState.ThumbSticks.Right.X, m_currentState.ThumbSticks.Right.X, -m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_JOYSTICK_LEFT);
        UpdateDictionaryJoystickReverse(m_prevState.ThumbSticks.Right.Y, m_currentState.ThumbSticks.Right.Y, -m_flDeadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons.RIGHT_JOYSTICK_DOWN);
    }

    /**
    * \fn UpdateDictionaryButton
    * \brief Update the input state of buttons.
    * \param prevState    - Previous state of the button on frame - 1.
    *                currentState - Current state of the button on frame.
    *                matchButton  - Button input to update in the dictionary.
    * 
    **/
    void UpdateDictionaryButton(ButtonState prevState, ButtonState currentState, LPK_DispatchOnGamepadInput.LPK_ControllerButtons matchButton)
    {
        if (prevState == ButtonState.Pressed && currentState == ButtonState.Pressed)
            m_dInputList[matchButton] = LPK_ReturnInput.HELD;
        else if (prevState == ButtonState.Pressed && currentState == ButtonState.Released)
            m_dInputList[matchButton] = LPK_ReturnInput.RELEASED;
        else if (prevState == ButtonState.Released && currentState == ButtonState.Pressed)
            m_dInputList[matchButton] = LPK_ReturnInput.PRESSED;
        else
            m_dInputList[matchButton] = LPK_ReturnInput.NONE;
    }

    /**
    * \fn UpdateDictionaryTrigger
    * \brief Update the input state of triggers.
    * \param prevState    - Previous state of the trigger on frame - 1.
    *                currentState - Current state of the trigger on frame.
    *                matchButton  - Trigger input to update in the dictionary.
    * 
    **/
    void UpdateDictionaryTrigger(float prevState, float currentState, LPK_DispatchOnGamepadInput.LPK_ControllerButtons matchButton)
    {
        if (prevState >= m_flDeadZone && currentState >= m_flDeadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.HELD;
        else if (prevState >= m_flDeadZone && currentState < m_flDeadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.RELEASED;
        else if (prevState < m_flDeadZone && currentState >= m_flDeadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.PRESSED;
        else
            m_dInputList[matchButton] = LPK_ReturnInput.NONE;
    }

    /**
    * \fn UpdateDictionaryTrigger
    * \brief Update the input state of joysticks in right and up.
    * \param prevState    - Previous state of the joystick on frame - 1.
    *                currentState - Current state of the joystick on frame.
    *                deadZone     - Deadzone of the joystick to ignore input.
    *                matchButton  - Joystick input to update in the dictionary.
    * 
    **/
    void UpdateDictionaryJoystick(float prevState, float currentState, float deadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons matchButton)
    {
        if (prevState >= deadZone && currentState >= deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.HELD;
        else if (prevState >= deadZone && currentState < deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.RELEASED;
        else if (prevState < deadZone && currentState >= deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.PRESSED;
        else
            m_dInputList[matchButton] = LPK_ReturnInput.NONE;
    }

    /**
    * \fn UpdateDictionaryTrigger
    * \brief Update the input state of joysticks in left and down.
    * \param prevState    - Previous state of the joystick on frame - 1.
    *                currentState - Current state of the joystick on frame.
    *                deadZone     - Deadzone of the joystick to ignore input.
    *                matchButton  - Joystick input to update in the dictionary.
    * 
    **/
    void UpdateDictionaryJoystickReverse(float prevState, float currentState, float deadZone, LPK_DispatchOnGamepadInput.LPK_ControllerButtons matchButton)
    {
        if (prevState <= deadZone && currentState <= deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.HELD;
        else if (prevState <= deadZone && currentState > deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.RELEASED;
        else if (prevState > deadZone && currentState <= deadZone)
            m_dInputList[matchButton] = LPK_ReturnInput.PRESSED;
        else
            m_dInputList[matchButton] = LPK_ReturnInput.NONE;
    }

    /**
    * \fn GetDictioanryValue
    * \brief Get the current press value stored in the input dictionary.
    * \param input - Button to check value for.
    *                mode  - Mode of input to detect.
    * \return bool - Input status to report back.
    **/
    public bool GetDictioanryValue(LPK_DispatchOnGamepadInput.LPK_ControllerButtons input, LPK_ReturnInput mode)
    {
        //Single button check.
        if (input != LPK_DispatchOnGamepadInput.LPK_ControllerButtons.ANY)
        {
            if (m_dInputList[input] == mode)
                return true;
            else
                return false;
        }

        //Any button check.
        else
        {
            for (int i = 0; i < m_dInputList.Count; i++)
            {
                if (m_dInputList[(LPK_DispatchOnGamepadInput.LPK_ControllerButtons)i] == mode)
                    return true;
            }
        }

        return false;
    }
}

/**
* \class LPK_DispatchOnGamepadInput
* \brief Component to sent events on gamepad input.
**/
public class LPK_DispatchOnGamepadInput : LPK_LogicBase
{
    /************************************************************************************/

    public enum LPK_InputMode
    {
        PRESSED,
        RELEASED,
        HELD,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Start the follower active on spawn.")]
    [Rename("Start Active")]
    public bool m_bActive = true;

    [Tooltip("How to change active state when events are received.")]
    [Rename("Toggle Type")]
    public LPK_ToggleType m_eToggleType;

    [Tooltip("Which gamepad input will trigger event sending.")]
    [Rename("Gamepad Number")]
    public LPK_ControllerNumber m_eControllerNumber;

    [Tooltip("Which button on the gamepad to detect input from.")]
    [Rename("Gamepad Button")]
    public LPK_ControllerButtons m_eInputButton;

    [Tooltip("What mode should cause the event dispatch.")]
    [Rename("Input Mode")]
    public LPK_InputMode m_eInputMode = LPK_InputMode.PRESSED;

    [Tooltip("How far the trigger/joystick must be pushed in to register detection.  0 is not at all (hence not allowed), 1 is all the way.")]
    [Range(0.01f, 1.0f)]
    public float m_TriggerDetectZone = 0.5f;

    [Header("Event Receiving Info")]

    [Tooltip("Which event will trigger this component to be active.")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for gamepad input detection.")]
    public LPK_EventReceivers GamepadInputReceivers;

    /************************************************************************************/

    //List of gamepads to listen to.
    List<GamepadInputStatus> m_pGamepadStatuses = new List<GamepadInputStatus>();

    /**
    * \fn OnStart
    * \brief Sets up which gamepad to listen for input on.
    * 
    * 
    **/
    override protected void OnStart()
    {
        InitializeEvent(m_EventTrigger, OnEvent);

        if (m_eControllerNumber == LPK_ControllerNumber.ONE)
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.One, m_TriggerDetectZone));
        else if (m_eControllerNumber == LPK_ControllerNumber.TWO)
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Two, m_TriggerDetectZone));
        else if (m_eControllerNumber == LPK_ControllerNumber.THREE)
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Three, m_TriggerDetectZone));
        else if (m_eControllerNumber == LPK_ControllerNumber.FOUR)
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Four, m_TriggerDetectZone));
        else
        {
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.One, m_TriggerDetectZone));
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Two, m_TriggerDetectZone));
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Three, m_TriggerDetectZone));
            m_pGamepadStatuses.Add(new GamepadInputStatus(PlayerIndex.Four, m_TriggerDetectZone));
        }
    }

    /**
    * \fn OnEvent
    * \brief Changes active state of the path follower.
    * \param data - Event info to parse.
    * 
    **/
    override protected void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //Invalid event
        if (!ShouldRespondToEvent(data))
            return;

        if (m_eToggleType == LPK_ToggleType.ON)
            m_bActive = true;
        else if (m_eToggleType == LPK_ToggleType.OFF)
            m_bActive = false;
        else if (m_eToggleType == LPK_ToggleType.TOGGLE)
            m_bActive = !m_bActive;

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Event Received");
    }

    /**
    * \fn OnUpdate
    * \brief Manages input detection.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        //NOTENOTE: We still want to update input even if event detection and sending is not set to be active, otherwise we will get out of sync.
        SetPrevFrameInput();
        SetCurrentFrameInput();

        if (!m_bActive)
            return;

        for (int i = 0; i < m_pGamepadStatuses.Count; i++)
            m_pGamepadStatuses[i].UpdateDictionary();

        //Check the input based on the mode.
        for (int i = 0; i < m_pGamepadStatuses.Count; i++)
        {
            if ((int)m_pGamepadStatuses[i].m_iID != (int)m_eControllerNumber && m_eControllerNumber != LPK_ControllerNumber.ANY)
                continue;

            //Held detected with proper button.
            if (m_pGamepadStatuses[i].GetDictioanryValue(m_eInputButton, GamepadInputStatus.LPK_ReturnInput.HELD) && m_eInputMode == LPK_InputMode.HELD)
                DispatchEvent();
            //Pressed detected with proper button.
            else if (m_pGamepadStatuses[i].GetDictioanryValue(m_eInputButton, GamepadInputStatus.LPK_ReturnInput.PRESSED) && m_eInputMode == LPK_InputMode.PRESSED)
                DispatchEvent();
            //Released detected with propper button.
            else if (m_pGamepadStatuses[i].GetDictioanryValue(m_eInputButton, GamepadInputStatus.LPK_ReturnInput.RELEASED) && m_eInputMode == LPK_InputMode.RELEASED)
                DispatchEvent();
        }
    }

    /**
    * \fn SetPrevFrameInput
    * \brief Set the previous frame inputs.
    * 
    * 
    **/
    void SetPrevFrameInput()
    {
        for (int i = 0; i < m_pGamepadStatuses.Count; i++)
            m_pGamepadStatuses[i].m_prevState = m_pGamepadStatuses[i].m_currentState;
    }

    /**
    * \fn SetCurrentFrameInput
    * \brief Set the current states of the gamepad.
    * 
    * 
    **/
    void SetCurrentFrameInput()
    {
        for(int i = 0; i < m_pGamepadStatuses.Count; i++)
        {
            PlayerIndex curIndex = m_pGamepadStatuses[i].m_iID;

            //Gamepad is not connected - set to a new state of everything defaulting.
            if (!GamePad.GetState(curIndex).IsConnected)
                m_pGamepadStatuses[i].m_currentState = new GamePadState();
            else
                m_pGamepadStatuses[i].m_currentState = GamePad.GetState(curIndex);
        }
    }

    /**
    * \fn DispatchEvent
    * \brief Dispatches the gamepad input event if conditions were met in CompareInputStates.
    * 
    * 
    **/
    void DispatchEvent()
    {
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, GamepadInputReceivers);
        data.m_PressedGamepadButton = m_eInputButton;
        data.m_PressedGamepadNumber = m_eControllerNumber;

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_GamepadInput };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Gamepad event dispatched");
    }
}
