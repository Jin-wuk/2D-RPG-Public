# 2D Action RPG

몬스터를 사냥해 모은 장비를 강화하고 보스를 쓰러뜨리는 **횡스크롤 2D 액션 RPG**입니다.
기획부터 끝까지 혼자 만든 첫 개인 프로젝트로, 전투·스킬, 인벤토리·장비·확률형 강화, 상점, 포탈 맵 이동, 공격 패턴 5종을 가진 보스까지 구현했습니다.

- **개발 형태**: 1인 개인 프로젝트 (기획·프로그래밍 전체)
- **개발 기간**: 2025.07 – 2026.01
- **엔진**: Unity 2022.3 (2D), C#

> **이 레포는 코드 열람용입니다.**
> 원본 프로젝트에는 재배포가 금지된 외부 아트 에셋(Unity Asset Store 등)이 포함되어 있어, 직접 작성한 C# 스크립트(`Assets/02_Scripts`)만 공개했습니다. 씬·프리팹·아트가 없으므로 이 레포만으로는 실행되지 않습니다.

## 핵심 시스템

| 시스템 | 한 줄 설명 | 주요 코드 |
| --- | --- | --- |
| 보스 AI | 공격 패턴마다 `Enter / Update / Exit`를 가진 상태 클래스로 나누고, 거리·쿨타임·페이즈로 후보를 구성해 무작위로 다음 패턴을 고릅니다. HP 50% 이하에서 광폭화해 쿨타임이 줄고 전용 패턴이 열립니다. | [BossController.cs](Assets/02_Scripts/Boss/BossController.cs)<br>[BossBaseState.cs](Assets/02_Scripts/Boss/BossBaseState.cs)<br>[Boss/](Assets/02_Scripts/Boss/) |
| 장비 강화 | 아이템 원본(ScriptableObject)은 그대로 두고, 획득할 때마다 복제본을 만들어 아이템마다 다른 강화 수치를 갖게 합니다. 강화는 확률형이며 실패하면 아이템이 파괴됩니다. | [ItemFactory.cs](Assets/02_Scripts/Item/ItemFactory.cs)<br>[InventoryManager.cs](Assets/02_Scripts/Inventory/InventoryManager.cs)<br>[EnhanceManager.cs](Assets/02_Scripts/Enchance/EnhanceManager.cs) |
| 맵 이동·리스폰 | 플레이어가 있는 맵만 켜 두는 구조에서, 작업의 수명에 맞춰 코루틴의 실행 주체를 정했습니다. 리스폰은 맵이 켜져 있을 때만 흐르는 `Update` 타이머로, 로딩 연출은 항상 살아 있는 `LoadingManager`에서 실행합니다. | [EnemySpawner.cs](Assets/02_Scripts/Enemy/EnemySpawner.cs)<br>[Portal.cs](Assets/02_Scripts/Map/Portal.cs) |

### 보스 패턴

| 상태 | 내용 |
| --- | --- |
| `BossIdleState` | 대기 후 다음 패턴 선택 |
| `BossJumpState` | 기 모으기 → 도약 → 공중 정지(범위 예고) → 내려찍기 |
| `BossLaserState` | 플레이어 방향 레이저 (광폭화 시 1발 → 3발) |
| `BossSkyFallState` | 천장에서 레이저 낙하, 안전 구역을 남겨 둠 |
| `BossExplodeState` | 광폭화 진입 시 즉시 발동하는 원형 폭발 |
| `BossLaserStormState` | 벽·천장에서 추적 레이저 12발 (광폭화 전용) |

다섯 공격 패턴은 모두 먼저 예고한 뒤 발동합니다. 내려찍기와 폭발은 바닥 범위 표시(`AttackIndicator`)로, 레이저 3종은 얇은 조준선(`LineRenderer`)으로 예고합니다. 피격 정보는 `HitData`(데미지·넉백 방향과 세기)로 묶어 공격하는 쪽이 정합니다.

## 폴더 구조

```
Assets/02_Scripts
├── Boss        보스 컨트롤러와 패턴 상태 6종, 공격 예고
├── Player      이동·전투·체력·스탯·레벨업, 아이템 줍기
├── Enemy       일반 몬스터 AI, 스포너, HP 바, 처치 보상
├── Skill       스킬 사용·퀵슬롯·강화
├── Inventory   인벤토리·장비 슬롯
├── Item        ItemFactory (원본 복제)
├── ItemDrop    몬스터 드랍
├── Enchance    확률형 장비 강화
├── Shop        상점 구매·판매
├── Map         맵 전환(MapManager), 포탈
├── Data        아이템·드랍 테이블·스킬 ScriptableObject 정의
└── UI          로딩 연출(LoadingManager), 툴팁, 데미지 표시 등
```
