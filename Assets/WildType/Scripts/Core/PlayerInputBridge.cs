using UnityEngine;
using UnityEngine.InputSystem;
namespace WildType
{
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerInputBridge : MonoBehaviour
    {
        StageSession session;
        int pauseFrame = -1;
        int mateFrame = -1;
        bool pointerWasLocked, guiEscapeHeld, guiFamilyHeld, guiMateHeld;
        public void Configure(StageSession value) { session = value; }
        void Update()
        {
            if (!session || !session.Ready) return;
            var keyboard = Keyboard.current; var pad = Gamepad.current; var mouse = Mouse.current;
            if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame))
                TogglePause();
            if ((keyboard != null && keyboard.fKey.wasPressedThisFrame) || (pad != null && pad.buttonNorth.wasPressedThisFrame))
                TogglePause();
            if (session.Paused || !session.Player || session.Player.Vitals.Dead) return;
            Vector2 move = pad != null ? pad.leftStick.ReadValue() : Vector2.zero;
            if (keyboard != null)
            {
                move.x += (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
                move.y += (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            }
            move = Vector2.ClampMagnitude(move, 1);
            Vector3 forward = session.orbit.transform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = session.orbit.transform.right; right.y = 0; right.Normalize();
            session.Player.DesiredDirection = forward * move.y + right * move.x;
            session.Player.WantsSprint = (keyboard != null && keyboard.leftShiftKey.isPressed) || (pad != null && pad.leftStickButton.isPressed);
            if ((keyboard != null && keyboard.eKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame))
                session.Player.Interaction.TryEat();
            if ((keyboard != null && keyboard.mKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame))
                RequestMate();
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Vector2 look = mouse != null && locked && pointerWasLocked ? mouse.delta.ReadValue() * .13f : Vector2.zero;
            pointerWasLocked = locked;
            if (pad != null) look += pad.rightStick.ReadValue() * (115 * Time.unscaledDeltaTime);
            float zoom = mouse != null ? mouse.scroll.ReadValue().y / 120f : 0;
            session.orbit.Orbit(look, zoom);
        }
        void TogglePause()
        {
            if (!session || !session.Ready || pauseFrame == Time.frameCount) return;
            pauseFrame = Time.frameCount; session.SetPaused(!session.Paused); pointerWasLocked = false;
        }
        void OnGUI()
        {
            // Editor Game-view routing can consume brief taps before the Input System update.
            // Held-key and frame latches keep both routes from repeating/toggling twice.
            var key = Event.current.keyCode;
            if (key != KeyCode.Escape && key != KeyCode.F && key != KeyCode.M) return;
            if (Event.current.type == EventType.KeyUp)
            { if (key == KeyCode.Escape) guiEscapeHeld = false; if (key == KeyCode.F) guiFamilyHeld = false; if (key == KeyCode.M) guiMateHeld = false; }
            if (Event.current.type != EventType.KeyDown) return;
            if (key == KeyCode.M) { if (!guiMateHeld) { guiMateHeld = true; RequestMate(); } }
            else if (key == KeyCode.F) { if (!guiFamilyHeld) { guiFamilyHeld = true; TogglePause(); } }
            else if (!guiEscapeHeld) { guiEscapeHeld = true; TogglePause(); }
        }
        void RequestMate()
        {
            if (!session || !session.Ready || session.Paused || !session.Player || session.Player.Vitals.Dead || mateFrame == Time.frameCount) return;
            mateFrame = Time.frameCount; session.Generations.TryPlayerMate();
        }
        void OnApplicationFocus(bool focused) { if (!focused) { guiEscapeHeld = guiFamilyHeld = guiMateHeld = false; pointerWasLocked = false; } }
    }
}
