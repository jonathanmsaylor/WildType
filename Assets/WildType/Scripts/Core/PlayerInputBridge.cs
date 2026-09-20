using UnityEngine;
using UnityEngine.InputSystem;
namespace WildType
{
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerInputBridge : MonoBehaviour
    {
        StageSession session;
        int pauseFrame = -1;
        bool pointerWasLocked, guiEscapeHeld;
        public void Configure(StageSession value) { session = value; }
        void Update()
        {
            if (!session || !session.Ready) return;
            var keyboard = Keyboard.current; var pad = Gamepad.current; var mouse = Mouse.current;
            if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame))
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
            // Editor Game-view shortcuts can consume Escape before the Input System update.
            // Both input routes use one frame latch, so a key never toggles twice.
            if (Event.current.keyCode != KeyCode.Escape) return;
            if (Event.current.type == EventType.KeyUp) guiEscapeHeld = false;
            if (Event.current.type == EventType.KeyDown && !guiEscapeHeld)
            {
                guiEscapeHeld = true; TogglePause();
            }
        }
        void OnApplicationFocus(bool focused) { if (!focused) { guiEscapeHeld = false; pointerWasLocked = false; } }
    }
}
