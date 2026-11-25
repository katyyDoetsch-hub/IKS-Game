using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ConcreteLife
{
    /// <summary>
    /// Testtasten nur fuer Entwickler (Besucher haben keine Tastatur).
    /// R = Szene neu starten.
    /// </summary>
    public class DevKeys : MonoBehaviour
    {
        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
