using System;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using MAX.Data;
using MAX.Game;

namespace FFTestMod;

public class CustomFactoryManagerPatch {
    [HarmonyPatch(typeof(CustomFactoryManager), "EnterCustomFactoryInternal")]
    [HarmonyPrefix]
    static bool EnterCustomFactoryInternal_Prefix(
        CustomFactoryManager __instance,
        ref UniTaskVoid __result,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent = false
    ) {
        __result = EnterCustomFactoryInternal_MultiStep(__instance, customFactoryData, cameraTarget, isMovingToParent);
        return false;
    }
    //
    // static async UniTaskVoid EnterCustomFactoryInternal_MultiStep(CustomFactoryManager instance, ref bool ___IsEnteringCustomFactory) {
    //     AccessTools.Field(typeof(CustomFactoryManager), "IsEnteringCustomFactory").SetValue(instance, true);
    //     CustomFactoryManager.OnEnteringCustomFactoryHandler enteringCustomFactory = this.OnEnteringCustomFactory;
    //     if (enteringCustomFactory != null)
    //         enteringCustomFactory(customFactoryData);
    //     CameraController camController = Manager<CameraManager>.Instance.Controller;
    //     if (!isMovingToParent)
    //         camController.ZoomInToProcessData((IProcessData)customFactoryData);
    //     else
    //         camController.ZoomOut();
    //     TweenSettings<float> lensDistortionTweenSettings = isMovingToParent
    //         ? this.m_ZoomOutLensDistortionTweenSettings
    //         : this.m_ZoomInLensDistortionTweenSettings;
    //     Manager<GlobalVolumeManager>.Instance.PlayLensDistortionAnim(lensDistortionTweenSettings);
    //     (isMovingToParent ? this.m_ExitSound : this.m_EnterSound)?.UIPlay();
    //     await Manager<BlackFadeManager>.Instance.FadeInAsync();
    //     Manager<TimeManager>.Instance.Pause();
    //     UniTask uniTask = UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
    //     await uniTask;
    //     if (this.m_IsEditingCustomFactory)
    //         this.SaveEditedCustomFactory(ref customFactoryData, ref cameraTarget, ref isMovingToParent);
    //     bool isEnteringCustomFactory = customFactoryData != null;
    //     Manager<LoadedCustomFactoryManager>.Instance.ClearCustomFactory(this.CurrentDataManager);
    //     Manager<NestedScreenManager>.Instance.ForceClearFactoriesInAnim();
    //     uniTask = UniTask.NextFrame();
    //     await uniTask;
    //     CustomInstructionData prevEditingFactoryData = this.CurrentEditingCustomFactoryData;
    //     this.CurrentEditingCustomFactoryData = customFactoryData;
    //     uniTask = UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
    //     await uniTask;
    //     this.m_IsEditingCustomFactory = false;
    //     if (isEnteringCustomFactory) {
    //         Manager<LoadedCustomFactoryManager>.Instance.InstantiateFromCustomFactory(customFactoryData);
    //         this.FactoryHierarchyPath.gameObject.SetActive(true);
    //         this.FactoryHierarchyPath.SetPath(customFactoryData);
    //         this.m_EditMenuBar.ExitEditMode();
    //     } else {
    //         Manager<LoadedCustomFactoryManager>.Instance.InstantiateFactoryDefinition(
    //             this.m_Definitions.MainFactoryDefinition, DataManager.Main);
    //         this.FactoryHierarchyPath.gameObject.SetActive(false);
    //         this.m_EditMenuBar.EnterEditMode(false);
    //     }
    //
    //     this.UpdateFloor();
    //     if (cameraTarget != null)
    //         camController.TeleportCameraToProcessData((IProcessData)cameraTarget);
    //     else
    //         camController.TeleportToBoundingBoxCenter();
    //     camController.ResetZoom(!isMovingToParent ? 1f : 0.0f);
    //     uniTask = UniTask.WaitForEndOfFrame();
    //     await uniTask;
    //     uniTask = UniTask.WaitForEndOfFrame();
    //     await uniTask;
    //     Manager<TimeManager>.Instance.Resume();
    //     if (isMovingToParent)
    //         camController.Zoom(0.5f);
    //     else
    //         camController.ZoomToBoundingBox();
    //     Manager<GlobalVolumeManager>.Instance.PlayLensDistortionAnim(lensDistortionTweenSettings.WithDirection(false));
    //     uniTask = Manager<BlackFadeManager>.Instance.FadeOutAsync();
    //     await uniTask;
    //     this.IsEnteringCustomFactory = false;
    //     CustomFactoryManager.OnEnteringCustomFactoryHandler exitedCustomFactory = this.OnExitedCustomFactory;
    //     if (exitedCustomFactory != null)
    //         exitedCustomFactory(prevEditingFactoryData);
    //     if (!isEnteringCustomFactory) {
    //         camController = (CameraController)null;
    //         lensDistortionTweenSettings = new TweenSettings<float>();
    //         prevEditingFactoryData = (CustomInstructionData)null;
    //     } else {
    //         CustomFactoryManager.OnEnteringCustomFactoryHandler enteredCustomFactory = this.OnEnteredCustomFactory;
    //         if (enteredCustomFactory == null) {
    //             camController = (CameraController)null;
    //             lensDistortionTweenSettings = new TweenSettings<float>();
    //             prevEditingFactoryData = (CustomInstructionData)null;
    //         } else {
    //             enteredCustomFactory(customFactoryData);
    //             camController = (CameraController)null;
    //             lensDistortionTweenSettings = new TweenSettings<float>();
    //             prevEditingFactoryData = (CustomInstructionData)null;
    //         }
    //     }
    // }

    static async UniTaskVoid EnterCustomFactoryInternal_MultiStep(
        CustomFactoryManager instance,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent = false
    ) {
        if (isMovingToParent) {
            var current = instance.CurrentEditingCustomFactoryData;
            while (current != null && current != customFactoryData) {
                var next = current.Owner.CustomInstructionOwner;
                await EnterCustomFactoryInternal_One(instance, next, current, isMovingToParent);
                current = next;
            }
        } else {
            await EnterCustomFactoryInternal_One(instance, customFactoryData, cameraTarget, isMovingToParent);
        }
    }

    static async UniTask EnterCustomFactoryInternal_One(
        CustomFactoryManager instance,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent = false
    ) {
        EnterCustomFactoryInternal_Original(instance, customFactoryData, cameraTarget, isMovingToParent).Forget();
        await UniTask.WaitWhile(() => instance.IsEnteringCustomFactory);
    }

    [HarmonyPatch(typeof(CustomFactoryManager), "EnterCustomFactoryInternal")]
    [HarmonyReversePatch]
    static async UniTaskVoid EnterCustomFactoryInternal_Original(
        CustomFactoryManager instance,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent = false
    ) => throw new NotImplementedException();
}