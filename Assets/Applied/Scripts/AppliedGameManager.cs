using UnityEngine;
using UnityEngine.InputSystem;

// 응용 버전 게임 흐름.
// 상태: Menu(시작 화면) → Playing → Settings(일시 정지) / GameOver → Playing(다시 하기) 또는 Menu(시작 화면으로).
// 시작 전·설정 중·게임 오버 중에는 시간을 멈추고(timeScale 0) 플레이어 입력과 기믹 생성을 끈다.
// 새 판은 씬을 다시 읽지 않고(빌드 설정 변경 없이) 시간·공·기믹·기둥·플레이어·입력 상태를 처음으로 되돌린다.
public class AppliedGameManager : MonoBehaviour
{
    public enum State { Menu, Playing, Settings, GameOver }

    [SerializeField] private AppliedPlayerController player;
    [SerializeField] private AppliedBallSpawner spawner;
    [SerializeField] private AppliedSidePusher sidePusher;
    [SerializeField] private AppliedSweepBar sweepBar;
    [SerializeField] private AppliedSlamBarSpawner slamSpawner;
    [SerializeField] private AppliedSideBallSpawner sideBallSpawner;
    [SerializeField] private AppliedPillarMover[] pillars;
    [SerializeField] private AppliedKeyBindings keyBindings;
    [SerializeField] private AppliedMenuUI menuUI;
    // 플레이어가 이 높이보다 아래로 떨어지면 사망(제안 기본값, 미확정).
    [SerializeField] private float killHeight = -2f;

    private Vector3 playerStartPosition;
    private float survivalTime;
    private State state = State.Menu;
    // 설정 창을 닫으면 돌아갈 상태(시작 화면 또는 게임 중).
    private State settingsReturnState = State.Menu;
    private bool hasWaitingAction;
    private AppliedKeyBindings.Action waitingAction;

    public State CurrentState => state;
    public bool IsGameOver => state == State.GameOver;
    public float SurvivalTime => survivalTime;

    private void Start()
    {
        if (player != null) playerStartPosition = player.transform.position;
        menuUI.Build(this);
        RefreshKeyLabels();
        BackToMenu();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (state == State.Settings)
        {
            if (hasWaitingAction) ListenForRebind(keyboard);
            else if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) CloseSettings();
            return;
        }

        if (keyboard != null)
        {
            if (state == State.Playing && keyboard.kKey.wasPressedThisFrame) { OpenSettings(); return; }
            if ((state == State.Playing || state == State.GameOver) && keyboard.rKey.wasPressedThisFrame) { PlayAgain(); return; }
        }

        if (state != State.Playing) return;

        survivalTime += Time.deltaTime;
        menuUI.SetTime(survivalTime);
        if (player != null && player.transform.position.y < killHeight) GameOver();
    }

    // ---- 상태 전환 (버튼과 키가 함께 쓴다) ----

    public void StartGame()
    {
        ResetRound();
        SetGameplayActive(true);
        state = State.Playing;
        menuUI.HideAllPanels();
        UpdateHint();
        Debug.Log("State: Playing");
    }

    public void PlayAgain()
    {
        if (state != State.Playing && state != State.GameOver) return;
        Debug.Log("Play Again");
        StartGame();
    }

    public void BackToMenu()
    {
        ResetRound();
        SetGameplayActive(false);
        state = State.Menu;
        menuUI.ShowMenu();
        UpdateHint();
        Debug.Log("State: Menu");
    }

    public void GameOver()
    {
        if (state != State.Playing) return;
        SetGameplayActive(false);
        state = State.GameOver;
        menuUI.ShowGameOver(survivalTime);
        UpdateHint();
        Debug.Log("Game Over");
    }

    public void OpenSettings()
    {
        if (state != State.Menu && state != State.Playing) return;
        settingsReturnState = state;
        // 설정 중에는 시간을 멈추고 플레이어 입력을 끈다(점프 요청도 지워진다).
        SetGameplayActive(false);
        state = State.Settings;
        hasWaitingAction = false;
        RefreshKeyLabels();
        menuUI.SetSettingsMessage("");
        menuUI.ShowSettings();
        Debug.Log("State: Settings");
    }

    // 뒤로가기: 설정을 연 곳(시작 화면 또는 게임 중)으로 돌아간다.
    public void CloseSettings()
    {
        if (state != State.Settings) return;
        hasWaitingAction = false;
        state = settingsReturnState;
        if (state == State.Playing)
        {
            menuUI.HideAllPanels();
            SetGameplayActive(true);
        }
        else
        {
            menuUI.ShowMenu();
        }
        UpdateHint();
        Debug.Log("State: " + state);
    }

    // ---- 키 설정 ----

    public void BeginRebind(AppliedKeyBindings.Action action)
    {
        if (state != State.Settings) return;
        waitingAction = action;
        hasWaitingAction = true;
        RefreshKeyLabels();
        menuUI.SetKeyLabel(action, "Press a key...");
        menuUI.SetSettingsMessage("Press a key (Esc = cancel)");
    }

    public void ResetKeysToDefault()
    {
        // 키 입력을 기다리던 중이었다면 그 대기도 함께 끝낸다.
        hasWaitingAction = false;
        keyBindings.ResetToDefault();
        RefreshKeyLabels();
        menuUI.SetSettingsMessage("Reset to WASD + Space");
    }

    private void ListenForRebind(Keyboard keyboard)
    {
        if (keyboard == null) return;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            hasWaitingAction = false;
            RefreshKeyLabels();
            menuUI.SetSettingsMessage("Canceled");
            return;
        }

        foreach (var control in keyboard.allKeys)
        {
            if (control == null || !control.wasPressedThisFrame) continue;
            Key key = control.keyCode;
            if (key == Key.R || key == Key.K)
            {
                menuUI.SetSettingsMessage("R and K are reserved (Play Again / Settings)");
                return;
            }
            // 이미 다른 동작이 쓰는 키면 두 동작의 키를 맞바꾼다(중복 등록 방지).
            menuUI.SetSettingsMessage(keyBindings.SetKey(waitingAction, key));
            hasWaitingAction = false;
            RefreshKeyLabels();
            return;
        }
    }

    private void RefreshKeyLabels()
    {
        for (int i = 0; i < AppliedKeyBindings.ActionCount; i++)
        {
            var action = (AppliedKeyBindings.Action)i;
            menuUI.SetKeyLabel(action, keyBindings.GetKey(action).ToString());
        }
    }

    // ---- 새 판 준비 ----

    // 시간·공·기믹·기둥·플레이어·입력 상태를 처음으로 되돌린다. 난이도는 생존 시간으로 계산하므로 함께 초기화된다.
    private void ResetRound()
    {
        survivalTime = 0f;
        hasWaitingAction = false;
        menuUI.SetTime(0f);

        if (spawner != null) spawner.ResetSpawner();
        if (sidePusher != null) sidePusher.ResetHazard();
        if (sweepBar != null) sweepBar.ResetHazard();
        if (slamSpawner != null) slamSpawner.ResetSpawner();
        if (sideBallSpawner != null) sideBallSpawner.ResetSpawner();
        if (pillars != null)
        {
            foreach (AppliedPillarMover pillar in pillars) pillar.ResetPillar();
        }
        if (player != null) player.ResetState(playerStartPosition);
    }

    // 게임 진행 중에만 시간이 흐르고 플레이어 입력과 기믹 생성이 켜진다.
    private void SetGameplayActive(bool active)
    {
        Time.timeScale = active ? 1f : 0f;
        if (player != null) player.enabled = active;
        if (spawner != null) spawner.enabled = active;
        if (slamSpawner != null) slamSpawner.enabled = active;
        if (sideBallSpawner != null) sideBallSpawner.enabled = active;
    }

    private void UpdateHint()
    {
        string jump = keyBindings.GetKey(AppliedKeyBindings.Action.Jump).ToString();
        menuUI.SetHint(state == State.Playing
            ? $"[{jump}] Jump   [K] Settings (pause)   [R] Play Again   Fall off = Game Over"
            : "");
    }
}
