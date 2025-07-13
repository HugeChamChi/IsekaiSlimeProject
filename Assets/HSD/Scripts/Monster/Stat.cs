using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class StatFloat : Stat<float, float>
{
    public StatFloat() : base((a, b) => a + b, x => x) { }
}

[Serializable]
public class Stat<T,T1>
{
    [Serializable]
    private readonly struct Modifier<T>
    {
        public readonly T value;
        public readonly string source;

        public Modifier(T value, string source)
        {
            this.value = value;
            this.source = source;
        }
    }

    [SerializeField] private T baseValue;
    [SerializeField] private readonly List<Modifier<T>> modifiers = new List<Modifier<T>>();
    public T1 Value => GetValue();

    private T1 finalValue;
    private bool isChangeValue;

    private readonly Func<T, T, T> combineFunc;
    private readonly Func<T, T1> convertFunc;

    private event Action<T1> onValueChanged;

    /// <param name="combineFunc">
    /// 두 값을 결합하는 함수입니다. 예: <c>(a, b) => a + b</c><br/>
    /// 예를 들어, 스탯을 누적하거나 합칠 때 사용됩니다.
    /// </param>
    /// <param name="convertFunc">
    /// 타입 <typeparamref name="T"/>을 <typeparamref name="T1"/>으로 변환하는 함수입니다.<br/>
    /// 예: <c>value => Mathf.Clamp(value, 0, 100)</c><br/>
    /// 주로 정규화, 범위 제한 등에 사용됩니다.
    /// </param>
    public Stat(Func<T, T, T> combineFunc , Func<T, T1> convertFunc = null)
    {
        this.combineFunc = combineFunc;
        this.convertFunc = convertFunc;
    }

    public void Initialize(T value)
    {
        baseValue = value;
        FinalValueCalculator();
        isChangeValue = false;
    }

    public void SetBaseValue(T value) { baseValue = value; Notify(); }

    public void AddModifier(T value, string source) { modifiers.Add(new Modifier<T>(value, source)); Notify(); }

    public void RemoveModifier(T value, string source) { modifiers.Remove(new Modifier<T>(value, source)); Notify(); }
    public bool CheckDuplication(string source) => modifiers.Any(m => m.source == source);
    public void RemoveModifierAll(string source) { modifiers.RemoveAll(m => m.source == source); Notify(); }

    public void ClearModifier() => modifiers.Clear();

    public int GetModifierCount() => modifiers.Count;

    private T1 GetValue()
    {
        if (isChangeValue)
        {
            FinalValueCalculator();            
            isChangeValue = false;
        }        

        return finalValue;
    }

    private void Notify()
    {
        isChangeValue = true;
        onValueChanged?.Invoke(GetValue());
    }

    private void FinalValueCalculator()
    {
        T result = baseValue;

        foreach (var mod in modifiers)
        {
            result = combineFunc(result, mod.value);
        }

        if(convertFunc != null)
            finalValue = convertFunc(result);
    }

    public void AddEvent(Action<T1> action)     => onValueChanged += action;
    public void RemoveEvent(Action<T1> action)  => onValueChanged -= action;
    public void ClearEvent() { if (onValueChanged == null) return; foreach (Action<T1> action in onValueChanged.GetInvocationList()) onValueChanged -= action; }
}
