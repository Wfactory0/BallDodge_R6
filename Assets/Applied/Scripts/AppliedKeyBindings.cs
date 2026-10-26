using UnityEngine;
using UnityEngine.InputSystem;

// 응용 버전 조작키 설정. 이동 4방향과 점프 키를 바꿀 수 있고, 재시작 후에도 유지되도록 PlayerPrefs에 저장한다.
public class AppliedKeyBindings : MonoBehaviour
{
    public enum Action { Up, Down, Left, Right, Jump }

    public const int ActionCount = 5;
    private const string PrefsPrefix = "BallDodgeApplied.Key.";

    private readonly Key[] defaultKeys = { Key.W, Key.S, Key.A, Key.D, Key.Space };

    private readonly Key[] keys = new Key[ActionCount];

    private void Awake()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = (Key)PlayerPrefs.GetInt(PrefsPrefix + (Action)i, (int)defaultKeys[i]);
        }
        // 저장값이 깨져 같은 키가 두 동작에 걸려 있으면 기본값으로 되돌린다.
        if (HasDuplicate()) ResetToDefault();
    }

    public Key GetKey(Action action) => keys[(int)action];

    public bool IsPressed(Action action)
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[keys[(int)action]].isPressed;
    }

    public bool WasPressedThisFrame(Action action)
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[keys[(int)action]].wasPressedThisFrame;
    }

    // 키를 지정한다. 이미 다른 동작이 쓰는 키면 두 동작의 키를 맞바꿔 한 키가 두 동작에 걸리지 않게 한다.
    // 바뀐 내용을 설명하는 문장을 돌려준다.
    public string SetKey(Action action, Key key)
    {
        Key previous = keys[(int)action];
        string result = $"{action} = {key}";
        for (int i = 0; i < keys.Length; i++)
        {
            if (i == (int)action || keys[i] != key) continue;
            Store((Action)i, previous);
            result += $" (swapped: {(Action)i} = {previous})";
        }
        Store(action, key);
        PlayerPrefs.Save();
        return result;
    }

    public void ResetToDefault()
    {
        for (int i = 0; i < keys.Length; i++) Store((Action)i, defaultKeys[i]);
        PlayerPrefs.Save();
    }

    private void Store(Action action, Key key)
    {
        keys[(int)action] = key;
        PlayerPrefs.SetInt(PrefsPrefix + action, (int)key);
    }

    private bool HasDuplicate()
    {
        for (int i = 0; i < keys.Length; i++)
            for (int j = i + 1; j < keys.Length; j++)
                if (keys[i] == keys[j]) return true;
        return false;
    }
}
