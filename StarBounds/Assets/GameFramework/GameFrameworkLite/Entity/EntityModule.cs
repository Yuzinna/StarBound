// Assets/GameFrameworkLite/Entity/EntityModule.cs
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 내부에서 사용하는 엔티티 정보 구조체.
	/// </summary>
	internal sealed class EntityInfo
	{
		public int Id;              // 엔티티 ID
		public string AssetName;    // 리소스 경로
		public string GroupName;    // 그룹 이름
		public GameObject Instance; // 실제 GameObject 인스턴스
		public EntityLogic Logic;   // 로직 스크립트
	}

	/// <summary>
	/// 엔티티 그룹.
	/// Player, Enemy, Projectile 등 그룹별로 분류 가능.
	/// </summary>
	internal sealed class EntityGroup
	{
		public string Name;                 // 그룹 이름
		public Transform Root;             // Hierarchy 정리를 위한 루트 트랜스폼
		public readonly List<EntityInfo> Entities = new List<EntityInfo>();

		public EntityGroup(string name, Transform root)
		{
			Name = name;
			Root = root;
		}
	}

	/// <summary>
	/// UGF 스타일의 경량 엔티티 시스템.
	/// ShowEntity / HideEntity 기반.
	/// </summary>
	public sealed class EntityModule : IGameFrameworkModule
	{
		private readonly ResourceModule _resourceModule;
		private readonly ObjectPoolModule _objectPoolModule;

		// ID로 엔티티 찾기 위한 딕셔너리
		private readonly Dictionary<int, EntityInfo> _entitiesById =
			new Dictionary<int, EntityInfo>();

		// 그룹 이름 → 그룹
		private readonly Dictionary<string, EntityGroup> _groups =
			new Dictionary<string, EntityGroup>();

		// 모든 그룹을 담는 루트 트랜스폼
		private Transform _entityRoot;

		public EntityModule(ResourceModule resourceModule, ObjectPoolModule objectPoolModule)
		{
			_resourceModule = resourceModule;
			_objectPoolModule = objectPoolModule;

			// Hierarchy 정리를 위한 최상위 루트 생성
			var rootGo = new GameObject("[Entities]");
			Object.DontDestroyOnLoad(rootGo);
			_entityRoot = rootGo.transform;
		}

		/// <summary>
		/// 엔티티 그룹을 수동으로 추가.
		/// parent를 지정하면 그 밑으로 그룹이 생성됨.
		/// </summary>
		public void AddGroup(string groupName, Transform parent = null)
		{
			if (string.IsNullOrEmpty(groupName)) return;
			if (_groups.ContainsKey(groupName)) return;

			var go = new GameObject(groupName);
			go.transform.SetParent(parent ?? _entityRoot, false);
			_groups.Add(groupName, new EntityGroup(groupName, go.transform));
		}

		/// <summary>
		/// 그룹이 없으면 자동 생성 후 반환.
		/// </summary>
		private EntityGroup GetOrCreateGroup(string groupName)
		{
			if (string.IsNullOrEmpty(groupName))
				groupName = "Default";

			EntityGroup group;
			if (!_groups.TryGetValue(groupName, out group))
			{
				AddGroup(groupName);
				group = _groups[groupName];
			}
			return group;
		}

		/// <summary>
		/// 엔티티 생성(또는 풀에서 꺼내기).
		/// id        : 엔티티 고유 ID
		/// assetPath : Resources 경로 (예: "Entities/Player")
		/// groupName : 엔티티 그룹 이름
		/// userData  : 초기화 시 로직으로 넘길 데이터
		/// </summary>
		public void ShowEntity(int id, string assetPath, string groupName, object userData = null)
		{
			if (_entitiesById.ContainsKey(id))
			{
				Debug.LogError($"[EntityModule] Entity id {id} already exists.");
				return;
			}

			var group = GetOrCreateGroup(groupName);

			// ObjectPoolModule과 ResourceModule을 함께 사용해서 인스턴스 확보
			GameObject instance = _objectPoolModule.GetFromPool(assetPath, () =>
			{
				return _resourceModule.Instantiate(assetPath, Vector3.zero, Quaternion.identity, group.Root);
			});

			if (instance == null)
			{
				Debug.LogError($"[EntityModule] Failed to instantiate entity from '{assetPath}'.");
				return;
			}

			instance.transform.SetParent(group.Root, true);

			var logic = instance.GetComponent<EntityLogic>();
			if (logic == null)
			{
				Debug.LogError($"[EntityModule] Entity prefab '{assetPath}' has no EntityLogic.");
				return;
			}

			logic.Id = id;
			logic.AssetName = assetPath;
			logic.GroupName = groupName;

			var info = new EntityInfo
			{
				Id = id,
				AssetName = assetPath,
				GroupName = groupName,
				Instance = instance,
				Logic = logic
			};

			_entitiesById.Add(id, info);
			group.Entities.Add(info);

			// 엔티티 라이프사이클 콜백 호출
			logic.OnInit(userData);
			logic.OnShow(userData);
		}

		/// <summary>
		/// 엔티티 숨기기 (실제로는 풀에 반환).
		/// </summary>
		public void HideEntity(int id, object userData = null)
		{
			EntityInfo info;
			if (!_entitiesById.TryGetValue(id, out info))
				return;

			info.Logic.OnHide(userData);

			// 풀로 반환
			_objectPoolModule.ReleaseToPool(info.AssetName, info.Instance);

			_entitiesById.Remove(id);
			EntityGroup group;
			if (_groups.TryGetValue(info.GroupName, out group))
			{
				group.Entities.Remove(info);
			}
		}

		/// <summary>
		/// ID로 EntityLogic 가져오기.
		/// 위치 이동, 상태 변경 등에 사용.
		/// </summary>
		public EntityLogic GetEntityLogic(int id)
		{
			EntityInfo info;
			if (_entitiesById.TryGetValue(id, out info))
				return info.Logic;
			return null;
		}

		/// <summary>
		/// 매 프레임 엔티티들의 OnUpdate 호출.
		/// </summary>
		public void Update(float deltaTime, float realDeltaTime)
		{
			foreach (var kv in _entitiesById)
			{
				kv.Value.Logic.OnUpdate(deltaTime, realDeltaTime);
			}
		}

		/// <summary>
		/// 모든 엔티티 파괴 및 그룹 정리.
		/// </summary>
		public void Shutdown()
		{
			foreach (var kv in _entitiesById)
			{
				if (kv.Value.Instance != null)
					Object.Destroy(kv.Value.Instance);
			}
			_entitiesById.Clear();
			_groups.Clear();

			if (_entityRoot != null)
				Object.Destroy(_entityRoot.gameObject);
		}
	}
}
