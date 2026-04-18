using UnityEngine;
using Project.Scripts.Fractures;

namespace Project.Scripts.Fractures
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PreFractureSetup : MonoBehaviour
    {
        [Header("Fracture Settings")]
        public Anchor anchor = Anchor.Bottom;
        public int seed = 0;
        public int totalChunks = 30;
        public Material insideMaterial;
        public Material outsideMaterial;
        public float jointBreakForce = 500f;
        public float density = 1f;
    }
}