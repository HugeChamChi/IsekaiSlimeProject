using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public interface IEffectable
{
    public void Apply(EffectType type, float amount = 0);
    public void Revoke(EffectType type, float amount = 0);
}
