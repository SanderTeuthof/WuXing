using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AddCapsuleCollidersToObjectEditor : EditorWindow
{
    private Vector3 colliderSize = new Vector3(0.1f, 0.5f, 0.1f); // Default size for the capsule colliders
    private bool alignWithBone = true; // Align colliders with bone direction

    [MenuItem("Tools/Add Capsule Colliders to Selected Objects")]
    public static void ShowWindow()
    {
        GetWindow<AddCapsuleCollidersToObjectEditor>("Add Capsule Colliders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Capsule Collider Settings", EditorStyles.boldLabel);

        colliderSize = EditorGUILayout.Vector3Field("Collider Size", colliderSize);
        alignWithBone = EditorGUILayout.Toggle("Align with Bone", alignWithBone);

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

        // Set collider size (you can adjust this based on your needs)
        collider.height = colliderSize.y;
        collider.radius = colliderSize.x / 2;

        if (alignWithBone)
        {
            collider.direction = GetColliderDirection(bone);
        }
    }

    private int GetColliderDirection(Transform bone)
    {
        Vector3 localUp = bone.InverseTransformDirection(Vector3.up);
        Vector3 localForward = bone.InverseTransformDirection(Vector3.forward);

        if (Mathf.Abs(localUp.y) > Mathf.Abs(localForward.y))
        {
            return 1; // Align with Y axis (common for humanoid bones)
        }
        else
        {
            return 2; // Align with Z axis
        }
    }
}
