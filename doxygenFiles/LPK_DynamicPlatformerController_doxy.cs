/***************************************************
\file           LPK_DynamicPlatformerController.cs
\author        Christopher Onorati
\date   2/25/2019
\version   2018.3.4

\brief
  This component allows for using the keyboard to move a 
  character by applying velocity to its dynamic RigidBody

This script is a basic and generic implementation of its 
functionality. It is designed for educational purposes and 
aimed at helping beginners.

\copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \class LPK_DynamicPlatformerController
* \brief Implementation of a basic platformer character.
**/
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class LPK_DynamicPlatformerController : LPK_LogicBase
{
    /************************************************************************************/

    public enum PlatformerJumpInputType
    {
        PRESS,
        HOLD,
    };

    public enum LPK_GroundedDetectionType
    {
        COLLISION,
        FEET,
        RAYCAST,
    };

    /************************************************************************************/

    [Header("Component Properties")]

    [Tooltip("Whether the character can move.")]
    [Rename("Can Move")]
    public bool m_bCanMove = true;

    [Tooltip("Whether the character can jump.")]
    [Rename("Can Jump")]
    public bool m_bCanJump = true;

    [Tooltip("Whether the character can jump and move at the same time.")]
    [Rename("Can Move While Jumping")]
    public bool m_bCanMoveWhileJumping = true;

    [Tooltip("Velocity applied in the X axis when moving.")]
    [Rename("Move Speed")]
    public float m_flMoveSpeed = 2.0f;

    [Tooltip("Max velocity in the X and Y axis.")]
    [Rename("Max Speed")]
    public Vector2 m_vecMaxSpeed = new Vector2(30, 30);

    [Tooltip("Deceleation to be applied every frame (Expected values from 0 to 1).")]
    [Range(0, 1)]
    public float m_Deceleration = 0.2f;

    [Header("Jump Properties")]

    [Tooltip("Determines how to handle jump input for the character.")]
    [Rename("Jump Input Mode.")]
    public PlatformerJumpInputType m_eJumpInputType;

    [Tooltip("Allow a single jump midair, even if midair jumps are allowed, if the player falls off a ledge.  If midair jumps are allowed, the first jump used when falling off a ledge will not count towards a midair jump.")]
    [Rename("Allow Grace Jump.")]
    public bool m_bAllowGraceJump = false;

    [System.Serializable]
    public class GroundedInfo
    {
        [Tooltip("Whether the character spawns on the ground when the scene starts.")]
        [Rename("Starts Grounded")]
        public bool m_bGrounded = true;

        [Tooltip("How to handle the gounded collision check.")]
        [Rename("Grounded Detection Style")]
        public LPK_GroundedDetectionType m_eGroundedCheckType;

        [Tooltip("Scale the length of the raycast for RAYCAST grounded detction.  If set to one, the ray will be the length of the object's collider / 2 - which is ideal in most cases.")]
        [Rename("Raycast Scalar")]
        public float m_flRaycastLength = 1.0f;

        [Tooltip("Set the collidet to use for grounded detction if set to FEET.  If this collider touches anything, other than this object, then this controller is grounded.")]
        [Rename("Feet Collider")]
        public Collider2D m_pFeetCollider;
    }

    public GroundedInfo m_GroundedInfo;

    [Tooltip("Scalar to restrict movement while midair.  Will scale with the amount of air jumps made.")]
    [Rename("Jump Movement Deceleration")]
    public float m_flJumpMovementDeceleration = 1.0f;

    [Tooltip("Decrease velocity impact of susequent jumps.")]
    [Rename("Reduce Air Jump Speed")]
    public bool m_bAirJumpDecreaseVelocity = true;

    [Tooltip("Velocity on the Y axis applied when jumping.")]
    [Rename("Jump Force")]
    public float m_flJumpSpeed = 8.0f;

    [Tooltip("Maximum number of jumps to use in mid-air. Air jumps are reset when grounded.")]
    [Rename("Max Air Jumps")]
    public int m_iMaxAirJumps = 0;

    [Tooltip("Total time for a press to be held to reach max height.  Only used with HOLD.")]
    [Rename("Jump Max Hold Time")]
    public float m_flMaxAirTime = 1.0f;

    [Header("Input Properties")]

    [Tooltip("Virtual button used to move left.")]
    [Rename("Left Input")]
    public string m_MoveLeftButton = "MoveLeft";

    [Tooltip("Virtual button used to move right.")]
    [Rename("Right Input")]
    public string m_MoveRightButton = "MoveRight";

    [Tooltip("Virtual button used to move right.")]
    [Rename("Jump Input")]
    public string m_JumpButton = "Jump";

    [Header("Event Sending Info")]

    [Tooltip("Receiver Game Objects for when the character jumps.")]
    public LPK_EventReceivers m_CharacterJumpEventReceivers;

    [Tooltip("Receiver Game Objects for when the character lands.")]
    public LPK_EventReceivers m_CharacterLandEventReceivers;

    /************************************************************************************/
  
    //Number of mid-air jumps used
    int m_iAirJumpsUsed = 0;

    //Time the character has held the jump button down.  Used for HOLD.
    float m_flAirTime = 0.0f;

    //Flag to detect jumps status.
    bool m_bIsJumping = false;

    //Int counter used to not detect contactpoints on the feet when collision ends.
    int m_iDelayFrame = 2;

    /************************************************************************************/

    private Rigidbody2D m_cRigidBody;
    private SpriteRenderer m_cSpriteRenderer;
    private BoxCollider2D m_cCollider;

    /**
    * \fn OnStart
    * \brief Checks to ensure proper components are on the object for movement.
    * 
    * 
    **/
    override protected void OnStart()
    {
        m_cRigidBody = GetComponent<Rigidbody2D>();
        m_cCollider = GetComponent<BoxCollider2D>();
    }

    /**
    * \fn OnUpdate
    * \brief Manages player input.
    * 
    * 
    **/
    override protected void OnUpdate()
    {
        if (m_bPrintDebug)
            LPK_PrintDebug(this, "Update Controller");

        /*----------MOVEMENT----------*/

        //Variable to determine the movement direction
        float moveDir = 0.0f;

        //If the left key is pressed, make the character face and move left
        if (!string.IsNullOrEmpty(m_MoveLeftButton) && Input.GetButton(m_MoveLeftButton))
        {
            moveDir -= 1.0f / Mathf.Max(1.0f, (m_iAirJumpsUsed * m_flJumpMovementDeceleration) + 1.0f);
            transform.eulerAngles = new Vector3(0, 180, 0);

            if (m_bPrintDebug)
                LPK_PrintError(this, "Move left.");
        }

        //If the left key is pressed, make the character face and move right
        if (!string.IsNullOrEmpty(m_MoveRightButton) && Input.GetButton(m_MoveRightButton))
        {
            moveDir += 1.0f / Mathf.Max(1.0f, (m_iAirJumpsUsed * m_flJumpMovementDeceleration) + 1.0f);
            transform.eulerAngles = new Vector3( 0, 0, 0);

            if (m_bPrintDebug)
                LPK_PrintError(this, "Move right.");
        }

        if ((m_bCanMove && m_bCanMoveWhileJumping) || (m_bCanMove && !m_bCanMoveWhileJumping && !m_bIsJumping) || (!m_bCanMove && m_bCanMoveWhileJumping && m_bIsJumping))
        {
            //Take speed from previous frame and apply some deceleration
            float oldSpeed = m_cRigidBody.velocity.x * (1 - m_Deceleration);

            //Calculate new speed to be added this frame
            float newSpeed = moveDir * m_flMoveSpeed;

            //Set character's new velocity based on the given movemnt input (leave the Y component as is)
            m_cRigidBody.velocity = new Vector3(oldSpeed + newSpeed, m_cRigidBody.velocity.y, 0);
        }

        /*----------JUMPING----------*/

        m_iDelayFrame--;

        //If jump key is pressed
        if (m_eJumpInputType == PlatformerJumpInputType.PRESS)
            JumpInputPress();
        else
            JumpInputHold();

        /*----------SPEED CLAMP----------*/

        m_cRigidBody.velocity.Set(Mathf.Clamp(m_cRigidBody.velocity.x, -m_vecMaxSpeed.x, m_vecMaxSpeed.x),
                                  Mathf.Clamp(m_cRigidBody.velocity.y, -m_vecMaxSpeed.y, m_vecMaxSpeed.y));

        /*-----------GROUNDED------------*/
        if(m_GroundedInfo.m_eGroundedCheckType == LPK_GroundedDetectionType.RAYCAST)
            CheckGroundedRaycast();
        else if (m_GroundedInfo.m_eGroundedCheckType == LPK_GroundedDetectionType.FEET && m_iDelayFrame <= 0)
        {
            if (m_GroundedInfo.m_pFeetCollider == null)
            {
                if (m_bPrintDebug)
                    LPK_PrintError(this, "Cannot find feet object that has a 2D collider.");
            }
            else
                CheckGroundedFeet();
        }
    }

    /**
    * \fn JumpInputPress
    * \brief Manages jump input if set to respond to PRESS.
    * 
    * 
    **/
    void JumpInputPress()
    {
        if (!string.IsNullOrEmpty(m_JumpButton) && Input.GetButtonDown(m_JumpButton))
        {
            //If the character is grounded
            if (m_GroundedInfo.m_bGrounded)
            {
                //Apply upward velocity based on specified speed
                m_cRigidBody.velocity = new Vector3(m_cRigidBody.velocity.x, m_flJumpSpeed, 0);
                m_GroundedInfo.m_bGrounded = false;
                m_iDelayFrame = 2;

                m_bIsJumping = true;

                //Dispatch Jump
                LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CharacterJumpEventReceivers);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterJump };

                LPK_EventManager.InvokeEvent(sendEvent, data);

                if (m_bPrintDebug)
                    LPK_PrintError(this, "Jumping via PRESS input.");
            }

            //If character isnt grounded but air jump is enabled and available
            else if (m_iMaxAirJumps >= 1 && m_iAirJumpsUsed < m_iMaxAirJumps)
            {
                m_iAirJumpsUsed++;

                //Apply upward velocity based on specified speed
                if (m_bAirJumpDecreaseVelocity)
                    m_cRigidBody.velocity = new Vector3(m_cRigidBody.velocity.x, (m_flJumpSpeed) / m_iAirJumpsUsed, 0);
                else
                    m_cRigidBody.velocity = new Vector3(m_cRigidBody.velocity.x, m_flJumpSpeed, 0);

                m_bIsJumping = true;

                //Dispatch Jump
                LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CharacterJumpEventReceivers);

                LPK_EventList sendEvent = new LPK_EventList();
                sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterJump };

                LPK_EventManager.InvokeEvent(sendEvent, data);

                if (m_bPrintDebug)
                    LPK_PrintError(this, "Air jump via PRESS input.");
            }
        }
    }

    /**
    * \fn JumpInputHold
    * \brief Manages jump input if set to respond to HOLD.
    * 
    * 
    **/
    void JumpInputHold()
    {
        //Resets data on initial button press.
        if(!string.IsNullOrEmpty(m_JumpButton) && Input.GetButtonDown(m_JumpButton))
        {
            if (m_iAirJumpsUsed > m_iMaxAirJumps)
                return;

            if (!m_GroundedInfo.m_bGrounded)
                m_iAirJumpsUsed++;

            m_flAirTime = 0.0f;
            m_bIsJumping = true;

            //Dispatch event.
            LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CharacterJumpEventReceivers);

            LPK_EventList sendEvent = new LPK_EventList();
            sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterJump };

            LPK_EventManager.InvokeEvent(sendEvent, data);

            m_cRigidBody.velocity = new Vector3(m_cRigidBody.velocity.x, m_flJumpSpeed, 0);

            if (m_bPrintDebug)
                LPK_PrintError(this, "Jumping via HOLD input.");
        }

        //Manages actual velocity change.
        if (!string.IsNullOrEmpty(m_JumpButton) && Input.GetButton(m_JumpButton) && m_flAirTime <= m_flMaxAirTime)
        {
            if (m_iAirJumpsUsed > m_iMaxAirJumps)
                return;

            m_cRigidBody.velocity = new Vector3(m_cRigidBody.velocity.x, m_flJumpSpeed, 0);

            m_flAirTime += Time.deltaTime;
        }
    }

    /**
    * \fn CheckGroundedRaycast
    * \brief Checks grounded via raycast detection.
    * 
    * 
    **/
    void CheckGroundedRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, m_cCollider.size.y / 2 * m_GroundedInfo.m_flRaycastLength + 0.05f);

        //Epsilon of 0.05f
        if (hit.collider)
            GroundCharacter();
    }

    /**
    * \fn CheckGroundedFeet
    * \brief Checks grounded via feet child collision detection.
    * 
    * 
    **/
    void CheckGroundedFeet()
    {
        //Trigger check.
        if (m_GroundedInfo.m_pFeetCollider.isTrigger)
        {
            ContactFilter2D filter = new ContactFilter2D();
            Collider2D[] colliders = new Collider2D[16];

            if (m_GroundedInfo.m_pFeetCollider.OverlapCollider(filter, colliders) > 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (colliders[i] != null && colliders[i].gameObject != gameObject)
                    {
                        GroundCharacter();
                        return;
                    }
                }
            }

            return;
        }

        //NOTENOTE: 16 should be way more than enough.  Increase to detect more colliders but lower performance.
        ContactPoint2D[] hits = new ContactPoint2D[16];
        m_GroundedInfo.m_pFeetCollider.GetContacts(hits);

        //Go through each contact point to make sure that the character is **ACTUALLY** on the ground.
        foreach (var d in hits)
        {
            if (d.collider != null && d.collider.gameObject != gameObject)
            {
                GroundCharacter();
                return;
            }
        }
    }

    /**
    * \fn LPK_OnCollisionEnter2D
    * \brief Manages collision detection for grounding the character.  Super basic.
    * \param col - Holds information on the collision event.
    * 
    **/
    override protected void LPK_OnCollisionEnter2D(Collision2D col)
    {
        if (m_GroundedInfo.m_eGroundedCheckType != LPK_GroundedDetectionType.COLLISION)
            return;

        if (col.collider.isTrigger)
            return;

        //Go through each contact point to make sure that the character is **ACTUALLY** on the ground.
        foreach (var d in col.contacts)
        {
            if (d.point.y >= transform.position.y)
                return;
        }

        GroundCharacter();
    }

    /**
    * \fn LPK_OnCollisionExit2D
    * \brief Allows a grace jump when falling off a ledge, if desired.
    * \param col - Holds information on the collision event.
    * 
    **/
    protected override void LPK_OnCollisionExit2D(Collision2D col)
    {
        if (m_bAllowGraceJump)
            return;
        else
            m_GroundedInfo.m_bGrounded = false;
    }

    /**
    * \fn GroundCharacter
    * \brief Sets the character to be grounded, with an event dispatched.
    * 
    * 
    **/
    private void GroundCharacter()
    {
        //Dispatch event if we just landed
        LPK_EventManager.LPK_EventData data = new LPK_EventManager.LPK_EventData(gameObject, m_CharacterLandEventReceivers);

        LPK_EventList sendEvent = new LPK_EventList();
        sendEvent.m_CharacterEventTrigger = new LPK_EventList.LPK_CHARACTER_EVENTS[] { LPK_EventList.LPK_CHARACTER_EVENTS.LPK_CharacterLand };

        LPK_EventManager.InvokeEvent(sendEvent, data);

        //Set grounded flag and reset jumps
        m_GroundedInfo.m_bGrounded = true;
        m_bIsJumping = false;
        m_iAirJumpsUsed = 0;

        if (m_bPrintDebug)
            LPK_PrintError(this, "Character grounded.");
    }
}
