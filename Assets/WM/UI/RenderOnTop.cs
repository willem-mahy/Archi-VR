using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RenderOnTop : MonoBehaviour
{
    public UnityEngine.Rendering.CompareFunction comparison = UnityEngine.Rendering.CompareFunction.Always;

    public bool apply = false;

    private void Start()
    {
        Apply();
    }

    private void Update()
    {
        if (apply)
        {
            Apply();
        }
    }

    private void Apply()
    {
        //WM.Logger.Debug("RenderOnTop.Apply()");

        apply = false;
        
        var graphic = GetComponent<Graphic>();

        if (graphic == null)
        {
            WM.Logger.Warning("RenderOnTop: '" + gameObject.name + "' contains no graphic component!");
            return;
        }

        // Get a handle to the existing material from the graphic.
        var existingGlobalMat = graphic.materialForRendering;

        // Make a copy of the existing material.
        var updatedMaterial = new Material(existingGlobalMat);

        // Set the Z test mode on the copy.
        updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);

        // Set the copy material as the graphic material.
        graphic.material = updatedMaterial;
    }
}