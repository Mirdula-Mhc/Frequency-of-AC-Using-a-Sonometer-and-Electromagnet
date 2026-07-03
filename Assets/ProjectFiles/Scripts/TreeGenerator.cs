using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public static void CreateTree(Vector3 position)
    {
        // Create root container
        GameObject tree = new GameObject("Tree");
        tree.transform.position = position;

        // Create trunk
        GameObject trunk = CreateCylinder("Trunk", Vector3.zero, new Vector3(0.5f, 3f, 0.5f), tree.transform);
        trunk.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f); // Brown

        // Create foliage (sphere)
        GameObject foliage = CreateSphere("Foliage", new Vector3(0, 2.5f, 0), Vector3.one * 2.5f, tree.transform);
        foliage.GetComponent<Renderer>().material.color = new Color(0.2f, 0.6f, 0.2f); // Green

        // Remove colliders if not needed
        if (trunk.TryGetComponent<Collider>(out var trunkCollider))
            DestroyImmediate(trunkCollider);
        if (foliage.TryGetComponent<Collider>(out var foliageCollider))
            DestroyImmediate(foliageCollider);
    }

    private static GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent);
        cylinder.transform.localPosition = position;
        cylinder.transform.localScale = scale;
        return cylinder;
    }

    private static GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = scale;
        return sphere;
    }
}
