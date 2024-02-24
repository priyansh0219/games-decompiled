using System;
using System.Collections.Generic;
using ProtoBuf;
using ProtoBuf.Meta;
using Story;
using UnityEngine;

public sealed class ProtobufSerializerPrecompiled : TypeModel
{
	public const ulong version = 13uL;

	private static readonly Dictionary<Type, int> knownTypes = new Dictionary<Type, int>
	{
		{
			typeof(AmbientLightSettings),
			1804294731
		},
		{
			typeof(AmbientSettings),
			2000594119
		},
		{
			typeof(AnalyticsController),
			7443242
		},
		{
			typeof(AnimatorParameterValue),
			880630407
		},
		{
			typeof(AnteChamber),
			624729416
		},
		{
			typeof(AtmosphereVolume),
			239375628
		},
		{
			typeof(AttackLargeTarget),
			773384208
		},
		{
			typeof(AttractedByLargeTarget),
			1117396675
		},
		{
			typeof(AuroraWarnings),
			1307861365
		},
		{
			typeof(Base),
			1966747463
		},
		{
			typeof(Base.Face),
			497398254
		},
		{
			typeof(BaseAddBulkheadGhost),
			1896640149
		},
		{
			typeof(BaseAddCellGhost),
			383673295
		},
		{
			typeof(BaseAddConnectorGhost),
			633495696
		},
		{
			typeof(BaseAddCorridorGhost),
			1506893401
		},
		{
			typeof(BaseAddFaceGhost),
			1090472122
		},
		{
			typeof(BaseAddFaceModuleGhost),
			1761765682
		},
		{
			typeof(BaseAddLadderGhost),
			2060501761
		},
		{
			typeof(BaseAddMapRoomGhost),
			1422648694
		},
		{
			typeof(BaseAddModuleGhost),
			862251519
		},
		{
			typeof(BaseAddPartitionDoorGhost),
			1406047219
		},
		{
			typeof(BaseAddPartitionGhost),
			490474621
		},
		{
			typeof(BaseAddWaterPark),
			1127946067
		},
		{
			typeof(BaseBioReactor),
			1584912799
		},
		{
			typeof(BaseCell),
			2146397475
		},
		{
			typeof(BaseDeconstructable),
			1469570205
		},
		{
			typeof(BaseExplicitFace),
			371084514
		},
		{
			typeof(BaseFloodSim),
			1725266300
		},
		{
			typeof(BaseGhost),
			1585678550
		},
		{
			typeof(BaseNuclearReactor),
			1384707171
		},
		{
			typeof(BasePipeConnector),
			882866214
		},
		{
			typeof(BasePowerDistributor),
			1504796287
		},
		{
			typeof(BaseRoot),
			311542795
		},
		{
			typeof(BaseSpotLight),
			1045809045
		},
		{
			typeof(BaseUpgradeConsole),
			1537880658
		},
		{
			typeof(Battery),
			1659399257
		},
		{
			typeof(BatteryCharger),
			788408569
		},
		{
			typeof(BatterySource),
			929691462
		},
		{
			typeof(Beacon),
			1711377162
		},
		{
			typeof(Bioreactor),
			1011566978
		},
		{
			typeof(BirdBehaviour),
			40841636
		},
		{
			typeof(Biter),
			649861234
		},
		{
			typeof(Bladderfish),
			953017598
		},
		{
			typeof(Bleeder),
			1041366111
		},
		{
			typeof(BloomCreature),
			829906308
		},
		{
			typeof(BlueprintHandTarget),
			1012963343
		},
		{
			typeof(BoneShark),
			242904679
		},
		{
			typeof(Boomerang),
			1716510642
		},
		{
			typeof(Breach),
			1772819529
		},
		{
			typeof(CaveCrawler),
			1176920975
		},
		{
			typeof(CellManager.CellHeader),
			1956492848
		},
		{
			typeof(CellManager.CellHeaderEx),
			949139443
		},
		{
			typeof(CellManager.CellsFileHeader),
			153467737
		},
		{
			typeof(Charger),
			642877324
		},
		{
			typeof(CoffeeMachineLegacy),
			237257528
		},
		{
			typeof(CollectShiny),
			812739867
		},
		{
			typeof(ColoredLabel),
			515927774
		},
		{
			typeof(Constructable),
			396637907
		},
		{
			typeof(ConstructableBase),
			817046334
		},
		{
			typeof(Constructor),
			326697518
		},
		{
			typeof(ConstructorInput),
			257036954
		},
		{
			typeof(CrabSnake),
			1998792992
		},
		{
			typeof(CrabSquid),
			49368518
		},
		{
			typeof(Crafter),
			135876261
		},
		{
			typeof(CrafterLogic),
			2100222829
		},
		{
			typeof(CraftingAnalytics),
			1703576080
		},
		{
			typeof(CraftingAnalytics.EntryData),
			1103056798
		},
		{
			typeof(Crash),
			1234455261
		},
		{
			typeof(CrashedShipExploder),
			1711422107
		},
		{
			typeof(CrashHome),
			660968298
		},
		{
			typeof(Creature),
			356643005
		},
		{
			typeof(CreatureBehaviour),
			406373562
		},
		{
			typeof(CreatureDeath),
			801838245
		},
		{
			typeof(CreatureEgg),
			59821228
		},
		{
			typeof(CreatureFriend),
			2001815917
		},
		{
			typeof(CreepvineSeed),
			730995178
		},
		{
			typeof(CurrentGenerator),
			860890656
		},
		{
			typeof(CuteFish),
			1880729675
		},
		{
			typeof(CyclopsDecoyLoadingTube),
			1489909791
		},
		{
			typeof(CyclopsLightingPanel),
			2083308735
		},
		{
			typeof(CyclopsMotorMode),
			2062152363
		},
		{
			typeof(DayNightCycle),
			315488296
		},
		{
			typeof(DayNightLight),
			26260290
		},
		{
			typeof(DisableBeforeExplosion),
			898747676
		},
		{
			typeof(DiveReel),
			10881664
		},
		{
			typeof(DiveReelAnchor),
			1954617231
		},
		{
			typeof(Drillable),
			2100518191
		},
		{
			typeof(DropEnzymes),
			662718200
		},
		{
			typeof(Durable),
			790665541
		},
		{
			typeof(Eatable),
			1080881920
		},
		{
			typeof(EnergyMixin),
			1427962681
		},
		{
			typeof(EntitySlot),
			1922274571
		},
		{
			typeof(EntitySlotData),
			1404549125
		},
		{
			typeof(EntitySlotsPlaceholder),
			1971823303
		},
		{
			typeof(EscapePod),
			443301464
		},
		{
			typeof(ExecutionOrderTest),
			1433797848
		},
		{
			typeof(Exosuit),
			1174302959
		},
		{
			typeof(Eyeye),
			351821017
		},
		{
			typeof(Fabricator),
			606619525
		},
		{
			typeof(FairRandomizer),
			776720259
		},
		{
			typeof(FiltrationMachine),
			1004993247
		},
		{
			typeof(FireExtinguisher),
			325421431
		},
		{
			typeof(FireExtinguisherHolder),
			1618351061
		},
		{
			typeof(Flare),
			304513582
		},
		{
			typeof(FogSettings),
			108625815
		},
		{
			typeof(FruitPlant),
			41335609
		},
		{
			typeof(Garryfish),
			1816388137
		},
		{
			typeof(GasoPod),
			476284185
		},
		{
			typeof(GenericConsole),
			459225532
		},
		{
			typeof(GhostCrafter),
			630473610
		},
		{
			typeof(GhostLeviatanVoid),
			1817260405
		},
		{
			typeof(GhostLeviathan),
			35332337
		},
		{
			typeof(GhostPickupable),
			530216075
		},
		{
			typeof(GhostRay),
			1389926401
		},
		{
			typeof(Grabcrab),
			2033371878
		},
		{
			typeof(Grid3<float>),
			1182280616
		},
		{
			typeof(Grid3<Vector3>),
			1765532648
		},
		{
			typeof(Grid3Shape),
			997267884
		},
		{
			typeof(Grower),
			697741374
		},
		{
			typeof(GrowingPlant),
			792963384
		},
		{
			typeof(GrownPlant),
			113236186
		},
		{
			typeof(HandTarget),
			2066256250
		},
		{
			typeof(Holefish),
			646820922
		},
		{
			typeof(Hoopfish),
			328727906
		},
		{
			typeof(Hoverfish),
			1859566378
		},
		{
			typeof(Incubator),
			1024693509
		},
		{
			typeof(InfectedMixin),
			2004070125
		},
		{
			typeof(Int3),
			1691070576
		},
		{
			typeof(Int3.Bounds),
			1164389947
		},
		{
			typeof(Int3Class),
			289177516
		},
		{
			typeof(Inventory),
			51037994
		},
		{
			typeof(Jellyray),
			1338410882
		},
		{
			typeof(JointHelper),
			1430634702
		},
		{
			typeof(Jumper),
			1252700319
		},
		{
			typeof(KeypadDoorConsole),
			1032585975
		},
		{
			typeof(KeypadDoorConsoleUnlock),
			1059166753
		},
		{
			typeof(LargeRoomWaterPark),
			1793440205
		},
		{
			typeof(LargeWorldBatchRoot),
			1878920823
		},
		{
			typeof(LargeWorldEntity),
			577496260
		},
		{
			typeof(LargeWorldEntityCell),
			122249312
		},
		{
			typeof(LaserCutObject),
			769725772
		},
		{
			typeof(LavaLarva),
			353579366
		},
		{
			typeof(LavaLizard),
			1882829928
		},
		{
			typeof(LavaShell),
			1703248490
		},
		{
			typeof(LeakingRadiation),
			656832482
		},
		{
			typeof(LEDLight),
			200911463
		},
		{
			typeof(Leviathan),
			1332964068
		},
		{
			typeof(LiveMixin),
			729882159
		},
		{
			typeof(MapRoomCamera),
			1855998104
		},
		{
			typeof(MapRoomCameraDocking),
			1304741297
		},
		{
			typeof(MapRoomFunctionality),
			503490010
		},
		{
			typeof(MapRoomScreen),
			1343493277
		},
		{
			typeof(MedicalCabinet),
			670501331
		},
		{
			typeof(Mesmer),
			123477383
		},
		{
			typeof(NitrogenLevel),
			1761034218
		},
		{
			typeof(NotificationManager.NotificationData),
			1874975849
		},
		{
			typeof(NotificationManager.NotificationId),
			1731663660
		},
		{
			typeof(NotificationManager.SerializedData),
			348559960
		},
		{
			typeof(NuclearReactor),
			802993602
		},
		{
			typeof(OculusFish),
			1829844631
		},
		{
			typeof(Oxygen),
			190681690
		},
		{
			typeof(OxygenPipe),
			569028242
		},
		{
			typeof(PDAEncyclopedia.Entry),
			1244614203
		},
		{
			typeof(PDALog.Entry),
			1329426611
		},
		{
			typeof(PDAScanner.Data),
			2137760429
		},
		{
			typeof(PDAScanner.Entry),
			248259089
		},
		{
			typeof(Peeper),
			1124891617
		},
		{
			typeof(PickPrefab),
			1289092887
		},
		{
			typeof(Pickupable),
			723629918
		},
		{
			typeof(PictureFrame),
			1527139359
		},
		{
			typeof(PingInstance),
			205484837
		},
		{
			typeof(Pipe),
			837446894
		},
		{
			typeof(PipeSurfaceFloater),
			955443574
		},
		{
			typeof(Plantable),
			542223321
		},
		{
			typeof(Player),
			1875862075
		},
		{
			typeof(PlayerSoundTrigger),
			10406022
		},
		{
			typeof(PlayerTimeCapsule),
			407275749
		},
		{
			typeof(PlayerWorldArrows),
			1675048343
		},
		{
			typeof(PowerCellCharger),
			1386435207
		},
		{
			typeof(PowerCrafter),
			1800229096
		},
		{
			typeof(PowerGenerator),
			797173896
		},
		{
			typeof(PowerSource),
			1522229446
		},
		{
			typeof(PrecursorAquariumPlatformTrigger),
			356984973
		},
		{
			typeof(PrecursorComputerTerminal),
			1541346828
		},
		{
			typeof(PrecursorDisableGunTerminal),
			1457139245
		},
		{
			typeof(PrecursorDoorKeyColumn),
			1505210158
		},
		{
			typeof(PrecursorElevator),
			185046565
		},
		{
			typeof(PrecursorGunStoryEvents),
			285680569
		},
		{
			typeof(PrecursorKeyTerminal),
			2051895012
		},
		{
			typeof(PrecursorPrisonVent),
			496960061
		},
		{
			typeof(PrecursorSurfaceVent),
			1169611549
		},
		{
			typeof(PrecursorTeleporter),
			1340645883
		},
		{
			typeof(PrecursorTeleporterActivationTerminal),
			1713660373
		},
		{
			typeof(PrefabPlaceholdersGroup),
			2111234577
		},
		{
			typeof(PrisonManager),
			851981872
		},
		{
			typeof(PropulseCannonAmmoHandler),
			1677184373
		},
		{
			typeof(ProtobufSerializer.ComponentHeader),
			313352173
		},
		{
			typeof(ProtobufSerializer.GameObjectData),
			1159039820
		},
		{
			typeof(ProtobufSerializer.LoopHeader),
			69304398
		},
		{
			typeof(ProtobufSerializer.StreamHeader),
			217879716
		},
		{
			typeof(ProtobufSerializerCornerCases),
			328142573
		},
		{
			typeof(RabbitRay),
			1082285174
		},
		{
			typeof(ReaperLeviathan),
			1702762571
		},
		{
			typeof(Reefback),
			1651949009
		},
		{
			typeof(ReefbackCreature),
			803494206
		},
		{
			typeof(ReefbackLife),
			1077102969
		},
		{
			typeof(ReefbackPlant),
			1948869956
		},
		{
			typeof(Reginald),
			924544622
		},
		{
			typeof(ResourceTrackerDatabase),
			859052613
		},
		{
			typeof(ResourceTrackerDatabase.ResourceInfo),
			161874211
		},
		{
			typeof(Respawn),
			11492366
		},
		{
			typeof(RestoreAnimatorState),
			253947874
		},
		{
			typeof(RestoreDayNightCycle),
			273953122
		},
		{
			typeof(RestoreEscapePodPosition),
			1092217753
		},
		{
			typeof(RestoreEscapePodStorage),
			1755470585
		},
		{
			typeof(RestoreInventoryStorage),
			2024062491
		},
		{
			typeof(Rocket),
			1273669126
		},
		{
			typeof(RocketConstructorInput),
			1170326782
		},
		{
			typeof(RocketPreflightCheckManager),
			916327342
		},
		{
			typeof(Roost),
			82819465
		},
		{
			typeof(SandShark),
			81014147
		},
		{
			typeof(SaveLoadManager.OptionsCache),
			846562516
		},
		{
			typeof(SceneObjectData),
			1881457915
		},
		{
			typeof(SceneObjectDataSet),
			2013353155
		},
		{
			typeof(SeaDragon),
			1606650114
		},
		{
			typeof(SeaEmperorBaby),
			1327055215
		},
		{
			typeof(SeaEmperorJuvenile),
			1921909083
		},
		{
			typeof(Sealed),
			1086395194
		},
		{
			typeof(SeaMoth),
			2106147891
		},
		{
			typeof(SeamothStorageContainer),
			1319564559
		},
		{
			typeof(SeaTreader),
			574222852
		},
		{
			typeof(Shocker),
			1535225367
		},
		{
			typeof(Sign),
			1084867387
		},
		{
			typeof(Signal),
			1261146858
		},
		{
			typeof(SignalPing),
			1732754958
		},
		{
			typeof(Skyray),
			469144263
		},
		{
			typeof(Slime),
			1355655442
		},
		{
			typeof(SolarPanel),
			524984727
		},
		{
			typeof(Spadefish),
			647827065
		},
		{
			typeof(SpawnStoredLoot),
			1993981848
		},
		{
			typeof(SpineEel),
			1058623975
		},
		{
			typeof(Stalker),
			1440414490
		},
		{
			typeof(StarshipDoor),
			737691900
		},
		{
			typeof(Stillsuit),
			242125589
		},
		{
			typeof(StorageContainer),
			366077262
		},
		{
			typeof(ScheduledGoal),
			704466011
		},
		{
			typeof(StoryGoalManager),
			1982063882
		},
		{
			typeof(StoryGoalScheduler),
			1506278612
		},
		{
			typeof(StoryGoalCustomEventHandler),
			502247445
		},
		{
			typeof(SubFire),
			1537030892
		},
		{
			typeof(SubRoot),
			1180542256
		},
		{
			typeof(SunlightSettings),
			1603999327
		},
		{
			typeof(SupplyCrate),
			1555952712
		},
		{
			typeof(Survival),
			888605288
		},
		{
			typeof(SwimRandom),
			225324449
		},
		{
			typeof(SwimToMeat),
			1157251956
		},
		{
			typeof(KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>),
			1852174394
		},
		{
			typeof(KeyValuePair<int, SceneObjectData>),
			1116754719
		},
		{
			typeof(KeyValuePair<string, PDAEncyclopedia.Entry>),
			121008834
		},
		{
			typeof(KeyValuePair<string, PDALog.Entry>),
			1062675616
		},
		{
			typeof(KeyValuePair<string, SceneObjectData>),
			890247896
		},
		{
			typeof(KeyValuePair<string, bool>),
			1489031418
		},
		{
			typeof(KeyValuePair<string, int>),
			224877162
		},
		{
			typeof(KeyValuePair<string, float>),
			714689774
		},
		{
			typeof(KeyValuePair<string, string>),
			524780017
		},
		{
			typeof(KeyValuePair<string, TimeCapsuleContent>),
			1249808124
		},
		{
			typeof(KeyValuePair<TechType, CraftingAnalytics.EntryData>),
			1158933531
		},
		{
			typeof(TechFragment),
			212928398
		},
		{
			typeof(TechLight),
			1671198874
		},
		{
			typeof(TeleporterManager),
			2104816441
		},
		{
			typeof(Terraformer),
			645269343
		},
		{
			typeof(ThermalPlant),
			1983352738
		},
		{
			typeof(TileInstance),
			746584541
		},
		{
			typeof(TimeCapsule),
			1386031502
		},
		{
			typeof(TimeCapsuleContent),
			1111417537
		},
		{
			typeof(TimeCapsuleItem),
			2044575597
		},
		{
			typeof(ToggleLights),
			1877364825
		},
		{
			typeof(AnimationCurve),
			118512508
		},
		{
			typeof(Behaviour),
			1825589276
		},
		{
			typeof(Bounds),
			391689956
		},
		{
			typeof(BoxCollider),
			892833698
		},
		{
			typeof(CapsuleCollider),
			1590521044
		},
		{
			typeof(Collider),
			72619689
		},
		{
			typeof(Color),
			1404661584
		},
		{
			typeof(Component),
			394322812
		},
		{
			typeof(Gradient),
			175529349
		},
		{
			typeof(GradientAlphaKey),
			1518696882
		},
		{
			typeof(GradientColorKey),
			1747546845
		},
		{
			typeof(Keyframe),
			1975582319
		},
		{
			typeof(Light),
			1364639273
		},
		{
			typeof(MonoBehaviour),
			2028243609
		},
		{
			typeof(UnityEngine.Object),
			1891515754
		},
		{
			typeof(Quaternion),
			605020259
		},
		{
			typeof(SphereCollider),
			1762542304
		},
		{
			typeof(Transform),
			149935601
		},
		{
			typeof(Vector2),
			1181346080
		},
		{
			typeof(Vector3),
			1181346079
		},
		{
			typeof(Vector4),
			1181346078
		},
		{
			typeof(UnstuckPlayer),
			438265826
		},
		{
			typeof(UpgradeConsole),
			1297003913
		},
		{
			typeof(Vehicle),
			1113570486
		},
		{
			typeof(VFXConstructing),
			2124310223
		},
		{
			typeof(Warper),
			328683587
		},
		{
			typeof(WaterPark),
			522953267
		},
		{
			typeof(WaterParkCreature),
			1054395028
		},
		{
			typeof(WeldableWallPanelGeneric),
			840818195
		},
		{
			typeof(Workbench),
			1333430695
		}
	};

	private static readonly HashSet<Type> emptyContracts = new HashSet<Type>
	{
		typeof(AttackLargeTarget),
		typeof(AttractedByLargeTarget),
		typeof(BaseCell),
		typeof(Bioreactor),
		typeof(Breach),
		typeof(CollectShiny),
		typeof(ConstructorInput),
		typeof(Crafter),
		typeof(DisableBeforeExplosion),
		typeof(Durable),
		typeof(ExecutionOrderTest),
		typeof(Fabricator),
		typeof(GhostCrafter),
		typeof(GhostPickupable),
		typeof(GrowingPlant),
		typeof(HandTarget),
		typeof(LargeWorldEntityCell),
		typeof(MapRoomScreen),
		typeof(NuclearReactor),
		typeof(PowerCrafter),
		typeof(PowerGenerator),
		typeof(PrecursorGunStoryEvents),
		typeof(ReefbackCreature),
		typeof(ReefbackPlant),
		typeof(RocketConstructorInput),
		typeof(SwimRandom),
		typeof(SwimToMeat),
		typeof(KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>),
		typeof(KeyValuePair<int, SceneObjectData>),
		typeof(KeyValuePair<string, PDAEncyclopedia.Entry>),
		typeof(KeyValuePair<string, PDALog.Entry>),
		typeof(KeyValuePair<string, SceneObjectData>),
		typeof(KeyValuePair<string, bool>),
		typeof(KeyValuePair<string, int>),
		typeof(KeyValuePair<string, float>),
		typeof(KeyValuePair<string, string>),
		typeof(KeyValuePair<string, TimeCapsuleContent>),
		typeof(KeyValuePair<TechType, CraftingAnalytics.EntryData>),
		typeof(Component),
		typeof(UnityEngine.Object),
		typeof(Workbench)
	};

	public static bool IsEmptyContract(Type key)
	{
		return emptyContracts.Contains(key);
	}

	protected override int GetKeyImpl(Type key)
	{
		if (knownTypes.TryGetValue(key, out var value))
		{
			return value;
		}
		return -1;
	}

	protected override void Serialize(int num, object obj, ProtoWriter writer)
	{
		switch (num)
		{
		case 7443242:
			Serialize7443242((AnalyticsController)obj, 7443242, writer);
			break;
		case 10406022:
			Serialize10406022((PlayerSoundTrigger)obj, 10406022, writer);
			break;
		case 10881664:
			Serialize10881664((DiveReel)obj, 10881664, writer);
			break;
		case 11492366:
			Serialize11492366((Respawn)obj, 11492366, writer);
			break;
		case 26260290:
			Serialize26260290((DayNightLight)obj, 26260290, writer);
			break;
		case 35332337:
			Serialize356643005((GhostLeviathan)obj, 35332337, writer);
			break;
		case 40841636:
			Serialize356643005((BirdBehaviour)obj, 40841636, writer);
			break;
		case 41335609:
			Serialize41335609((FruitPlant)obj, 41335609, writer);
			break;
		case 49368518:
			Serialize356643005((CrabSquid)obj, 49368518, writer);
			break;
		case 51037994:
			Serialize51037994((Inventory)obj, 51037994, writer);
			break;
		case 59821228:
			Serialize59821228((CreatureEgg)obj, 59821228, writer);
			break;
		case 69304398:
			Serialize69304398((ProtobufSerializer.LoopHeader)obj, 69304398, writer);
			break;
		case 72619689:
			Serialize72619689((Collider)obj, 72619689, writer);
			break;
		case 81014147:
			Serialize356643005((SandShark)obj, 81014147, writer);
			break;
		case 82819465:
			Serialize82819465((Roost)obj, 82819465, writer);
			break;
		case 108625815:
			Serialize108625815((FogSettings)obj, 108625815, writer);
			break;
		case 113236186:
			Serialize2066256250((GrownPlant)obj, 113236186, writer);
			break;
		case 118512508:
			Serialize118512508((AnimationCurve)obj, 118512508, writer);
			break;
		case 121008834:
			Serialize121008834((KeyValuePair<string, PDAEncyclopedia.Entry>)obj, 121008834, writer);
			break;
		case 122249312:
			Serialize122249312((LargeWorldEntityCell)obj, 122249312, writer);
			break;
		case 123477383:
			Serialize356643005((Mesmer)obj, 123477383, writer);
			break;
		case 135876261:
			Serialize2066256250((Crafter)obj, 135876261, writer);
			break;
		case 149935601:
			Serialize149935601((Transform)obj, 149935601, writer);
			break;
		case 153467737:
			Serialize153467737((CellManager.CellsFileHeader)obj, 153467737, writer);
			break;
		case 161874211:
			Serialize161874211((ResourceTrackerDatabase.ResourceInfo)obj, 161874211, writer);
			break;
		case 175529349:
			Serialize175529349((Gradient)obj, 175529349, writer);
			break;
		case 185046565:
			Serialize185046565((PrecursorElevator)obj, 185046565, writer);
			break;
		case 190681690:
			Serialize190681690((Oxygen)obj, 190681690, writer);
			break;
		case 200911463:
			Serialize200911463((LEDLight)obj, 200911463, writer);
			break;
		case 205484837:
			Serialize205484837((PingInstance)obj, 205484837, writer);
			break;
		case 212928398:
			Serialize212928398((TechFragment)obj, 212928398, writer);
			break;
		case 217879716:
			Serialize217879716((ProtobufSerializer.StreamHeader)obj, 217879716, writer);
			break;
		case 224877162:
			Serialize224877162((KeyValuePair<string, int>)obj, 224877162, writer);
			break;
		case 225324449:
			Serialize225324449((SwimRandom)obj, 225324449, writer);
			break;
		case 237257528:
			Serialize237257528((CoffeeMachineLegacy)obj, 237257528, writer);
			break;
		case 239375628:
			Serialize239375628((AtmosphereVolume)obj, 239375628, writer);
			break;
		case 242125589:
			Serialize242125589((Stillsuit)obj, 242125589, writer);
			break;
		case 242904679:
			Serialize356643005((BoneShark)obj, 242904679, writer);
			break;
		case 248259089:
			Serialize248259089((PDAScanner.Entry)obj, 248259089, writer);
			break;
		case 253947874:
			Serialize253947874((RestoreAnimatorState)obj, 253947874, writer);
			break;
		case 257036954:
			Serialize2066256250((ConstructorInput)obj, 257036954, writer);
			break;
		case 273953122:
			Serialize273953122((RestoreDayNightCycle)obj, 273953122, writer);
			break;
		case 285680569:
			Serialize285680569((PrecursorGunStoryEvents)obj, 285680569, writer);
			break;
		case 289177516:
			Serialize289177516((Int3Class)obj, 289177516, writer);
			break;
		case 304513582:
			Serialize304513582((Flare)obj, 304513582, writer);
			break;
		case 311542795:
			Serialize1180542256((BaseRoot)obj, 311542795, writer);
			break;
		case 313352173:
			Serialize313352173((ProtobufSerializer.ComponentHeader)obj, 313352173, writer);
			break;
		case 315488296:
			Serialize315488296((DayNightCycle)obj, 315488296, writer);
			break;
		case 325421431:
			Serialize325421431((FireExtinguisher)obj, 325421431, writer);
			break;
		case 326697518:
			Serialize326697518((Constructor)obj, 326697518, writer);
			break;
		case 328142573:
			Serialize328142573((ProtobufSerializerCornerCases)obj, 328142573, writer);
			break;
		case 328683587:
			Serialize356643005((Warper)obj, 328683587, writer);
			break;
		case 328727906:
			Serialize356643005((Hoopfish)obj, 328727906, writer);
			break;
		case 348559960:
			Serialize348559960((NotificationManager.SerializedData)obj, 348559960, writer);
			break;
		case 351821017:
			Serialize356643005((Eyeye)obj, 351821017, writer);
			break;
		case 353579366:
			Serialize356643005((LavaLarva)obj, 353579366, writer);
			break;
		case 356643005:
			Serialize356643005((Creature)obj, 356643005, writer);
			break;
		case 356984973:
			Serialize356984973((PrecursorAquariumPlatformTrigger)obj, 356984973, writer);
			break;
		case 366077262:
			Serialize2066256250((StorageContainer)obj, 366077262, writer);
			break;
		case 371084514:
			Serialize371084514((BaseExplicitFace)obj, 371084514, writer);
			break;
		case 383673295:
			Serialize1585678550((BaseAddCellGhost)obj, 383673295, writer);
			break;
		case 391689956:
			Serialize391689956((Bounds)obj, 391689956, writer);
			break;
		case 394322812:
			Serialize394322812((Component)obj, 394322812, writer);
			break;
		case 396637907:
			Serialize2066256250((Constructable)obj, 396637907, writer);
			break;
		case 406373562:
			Serialize406373562((CreatureBehaviour)obj, 406373562, writer);
			break;
		case 407275749:
			Serialize407275749((PlayerTimeCapsule)obj, 407275749, writer);
			break;
		case 438265826:
			Serialize438265826((UnstuckPlayer)obj, 438265826, writer);
			break;
		case 443301464:
			Serialize443301464((EscapePod)obj, 443301464, writer);
			break;
		case 459225532:
			Serialize459225532((GenericConsole)obj, 459225532, writer);
			break;
		case 469144263:
			Serialize356643005((Skyray)obj, 469144263, writer);
			break;
		case 476284185:
			Serialize356643005((GasoPod)obj, 476284185, writer);
			break;
		case 490474621:
			Serialize1585678550((BaseAddPartitionGhost)obj, 490474621, writer);
			break;
		case 496960061:
			Serialize496960061((PrecursorPrisonVent)obj, 496960061, writer);
			break;
		case 497398254:
			Serialize497398254((Base.Face)obj, 497398254, writer);
			break;
		case 502247445:
			Serialize502247445((StoryGoalCustomEventHandler)obj, 502247445, writer);
			break;
		case 503490010:
			Serialize503490010((MapRoomFunctionality)obj, 503490010, writer);
			break;
		case 515927774:
			Serialize2066256250((ColoredLabel)obj, 515927774, writer);
			break;
		case 522953267:
			Serialize522953267((WaterPark)obj, 522953267, writer);
			break;
		case 524780017:
			Serialize524780017((KeyValuePair<string, string>)obj, 524780017, writer);
			break;
		case 524984727:
			Serialize524984727((SolarPanel)obj, 524984727, writer);
			break;
		case 530216075:
			Serialize2066256250((GhostPickupable)obj, 530216075, writer);
			break;
		case 542223321:
			Serialize542223321((Plantable)obj, 542223321, writer);
			break;
		case 569028242:
			Serialize569028242((OxygenPipe)obj, 569028242, writer);
			break;
		case 574222852:
			Serialize356643005((SeaTreader)obj, 574222852, writer);
			break;
		case 577496260:
			Serialize577496260((LargeWorldEntity)obj, 577496260, writer);
			break;
		case 605020259:
			Serialize605020259((Quaternion)obj, 605020259, writer);
			break;
		case 606619525:
			Serialize2066256250((Fabricator)obj, 606619525, writer);
			break;
		case 624729416:
			Serialize624729416((AnteChamber)obj, 624729416, writer);
			break;
		case 630473610:
			Serialize2066256250((GhostCrafter)obj, 630473610, writer);
			break;
		case 633495696:
			Serialize1585678550((BaseAddConnectorGhost)obj, 633495696, writer);
			break;
		case 642877324:
			Serialize642877324((Charger)obj, 642877324, writer);
			break;
		case 645269343:
			Serialize645269343((Terraformer)obj, 645269343, writer);
			break;
		case 646820922:
			Serialize356643005((Holefish)obj, 646820922, writer);
			break;
		case 647827065:
			Serialize356643005((Spadefish)obj, 647827065, writer);
			break;
		case 649861234:
			Serialize356643005((Biter)obj, 649861234, writer);
			break;
		case 656832482:
			Serialize656832482((LeakingRadiation)obj, 656832482, writer);
			break;
		case 660968298:
			Serialize660968298((CrashHome)obj, 660968298, writer);
			break;
		case 662718200:
			Serialize662718200((DropEnzymes)obj, 662718200, writer);
			break;
		case 670501331:
			Serialize2066256250((MedicalCabinet)obj, 670501331, writer);
			break;
		case 697741374:
			Serialize356643005((Grower)obj, 697741374, writer);
			break;
		case 704466011:
			Serialize704466011((ScheduledGoal)obj, 704466011, writer);
			break;
		case 714689774:
			Serialize714689774((KeyValuePair<string, float>)obj, 714689774, writer);
			break;
		case 723629918:
			Serialize2066256250((Pickupable)obj, 723629918, writer);
			break;
		case 729882159:
			Serialize729882159((LiveMixin)obj, 729882159, writer);
			break;
		case 730995178:
			Serialize2066256250((CreepvineSeed)obj, 730995178, writer);
			break;
		case 737691900:
			Serialize2066256250((StarshipDoor)obj, 737691900, writer);
			break;
		case 746584541:
			Serialize746584541((TileInstance)obj, 746584541, writer);
			break;
		case 769725772:
			Serialize769725772((LaserCutObject)obj, 769725772, writer);
			break;
		case 773384208:
			Serialize773384208((AttackLargeTarget)obj, 773384208, writer);
			break;
		case 776720259:
			Serialize776720259((FairRandomizer)obj, 776720259, writer);
			break;
		case 788408569:
			Serialize642877324((BatteryCharger)obj, 788408569, writer);
			break;
		case 790665541:
			Serialize790665541((Durable)obj, 790665541, writer);
			break;
		case 792963384:
			Serialize2066256250((GrowingPlant)obj, 792963384, writer);
			break;
		case 797173896:
			Serialize797173896((PowerGenerator)obj, 797173896, writer);
			break;
		case 801838245:
			Serialize801838245((CreatureDeath)obj, 801838245, writer);
			break;
		case 802993602:
			Serialize2066256250((NuclearReactor)obj, 802993602, writer);
			break;
		case 803494206:
			Serialize803494206((ReefbackCreature)obj, 803494206, writer);
			break;
		case 812739867:
			Serialize812739867((CollectShiny)obj, 812739867, writer);
			break;
		case 817046334:
			Serialize2066256250((ConstructableBase)obj, 817046334, writer);
			break;
		case 829906308:
			Serialize356643005((BloomCreature)obj, 829906308, writer);
			break;
		case 837446894:
			Serialize837446894((Pipe)obj, 837446894, writer);
			break;
		case 840818195:
			Serialize2066256250((WeldableWallPanelGeneric)obj, 840818195, writer);
			break;
		case 846562516:
			Serialize846562516((SaveLoadManager.OptionsCache)obj, 846562516, writer);
			break;
		case 851981872:
			Serialize851981872((PrisonManager)obj, 851981872, writer);
			break;
		case 859052613:
			Serialize859052613((ResourceTrackerDatabase)obj, 859052613, writer);
			break;
		case 860890656:
			Serialize860890656((CurrentGenerator)obj, 860890656, writer);
			break;
		case 862251519:
			Serialize1585678550((BaseAddModuleGhost)obj, 862251519, writer);
			break;
		case 880630407:
			Serialize880630407((AnimatorParameterValue)obj, 880630407, writer);
			break;
		case 882866214:
			Serialize2066256250((BasePipeConnector)obj, 882866214, writer);
			break;
		case 888605288:
			Serialize888605288((Survival)obj, 888605288, writer);
			break;
		case 890247896:
			Serialize890247896((KeyValuePair<string, SceneObjectData>)obj, 890247896, writer);
			break;
		case 892833698:
			Serialize892833698((BoxCollider)obj, 892833698, writer);
			break;
		case 898747676:
			Serialize898747676((DisableBeforeExplosion)obj, 898747676, writer);
			break;
		case 916327342:
			Serialize916327342((RocketPreflightCheckManager)obj, 916327342, writer);
			break;
		case 924544622:
			Serialize356643005((Reginald)obj, 924544622, writer);
			break;
		case 929691462:
			Serialize1427962681((BatterySource)obj, 929691462, writer);
			break;
		case 949139443:
			Serialize949139443((CellManager.CellHeaderEx)obj, 949139443, writer);
			break;
		case 953017598:
			Serialize356643005((Bladderfish)obj, 953017598, writer);
			break;
		case 955443574:
			Serialize955443574((PipeSurfaceFloater)obj, 955443574, writer);
			break;
		case 997267884:
			Serialize997267884((Grid3Shape)obj, 997267884, writer);
			break;
		case 1004993247:
			Serialize1004993247((FiltrationMachine)obj, 1004993247, writer);
			break;
		case 1011566978:
			Serialize2066256250((Bioreactor)obj, 1011566978, writer);
			break;
		case 1012963343:
			Serialize1012963343((BlueprintHandTarget)obj, 1012963343, writer);
			break;
		case 1024693509:
			Serialize1024693509((Incubator)obj, 1024693509, writer);
			break;
		case 1032585975:
			Serialize1032585975((KeypadDoorConsole)obj, 1032585975, writer);
			break;
		case 1041366111:
			Serialize356643005((Bleeder)obj, 1041366111, writer);
			break;
		case 1045809045:
			Serialize2066256250((BaseSpotLight)obj, 1045809045, writer);
			break;
		case 1054395028:
			Serialize1054395028((WaterParkCreature)obj, 1054395028, writer);
			break;
		case 1058623975:
			Serialize356643005((SpineEel)obj, 1058623975, writer);
			break;
		case 1059166753:
			Serialize1059166753((KeypadDoorConsoleUnlock)obj, 1059166753, writer);
			break;
		case 1062675616:
			Serialize1062675616((KeyValuePair<string, PDALog.Entry>)obj, 1062675616, writer);
			break;
		case 1077102969:
			Serialize1077102969((ReefbackLife)obj, 1077102969, writer);
			break;
		case 1080881920:
			Serialize1080881920((Eatable)obj, 1080881920, writer);
			break;
		case 1082285174:
			Serialize356643005((RabbitRay)obj, 1082285174, writer);
			break;
		case 1084867387:
			Serialize2066256250((Sign)obj, 1084867387, writer);
			break;
		case 1086395194:
			Serialize1086395194((Sealed)obj, 1086395194, writer);
			break;
		case 1090472122:
			Serialize1585678550((BaseAddFaceGhost)obj, 1090472122, writer);
			break;
		case 1092217753:
			Serialize1092217753((RestoreEscapePodPosition)obj, 1092217753, writer);
			break;
		case 1103056798:
			Serialize1103056798((CraftingAnalytics.EntryData)obj, 1103056798, writer);
			break;
		case 1111417537:
			Serialize1111417537((TimeCapsuleContent)obj, 1111417537, writer);
			break;
		case 1113570486:
			Serialize2066256250((Vehicle)obj, 1113570486, writer);
			break;
		case 1116754719:
			Serialize1116754719((KeyValuePair<int, SceneObjectData>)obj, 1116754719, writer);
			break;
		case 1117396675:
			Serialize1117396675((AttractedByLargeTarget)obj, 1117396675, writer);
			break;
		case 1124891617:
			Serialize356643005((Peeper)obj, 1124891617, writer);
			break;
		case 1127946067:
			Serialize1585678550((BaseAddWaterPark)obj, 1127946067, writer);
			break;
		case 1157251956:
			Serialize1157251956((SwimToMeat)obj, 1157251956, writer);
			break;
		case 1158933531:
			Serialize1158933531((KeyValuePair<TechType, CraftingAnalytics.EntryData>)obj, 1158933531, writer);
			break;
		case 1159039820:
			Serialize1159039820((ProtobufSerializer.GameObjectData)obj, 1159039820, writer);
			break;
		case 1164389947:
			Serialize1164389947((Int3.Bounds)obj, 1164389947, writer);
			break;
		case 1169611549:
			Serialize1169611549((PrecursorSurfaceVent)obj, 1169611549, writer);
			break;
		case 1170326782:
			Serialize2066256250((RocketConstructorInput)obj, 1170326782, writer);
			break;
		case 1174302959:
			Serialize2066256250((Exosuit)obj, 1174302959, writer);
			break;
		case 1176920975:
			Serialize356643005((CaveCrawler)obj, 1176920975, writer);
			break;
		case 1180542256:
			Serialize1180542256((SubRoot)obj, 1180542256, writer);
			break;
		case 1181346078:
			Serialize1181346078((Vector4)obj, 1181346078, writer);
			break;
		case 1181346079:
			Serialize1181346079((Vector3)obj, 1181346079, writer);
			break;
		case 1181346080:
			Serialize1181346080((Vector2)obj, 1181346080, writer);
			break;
		case 1182280616:
			Serialize1182280616((Grid3<float>)obj, 1182280616, writer);
			break;
		case 1234455261:
			Serialize356643005((Crash)obj, 1234455261, writer);
			break;
		case 1244614203:
			Serialize1244614203((PDAEncyclopedia.Entry)obj, 1244614203, writer);
			break;
		case 1249808124:
			Serialize1249808124((KeyValuePair<string, TimeCapsuleContent>)obj, 1249808124, writer);
			break;
		case 1252700319:
			Serialize356643005((Jumper)obj, 1252700319, writer);
			break;
		case 1261146858:
			Serialize1261146858((Signal)obj, 1261146858, writer);
			break;
		case 1273669126:
			Serialize1273669126((Rocket)obj, 1273669126, writer);
			break;
		case 1289092887:
			Serialize2066256250((PickPrefab)obj, 1289092887, writer);
			break;
		case 1297003913:
			Serialize2066256250((UpgradeConsole)obj, 1297003913, writer);
			break;
		case 1304741297:
			Serialize1304741297((MapRoomCameraDocking)obj, 1304741297, writer);
			break;
		case 1307861365:
			Serialize1307861365((AuroraWarnings)obj, 1307861365, writer);
			break;
		case 1319564559:
			Serialize1319564559((SeamothStorageContainer)obj, 1319564559, writer);
			break;
		case 1327055215:
			Serialize356643005((SeaEmperorBaby)obj, 1327055215, writer);
			break;
		case 1329426611:
			Serialize1329426611((PDALog.Entry)obj, 1329426611, writer);
			break;
		case 1332964068:
			Serialize356643005((Leviathan)obj, 1332964068, writer);
			break;
		case 1333430695:
			Serialize2066256250((Workbench)obj, 1333430695, writer);
			break;
		case 1338410882:
			Serialize356643005((Jellyray)obj, 1338410882, writer);
			break;
		case 1340645883:
			Serialize1340645883((PrecursorTeleporter)obj, 1340645883, writer);
			break;
		case 1343493277:
			Serialize2066256250((MapRoomScreen)obj, 1343493277, writer);
			break;
		case 1355655442:
			Serialize356643005((Slime)obj, 1355655442, writer);
			break;
		case 1364639273:
			Serialize1364639273((Light)obj, 1364639273, writer);
			break;
		case 1384707171:
			Serialize1384707171((BaseNuclearReactor)obj, 1384707171, writer);
			break;
		case 1386031502:
			Serialize1386031502((TimeCapsule)obj, 1386031502, writer);
			break;
		case 1386435207:
			Serialize642877324((PowerCellCharger)obj, 1386435207, writer);
			break;
		case 1389926401:
			Serialize356643005((GhostRay)obj, 1389926401, writer);
			break;
		case 1404549125:
			Serialize1404549125((EntitySlotData)obj, 1404549125, writer);
			break;
		case 1404661584:
			Serialize1404661584((Color)obj, 1404661584, writer);
			break;
		case 1406047219:
			Serialize1585678550((BaseAddPartitionDoorGhost)obj, 1406047219, writer);
			break;
		case 1422648694:
			Serialize1585678550((BaseAddMapRoomGhost)obj, 1422648694, writer);
			break;
		case 1427962681:
			Serialize1427962681((EnergyMixin)obj, 1427962681, writer);
			break;
		case 1430634702:
			Serialize1430634702((JointHelper)obj, 1430634702, writer);
			break;
		case 1433797848:
			Serialize1433797848((ExecutionOrderTest)obj, 1433797848, writer);
			break;
		case 1440414490:
			Serialize356643005((Stalker)obj, 1440414490, writer);
			break;
		case 1457139245:
			Serialize2066256250((PrecursorDisableGunTerminal)obj, 1457139245, writer);
			break;
		case 1469570205:
			Serialize1469570205((BaseDeconstructable)obj, 1469570205, writer);
			break;
		case 1489031418:
			Serialize1489031418((KeyValuePair<string, bool>)obj, 1489031418, writer);
			break;
		case 1489909791:
			Serialize1489909791((CyclopsDecoyLoadingTube)obj, 1489909791, writer);
			break;
		case 1504796287:
			Serialize2066256250((BasePowerDistributor)obj, 1504796287, writer);
			break;
		case 1505210158:
			Serialize2066256250((PrecursorDoorKeyColumn)obj, 1505210158, writer);
			break;
		case 1506278612:
			Serialize1506278612((StoryGoalScheduler)obj, 1506278612, writer);
			break;
		case 1506893401:
			Serialize1585678550((BaseAddCorridorGhost)obj, 1506893401, writer);
			break;
		case 1518696882:
			Serialize1518696882((GradientAlphaKey)obj, 1518696882, writer);
			break;
		case 1522229446:
			Serialize1522229446((PowerSource)obj, 1522229446, writer);
			break;
		case 1527139359:
			Serialize1527139359((PictureFrame)obj, 1527139359, writer);
			break;
		case 1535225367:
			Serialize356643005((Shocker)obj, 1535225367, writer);
			break;
		case 1537030892:
			Serialize1537030892((SubFire)obj, 1537030892, writer);
			break;
		case 1537880658:
			Serialize1537880658((BaseUpgradeConsole)obj, 1537880658, writer);
			break;
		case 1541346828:
			Serialize1541346828((PrecursorComputerTerminal)obj, 1541346828, writer);
			break;
		case 1555952712:
			Serialize2066256250((SupplyCrate)obj, 1555952712, writer);
			break;
		case 1584912799:
			Serialize1584912799((BaseBioReactor)obj, 1584912799, writer);
			break;
		case 1585678550:
			Serialize1585678550((BaseGhost)obj, 1585678550, writer);
			break;
		case 1590521044:
			Serialize1590521044((CapsuleCollider)obj, 1590521044, writer);
			break;
		case 1603999327:
			Serialize1603999327((SunlightSettings)obj, 1603999327, writer);
			break;
		case 1606650114:
			Serialize356643005((SeaDragon)obj, 1606650114, writer);
			break;
		case 1618351061:
			Serialize1618351061((FireExtinguisherHolder)obj, 1618351061, writer);
			break;
		case 1651949009:
			Serialize356643005((Reefback)obj, 1651949009, writer);
			break;
		case 1659399257:
			Serialize1659399257((Battery)obj, 1659399257, writer);
			break;
		case 1671198874:
			Serialize1671198874((TechLight)obj, 1671198874, writer);
			break;
		case 1675048343:
			Serialize1675048343((PlayerWorldArrows)obj, 1675048343, writer);
			break;
		case 1677184373:
			Serialize1677184373((PropulseCannonAmmoHandler)obj, 1677184373, writer);
			break;
		case 1691070576:
			Serialize1691070576((Int3)obj, 1691070576, writer);
			break;
		case 1702762571:
			Serialize356643005((ReaperLeviathan)obj, 1702762571, writer);
			break;
		case 1703248490:
			Serialize1703248490((LavaShell)obj, 1703248490, writer);
			break;
		case 1703576080:
			Serialize1703576080((CraftingAnalytics)obj, 1703576080, writer);
			break;
		case 1711377162:
			Serialize1711377162((Beacon)obj, 1711377162, writer);
			break;
		case 1711422107:
			Serialize1711422107((CrashedShipExploder)obj, 1711422107, writer);
			break;
		case 1713660373:
			Serialize2066256250((PrecursorTeleporterActivationTerminal)obj, 1713660373, writer);
			break;
		case 1716510642:
			Serialize356643005((Boomerang)obj, 1716510642, writer);
			break;
		case 1725266300:
			Serialize1725266300((BaseFloodSim)obj, 1725266300, writer);
			break;
		case 1731663660:
			Serialize1731663660((NotificationManager.NotificationId)obj, 1731663660, writer);
			break;
		case 1732754958:
			Serialize1732754958((SignalPing)obj, 1732754958, writer);
			break;
		case 1747546845:
			Serialize1747546845((GradientColorKey)obj, 1747546845, writer);
			break;
		case 1755470585:
			Serialize1755470585((RestoreEscapePodStorage)obj, 1755470585, writer);
			break;
		case 1761034218:
			Serialize1761034218((NitrogenLevel)obj, 1761034218, writer);
			break;
		case 1761765682:
			Serialize1585678550((BaseAddFaceModuleGhost)obj, 1761765682, writer);
			break;
		case 1762542304:
			Serialize1762542304((SphereCollider)obj, 1762542304, writer);
			break;
		case 1765532648:
			Serialize1765532648((Grid3<Vector3>)obj, 1765532648, writer);
			break;
		case 1772819529:
			Serialize1772819529((Breach)obj, 1772819529, writer);
			break;
		case 1793440205:
			Serialize522953267((LargeRoomWaterPark)obj, 1793440205, writer);
			break;
		case 1800229096:
			Serialize2066256250((PowerCrafter)obj, 1800229096, writer);
			break;
		case 1804294731:
			Serialize1804294731((AmbientLightSettings)obj, 1804294731, writer);
			break;
		case 1816388137:
			Serialize356643005((Garryfish)obj, 1816388137, writer);
			break;
		case 1817260405:
			Serialize356643005((GhostLeviatanVoid)obj, 1817260405, writer);
			break;
		case 1825589276:
			Serialize1825589276((Behaviour)obj, 1825589276, writer);
			break;
		case 1829844631:
			Serialize356643005((OculusFish)obj, 1829844631, writer);
			break;
		case 1852174394:
			Serialize1852174394((KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>)obj, 1852174394, writer);
			break;
		case 1855998104:
			Serialize1855998104((MapRoomCamera)obj, 1855998104, writer);
			break;
		case 1859566378:
			Serialize356643005((Hoverfish)obj, 1859566378, writer);
			break;
		case 1874975849:
			Serialize1874975849((NotificationManager.NotificationData)obj, 1874975849, writer);
			break;
		case 1875862075:
			Serialize1875862075((Player)obj, 1875862075, writer);
			break;
		case 1877364825:
			Serialize1877364825((ToggleLights)obj, 1877364825, writer);
			break;
		case 1878920823:
			Serialize1878920823((LargeWorldBatchRoot)obj, 1878920823, writer);
			break;
		case 1880729675:
			Serialize356643005((CuteFish)obj, 1880729675, writer);
			break;
		case 1881457915:
			Serialize1881457915((SceneObjectData)obj, 1881457915, writer);
			break;
		case 1882829928:
			Serialize356643005((LavaLizard)obj, 1882829928, writer);
			break;
		case 1891515754:
			Serialize1891515754((UnityEngine.Object)obj, 1891515754, writer);
			break;
		case 1896640149:
			Serialize1585678550((BaseAddBulkheadGhost)obj, 1896640149, writer);
			break;
		case 1921909083:
			Serialize356643005((SeaEmperorJuvenile)obj, 1921909083, writer);
			break;
		case 1922274571:
			Serialize1922274571((EntitySlot)obj, 1922274571, writer);
			break;
		case 1948869956:
			Serialize1948869956((ReefbackPlant)obj, 1948869956, writer);
			break;
		case 1954617231:
			Serialize2066256250((DiveReelAnchor)obj, 1954617231, writer);
			break;
		case 1956492848:
			Serialize1956492848((CellManager.CellHeader)obj, 1956492848, writer);
			break;
		case 1966747463:
			Serialize1966747463((Base)obj, 1966747463, writer);
			break;
		case 1971823303:
			Serialize1971823303((EntitySlotsPlaceholder)obj, 1971823303, writer);
			break;
		case 1975582319:
			Serialize1975582319((Keyframe)obj, 1975582319, writer);
			break;
		case 1982063882:
			Serialize1982063882((StoryGoalManager)obj, 1982063882, writer);
			break;
		case 1983352738:
			Serialize2066256250((ThermalPlant)obj, 1983352738, writer);
			break;
		case 1993981848:
			Serialize1993981848((SpawnStoredLoot)obj, 1993981848, writer);
			break;
		case 1998792992:
			Serialize356643005((CrabSnake)obj, 1998792992, writer);
			break;
		case 2000594119:
			Serialize2000594119((AmbientSettings)obj, 2000594119, writer);
			break;
		case 2001815917:
			Serialize2001815917((CreatureFriend)obj, 2001815917, writer);
			break;
		case 2004070125:
			Serialize2004070125((InfectedMixin)obj, 2004070125, writer);
			break;
		case 2013353155:
			Serialize2013353155((SceneObjectDataSet)obj, 2013353155, writer);
			break;
		case 2024062491:
			Serialize2024062491((RestoreInventoryStorage)obj, 2024062491, writer);
			break;
		case 2028243609:
			Serialize2028243609((MonoBehaviour)obj, 2028243609, writer);
			break;
		case 2033371878:
			Serialize356643005((Grabcrab)obj, 2033371878, writer);
			break;
		case 2044575597:
			Serialize2044575597((TimeCapsuleItem)obj, 2044575597, writer);
			break;
		case 2051895012:
			Serialize2066256250((PrecursorKeyTerminal)obj, 2051895012, writer);
			break;
		case 2060501761:
			Serialize1585678550((BaseAddLadderGhost)obj, 2060501761, writer);
			break;
		case 2062152363:
			Serialize2062152363((CyclopsMotorMode)obj, 2062152363, writer);
			break;
		case 2066256250:
			Serialize2066256250((HandTarget)obj, 2066256250, writer);
			break;
		case 2083308735:
			Serialize2083308735((CyclopsLightingPanel)obj, 2083308735, writer);
			break;
		case 2100222829:
			Serialize2100222829((CrafterLogic)obj, 2100222829, writer);
			break;
		case 2100518191:
			Serialize2100518191((Drillable)obj, 2100518191, writer);
			break;
		case 2104816441:
			Serialize2104816441((TeleporterManager)obj, 2104816441, writer);
			break;
		case 2106147891:
			Serialize2066256250((SeaMoth)obj, 2106147891, writer);
			break;
		case 2111234577:
			Serialize2111234577((PrefabPlaceholdersGroup)obj, 2111234577, writer);
			break;
		case 2124310223:
			Serialize2124310223((VFXConstructing)obj, 2124310223, writer);
			break;
		case 2137760429:
			Serialize2137760429((PDAScanner.Data)obj, 2137760429, writer);
			break;
		case 2146397475:
			Serialize2146397475((BaseCell)obj, 2146397475, writer);
			break;
		}
	}

	protected override object Deserialize(int num, object obj, ProtoReader reader)
	{
		switch (num)
		{
		case 7443242:
			return Deserialize7443242((AnalyticsController)obj, reader);
		case 10406022:
			return Deserialize10406022((PlayerSoundTrigger)obj, reader);
		case 10881664:
			return Deserialize10881664((DiveReel)obj, reader);
		case 11492366:
			return Deserialize11492366((Respawn)obj, reader);
		case 26260290:
			return Deserialize26260290((DayNightLight)obj, reader);
		case 35332337:
			return Deserialize356643005((GhostLeviathan)obj, reader);
		case 40841636:
			return Deserialize356643005((BirdBehaviour)obj, reader);
		case 41335609:
			return Deserialize41335609((FruitPlant)obj, reader);
		case 49368518:
			return Deserialize356643005((CrabSquid)obj, reader);
		case 51037994:
			return Deserialize51037994((Inventory)obj, reader);
		case 59821228:
			return Deserialize59821228((CreatureEgg)obj, reader);
		case 69304398:
			return Deserialize69304398((ProtobufSerializer.LoopHeader)obj, reader);
		case 72619689:
			return Deserialize72619689((Collider)obj, reader);
		case 81014147:
			return Deserialize356643005((SandShark)obj, reader);
		case 82819465:
			return Deserialize82819465((Roost)obj, reader);
		case 108625815:
			return Deserialize108625815((FogSettings)obj, reader);
		case 113236186:
			return Deserialize2066256250((GrownPlant)obj, reader);
		case 118512508:
			return Deserialize118512508((AnimationCurve)obj, reader);
		case 121008834:
			return Deserialize121008834((KeyValuePair<string, PDAEncyclopedia.Entry>)obj, reader);
		case 122249312:
			return Deserialize122249312((LargeWorldEntityCell)obj, reader);
		case 123477383:
			return Deserialize356643005((Mesmer)obj, reader);
		case 135876261:
			return Deserialize2066256250((Crafter)obj, reader);
		case 149935601:
			return Deserialize149935601((Transform)obj, reader);
		case 153467737:
			return Deserialize153467737((CellManager.CellsFileHeader)obj, reader);
		case 161874211:
			return Deserialize161874211((ResourceTrackerDatabase.ResourceInfo)obj, reader);
		case 175529349:
			return Deserialize175529349((Gradient)obj, reader);
		case 185046565:
			return Deserialize185046565((PrecursorElevator)obj, reader);
		case 190681690:
			return Deserialize190681690((Oxygen)obj, reader);
		case 200911463:
			return Deserialize200911463((LEDLight)obj, reader);
		case 205484837:
			return Deserialize205484837((PingInstance)obj, reader);
		case 212928398:
			return Deserialize212928398((TechFragment)obj, reader);
		case 217879716:
			return Deserialize217879716((ProtobufSerializer.StreamHeader)obj, reader);
		case 224877162:
			return Deserialize224877162((KeyValuePair<string, int>)obj, reader);
		case 225324449:
			return Deserialize225324449((SwimRandom)obj, reader);
		case 237257528:
			return Deserialize237257528((CoffeeMachineLegacy)obj, reader);
		case 239375628:
			return Deserialize239375628((AtmosphereVolume)obj, reader);
		case 242125589:
			return Deserialize242125589((Stillsuit)obj, reader);
		case 242904679:
			return Deserialize356643005((BoneShark)obj, reader);
		case 248259089:
			return Deserialize248259089((PDAScanner.Entry)obj, reader);
		case 253947874:
			return Deserialize253947874((RestoreAnimatorState)obj, reader);
		case 257036954:
			return Deserialize2066256250((ConstructorInput)obj, reader);
		case 273953122:
			return Deserialize273953122((RestoreDayNightCycle)obj, reader);
		case 285680569:
			return Deserialize285680569((PrecursorGunStoryEvents)obj, reader);
		case 289177516:
			return Deserialize289177516((Int3Class)obj, reader);
		case 304513582:
			return Deserialize304513582((Flare)obj, reader);
		case 311542795:
			return Deserialize1180542256((BaseRoot)obj, reader);
		case 313352173:
			return Deserialize313352173((ProtobufSerializer.ComponentHeader)obj, reader);
		case 315488296:
			return Deserialize315488296((DayNightCycle)obj, reader);
		case 325421431:
			return Deserialize325421431((FireExtinguisher)obj, reader);
		case 326697518:
			return Deserialize326697518((Constructor)obj, reader);
		case 328142573:
			return Deserialize328142573((ProtobufSerializerCornerCases)obj, reader);
		case 328683587:
			return Deserialize356643005((Warper)obj, reader);
		case 328727906:
			return Deserialize356643005((Hoopfish)obj, reader);
		case 348559960:
			return Deserialize348559960((NotificationManager.SerializedData)obj, reader);
		case 351821017:
			return Deserialize356643005((Eyeye)obj, reader);
		case 353579366:
			return Deserialize356643005((LavaLarva)obj, reader);
		case 356643005:
			return Deserialize356643005((Creature)obj, reader);
		case 356984973:
			return Deserialize356984973((PrecursorAquariumPlatformTrigger)obj, reader);
		case 366077262:
			return Deserialize2066256250((StorageContainer)obj, reader);
		case 371084514:
			return Deserialize371084514((BaseExplicitFace)obj, reader);
		case 383673295:
			return Deserialize1585678550((BaseAddCellGhost)obj, reader);
		case 391689956:
			return Deserialize391689956((Bounds)obj, reader);
		case 394322812:
			return Deserialize394322812((Component)obj, reader);
		case 396637907:
			return Deserialize2066256250((Constructable)obj, reader);
		case 406373562:
			return Deserialize406373562((CreatureBehaviour)obj, reader);
		case 407275749:
			return Deserialize407275749((PlayerTimeCapsule)obj, reader);
		case 438265826:
			return Deserialize438265826((UnstuckPlayer)obj, reader);
		case 443301464:
			return Deserialize443301464((EscapePod)obj, reader);
		case 459225532:
			return Deserialize459225532((GenericConsole)obj, reader);
		case 469144263:
			return Deserialize356643005((Skyray)obj, reader);
		case 476284185:
			return Deserialize356643005((GasoPod)obj, reader);
		case 490474621:
			return Deserialize1585678550((BaseAddPartitionGhost)obj, reader);
		case 496960061:
			return Deserialize496960061((PrecursorPrisonVent)obj, reader);
		case 497398254:
			return Deserialize497398254((Base.Face)obj, reader);
		case 502247445:
			return Deserialize502247445((StoryGoalCustomEventHandler)obj, reader);
		case 503490010:
			return Deserialize503490010((MapRoomFunctionality)obj, reader);
		case 515927774:
			return Deserialize2066256250((ColoredLabel)obj, reader);
		case 522953267:
			return Deserialize522953267((WaterPark)obj, reader);
		case 524780017:
			return Deserialize524780017((KeyValuePair<string, string>)obj, reader);
		case 524984727:
			return Deserialize524984727((SolarPanel)obj, reader);
		case 530216075:
			return Deserialize2066256250((GhostPickupable)obj, reader);
		case 542223321:
			return Deserialize542223321((Plantable)obj, reader);
		case 569028242:
			return Deserialize569028242((OxygenPipe)obj, reader);
		case 574222852:
			return Deserialize356643005((SeaTreader)obj, reader);
		case 577496260:
			return Deserialize577496260((LargeWorldEntity)obj, reader);
		case 605020259:
			return Deserialize605020259((Quaternion)obj, reader);
		case 606619525:
			return Deserialize2066256250((Fabricator)obj, reader);
		case 624729416:
			return Deserialize624729416((AnteChamber)obj, reader);
		case 630473610:
			return Deserialize2066256250((GhostCrafter)obj, reader);
		case 633495696:
			return Deserialize1585678550((BaseAddConnectorGhost)obj, reader);
		case 642877324:
			return Deserialize642877324((Charger)obj, reader);
		case 645269343:
			return Deserialize645269343((Terraformer)obj, reader);
		case 646820922:
			return Deserialize356643005((Holefish)obj, reader);
		case 647827065:
			return Deserialize356643005((Spadefish)obj, reader);
		case 649861234:
			return Deserialize356643005((Biter)obj, reader);
		case 656832482:
			return Deserialize656832482((LeakingRadiation)obj, reader);
		case 660968298:
			return Deserialize660968298((CrashHome)obj, reader);
		case 662718200:
			return Deserialize662718200((DropEnzymes)obj, reader);
		case 670501331:
			return Deserialize2066256250((MedicalCabinet)obj, reader);
		case 697741374:
			return Deserialize356643005((Grower)obj, reader);
		case 704466011:
			return Deserialize704466011((ScheduledGoal)obj, reader);
		case 714689774:
			return Deserialize714689774((KeyValuePair<string, float>)obj, reader);
		case 723629918:
			return Deserialize2066256250((Pickupable)obj, reader);
		case 729882159:
			return Deserialize729882159((LiveMixin)obj, reader);
		case 730995178:
			return Deserialize2066256250((CreepvineSeed)obj, reader);
		case 737691900:
			return Deserialize2066256250((StarshipDoor)obj, reader);
		case 746584541:
			return Deserialize746584541((TileInstance)obj, reader);
		case 769725772:
			return Deserialize769725772((LaserCutObject)obj, reader);
		case 773384208:
			return Deserialize773384208((AttackLargeTarget)obj, reader);
		case 776720259:
			return Deserialize776720259((FairRandomizer)obj, reader);
		case 788408569:
			return Deserialize642877324((BatteryCharger)obj, reader);
		case 790665541:
			return Deserialize790665541((Durable)obj, reader);
		case 792963384:
			return Deserialize2066256250((GrowingPlant)obj, reader);
		case 797173896:
			return Deserialize797173896((PowerGenerator)obj, reader);
		case 801838245:
			return Deserialize801838245((CreatureDeath)obj, reader);
		case 802993602:
			return Deserialize2066256250((NuclearReactor)obj, reader);
		case 803494206:
			return Deserialize803494206((ReefbackCreature)obj, reader);
		case 812739867:
			return Deserialize812739867((CollectShiny)obj, reader);
		case 817046334:
			return Deserialize2066256250((ConstructableBase)obj, reader);
		case 829906308:
			return Deserialize356643005((BloomCreature)obj, reader);
		case 837446894:
			return Deserialize837446894((Pipe)obj, reader);
		case 840818195:
			return Deserialize2066256250((WeldableWallPanelGeneric)obj, reader);
		case 846562516:
			return Deserialize846562516((SaveLoadManager.OptionsCache)obj, reader);
		case 851981872:
			return Deserialize851981872((PrisonManager)obj, reader);
		case 859052613:
			return Deserialize859052613((ResourceTrackerDatabase)obj, reader);
		case 860890656:
			return Deserialize860890656((CurrentGenerator)obj, reader);
		case 862251519:
			return Deserialize1585678550((BaseAddModuleGhost)obj, reader);
		case 880630407:
			return Deserialize880630407((AnimatorParameterValue)obj, reader);
		case 882866214:
			return Deserialize2066256250((BasePipeConnector)obj, reader);
		case 888605288:
			return Deserialize888605288((Survival)obj, reader);
		case 890247896:
			return Deserialize890247896((KeyValuePair<string, SceneObjectData>)obj, reader);
		case 892833698:
			return Deserialize892833698((BoxCollider)obj, reader);
		case 898747676:
			return Deserialize898747676((DisableBeforeExplosion)obj, reader);
		case 916327342:
			return Deserialize916327342((RocketPreflightCheckManager)obj, reader);
		case 924544622:
			return Deserialize356643005((Reginald)obj, reader);
		case 929691462:
			return Deserialize1427962681((BatterySource)obj, reader);
		case 949139443:
			return Deserialize949139443((CellManager.CellHeaderEx)obj, reader);
		case 953017598:
			return Deserialize356643005((Bladderfish)obj, reader);
		case 955443574:
			return Deserialize955443574((PipeSurfaceFloater)obj, reader);
		case 997267884:
			return Deserialize997267884((Grid3Shape)obj, reader);
		case 1004993247:
			return Deserialize1004993247((FiltrationMachine)obj, reader);
		case 1011566978:
			return Deserialize2066256250((Bioreactor)obj, reader);
		case 1012963343:
			return Deserialize1012963343((BlueprintHandTarget)obj, reader);
		case 1024693509:
			return Deserialize1024693509((Incubator)obj, reader);
		case 1032585975:
			return Deserialize1032585975((KeypadDoorConsole)obj, reader);
		case 1041366111:
			return Deserialize356643005((Bleeder)obj, reader);
		case 1045809045:
			return Deserialize2066256250((BaseSpotLight)obj, reader);
		case 1054395028:
			return Deserialize1054395028((WaterParkCreature)obj, reader);
		case 1058623975:
			return Deserialize356643005((SpineEel)obj, reader);
		case 1059166753:
			return Deserialize1059166753((KeypadDoorConsoleUnlock)obj, reader);
		case 1062675616:
			return Deserialize1062675616((KeyValuePair<string, PDALog.Entry>)obj, reader);
		case 1077102969:
			return Deserialize1077102969((ReefbackLife)obj, reader);
		case 1080881920:
			return Deserialize1080881920((Eatable)obj, reader);
		case 1082285174:
			return Deserialize356643005((RabbitRay)obj, reader);
		case 1084867387:
			return Deserialize2066256250((Sign)obj, reader);
		case 1086395194:
			return Deserialize1086395194((Sealed)obj, reader);
		case 1090472122:
			return Deserialize1585678550((BaseAddFaceGhost)obj, reader);
		case 1092217753:
			return Deserialize1092217753((RestoreEscapePodPosition)obj, reader);
		case 1103056798:
			return Deserialize1103056798((CraftingAnalytics.EntryData)obj, reader);
		case 1111417537:
			return Deserialize1111417537((TimeCapsuleContent)obj, reader);
		case 1113570486:
			return Deserialize2066256250((Vehicle)obj, reader);
		case 1116754719:
			return Deserialize1116754719((KeyValuePair<int, SceneObjectData>)obj, reader);
		case 1117396675:
			return Deserialize1117396675((AttractedByLargeTarget)obj, reader);
		case 1124891617:
			return Deserialize356643005((Peeper)obj, reader);
		case 1127946067:
			return Deserialize1585678550((BaseAddWaterPark)obj, reader);
		case 1157251956:
			return Deserialize1157251956((SwimToMeat)obj, reader);
		case 1158933531:
			return Deserialize1158933531((KeyValuePair<TechType, CraftingAnalytics.EntryData>)obj, reader);
		case 1159039820:
			return Deserialize1159039820((ProtobufSerializer.GameObjectData)obj, reader);
		case 1164389947:
			return Deserialize1164389947((Int3.Bounds)obj, reader);
		case 1169611549:
			return Deserialize1169611549((PrecursorSurfaceVent)obj, reader);
		case 1170326782:
			return Deserialize2066256250((RocketConstructorInput)obj, reader);
		case 1174302959:
			return Deserialize2066256250((Exosuit)obj, reader);
		case 1176920975:
			return Deserialize356643005((CaveCrawler)obj, reader);
		case 1180542256:
			return Deserialize1180542256((SubRoot)obj, reader);
		case 1181346078:
			return Deserialize1181346078((Vector4)obj, reader);
		case 1181346079:
			return Deserialize1181346079((Vector3)obj, reader);
		case 1181346080:
			return Deserialize1181346080((Vector2)obj, reader);
		case 1182280616:
			return Deserialize1182280616((Grid3<float>)obj, reader);
		case 1234455261:
			return Deserialize356643005((Crash)obj, reader);
		case 1244614203:
			return Deserialize1244614203((PDAEncyclopedia.Entry)obj, reader);
		case 1249808124:
			return Deserialize1249808124((KeyValuePair<string, TimeCapsuleContent>)obj, reader);
		case 1252700319:
			return Deserialize356643005((Jumper)obj, reader);
		case 1261146858:
			return Deserialize1261146858((Signal)obj, reader);
		case 1273669126:
			return Deserialize1273669126((Rocket)obj, reader);
		case 1289092887:
			return Deserialize2066256250((PickPrefab)obj, reader);
		case 1297003913:
			return Deserialize2066256250((UpgradeConsole)obj, reader);
		case 1304741297:
			return Deserialize1304741297((MapRoomCameraDocking)obj, reader);
		case 1307861365:
			return Deserialize1307861365((AuroraWarnings)obj, reader);
		case 1319564559:
			return Deserialize1319564559((SeamothStorageContainer)obj, reader);
		case 1327055215:
			return Deserialize356643005((SeaEmperorBaby)obj, reader);
		case 1329426611:
			return Deserialize1329426611((PDALog.Entry)obj, reader);
		case 1332964068:
			return Deserialize356643005((Leviathan)obj, reader);
		case 1333430695:
			return Deserialize2066256250((Workbench)obj, reader);
		case 1338410882:
			return Deserialize356643005((Jellyray)obj, reader);
		case 1340645883:
			return Deserialize1340645883((PrecursorTeleporter)obj, reader);
		case 1343493277:
			return Deserialize2066256250((MapRoomScreen)obj, reader);
		case 1355655442:
			return Deserialize356643005((Slime)obj, reader);
		case 1364639273:
			return Deserialize1364639273((Light)obj, reader);
		case 1384707171:
			return Deserialize1384707171((BaseNuclearReactor)obj, reader);
		case 1386031502:
			return Deserialize1386031502((TimeCapsule)obj, reader);
		case 1386435207:
			return Deserialize642877324((PowerCellCharger)obj, reader);
		case 1389926401:
			return Deserialize356643005((GhostRay)obj, reader);
		case 1404549125:
			return Deserialize1404549125((EntitySlotData)obj, reader);
		case 1404661584:
			return Deserialize1404661584((Color)obj, reader);
		case 1406047219:
			return Deserialize1585678550((BaseAddPartitionDoorGhost)obj, reader);
		case 1422648694:
			return Deserialize1585678550((BaseAddMapRoomGhost)obj, reader);
		case 1427962681:
			return Deserialize1427962681((EnergyMixin)obj, reader);
		case 1430634702:
			return Deserialize1430634702((JointHelper)obj, reader);
		case 1433797848:
			return Deserialize1433797848((ExecutionOrderTest)obj, reader);
		case 1440414490:
			return Deserialize356643005((Stalker)obj, reader);
		case 1457139245:
			return Deserialize2066256250((PrecursorDisableGunTerminal)obj, reader);
		case 1469570205:
			return Deserialize1469570205((BaseDeconstructable)obj, reader);
		case 1489031418:
			return Deserialize1489031418((KeyValuePair<string, bool>)obj, reader);
		case 1489909791:
			return Deserialize1489909791((CyclopsDecoyLoadingTube)obj, reader);
		case 1504796287:
			return Deserialize2066256250((BasePowerDistributor)obj, reader);
		case 1505210158:
			return Deserialize2066256250((PrecursorDoorKeyColumn)obj, reader);
		case 1506278612:
			return Deserialize1506278612((StoryGoalScheduler)obj, reader);
		case 1506893401:
			return Deserialize1585678550((BaseAddCorridorGhost)obj, reader);
		case 1518696882:
			return Deserialize1518696882((GradientAlphaKey)obj, reader);
		case 1522229446:
			return Deserialize1522229446((PowerSource)obj, reader);
		case 1527139359:
			return Deserialize1527139359((PictureFrame)obj, reader);
		case 1535225367:
			return Deserialize356643005((Shocker)obj, reader);
		case 1537030892:
			return Deserialize1537030892((SubFire)obj, reader);
		case 1537880658:
			return Deserialize1537880658((BaseUpgradeConsole)obj, reader);
		case 1541346828:
			return Deserialize1541346828((PrecursorComputerTerminal)obj, reader);
		case 1555952712:
			return Deserialize2066256250((SupplyCrate)obj, reader);
		case 1584912799:
			return Deserialize1584912799((BaseBioReactor)obj, reader);
		case 1585678550:
			return Deserialize1585678550((BaseGhost)obj, reader);
		case 1590521044:
			return Deserialize1590521044((CapsuleCollider)obj, reader);
		case 1603999327:
			return Deserialize1603999327((SunlightSettings)obj, reader);
		case 1606650114:
			return Deserialize356643005((SeaDragon)obj, reader);
		case 1618351061:
			return Deserialize1618351061((FireExtinguisherHolder)obj, reader);
		case 1651949009:
			return Deserialize356643005((Reefback)obj, reader);
		case 1659399257:
			return Deserialize1659399257((Battery)obj, reader);
		case 1671198874:
			return Deserialize1671198874((TechLight)obj, reader);
		case 1675048343:
			return Deserialize1675048343((PlayerWorldArrows)obj, reader);
		case 1677184373:
			return Deserialize1677184373((PropulseCannonAmmoHandler)obj, reader);
		case 1691070576:
			return Deserialize1691070576((Int3)obj, reader);
		case 1702762571:
			return Deserialize356643005((ReaperLeviathan)obj, reader);
		case 1703248490:
			return Deserialize1703248490((LavaShell)obj, reader);
		case 1703576080:
			return Deserialize1703576080((CraftingAnalytics)obj, reader);
		case 1711377162:
			return Deserialize1711377162((Beacon)obj, reader);
		case 1711422107:
			return Deserialize1711422107((CrashedShipExploder)obj, reader);
		case 1713660373:
			return Deserialize2066256250((PrecursorTeleporterActivationTerminal)obj, reader);
		case 1716510642:
			return Deserialize356643005((Boomerang)obj, reader);
		case 1725266300:
			return Deserialize1725266300((BaseFloodSim)obj, reader);
		case 1731663660:
			return Deserialize1731663660((NotificationManager.NotificationId)obj, reader);
		case 1732754958:
			return Deserialize1732754958((SignalPing)obj, reader);
		case 1747546845:
			return Deserialize1747546845((GradientColorKey)obj, reader);
		case 1755470585:
			return Deserialize1755470585((RestoreEscapePodStorage)obj, reader);
		case 1761034218:
			return Deserialize1761034218((NitrogenLevel)obj, reader);
		case 1761765682:
			return Deserialize1585678550((BaseAddFaceModuleGhost)obj, reader);
		case 1762542304:
			return Deserialize1762542304((SphereCollider)obj, reader);
		case 1765532648:
			return Deserialize1765532648((Grid3<Vector3>)obj, reader);
		case 1772819529:
			return Deserialize1772819529((Breach)obj, reader);
		case 1793440205:
			return Deserialize522953267((LargeRoomWaterPark)obj, reader);
		case 1800229096:
			return Deserialize2066256250((PowerCrafter)obj, reader);
		case 1804294731:
			return Deserialize1804294731((AmbientLightSettings)obj, reader);
		case 1816388137:
			return Deserialize356643005((Garryfish)obj, reader);
		case 1817260405:
			return Deserialize356643005((GhostLeviatanVoid)obj, reader);
		case 1825589276:
			return Deserialize1825589276((Behaviour)obj, reader);
		case 1829844631:
			return Deserialize356643005((OculusFish)obj, reader);
		case 1852174394:
			return Deserialize1852174394((KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>)obj, reader);
		case 1855998104:
			return Deserialize1855998104((MapRoomCamera)obj, reader);
		case 1859566378:
			return Deserialize356643005((Hoverfish)obj, reader);
		case 1874975849:
			return Deserialize1874975849((NotificationManager.NotificationData)obj, reader);
		case 1875862075:
			return Deserialize1875862075((Player)obj, reader);
		case 1877364825:
			return Deserialize1877364825((ToggleLights)obj, reader);
		case 1878920823:
			return Deserialize1878920823((LargeWorldBatchRoot)obj, reader);
		case 1880729675:
			return Deserialize356643005((CuteFish)obj, reader);
		case 1881457915:
			return Deserialize1881457915((SceneObjectData)obj, reader);
		case 1882829928:
			return Deserialize356643005((LavaLizard)obj, reader);
		case 1891515754:
			return Deserialize1891515754((UnityEngine.Object)obj, reader);
		case 1896640149:
			return Deserialize1585678550((BaseAddBulkheadGhost)obj, reader);
		case 1921909083:
			return Deserialize356643005((SeaEmperorJuvenile)obj, reader);
		case 1922274571:
			return Deserialize1922274571((EntitySlot)obj, reader);
		case 1948869956:
			return Deserialize1948869956((ReefbackPlant)obj, reader);
		case 1954617231:
			return Deserialize2066256250((DiveReelAnchor)obj, reader);
		case 1956492848:
			return Deserialize1956492848((CellManager.CellHeader)obj, reader);
		case 1966747463:
			return Deserialize1966747463((Base)obj, reader);
		case 1971823303:
			return Deserialize1971823303((EntitySlotsPlaceholder)obj, reader);
		case 1975582319:
			return Deserialize1975582319((Keyframe)obj, reader);
		case 1982063882:
			return Deserialize1982063882((StoryGoalManager)obj, reader);
		case 1983352738:
			return Deserialize2066256250((ThermalPlant)obj, reader);
		case 1993981848:
			return Deserialize1993981848((SpawnStoredLoot)obj, reader);
		case 1998792992:
			return Deserialize356643005((CrabSnake)obj, reader);
		case 2000594119:
			return Deserialize2000594119((AmbientSettings)obj, reader);
		case 2001815917:
			return Deserialize2001815917((CreatureFriend)obj, reader);
		case 2004070125:
			return Deserialize2004070125((InfectedMixin)obj, reader);
		case 2013353155:
			return Deserialize2013353155((SceneObjectDataSet)obj, reader);
		case 2024062491:
			return Deserialize2024062491((RestoreInventoryStorage)obj, reader);
		case 2028243609:
			return Deserialize2028243609((MonoBehaviour)obj, reader);
		case 2033371878:
			return Deserialize356643005((Grabcrab)obj, reader);
		case 2044575597:
			return Deserialize2044575597((TimeCapsuleItem)obj, reader);
		case 2051895012:
			return Deserialize2066256250((PrecursorKeyTerminal)obj, reader);
		case 2060501761:
			return Deserialize1585678550((BaseAddLadderGhost)obj, reader);
		case 2062152363:
			return Deserialize2062152363((CyclopsMotorMode)obj, reader);
		case 2066256250:
			return Deserialize2066256250((HandTarget)obj, reader);
		case 2083308735:
			return Deserialize2083308735((CyclopsLightingPanel)obj, reader);
		case 2100222829:
			return Deserialize2100222829((CrafterLogic)obj, reader);
		case 2100518191:
			return Deserialize2100518191((Drillable)obj, reader);
		case 2104816441:
			return Deserialize2104816441((TeleporterManager)obj, reader);
		case 2106147891:
			return Deserialize2066256250((SeaMoth)obj, reader);
		case 2111234577:
			return Deserialize2111234577((PrefabPlaceholdersGroup)obj, reader);
		case 2124310223:
			return Deserialize2124310223((VFXConstructing)obj, reader);
		case 2137760429:
			return Deserialize2137760429((PDAScanner.Data)obj, reader);
		case 2146397475:
			return Deserialize2146397475((BaseCell)obj, reader);
		default:
			return null;
		}
	}

	private void Serialize1804294731(AmbientLightSettings obj, int objTypeId, ProtoWriter writer)
	{
		obj.OnBeforeSerialization();
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.color, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.dayNightColor != null)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.dayNightColor, writer);
			Serialize175529349(obj.dayNightColor, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.scale, writer);
	}

	private AmbientLightSettings Deserialize1804294731(AmbientLightSettings obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new AmbientLightSettings();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.color = Deserialize1404661584(obj.color, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 3:
				obj.version = reader.ReadInt32();
				break;
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.dayNightColor = Deserialize175529349(obj.dayNightColor, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 5:
				obj.scale = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj.OnAfterDeserialization();
		return obj;
	}

	private void Serialize2000594119(AmbientSettings obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.ambientLight, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private AmbientSettings Deserialize2000594119(AmbientSettings obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.ambientLight = Deserialize1404661584(obj.ambientLight, reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize7443242(AnalyticsController obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj._playthroughId != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj._playthroughId, writer);
		}
		if (obj._tags == null)
		{
			return;
		}
		HashSet<string>.Enumerator enumerator = obj._tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private AnalyticsController Deserialize7443242(AnalyticsController obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj._playthroughId = reader.ReadString();
				break;
			case 3:
			{
				HashSet<string> hashSet = obj._tags ?? new HashSet<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					hashSet.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize880630407(AnimatorParameterValue obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.paramHash, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.paramType, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.boolValue, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.intValue, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.floatValue, writer);
	}

	private AnimatorParameterValue Deserialize880630407(AnimatorParameterValue obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new AnimatorParameterValue();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.paramHash = reader.ReadInt32();
				break;
			case 3:
				obj.paramType = (AnimatorControllerParameterType)reader.ReadInt32();
				break;
			case 4:
				obj.boolValue = reader.ReadBoolean();
				break;
			case 5:
				obj.intValue = reader.ReadInt32();
				break;
			case 6:
				obj.floatValue = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize624729416(AnteChamber obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeScanBegin, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.scanning, writer);
	}

	private AnteChamber Deserialize624729416(AnteChamber obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeScanBegin = reader.ReadSingle();
				break;
			case 3:
				obj.scanning = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize239375628(AtmosphereVolume obj, int objTypeId, ProtoWriter writer)
	{
		obj.OnBeforeSerialization();
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.priority, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.fogColor, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fogStartDistance, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fogMaxDistance, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fadeDefaultLights, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fadeRate, writer);
		if (obj.fog != null)
		{
			ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.fog, writer);
			Serialize108625815(obj.fog, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		if (obj.sun != null)
		{
			ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(obj.sun, writer);
			Serialize1603999327(obj.sun, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
		}
		if (obj.amb != null)
		{
			ProtoWriter.WriteFieldHeader(9, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(obj.amb, writer);
			Serialize1804294731(obj.amb, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
		}
		ProtoWriter.WriteFieldHeader(10, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.overrideBiome != null)
		{
			ProtoWriter.WriteFieldHeader(11, WireType.String, writer);
			ProtoWriter.WriteString(obj.overrideBiome, writer);
		}
		ProtoWriter.WriteFieldHeader(13, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.highDetail, writer);
		ProtoWriter.WriteFieldHeader(14, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.affectsVisuals, writer);
	}

	private AtmosphereVolume Deserialize239375628(AtmosphereVolume obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.priority = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj.fogColor = Deserialize1404661584(obj.fogColor, reader);
				ProtoReader.EndSubItem(token4, reader);
				break;
			}
			case 3:
				obj.fogStartDistance = reader.ReadSingle();
				break;
			case 4:
				obj.fogMaxDistance = reader.ReadSingle();
				break;
			case 5:
				obj.fadeDefaultLights = reader.ReadSingle();
				break;
			case 6:
				obj.fadeRate = reader.ReadSingle();
				break;
			case 7:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.fog = Deserialize108625815(obj.fog, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 8:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.sun = Deserialize1603999327(obj.sun, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 9:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.amb = Deserialize1804294731(obj.amb, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 10:
				obj.version = reader.ReadInt32();
				break;
			case 11:
				obj.overrideBiome = reader.ReadString();
				break;
			case 13:
				obj.highDetail = reader.ReadBoolean();
				break;
			case 14:
				obj.affectsVisuals = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		obj.OnAfterDeserialization();
		return obj;
	}

	private void Serialize773384208(AttackLargeTarget obj, int objTypeId, ProtoWriter writer)
	{
	}

	private AttackLargeTarget Deserialize773384208(AttackLargeTarget obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1117396675(AttractedByLargeTarget obj, int objTypeId, ProtoWriter writer)
	{
	}

	private AttractedByLargeTarget Deserialize1117396675(AttractedByLargeTarget obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1307861365(AuroraWarnings obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeSerialized, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private AuroraWarnings Deserialize1307861365(AuroraWarnings obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 3:
				obj.timeSerialized = reader.ReadSingle();
				break;
			case 4:
				obj.version = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1966747463(Base obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize997267884(obj.baseShape, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.faces != null)
		{
			Base.FaceType[] faces = obj.faces;
			foreach (Base.FaceType value in faces)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)value, writer);
			}
		}
		if (obj.cells != null)
		{
			Base.CellType[] cells = obj.cells;
			foreach (Base.CellType value2 in cells)
			{
				ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)value2, writer);
			}
		}
		if (obj.links != null)
		{
			byte[] links = obj.links;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(links, writer);
		}
		ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.cellOffset, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
		if (obj.masks != null)
		{
			byte[] masks = obj.masks;
			ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
			ProtoWriter.WriteBytes(masks, writer);
		}
		if (obj.isGlass != null)
		{
			bool[] isGlass = obj.isGlass;
			foreach (bool value3 in isGlass)
			{
				ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
				ProtoWriter.WriteBoolean(value3, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
		SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.anchor, objTypeId, writer);
		ProtoWriter.EndSubItem(token3, writer);
	}

	private Base Deserialize1966747463(Base obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.baseShape = Deserialize997267884(obj.baseShape, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				List<Base.FaceType> list2 = new List<Base.FaceType>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					Base.FaceType faceType = Base.FaceType.None;
					faceType = (Base.FaceType)reader.ReadInt32();
					list2.Add(faceType);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.faces = list2.ToArray();
				break;
			}
			case 3:
			{
				List<Base.CellType> list3 = new List<Base.CellType>();
				int fieldNumber3 = reader.FieldNumber;
				do
				{
					Base.CellType cellType = Base.CellType.Empty;
					cellType = (Base.CellType)reader.ReadInt32();
					list3.Add(cellType);
				}
				while (reader.TryReadFieldHeader(fieldNumber3));
				obj.cells = list3.ToArray();
				break;
			}
			case 4:
				obj.links = ProtoReader.AppendBytes(obj.links, reader);
				break;
			case 5:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.cellOffset = Deserialize1691070576(obj.cellOffset, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 6:
				obj.masks = ProtoReader.AppendBytes(obj.masks, reader);
				break;
			case 7:
			{
				List<bool> list = new List<bool>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					bool flag = false;
					flag = reader.ReadBoolean();
					list.Add(flag);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.isGlass = list.ToArray();
				break;
			}
			case 8:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchor = Deserialize1691070576(obj.anchor, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize497398254(Base.Face obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.cell, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.direction, writer);
	}

	private Base.Face Deserialize497398254(Base.Face obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.cell = Deserialize1691070576(obj.cell, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 2:
				obj.direction = (Base.Direction)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1896640149(BaseAddBulkheadGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddBulkheadGhost Deserialize1896640149(BaseAddBulkheadGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize383673295(BaseAddCellGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddCellGhost Deserialize383673295(BaseAddCellGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize633495696(BaseAddConnectorGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddConnectorGhost Deserialize633495696(BaseAddConnectorGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1506893401(BaseAddCorridorGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddCorridorGhost Deserialize1506893401(BaseAddCorridorGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1090472122(BaseAddFaceGhost obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 1761765682)
		{
			ProtoWriter.WriteFieldHeader(701, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1761765682(obj as BaseAddFaceModuleGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		if (obj.anchoredFace.HasValue)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.anchoredFace.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
	}

	private BaseAddFaceGhost Deserialize1090472122(BaseAddFaceGhost obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 701)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize1761765682(obj as BaseAddFaceModuleGhost, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			if (num == 1)
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.anchoredFace = Deserialize497398254(obj.anchoredFace.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token2, reader);
			}
			else
			{
				reader.SkipField();
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1761765682(BaseAddFaceModuleGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddFaceModuleGhost Deserialize1761765682(BaseAddFaceModuleGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize2060501761(BaseAddLadderGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddLadderGhost Deserialize2060501761(BaseAddLadderGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1422648694(BaseAddMapRoomGhost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseAddMapRoomGhost Deserialize1422648694(BaseAddMapRoomGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize862251519(BaseAddModuleGhost obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.anchoredFace.HasValue)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.anchoredFace.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BaseAddModuleGhost Deserialize862251519(BaseAddModuleGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchoredFace = Deserialize497398254(obj.anchoredFace.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1406047219(BaseAddPartitionDoorGhost obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.anchoredFace.HasValue)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.anchoredFace.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BaseAddPartitionDoorGhost Deserialize1406047219(BaseAddPartitionDoorGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchoredFace = Deserialize497398254(obj.anchoredFace.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize490474621(BaseAddPartitionGhost obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.anchoredCell.HasValue)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1691070576(obj.anchoredCell.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BaseAddPartitionGhost Deserialize490474621(BaseAddPartitionGhost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchoredCell = Deserialize1691070576(obj.anchoredCell.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1127946067(BaseAddWaterPark obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.anchoredFace.HasValue)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.anchoredFace.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BaseAddWaterPark Deserialize1127946067(BaseAddWaterPark obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchoredFace = Deserialize497398254(obj.anchoredFace.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1584912799(BaseBioReactor obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj._protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize497398254(obj._moduleFace, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._constructed, writer);
		if (obj._serializedStorage != null)
		{
			byte[] serializedStorage = obj._serializedStorage;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedStorage, writer);
		}
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._toConsume, writer);
	}

	private BaseBioReactor Deserialize1584912799(BaseBioReactor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._protoVersion = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj._moduleFace = Deserialize497398254(obj._moduleFace, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 3:
				obj._constructed = reader.ReadSingle();
				break;
			case 4:
				obj._serializedStorage = ProtoReader.AppendBytes(obj._serializedStorage, reader);
				break;
			case 5:
				obj._toConsume = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2146397475(BaseCell obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseCell Deserialize2146397475(BaseCell obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1469570205(BaseDeconstructable obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1164389947(obj.bounds, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.face.HasValue)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.face.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.faceType, writer);
	}

	private BaseDeconstructable Deserialize1469570205(BaseDeconstructable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.bounds = Deserialize1164389947(obj.bounds, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.face = Deserialize497398254(obj.face.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 3:
				obj.faceType = (Base.FaceType)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize371084514(BaseExplicitFace obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.face.HasValue)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.face.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BaseExplicitFace Deserialize371084514(BaseExplicitFace obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.face = Deserialize497398254(obj.face.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1725266300(BaseFloodSim obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.flatValueGrid != null)
		{
			float[] flatValueGrid = obj.flatValueGrid;
			foreach (float value in flatValueGrid)
			{
				ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
				ProtoWriter.WriteSingle(value, writer);
			}
		}
	}

	private BaseFloodSim Deserialize1725266300(BaseFloodSim obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				List<float> list = new List<float>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					float num2 = 0f;
					num2 = reader.ReadSingle();
					list.Add(num2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.flatValueGrid = list.ToArray();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1585678550(BaseGhost obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 1896640149:
		{
			ProtoWriter.WriteFieldHeader(601, WireType.String, writer);
			SubItemToken token11 = ProtoWriter.StartSubItem(null, writer);
			Serialize1896640149(obj as BaseAddBulkheadGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token11, writer);
			break;
		}
		case 383673295:
		{
			ProtoWriter.WriteFieldHeader(602, WireType.String, writer);
			SubItemToken token10 = ProtoWriter.StartSubItem(null, writer);
			Serialize383673295(obj as BaseAddCellGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token10, writer);
			break;
		}
		case 633495696:
		{
			ProtoWriter.WriteFieldHeader(603, WireType.String, writer);
			SubItemToken token9 = ProtoWriter.StartSubItem(null, writer);
			Serialize633495696(obj as BaseAddConnectorGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token9, writer);
			break;
		}
		case 1506893401:
		{
			ProtoWriter.WriteFieldHeader(604, WireType.String, writer);
			SubItemToken token8 = ProtoWriter.StartSubItem(null, writer);
			Serialize1506893401(obj as BaseAddCorridorGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token8, writer);
			break;
		}
		case 1090472122:
		case 1761765682:
		{
			ProtoWriter.WriteFieldHeader(605, WireType.String, writer);
			SubItemToken token7 = ProtoWriter.StartSubItem(null, writer);
			Serialize1090472122(obj as BaseAddFaceGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token7, writer);
			break;
		}
		case 2060501761:
		{
			ProtoWriter.WriteFieldHeader(606, WireType.String, writer);
			SubItemToken token6 = ProtoWriter.StartSubItem(null, writer);
			Serialize2060501761(obj as BaseAddLadderGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token6, writer);
			break;
		}
		case 1127946067:
		{
			ProtoWriter.WriteFieldHeader(607, WireType.String, writer);
			SubItemToken token5 = ProtoWriter.StartSubItem(null, writer);
			Serialize1127946067(obj as BaseAddWaterPark, objTypeId, writer);
			ProtoWriter.EndSubItem(token5, writer);
			break;
		}
		case 1422648694:
		{
			ProtoWriter.WriteFieldHeader(608, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
			Serialize1422648694(obj as BaseAddMapRoomGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
			break;
		}
		case 862251519:
		{
			ProtoWriter.WriteFieldHeader(609, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize862251519(obj as BaseAddModuleGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
			break;
		}
		case 490474621:
		{
			ProtoWriter.WriteFieldHeader(611, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize490474621(obj as BaseAddPartitionGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1406047219:
		{
			ProtoWriter.WriteFieldHeader(612, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1406047219(obj as BaseAddPartitionDoorGhost, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token12 = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.targetOffset, objTypeId, writer);
		ProtoWriter.EndSubItem(token12, writer);
	}

	private BaseGhost Deserialize1585678550(BaseGhost obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 601:
			{
				SubItemToken token11 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1896640149(obj as BaseAddBulkheadGhost, reader);
				ProtoReader.EndSubItem(token11, reader);
				goto IL_01b6;
			}
			case 602:
			{
				SubItemToken token10 = ProtoReader.StartSubItem(reader);
				obj = Deserialize383673295(obj as BaseAddCellGhost, reader);
				ProtoReader.EndSubItem(token10, reader);
				goto IL_01b6;
			}
			case 603:
			{
				SubItemToken token9 = ProtoReader.StartSubItem(reader);
				obj = Deserialize633495696(obj as BaseAddConnectorGhost, reader);
				ProtoReader.EndSubItem(token9, reader);
				goto IL_01b6;
			}
			case 604:
			{
				SubItemToken token8 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1506893401(obj as BaseAddCorridorGhost, reader);
				ProtoReader.EndSubItem(token8, reader);
				goto IL_01b6;
			}
			case 605:
			{
				SubItemToken token7 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1090472122(obj as BaseAddFaceGhost, reader);
				ProtoReader.EndSubItem(token7, reader);
				goto IL_01b6;
			}
			case 606:
			{
				SubItemToken token6 = ProtoReader.StartSubItem(reader);
				obj = Deserialize2060501761(obj as BaseAddLadderGhost, reader);
				ProtoReader.EndSubItem(token6, reader);
				goto IL_01b6;
			}
			case 607:
			{
				SubItemToken token5 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1127946067(obj as BaseAddWaterPark, reader);
				ProtoReader.EndSubItem(token5, reader);
				goto IL_01b6;
			}
			case 608:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1422648694(obj as BaseAddMapRoomGhost, reader);
				ProtoReader.EndSubItem(token4, reader);
				goto IL_01b6;
			}
			case 609:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj = Deserialize862251519(obj as BaseAddModuleGhost, reader);
				ProtoReader.EndSubItem(token3, reader);
				goto IL_01b6;
			}
			case 611:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize490474621(obj as BaseAddPartitionGhost, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_01b6;
			}
			case 612:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1406047219(obj as BaseAddPartitionDoorGhost, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_01b6;
			}
			}
			break;
			IL_01b6:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			if (num == 1)
			{
				SubItemToken token12 = ProtoReader.StartSubItem(reader);
				obj.targetOffset = Deserialize1691070576(obj.targetOffset, reader);
				ProtoReader.EndSubItem(token12, reader);
			}
			else
			{
				reader.SkipField();
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1384707171(BaseNuclearReactor obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj._protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize497398254(obj._moduleFace, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._constructed, writer);
		if (obj._serializedEquipment != null)
		{
			byte[] serializedEquipment = obj._serializedEquipment;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedEquipment, writer);
		}
		if (obj._serializedEquipmentSlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj._serializedEquipmentSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._toConsume, writer);
	}

	private BaseNuclearReactor Deserialize1384707171(BaseNuclearReactor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._protoVersion = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj._moduleFace = Deserialize497398254(obj._moduleFace, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 3:
				obj._constructed = reader.ReadSingle();
				break;
			case 4:
				obj._serializedEquipment = ProtoReader.AppendBytes(obj._serializedEquipment, reader);
				break;
			case 5:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj._serializedEquipmentSlots = dictionary;
				break;
			}
			case 6:
				obj._toConsume = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize882866214(BasePipeConnector obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.childPipeUID == null)
		{
			return;
		}
		string[] childPipeUID = obj.childPipeUID;
		foreach (string text in childPipeUID)
		{
			if (text != null)
			{
				ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
				ProtoWriter.WriteString(text, writer);
			}
		}
	}

	private BasePipeConnector Deserialize882866214(BasePipeConnector obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				List<string> list = new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.childPipeUID = list.ToArray();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1504796287(BasePowerDistributor obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BasePowerDistributor Deserialize1504796287(BasePowerDistributor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize311542795(BaseRoot obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseRoot Deserialize311542795(BaseRoot obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1045809045(BaseSpotLight obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BaseSpotLight Deserialize1045809045(BaseSpotLight obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1537880658(BaseUpgradeConsole obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj._protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize497398254(obj._moduleFace, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._constructed, writer);
	}

	private BaseUpgradeConsole Deserialize1537880658(BaseUpgradeConsole obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._protoVersion = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj._moduleFace = Deserialize497398254(obj._moduleFace, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 3:
				obj._constructed = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1659399257(Battery obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._charge, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._capacity, writer);
	}

	private Battery Deserialize1659399257(Battery obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.protoVersion = reader.ReadInt32();
				break;
			case 2:
				obj._charge = reader.ReadSingle();
				break;
			case 3:
				obj._capacity = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize788408569(BatteryCharger obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BatteryCharger Deserialize788408569(BatteryCharger obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize929691462(BatterySource obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BatterySource Deserialize929691462(BatterySource obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1711377162(Beacon obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.label != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.label, writer);
		}
	}

	private Beacon Deserialize1711377162(Beacon obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.label = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1011566978(Bioreactor obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Bioreactor Deserialize1011566978(Bioreactor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize40841636(BirdBehaviour obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 469144263)
		{
			ProtoWriter.WriteFieldHeader(4210, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize469144263(obj as Skyray, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private BirdBehaviour Deserialize40841636(BirdBehaviour obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 4210)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize469144263(obj as Skyray, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			reader.SkipField();
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize649861234(Biter obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Biter Deserialize649861234(Biter obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize953017598(Bladderfish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Bladderfish Deserialize953017598(Bladderfish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1041366111(Bleeder obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Bleeder Deserialize1041366111(Bleeder obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize829906308(BloomCreature obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BloomCreature Deserialize829906308(BloomCreature obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1012963343(BlueprintHandTarget obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.used, writer);
	}

	private BlueprintHandTarget Deserialize1012963343(BlueprintHandTarget obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.used = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize242904679(BoneShark obj, int objTypeId, ProtoWriter writer)
	{
	}

	private BoneShark Deserialize242904679(BoneShark obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1716510642(Boomerang obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Boomerang Deserialize1716510642(Boomerang obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1772819529(Breach obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Breach Deserialize1772819529(Breach obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1176920975(CaveCrawler obj, int objTypeId, ProtoWriter writer)
	{
	}

	private CaveCrawler Deserialize1176920975(CaveCrawler obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1956492848(CellManager.CellHeader obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.cellId, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.level, writer);
	}

	private CellManager.CellHeader Deserialize1956492848(CellManager.CellHeader obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new CellManager.CellHeader();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.cellId = Deserialize1691070576(obj.cellId, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 2:
				obj.level = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize949139443(CellManager.CellHeaderEx obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.cellId, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.level, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.dataLength, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.legacyDataLength, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.waiterDataLength, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.allowSpawnRestrictions, writer);
	}

	private CellManager.CellHeaderEx Deserialize949139443(CellManager.CellHeaderEx obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new CellManager.CellHeaderEx();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.cellId = Deserialize1691070576(obj.cellId, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 2:
				obj.level = reader.ReadInt32();
				break;
			case 3:
				obj.dataLength = reader.ReadInt32();
				break;
			case 4:
				obj.legacyDataLength = reader.ReadInt32();
				break;
			case 5:
				obj.waiterDataLength = reader.ReadInt32();
				break;
			case 6:
				obj.allowSpawnRestrictions = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize153467737(CellManager.CellsFileHeader obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.numCells, writer);
	}

	private CellManager.CellsFileHeader Deserialize153467737(CellManager.CellsFileHeader obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new CellManager.CellsFileHeader();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.numCells = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize642877324(Charger obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 788408569:
		{
			ProtoWriter.WriteFieldHeader(100, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize788408569(obj as BatteryCharger, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1386435207:
		{
			ProtoWriter.WriteFieldHeader(200, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1386435207(obj as PowerCellCharger, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.protoVersion, writer);
		if (obj.serializedSlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj.serializedSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token3, writer);
			}
		}
	}

	private Charger Deserialize642877324(Charger obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 100:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize788408569(obj as BatteryCharger, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0051;
			}
			case 200:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1386435207(obj as PowerCellCharger, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0051;
			}
			}
			break;
			IL_0051:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.protoVersion = reader.ReadInt32();
				break;
			case 2:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token3 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token3, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.serializedSlots = dictionary;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize237257528(CoffeeMachineLegacy obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private CoffeeMachineLegacy Deserialize237257528(CoffeeMachineLegacy obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.version = reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize812739867(CollectShiny obj, int objTypeId, ProtoWriter writer)
	{
	}

	private CollectShiny Deserialize812739867(CollectShiny obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize515927774(ColoredLabel obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.text != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.text, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.colorIndex, writer);
	}

	private ColoredLabel Deserialize515927774(ColoredLabel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.text = reader.ReadString();
				break;
			case 3:
				obj.colorIndex = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize396637907(Constructable obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 817046334:
		{
			ProtoWriter.WriteFieldHeader(3100, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
			Serialize817046334(obj as ConstructableBase, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
			break;
		}
		case 1504796287:
		{
			ProtoWriter.WriteFieldHeader(3200, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize1504796287(obj as BasePowerDistributor, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
			break;
		}
		case 1045809045:
		{
			ProtoWriter.WriteFieldHeader(3201, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize1045809045(obj as BaseSpotLight, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 882866214:
		{
			ProtoWriter.WriteFieldHeader(3202, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize882866214(obj as BasePipeConnector, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._constructed, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.constructedAmount, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.techType, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isNew, writer);
		ProtoWriter.WriteFieldHeader(8, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isInside, writer);
	}

	private Constructable Deserialize396637907(Constructable obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 3100:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj = Deserialize817046334(obj as ConstructableBase, reader);
				ProtoReader.EndSubItem(token4, reader);
				goto IL_00a1;
			}
			case 3200:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1504796287(obj as BasePowerDistributor, reader);
				ProtoReader.EndSubItem(token3, reader);
				goto IL_00a1;
			}
			case 3201:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1045809045(obj as BaseSpotLight, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_00a1;
			}
			case 3202:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize882866214(obj as BasePipeConnector, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_00a1;
			}
			}
			break;
			IL_00a1:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj._constructed = reader.ReadBoolean();
				break;
			case 5:
				obj.constructedAmount = reader.ReadSingle();
				break;
			case 6:
				obj.techType = (TechType)reader.ReadInt32();
				break;
			case 7:
				obj.isNew = reader.ReadBoolean();
				break;
			case 8:
				obj.isInside = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize817046334(ConstructableBase obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.faceLinkedModuleType, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.faceLinkedModulePosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.moduleFace.HasValue)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize497398254(obj.moduleFace.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
	}

	private ConstructableBase Deserialize817046334(ConstructableBase obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.protoVersion = reader.ReadInt32();
				break;
			case 2:
				obj.faceLinkedModuleType = (TechType)reader.ReadInt32();
				break;
			case 3:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.faceLinkedModulePosition = Deserialize1181346079(obj.faceLinkedModulePosition, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.moduleFace = Deserialize497398254(obj.moduleFace.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize326697518(Constructor obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.deployed, writer);
	}

	private Constructor Deserialize326697518(Constructor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.deployed = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize257036954(ConstructorInput obj, int objTypeId, ProtoWriter writer)
	{
	}

	private ConstructorInput Deserialize257036954(ConstructorInput obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1998792992(CrabSnake obj, int objTypeId, ProtoWriter writer)
	{
	}

	private CrabSnake Deserialize1998792992(CrabSnake obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize49368518(CrabSquid obj, int objTypeId, ProtoWriter writer)
	{
	}

	private CrabSquid Deserialize49368518(CrabSquid obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize135876261(Crafter obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 802993602:
		case 1011566978:
		case 1800229096:
		{
			ProtoWriter.WriteFieldHeader(5100, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
			Serialize1800229096(obj as PowerCrafter, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
			break;
		}
		case 257036954:
		{
			ProtoWriter.WriteFieldHeader(5200, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize257036954(obj as ConstructorInput, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
			break;
		}
		case 606619525:
		case 630473610:
		case 1333430695:
		{
			ProtoWriter.WriteFieldHeader(5500, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize630473610(obj as GhostCrafter, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1170326782:
		{
			ProtoWriter.WriteFieldHeader(5600, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1170326782(obj as RocketConstructorInput, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
	}

	private Crafter Deserialize135876261(Crafter obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 5100:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1800229096(obj as PowerCrafter, reader);
				ProtoReader.EndSubItem(token4, reader);
				goto IL_009e;
			}
			case 5200:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj = Deserialize257036954(obj as ConstructorInput, reader);
				ProtoReader.EndSubItem(token3, reader);
				goto IL_009e;
			}
			case 5500:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize630473610(obj as GhostCrafter, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_009e;
			}
			case 5600:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1170326782(obj as RocketConstructorInput, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_009e;
			}
			}
			break;
			IL_009e:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			reader.SkipField();
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize2100222829(CrafterLogic obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeCraftingBegin, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeCraftingEnd, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.craftingTechType, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.linkedIndex, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.numCrafted, writer);
	}

	private CrafterLogic Deserialize2100222829(CrafterLogic obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeCraftingBegin = reader.ReadSingle();
				break;
			case 3:
				obj.timeCraftingEnd = reader.ReadSingle();
				break;
			case 4:
				obj.craftingTechType = (TechType)reader.ReadInt32();
				break;
			case 5:
				obj.linkedIndex = reader.ReadInt32();
				break;
			case 6:
				obj.numCrafted = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1703576080(CraftingAnalytics obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj._serializedVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._active, writer);
		if (obj.entries != null)
		{
			Dictionary<TechType, CraftingAnalytics.EntryData>.Enumerator enumerator = obj.entries.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<TechType, CraftingAnalytics.EntryData> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1158933531(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private CraftingAnalytics Deserialize1703576080(CraftingAnalytics obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._serializedVersion = reader.ReadInt32();
				break;
			case 2:
				obj._active = reader.ReadBoolean();
				break;
			case 3:
			{
				Dictionary<TechType, CraftingAnalytics.EntryData> dictionary = obj.entries ?? new Dictionary<TechType, CraftingAnalytics.EntryData>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<TechType, CraftingAnalytics.EntryData> obj2 = default(KeyValuePair<TechType, CraftingAnalytics.EntryData>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1158933531(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1103056798(CraftingAnalytics.EntryData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.timeScanFirst, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.timeScanLast, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.craftCount, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.timeCraftFirst, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.timeCraftLast, writer);
	}

	private CraftingAnalytics.EntryData Deserialize1103056798(CraftingAnalytics.EntryData obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.timeScanFirst = reader.ReadInt32();
				break;
			case 2:
				obj.timeScanLast = reader.ReadInt32();
				break;
			case 3:
				obj.craftCount = reader.ReadInt32();
				break;
			case 4:
				obj.timeCraftFirst = reader.ReadInt32();
				break;
			case 5:
				obj.timeCraftLast = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1234455261(Crash obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Crash Deserialize1234455261(Crash obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1711422107(CrashedShipExploder obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeToStartCountdown, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeSerialized, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeToStartWarning, writer);
	}

	private CrashedShipExploder Deserialize1711422107(CrashedShipExploder obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.timeToStartCountdown = reader.ReadSingle();
				break;
			case 2:
				obj.timeSerialized = reader.ReadSingle();
				break;
			case 3:
				obj.version = reader.ReadInt32();
				break;
			case 4:
				obj.timeToStartWarning = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize660968298(CrashHome obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.spawnTime, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private CrashHome Deserialize660968298(CrashHome obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 2:
				obj.spawnTime = reader.ReadSingle();
				break;
			case 3:
				obj.version = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize356643005(Creature obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 829906308:
		{
			ProtoWriter.WriteFieldHeader(1000, WireType.String, writer);
			SubItemToken token46 = ProtoWriter.StartSubItem(null, writer);
			Serialize829906308(obj as BloomCreature, objTypeId, writer);
			ProtoWriter.EndSubItem(token46, writer);
			break;
		}
		case 1716510642:
		{
			ProtoWriter.WriteFieldHeader(1200, WireType.String, writer);
			SubItemToken token45 = ProtoWriter.StartSubItem(null, writer);
			Serialize1716510642(obj as Boomerang, objTypeId, writer);
			ProtoWriter.EndSubItem(token45, writer);
			break;
		}
		case 353579366:
		{
			ProtoWriter.WriteFieldHeader(1300, WireType.String, writer);
			SubItemToken token44 = ProtoWriter.StartSubItem(null, writer);
			Serialize353579366(obj as LavaLarva, objTypeId, writer);
			ProtoWriter.EndSubItem(token44, writer);
			break;
		}
		case 1829844631:
		{
			ProtoWriter.WriteFieldHeader(1400, WireType.String, writer);
			SubItemToken token43 = ProtoWriter.StartSubItem(null, writer);
			Serialize1829844631(obj as OculusFish, objTypeId, writer);
			ProtoWriter.EndSubItem(token43, writer);
			break;
		}
		case 351821017:
		{
			ProtoWriter.WriteFieldHeader(1500, WireType.String, writer);
			SubItemToken token42 = ProtoWriter.StartSubItem(null, writer);
			Serialize351821017(obj as Eyeye, objTypeId, writer);
			ProtoWriter.EndSubItem(token42, writer);
			break;
		}
		case 1816388137:
		{
			ProtoWriter.WriteFieldHeader(1600, WireType.String, writer);
			SubItemToken token41 = ProtoWriter.StartSubItem(null, writer);
			Serialize1816388137(obj as Garryfish, objTypeId, writer);
			ProtoWriter.EndSubItem(token41, writer);
			break;
		}
		case 476284185:
		{
			ProtoWriter.WriteFieldHeader(1700, WireType.String, writer);
			SubItemToken token40 = ProtoWriter.StartSubItem(null, writer);
			Serialize476284185(obj as GasoPod, objTypeId, writer);
			ProtoWriter.EndSubItem(token40, writer);
			break;
		}
		case 2033371878:
		{
			ProtoWriter.WriteFieldHeader(1800, WireType.String, writer);
			SubItemToken token39 = ProtoWriter.StartSubItem(null, writer);
			Serialize2033371878(obj as Grabcrab, objTypeId, writer);
			ProtoWriter.EndSubItem(token39, writer);
			break;
		}
		case 697741374:
		{
			ProtoWriter.WriteFieldHeader(1900, WireType.String, writer);
			SubItemToken token38 = ProtoWriter.StartSubItem(null, writer);
			Serialize697741374(obj as Grower, objTypeId, writer);
			ProtoWriter.EndSubItem(token38, writer);
			break;
		}
		case 646820922:
		{
			ProtoWriter.WriteFieldHeader(2000, WireType.String, writer);
			SubItemToken token37 = ProtoWriter.StartSubItem(null, writer);
			Serialize646820922(obj as Holefish, objTypeId, writer);
			ProtoWriter.EndSubItem(token37, writer);
			break;
		}
		case 1859566378:
		{
			ProtoWriter.WriteFieldHeader(2100, WireType.String, writer);
			SubItemToken token36 = ProtoWriter.StartSubItem(null, writer);
			Serialize1859566378(obj as Hoverfish, objTypeId, writer);
			ProtoWriter.EndSubItem(token36, writer);
			break;
		}
		case 1338410882:
		{
			ProtoWriter.WriteFieldHeader(2200, WireType.String, writer);
			SubItemToken token35 = ProtoWriter.StartSubItem(null, writer);
			Serialize1338410882(obj as Jellyray, objTypeId, writer);
			ProtoWriter.EndSubItem(token35, writer);
			break;
		}
		case 1252700319:
		{
			ProtoWriter.WriteFieldHeader(2300, WireType.String, writer);
			SubItemToken token34 = ProtoWriter.StartSubItem(null, writer);
			Serialize1252700319(obj as Jumper, objTypeId, writer);
			ProtoWriter.EndSubItem(token34, writer);
			break;
		}
		case 1124891617:
		{
			ProtoWriter.WriteFieldHeader(2400, WireType.String, writer);
			SubItemToken token33 = ProtoWriter.StartSubItem(null, writer);
			Serialize1124891617(obj as Peeper, objTypeId, writer);
			ProtoWriter.EndSubItem(token33, writer);
			break;
		}
		case 1082285174:
		{
			ProtoWriter.WriteFieldHeader(2500, WireType.String, writer);
			SubItemToken token32 = ProtoWriter.StartSubItem(null, writer);
			Serialize1082285174(obj as RabbitRay, objTypeId, writer);
			ProtoWriter.EndSubItem(token32, writer);
			break;
		}
		case 1651949009:
		{
			ProtoWriter.WriteFieldHeader(2600, WireType.String, writer);
			SubItemToken token31 = ProtoWriter.StartSubItem(null, writer);
			Serialize1651949009(obj as Reefback, objTypeId, writer);
			ProtoWriter.EndSubItem(token31, writer);
			break;
		}
		case 924544622:
		{
			ProtoWriter.WriteFieldHeader(2700, WireType.String, writer);
			SubItemToken token30 = ProtoWriter.StartSubItem(null, writer);
			Serialize924544622(obj as Reginald, objTypeId, writer);
			ProtoWriter.EndSubItem(token30, writer);
			break;
		}
		case 81014147:
		{
			ProtoWriter.WriteFieldHeader(2800, WireType.String, writer);
			SubItemToken token29 = ProtoWriter.StartSubItem(null, writer);
			Serialize81014147(obj as SandShark, objTypeId, writer);
			ProtoWriter.EndSubItem(token29, writer);
			break;
		}
		case 647827065:
		{
			ProtoWriter.WriteFieldHeader(2900, WireType.String, writer);
			SubItemToken token28 = ProtoWriter.StartSubItem(null, writer);
			Serialize647827065(obj as Spadefish, objTypeId, writer);
			ProtoWriter.EndSubItem(token28, writer);
			break;
		}
		case 1440414490:
		{
			ProtoWriter.WriteFieldHeader(3000, WireType.String, writer);
			SubItemToken token27 = ProtoWriter.StartSubItem(null, writer);
			Serialize1440414490(obj as Stalker, objTypeId, writer);
			ProtoWriter.EndSubItem(token27, writer);
			break;
		}
		case 953017598:
		{
			ProtoWriter.WriteFieldHeader(3100, WireType.String, writer);
			SubItemToken token26 = ProtoWriter.StartSubItem(null, writer);
			Serialize953017598(obj as Bladderfish, objTypeId, writer);
			ProtoWriter.EndSubItem(token26, writer);
			break;
		}
		case 328727906:
		{
			ProtoWriter.WriteFieldHeader(3200, WireType.String, writer);
			SubItemToken token25 = ProtoWriter.StartSubItem(null, writer);
			Serialize328727906(obj as Hoopfish, objTypeId, writer);
			ProtoWriter.EndSubItem(token25, writer);
			break;
		}
		case 123477383:
		{
			ProtoWriter.WriteFieldHeader(3300, WireType.String, writer);
			SubItemToken token24 = ProtoWriter.StartSubItem(null, writer);
			Serialize123477383(obj as Mesmer, objTypeId, writer);
			ProtoWriter.EndSubItem(token24, writer);
			break;
		}
		case 1041366111:
		{
			ProtoWriter.WriteFieldHeader(3400, WireType.String, writer);
			SubItemToken token23 = ProtoWriter.StartSubItem(null, writer);
			Serialize1041366111(obj as Bleeder, objTypeId, writer);
			ProtoWriter.EndSubItem(token23, writer);
			break;
		}
		case 1355655442:
		{
			ProtoWriter.WriteFieldHeader(3500, WireType.String, writer);
			SubItemToken token22 = ProtoWriter.StartSubItem(null, writer);
			Serialize1355655442(obj as Slime, objTypeId, writer);
			ProtoWriter.EndSubItem(token22, writer);
			break;
		}
		case 1234455261:
		{
			ProtoWriter.WriteFieldHeader(3600, WireType.String, writer);
			SubItemToken token21 = ProtoWriter.StartSubItem(null, writer);
			Serialize1234455261(obj as Crash, objTypeId, writer);
			ProtoWriter.EndSubItem(token21, writer);
			break;
		}
		case 242904679:
		{
			ProtoWriter.WriteFieldHeader(3700, WireType.String, writer);
			SubItemToken token20 = ProtoWriter.StartSubItem(null, writer);
			Serialize242904679(obj as BoneShark, objTypeId, writer);
			ProtoWriter.EndSubItem(token20, writer);
			break;
		}
		case 1880729675:
		{
			ProtoWriter.WriteFieldHeader(3800, WireType.String, writer);
			SubItemToken token19 = ProtoWriter.StartSubItem(null, writer);
			Serialize1880729675(obj as CuteFish, objTypeId, writer);
			ProtoWriter.EndSubItem(token19, writer);
			break;
		}
		case 1332964068:
		{
			ProtoWriter.WriteFieldHeader(3900, WireType.String, writer);
			SubItemToken token18 = ProtoWriter.StartSubItem(null, writer);
			Serialize1332964068(obj as Leviathan, objTypeId, writer);
			ProtoWriter.EndSubItem(token18, writer);
			break;
		}
		case 1702762571:
		{
			ProtoWriter.WriteFieldHeader(4000, WireType.String, writer);
			SubItemToken token17 = ProtoWriter.StartSubItem(null, writer);
			Serialize1702762571(obj as ReaperLeviathan, objTypeId, writer);
			ProtoWriter.EndSubItem(token17, writer);
			break;
		}
		case 1176920975:
		{
			ProtoWriter.WriteFieldHeader(4100, WireType.String, writer);
			SubItemToken token16 = ProtoWriter.StartSubItem(null, writer);
			Serialize1176920975(obj as CaveCrawler, objTypeId, writer);
			ProtoWriter.EndSubItem(token16, writer);
			break;
		}
		case 40841636:
		case 469144263:
		{
			ProtoWriter.WriteFieldHeader(4200, WireType.String, writer);
			SubItemToken token15 = ProtoWriter.StartSubItem(null, writer);
			Serialize40841636(obj as BirdBehaviour, objTypeId, writer);
			ProtoWriter.EndSubItem(token15, writer);
			break;
		}
		case 649861234:
		{
			ProtoWriter.WriteFieldHeader(4400, WireType.String, writer);
			SubItemToken token14 = ProtoWriter.StartSubItem(null, writer);
			Serialize649861234(obj as Biter, objTypeId, writer);
			ProtoWriter.EndSubItem(token14, writer);
			break;
		}
		case 1535225367:
		{
			ProtoWriter.WriteFieldHeader(4500, WireType.String, writer);
			SubItemToken token13 = ProtoWriter.StartSubItem(null, writer);
			Serialize1535225367(obj as Shocker, objTypeId, writer);
			ProtoWriter.EndSubItem(token13, writer);
			break;
		}
		case 1998792992:
		{
			ProtoWriter.WriteFieldHeader(4600, WireType.String, writer);
			SubItemToken token12 = ProtoWriter.StartSubItem(null, writer);
			Serialize1998792992(obj as CrabSnake, objTypeId, writer);
			ProtoWriter.EndSubItem(token12, writer);
			break;
		}
		case 1058623975:
		{
			ProtoWriter.WriteFieldHeader(4700, WireType.String, writer);
			SubItemToken token11 = ProtoWriter.StartSubItem(null, writer);
			Serialize1058623975(obj as SpineEel, objTypeId, writer);
			ProtoWriter.EndSubItem(token11, writer);
			break;
		}
		case 574222852:
		{
			ProtoWriter.WriteFieldHeader(4800, WireType.String, writer);
			SubItemToken token10 = ProtoWriter.StartSubItem(null, writer);
			Serialize574222852(obj as SeaTreader, objTypeId, writer);
			ProtoWriter.EndSubItem(token10, writer);
			break;
		}
		case 49368518:
		{
			ProtoWriter.WriteFieldHeader(4900, WireType.String, writer);
			SubItemToken token9 = ProtoWriter.StartSubItem(null, writer);
			Serialize49368518(obj as CrabSquid, objTypeId, writer);
			ProtoWriter.EndSubItem(token9, writer);
			break;
		}
		case 328683587:
		{
			ProtoWriter.WriteFieldHeader(4910, WireType.String, writer);
			SubItemToken token8 = ProtoWriter.StartSubItem(null, writer);
			Serialize328683587(obj as Warper, objTypeId, writer);
			ProtoWriter.EndSubItem(token8, writer);
			break;
		}
		case 1882829928:
		{
			ProtoWriter.WriteFieldHeader(4920, WireType.String, writer);
			SubItemToken token7 = ProtoWriter.StartSubItem(null, writer);
			Serialize1882829928(obj as LavaLizard, objTypeId, writer);
			ProtoWriter.EndSubItem(token7, writer);
			break;
		}
		case 1606650114:
		{
			ProtoWriter.WriteFieldHeader(5000, WireType.String, writer);
			SubItemToken token6 = ProtoWriter.StartSubItem(null, writer);
			Serialize1606650114(obj as SeaDragon, objTypeId, writer);
			ProtoWriter.EndSubItem(token6, writer);
			break;
		}
		case 1389926401:
		{
			ProtoWriter.WriteFieldHeader(5100, WireType.String, writer);
			SubItemToken token5 = ProtoWriter.StartSubItem(null, writer);
			Serialize1389926401(obj as GhostRay, objTypeId, writer);
			ProtoWriter.EndSubItem(token5, writer);
			break;
		}
		case 1327055215:
		{
			ProtoWriter.WriteFieldHeader(5200, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
			Serialize1327055215(obj as SeaEmperorBaby, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
			break;
		}
		case 35332337:
		{
			ProtoWriter.WriteFieldHeader(5300, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize35332337(obj as GhostLeviathan, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
			break;
		}
		case 1921909083:
		{
			ProtoWriter.WriteFieldHeader(5400, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize1921909083(obj as SeaEmperorJuvenile, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1817260405:
		{
			ProtoWriter.WriteFieldHeader(5500, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1817260405(obj as GhostLeviatanVoid, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token47 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.leashPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token47, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isInitialized, writer);
	}

	private Creature Deserialize356643005(Creature obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 1000:
			{
				SubItemToken token46 = ProtoReader.StartSubItem(reader);
				obj = Deserialize829906308(obj as BloomCreature, reader);
				ProtoReader.EndSubItem(token46, reader);
				goto IL_0731;
			}
			case 1200:
			{
				SubItemToken token45 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1716510642(obj as Boomerang, reader);
				ProtoReader.EndSubItem(token45, reader);
				goto IL_0731;
			}
			case 1300:
			{
				SubItemToken token44 = ProtoReader.StartSubItem(reader);
				obj = Deserialize353579366(obj as LavaLarva, reader);
				ProtoReader.EndSubItem(token44, reader);
				goto IL_0731;
			}
			case 1400:
			{
				SubItemToken token43 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1829844631(obj as OculusFish, reader);
				ProtoReader.EndSubItem(token43, reader);
				goto IL_0731;
			}
			case 1500:
			{
				SubItemToken token42 = ProtoReader.StartSubItem(reader);
				obj = Deserialize351821017(obj as Eyeye, reader);
				ProtoReader.EndSubItem(token42, reader);
				goto IL_0731;
			}
			case 1600:
			{
				SubItemToken token41 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1816388137(obj as Garryfish, reader);
				ProtoReader.EndSubItem(token41, reader);
				goto IL_0731;
			}
			case 1700:
			{
				SubItemToken token40 = ProtoReader.StartSubItem(reader);
				obj = Deserialize476284185(obj as GasoPod, reader);
				ProtoReader.EndSubItem(token40, reader);
				goto IL_0731;
			}
			case 1800:
			{
				SubItemToken token39 = ProtoReader.StartSubItem(reader);
				obj = Deserialize2033371878(obj as Grabcrab, reader);
				ProtoReader.EndSubItem(token39, reader);
				goto IL_0731;
			}
			case 1900:
			{
				SubItemToken token38 = ProtoReader.StartSubItem(reader);
				obj = Deserialize697741374(obj as Grower, reader);
				ProtoReader.EndSubItem(token38, reader);
				goto IL_0731;
			}
			case 2000:
			{
				SubItemToken token37 = ProtoReader.StartSubItem(reader);
				obj = Deserialize646820922(obj as Holefish, reader);
				ProtoReader.EndSubItem(token37, reader);
				goto IL_0731;
			}
			case 2100:
			{
				SubItemToken token36 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1859566378(obj as Hoverfish, reader);
				ProtoReader.EndSubItem(token36, reader);
				goto IL_0731;
			}
			case 2200:
			{
				SubItemToken token35 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1338410882(obj as Jellyray, reader);
				ProtoReader.EndSubItem(token35, reader);
				goto IL_0731;
			}
			case 2300:
			{
				SubItemToken token34 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1252700319(obj as Jumper, reader);
				ProtoReader.EndSubItem(token34, reader);
				goto IL_0731;
			}
			case 2400:
			{
				SubItemToken token33 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1124891617(obj as Peeper, reader);
				ProtoReader.EndSubItem(token33, reader);
				goto IL_0731;
			}
			case 2500:
			{
				SubItemToken token32 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1082285174(obj as RabbitRay, reader);
				ProtoReader.EndSubItem(token32, reader);
				goto IL_0731;
			}
			case 2600:
			{
				SubItemToken token31 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1651949009(obj as Reefback, reader);
				ProtoReader.EndSubItem(token31, reader);
				goto IL_0731;
			}
			case 2700:
			{
				SubItemToken token30 = ProtoReader.StartSubItem(reader);
				obj = Deserialize924544622(obj as Reginald, reader);
				ProtoReader.EndSubItem(token30, reader);
				goto IL_0731;
			}
			case 2800:
			{
				SubItemToken token29 = ProtoReader.StartSubItem(reader);
				obj = Deserialize81014147(obj as SandShark, reader);
				ProtoReader.EndSubItem(token29, reader);
				goto IL_0731;
			}
			case 2900:
			{
				SubItemToken token28 = ProtoReader.StartSubItem(reader);
				obj = Deserialize647827065(obj as Spadefish, reader);
				ProtoReader.EndSubItem(token28, reader);
				goto IL_0731;
			}
			case 3000:
			{
				SubItemToken token27 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1440414490(obj as Stalker, reader);
				ProtoReader.EndSubItem(token27, reader);
				goto IL_0731;
			}
			case 3100:
			{
				SubItemToken token26 = ProtoReader.StartSubItem(reader);
				obj = Deserialize953017598(obj as Bladderfish, reader);
				ProtoReader.EndSubItem(token26, reader);
				goto IL_0731;
			}
			case 3200:
			{
				SubItemToken token25 = ProtoReader.StartSubItem(reader);
				obj = Deserialize328727906(obj as Hoopfish, reader);
				ProtoReader.EndSubItem(token25, reader);
				goto IL_0731;
			}
			case 3300:
			{
				SubItemToken token24 = ProtoReader.StartSubItem(reader);
				obj = Deserialize123477383(obj as Mesmer, reader);
				ProtoReader.EndSubItem(token24, reader);
				goto IL_0731;
			}
			case 3400:
			{
				SubItemToken token23 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1041366111(obj as Bleeder, reader);
				ProtoReader.EndSubItem(token23, reader);
				goto IL_0731;
			}
			case 3500:
			{
				SubItemToken token22 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1355655442(obj as Slime, reader);
				ProtoReader.EndSubItem(token22, reader);
				goto IL_0731;
			}
			case 3600:
			{
				SubItemToken token21 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1234455261(obj as Crash, reader);
				ProtoReader.EndSubItem(token21, reader);
				goto IL_0731;
			}
			case 3700:
			{
				SubItemToken token20 = ProtoReader.StartSubItem(reader);
				obj = Deserialize242904679(obj as BoneShark, reader);
				ProtoReader.EndSubItem(token20, reader);
				goto IL_0731;
			}
			case 3800:
			{
				SubItemToken token19 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1880729675(obj as CuteFish, reader);
				ProtoReader.EndSubItem(token19, reader);
				goto IL_0731;
			}
			case 3900:
			{
				SubItemToken token18 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1332964068(obj as Leviathan, reader);
				ProtoReader.EndSubItem(token18, reader);
				goto IL_0731;
			}
			case 4000:
			{
				SubItemToken token17 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1702762571(obj as ReaperLeviathan, reader);
				ProtoReader.EndSubItem(token17, reader);
				goto IL_0731;
			}
			case 4100:
			{
				SubItemToken token16 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1176920975(obj as CaveCrawler, reader);
				ProtoReader.EndSubItem(token16, reader);
				goto IL_0731;
			}
			case 4200:
			{
				SubItemToken token15 = ProtoReader.StartSubItem(reader);
				obj = Deserialize40841636(obj as BirdBehaviour, reader);
				ProtoReader.EndSubItem(token15, reader);
				goto IL_0731;
			}
			case 4400:
			{
				SubItemToken token14 = ProtoReader.StartSubItem(reader);
				obj = Deserialize649861234(obj as Biter, reader);
				ProtoReader.EndSubItem(token14, reader);
				goto IL_0731;
			}
			case 4500:
			{
				SubItemToken token13 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1535225367(obj as Shocker, reader);
				ProtoReader.EndSubItem(token13, reader);
				goto IL_0731;
			}
			case 4600:
			{
				SubItemToken token12 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1998792992(obj as CrabSnake, reader);
				ProtoReader.EndSubItem(token12, reader);
				goto IL_0731;
			}
			case 4700:
			{
				SubItemToken token11 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1058623975(obj as SpineEel, reader);
				ProtoReader.EndSubItem(token11, reader);
				goto IL_0731;
			}
			case 4800:
			{
				SubItemToken token10 = ProtoReader.StartSubItem(reader);
				obj = Deserialize574222852(obj as SeaTreader, reader);
				ProtoReader.EndSubItem(token10, reader);
				goto IL_0731;
			}
			case 4900:
			{
				SubItemToken token9 = ProtoReader.StartSubItem(reader);
				obj = Deserialize49368518(obj as CrabSquid, reader);
				ProtoReader.EndSubItem(token9, reader);
				goto IL_0731;
			}
			case 4910:
			{
				SubItemToken token8 = ProtoReader.StartSubItem(reader);
				obj = Deserialize328683587(obj as Warper, reader);
				ProtoReader.EndSubItem(token8, reader);
				goto IL_0731;
			}
			case 4920:
			{
				SubItemToken token7 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1882829928(obj as LavaLizard, reader);
				ProtoReader.EndSubItem(token7, reader);
				goto IL_0731;
			}
			case 5000:
			{
				SubItemToken token6 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1606650114(obj as SeaDragon, reader);
				ProtoReader.EndSubItem(token6, reader);
				goto IL_0731;
			}
			case 5100:
			{
				SubItemToken token5 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1389926401(obj as GhostRay, reader);
				ProtoReader.EndSubItem(token5, reader);
				goto IL_0731;
			}
			case 5200:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1327055215(obj as SeaEmperorBaby, reader);
				ProtoReader.EndSubItem(token4, reader);
				goto IL_0731;
			}
			case 5300:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj = Deserialize35332337(obj as GhostLeviathan, reader);
				ProtoReader.EndSubItem(token3, reader);
				goto IL_0731;
			}
			case 5400:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1921909083(obj as SeaEmperorJuvenile, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0731;
			}
			case 5500:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1817260405(obj as GhostLeviatanVoid, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0731;
			}
			}
			break;
			IL_0731:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token47 = ProtoReader.StartSubItem(reader);
				obj.leashPosition = Deserialize1181346079(obj.leashPosition, reader);
				ProtoReader.EndSubItem(token47, reader);
				break;
			}
			case 2:
				obj.version = reader.ReadInt32();
				break;
			case 3:
				obj.isInitialized = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize406373562(CreatureBehaviour obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.leashPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private CreatureBehaviour Deserialize406373562(CreatureBehaviour obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.leashPosition = Deserialize1181346079(obj.leashPosition, reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize801838245(CreatureDeath obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasSpawnedRespawner, writer);
	}

	private CreatureDeath Deserialize801838245(CreatureDeath obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 3)
			{
				obj.hasSpawnedRespawner = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize59821228(CreatureEgg obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.progress, writer);
	}

	private CreatureEgg Deserialize59821228(CreatureEgg obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.progress = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2001815917(CreatureFriend obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed64, writer);
		ProtoWriter.WriteDouble(obj.timeFriendshipEnd, writer);
		if (obj.currentFriendUID != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteString(obj.currentFriendUID, writer);
		}
	}

	private CreatureFriend Deserialize2001815917(CreatureFriend obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeFriendshipEnd = reader.ReadDouble();
				break;
			case 3:
				obj.currentFriendUID = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize730995178(CreepvineSeed obj, int objTypeId, ProtoWriter writer)
	{
	}

	private CreepvineSeed Deserialize730995178(CreepvineSeed obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize860890656(CurrentGenerator obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isActive, writer);
	}

	private CurrentGenerator Deserialize860890656(CurrentGenerator obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.isActive = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1880729675(CuteFish obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._followingPlayer, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._goodbyePlayed, writer);
	}

	private CuteFish Deserialize1880729675(CuteFish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._followingPlayer = reader.ReadBoolean();
				break;
			case 2:
				obj._goodbyePlayed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1489909791(CyclopsDecoyLoadingTube obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.serializedDecoySlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj.serializedDecoySlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private CyclopsDecoyLoadingTube Deserialize1489909791(CyclopsDecoyLoadingTube obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.serializedDecoySlots = dictionary;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2083308735(CyclopsLightingPanel obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.lightingOn, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.floodlightsOn, writer);
	}

	private CyclopsLightingPanel Deserialize2083308735(CyclopsLightingPanel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.lightingOn = reader.ReadBoolean();
				break;
			case 2:
				obj.floodlightsOn = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2062152363(CyclopsMotorMode obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.engineOn, writer);
	}

	private CyclopsMotorMode Deserialize2062152363(CyclopsMotorMode obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.engineOn = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize315488296(DayNightCycle obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timePassedDeprecated, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed64, writer);
		ProtoWriter.WriteDouble(obj.timePassedAsDouble, writer);
	}

	private DayNightCycle Deserialize315488296(DayNightCycle obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timePassedDeprecated = reader.ReadSingle();
				break;
			case 3:
				obj.timePassedAsDouble = reader.ReadDouble();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize26260290(DayNightLight obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.colorR != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.colorR, writer);
			Serialize118512508(obj.colorR, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		if (obj.colorG != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.colorG, writer);
			Serialize118512508(obj.colorG, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		if (obj.colorB != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(obj.colorB, writer);
			Serialize118512508(obj.colorB, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
		}
		if (obj.intensity != null)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(obj.intensity, writer);
			Serialize118512508(obj.intensity, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
		}
		if (obj.sunFraction != null)
		{
			ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
			SubItemToken token5 = ProtoWriter.StartSubItem(obj.sunFraction, writer);
			Serialize118512508(obj.sunFraction, objTypeId, writer);
			ProtoWriter.EndSubItem(token5, writer);
		}
	}

	private DayNightLight Deserialize26260290(DayNightLight obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token5 = ProtoReader.StartSubItem(reader);
				obj.colorR = Deserialize118512508(obj.colorR, reader);
				ProtoReader.EndSubItem(token5, reader);
				break;
			}
			case 2:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj.colorG = Deserialize118512508(obj.colorG, reader);
				ProtoReader.EndSubItem(token4, reader);
				break;
			}
			case 3:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.colorB = Deserialize118512508(obj.colorB, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 4:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.intensity = Deserialize118512508(obj.intensity, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 5:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.sunFraction = Deserialize118512508(obj.sunFraction, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize898747676(DisableBeforeExplosion obj, int objTypeId, ProtoWriter writer)
	{
	}

	private DisableBeforeExplosion Deserialize898747676(DisableBeforeExplosion obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize10881664(DiveReel obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.state, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.nodePositions != null)
		{
			List<Vector3>.Enumerator enumerator = obj.nodePositions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Vector3 current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private DiveReel Deserialize10881664(DiveReel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.state = reader.ReadInt32();
				break;
			case 2:
				obj.version = reader.ReadInt32();
				break;
			case 3:
			{
				List<Vector3> list = new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.nodePositions = list;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1954617231(DiveReelAnchor obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.reelId != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.reelId, writer);
		}
		if (obj.contactPts != null)
		{
			List<Vector3>.Enumerator enumerator = obj.contactPts.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Vector3 current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.lastContactUnraveling, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.prevLineEndPos, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.reelWasDropped, writer);
	}

	private DiveReelAnchor Deserialize1954617231(DiveReelAnchor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.reelId = reader.ReadString();
				break;
			case 2:
			{
				List<Vector3> list = obj.contactPts ?? new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token2, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			case 3:
				obj.lastContactUnraveling = reader.ReadBoolean();
				break;
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.prevLineEndPos = Deserialize1181346079(obj.prevLineEndPos, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 5:
				obj.reelWasDropped = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2100518191(Drillable obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.health != null)
		{
			float[] health = obj.health;
			foreach (float value in health)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
				ProtoWriter.WriteSingle(value, writer);
			}
		}
	}

	private Drillable Deserialize2100518191(Drillable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				List<float> list = new List<float>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					float num2 = 0f;
					num2 = reader.ReadSingle();
					list.Add(num2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.health = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize662718200(DropEnzymes obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeNextDrop, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.firstDrop, writer);
	}

	private DropEnzymes Deserialize662718200(DropEnzymes obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.timeNextDrop = reader.ReadSingle();
				break;
			case 2:
				obj.firstDrop = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize790665541(Durable obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Durable Deserialize790665541(Durable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1080881920(Eatable obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeDecayStart, writer);
	}

	private Eatable Deserialize1080881920(Eatable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.timeDecayStart = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1427962681(EnergyMixin obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 929691462)
		{
			ProtoWriter.WriteFieldHeader(1000, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize929691462(obj as BatterySource, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.energy, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.maxEnergy, writer);
	}

	private EnergyMixin Deserialize1427962681(EnergyMixin obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 1000)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize929691462(obj as BatterySource, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.energy = reader.ReadSingle();
				break;
			case 2:
				obj.maxEnergy = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1922274571(EntitySlot obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.allowedTypes != null)
		{
			List<EntitySlot.Type>.Enumerator enumerator = obj.allowedTypes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EntitySlot.Type current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.biomeType, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.autoGenerated, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.density, writer);
	}

	private EntitySlot Deserialize1922274571(EntitySlot obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 2:
			{
				List<EntitySlot.Type> list = new List<EntitySlot.Type>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					EntitySlot.Type type = EntitySlot.Type.Small;
					type = (EntitySlot.Type)reader.ReadInt32();
					list.Add(type);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.allowedTypes = list;
				break;
			}
			case 4:
				obj.biomeType = (BiomeType)reader.ReadInt32();
				break;
			case 6:
				obj.autoGenerated = reader.ReadBoolean();
				break;
			case 7:
				obj.density = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1404549125(EntitySlotData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.biomeType, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.allowedTypes, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.density, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.localPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize605020259(obj.localRotation, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
	}

	private EntitySlotData Deserialize1404549125(EntitySlotData obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new EntitySlotData();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.biomeType = (BiomeType)reader.ReadInt32();
				break;
			case 3:
				obj.allowedTypes = (EntitySlotData.EntitySlotType)reader.ReadInt32();
				break;
			case 4:
				obj.density = reader.ReadSingle();
				break;
			case 5:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.localPosition = Deserialize1181346079(obj.localPosition, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 6:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.localRotation = Deserialize605020259(obj.localRotation, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1971823303(EntitySlotsPlaceholder obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.slotsData == null)
		{
			return;
		}
		EntitySlotData[] slotsData = obj.slotsData;
		foreach (EntitySlotData entitySlotData in slotsData)
		{
			if (entitySlotData != null)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(entitySlotData, writer);
				Serialize1404549125(entitySlotData, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private EntitySlotsPlaceholder Deserialize1971823303(EntitySlotsPlaceholder obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				List<EntitySlotData> list = new List<EntitySlotData>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					EntitySlotData obj2 = new EntitySlotData();
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1404549125(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.slotsData = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize443301464(EscapePod obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.serializedStorage != null)
		{
			byte[] serializedStorage = obj.serializedStorage;
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedStorage, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.startedIntroCinematic, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.anchorPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.bottomHatchUsed, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.topHatchUsed, writer);
	}

	private EscapePod Deserialize443301464(EscapePod obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.serializedStorage = ProtoReader.AppendBytes(obj.serializedStorage, reader);
				break;
			case 3:
				obj.startedIntroCinematic = reader.ReadBoolean();
				break;
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.anchorPosition = Deserialize1181346079(obj.anchorPosition, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 5:
				obj.bottomHatchUsed = reader.ReadBoolean();
				break;
			case 6:
				obj.topHatchUsed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1433797848(ExecutionOrderTest obj, int objTypeId, ProtoWriter writer)
	{
	}

	private ExecutionOrderTest Deserialize1433797848(ExecutionOrderTest obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1174302959(Exosuit obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Exosuit Deserialize1174302959(Exosuit obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize351821017(Eyeye obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Eyeye Deserialize351821017(Eyeye obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize606619525(Fabricator obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Fabricator Deserialize606619525(Fabricator obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize776720259(FairRandomizer obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeLastCheck, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.entropy, writer);
	}

	private FairRandomizer Deserialize776720259(FairRandomizer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeLastCheck = reader.ReadSingle();
				break;
			case 3:
				obj.entropy = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1004993247(FiltrationMachine obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeRemainingWater, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeRemainingSalt, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize497398254(obj._moduleFace, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._constructed, writer);
	}

	private FiltrationMachine Deserialize1004993247(FiltrationMachine obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeRemainingWater = reader.ReadSingle();
				break;
			case 3:
				obj.timeRemainingSalt = reader.ReadSingle();
				break;
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj._moduleFace = Deserialize497398254(obj._moduleFace, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 5:
				obj._constructed = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize325421431(FireExtinguisher obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fuel, writer);
	}

	private FireExtinguisher Deserialize325421431(FireExtinguisher obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.fuel = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1618351061(FireExtinguisherHolder obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasTank, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fuel, writer);
	}

	private FireExtinguisherHolder Deserialize1618351061(FireExtinguisherHolder obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.hasTank = reader.ReadBoolean();
				break;
			case 3:
				obj.fuel = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize304513582(Flare obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.energyLeft, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.flareActiveState, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasBeenThrown, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.flareActivateTime, writer);
	}

	private Flare Deserialize304513582(Flare obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.energyLeft = reader.ReadSingle();
				break;
			case 2:
				obj.flareActiveState = reader.ReadBoolean();
				break;
			case 3:
				obj.hasBeenThrown = reader.ReadBoolean();
				break;
			case 4:
				obj.flareActivateTime = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize108625815(FogSettings obj, int objTypeId, ProtoWriter writer)
	{
		obj.OnBeforeSerialization();
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.color, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.startDistance, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.maxDistance, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.absorptionSpeed, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.sunGlowAmount, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.dayNightColor != null)
		{
			ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.dayNightColor, writer);
			Serialize175529349(obj.dayNightColor, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		ProtoWriter.WriteFieldHeader(9, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.depthDispersion, writer);
		ProtoWriter.WriteFieldHeader(10, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.scatteringScale, writer);
	}

	private FogSettings Deserialize108625815(FogSettings obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new FogSettings();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.color = Deserialize1404661584(obj.color, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 3:
				obj.startDistance = reader.ReadSingle();
				break;
			case 4:
				obj.maxDistance = reader.ReadSingle();
				break;
			case 5:
				obj.absorptionSpeed = reader.ReadSingle();
				break;
			case 6:
				obj.sunGlowAmount = reader.ReadSingle();
				break;
			case 7:
				obj.version = reader.ReadInt32();
				break;
			case 8:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.dayNightColor = Deserialize175529349(obj.dayNightColor, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 9:
				obj.depthDispersion = reader.ReadSingle();
				break;
			case 10:
				obj.scatteringScale = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj.OnAfterDeserialization();
		return obj;
	}

	private void Serialize41335609(FruitPlant obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.fruitSpawnEnabled, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeNextFruit, writer);
	}

	private FruitPlant Deserialize41335609(FruitPlant obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.fruitSpawnEnabled = reader.ReadBoolean();
				break;
			case 3:
				obj.timeNextFruit = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1816388137(Garryfish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Garryfish Deserialize1816388137(Garryfish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize476284185(GasoPod obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GasoPod Deserialize476284185(GasoPod obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize459225532(GenericConsole obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.gotUsed, writer);
	}

	private GenericConsole Deserialize459225532(GenericConsole obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.gotUsed = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize630473610(GhostCrafter obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 606619525:
		{
			ProtoWriter.WriteFieldHeader(5100, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize606619525(obj as Fabricator, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1333430695:
		{
			ProtoWriter.WriteFieldHeader(5200, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1333430695(obj as Workbench, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
	}

	private GhostCrafter Deserialize630473610(GhostCrafter obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 5100:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize606619525(obj as Fabricator, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0051;
			}
			case 5200:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1333430695(obj as Workbench, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0051;
			}
			}
			break;
			IL_0051:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			reader.SkipField();
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1817260405(GhostLeviatanVoid obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GhostLeviatanVoid Deserialize1817260405(GhostLeviatanVoid obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize35332337(GhostLeviathan obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GhostLeviathan Deserialize35332337(GhostLeviathan obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize530216075(GhostPickupable obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GhostPickupable Deserialize530216075(GhostPickupable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1389926401(GhostRay obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GhostRay Deserialize1389926401(GhostRay obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize2033371878(Grabcrab obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Grabcrab Deserialize2033371878(Grabcrab obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1182280616(Grid3<float> obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize997267884(obj.shape, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.values != null)
		{
			float[] values = obj.values;
			foreach (float value in values)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
				ProtoWriter.WriteSingle(value, writer);
			}
		}
	}

	private Grid3<float> Deserialize1182280616(Grid3<float> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new Grid3<float>();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.shape = Deserialize997267884(obj.shape, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 2:
			{
				List<float> list = new List<float>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					float num2 = 0f;
					num2 = reader.ReadSingle();
					list.Add(num2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.values = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1765532648(Grid3<Vector3> obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize997267884(obj.shape, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.values != null)
		{
			Vector3[] values = obj.values;
			foreach (Vector3 obj2 in values)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
	}

	private Grid3<Vector3> Deserialize1765532648(Grid3<Vector3> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new Grid3<Vector3>();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.shape = Deserialize997267884(obj.shape, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				List<Vector3> list = new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.values = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize997267884(Grid3Shape obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.z, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.xy, writer);
	}

	private Grid3Shape Deserialize997267884(Grid3Shape obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadInt32();
				break;
			case 2:
				obj.y = reader.ReadInt32();
				break;
			case 3:
				obj.z = reader.ReadInt32();
				break;
			case 4:
				obj.xy = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize697741374(Grower obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Grower Deserialize697741374(Grower obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize792963384(GrowingPlant obj, int objTypeId, ProtoWriter writer)
	{
	}

	private GrowingPlant Deserialize792963384(GrowingPlant obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize113236186(GrownPlant obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.seedUID != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.seedUID, writer);
		}
	}

	private GrownPlant Deserialize113236186(GrownPlant obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.seedUID = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2066256250(HandTarget obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 723629918:
		case 730995178:
		{
			ProtoWriter.WriteFieldHeader(1000, WireType.String, writer);
			SubItemToken token23 = ProtoWriter.StartSubItem(null, writer);
			Serialize723629918(obj as Pickupable, objTypeId, writer);
			ProtoWriter.EndSubItem(token23, writer);
			break;
		}
		case 366077262:
		{
			ProtoWriter.WriteFieldHeader(1100, WireType.String, writer);
			SubItemToken token22 = ProtoWriter.StartSubItem(null, writer);
			Serialize366077262(obj as StorageContainer, objTypeId, writer);
			ProtoWriter.EndSubItem(token22, writer);
			break;
		}
		case 1954617231:
		{
			ProtoWriter.WriteFieldHeader(1200, WireType.String, writer);
			SubItemToken token21 = ProtoWriter.StartSubItem(null, writer);
			Serialize1954617231(obj as DiveReelAnchor, objTypeId, writer);
			ProtoWriter.EndSubItem(token21, writer);
			break;
		}
		case 1297003913:
		{
			ProtoWriter.WriteFieldHeader(1300, WireType.String, writer);
			SubItemToken token20 = ProtoWriter.StartSubItem(null, writer);
			Serialize1297003913(obj as UpgradeConsole, objTypeId, writer);
			ProtoWriter.EndSubItem(token20, writer);
			break;
		}
		case 1084867387:
		{
			ProtoWriter.WriteFieldHeader(1400, WireType.String, writer);
			SubItemToken token19 = ProtoWriter.StartSubItem(null, writer);
			Serialize1084867387(obj as Sign, objTypeId, writer);
			ProtoWriter.EndSubItem(token19, writer);
			break;
		}
		case 515927774:
		{
			ProtoWriter.WriteFieldHeader(1500, WireType.String, writer);
			SubItemToken token18 = ProtoWriter.StartSubItem(null, writer);
			Serialize515927774(obj as ColoredLabel, objTypeId, writer);
			ProtoWriter.EndSubItem(token18, writer);
			break;
		}
		case 1289092887:
		{
			ProtoWriter.WriteFieldHeader(1600, WireType.String, writer);
			SubItemToken token17 = ProtoWriter.StartSubItem(null, writer);
			Serialize1289092887(obj as PickPrefab, objTypeId, writer);
			ProtoWriter.EndSubItem(token17, writer);
			break;
		}
		case 1983352738:
		{
			ProtoWriter.WriteFieldHeader(1800, WireType.String, writer);
			SubItemToken token16 = ProtoWriter.StartSubItem(null, writer);
			Serialize1983352738(obj as ThermalPlant, objTypeId, writer);
			ProtoWriter.EndSubItem(token16, writer);
			break;
		}
		case 113236186:
		{
			ProtoWriter.WriteFieldHeader(1900, WireType.String, writer);
			SubItemToken token15 = ProtoWriter.StartSubItem(null, writer);
			Serialize113236186(obj as GrownPlant, objTypeId, writer);
			ProtoWriter.EndSubItem(token15, writer);
			break;
		}
		case 792963384:
		{
			ProtoWriter.WriteFieldHeader(1950, WireType.String, writer);
			SubItemToken token14 = ProtoWriter.StartSubItem(null, writer);
			Serialize792963384(obj as GrowingPlant, objTypeId, writer);
			ProtoWriter.EndSubItem(token14, writer);
			break;
		}
		case 1113570486:
		case 1174302959:
		case 2106147891:
		{
			ProtoWriter.WriteFieldHeader(2000, WireType.String, writer);
			SubItemToken token13 = ProtoWriter.StartSubItem(null, writer);
			Serialize1113570486(obj as Vehicle, objTypeId, writer);
			ProtoWriter.EndSubItem(token13, writer);
			break;
		}
		case 396637907:
		case 817046334:
		case 882866214:
		case 1045809045:
		case 1504796287:
		{
			ProtoWriter.WriteFieldHeader(3000, WireType.String, writer);
			SubItemToken token12 = ProtoWriter.StartSubItem(null, writer);
			Serialize396637907(obj as Constructable, objTypeId, writer);
			ProtoWriter.EndSubItem(token12, writer);
			break;
		}
		case 1555952712:
		{
			ProtoWriter.WriteFieldHeader(4000, WireType.String, writer);
			SubItemToken token11 = ProtoWriter.StartSubItem(null, writer);
			Serialize1555952712(obj as SupplyCrate, objTypeId, writer);
			ProtoWriter.EndSubItem(token11, writer);
			break;
		}
		case 135876261:
		case 257036954:
		case 606619525:
		case 630473610:
		case 802993602:
		case 1011566978:
		case 1170326782:
		case 1333430695:
		case 1800229096:
		{
			ProtoWriter.WriteFieldHeader(5000, WireType.String, writer);
			SubItemToken token10 = ProtoWriter.StartSubItem(null, writer);
			Serialize135876261(obj as Crafter, objTypeId, writer);
			ProtoWriter.EndSubItem(token10, writer);
			break;
		}
		case 737691900:
		{
			ProtoWriter.WriteFieldHeader(6000, WireType.String, writer);
			SubItemToken token9 = ProtoWriter.StartSubItem(null, writer);
			Serialize737691900(obj as StarshipDoor, objTypeId, writer);
			ProtoWriter.EndSubItem(token9, writer);
			break;
		}
		case 670501331:
		{
			ProtoWriter.WriteFieldHeader(7000, WireType.String, writer);
			SubItemToken token8 = ProtoWriter.StartSubItem(null, writer);
			Serialize670501331(obj as MedicalCabinet, objTypeId, writer);
			ProtoWriter.EndSubItem(token8, writer);
			break;
		}
		case 1343493277:
		{
			ProtoWriter.WriteFieldHeader(8000, WireType.String, writer);
			SubItemToken token7 = ProtoWriter.StartSubItem(null, writer);
			Serialize1343493277(obj as MapRoomScreen, objTypeId, writer);
			ProtoWriter.EndSubItem(token7, writer);
			break;
		}
		case 530216075:
		{
			ProtoWriter.WriteFieldHeader(10000, WireType.String, writer);
			SubItemToken token6 = ProtoWriter.StartSubItem(null, writer);
			Serialize530216075(obj as GhostPickupable, objTypeId, writer);
			ProtoWriter.EndSubItem(token6, writer);
			break;
		}
		case 840818195:
		{
			ProtoWriter.WriteFieldHeader(11000, WireType.String, writer);
			SubItemToken token5 = ProtoWriter.StartSubItem(null, writer);
			Serialize840818195(obj as WeldableWallPanelGeneric, objTypeId, writer);
			ProtoWriter.EndSubItem(token5, writer);
			break;
		}
		case 1505210158:
		{
			ProtoWriter.WriteFieldHeader(12000, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
			Serialize1505210158(obj as PrecursorDoorKeyColumn, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
			break;
		}
		case 2051895012:
		{
			ProtoWriter.WriteFieldHeader(13000, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize2051895012(obj as PrecursorKeyTerminal, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
			break;
		}
		case 1713660373:
		{
			ProtoWriter.WriteFieldHeader(14000, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize1713660373(obj as PrecursorTeleporterActivationTerminal, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1457139245:
		{
			ProtoWriter.WriteFieldHeader(15000, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1457139245(obj as PrecursorDisableGunTerminal, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
	}

	private HandTarget Deserialize2066256250(HandTarget obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 1000:
			{
				SubItemToken token23 = ProtoReader.StartSubItem(reader);
				obj = Deserialize723629918(obj as Pickupable, reader);
				ProtoReader.EndSubItem(token23, reader);
				goto IL_0396;
			}
			case 1100:
			{
				SubItemToken token22 = ProtoReader.StartSubItem(reader);
				obj = Deserialize366077262(obj as StorageContainer, reader);
				ProtoReader.EndSubItem(token22, reader);
				goto IL_0396;
			}
			case 1200:
			{
				SubItemToken token21 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1954617231(obj as DiveReelAnchor, reader);
				ProtoReader.EndSubItem(token21, reader);
				goto IL_0396;
			}
			case 1300:
			{
				SubItemToken token20 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1297003913(obj as UpgradeConsole, reader);
				ProtoReader.EndSubItem(token20, reader);
				goto IL_0396;
			}
			case 1400:
			{
				SubItemToken token19 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1084867387(obj as Sign, reader);
				ProtoReader.EndSubItem(token19, reader);
				goto IL_0396;
			}
			case 1500:
			{
				SubItemToken token18 = ProtoReader.StartSubItem(reader);
				obj = Deserialize515927774(obj as ColoredLabel, reader);
				ProtoReader.EndSubItem(token18, reader);
				goto IL_0396;
			}
			case 1600:
			{
				SubItemToken token17 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1289092887(obj as PickPrefab, reader);
				ProtoReader.EndSubItem(token17, reader);
				goto IL_0396;
			}
			case 1800:
			{
				SubItemToken token16 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1983352738(obj as ThermalPlant, reader);
				ProtoReader.EndSubItem(token16, reader);
				goto IL_0396;
			}
			case 1900:
			{
				SubItemToken token15 = ProtoReader.StartSubItem(reader);
				obj = Deserialize113236186(obj as GrownPlant, reader);
				ProtoReader.EndSubItem(token15, reader);
				goto IL_0396;
			}
			case 1950:
			{
				SubItemToken token14 = ProtoReader.StartSubItem(reader);
				obj = Deserialize792963384(obj as GrowingPlant, reader);
				ProtoReader.EndSubItem(token14, reader);
				goto IL_0396;
			}
			case 2000:
			{
				SubItemToken token13 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1113570486(obj as Vehicle, reader);
				ProtoReader.EndSubItem(token13, reader);
				goto IL_0396;
			}
			case 3000:
			{
				SubItemToken token12 = ProtoReader.StartSubItem(reader);
				obj = Deserialize396637907(obj as Constructable, reader);
				ProtoReader.EndSubItem(token12, reader);
				goto IL_0396;
			}
			case 4000:
			{
				SubItemToken token11 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1555952712(obj as SupplyCrate, reader);
				ProtoReader.EndSubItem(token11, reader);
				goto IL_0396;
			}
			case 5000:
			{
				SubItemToken token10 = ProtoReader.StartSubItem(reader);
				obj = Deserialize135876261(obj as Crafter, reader);
				ProtoReader.EndSubItem(token10, reader);
				goto IL_0396;
			}
			case 6000:
			{
				SubItemToken token9 = ProtoReader.StartSubItem(reader);
				obj = Deserialize737691900(obj as StarshipDoor, reader);
				ProtoReader.EndSubItem(token9, reader);
				goto IL_0396;
			}
			case 7000:
			{
				SubItemToken token8 = ProtoReader.StartSubItem(reader);
				obj = Deserialize670501331(obj as MedicalCabinet, reader);
				ProtoReader.EndSubItem(token8, reader);
				goto IL_0396;
			}
			case 8000:
			{
				SubItemToken token7 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1343493277(obj as MapRoomScreen, reader);
				ProtoReader.EndSubItem(token7, reader);
				goto IL_0396;
			}
			case 10000:
			{
				SubItemToken token6 = ProtoReader.StartSubItem(reader);
				obj = Deserialize530216075(obj as GhostPickupable, reader);
				ProtoReader.EndSubItem(token6, reader);
				goto IL_0396;
			}
			case 11000:
			{
				SubItemToken token5 = ProtoReader.StartSubItem(reader);
				obj = Deserialize840818195(obj as WeldableWallPanelGeneric, reader);
				ProtoReader.EndSubItem(token5, reader);
				goto IL_0396;
			}
			case 12000:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1505210158(obj as PrecursorDoorKeyColumn, reader);
				ProtoReader.EndSubItem(token4, reader);
				goto IL_0396;
			}
			case 13000:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj = Deserialize2051895012(obj as PrecursorKeyTerminal, reader);
				ProtoReader.EndSubItem(token3, reader);
				goto IL_0396;
			}
			case 14000:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1713660373(obj as PrecursorTeleporterActivationTerminal, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0396;
			}
			case 15000:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1457139245(obj as PrecursorDisableGunTerminal, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0396;
			}
			}
			break;
			IL_0396:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			reader.SkipField();
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize646820922(Holefish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Holefish Deserialize646820922(Holefish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize328727906(Hoopfish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Hoopfish Deserialize328727906(Hoopfish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1859566378(Hoverfish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Hoverfish Deserialize1859566378(Hoverfish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1024693509(Incubator obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.powered, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hatched, writer);
	}

	private Incubator Deserialize1024693509(Incubator obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.powered = reader.ReadBoolean();
				break;
			case 3:
				obj.hatched = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2004070125(InfectedMixin obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.infectedAmount, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private InfectedMixin Deserialize2004070125(InfectedMixin obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.infectedAmount = reader.ReadSingle();
				break;
			case 2:
				obj.version = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1691070576(Int3 obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.z, writer);
	}

	private Int3 Deserialize1691070576(Int3 obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadInt32();
				break;
			case 2:
				obj.y = reader.ReadInt32();
				break;
			case 3:
				obj.z = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1164389947(Int3.Bounds obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.mins, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.maxs, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
	}

	private Int3.Bounds Deserialize1164389947(Int3.Bounds obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.mins = Deserialize1691070576(obj.mins, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.maxs = Deserialize1691070576(obj.maxs, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize289177516(Int3Class obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.z, writer);
	}

	private Int3Class Deserialize289177516(Int3Class obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new Int3Class();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadInt32();
				break;
			case 2:
				obj.y = reader.ReadInt32();
				break;
			case 3:
				obj.z = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize51037994(Inventory obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.serializedStorage != null)
		{
			byte[] serializedStorage = obj.serializedStorage;
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedStorage, writer);
		}
		if (obj.serializedQuickSlots != null)
		{
			string[] serializedQuickSlots = obj.serializedQuickSlots;
			foreach (string text in serializedQuickSlots)
			{
				if (text != null)
				{
					ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
					ProtoWriter.WriteString(text, writer);
				}
			}
		}
		if (obj.serializedEquipment != null)
		{
			byte[] serializedEquipment = obj.serializedEquipment;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedEquipment, writer);
		}
		if (obj.serializedEquipmentSlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj.serializedEquipmentSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		if (obj.serializedPendingItems != null)
		{
			byte[] serializedPendingItems = obj.serializedPendingItems;
			ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedPendingItems, writer);
		}
	}

	private Inventory Deserialize51037994(Inventory obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.serializedStorage = ProtoReader.AppendBytes(obj.serializedStorage, reader);
				break;
			case 3:
			{
				List<string> list = new List<string>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.serializedQuickSlots = list.ToArray();
				break;
			}
			case 4:
				obj.serializedEquipment = ProtoReader.AppendBytes(obj.serializedEquipment, reader);
				break;
			case 5:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.serializedEquipmentSlots = dictionary;
				break;
			}
			case 7:
				obj.serializedPendingItems = ProtoReader.AppendBytes(obj.serializedPendingItems, reader);
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1338410882(Jellyray obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Jellyray Deserialize1338410882(Jellyray obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1430634702(JointHelper obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.connectedObjectUid != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.connectedObjectUid, writer);
		}
		if (obj.jointType != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.jointType, writer);
		}
	}

	private JointHelper Deserialize1430634702(JointHelper obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.connectedObjectUid = reader.ReadString();
				break;
			case 2:
				obj.jointType = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1252700319(Jumper obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Jumper Deserialize1252700319(Jumper obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1032585975(KeypadDoorConsole obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.unlocked, writer);
	}

	private KeypadDoorConsole Deserialize1032585975(KeypadDoorConsole obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.unlocked = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1059166753(KeypadDoorConsoleUnlock obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.unlocked, writer);
	}

	private KeypadDoorConsoleUnlock Deserialize1059166753(KeypadDoorConsoleUnlock obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.unlocked = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1793440205(LargeRoomWaterPark obj, int objTypeId, ProtoWriter writer)
	{
	}

	private LargeRoomWaterPark Deserialize1793440205(LargeRoomWaterPark obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1878920823(LargeWorldBatchRoot obj, int objTypeId, ProtoWriter writer)
	{
		obj.OnBeforeSerialization();
		if (obj.overrideBiome != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.overrideBiome, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.fogColor, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fogStartDistance, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fogMaxDistance, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fadeDefaultLights, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fadeRate, writer);
		if (obj.fog != null)
		{
			ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.fog, writer);
			Serialize108625815(obj.fog, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
		if (obj.sun != null)
		{
			ProtoWriter.WriteFieldHeader(9, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(obj.sun, writer);
			Serialize1603999327(obj.sun, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
		}
		if (obj.amb != null)
		{
			ProtoWriter.WriteFieldHeader(10, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(obj.amb, writer);
			Serialize1804294731(obj.amb, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
		}
		ProtoWriter.WriteFieldHeader(11, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.atmospherePrefabClassId != null)
		{
			ProtoWriter.WriteFieldHeader(12, WireType.String, writer);
			ProtoWriter.WriteString(obj.atmospherePrefabClassId, writer);
		}
	}

	private LargeWorldBatchRoot Deserialize1878920823(LargeWorldBatchRoot obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 2:
				obj.overrideBiome = reader.ReadString();
				break;
			case 3:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj.fogColor = Deserialize1404661584(obj.fogColor, reader);
				ProtoReader.EndSubItem(token4, reader);
				break;
			}
			case 4:
				obj.fogStartDistance = reader.ReadSingle();
				break;
			case 5:
				obj.fogMaxDistance = reader.ReadSingle();
				break;
			case 6:
				obj.fadeDefaultLights = reader.ReadSingle();
				break;
			case 7:
				obj.fadeRate = reader.ReadSingle();
				break;
			case 8:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.fog = Deserialize108625815(obj.fog, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 9:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.sun = Deserialize1603999327(obj.sun, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 10:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.amb = Deserialize1804294731(obj.amb, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 11:
				obj.version = reader.ReadInt32();
				break;
			case 12:
				obj.atmospherePrefabClassId = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		obj.OnAfterDeserialization();
		return obj;
	}

	private void Serialize577496260(LargeWorldEntity obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.cellLevel, writer);
	}

	private LargeWorldEntity Deserialize577496260(LargeWorldEntity obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 2)
			{
				obj.cellLevel = (LargeWorldEntity.CellLevel)reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize122249312(LargeWorldEntityCell obj, int objTypeId, ProtoWriter writer)
	{
	}

	private LargeWorldEntityCell Deserialize122249312(LargeWorldEntityCell obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize769725772(LaserCutObject obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isCutOpen, writer);
	}

	private LaserCutObject Deserialize769725772(LaserCutObject obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.isCutOpen = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize353579366(LavaLarva obj, int objTypeId, ProtoWriter writer)
	{
	}

	private LavaLarva Deserialize353579366(LavaLarva obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1882829928(LavaLizard obj, int objTypeId, ProtoWriter writer)
	{
	}

	private LavaLizard Deserialize1882829928(LavaLizard obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1703248490(LavaShell obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.armorPoints, writer);
	}

	private LavaShell Deserialize1703248490(LavaShell obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.armorPoints = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize656832482(LeakingRadiation obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.currentRadius, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.radiationFixed, writer);
	}

	private LeakingRadiation Deserialize656832482(LeakingRadiation obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.currentRadius = reader.ReadSingle();
				break;
			case 3:
				obj.radiationFixed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize200911463(LEDLight obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.deployed, writer);
	}

	private LEDLight Deserialize200911463(LEDLight obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.deployed = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1332964068(Leviathan obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Leviathan Deserialize1332964068(Leviathan obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize729882159(LiveMixin obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.health, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.tempDamage, writer);
	}

	private LiveMixin Deserialize729882159(LiveMixin obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.health = reader.ReadSingle();
				break;
			case 3:
				obj.tempDamage = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1855998104(MapRoomCamera obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.cameraNumber, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.lightState, writer);
	}

	private MapRoomCamera Deserialize1855998104(MapRoomCamera obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.cameraNumber = reader.ReadInt32();
				break;
			case 3:
				obj.lightState = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1304741297(MapRoomCameraDocking obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.cameraDocked, writer);
	}

	private MapRoomCameraDocking Deserialize1304741297(MapRoomCameraDocking obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.cameraDocked = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize503490010(MapRoomFunctionality obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.numNodesScanned, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.typeToScan, writer);
	}

	private MapRoomFunctionality Deserialize503490010(MapRoomFunctionality obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.numNodesScanned = reader.ReadInt32();
				break;
			case 3:
				obj.typeToScan = (TechType)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1343493277(MapRoomScreen obj, int objTypeId, ProtoWriter writer)
	{
	}

	private MapRoomScreen Deserialize1343493277(MapRoomScreen obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize670501331(MedicalCabinet obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasMedKit, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeSpawnMedKit, writer);
	}

	private MedicalCabinet Deserialize670501331(MedicalCabinet obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.hasMedKit = reader.ReadBoolean();
				break;
			case 2:
				obj.timeSpawnMedKit = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize123477383(Mesmer obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Mesmer Deserialize123477383(Mesmer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1761034218(NitrogenLevel obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.nitrogenLevel, writer);
	}

	private NitrogenLevel Deserialize1761034218(NitrogenLevel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.nitrogenLevel = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1874975849(NotificationManager.NotificationData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.duration, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeLeft, writer);
	}

	private NotificationManager.NotificationData Deserialize1874975849(NotificationManager.NotificationData obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new NotificationManager.NotificationData();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.duration = reader.ReadSingle();
				break;
			case 2:
				obj.timeLeft = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1731663660(NotificationManager.NotificationId obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.group, writer);
		if (obj.key != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.key, writer);
		}
	}

	private NotificationManager.NotificationId Deserialize1731663660(NotificationManager.NotificationId obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.group = (NotificationManager.Group)reader.ReadInt32();
				break;
			case 2:
				obj.key = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize348559960(NotificationManager.SerializedData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.protoVersion, writer);
		if (obj.notifications != null)
		{
			Dictionary<NotificationManager.NotificationId, NotificationManager.NotificationData>.Enumerator enumerator = obj.notifications.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1852174394(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private NotificationManager.SerializedData Deserialize348559960(NotificationManager.SerializedData obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new NotificationManager.SerializedData();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.protoVersion = reader.ReadInt32();
				break;
			case 2:
			{
				Dictionary<NotificationManager.NotificationId, NotificationManager.NotificationData> dictionary = obj.notifications ?? new Dictionary<NotificationManager.NotificationId, NotificationManager.NotificationData>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData> obj2 = default(KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1852174394(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize802993602(NuclearReactor obj, int objTypeId, ProtoWriter writer)
	{
	}

	private NuclearReactor Deserialize802993602(NuclearReactor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1829844631(OculusFish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private OculusFish Deserialize1829844631(OculusFish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize190681690(Oxygen obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.oxygenAvailable, writer);
	}

	private Oxygen Deserialize190681690(Oxygen obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.oxygenAvailable = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize569028242(OxygenPipe obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.parentPipeUID != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.parentPipeUID, writer);
		}
		if (obj.rootPipeUID != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.rootPipeUID, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.parentPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.childPipeUID == null)
		{
			return;
		}
		string[] childPipeUID = obj.childPipeUID;
		foreach (string text in childPipeUID)
		{
			if (text != null)
			{
				ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
				ProtoWriter.WriteString(text, writer);
			}
		}
	}

	private OxygenPipe Deserialize569028242(OxygenPipe obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.parentPipeUID = reader.ReadString();
				break;
			case 2:
				obj.rootPipeUID = reader.ReadString();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.parentPosition = Deserialize1181346079(obj.parentPosition, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.version = reader.ReadInt32();
				break;
			case 5:
			{
				List<string> list = new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.childPipeUID = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1244614203(PDAEncyclopedia.Entry obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timestamp, writer);
		if (obj.timeCapsuleId != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.timeCapsuleId, writer);
		}
	}

	private PDAEncyclopedia.Entry Deserialize1244614203(PDAEncyclopedia.Entry obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new PDAEncyclopedia.Entry();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.timestamp = reader.ReadSingle();
				break;
			case 2:
				obj.timeCapsuleId = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1329426611(PDALog.Entry obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timestamp, writer);
	}

	private PDALog.Entry Deserialize1329426611(PDALog.Entry obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new PDALog.Entry();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			if (num == 1)
			{
				obj.timestamp = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize2137760429(PDAScanner.Data obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.fragments != null)
		{
			Dictionary<string, float>.Enumerator enumerator = obj.fragments.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, float> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize714689774(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		if (obj.partial != null)
		{
			List<PDAScanner.Entry>.Enumerator enumerator2 = obj.partial.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PDAScanner.Entry current2 = enumerator2.Current;
				if (current2 != null)
				{
					ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
					SubItemToken token2 = ProtoWriter.StartSubItem(current2, writer);
					Serialize248259089(current2, objTypeId, writer);
					ProtoWriter.EndSubItem(token2, writer);
				}
			}
		}
		if (obj.complete != null)
		{
			HashSet<TechType>.Enumerator enumerator3 = obj.complete.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				TechType current3 = enumerator3.Current;
				ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current3, writer);
			}
		}
	}

	private PDAScanner.Data Deserialize2137760429(PDAScanner.Data obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new PDAScanner.Data();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				Dictionary<string, float> dictionary = new Dictionary<string, float>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, float> obj2 = default(KeyValuePair<string, float>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize714689774(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.fragments = dictionary;
				break;
			}
			case 3:
			{
				List<PDAScanner.Entry> list = new List<PDAScanner.Entry>();
				int fieldNumber3 = reader.FieldNumber;
				do
				{
					PDAScanner.Entry obj3 = new PDAScanner.Entry();
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize248259089(obj3, reader);
					ProtoReader.EndSubItem(token2, reader);
					list.Add(obj3);
				}
				while (reader.TryReadFieldHeader(fieldNumber3));
				obj.partial = list;
				break;
			}
			case 4:
			{
				HashSet<TechType> hashSet = new HashSet<TechType>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					TechType techType = TechType.None;
					techType = (TechType)reader.ReadInt32();
					hashSet.Add(techType);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.complete = hashSet;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize248259089(PDAScanner.Entry obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.techType, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.progress, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.unlocked, writer);
	}

	private PDAScanner.Entry Deserialize248259089(PDAScanner.Entry obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new PDAScanner.Entry();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.techType = (TechType)reader.ReadInt32();
				break;
			case 2:
				obj.progress = reader.ReadSingle();
				break;
			case 3:
				obj.unlocked = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1124891617(Peeper obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.enzymeAmount, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeRechargeEnzyme, writer);
	}

	private Peeper Deserialize1124891617(Peeper obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.enzymeAmount = reader.ReadSingle();
				break;
			case 3:
				obj.timeRechargeEnzyme = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1289092887(PickPrefab obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.pickedState, writer);
	}

	private PickPrefab Deserialize1289092887(PickPrefab obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.pickedState = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize723629918(Pickupable obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 730995178)
		{
			ProtoWriter.WriteFieldHeader(1010, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize730995178(obj as CreepvineSeed, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.overrideTechType, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.overrideTechUsed, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isLootCube, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isPickupable, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.destroyOnDeath, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._attached, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._isInSub, writer);
		ProtoWriter.WriteFieldHeader(8, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(9, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.isKinematic, writer);
	}

	private Pickupable Deserialize723629918(Pickupable obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 1010)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize730995178(obj as CreepvineSeed, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.overrideTechType = (TechType)reader.ReadInt32();
				break;
			case 2:
				obj.overrideTechUsed = reader.ReadBoolean();
				break;
			case 3:
				obj.isLootCube = reader.ReadBoolean();
				break;
			case 4:
				obj.isPickupable = reader.ReadBoolean();
				break;
			case 5:
				obj.destroyOnDeath = reader.ReadBoolean();
				break;
			case 6:
				obj._attached = reader.ReadBoolean();
				break;
			case 7:
				obj._isInSub = reader.ReadBoolean();
				break;
			case 8:
				obj.version = reader.ReadInt32();
				break;
			case 9:
				obj.isKinematic = (PickupableKinematicState)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1527139359(PictureFrame obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.fileName != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.fileName, writer);
		}
	}

	private PictureFrame Deserialize1527139359(PictureFrame obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.fileName = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize205484837(PingInstance obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.currentVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.visible, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.colorIndex, writer);
		if (obj._id != null)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteString(obj._id, writer);
		}
	}

	private PingInstance Deserialize205484837(PingInstance obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.currentVersion = reader.ReadInt32();
				break;
			case 2:
				obj.visible = reader.ReadBoolean();
				break;
			case 3:
				obj.colorIndex = reader.ReadInt32();
				break;
			case 4:
				obj._id = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize837446894(Pipe obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.parentPipeUID != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.parentPipeUID, writer);
		}
	}

	private Pipe Deserialize837446894(Pipe obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.parentPipeUID = reader.ReadString();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize955443574(PipeSurfaceFloater obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.childPipeUID != null)
		{
			string[] childPipeUID = obj.childPipeUID;
			foreach (string text in childPipeUID)
			{
				if (text != null)
				{
					ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
					ProtoWriter.WriteString(text, writer);
				}
			}
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.deployed, writer);
	}

	private PipeSurfaceFloater Deserialize955443574(PipeSurfaceFloater obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				List<string> list = new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.childPipeUID = list.ToArray();
				break;
			}
			case 2:
				obj.deployed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize542223321(Plantable obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.planterSlotId, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.plantAge, writer);
	}

	private Plantable Deserialize542223321(Plantable obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.planterSlotId = reader.ReadInt32();
				break;
			case 3:
				obj.plantAge = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1875862075(Player obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.serializedIsUnderwater, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.serializedDepthClass, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.serializedEscapePod, writer);
		if (obj.knownTech != null)
		{
			List<TechType>.Enumerator enumerator = obj.knownTech.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TechType current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current, writer);
			}
		}
		if (obj.currentSubUID != null)
		{
			ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
			ProtoWriter.WriteString(obj.currentSubUID, writer);
		}
		if (obj.journal != null)
		{
			Dictionary<string, PDALog.Entry>.Enumerator enumerator2 = obj.journal.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, PDALog.Entry> current2 = enumerator2.Current;
				ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1062675616(current2, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		if (obj.encyclopedia != null)
		{
			Dictionary<string, PDAEncyclopedia.Entry>.Enumerator enumerator3 = obj.encyclopedia.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				KeyValuePair<string, PDAEncyclopedia.Entry> current3 = enumerator3.Current;
				ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize121008834(current3, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
		if (obj.scanner != null)
		{
			ProtoWriter.WriteFieldHeader(9, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(obj.scanner, writer);
			Serialize2137760429(obj.scanner, objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
		}
		if (obj.currentWaterParkUID != null)
		{
			ProtoWriter.WriteFieldHeader(10, WireType.String, writer);
			ProtoWriter.WriteString(obj.currentWaterParkUID, writer);
		}
		if (obj.usedTools != null)
		{
			HashSet<TechType>.Enumerator enumerator4 = obj.usedTools.GetEnumerator();
			while (enumerator4.MoveNext())
			{
				TechType current4 = enumerator4.Current;
				ProtoWriter.WriteFieldHeader(11, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current4, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(12, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.precursorOutOfWater, writer);
		if (obj.analyzedTech != null)
		{
			HashSet<TechType>.Enumerator enumerator5 = obj.analyzedTech.GetEnumerator();
			while (enumerator5.MoveNext())
			{
				TechType current5 = enumerator5.Current;
				ProtoWriter.WriteFieldHeader(13, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current5, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(14, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isSick, writer);
		if (obj.notifications != null)
		{
			ProtoWriter.WriteFieldHeader(15, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(obj.notifications, writer);
			Serialize348559960(obj.notifications, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
		}
		ProtoWriter.WriteFieldHeader(16, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._displaySurfaceWater, writer);
		ProtoWriter.WriteFieldHeader(17, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeLastSleep, writer);
		ProtoWriter.WriteFieldHeader(18, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.infectionRevealed, writer);
		if (obj.timeCapsules != null)
		{
			Dictionary<string, TimeCapsuleContent>.Enumerator enumerator6 = obj.timeCapsules.GetEnumerator();
			while (enumerator6.MoveNext())
			{
				KeyValuePair<string, TimeCapsuleContent> current6 = enumerator6.Current;
				ProtoWriter.WriteFieldHeader(19, WireType.String, writer);
				SubItemToken token5 = ProtoWriter.StartSubItem(null, writer);
				Serialize1249808124(current6, objTypeId, writer);
				ProtoWriter.EndSubItem(token5, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(20, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasUsedConsole, writer);
		ProtoWriter.WriteFieldHeader(21, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.rotationX, writer);
		ProtoWriter.WriteFieldHeader(22, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.rotationY, writer);
		ProtoWriter.WriteFieldHeader(23, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.suffocationState, writer);
		ProtoWriter.WriteFieldHeader(24, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.suffocationProgress, writer);
		if (obj.lastValidSubUID != null)
		{
			ProtoWriter.WriteFieldHeader(25, WireType.String, writer);
			ProtoWriter.WriteString(obj.lastValidSubUID, writer);
		}
		if (obj.pins != null)
		{
			List<TechType>.Enumerator enumerator7 = obj.pins.GetEnumerator();
			while (enumerator7.MoveNext())
			{
				TechType current7 = enumerator7.Current;
				ProtoWriter.WriteFieldHeader(26, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current7, writer);
			}
		}
	}

	private Player Deserialize1875862075(Player obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.serializedIsUnderwater = reader.ReadBoolean();
				break;
			case 3:
				obj.serializedDepthClass = reader.ReadInt32();
				break;
			case 4:
				obj.serializedEscapePod = reader.ReadBoolean();
				break;
			case 5:
			{
				List<TechType> list2 = new List<TechType>();
				int fieldNumber7 = reader.FieldNumber;
				do
				{
					TechType techType4 = TechType.None;
					techType4 = (TechType)reader.ReadInt32();
					list2.Add(techType4);
				}
				while (reader.TryReadFieldHeader(fieldNumber7));
				obj.knownTech = list2;
				break;
			}
			case 6:
				obj.currentSubUID = reader.ReadString();
				break;
			case 7:
			{
				Dictionary<string, PDALog.Entry> dictionary3 = new Dictionary<string, PDALog.Entry>();
				int fieldNumber5 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, PDALog.Entry> obj4 = default(KeyValuePair<string, PDALog.Entry>);
					SubItemToken token5 = ProtoReader.StartSubItem(reader);
					obj4 = Deserialize1062675616(obj4, reader);
					ProtoReader.EndSubItem(token5, reader);
					dictionary3[obj4.Key] = obj4.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber5));
				obj.journal = dictionary3;
				break;
			}
			case 8:
			{
				Dictionary<string, PDAEncyclopedia.Entry> dictionary = new Dictionary<string, PDAEncyclopedia.Entry>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, PDAEncyclopedia.Entry> obj2 = default(KeyValuePair<string, PDAEncyclopedia.Entry>);
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize121008834(obj2, reader);
					ProtoReader.EndSubItem(token2, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.encyclopedia = dictionary;
				break;
			}
			case 9:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.scanner = Deserialize2137760429(obj.scanner, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 10:
				obj.currentWaterParkUID = reader.ReadString();
				break;
			case 11:
			{
				HashSet<TechType> hashSet2 = obj.usedTools ?? new HashSet<TechType>();
				int fieldNumber6 = reader.FieldNumber;
				do
				{
					TechType techType3 = TechType.None;
					techType3 = (TechType)reader.ReadInt32();
					hashSet2.Add(techType3);
				}
				while (reader.TryReadFieldHeader(fieldNumber6));
				break;
			}
			case 12:
				obj.precursorOutOfWater = reader.ReadBoolean();
				break;
			case 13:
			{
				HashSet<TechType> hashSet = new HashSet<TechType>();
				int fieldNumber4 = reader.FieldNumber;
				do
				{
					TechType techType2 = TechType.None;
					techType2 = (TechType)reader.ReadInt32();
					hashSet.Add(techType2);
				}
				while (reader.TryReadFieldHeader(fieldNumber4));
				obj.analyzedTech = hashSet;
				break;
			}
			case 14:
				obj.isSick = reader.ReadBoolean();
				break;
			case 15:
			{
				SubItemToken token4 = ProtoReader.StartSubItem(reader);
				obj.notifications = Deserialize348559960(obj.notifications, reader);
				ProtoReader.EndSubItem(token4, reader);
				break;
			}
			case 16:
				obj._displaySurfaceWater = reader.ReadBoolean();
				break;
			case 17:
				obj.timeLastSleep = reader.ReadSingle();
				break;
			case 18:
				obj.infectionRevealed = reader.ReadBoolean();
				break;
			case 19:
			{
				Dictionary<string, TimeCapsuleContent> dictionary2 = new Dictionary<string, TimeCapsuleContent>();
				int fieldNumber3 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, TimeCapsuleContent> obj3 = default(KeyValuePair<string, TimeCapsuleContent>);
					SubItemToken token3 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize1249808124(obj3, reader);
					ProtoReader.EndSubItem(token3, reader);
					dictionary2[obj3.Key] = obj3.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber3));
				obj.timeCapsules = dictionary2;
				break;
			}
			case 20:
				obj.hasUsedConsole = reader.ReadBoolean();
				break;
			case 21:
				obj.rotationX = reader.ReadSingle();
				break;
			case 22:
				obj.rotationY = reader.ReadSingle();
				break;
			case 23:
				obj.suffocationState = (Player.SuffocationState)reader.ReadInt32();
				break;
			case 24:
				obj.suffocationProgress = reader.ReadSingle();
				break;
			case 25:
				obj.lastValidSubUID = reader.ReadString();
				break;
			case 26:
			{
				List<TechType> list = new List<TechType>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					TechType techType = TechType.None;
					techType = (TechType)reader.ReadInt32();
					list.Add(techType);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.pins = list;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize10406022(PlayerSoundTrigger obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.triggered, writer);
	}

	private PlayerSoundTrigger Deserialize10406022(PlayerSoundTrigger obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.triggered = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize407275749(PlayerTimeCapsule obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj._serializedVersion, writer);
		if (obj._text != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj._text, writer);
		}
		if (obj._title != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteString(obj._title, writer);
		}
		if (obj._image != null)
		{
			byte[] image = obj._image;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(image, writer);
		}
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._submit, writer);
		if (obj._openQueue == null)
		{
			return;
		}
		List<string>.Enumerator enumerator = obj._openQueue.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private PlayerTimeCapsule Deserialize407275749(PlayerTimeCapsule obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj._serializedVersion = reader.ReadInt32();
				break;
			case 2:
				obj._text = reader.ReadString();
				break;
			case 3:
				obj._title = reader.ReadString();
				break;
			case 4:
				obj._image = ProtoReader.AppendBytes(obj._image, reader);
				break;
			case 5:
				obj._submit = reader.ReadBoolean();
				break;
			case 6:
			{
				List<string> list = obj._openQueue ?? new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1675048343(PlayerWorldArrows obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.completedCustomGoals == null)
		{
			return;
		}
		HashSet<string>.Enumerator enumerator = obj.completedCustomGoals.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private PlayerWorldArrows Deserialize1675048343(PlayerWorldArrows obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				HashSet<string> hashSet = obj.completedCustomGoals ?? new HashSet<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					hashSet.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1386435207(PowerCellCharger obj, int objTypeId, ProtoWriter writer)
	{
	}

	private PowerCellCharger Deserialize1386435207(PowerCellCharger obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1800229096(PowerCrafter obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 1011566978:
		{
			ProtoWriter.WriteFieldHeader(5110, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize1011566978(obj as Bioreactor, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 802993602:
		{
			ProtoWriter.WriteFieldHeader(5120, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize802993602(obj as NuclearReactor, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
	}

	private PowerCrafter Deserialize1800229096(PowerCrafter obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 5110:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize1011566978(obj as Bioreactor, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0051;
			}
			case 5120:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize802993602(obj as NuclearReactor, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0051;
			}
			}
			break;
			IL_0051:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			reader.SkipField();
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize797173896(PowerGenerator obj, int objTypeId, ProtoWriter writer)
	{
	}

	private PowerGenerator Deserialize797173896(PowerGenerator obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1522229446(PowerSource obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.power, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.maxPower, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private PowerSource Deserialize1522229446(PowerSource obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.power = reader.ReadSingle();
				break;
			case 2:
				obj.maxPower = reader.ReadSingle();
				break;
			case 3:
				obj.version = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize356984973(PrecursorAquariumPlatformTrigger obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.triggered, writer);
	}

	private PrecursorAquariumPlatformTrigger Deserialize356984973(PrecursorAquariumPlatformTrigger obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.triggered = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1541346828(PrecursorComputerTerminal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.used, writer);
	}

	private PrecursorComputerTerminal Deserialize1541346828(PrecursorComputerTerminal obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.used = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1457139245(PrecursorDisableGunTerminal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.firstUse, writer);
	}

	private PrecursorDisableGunTerminal Deserialize1457139245(PrecursorDisableGunTerminal obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 2)
			{
				obj.firstUse = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1505210158(PrecursorDoorKeyColumn obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.unlocked, writer);
	}

	private PrecursorDoorKeyColumn Deserialize1505210158(PrecursorDoorKeyColumn obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.unlocked = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize185046565(PrecursorElevator obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.elevatorPointIndex, writer);
	}

	private PrecursorElevator Deserialize185046565(PrecursorElevator obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.elevatorPointIndex = reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize285680569(PrecursorGunStoryEvents obj, int objTypeId, ProtoWriter writer)
	{
	}

	private PrecursorGunStoryEvents Deserialize285680569(PrecursorGunStoryEvents obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize2051895012(PrecursorKeyTerminal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.slotted, writer);
	}

	private PrecursorKeyTerminal Deserialize2051895012(PrecursorKeyTerminal obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.slotted = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize496960061(PrecursorPrisonVent obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.numStoredPeepers, writer);
	}

	private PrecursorPrisonVent Deserialize496960061(PrecursorPrisonVent obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.numStoredPeepers = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1169611549(PrecursorSurfaceVent obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeNextEmit, writer);
	}

	private PrecursorSurfaceVent Deserialize1169611549(PrecursorSurfaceVent obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeNextEmit = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1340645883(PrecursorTeleporter obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isOpen, writer);
	}

	private PrecursorTeleporter Deserialize1340645883(PrecursorTeleporter obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.isOpen = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1713660373(PrecursorTeleporterActivationTerminal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.unlocked, writer);
	}

	private PrecursorTeleporterActivationTerminal Deserialize1713660373(PrecursorTeleporterActivationTerminal obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.unlocked = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2111234577(PrefabPlaceholdersGroup obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isInitialized, writer);
	}

	private PrefabPlaceholdersGroup Deserialize2111234577(PrefabPlaceholdersGroup obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.isInitialized = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize851981872(PrisonManager obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.numCreatures, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.exitPoint, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.babiesHatched, writer);
	}

	private PrisonManager Deserialize851981872(PrisonManager obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.numCreatures = reader.ReadInt32();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.exitPoint = Deserialize1181346079(obj.exitPoint, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.babiesHatched = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1677184373(PropulseCannonAmmoHandler obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.addedDamageOnImpact, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.behaviorWasEnabled, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.wasShot, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeShot, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.locomotionWasEnabled, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.velocity, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private PropulseCannonAmmoHandler Deserialize1677184373(PropulseCannonAmmoHandler obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.addedDamageOnImpact = reader.ReadBoolean();
				break;
			case 3:
				obj.behaviorWasEnabled = reader.ReadBoolean();
				break;
			case 4:
				obj.wasShot = reader.ReadBoolean();
				break;
			case 5:
				obj.timeShot = reader.ReadSingle();
				break;
			case 6:
				obj.locomotionWasEnabled = reader.ReadBoolean();
				break;
			case 7:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.velocity = Deserialize1181346079(obj.velocity, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize313352173(ProtobufSerializer.ComponentHeader obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.TypeName != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.TypeName, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.IsEnabled, writer);
	}

	private ProtobufSerializer.ComponentHeader Deserialize313352173(ProtobufSerializer.ComponentHeader obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ProtobufSerializer.ComponentHeader();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.TypeName = reader.ReadString();
				break;
			case 2:
				obj.IsEnabled = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1159039820(ProtobufSerializer.GameObjectData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.CreateEmptyObject, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.IsActive, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Layer, writer);
		if (obj.Tag != null)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteString(obj.Tag, writer);
		}
		if (obj.Id != null)
		{
			ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
			ProtoWriter.WriteString(obj.Id, writer);
		}
		if (obj.ClassId != null)
		{
			ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
			ProtoWriter.WriteString(obj.ClassId, writer);
		}
		if (obj.Parent != null)
		{
			ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
			ProtoWriter.WriteString(obj.Parent, writer);
		}
		ProtoWriter.WriteFieldHeader(9, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.OverridePrefab, writer);
		ProtoWriter.WriteFieldHeader(10, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.MergeObject, writer);
	}

	private ProtobufSerializer.GameObjectData Deserialize1159039820(ProtobufSerializer.GameObjectData obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ProtobufSerializer.GameObjectData();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.CreateEmptyObject = reader.ReadBoolean();
				break;
			case 2:
				obj.IsActive = reader.ReadBoolean();
				break;
			case 3:
				obj.Layer = reader.ReadInt32();
				break;
			case 4:
				obj.Tag = reader.ReadString();
				break;
			case 6:
				obj.Id = reader.ReadString();
				break;
			case 7:
				obj.ClassId = reader.ReadString();
				break;
			case 8:
				obj.Parent = reader.ReadString();
				break;
			case 9:
				obj.OverridePrefab = reader.ReadBoolean();
				break;
			case 10:
				obj.MergeObject = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize69304398(ProtobufSerializer.LoopHeader obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Count, writer);
	}

	private ProtobufSerializer.LoopHeader Deserialize69304398(ProtobufSerializer.LoopHeader obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ProtobufSerializer.LoopHeader();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			if (num == 1)
			{
				obj.Count = reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize217879716(ProtobufSerializer.StreamHeader obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Signature, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Version, writer);
	}

	private ProtobufSerializer.StreamHeader Deserialize217879716(ProtobufSerializer.StreamHeader obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ProtobufSerializer.StreamHeader();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.Signature = reader.ReadInt32();
				break;
			case 2:
				obj.Version = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize328142573(ProtobufSerializerCornerCases obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.ListOfClassInstances != null)
		{
			List<SceneObjectData>.Enumerator enumerator = obj.ListOfClassInstances.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SceneObjectData current = enumerator.Current;
				if (current != null)
				{
					ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
					SubItemToken token = ProtoWriter.StartSubItem(current, writer);
					Serialize1881457915(current, objTypeId, writer);
					ProtoWriter.EndSubItem(token, writer);
				}
			}
		}
		if (obj.DictionaryOfClassInstances != null)
		{
			Dictionary<int, SceneObjectData>.Enumerator enumerator2 = obj.DictionaryOfClassInstances.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<int, SceneObjectData> current2 = enumerator2.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize1116754719(current2, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
		if (obj.NullableStruct.HasValue)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
			Serialize1181346079(obj.NullableStruct.GetValueOrDefault(), objTypeId, writer);
			ProtoWriter.EndSubItem(token3, writer);
		}
		if (obj.NullableEnum.HasValue)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
			ProtoWriter.WriteInt32((int)obj.NullableEnum.GetValueOrDefault(), writer);
		}
		if (obj.FloatGrid != null)
		{
			ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
			SubItemToken token4 = ProtoWriter.StartSubItem(obj.FloatGrid, writer);
			Serialize1182280616(obj.FloatGrid, objTypeId, writer);
			ProtoWriter.EndSubItem(token4, writer);
		}
		if (obj.Vector3Grid != null)
		{
			ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
			SubItemToken token5 = ProtoWriter.StartSubItem(obj.Vector3Grid, writer);
			Serialize1765532648(obj.Vector3Grid, objTypeId, writer);
			ProtoWriter.EndSubItem(token5, writer);
		}
		if (obj.EmptyArray != null)
		{
			int[] emptyArray = obj.EmptyArray;
			foreach (int value in emptyArray)
			{
				ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
				ProtoWriter.WriteInt32(value, writer);
			}
		}
		if (obj.InitializedSet == null)
		{
			return;
		}
		HashSet<string>.Enumerator enumerator3 = obj.InitializedSet.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			string current3 = enumerator3.Current;
			if (current3 != null)
			{
				ProtoWriter.WriteFieldHeader(8, WireType.String, writer);
				ProtoWriter.WriteString(current3, writer);
			}
		}
	}

	private ProtobufSerializerCornerCases Deserialize328142573(ProtobufSerializerCornerCases obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ProtobufSerializerCornerCases();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				List<SceneObjectData> list2 = new List<SceneObjectData>();
				int fieldNumber4 = reader.FieldNumber;
				do
				{
					SceneObjectData obj3 = new SceneObjectData();
					SubItemToken token4 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize1881457915(obj3, reader);
					ProtoReader.EndSubItem(token4, reader);
					list2.Add(obj3);
				}
				while (reader.TryReadFieldHeader(fieldNumber4));
				obj.ListOfClassInstances = list2;
				break;
			}
			case 2:
			{
				Dictionary<int, SceneObjectData> dictionary = new Dictionary<int, SceneObjectData>();
				int fieldNumber3 = reader.FieldNumber;
				do
				{
					KeyValuePair<int, SceneObjectData> obj2 = default(KeyValuePair<int, SceneObjectData>);
					SubItemToken token3 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1116754719(obj2, reader);
					ProtoReader.EndSubItem(token3, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber3));
				obj.DictionaryOfClassInstances = dictionary;
				break;
			}
			case 3:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.NullableStruct = Deserialize1181346079(obj.NullableStruct.GetValueOrDefault(), reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 4:
				obj.NullableEnum = (CubemapFace)reader.ReadInt32();
				break;
			case 5:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.FloatGrid = Deserialize1182280616(obj.FloatGrid, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 6:
			{
				SubItemToken token5 = ProtoReader.StartSubItem(reader);
				obj.Vector3Grid = Deserialize1765532648(obj.Vector3Grid, reader);
				ProtoReader.EndSubItem(token5, reader);
				break;
			}
			case 7:
			{
				List<int> list = new List<int>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					int num2 = 0;
					num2 = reader.ReadInt32();
					list.Add(num2);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.EmptyArray = list.ToArray();
				break;
			}
			case 8:
			{
				HashSet<string> hashSet = obj.InitializedSet ?? new HashSet<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					hashSet.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1082285174(RabbitRay obj, int objTypeId, ProtoWriter writer)
	{
	}

	private RabbitRay Deserialize1082285174(RabbitRay obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1702762571(ReaperLeviathan obj, int objTypeId, ProtoWriter writer)
	{
	}

	private ReaperLeviathan Deserialize1702762571(ReaperLeviathan obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1651949009(Reefback obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Reefback Deserialize1651949009(Reefback obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize803494206(ReefbackCreature obj, int objTypeId, ProtoWriter writer)
	{
	}

	private ReefbackCreature Deserialize803494206(ReefbackCreature obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1077102969(ReefbackLife obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.initialized, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasCorals, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.grassIndex, writer);
	}

	private ReefbackLife Deserialize1077102969(ReefbackLife obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.initialized = reader.ReadBoolean();
				break;
			case 3:
				obj.hasCorals = reader.ReadBoolean();
				break;
			case 4:
				obj.grassIndex = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1948869956(ReefbackPlant obj, int objTypeId, ProtoWriter writer)
	{
	}

	private ReefbackPlant Deserialize1948869956(ReefbackPlant obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize924544622(Reginald obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Reginald Deserialize924544622(Reginald obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize859052613(ResourceTrackerDatabase obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.serializedVersion, writer);
		if (obj.savedResources != null)
		{
			List<ResourceTrackerDatabase.ResourceInfo>.Enumerator enumerator = obj.savedResources.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ResourceTrackerDatabase.ResourceInfo current = enumerator.Current;
				if (current != null)
				{
					ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
					SubItemToken token = ProtoWriter.StartSubItem(current, writer);
					Serialize161874211(current, objTypeId, writer);
					ProtoWriter.EndSubItem(token, writer);
				}
			}
		}
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.storedChangeSet, writer);
	}

	private ResourceTrackerDatabase Deserialize859052613(ResourceTrackerDatabase obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.serializedVersion = reader.ReadInt32();
				break;
			case 2:
			{
				List<ResourceTrackerDatabase.ResourceInfo> list = obj.savedResources ?? new List<ResourceTrackerDatabase.ResourceInfo>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					ResourceTrackerDatabase.ResourceInfo obj2 = new ResourceTrackerDatabase.ResourceInfo();
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize161874211(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			case 3:
				obj.storedChangeSet = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize161874211(ResourceTrackerDatabase.ResourceInfo obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.uniqueId != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.uniqueId, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.techType, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.position, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private ResourceTrackerDatabase.ResourceInfo Deserialize161874211(ResourceTrackerDatabase.ResourceInfo obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ResourceTrackerDatabase.ResourceInfo();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.uniqueId = reader.ReadString();
				break;
			case 2:
				obj.techType = (TechType)reader.ReadInt32();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.position = Deserialize1181346079(obj.position, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize11492366(Respawn obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.spawnTime, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.techType, writer);
		if (obj.addComponents == null)
		{
			return;
		}
		List<string>.Enumerator enumerator = obj.addComponents.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private Respawn Deserialize11492366(Respawn obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.spawnTime = reader.ReadSingle();
				break;
			case 3:
				obj.techType = (TechType)reader.ReadInt32();
				break;
			case 4:
			{
				List<string> list = obj.addComponents ?? new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize253947874(RestoreAnimatorState obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.stateNameHash, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.normalizedTime, writer);
		if (obj.parameterValues == null)
		{
			return;
		}
		List<AnimatorParameterValue>.Enumerator enumerator = obj.parameterValues.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AnimatorParameterValue current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(current, writer);
				Serialize880630407(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private RestoreAnimatorState Deserialize253947874(RestoreAnimatorState obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.stateNameHash = reader.ReadInt32();
				break;
			case 3:
				obj.normalizedTime = reader.ReadSingle();
				break;
			case 4:
			{
				List<AnimatorParameterValue> list = obj.parameterValues ?? new List<AnimatorParameterValue>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					AnimatorParameterValue obj2 = new AnimatorParameterValue();
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize880630407(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize273953122(RestoreDayNightCycle obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timePassed, writer);
	}

	private RestoreDayNightCycle Deserialize273953122(RestoreDayNightCycle obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.timePassed = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1092217753(RestoreEscapePodPosition obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.position, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private RestoreEscapePodPosition Deserialize1092217753(RestoreEscapePodPosition obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.position = Deserialize1181346079(obj.position, reader);
				ProtoReader.EndSubItem(token, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1755470585(RestoreEscapePodStorage obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.serialData != null)
		{
			byte[] serialData = obj.serialData;
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteBytes(serialData, writer);
		}
	}

	private RestoreEscapePodStorage Deserialize1755470585(RestoreEscapePodStorage obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.serialData = ProtoReader.AppendBytes(obj.serialData, reader);
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize2024062491(RestoreInventoryStorage obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.serialData != null)
		{
			byte[] serialData = obj.serialData;
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteBytes(serialData, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.food, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.water, writer);
		if (obj.completedCustomGoals == null)
		{
			return;
		}
		List<string>.Enumerator enumerator = obj.completedCustomGoals.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private RestoreInventoryStorage Deserialize2024062491(RestoreInventoryStorage obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.serialData = ProtoReader.AppendBytes(obj.serialData, reader);
				break;
			case 2:
				obj.food = reader.ReadSingle();
				break;
			case 3:
				obj.water = reader.ReadSingle();
				break;
			case 4:
			{
				List<string> list = new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.completedCustomGoals = list;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1273669126(Rocket obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.currentRocketStage, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.elevatorState, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.elevatorPosition, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.currentVersion, writer);
		if (obj.rocketName != null)
		{
			ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
			ProtoWriter.WriteString(obj.rocketName, writer);
		}
		if (obj.rocketColors != null)
		{
			Vector3[] rocketColors = obj.rocketColors;
			foreach (Vector3 obj2 in rocketColors)
			{
				ProtoWriter.WriteFieldHeader(6, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private Rocket Deserialize1273669126(Rocket obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.currentRocketStage = reader.ReadInt32();
				break;
			case 2:
				obj.elevatorState = (Rocket.RocketElevatorStates)reader.ReadInt32();
				break;
			case 3:
				obj.elevatorPosition = reader.ReadSingle();
				break;
			case 4:
				obj.currentVersion = reader.ReadInt32();
				break;
			case 5:
				obj.rocketName = reader.ReadString();
				break;
			case 6:
			{
				List<Vector3> list = new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.rocketColors = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1170326782(RocketConstructorInput obj, int objTypeId, ProtoWriter writer)
	{
	}

	private RocketConstructorInput Deserialize1170326782(RocketConstructorInput obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize916327342(RocketPreflightCheckManager obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.currentVersion, writer);
		if (obj.preflightChecks != null)
		{
			HashSet<PreflightCheck>.Enumerator enumerator = obj.preflightChecks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PreflightCheck current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
				ProtoWriter.WriteInt32((int)current, writer);
			}
		}
	}

	private RocketPreflightCheckManager Deserialize916327342(RocketPreflightCheckManager obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.currentVersion = reader.ReadInt32();
				break;
			case 2:
			{
				HashSet<PreflightCheck> hashSet = obj.preflightChecks ?? new HashSet<PreflightCheck>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					PreflightCheck preflightCheck = PreflightCheck.LifeSupport;
					preflightCheck = (PreflightCheck)reader.ReadInt32();
					hashSet.Add(preflightCheck);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize82819465(Roost obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Roost Deserialize82819465(Roost obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize81014147(SandShark obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SandShark Deserialize81014147(SandShark obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize846562516(SaveLoadManager.OptionsCache obj, int objTypeId, ProtoWriter writer)
	{
		if (obj._floats != null)
		{
			Dictionary<string, float>.Enumerator enumerator = obj._floats.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, float> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize714689774(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		if (obj._strings != null)
		{
			Dictionary<string, string>.Enumerator enumerator2 = obj._strings.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, string> current2 = enumerator2.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current2, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
		if (obj._bools != null)
		{
			Dictionary<string, bool>.Enumerator enumerator3 = obj._bools.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				KeyValuePair<string, bool> current3 = enumerator3.Current;
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
				Serialize1489031418(current3, objTypeId, writer);
				ProtoWriter.EndSubItem(token3, writer);
			}
		}
		if (obj._ints != null)
		{
			Dictionary<string, int>.Enumerator enumerator4 = obj._ints.GetEnumerator();
			while (enumerator4.MoveNext())
			{
				KeyValuePair<string, int> current4 = enumerator4.Current;
				ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
				SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
				Serialize224877162(current4, objTypeId, writer);
				ProtoWriter.EndSubItem(token4, writer);
			}
		}
	}

	private SaveLoadManager.OptionsCache Deserialize846562516(SaveLoadManager.OptionsCache obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new SaveLoadManager.OptionsCache();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				Dictionary<string, float> dictionary4 = obj._floats ?? new Dictionary<string, float>();
				int fieldNumber4 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, float> obj5 = default(KeyValuePair<string, float>);
					SubItemToken token4 = ProtoReader.StartSubItem(reader);
					obj5 = Deserialize714689774(obj5, reader);
					ProtoReader.EndSubItem(token4, reader);
					dictionary4[obj5.Key] = obj5.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber4));
				break;
			}
			case 2:
			{
				Dictionary<string, string> dictionary2 = obj._strings ?? new Dictionary<string, string>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj3 = default(KeyValuePair<string, string>);
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize524780017(obj3, reader);
					ProtoReader.EndSubItem(token2, reader);
					dictionary2[obj3.Key] = obj3.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				break;
			}
			case 3:
			{
				Dictionary<string, bool> dictionary3 = obj._bools ?? new Dictionary<string, bool>();
				int fieldNumber3 = reader.FieldNumber;
				do
				{
					KeyValuePair<string, bool> obj4 = default(KeyValuePair<string, bool>);
					SubItemToken token3 = ProtoReader.StartSubItem(reader);
					obj4 = Deserialize1489031418(obj4, reader);
					ProtoReader.EndSubItem(token3, reader);
					dictionary3[obj4.Key] = obj4.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber3));
				break;
			}
			case 4:
			{
				Dictionary<string, int> dictionary = obj._ints ?? new Dictionary<string, int>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, int> obj2 = default(KeyValuePair<string, int>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize224877162(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1881457915(SceneObjectData obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.uniqueName != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.uniqueName, writer);
		}
		if (obj.serialData != null)
		{
			byte[] serialData = obj.serialData;
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteBytes(serialData, writer);
		}
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isObjectTree, writer);
	}

	private SceneObjectData Deserialize1881457915(SceneObjectData obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new SceneObjectData();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.uniqueName = reader.ReadString();
				break;
			case 3:
				obj.serialData = ProtoReader.AppendBytes(obj.serialData, reader);
				break;
			case 4:
				obj.isObjectTree = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize2013353155(SceneObjectDataSet obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.items != null)
		{
			Dictionary<string, SceneObjectData>.Enumerator enumerator = obj.items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, SceneObjectData> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize890247896(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private SceneObjectDataSet Deserialize2013353155(SceneObjectDataSet obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new SceneObjectDataSet();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				Dictionary<string, SceneObjectData> dictionary = obj.items ?? new Dictionary<string, SceneObjectData>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, SceneObjectData> obj2 = default(KeyValuePair<string, SceneObjectData>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize890247896(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1606650114(SeaDragon obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SeaDragon Deserialize1606650114(SeaDragon obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1327055215(SeaEmperorBaby obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SeaEmperorBaby Deserialize1327055215(SeaEmperorBaby obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1921909083(SeaEmperorJuvenile obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SeaEmperorJuvenile Deserialize1921909083(SeaEmperorJuvenile obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1086395194(Sealed obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.openedAmount, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.maxOpenedAmount, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj._sealed, writer);
	}

	private Sealed Deserialize1086395194(Sealed obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.openedAmount = reader.ReadSingle();
				break;
			case 3:
				obj.maxOpenedAmount = reader.ReadSingle();
				break;
			case 4:
				obj._sealed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2106147891(SeaMoth obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SeaMoth Deserialize2106147891(SeaMoth obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1319564559(SeamothStorageContainer obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.serializedStorage != null)
		{
			byte[] serializedStorage = obj.serializedStorage;
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedStorage, writer);
		}
	}

	private SeamothStorageContainer Deserialize1319564559(SeamothStorageContainer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.serializedStorage = ProtoReader.AppendBytes(obj.serializedStorage, reader);
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize574222852(SeaTreader obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.treader_version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.grazingTimeLeft, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.reverseDirection, writer);
	}

	private SeaTreader Deserialize574222852(SeaTreader obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.treader_version = reader.ReadInt32();
				break;
			case 2:
				obj.grazingTimeLeft = reader.ReadSingle();
				break;
			case 3:
				obj.reverseDirection = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1535225367(Shocker obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Shocker Deserialize1535225367(Shocker obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1084867387(Sign obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.text != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.text, writer);
		}
		if (obj.elements != null)
		{
			bool[] elements = obj.elements;
			foreach (bool value in elements)
			{
				ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
				ProtoWriter.WriteBoolean(value, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.scaleIndex, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.colorIndex, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.backgroundEnabled, writer);
	}

	private Sign Deserialize1084867387(Sign obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.text = reader.ReadString();
				break;
			case 3:
			{
				List<bool> list = new List<bool>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					bool flag = false;
					flag = reader.ReadBoolean();
					list.Add(flag);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.elements = list.ToArray();
				break;
			}
			case 4:
				obj.scaleIndex = reader.ReadInt32();
				break;
			case 5:
				obj.colorIndex = reader.ReadInt32();
				break;
			case 6:
				obj.backgroundEnabled = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1261146858(Signal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.targetPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.targetDescription != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.targetDescription, writer);
		}
	}

	private Signal Deserialize1261146858(Signal obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.targetPosition = Deserialize1181346079(obj.targetPosition, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 2:
				obj.targetDescription = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1732754958(SignalPing obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.currentVersion, writer);
		if (obj.descriptionKey != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.descriptionKey, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.pos, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private SignalPing Deserialize1732754958(SignalPing obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.currentVersion = reader.ReadInt32();
				break;
			case 2:
				obj.descriptionKey = reader.ReadString();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.pos = Deserialize1181346079(obj.pos, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize469144263(Skyray obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Skyray Deserialize469144263(Skyray obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1355655442(Slime obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Slime Deserialize1355655442(Slime obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize524984727(SolarPanel obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.biomeSunlightScale, writer);
	}

	private SolarPanel Deserialize524984727(SolarPanel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.biomeSunlightScale = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize647827065(Spadefish obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Spadefish Deserialize647827065(Spadefish obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1993981848(SpawnStoredLoot obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.lootSpawned, writer);
	}

	private SpawnStoredLoot Deserialize1993981848(SpawnStoredLoot obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.lootSpawned = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1058623975(SpineEel obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SpineEel Deserialize1058623975(SpineEel obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1440414490(Stalker obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Stalker Deserialize1440414490(Stalker obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize737691900(StarshipDoor obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.doorOpen, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.doorLocked, writer);
	}

	private StarshipDoor Deserialize737691900(StarshipDoor obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.doorOpen = reader.ReadBoolean();
				break;
			case 2:
				obj.doorLocked = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize242125589(Stillsuit obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.waterCaptured, writer);
	}

	private Stillsuit Deserialize242125589(Stillsuit obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.waterCaptured = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize366077262(StorageContainer obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
	}

	private StorageContainer Deserialize366077262(StorageContainer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.version = reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize704466011(ScheduledGoal obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeExecute, writer);
		if (obj.goalKey != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteString(obj.goalKey, writer);
		}
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.goalType, writer);
	}

	private ScheduledGoal Deserialize704466011(ScheduledGoal obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new ScheduledGoal();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.timeExecute = reader.ReadSingle();
				break;
			case 3:
				obj.goalKey = reader.ReadString();
				break;
			case 4:
				obj.goalType = (Story.GoalType)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1982063882(StoryGoalManager obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.completedGoals != null)
		{
			HashSet<string>.Enumerator enumerator = obj.completedGoals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (current != null)
				{
					ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
					ProtoWriter.WriteString(current, writer);
				}
			}
		}
		if (obj.pendingRadioMessages == null)
		{
			return;
		}
		List<string>.Enumerator enumerator2 = obj.pendingRadioMessages.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			string current2 = enumerator2.Current;
			if (current2 != null)
			{
				ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
				ProtoWriter.WriteString(current2, writer);
			}
		}
	}

	private StoryGoalManager Deserialize1982063882(StoryGoalManager obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				HashSet<string> hashSet = obj.completedGoals ?? new HashSet<string>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					string text2 = null;
					text2 = reader.ReadString();
					hashSet.Add(text2);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				break;
			}
			case 4:
			{
				List<string> list = obj.pendingRadioMessages ?? new List<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					list.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1506278612(StoryGoalScheduler obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.schedule == null)
		{
			return;
		}
		List<ScheduledGoal>.Enumerator enumerator = obj.schedule.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ScheduledGoal current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(current, writer);
				Serialize704466011(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private StoryGoalScheduler Deserialize1506278612(StoryGoalScheduler obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				List<ScheduledGoal> list = obj.schedule ?? new List<ScheduledGoal>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					ScheduledGoal obj2 = new ScheduledGoal();
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize704466011(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize502247445(StoryGoalCustomEventHandler obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.countdownActive, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.countdownStartingTime, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.gunDisabled, writer);
	}

	private StoryGoalCustomEventHandler Deserialize502247445(StoryGoalCustomEventHandler obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.countdownActive = reader.ReadBoolean();
				break;
			case 2:
				obj.countdownStartingTime = reader.ReadSingle();
				break;
			case 3:
				obj.gunDisabled = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1537030892(SubFire obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.fireCount, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.curSmokeVal, writer);
	}

	private SubFire Deserialize1537030892(SubFire obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.fireCount = reader.ReadInt32();
				break;
			case 3:
				obj.curSmokeVal = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1180542256(SubRoot obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 311542795)
		{
			ProtoWriter.WriteFieldHeader(100, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize311542795(obj as BaseRoot, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.floodFraction, writer);
		if (obj.subName != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.subName, writer);
		}
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.subColor, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.subColors != null)
		{
			Vector3[] subColors = obj.subColors;
			foreach (Vector3 obj2 in subColors)
			{
				ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
				SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token3, writer);
			}
		}
	}

	private SubRoot Deserialize1180542256(SubRoot obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 100)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize311542795(obj as BaseRoot, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.floodFraction = reader.ReadSingle();
				break;
			case 2:
				obj.subName = reader.ReadString();
				break;
			case 3:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.subColor = Deserialize1181346079(obj.subColor, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 4:
				obj.version = reader.ReadInt32();
				break;
			case 5:
			{
				List<Vector3> list = new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token2, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.subColors = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1603999327(SunlightSettings obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.fade, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.color, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.replaceFraction, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.shadowed, writer);
	}

	private SunlightSettings Deserialize1603999327(SunlightSettings obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new SunlightSettings();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
				obj.fade = reader.ReadSingle();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.color = Deserialize1404661584(obj.color, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.replaceFraction = reader.ReadSingle();
				break;
			case 5:
				obj.shadowed = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1555952712(SupplyCrate obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.open, writer);
	}

	private SupplyCrate Deserialize1555952712(SupplyCrate obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.open = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize888605288(Survival obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.food, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.water, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.stomach, writer);
	}

	private Survival Deserialize888605288(Survival obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.food = reader.ReadSingle();
				break;
			case 3:
				obj.water = reader.ReadSingle();
				break;
			case 4:
				obj.stomach = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize225324449(SwimRandom obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SwimRandom Deserialize225324449(SwimRandom obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1157251956(SwimToMeat obj, int objTypeId, ProtoWriter writer)
	{
	}

	private SwimToMeat Deserialize1157251956(SwimToMeat obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize1852174394(KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData> obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1731663660(obj.Key, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1874975849(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
		}
	}

	private KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData> Deserialize1852174394(KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		NotificationManager.NotificationId notificationId = obj.Key;
		NotificationManager.NotificationData notificationData = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				notificationId = Deserialize1731663660(notificationId, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				notificationData = Deserialize1874975849(notificationData, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<NotificationManager.NotificationId, NotificationManager.NotificationData>(notificationId, notificationData);
		return obj;
	}

	private void Serialize1116754719(KeyValuePair<int, SceneObjectData> obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Key, writer);
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1881457915(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private KeyValuePair<int, SceneObjectData> Deserialize1116754719(KeyValuePair<int, SceneObjectData> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		int key = obj.Key;
		SceneObjectData sceneObjectData = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				sceneObjectData = Deserialize1881457915(sceneObjectData, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<int, SceneObjectData>(key, sceneObjectData);
		return obj;
	}

	private void Serialize121008834(KeyValuePair<string, PDAEncyclopedia.Entry> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1244614203(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private KeyValuePair<string, PDAEncyclopedia.Entry> Deserialize121008834(KeyValuePair<string, PDAEncyclopedia.Entry> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		PDAEncyclopedia.Entry entry = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				entry = Deserialize1244614203(entry, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, PDAEncyclopedia.Entry>(key, entry);
		return obj;
	}

	private void Serialize1062675616(KeyValuePair<string, PDALog.Entry> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1329426611(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private KeyValuePair<string, PDALog.Entry> Deserialize1062675616(KeyValuePair<string, PDALog.Entry> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		PDALog.Entry entry = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				entry = Deserialize1329426611(entry, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, PDALog.Entry>(key, entry);
		return obj;
	}

	private void Serialize890247896(KeyValuePair<string, SceneObjectData> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1881457915(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private KeyValuePair<string, SceneObjectData> Deserialize890247896(KeyValuePair<string, SceneObjectData> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		SceneObjectData sceneObjectData = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				sceneObjectData = Deserialize1881457915(sceneObjectData, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, SceneObjectData>(key, sceneObjectData);
		return obj;
	}

	private void Serialize1489031418(KeyValuePair<string, bool> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.Value, writer);
	}

	private KeyValuePair<string, bool> Deserialize1489031418(KeyValuePair<string, bool> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		bool value = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
				value = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, bool>(key, value);
		return obj;
	}

	private void Serialize224877162(KeyValuePair<string, int> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.Value, writer);
	}

	private KeyValuePair<string, int> Deserialize224877162(KeyValuePair<string, int> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		int value = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
				value = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, int>(key, value);
		return obj;
	}

	private void Serialize714689774(KeyValuePair<string, float> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.Value, writer);
	}

	private KeyValuePair<string, float> Deserialize714689774(KeyValuePair<string, float> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		float value = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
				value = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, float>(key, value);
		return obj;
	}

	private void Serialize524780017(KeyValuePair<string, string> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.Value, writer);
		}
	}

	private KeyValuePair<string, string> Deserialize524780017(KeyValuePair<string, string> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		string value = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
				value = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, string>(key, value);
		return obj;
	}

	private void Serialize1249808124(KeyValuePair<string, TimeCapsuleContent> obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.Key != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.Key, writer);
		}
		if (obj.Value != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(obj.Value, writer);
			Serialize1111417537(obj.Value, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
	}

	private KeyValuePair<string, TimeCapsuleContent> Deserialize1249808124(KeyValuePair<string, TimeCapsuleContent> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		string key = obj.Key;
		TimeCapsuleContent timeCapsuleContent = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = reader.ReadString();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				timeCapsuleContent = Deserialize1111417537(timeCapsuleContent, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<string, TimeCapsuleContent>(key, timeCapsuleContent);
		return obj;
	}

	private void Serialize1158933531(KeyValuePair<TechType, CraftingAnalytics.EntryData> obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.Key, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1103056798(obj.Value, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private KeyValuePair<TechType, CraftingAnalytics.EntryData> Deserialize1158933531(KeyValuePair<TechType, CraftingAnalytics.EntryData> obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		TechType key = obj.Key;
		CraftingAnalytics.EntryData entryData = obj.Value;
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				key = (TechType)reader.ReadInt32();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				entryData = Deserialize1103056798(entryData, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		obj = new KeyValuePair<TechType, CraftingAnalytics.EntryData>(key, entryData);
		return obj;
	}

	private void Serialize212928398(TechFragment obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.techTypeChosen, writer);
	}

	private TechFragment Deserialize212928398(TechFragment obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.techTypeChosen = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1671198874(TechLight obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.placedByPlayer, writer);
	}

	private TechLight Deserialize1671198874(TechLight obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.placedByPlayer = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2104816441(TeleporterManager obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.activeTeleporters == null)
		{
			return;
		}
		HashSet<string>.Enumerator enumerator = obj.activeTeleporters.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string current = enumerator.Current;
			if (current != null)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				ProtoWriter.WriteString(current, writer);
			}
		}
	}

	private TeleporterManager Deserialize2104816441(TeleporterManager obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				HashSet<string> hashSet = obj.activeTeleporters ?? new HashSet<string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					string text = null;
					text = reader.ReadString();
					hashSet.Add(text);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize645269343(Terraformer obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.ammo, writer);
	}

	private Terraformer Deserialize645269343(Terraformer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.ammo = reader.ReadInt32();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1983352738(ThermalPlant obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.temperature, writer);
	}

	private ThermalPlant Deserialize1983352738(ThermalPlant obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.temperature = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize746584541(TileInstance obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.resourcePath != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.resourcePath, writer);
		}
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.origin, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1691070576(obj.gridOffset, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.gridSize, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.turns, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.blendMode, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteByte(obj.multiplyPassiveType, writer);
		ProtoWriter.WriteFieldHeader(8, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.layer, writer);
		ProtoWriter.WriteFieldHeader(9, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.clearHeightmap, writer);
	}

	private TileInstance Deserialize746584541(TileInstance obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.resourcePath = reader.ReadString();
				break;
			case 2:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.origin = Deserialize1691070576(obj.origin, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.gridOffset = Deserialize1691070576(obj.gridOffset, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.gridSize = reader.ReadInt32();
				break;
			case 5:
				obj.turns = reader.ReadInt32();
				break;
			case 6:
				obj.blendMode = (VoxelBlendMode)reader.ReadInt32();
				break;
			case 7:
				obj.multiplyPassiveType = reader.ReadByte();
				break;
			case 8:
				obj.layer = reader.ReadInt32();
				break;
			case 9:
				obj.clearHeightmap = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1386031502(TimeCapsule obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.protoVersion, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.spawned, writer);
		if (obj.instanceId != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteString(obj.instanceId, writer);
		}
		if (obj.id != null)
		{
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteString(obj.id, writer);
		}
	}

	private TimeCapsule Deserialize1386031502(TimeCapsule obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.protoVersion = reader.ReadInt32();
				break;
			case 2:
				obj.spawned = reader.ReadBoolean();
				break;
			case 3:
				obj.instanceId = reader.ReadString();
				break;
			case 4:
				obj.id = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1111417537(TimeCapsuleContent obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.title != null)
		{
			ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
			ProtoWriter.WriteString(obj.title, writer);
		}
		if (obj.text != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.text, writer);
		}
		if (obj.imageUrl != null)
		{
			ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
			ProtoWriter.WriteString(obj.imageUrl, writer);
		}
		if (obj.items != null)
		{
			List<TimeCapsuleItem>.Enumerator enumerator = obj.items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TimeCapsuleItem current = enumerator.Current;
				if (current != null)
				{
					ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
					SubItemToken token = ProtoWriter.StartSubItem(current, writer);
					Serialize2044575597(current, objTypeId, writer);
					ProtoWriter.EndSubItem(token, writer);
				}
			}
		}
		if (obj.updatedAt != null)
		{
			ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
			ProtoWriter.WriteString(obj.updatedAt, writer);
		}
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isActive, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.copiesFound, writer);
	}

	private TimeCapsuleContent Deserialize1111417537(TimeCapsuleContent obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new TimeCapsuleContent();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.title = reader.ReadString();
				break;
			case 2:
				obj.text = reader.ReadString();
				break;
			case 3:
				obj.imageUrl = reader.ReadString();
				break;
			case 4:
			{
				List<TimeCapsuleItem> list = new List<TimeCapsuleItem>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					TimeCapsuleItem obj2 = new TimeCapsuleItem();
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize2044575597(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.items = list;
				break;
			}
			case 5:
				obj.updatedAt = reader.ReadString();
				break;
			case 6:
				obj.isActive = reader.ReadBoolean();
				break;
			case 7:
				obj.copiesFound = reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize2044575597(TimeCapsuleItem obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.techType, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.hasBattery, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.batteryType, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.batteryCharge, writer);
	}

	private TimeCapsuleItem Deserialize2044575597(TimeCapsuleItem obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new TimeCapsuleItem();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.techType = (TechType)reader.ReadInt32();
				break;
			case 2:
				obj.hasBattery = reader.ReadBoolean();
				break;
			case 3:
				obj.batteryType = (TechType)reader.ReadInt32();
				break;
			case 4:
				obj.batteryCharge = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1877364825(ToggleLights obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.lightsActive, writer);
	}

	private ToggleLights Deserialize1877364825(ToggleLights obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.lightsActive = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize118512508(AnimationCurve obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.keys != null)
		{
			Keyframe[] keys = obj.keys;
			foreach (Keyframe obj2 in keys)
			{
				ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1975582319(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.postWrapMode, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.preWrapMode, writer);
	}

	private AnimationCurve Deserialize118512508(AnimationCurve obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new AnimationCurve();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				List<Keyframe> list = new List<Keyframe>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Keyframe obj2 = default(Keyframe);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1975582319(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.keys = list.ToArray();
				break;
			}
			case 2:
				obj.postWrapMode = (WrapMode)reader.ReadInt32();
				break;
			case 3:
				obj.preWrapMode = (WrapMode)reader.ReadInt32();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1825589276(Behaviour obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
	}

	private Behaviour Deserialize1825589276(Behaviour obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.enabled = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize391689956(Bounds obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.center, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.extents, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
	}

	private Bounds Deserialize391689956(Bounds obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.center = Deserialize1181346079(obj.center, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.extents = Deserialize1181346079(obj.extents, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize892833698(BoxCollider obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isTrigger, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.center, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.size, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
	}

	private BoxCollider Deserialize892833698(BoxCollider obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
				obj.isTrigger = reader.ReadBoolean();
				break;
			case 3:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.center = Deserialize1181346079(obj.center, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.size = Deserialize1181346079(obj.size, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1590521044(CapsuleCollider obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isTrigger, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.center, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.radius, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.height, writer);
	}

	private CapsuleCollider Deserialize1590521044(CapsuleCollider obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
				obj.isTrigger = reader.ReadBoolean();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.center = Deserialize1181346079(obj.center, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.radius = reader.ReadSingle();
				break;
			case 5:
				obj.height = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize72619689(Collider obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isTrigger, writer);
	}

	private Collider Deserialize72619689(Collider obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
				obj.isTrigger = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1404661584(Color obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.r, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.g, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.b, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.a, writer);
	}

	private Color Deserialize1404661584(Color obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.r = reader.ReadSingle();
				break;
			case 2:
				obj.g = reader.ReadSingle();
				break;
			case 3:
				obj.b = reader.ReadSingle();
				break;
			case 4:
				obj.a = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize394322812(Component obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Component Deserialize394322812(Component obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize175529349(Gradient obj, int objTypeId, ProtoWriter writer)
	{
		if (obj.colorKeys != null)
		{
			GradientColorKey[] colorKeys = obj.colorKeys;
			foreach (GradientColorKey obj2 in colorKeys)
			{
				ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1747546845(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
		if (obj.alphaKeys != null)
		{
			GradientAlphaKey[] alphaKeys = obj.alphaKeys;
			foreach (GradientAlphaKey obj3 in alphaKeys)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
				Serialize1518696882(obj3, objTypeId, writer);
				ProtoWriter.EndSubItem(token2, writer);
			}
		}
	}

	private Gradient Deserialize175529349(Gradient obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		if (obj == null)
		{
			obj = new Gradient();
			ProtoReader.NoteObject(obj, reader);
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
			{
				List<GradientColorKey> list2 = new List<GradientColorKey>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					GradientColorKey obj3 = default(GradientColorKey);
					SubItemToken token2 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize1747546845(obj3, reader);
					ProtoReader.EndSubItem(token2, reader);
					list2.Add(obj3);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.colorKeys = list2.ToArray();
				break;
			}
			case 2:
			{
				List<GradientAlphaKey> list = new List<GradientAlphaKey>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					GradientAlphaKey obj2 = default(GradientAlphaKey);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1518696882(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.alphaKeys = list.ToArray();
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1518696882(GradientAlphaKey obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.time, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.alpha, writer);
	}

	private GradientAlphaKey Deserialize1518696882(GradientAlphaKey obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.time = reader.ReadSingle();
				break;
			case 2:
				obj.alpha = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1747546845(GradientColorKey obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.time, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.color, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
	}

	private GradientColorKey Deserialize1747546845(GradientColorKey obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.time = reader.ReadSingle();
				break;
			case 2:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.color = Deserialize1404661584(obj.color, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1975582319(Keyframe obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.inTangent, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.outTangent, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.tangentMode, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.time, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.value, writer);
	}

	private Keyframe Deserialize1975582319(Keyframe obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.inTangent = reader.ReadSingle();
				break;
			case 2:
				obj.outTangent = reader.ReadSingle();
				break;
			case 3:
				obj.tangentMode = reader.ReadInt32();
				break;
			case 4:
				obj.time = reader.ReadSingle();
				break;
			case 5:
				obj.value = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1364639273(Light obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.type, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.range, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.spotAngle, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1404661584(obj.color, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(5, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.intensity, writer);
		ProtoWriter.WriteFieldHeader(6, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.cookieSize, writer);
		ProtoWriter.WriteFieldHeader(7, WireType.Variant, writer);
		ProtoWriter.WriteInt32((int)obj.shadows, writer);
		ProtoWriter.WriteFieldHeader(8, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.shadowStrength, writer);
		ProtoWriter.WriteFieldHeader(9, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.shadowBias, writer);
	}

	private Light Deserialize1364639273(Light obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.type = (LightType)reader.ReadInt32();
				break;
			case 2:
				obj.range = reader.ReadSingle();
				break;
			case 3:
				obj.spotAngle = reader.ReadSingle();
				break;
			case 4:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.color = Deserialize1404661584(obj.color, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 5:
				obj.intensity = reader.ReadSingle();
				break;
			case 6:
				obj.cookieSize = reader.ReadSingle();
				break;
			case 7:
				obj.shadows = (LightShadows)reader.ReadInt32();
				break;
			case 8:
				obj.shadowStrength = reader.ReadSingle();
				break;
			case 9:
				obj.shadowBias = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize2028243609(MonoBehaviour obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.useGUILayout, writer);
	}

	private MonoBehaviour Deserialize2028243609(MonoBehaviour obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.useGUILayout = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1891515754(UnityEngine.Object obj, int objTypeId, ProtoWriter writer)
	{
	}

	private UnityEngine.Object Deserialize1891515754(UnityEngine.Object obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize605020259(Quaternion obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.z, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.w, writer);
	}

	private Quaternion Deserialize605020259(Quaternion obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadSingle();
				break;
			case 2:
				obj.y = reader.ReadSingle();
				break;
			case 3:
				obj.z = reader.ReadSingle();
				break;
			case 4:
				obj.w = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1762542304(SphereCollider obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.enabled, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.isTrigger, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.center, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.radius, writer);
	}

	private SphereCollider Deserialize1762542304(SphereCollider obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.enabled = reader.ReadBoolean();
				break;
			case 2:
				obj.isTrigger = reader.ReadBoolean();
				break;
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.center = Deserialize1181346079(obj.center, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			case 4:
				obj.radius = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize149935601(Transform obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
		SubItemToken token = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.localPosition, objTypeId, writer);
		ProtoWriter.EndSubItem(token, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize605020259(obj.localRotation, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
		Serialize1181346079(obj.localScale, objTypeId, writer);
		ProtoWriter.EndSubItem(token3, writer);
	}

	private Transform Deserialize149935601(Transform obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
			{
				SubItemToken token3 = ProtoReader.StartSubItem(reader);
				obj.localPosition = Deserialize1181346079(obj.localPosition, reader);
				ProtoReader.EndSubItem(token3, reader);
				break;
			}
			case 2:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj.localRotation = Deserialize605020259(obj.localRotation, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			case 3:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj.localScale = Deserialize1181346079(obj.localScale, reader);
				ProtoReader.EndSubItem(token, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1181346080(Vector2 obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.y, writer);
	}

	private Vector2 Deserialize1181346080(Vector2 obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadSingle();
				break;
			case 2:
				obj.y = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1181346079(Vector3 obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.z, writer);
	}

	private Vector3 Deserialize1181346079(Vector3 obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadSingle();
				break;
			case 2:
				obj.y = reader.ReadSingle();
				break;
			case 3:
				obj.z = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1181346078(Vector4 obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.x, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.y, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.z, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.w, writer);
	}

	private Vector4 Deserialize1181346078(Vector4 obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.x = reader.ReadSingle();
				break;
			case 2:
				obj.y = reader.ReadSingle();
				break;
			case 3:
				obj.z = reader.ReadSingle();
				break;
			case 4:
				obj.w = reader.ReadSingle();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize438265826(UnstuckPlayer obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.previousPositions != null)
		{
			List<Vector3>.Enumerator enumerator = obj.previousPositions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Vector3 current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private UnstuckPlayer Deserialize438265826(UnstuckPlayer obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
			{
				List<Vector3> list = obj.previousPositions ?? new List<Vector3>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					Vector3 obj2 = default(Vector3);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize1181346079(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					list.Add(obj2);
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1297003913(UpgradeConsole obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.serializedModuleSlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj.serializedModuleSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				SubItemToken token = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token, writer);
			}
		}
	}

	private UpgradeConsole Deserialize1297003913(UpgradeConsole obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 3:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.serializedModuleSlots = dictionary;
				break;
			}
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize1113570486(Vehicle obj, int objTypeId, ProtoWriter writer)
	{
		switch (objTypeId)
		{
		case 2106147891:
		{
			ProtoWriter.WriteFieldHeader(2100, WireType.String, writer);
			SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
			Serialize2106147891(obj as SeaMoth, objTypeId, writer);
			ProtoWriter.EndSubItem(token2, writer);
			break;
		}
		case 1174302959:
		{
			ProtoWriter.WriteFieldHeader(2200, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1174302959(obj as Exosuit, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
			break;
		}
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		if (obj.vehicleName != null)
		{
			ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
			ProtoWriter.WriteString(obj.vehicleName, writer);
		}
		if (obj.vehicleColors != null)
		{
			Vector3[] vehicleColors = obj.vehicleColors;
			foreach (Vector3 obj2 in vehicleColors)
			{
				ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
				SubItemToken token3 = ProtoWriter.StartSubItem(null, writer);
				Serialize1181346079(obj2, objTypeId, writer);
				ProtoWriter.EndSubItem(token3, writer);
			}
		}
		if (obj.serializedModules != null)
		{
			byte[] serializedModules = obj.serializedModules;
			ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
			ProtoWriter.WriteBytes(serializedModules, writer);
		}
		if (obj.serializedModuleSlots != null)
		{
			Dictionary<string, string>.Enumerator enumerator = obj.serializedModuleSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current = enumerator.Current;
				ProtoWriter.WriteFieldHeader(5, WireType.String, writer);
				SubItemToken token4 = ProtoWriter.StartSubItem(null, writer);
				Serialize524780017(current, objTypeId, writer);
				ProtoWriter.EndSubItem(token4, writer);
			}
		}
		ProtoWriter.WriteFieldHeader(6, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.precursorOutOfWater, writer);
		if (obj.pilotId != null)
		{
			ProtoWriter.WriteFieldHeader(7, WireType.String, writer);
			ProtoWriter.WriteString(obj.pilotId, writer);
		}
	}

	private Vehicle Deserialize1113570486(Vehicle obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (true)
		{
			switch (num)
			{
			case 2100:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj = Deserialize2106147891(obj as SeaMoth, reader);
				ProtoReader.EndSubItem(token2, reader);
				goto IL_0054;
			}
			case 2200:
			{
				SubItemToken token = ProtoReader.StartSubItem(reader);
				obj = Deserialize1174302959(obj as Exosuit, reader);
				ProtoReader.EndSubItem(token, reader);
				goto IL_0054;
			}
			}
			break;
			IL_0054:
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.vehicleName = reader.ReadString();
				break;
			case 3:
			{
				List<Vector3> list = new List<Vector3>();
				int fieldNumber2 = reader.FieldNumber;
				do
				{
					Vector3 obj3 = default(Vector3);
					SubItemToken token4 = ProtoReader.StartSubItem(reader);
					obj3 = Deserialize1181346079(obj3, reader);
					ProtoReader.EndSubItem(token4, reader);
					list.Add(obj3);
				}
				while (reader.TryReadFieldHeader(fieldNumber2));
				obj.vehicleColors = list.ToArray();
				break;
			}
			case 4:
				obj.serializedModules = ProtoReader.AppendBytes(obj.serializedModules, reader);
				break;
			case 5:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int fieldNumber = reader.FieldNumber;
				do
				{
					KeyValuePair<string, string> obj2 = default(KeyValuePair<string, string>);
					SubItemToken token3 = ProtoReader.StartSubItem(reader);
					obj2 = Deserialize524780017(obj2, reader);
					ProtoReader.EndSubItem(token3, reader);
					dictionary[obj2.Key] = obj2.Value;
				}
				while (reader.TryReadFieldHeader(fieldNumber));
				obj.serializedModuleSlots = dictionary;
				break;
			}
			case 6:
				obj.precursorOutOfWater = reader.ReadBoolean();
				break;
			case 7:
				obj.pilotId = reader.ReadString();
				break;
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize2124310223(VFXConstructing obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.constructed, writer);
	}

	private VFXConstructing Deserialize2124310223(VFXConstructing obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.constructed = reader.ReadSingle();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize328683587(Warper obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Warper Deserialize328683587(Warper obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}

	private void Serialize522953267(WaterPark obj, int objTypeId, ProtoWriter writer)
	{
		if (objTypeId == 1793440205)
		{
			ProtoWriter.WriteFieldHeader(1000, WireType.String, writer);
			SubItemToken token = ProtoWriter.StartSubItem(null, writer);
			Serialize1793440205(obj as LargeRoomWaterPark, objTypeId, writer);
			ProtoWriter.EndSubItem(token, writer);
		}
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj._constructed, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
		SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
		Serialize497398254(obj._moduleFace, objTypeId, writer);
		ProtoWriter.EndSubItem(token2, writer);
	}

	private WaterPark Deserialize522953267(WaterPark obj, ProtoReader reader)
	{
		int num = reader.ReadFieldHeader();
		while (num > 0 && num == 1000)
		{
			SubItemToken token = ProtoReader.StartSubItem(reader);
			obj = Deserialize1793440205(obj as LargeRoomWaterPark, reader);
			ProtoReader.EndSubItem(token, reader);
			num = reader.ReadFieldHeader();
		}
		while (num > 0)
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj._constructed = reader.ReadSingle();
				break;
			case 3:
			{
				SubItemToken token2 = ProtoReader.StartSubItem(reader);
				obj._moduleFace = Deserialize497398254(obj._moduleFace, reader);
				ProtoReader.EndSubItem(token2, reader);
				break;
			}
			default:
				reader.SkipField();
				break;
			}
			num = reader.ReadFieldHeader();
		}
		return obj;
	}

	private void Serialize1054395028(WaterParkCreature obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteInt32(obj.version, writer);
		ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.age, writer);
		ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, writer);
		ProtoWriter.WriteSingle(obj.timeNextBreed, writer);
		ProtoWriter.WriteFieldHeader(4, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.bornInside, writer);
	}

	private WaterParkCreature Deserialize1054395028(WaterParkCreature obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			switch (num)
			{
			case 1:
				obj.version = reader.ReadInt32();
				break;
			case 2:
				obj.age = reader.ReadSingle();
				break;
			case 3:
				obj.timeNextBreed = reader.ReadSingle();
				break;
			case 4:
				obj.bornInside = reader.ReadBoolean();
				break;
			default:
				reader.SkipField();
				break;
			}
		}
		return obj;
	}

	private void Serialize840818195(WeldableWallPanelGeneric obj, int objTypeId, ProtoWriter writer)
	{
		ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
		ProtoWriter.WriteBoolean(obj.repaired, writer);
	}

	private WeldableWallPanelGeneric Deserialize840818195(WeldableWallPanelGeneric obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			if (num == 1)
			{
				obj.repaired = reader.ReadBoolean();
			}
			else
			{
				reader.SkipField();
			}
		}
		return obj;
	}

	private void Serialize1333430695(Workbench obj, int objTypeId, ProtoWriter writer)
	{
	}

	private Workbench Deserialize1333430695(Workbench obj, ProtoReader reader)
	{
		for (int num = reader.ReadFieldHeader(); num > 0; num = reader.ReadFieldHeader())
		{
			reader.SkipField();
		}
		return obj;
	}
}
