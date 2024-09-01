using UnityEditor;
using UnityEngine;

public class AddCapsuleCollidersToObjectEditor : EditorWindow
{
    private Vector3 colliderSize = new Vector3(0.1f, 0.5f, 0.1f); // Default size for the capsule colliders
    private int selectedAxis = 1; // Default to aligning with the Y axis
    private readonly string[] axisOptions = { "X", "Y", "Z" }; // Axis options

    [MenuItem("Tools/Add Capsule Colliders to Selected Objects")]
    public static void ShowWindow()
    {
        GetWindow<AddCapsuleCollidersToObjectEditor>("Add Capsule Colliders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Capsule Collider Settings", EditorStyles.boldLabel);

        colliderSize = EditorGUILayout.Vector3Field("Collider Size", colliderSize);
        selectedAxis = EditorGUILayout.Popup("Align with Axis", selectedAxis, axisOptions);

        if (GUILayout.Button("Add Colliders"))
        {
            AddCollidersToSelectedObjects();
        }
    }

    private void AddCollidersToSelectedObjects()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            AddColliderToBone(obj.transform);
        }
    }

    private void AddColliderToBone(Transform bone)
    {
        CapsuleCollider collider = bone.gameObject.AddComponent<CapsuleCollider>();
        bone.gameObject.AddComponent<HitDetection>();

        // Set collider size
        collider.height = colliderSize.y;
        collider.radius = colliderSize.x / 2;

        // Set collider alignment
        collider.direction = selectedAxis;

        // Set collider to trigger mode
        collider.isTrigger = true;
    }
}
