using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeEyedGames
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour
    {
        private const string _deferredShaderName = "Decalicious/Deferred Decal";
        private const string _unlitShaderName = "Decalicious/Unlit Decal";
        private Shader _deferredShader;
        private Shader _unlitShader;

        public enum DecalRenderMode
        {
            Deferred,
            Unlit,
            Invalid
        }

        public DecalRenderMode RenderMode = DecalRenderMode.Invalid;

        [Tooltip("Set a Material with a Decalicious shader.")]
        public Material Material;

        [Tooltip("To which degree should the Decal be drawn? At 1, the Decal will be drawn with full effect. At 0, the Decal will not be drawn. Experiment with values greater than one.")]
        public float Fade = 1.0f;

        [Tooltip("Set a GameObject here to only draw this Decal on the MeshRenderer of the GO or any of its children.")]
        public GameObject LimitTo = null;

        [Tooltip("Enable to draw the Albedo / Emission pass of the Decal.")]
        public bool DrawAlbedo = true;

        [Tooltip("Use an interpolated light probe for this decal for indirect light. This breaks instancing for the decal and thus comes with a performance impact, so use with caution.")]
        public bool UseLightProbes = true;

        [Tooltip("Enable to draw the Normal / SpecGloss pass of the Decal.")]
        public bool DrawNormalAndGloss = true;

        [Tooltip("Enable perfect Normal / SpecGloss blending between decals. Costly and has no effect when decals don't overlap, so use with caution.")]
        public bool HighQualityBlending = false;

        protected Color _colorTransparent = new Color(0, 0, 0, 0);

        void OnEnable()
        {
            _deferredShader = Shader.Find(_deferredShaderName);
            _unlitShader = Shader.Find(_unlitShaderName);
        }

        void Reset()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            mf.sharedMesh = Resources.Load<Mesh>("DecalCube");

            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.materials = new Material[] { };
            mr.lightProbeUsage = LightProbeUsage.BlendProbes;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        void OnWillRenderObject()
        {
#if UNITY_EDITOR
            // Need to set the light probe usage for legacy (< Decalicious 1.4) decals,
            // only do it in editor to not affect game performance.
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.lightProbeUsage = LightProbeUsage.BlendProbes;
#endif

            if (Fade <= 0.0f)
                return;

            if (Material == null)
                return;

            if (Camera.current == null)
                return;

            if (Material == null)
                RenderMode = DecalRenderMode.Invalid;
            else if (Material.shader == _deferredShader)
                RenderMode = DecalRenderMode.Deferred;
            else if (Material.shader == _unlitShader)
                RenderMode = DecalRenderMode.Unlit;
            else
                RenderMode = DecalRenderMode.Invalid;

            DecaliciousRenderer renderer = Camera.current.GetComponent<DecaliciousRenderer>();
            if (renderer == null)
                renderer = Camera.current.gameObject.AddComponent<DecaliciousRenderer>();
#if UNITY_5_6_OR_NEWER
            Material.enableInstancing = renderer.UseInstancing;
#endif
            renderer.Add(this, LimitTo);
        }

        void OnDrawGizmos()
        {
            // Draw an invisible cube for selection
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = _colorTransparent;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            // Draw a faint wireframe
            Gizmos.color = Color.white * 0.2f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        void OnDrawGizmosSelected()
        {
            // Draw a well-visible wireframe
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white * 0.5f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}