using System.Linq;
using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Haengt beim Start das Kontur-Material an alle Modellteile darunter an.
    /// Funktioniert automatisch auch mit dem spaeteren Blender-Modell (MeshRenderer und SkinnedMeshRenderer).
    /// </summary>
    public class RimOutline : MonoBehaviour
    {
        public Material rimMaterial;

        void Start()
        {
            if (rimMaterial == null) return;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
                var mats = r.sharedMaterials;
                if (mats.Contains(rimMaterial)) continue;
                r.sharedMaterials = mats.Concat(new[] { rimMaterial }).ToArray();
            }
        }
    }
}
