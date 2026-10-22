using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BossStats : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor; // 원래 색상을 저장 (보통 흰색)
    private BossController controller;

    [Header("상태 변수")]
    public bool isInvincible = false; // 무적 여부
    public int maxHp = 500;
    public int currentHp;

    // 체력이 변할 때 UI를 갱신하기 위한 이벤트
    public System.Action<int, int> OnHpChanged;
    public System.Action OnBossDead;

    [Header("피격 연출 설정")]
    [SerializeField] private Color hitColor = Color.white; // 피격 시 변할 색상
    [SerializeField] private float flashDuration = 0.1f; // 색상이 유지되는 시간

    [Header("사망 연출 (슬로우 모션)")]
    [SerializeField] private float slowMotionScale = 0.5f;   // 0.x배속
    [SerializeField] private float slowMotionDuration = 5f;  // 실제 시간 기준 x초
    [SerializeField] private float appearanceFadeSpeed = 1.5f; // 등장 속도 (높을수록 빠름)

    [Header("데미지 폰트 설정")]
    [SerializeField] private GameObject damagePopupPrefab; // 일반 몬스터와 같은 프리팹 사용
    [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0); // 보스는 키가 크므로 높게 설정

    [Header("외부 연결 (제단에서 넘겨받음)")]
    [SerializeField] private GameObject exitPortal;
    private GameObject parentAltar; // [추가] 나를 소환한 제단 보관용

    public GameObject GetExitPortal() => exitPortal;
    public GameObject GetAltar() => parentAltar;

    public bool isDead { get; private set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<BossController>();

        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    // 제단에서 호출할 함수들
    public void SetAltar(GameObject altar) => parentAltar = altar;
    public void SetExitPortal(GameObject portal) => exitPortal = portal;


    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return; // 이미 죽었다면 무시

        // [핵심] 무적 상태라면 데미지 로직을 아예 실행하지 않음
        if (isInvincible)
        {
            Debug.Log("보스가 무적 상태입니다!");
            // 여기에 팅겨나가는 소리나 이펙트를 넣으면 좋습니다.
            return;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // UI에 현재 체력과 최대 체력을 전달합니다.
        OnHpChanged?.Invoke(currentHp, maxHp);

        SpawnDamagePopup(damage);

        // [추가] 컨트롤러에게 체력 변화를 알려 레이지 모드 체크
        if (controller != null)
        {
            controller.UpdateHp(currentHp, maxHp);
        }

        // [핵심] 색상 변경 코루틴 실행
        // 이미 실행 중인 코루틴이 있다면 멈추고 새로 시작 (연속 타격 시 색상 꼬임 방지)
        StopCoroutine("HitFlashRoutine");
        StartCoroutine("HitFlashRoutine");
        if (currentHp <= 0)
        {
            Die();
        }
    }


    private void SpawnDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        // 보스 머리 위(Offset 반영) 위치 계산
        Vector3 spawnPos = transform.position + popupOffset;

        // 팝업 생성
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

        // 팝업 스크립트 설정 (Enemy와 동일한 DamagePopup 스크립트 사용)
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            // 보스니까 데미지 숫자를 조금 더 크게 하거나 색상을 다르게 줄 수도 있습니다.
            popupScript.Setup(damage, Color.white);
        }
    }

    // [핵심] 레이지 모드 진입 시 호출되어 보스의 기본 색상을 변경함
    public void SetRageColor(Color newColor)
    {
        originalColor = newColor;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer == null) yield break;
        // 1. 색상을 지정한 피격 색상으로 변경
        spriteRenderer.color = hitColor;
        // 2. 잠시 대기
        yield return new WaitForSeconds(flashDuration);
        // 3. 원래 색상으로 복구
        spriteRenderer.color = originalColor;
    }


    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. [추가] 보스 컨트롤러를 가져와서 현재 상태(레이저 등)를 강제 종료
        BossController controller = GetComponent<BossController>();
        if (controller != null)
        {
            // ChangeState(null)을 호출하거나 Exit()를 직접 호출하여 
            // 레이저 코루틴과 프리팹을 정리합니다.
            controller.ChangeState(null);
        }

        // [핵심] 보스를 공중에 고정시키는 삼총사
        if (rb != null)
        {
            rb.velocity = Vector2.zero;    // 1. 현재 움직임 즉시 정지
            rb.isKinematic = true;         // 2. 중력 및 외력의 영향을 받지 않음 (공중 고정)
            rb.simulated = false;
        }

        // 2. 애니메이션 실행 및 UI 정리
        anim.SetTrigger("isDie");
        if (BossHPUI.Instance != null) BossHPUI.Instance.ClearUI();

        // 3. 충돌체 제거
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        // 4. 슬로우 모션 및 후속 연출 코루틴 시작
        StartCoroutine(DeathSequenceRoutine());

        OnBossDead?.Invoke();

    }

    private IEnumerator DeathSequenceRoutine()
    {
        // [슬로우 모션 시작]
        float originalFixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 슬로우 모션 지속 (실제 시간 기준)
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // [시간 정상화]
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // 5. [핵심] 시간이 정상으로 돌아온 후 포탈과 제단을 서서히 등장시킴
        if (exitPortal != null) StartCoroutine(FadeInObject(exitPortal));
        if (parentAltar != null)
        {
            parentAltar.SetActive(true); // 일단 활성화
            parentAltar.GetComponent<BossAltar>()?.ResetAltar();
            StartCoroutine(FadeInObject(parentAltar));
        }

        // 보스 소멸 대기
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
    // 오브젝트를 서서히 나타나게 하는 공용 코루틴
    private IEnumerator FadeInObject(GameObject obj)
    {
        obj.SetActive(true);

        // 모든 자식의 SpriteRenderer를 가져와서 투명도를 조절합니다.
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();

        // 처음엔 모두 투명하게 설정
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = 0;
            sr.color = c;
        }

        float alpha = 0;
        while (alpha < 1.0f)
        {
            alpha += Time.deltaTime * appearanceFadeSpeed;
            foreach (var sr in renderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            yield return null;
        }
    }
}
