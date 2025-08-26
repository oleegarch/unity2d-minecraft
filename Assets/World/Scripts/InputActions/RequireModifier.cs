// RequireModifier.cs
// Custom Input System Interaction for Unity: "Require Modifier"
// - Позволяет в Input Actions Editor выставить требование наличия или отсутствия модификатора (Ctrl/Shift/Alt)
// - Параметры (видны в инспекторе interaction): modifier (Ctrl/Shift/Alt) и invert (если true — требовать, чтобы модификатор НЕ был зажат)
// Примеры использования:
//  - Чтобы action срабатывал ТОЛЬКО когда зажат Ctrl: в Interactions выбери "Require Modifier", modifier = Ctrl, invert = false
//  - Чтобы action срабатывал ТОЛЬКО когда Ctrl НЕ зажат: modifier = Ctrl, invert = true

using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

[DisplayName("Require Modifier")]
public class RequireModifier : IInputInteraction
{
    public enum ModifierKey { Ctrl, Shift, Alt }

    // Выбираемый модификатор (в инспекторе Input Actions Editor будет видно)
    public ModifierKey modifier = ModifierKey.Ctrl;

    // invert = false: require modifier pressed
    // invert = true: require modifier NOT pressed
    public bool invert = false;

    public void Process(ref InputInteractionContext context)
    {
        var kb = Keyboard.current;
        bool pressed = false;

        if (kb != null)
        {
            switch (modifier)
            {
                case ModifierKey.Ctrl:
                    // ctrlKey объединяет оба ctrl'а, но на всякий случай проверяем боковые тоже
                    pressed = (kb.ctrlKey != null && kb.ctrlKey.isPressed) ||
                              (kb.leftCtrlKey != null && kb.leftCtrlKey.isPressed) ||
                              (kb.rightCtrlKey != null && kb.rightCtrlKey.isPressed);
                    break;

                case ModifierKey.Shift:
                    pressed = (kb.shiftKey != null && kb.shiftKey.isPressed) ||
                              (kb.leftShiftKey != null && kb.leftShiftKey.isPressed) ||
                              (kb.rightShiftKey != null && kb.rightShiftKey.isPressed);
                    break;

                case ModifierKey.Alt:
                    pressed = (kb.altKey != null && kb.altKey.isPressed) ||
                              (kb.leftAltKey != null && kb.leftAltKey.isPressed) ||
                              (kb.rightAltKey != null && kb.rightAltKey.isPressed);
                    break;
            }
        }

        bool condition = invert ? !pressed : pressed;

        if (condition)
        {
            // Если условие соблюдено — помечаем interaction как выполненное
            context.Performed();
        }
        else
        {
            // Иначе — отменяем
            context.Canceled();
        }
    }

    public void Reset() { }

    // Регистрация interaction при запуске (чтобы он появился в редакторе и в рантайме)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitAtRuntime()
    {
        InputSystem.RegisterInteraction<RequireModifier>();
    }

#if UNITY_EDITOR
    // Регистрация в редакторе (чтобы сразу был виден в Input Actions Editor)
    [UnityEditor.InitializeOnLoadMethod]
    static void InitInEditor()
    {
        InputSystem.RegisterInteraction<RequireModifier>();
    }
#endif
}