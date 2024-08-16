using UnityEditor;
using UnityEngine;

public class AnimationClipLengthTool : EditorWindow
{
    private Animator animator;

    [MenuItem("Tools/Animation Clip Length Tool")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipLengthTool>("Animation Clip Length Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Clip Length Tool", EditorStyles.boldLabel);

        // Field to assign an Animator
        animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);

        if (animator != null)
        {
            if (GUILayout.Button("Print Animation Clip Lengths"))
            {
                PrintAnimationClipLengths();
            }
        }
        else
        {
            GUILayout.Label("Please assign an Animator to use this tool.", EditorStyles.helpBox);
        }
    }

    private void PrintAnimationClipLengths()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned.");
            return;
        }

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        if (controller == null)
        {
            Debug.LogError("No Animator Controller assigned to the Animator.");
            return;
        }

        foreach (var clip in controller.animationClips)
        {
            Debug.Log($"Animation Clip: {clip.name}, Length: {clip.length} seconds");
        }
    }
}