using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class MaterialSmoothSwapper : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Target Materials")]
    [SerializeField] private List<Material> targetMaterials = new List<Material>();

    [Header("Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    private MaterialPropertyBlock mpb;
    private Coroutine routine;

    // URP/Lit color property
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("Renderer missing", this);
            enabled = false;
            return;
        }

        mpb = new MaterialPropertyBlock();
    }

    // =====================================================

    public void Swap(int index)
    {
        if (index < 0 || index >= targetMaterials.Count)
        {
            Log($"Invalid index: {index}");
            return;
        }

        var targetMat = targetMaterials[index];
        if (targetMat == null)
        {
            Log("Target material is null");
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(CrossFadeRoutine(targetMat));
    }

    // =====================================================

    private IEnumerator CrossFadeRoutine(Material targetMat)
    {
        var currentMat = targetRenderer.material;

        // Ensure both materials are in Transparent mode so alpha works
        SetupURPTransparent(currentMat);
        SetupURPTransparent(targetMat);

        // Get current color
        targetRenderer.GetPropertyBlock(mpb);
        Color currentColor = currentMat.HasProperty(BaseColor)
            ? currentMat.GetColor(BaseColor)
            : Color.white;

        // -------- Fade OUT current --------
        float t = 0f;
        while (t < duration * 0.5f)
        {
            t += Time.deltaTime;
            float n = curve.Evaluate(t / (duration * 0.5f));

            Color c = currentColor;
            c.a = 1f - n;

            mpb.SetColor(BaseColor, c);
            targetRenderer.SetPropertyBlock(mpb);

            yield return null;
        }

        // -------- Swap material --------
        targetRenderer.material = targetMat;

        // Reset MPB for new material
        targetRenderer.GetPropertyBlock(mpb);

        Color targetColor = targetMat.HasProperty(BaseColor)
            ? targetMat.GetColor(BaseColor)
            : Color.white;

        // -------- Fade IN new --------
        t = 0f;
        while (t < duration * 0.5f)
        {
            t += Time.deltaTime;
            float n = curve.Evaluate(t / (duration * 0.5f));

            Color c = targetColor;
            c.a = n;

            mpb.SetColor(BaseColor, c);
            targetRenderer.SetPropertyBlock(mpb);

            yield return null;
        }

        // Finalize (full alpha, clear MPB)
        mpb.Clear();
        targetRenderer.SetPropertyBlock(mpb);

        Log($"Swap complete → {targetMat.name}");
    }

    // =====================================================
    // URP/Lit transparent setup (runtime)

    private void SetupURPTransparent(Material mat)
    {
        if (mat == null) return;

        // URP Lit uses these keywords/properties for surface type
        mat.SetFloat("_Surface", 1); // 0=Opaque, 1=Transparent
        mat.SetFloat("_Blend", 0);   // Alpha blending
        mat.SetFloat("_ZWrite", 0);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    // =====================================================

    private void Log(string msg)
    {
        if (enableDebug)
            Debug.Log($"[MaterialSwapper] {msg}", this);
    }
}