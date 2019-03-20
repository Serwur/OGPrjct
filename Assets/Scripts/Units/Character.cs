using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public abstract class Character : Entity, PlayerInput.IStickListener, PlayerInput.IButtonListener, PlayerInput.ITriggerListener
{
    // Item który jest trzymany - PROPOZYCJA JEŻELI COŚ TAKIEGO BĘDZIE W GRZE
    //public Item holdingItem;

    // Referencja do klasy ekwipunku tj. przedmioty w plecaku - KOLEJNA PROPOZYCJA
    //public Equipment equipment;

    /*
     * Okej, ta klasa powinna być bazą pod dwie klasy postaci. Trzeba tu zawrzeć najważniejsze rzeczy takie jak sterowanie
     * najlepiej za pomocą klasy PlayerInput do wykorzystwania skilli. Referencje do drzewka talentów, ew. przedmiotów
     * które moglibyśmy dodać, jeżeli będzie taka potrzeba. Ekwipunek co najważniejsze chyba jeżeli nasza gra
     * będzie to obsługiwać.
     */

    [Header( "Useable Skills Reference" )]
    public Skill basicSkill;
    public Skill firstSkill;
    public Skill secondSkill;

    /// <summary>
    /// Do zaimplementowania skrypt ruchu gracza.
    /// </summary>
    public void Move(float x)
    {
        rb2d.velocity = new Vector2( x * moveSpeed.max * Time.fixedDeltaTime, rb2d.velocity.y ) ; 
    }

    public void Jump()
    {
        rb2d.velocity = new Vector2( rb2d.velocity.x, jumpPower.max );
    }

    public void OnButtonHeld(ButtonCode code)
    {
        Debug.Log("Held: " + code);
    }

    public void OnButtonPressed(ButtonCode code)
    {
        switch (code) {
            case ButtonCode.A:
                if ( rb2d.velocity.y == 0)
                    Jump();
                break;
            case ButtonCode.B:
                TemplateModifier jumpModifier = new TemplateModifier(0.2f, Modifier.Mod.POSITIVE, jumpPower, 5f);
                jumpModifier.StartTimer();
                break;
            case ButtonCode.X:
                // INTERAKCJA
                break;
            case ButtonCode.Y:
                TemplateModifier speedMod = new TemplateModifier( 0.1f, Modifier.Mod.NEGATIVE, moveSpeed, 3f );
                speedMod.StartTimer();
                break;
            case ButtonCode.LeftBumper:
                firstSkill.Use(); 
                break;
            case ButtonCode.RightBumper:
                secondSkill.Use();
                break;
            case ButtonCode.Start:
                break;
            case ButtonCode.Back:
                break;
            case ButtonCode.LeftStick:
                break;
            case ButtonCode.RightStick:
                break;
        }
    }

    public void OnButtonReleased(ButtonCode code)
    {
        Debug.Log("Released: " + code);
        switch (code) {
            case ButtonCode.A:
                break;
            case ButtonCode.B:
                break;
            case ButtonCode.X:
                break;
            case ButtonCode.Y:
                break;
            case ButtonCode.LeftBumper:
                break;
            case ButtonCode.RightBumper:
                break;
            case ButtonCode.Start:
                break;
            case ButtonCode.Back:
                break;
            case ButtonCode.LeftStick:
                break;
            case ButtonCode.RightStick:
                break;
        }
    }

    public void OnStickChange(JoystickDoubleAxis stick)
    {
    }

    public void OnStickHold(JoystickDoubleAxis stick)
    {
        switch (stick.Code) {
            case AxisCode.LeftStick:
                Move( stick.X );
                break;
            case AxisCode.RightStick:
                break;
        }
    }

    public void OnTriggerChange(JoystickAxis trigger)
    {
        if (trigger.Code == AxisCode.LeftTrigger) {
            // DRUGI SKILL
        }
    }

    public void OnTriggerHold(JoystickAxis trigger)
    {
        if ( trigger.Code == AxisCode.RightTrigger) {
            // BAZOWY ATAK
        }
    }

    public void OnDeadZone(JoystickDoubleAxis stick)
    {

    }
}
