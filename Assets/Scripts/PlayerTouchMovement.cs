using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using DG.Tweening;

public class PlayerTouchMovement : MonoBehaviour
{
    [SerializeField]
    private Vector2 JoystickSize = new Vector2(300, 300); // Size of the joystick
    [SerializeField]
    private FloatingJoystick Joystick; // Reference to the FloatingJoystick component
    [SerializeField]
    private NavMeshAgent Player; // Reference to the NavMeshAgent component

    private Finger MovementFinger; // Finger used for movement
    private Vector2 MovementAmount; // Amount of movement based on joystick input

    private void OnEnable()
    {
        // Enable enhanced touch support and subscribe to touch events
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleLoseFinger;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        // Unsubscribe from touch events and disable enhanced touch support
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleLoseFinger;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void HandleFingerMove(Finger MovedFinger)
    {
        if (MovedFinger == MovementFinger)
        {
            Vector2 knobPosition;
            float maxMovement = JoystickSize.x / 2f; // Maximum joystick movement radius
            ETouch.Touch currentTouch = MovedFinger.currentTouch;

            if (Vector2.Distance(
                    currentTouch.screenPosition,
                    Joystick.RectTransform.anchoredPosition
                ) > maxMovement)
            {
                knobPosition = (
                    currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition
                    ).normalized
                    * maxMovement;
            }
            else
            {
                knobPosition = currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition;
            }

            Joystick.Knob.anchoredPosition = knobPosition; // Update joystick knob position
            MovementAmount = knobPosition / maxMovement; // Calculate movement amount based on knob position
        }
    }

    private void HandleLoseFinger(Finger LostFinger)
    {
        if (LostFinger == MovementFinger)
        {
            MovementFinger = null;
            Joystick.Knob.anchoredPosition = Vector2.zero; // Reset joystick knob position
            Joystick.gameObject.SetActive(false); // Hide joystick
            MovementAmount = Vector2.zero; // Reset movement amount
        }
    }

    private void HandleFingerDown(Finger TouchedFinger)
    {
        if (MovementFinger == null && TouchedFinger.screenPosition.x <= Screen.width / 2f)
        {
            MovementFinger = TouchedFinger; // Assign the movement finger
            MovementAmount = Vector2.zero; // Reset movement amount
            Joystick.gameObject.SetActive(true); // Show joystick
            Joystick.RectTransform.sizeDelta = JoystickSize; // Set joystick size
            Joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition); // Position joystick
        }
    }

    private Vector2 ClampStartPosition(Vector2 StartPosition)
    {
        // Ensure the joystick start position is within screen bounds
        if (StartPosition.x < JoystickSize.x / 2)
        {
            StartPosition.x = JoystickSize.x / 2;
        }

        if (StartPosition.y < JoystickSize.y / 2)
        {
            StartPosition.y = JoystickSize.y / 2;
        }
        else if (StartPosition.y > Screen.height - JoystickSize.y / 2)
        {
            StartPosition.y = Screen.height - JoystickSize.y / 2;
        }

        return StartPosition;
    }

    private void Update()
    {
        // Calculate scaled movement based on joystick input and player speed
        Vector3 scaledMovement = Player.speed * Time.deltaTime * new Vector3(
            MovementAmount.x,
            0,
            MovementAmount.y
        );

        if (MovementAmount != Vector2.zero)
        {
            Player.Move(scaledMovement); // Move the player

            Vector3 lookDirection = new Vector3(MovementAmount.x, 0, MovementAmount.y);

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Smoothly rotate towards the target rotation
            Player.transform.rotation = Quaternion.RotateTowards(Player.transform.rotation, targetRotation, 360 * Time.deltaTime);
        }
    }

    private void OnGUI()
    {
        // Define GUI style for debug information
        GUIStyle labelStyle = new GUIStyle()
        {
            fontSize = 24,
            normal = new GUIStyleState()
            {
                textColor = Color.white
            }
        };
        if (MovementFinger != null)
        {
            // Display finger start and current positions
            GUI.Label(new Rect(10, 35, 500, 20), $"Finger Start Position: {MovementFinger.currentTouch.startScreenPosition}", labelStyle);
            GUI.Label(new Rect(10, 65, 500, 20), $"Finger Current Position: {MovementFinger.currentTouch.screenPosition}", labelStyle);
        }
        else
        {
            // Display no movement touch information
            GUI.Label(new Rect(10, 35, 500, 20), "No Current Movement Touch", labelStyle);
        }

        // Display screen size information
        GUI.Label(new Rect(10, 10, 500, 20), $"Screen Size ({Screen.width}, {Screen.height})", labelStyle);
    }
}
