using UnityEngine;

public class VertexColorChecker : MonoBehaviour
{
    void Start()
    {
        // 1. Get the MeshFilter component attached to the GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Mesh mesh = meshFilter.sharedMesh;

            // 2. Check if the mesh contains the Color vertex attribute
            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
            {
                Debug.Log($"{gameObject.name} HAS vertex color data!");

                // Optional: Safely fetch the colors using the modern API
                // var colors = new System.Collections.Generic.List<Color>();
                // mesh.GetColors(colors);
            }
            else
            {
                Debug.Log($"{gameObject.name} does NOT have vertex color data.");
            }
        }
        else
        {
            Debug.LogWarning("No MeshFilter or Mesh found on this GameObject.");
        }
    }
}