using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        // Create tree at position (0, 0, 0)
        TreeGenerator.CreateTree(Vector3.zero);
    }
}
