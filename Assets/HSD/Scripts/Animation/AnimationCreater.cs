using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if UNITY_EDITOR
public class AnimationCreater : MonoBehaviour
{
    [Serializable]
    public struct AnimationClipInfo
    {
        public Sprite[] sprites;
        public string monsterName;
        public string clipName;
    }
    public AnimationClipInfo[] clipInfos;
    
    [SerializeField] float fameRate;

    [ContextMenu("Create")]
    public void CreateAnimationClip()
    {
        for(int i =0; i < clipInfos.Length; i++)
        {
            AnimationClip clip = new AnimationClip();
            clip.frameRate = fameRate;

            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[clipInfos[i].sprites.Length];
            for (int j = 0; j < clipInfos[i].sprites.Length; j++)
            {
                keyframes[j] = new ObjectReferenceKeyframe();
                keyframes[j].time = j / clip.frameRate;
                keyframes[j].value = clipInfos[i].sprites[j];
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            if(!Directory.Exists($"Assets/Animation/Monster/{clipInfos[i].monsterName}"))
                Directory.CreateDirectory($"Assets/Animation/Monster/{clipInfos[i].monsterName}");

            string clipPath = $"Assets/Animation/Monster/{clipInfos[i].monsterName}/{clipInfos[i].clipName}.anim";

            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            {
                Debug.Log($"이미 존재하는 클립입니다: {clipPath}");
            }
            else
            {
                AssetDatabase.CreateAsset(clip, clipPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"AnimationClip 생성 완료: {clipPath}");
            }
            string controllerPath = $"Assets/Animation/Monster/{clipInfos[i].monsterName}/{clipInfos[i].monsterName}.controller";
            AnimatorController controller;

            if (!File.Exists(controllerPath))
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                Debug.Log($"AnimatorController 생성됨: {controllerPath}");
            }
            else
            {
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            }

            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;

            var existingState = Array.Find(sm.states, s => s.state.name == clipInfos[i].clipName);
            if (existingState.state == null)
            {
                AnimatorState state = sm.AddState(clipInfos[i].clipName);
                state.motion = clip;
                Debug.Log($"추가됨: {clipInfos[i].clipName} → {controllerPath}");
            }
            else
            {
                Debug.LogWarning($"이미 존재 : {clipInfos[i].clipName}");
            }
        }        
    }
}
#endif
