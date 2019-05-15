/***************************************************
\file           LPK_YourFirstComponent.cs
\author        Christopher Onorati
\date   1/17/2019
\version   2018.3.4

\brief
  This component holds thorough documentation on how the LPK
  event system can be interfaced for custom components.  Note
  that documentation on how to make custom events is in the file
  header for LPK_Utilities.  This documentation is how to make
  a component that reacts to events and sends events.

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_YourFirstComponent
* \brief Demonstration of how to write a component that works alongside the LPK.  Note
*               this component inherets from LPK_LogicBase and not MonoBehavior.
**/
public class LPK_YourFirstComponent : LPK_LogicBase
{
    [Header("Event Sending Info")]

    //NOTENOTE:  LPK_EventReceivers is a serialized class used to determine who will ==RECEIVE== the input from the event.  This is a
    //           serialized class.  Note that you cannot use the relabel attribute on snerialized classes, so the naming convention
    //           used in these files is broken here for ease of reading in the inspector.
    [Tooltip("Receiver Game Object for ==EVENT DESCRIPTOR HERE==.")]
    public LPK_EventReceivers ExampleEventReceivers;

    [Header("Event Receiving Info")]

    //NOTENOTE:  LPK_EventList is a serialized class that stores all events that can be received.  This is used to initialize events
    //           in the OnStart function seen below.
    [Tooltip("Which events this component would react to (react by calling a function hooked up to that event).")]
    public LPK_EventList m_EventTrigger = new LPK_EventList();

    // NOTENOTE: Components that inheret from LPK_LogicBase cannot use Start.  Override the protected function OnStart instead.
    //           This function works the same way as the Monobehavior function "Start".
    override protected void OnStart()
    {
        //NOTENOTE:  Function from LPK_LogicBase that sets a function to respond to event(s) when received.  Note that the function
        //           (in this case OnEvent), ==MUST== take an LPK_EventData as the ==ONLY== parameter.
        InitializeEvent(m_EventTrigger, OnEvent);

        //Your implementation here.
    }

    //NOTENOTE:  This function should hold the implementation of what should happen when an event is received.  Functions that should
    //           respond to events ==MUST== take ==ONLY== an LPK_EventData class as a parameter.
    protected override void OnEvent(LPK_EventManager.LPK_EventData data)
    {
        //NOTENOTE: Always validate the data to make sure the function should actually be triggered.  This is ==NOT== done by default.
        if(!ShouldRespondToEvent(data))
            return;

        //Your implementation here.
    }

    //NOTENOTE:  Contains an example of how to invoke an event.  You could name this function whatever you want, have multiple invoke functions,
    //           or even invoke events inside of a function that is doing other things (NOT RECOMMENDED).
    void ExampleInvokeFunction()
    {
        //NOTENOTE:  LPK_EventData is a class that holds lists of many datatypes that could be used to send information via events.  This is done
        //           already in LPK_Timer if you wish to see a working example.  All data sent ==MUST== have a sender (which should be the game object
        //           this component is on), as well as receivers who will react to the event.  If the user chooses not to set any receivers, than
        //           anything looking for that event call will be activated.
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, ExampleEventReceivers);

        //NOTENOTE:  Send the event by creating a new LPK_EventList and setting the arrays to hold the events that are being invoked.  You can invoke as many
        //           events as you want in a single call this way.  The example below only invokes a keyboard input event.
        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_InputEventTrigger = new LPK_EventList.LPK_INPUT_EVENTS[] { LPK_EventList.LPK_INPUT_EVENTS.LPK_KeyboardInput };

        //NOTENOTE:  Invokes the event.  The first parameter is the list of events we are invoking, the second parameter holds the data made above.  This data
        //           contains information such as the sender of the event, the receiver list, etc.
        LPK_EventManager.InvokeEvent(sendEvent, data);
    }

    // NOTENOTE: Components that inheret from LPK_LogicBase cannot use Update.  Override the protected function OnStart instead.
    //           This function works the same way as the Monobehavior function "Update".
    override protected void OnUpdate()
    {
        //Your implementation here.
    }

    // NOTENOTE: Components that inheret from LPK_LogicBase cannot use OnCollisionEnter.  Override the protected function LPK_OnCollisionEnter instead.
    //           This function works the same way as the Monobehavior function "OnCollisionEnter".  Note this is the same for all other collision functions.
    protected override void LPK_OnCollisionEnter(Collision col)
    {
        //Your implementation here.
    }

    // NOTENOTE: Components that inheret from LPK_LogicBase cannot use OnDestroy.  Override the protected function OnDestroyed instead.
    //           This function works the same way as the Monobehavior function "OnDestroy".
    protected override void OnDestroyed()
    {
        //NOTENOTE:  You ==MUST== detach functions when an object is destroyed, or you will leave floating, disconnected event messages.
        //           Note you do not need to manually call DetachFunction for the function OnEvent.  This is handeled automatically.
        //           If you want to make multiple event functions in a component, or use a different function other than the OnEvent
        //           function found within LPK_LogicBase, just remember to detach them!
        DetachFunction(OnEvent);
    }
}
