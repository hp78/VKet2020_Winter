#if VRC_SDK_VRCSDK3
using UnityEngine;
using VitDeck.Language;
using VRC.SDKBase;
using VRC.Udon;

namespace VitDeck.Validator
{

    /// <summary>
    /// Vket5 Udonの基本ルールセット。
    /// </summary>
    /// <remarks>
    /// 変数をabstractまたはvirtualプロパティで宣言し、継承先で上書きすることでワールドによる制限の違いを表現する。
    /// </remarks>
    public abstract class Vket5UdonRuleSetBase : IRuleSet
    {
        public abstract string RuleSetName
        {
            get;
        }

        protected readonly long MegaByte = 1048576;

        private readonly Vket5TargetFinder targetFinder = new Vket5TargetFinder();
        public IValidationTargetFinder TargetFinder => targetFinder;

        private readonly IObjectDetector officialPrefabsDetector;

        public Vket5UdonRuleSetBase() : base()
        {
            officialPrefabsDetector = new PrefabPartsDetector(
                Vket5UdonOfficialAssetData.AudioSourcePrefabGUIDs,
                Vket5UdonOfficialAssetData.AvatarPedestalPrefabGUIDs,
                Vket5UdonOfficialAssetData.PickupObjectSyncPrefabGUIDs,
                Vket5UdonOfficialAssetData.CanvasPrefabGUIDs,
                Vket5UdonOfficialAssetData.PointLightProbeGUIDs,
                Vket5UdonOfficialAssetData.UdonBehaviourPrefabGUIDs);
        }

        public IRule[] GetRules()
        {
            // デフォルトで使っていたAttribute式は宣言時にconst以外のメンバーが利用できない。
            // 継承したプロパティを参照して挙動を変えることが出来ない為、直接リストを返す方式に変更した。
            return new IRule[]
            {

                new UnityVersionRule(LocalizedMessage.Get("Vket5RuleSetBase.UnityVersionRule.Title", "2018.4.20f1"), "2018.4.20f1"),

                new VRCSDKVersionRule(LocalizedMessage.Get("Vket5RuleSetBase.VRCSDKVersionRule.Title"),
                    new VRCSDKVersion("2020.05.06.12.14"),
                    "https://files.vrchat.cloud/sdk/VRCSDK3-WORLD-2020.08.07.18.18_Public.unitypackage"),

                new ExistInSubmitFolderRule(LocalizedMessage.Get("Vket5RuleSetBase.ExistInSubmitFolderRule.Title"), Vket5UdonOfficialAssetData.GUIDs, targetFinder),

                new AssetGuidBlacklistRule(LocalizedMessage.Get("Vket5RuleSetBase.OfficialAssetDontContainRule.Title"), Vket5UdonOfficialAssetData.GUIDs),

                new AssetNamingRule(LocalizedMessage.Get("Vket5RuleSetBase.NameOfFileAndFolderRule.Title"), @"[a-zA-Z0-9 _\.\-\(\)]+"),

                new AssetPathLengthRule(LocalizedMessage.Get("Vket5RuleSetBase.FilePathLengthLimitRule.Title", 184), 184),

                new AssetExtentionBlacklistRule(LocalizedMessage.Get("Vket5RuleSetBase.MeshFileTypeBlacklistRule.Title"),
                                                new string[]{".ma", ".mb", "max", "c4d", ".blend"}
                ),

                new ContainMatOrTexInAssetRule(LocalizedMessage.Get("Vket5RuleSetBase.ContainMatOrTexInAssetRule.Title")),

                new FolderSizeRule(LocalizedMessage.Get("Vket5RuleSetBase.FolderSizeRule.Title"), FolderSizeLimit),

                new ExhibitStructureRule(LocalizedMessage.Get("Vket5RuleSetBase.ExhibitStructureRule.Title")),

                new StaticFlagRule(LocalizedMessage.Get("Vket5RuleSetBase.StaticFlagsRule.Title")),

                new BoothBoundsRule(LocalizedMessage.Get("Vket5RuleSetBase.BoothBoundsRule.Title"),
                    size: BoothSizeLimit,
                    margin: 0.01f,
                    Vector3.zero,
                    Vket5UdonOfficialAssetData.SizeIgnorePrefabGUIDs),

                new AssetTypeLimitRule(
                    LocalizedMessage.Get("Vket5RuleSetBase.MaterialLimitRule.Title", MaterialUsesLimit),
                    typeof(Material),
                    MaterialUsesLimit,
                    Vket5UdonOfficialAssetData.MaterialGUIDs),

                new LightmapSizeLimitRule(
                    LocalizedMessage.Get("Vket5RuleSetBase.LightMapsLimitRule.Title", LightmapCountLimit, 512),
                    lightmapCountLimit: LightmapCountLimit,
                    lightmapResolutionLimit: 512),

                new GlobalIlluminationBakedRule(LocalizedMessage.Get("Vket5RuleSetBase.GlobalIlluminationBakedRule.Title")),

                new UsableComponentListRule(LocalizedMessage.Get("Vket5RuleSetBase.UsableComponentListRule.Title"),
                    GetComponentReferences(),
                    ignorePrefabGUIDs: Vket5UdonOfficialAssetData.GUIDs),

                new SkinnedMeshRendererRule(LocalizedMessage.Get("Vket5RuleSetBase.SkinnedMeshRendererRule.Title")),

                new MeshRendererRule(LocalizedMessage.Get("Vket5RuleSetBase.MeshRendererRule.Title")),

                new ReflectionProbeRule(LocalizedMessage.Get("Vket5RuleSetBase.ReflectionProbeRule.Title")),

                new UseMeshColliderRule(LocalizedMessage.Get("Vket5RuleSetBase.UseMeshColliderRule.Title")),

                new LightCountLimitRule(LocalizedMessage.Get("Vket5RuleSetBase.DirectionalLightLimitRule.Title"), UnityEngine.LightType.Directional, 0),

                new LightConfigRule(LocalizedMessage.Get("Vket5RuleSetBase.PointLightConfigRule.Title"), UnityEngine.LightType.Point, ApprovedPointLightConfig),

                new LightConfigRule(LocalizedMessage.Get("Vket5RuleSetBase.SpotLightConfigRule.Title"), UnityEngine.LightType.Spot, ApprovedSpotLightConfig),

                new LightConfigRule(LocalizedMessage.Get("Vket5RuleSetBase.AreaLightConfigRule.Title"), UnityEngine.LightType.Area, ApprovedAreaLightConfig),

                new LightCountLimitRule(
                    LocalizedMessage.Get("Vket5RuleSetBase.AreaLightLimitRule.Title", AreaLightUsesLimit),
                    UnityEngine.LightType.Area,
                    AreaLightUsesLimit),

                new UseLightModeRule(LocalizedMessage.Get("Vket5RuleSetBase.PointLightModeRule.Title"), UnityEngine.LightType.Point, unusablePointLightModes),

                new UseLightModeRule(LocalizedMessage.Get("Vket5RuleSetBase.SpotLightModeRule.Title"), UnityEngine.LightType.Spot, unusableSpotLightModes),

                // new AnimationMakesMoveCollidersRule(LocalizedMessage.Get("Vket5RuleSetBase.AnimationMakesMoveCollidersRule.Title")),

                new AnimationClipRule(LocalizedMessage.Get("Vket5RuleSetBase.AnimationClipRule.Title")),

                new AnimationComponentRule(LocalizedMessage.Get("Vket5RuleSetBase.AnimationComponentRule.Title"), officialPrefabsDetector),

                new AnimatorComponentRule(LocalizedMessage.Get("Vket5RuleSetBase.AnimatorComponentRule.Title"),
                    new System.Type[]{
                        typeof(VRC_Pickup),
                        // typeof(VRC_ObjectSync)
                    },officialPrefabsDetector),

                new CanvasRenderModeRule(LocalizedMessage.Get("Vket5RuleSetBase.CanvasRenderModeRule.Title")),

                new CameraComponentRule(LocalizedMessage.Get("Vket5RuleSetBase.CameraComponentRule.Title"), maxRenderTextureSize: new Vector2(1024, 1024)),

                new CameraComponentMaxCountRule(LocalizedMessage.Get("Vket5RuleSetBase.CameraComponentMaxCountRule.Title"), limit: 1),

                new ProjectorComponentRule(LocalizedMessage.Get("Vket5RuleSetBase.ProjectorComponentRule.Title")),

                new ProjectorComponentMaxCountRule(LocalizedMessage.Get("Vket5RuleSetBase.ProjectorComponentMaxCountRule.Title"), limit: 1),

                new PickupObjectSyncPrefabRule(LocalizedMessage.Get("Vket5RuleSetBase.PickupObjectSyncRule.Title"), Vket5UdonOfficialAssetData.PickupObjectSyncPrefabGUIDs),

                new AvatarPedestalPrefabRule(LocalizedMessage.Get("Vket5RuleSetBase.AvatarPedestalPrefabRule.Title"), Vket5UdonOfficialAssetData.AvatarPedestalPrefabGUIDs),

                new AudioSourcePrefabRule(LocalizedMessage.Get("Vket5RuleSetBase.AudioSourcePrefabRule.Title"),  Vket5UdonOfficialAssetData.AudioSourcePrefabGUIDs),

                new RigidbodyRule(LocalizedMessage.Get("Vket5RuleSetBase.RigidbodyRule.Title")),

                new PrefabLimitRule(
                    LocalizedMessage.Get("Vket5RuleSetBase.UnusabePrefabRule.Title", ChairPrefabUsesLimit),
                    Vket5UdonOfficialAssetData.VRCSDKPrefabGUIDs,
                    0),

                new PrefabLimitRule(
                    LocalizedMessage.Get("Vket5RuleSetBase.PickupObjectSyncPrefabLimitRule.Title", PickupObjectSyncUsesLimit),
                    Vket5UdonOfficialAssetData.PickupObjectSyncPrefabGUIDs,
                    PickupObjectSyncUsesLimit),

                //// IN SDK3 Video Player is suspended.
                // new VideoPlayerComponentRule(LocalizedMessage.Get("Vket5RuleSetBase.VideoPlayerComponentRule.Title")),

                //// IN SDK3 Video Player is suspended.
                // new VideoPlayerComponentMaxCountRule(LocalizedMessage.Get("Vket5RuleSetBase.VideoPlayerComponentMaxCountRule.Title"), limit: 1),

                new AnimatorComponentMaxCountRule(LocalizedMessage.Get("Vket5RuleSetBase.AnimatorComponentMaxCountRule.Title"), limit: 50),

                // Udon Behaviour
                // UdonBehaviourを含むオブジェクト、UdonBehaviourによって操作を行うオブジェクトは全て入稿ルール C.Scene内階層規定におけるDynamicオブジェクトの階層下に入れてください
                new UdonDynamicObjectParentRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonDynamicObjectParentRule.Title")), 
                
                // 全てのUdonBehaviourオブジェクトの親であるDynamicオブジェクトは初期でInactive状態にしてください
                new UdonDynamicObjectInactiveRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonDynamicObjectInactiveRule.Title")), 

                // UdonBehaviourを含むオブジェクトのLayerはUserLayer23としてください
                new UdonBehaviourLayerConstraintRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonBehaviourLayerConstraintRule.Title")),

                // UdonBehaviourは1ブースあたり 25 まで
                new AssetTypeLimitRule(
                    LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonBehaviourLimitRule.Title", UdonBehaviourCountLimit),
                    typeof(UdonBehaviour),
                    UdonBehaviourCountLimit,
                    Vket5UdonOfficialAssetData.UdonBehaviourPrefabGUIDs),

                // SynchronizePositionが有効なUdonBehaviourは1ブースあたり 10 まで
                new UdonBehaviourSynchronizePositionCountLimitRule(
                    LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonBehaviourSynchronizePositionCountLimitRule.Title", UdonBehaviourSynchronizePositionCountLimit),
                    UdonBehaviourSynchronizePositionCountLimit
                ),

                // AllowOwnershipTransferOnCollisionは必ずFalseにすること
                new UdonBehaviourAllowOwnershipTransferOnCollisionIsFalseRule(LocalizedMessage.Get("UdonBehaviourAllowOwnershipTransferOnCollisionIsFalseRule.Title")),

                // VRCStation は1ブースあたり 8 まで
                new VRCStationCountLimitRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.VRCStationCountLimitRule.Title", VRCStationCountLimit), VRCStationCountLimit), 

                // VRCSpatialAudioSourceを含むオブジェクトは全てDynamicオブジェクトの階層下に入れてください
                new VRCSpatialAudioSourceDynamicObjectParentRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.X07_SpatialAudioDynamicObjectParentRule.Title")), 

                // VRCPickup は UdonBehaviour [AutoResetPickup] を持つ必要があります。
                new VRCPickupUdonBehaviourRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.X08_VRCPickupUdonBehaviourRule.Title")), 

                // UdonBehaviourによってオブジェクトをスペース外に移動させる行為は禁止
                // ⇒ スタッフによる目視確認

                // プレイヤーの設定(移動速度等)の変更はプレイヤーがスペース内にいる場合のみ許可されます
                // ⇒ スタッフによる目視確認

                // プレイヤーの位置変更(テレポート)は、プレイヤーがスペース内にいる状態 スペース内のどこかに移動させる
                // ⇒ スタッフによる目視確認

                // Udon Script
                // 使用禁止UdonAssembly
                new UsableUdonAssemblyListRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UsableUdonAssemblyListRule.Title"),
                    GetUdonAssemblyReferences(),
                    ignorePrefabGUIDs: Vket5UdonOfficialAssetData.GUIDs), 

                // [UdonSynced]を付与した変数は1ブースあたり 3 まで
                // [UdonSynced]を付与した変数は下記の型のみ使用できます bool, sbyte, byte, ushort, short, uint, int, float
                new UdonBehaviourSyncedVariablesRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonBehaviourSyncedVariablesRule.Title"), UdonScriptSyncedVariablesLimit), 

                // U#においては、全てのクラスは運営よりブース毎に指定するnamespaceに所属させてください
                new UdonSharpScriptNamespaceRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonSharpNameSpaceRule.Title"), "Vket5.Circle"), 

                // PhysicsクラスのCast関数 layerMaskを設定し、レイヤー23以外のコライダを無視するようにする, maxDistanceは最長で10メートルまで
                new UdonAssemblyPhysicsCastFunctionRule(LocalizedMessage.Get("Vket5UdonRuleSetBase.UdonAssemblyPhysicsCastFunctionRule.Title"), GetUdonAssemblyPhysicsCastFunctionReferences()), 

            };
        }

        protected abstract long FolderSizeLimit { get; }

        protected abstract Vector3 BoothSizeLimit { get; }

        protected abstract int UdonBehaviourCountLimit { get; }

        protected abstract int UdonBehaviourSynchronizePositionCountLimit { get; }

        protected abstract int UdonScriptSyncedVariablesLimit { get; }

        protected abstract int MaterialUsesLimit { get; }

        protected abstract int LightmapCountLimit { get; }

        protected abstract int VRCStationCountLimit { get; }

        private ComponentReference[] GetComponentReferences()
        {
            return new ComponentReference[] {
                new ComponentReference("Rigidbody", new string[]{"UnityEngine.Rigidbody"}, ValidationLevel.ALLOW),
                new ComponentReference("Cloth", new string[]{"UnityEngine.Cloth"}, ValidationLevel.DISALLOW),
                new ComponentReference("Collider", new string[]{"UnityEngine.SphereCollider", "UnityEngine.BoxCollider", "UnityEngine.SphereCollider", "UnityEngine.CapsuleCollider", "UnityEngine.MeshCollider", "UnityEngine.WheelCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone", new string[]{"DynamicBone"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone Collider", new string[]{"DynamicBoneCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Skinned Mesh Renderer", new string[]{"UnityEngine.SkinnedMeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Renderer ", new string[]{"UnityEngine.MeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Filter", new string[]{"UnityEngine.MeshFilter"}, ValidationLevel.ALLOW),
                new ComponentReference("Particle System", new string[]{"UnityEngine.ParticleSystem", "UnityEngine.ParticleSystemRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Trail Renderer", new string[]{"UnityEngine.TrailRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Line Renderer", new string[]{"UnityEngine.LineRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Light", new string[]{"UnityEngine.Light"}, AdvancedObjectValidationLevel),
                new ComponentReference("LightProbeGroup", new string[]{"UnityEngine.LightProbeGroup"}, AdvancedObjectValidationLevel),
                new ComponentReference("ReflectionProbe", new string[]{"UnityEngine.ReflectionProbe"}, AdvancedObjectValidationLevel),
                new ComponentReference("Camera", new string[]{"UnityEngine.Camera"}, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Projector", new string[]{"UnityEngine.Projector"}, MoreAdvancedObjectValidationLevel),
                new ComponentReference("LookatTarget", new string[]{"UnityStandardAssets.Cameras.LookatTarget" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("FollowTarget", new string[]{"UnityStandardAssets.Utility.FollowTarget" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Suspension", new string[]{"UnityStandardAssets.Vehicles.Car.Suspension" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Animator", new string[]{"UnityEngine.Animator"}, ValidationLevel.ALLOW),
                new ComponentReference("Animation", new string[]{"UnityEngine.Animation"}, ValidationLevel.ALLOW),
                new ComponentReference("Audio Source", new string[]{"UnityEngine.AudioSource", "ONSPAudioSource", "VRCSDK2.VRC_SpatialAudioSource"}, ValidationLevel.ALLOW),
                new ComponentReference("Canvas", new string[]{"UnityEngine.Canvas", "UnityEngine.CanvasGroup", "UnityEngine.RectTransform", "UnityEngine.UI.CanvasScaler", "UnityEngine.UI.GraphicRaycaster", "UnityEngine.UI.AspectRatioFitter", "UnityEngine.UI.LayoutElement", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.HorizontalLayoutGroup", "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.GridLayoutGroup", "UnityEngine.UI.Text", "UnityEngine.UI.Image", "UnityEngine.UI.RawImage", "UnityEngine.UI.Mask", "UnityEngine.UI.RectMask2D", "UnityEngine.UI.Button", "UnityEngine.UI.InputField", "UnityEngine.UI.Toggle", "UnityEngine.UI.ToggleGroup", "UnityEngine.UI.Slider", "UnityEngine.UI.Scrollbar", "UnityEngine.UI.Dropdown", "UnityEngine.UI.ScrollRect", "UnityEngine.UI.Selectable", "UnityEngine.UI.Shadow", "UnityEngine.UI.Outline", "UnityEngine.UI.PositionAsUV1", "UnityEngine.RectTransform", "UnityEngine.CanvasRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("VideoPlayer", new string[]{"UnityEngine.Video.VideoPlayer" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("EventSystem", new string[]{"UnityEngine.EventSystems.EventSystem", "UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("StandaloneInputModule", new string[]{"UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("PlayableDirector", new string[]{"UnityEngine.Playables.PlayableDirector" }, ValidationLevel.DISALLOW),

                // VRCSDK2
                new ComponentReference("VRC_Trigger", new string[]{"VRCSDK2.VRC_Trigger", "VRCSDK2.VRC_EventHandler"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Object Sync", new string[]{"VRCSDK2.VRC_ObjectSync"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Pickup", new string[]{"VRCSDK2.VRC_Pickup"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Audio Bank", new string[]{"VRCSDK2.VRC_AudioBank"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Avatar Pedestal", new string[]{"VRCSDK2.VRC_AvatarPedestal"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Ui Shape", new string[]{"VRCSDK2.VRC_UiShape"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Station", new string[]{"VRCSDK2.VRC_Station"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Mirror", new string[]{ "VRCSDK2.VRC_MirrorCamera", "VRCSDK2.VRC_MirrorReflection" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_PlayerAudioOverride", new string[]{"VRCSDK2.VRC_PlayerAudioOverride"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Panorama", new string[]{"VRCSDK2.scripts.Scenes.VRC_Panorama", "VRC.SDKBase.VRC_Panorama" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SyncVideoPlayer", new string[]{"VRCSDK2.VRC_SyncVideoPlayer", "VRCSDK2.VRC_SyncVideoStream", "VRC.SDKBase.VRC_SyncVideoPlayer", "VRC.SDKBase.VRC_SyncVideoStream" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SceneResetPosition", new string[]{"VRCSDK2.VRC_SceneResetPosition"}, ValidationLevel.DISALLOW),

                // VRCSDK3
                //// VRC_Trigger is obsolete. Use instead Udon Behaviour
                new ComponentReference("VRC Trigger", new string[]{"VRC.SDKBase.VRC_Trigger", "VRC.SDK3.Components.VRCTrigger"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Pickup", new string[]{"VRC.SDKBase.VRC_Pickup", "VRC.SDK3.Components.VRCPickup"}, ValidationLevel.ALLOW),
                new ComponentReference("VRC Station", new string[]{"VRC.SDKBase.VRCStation", "VRC.SDK3.Components.VRCStation"}, ValidationLevel.ALLOW),
                new ComponentReference("VRC Avatar Pedestal", new string[]{"VRC.SDKBase.VRC_AvatarPedestal", "VRC.SDK3.Components.VRCAvatarPedestal"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Mirror Reflection", new string[]{"VRC.SDKBase.VRC_MirrorReflection", "VRC.SDK3.Components.VRC_MirrorReflection"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Ui Shape", new string[]{"VRC.SDKBase.VRC_UiShape", "VRC.SDK3.Components.VRCUiShape"}, AdvancedObjectValidationLevel),
                new ComponentReference("VRC Url Input Field", new string[]{"VRC.SDK3.Components.VRCUrlInputField"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Visual Damage", new string[]{"VRC.SDKBase.VRC_VisualDamage", "VRC.SDK3.Components.VRCVisualDamage"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Spacial Audio Source", new string[]{"VRC.SDKBase.VRC_SpatialAudioSource", "VRC.SDK3.Components.VRCSpatialAudioSource"}, ValidationLevel.ALLOW),
                new ComponentReference("VRC Unity Video Player", new string[]{"VRC.SDK3.Video.Components.VRCUnityVideoPlayer", }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC AV Pro Video Player", new string[]{"VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer", "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker", "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Portal Marker", new string[]{"VRC.SDKBase.VRC_PortalMarker", "VRC.SDK3.Components.VRCPortalMarker"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Scene Descriptor", new string[]{"VRC.SDKBase.VRC_SceneDescriptor", "VRC.SDK3.Components.VRCSceneDescriptor"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Test Marker", new string[]{"VRC.SDK3.VRCTestMarker"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC Project Settings", new string[]{"VRCProjectSettings"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Sdk Builder", new string[]{"VRC.SDK3.Editor.VRC_SdkBuilder"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Event Handler(Obsolete)", new string[]{"VRC.SDKBase.VRC_EventHandler", "VRC.SDK3.Components.VRCEventHandler"}, ValidationLevel.DISALLOW),
                new ComponentReference("Udon Behaviour", new string[]{"VRC.Udon.UdonBehaviour", "VRC.SDKBase.VRC_Interactable"}, MoreAdvancedObjectValidationLevel),
            };
        }

        private UdonAssemblyReference[] GetUdonAssemblyReferences()
        {
            return new UdonAssemblyReference[]
            {
                // Variables
                new UdonAssemblyReference("Transform.root", new string[]{"__get_root__UnityEngineTransform", "__set_root__UnityEngineTransform"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("GameObject.Layer", new string[]{"UnityEngineGameObject.__get_layer__SystemInt32", "UnityEngineGameObject.__set_layer__SystemInt32"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("RenderSettings", new string[]{"UnityEngineRenderSettings"}, ValidationLevel.DISALLOW),

                // Functions
                new UdonAssemblyReference("UdonSharpBehaviour.VRCInstantiate", new string[]{"VRCInstantiate"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("GameObject.Find", new string[]{"__Find__SystemString__UnityEngineGameObject"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("Object.Destroy", new string[]{"UnityEngineObject.__Destroy__UnityEngineObject__SystemVoid"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("Object.DestroyImmediate", new string[]{"UnityEngineObject.__DestroyImmediate__UnityEngineObject__SystemVoid"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("Transform.DetachChildren", new string[]{"UnityEngineTransform.__DetachChildren__SystemVoid"}, ValidationLevel.DISALLOW),
                new UdonAssemblyReference("VRCSDK3VideoPlayer", new string[]{"VRCSDK3VideoComponentsBaseBaseVRCVideoPlayer"}, ValidationLevel.DISALLOW),
            };
        }

        private UdonAssemblyFunctionEssentialArgumentReference[] GetUdonAssemblyPhysicsCastFunctionReferences()
        {
            return new UdonAssemblyFunctionEssentialArgumentReference[]
            {
                new UdonAssemblyFunctionEssentialArgumentReference(
                    "Physics.RayCast",
                    "MaxDistance, LayerMask",
                    new []{"UnityEnginePhysics.__Raycast__", "UnityEnginePhysics.__RaycastAll__", "UnityEnginePhysics.__RaycastNonAlloc__"},
                    "_SystemSingle_SystemInt32_"), 
                new UdonAssemblyFunctionEssentialArgumentReference(
                    "Physics.BoxCast",
                    "MaxDistance, LayerMask",
                    new []{"UnityEnginePhysics.__Boxcast__", "UnityEnginePhysics.__BoxCastAll__", "UnityEnginePhysics.__BoxCastNonAlloc__"},
                    "_SystemSingle_SystemInt32_"), 
                new UdonAssemblyFunctionEssentialArgumentReference(
                    "Physics.SphereCast",
                    "MaxDistance, LayerMask",
                    new []{"UnityEnginePhysics.__SphereCast__", "UnityEnginePhysics.__SphereCastAll__", "UnityEnginePhysics.__SphereCastNonAlloc__"},
                    "_SystemSingle_SystemInt32_"), 
                new UdonAssemblyFunctionEssentialArgumentReference(
                    "Physics.CapsuleCast",
                    "MaxDistance, LayerMask",
                    new []{"UnityEnginePhysics.__CapsuleCast__", "UnityEnginePhysics.__CapsuleCastAll__", "UnityEnginePhysics.__CapsuleCastNonAlloc__"},
                    "_SystemSingle_SystemInt32_"), 
                new UdonAssemblyFunctionEssentialArgumentReference(
                    "Physics.LineCast",
                    "LayerMask",
                    new []{"UnityEnginePhysics.__Linecast__"},
                    "_SystemInt32_"), 
            };
        }
        protected virtual ValidationLevel AdvancedObjectValidationLevel
        {
            get
            {
                return ValidationLevel.ALLOW;
            }
        }

        protected virtual ValidationLevel MoreAdvancedObjectValidationLevel
        {
            get
            {
                return ValidationLevel.DISALLOW;
            }
        }

        protected abstract LightConfigRule.LightConfig ApprovedPointLightConfig { get; }

        protected abstract LightConfigRule.LightConfig ApprovedSpotLightConfig { get; }

        protected abstract LightConfigRule.LightConfig ApprovedAreaLightConfig { get; }

        protected abstract LightmapBakeType[] unusablePointLightModes { get; }

        protected abstract LightmapBakeType[] unusableSpotLightModes { get; }

        protected abstract int AreaLightUsesLimit { get; }

        protected abstract int ChairPrefabUsesLimit { get; }

        protected abstract int PickupObjectSyncUsesLimit { get; }

    }
}
#endif