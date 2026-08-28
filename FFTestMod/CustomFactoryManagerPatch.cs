using System;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using MAX;
using MAX.Data;
using MAX.Game;
using MAX.Save;
using MAX.UI;
using PrimeTween;
using Sonity;
using UnityEngine;

namespace FFTestMod;

public class CustomFactoryManagerPatch {
    // static async UniTaskVoid EnterCustomFactoryInternal_MultiStep(
    //     CustomFactoryManager instance,
    //     CustomInstructionData customFactoryData,
    //     CustomInstructionData cameraTarget,
    //     bool isMovingToParent = false
    // ) {
    //     if (isMovingToParent) {
    //         var current = instance.CurrentEditingCustomFactoryData;
    //         while (current != null && current != customFactoryData) {
    //             var next = current.Owner.CustomInstructionOwner;
    //             await EnterCustomFactoryInternal_One(instance, next, current, isMovingToParent);
    //             current = next;
    //         }
    //     } else {
    //         await EnterCustomFactoryInternal_One(instance, customFactoryData, cameraTarget, isMovingToParent);
    //     }
    // }

    [HarmonyPatch(typeof(CustomFactoryManager), "EnterCustomFactoryInternal")]
    [HarmonyPrefix]
    static bool EnterCustomFactoryInternal_Prefix(
        CustomFactoryManager __instance,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent,
        ref UniTaskVoid __result
    ) {
        __result = EnterCustomFactoryInternal_Custom(
            __instance,
            customFactoryData, cameraTarget, isMovingToParent
        );
        return false;
    }

    static async UniTaskVoid EnterCustomFactoryInternal_Custom(
        CustomFactoryManager instance,
        CustomInstructionData customFactoryData,
        CustomInstructionData cameraTarget,
        bool isMovingToParent
    ) {
        Plugin.Logger.LogInfo("ijidsgs");
        Manager<SupervisorManager>.Instance.ChangeState(SupervisorManager.ManagerStates.Lock);
        instance.SetPrivateAccessorWithReflection(nameof(CustomFactoryManager.IsEnteringCustomFactory), true);
        CustomFactoryManager.OnEnteringCustomFactoryHandler enteringCustomFactory =
            instance.GetPrivateFieldWithReflection<CustomFactoryManager.OnEnteringCustomFactoryHandler>(
                nameof(CustomFactoryManager.OnEnteringCustomFactory));
        if (enteringCustomFactory != null)
            enteringCustomFactory(customFactoryData);
        CameraController camController = Manager<CameraManager>.Instance.Controller;
        if (!isMovingToParent)
            camController.ZoomInToProcessData(customFactoryData);
        else
            camController.ZoomOut();
        TweenSettings<float> lensDistortionTweenSettings = isMovingToParent
            ? instance.GetPrivateFieldWithReflection<TweenSettings<float>>("m_ZoomOutLensDistortionTweenSettings")
            : instance.GetPrivateFieldWithReflection<TweenSettings<float>>("m_ZoomInLensDistortionTweenSettings");
        // Manager<GlobalVolumeManager>.Instance.PlayLensDistortionAnim(lensDistortionTweenSettings);
        (isMovingToParent
            ? instance.GetPrivateFieldWithReflection<SoundEvent>("m_ExitSound")
            : instance.GetPrivateFieldWithReflection<SoundEvent>("m_EnterSound"))?.UIPlay();
        // await Manager<BlackFadeManager>.Instance.FadeInAsync();
        await UniTask.Delay(200);
        Manager<TimeManager>.Instance.Pause();
        UniTask uniTask = UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
        await uniTask;
        if (instance.GetPrivateFieldWithReflection<bool>("m_IsEditingCustomFactory")) {
            var ps = new object[] { customFactoryData, cameraTarget, isMovingToParent };
            AccessTools.Method(typeof(CustomFactoryManager), "SaveEditedCustomFactory").Invoke(instance,
                ps);
            customFactoryData = (CustomInstructionData)ps[0];
            cameraTarget = (CustomInstructionData)ps[1];
            isMovingToParent = (bool)ps[2];
        }

        bool isEnteringCustomFactory = customFactoryData != null;
        Manager<LoadedCustomFactoryManager>.Instance.ClearCustomFactory(instance.CurrentDataManager);
        Manager<NestedScreenManager>.Instance.ForceClearFactoriesInAnim();
        uniTask = UniTask.NextFrame();
        await uniTask;
        CustomInstructionData prevEditingFactoryData = instance.CurrentEditingCustomFactoryData;
        instance.SetPrivateAccessorWithReflection("CurrentEditingCustomFactoryData", customFactoryData);
        uniTask = UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
        await uniTask;
        instance.SetPrivateFieldWithReflection("m_IsEditingCustomFactory", false);
        if (isEnteringCustomFactory) {
            Manager<LoadedCustomFactoryManager>.Instance.InstantiateFromCustomFactory(customFactoryData);
            instance.FactoryHierarchyPath.gameObject.SetActive(true);
            instance.FactoryHierarchyPath.SetPath(customFactoryData);
            instance.GetPrivateFieldWithReflection<EditMenuBar>("m_EditMenuBar").ExitEditMode();
        } else {
            Manager<LoadedCustomFactoryManager>.Instance.InstantiateFactoryDefinition(
                instance.GetPrivateFieldWithReflection<FactoryDefinitionsSaveController>("m_Definitions").MainFactoryDefinition, DataManager.Main);
            instance.FactoryHierarchyPath.gameObject.SetActive(false);
            instance.GetPrivateFieldWithReflection<EditMenuBar>("m_EditMenuBar").EnterEditMode(false);
        }

        AccessTools.Method(typeof(CustomFactoryManager), "UpdateFloor").Invoke(instance, []);
        if (cameraTarget != null)
            camController.TeleportCameraToProcessData((IProcessData)cameraTarget);
        else
            camController.TeleportToBoundingBoxCenter();
        camController.ResetZoom(!isMovingToParent ? 1f : 0.0f);
        uniTask = UniTask.WaitForEndOfFrame();
        await uniTask;
        uniTask = UniTask.WaitForEndOfFrame();
        await uniTask;
        Manager<TimeManager>.Instance.Resume();
        if (isMovingToParent)
            camController.Zoom(0.5f);
        else
            camController.ZoomToBoundingBox();
        // Manager<GlobalVolumeManager>.Instance.PlayLensDistortionAnim(lensDistortionTweenSettings.WithDirection(false));
        // uniTask = Manager<BlackFadeManager>.Instance.FadeOutAsync();
        // await uniTask;
        await UniTask.Delay(200);
        instance.SetPrivateAccessorWithReflection("IsEnteringCustomFactory", false);
        CustomFactoryManager.OnEnteringCustomFactoryHandler exitedCustomFactory = instance.GetPrivateFieldWithReflection<CustomFactoryManager.OnEnteringCustomFactoryHandler>("OnExitedCustomFactory");
        if (exitedCustomFactory != null)
            exitedCustomFactory(prevEditingFactoryData);
        if (!isEnteringCustomFactory) {
            camController = (CameraController)null;
            lensDistortionTweenSettings = new TweenSettings<float>();
            prevEditingFactoryData = (CustomInstructionData)null;
        } else {
            CustomFactoryManager.OnEnteringCustomFactoryHandler enteredCustomFactory = instance.GetPrivateFieldWithReflection<CustomFactoryManager.OnEnteringCustomFactoryHandler>("OnEnteredCustomFactory");
            if (enteredCustomFactory == null) {
                camController = (CameraController)null;
                lensDistortionTweenSettings = new TweenSettings<float>();
                prevEditingFactoryData = (CustomInstructionData)null;
            } else {
                enteredCustomFactory(customFactoryData);
                camController = (CameraController)null;
                lensDistortionTweenSettings = new TweenSettings<float>();
                prevEditingFactoryData = (CustomInstructionData)null;
            }
        }
    }
}