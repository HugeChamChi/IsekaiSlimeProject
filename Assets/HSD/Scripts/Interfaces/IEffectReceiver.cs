using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public interface IEffectReceiver
{
    void ReceiveEffect(StatType type, float amount, float duration, string source);
    IEnumerator EffectRoutine(StatType type, float amount, float duration, string source);
}
