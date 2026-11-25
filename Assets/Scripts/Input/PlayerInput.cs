using UnityEngine;
using UnityEngine.InputSystem;

namespace ConcreteLife
{
    /// <summary>
    /// Einzige Stelle, an der Eingaben gelesen werden. Alle anderen Skripte fragen nur diese Klasse.
    /// Ausstellung: Zeiger/Licht (IR-/LED-Pointer, Air-Mouse) = Maus, Drehregler = Mausrad bzw. Pfeiltasten.
    /// Fuer andere Geraete spaeter nur diese Klasse anpassen.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance { get; private set; }

        /// Zeigerposition in Bildschirm-Pixeln.
        public Vector2 PointerPosition { get; private set; }
        /// Zeiger wird gedrueckt gehalten.
        public bool IsPressed { get; private set; }
        /// Zeiger wurde in diesem Frame gedrueckt.
        public bool PressedThisFrame { get; private set; }
        /// Zeiger wurde in diesem Frame losgelassen.
        public bool ReleasedThisFrame { get; private set; }
        /// Drehregler in diesem Frame (+ = rechts, - = links), ca. 1 pro Raste.
        public float DialDelta { get; private set; }

        [Tooltip("Drehregler-Einheiten pro Sekunde, wenn eine Pfeiltaste gehalten wird")]
        public float dialKeyRate = 4f;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float dial = 0f;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                PointerPosition = mouse.position.ReadValue();
                IsPressed = mouse.leftButton.isPressed;
                PressedThisFrame = mouse.leftButton.wasPressedThisFrame;
                ReleasedThisFrame = mouse.leftButton.wasReleasedThisFrame;
                float s = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(s) > 10f) s /= 120f; // manche Systeme liefern 120 pro Raste
                dial += s;
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.rightArrowKey.isPressed) dial += dialKeyRate * Time.deltaTime;
                if (kb.leftArrowKey.isPressed) dial -= dialKeyRate * Time.deltaTime;
            }
            DialDelta = dial;
        }
    }
}
