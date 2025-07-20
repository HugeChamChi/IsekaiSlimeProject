using System.Collections.Generic;
using Unit;
using UnityEngine;

namespace Units
{
    public static class SkillRangePattern
    {
        public static readonly Dictionary<SkillRangeType, Vector2Int[]> Offsets = new()
        {
            {
                SkillRangeType.Short1,
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) }
            },
            { SkillRangeType.Short2, new[] { new Vector2Int(0, -1), new Vector2Int(0, 1) } },
            {
                SkillRangeType.Long1,
                new[]
                {
                    new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0),
                    new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1),
                    new Vector2Int(0, -2), new Vector2Int(-2, 0), new Vector2Int(2, 0), new Vector2Int(0, 2)
                }
            },
            {
                SkillRangeType.Long2,
                new[]
                {
                    new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(0, 2), new Vector2Int(2, 0), new Vector2Int(0, -2),
                    new Vector2Int(-2, 0), new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1),
                    new Vector2Int(1, -1), new Vector2Int(0, 3), new Vector2Int(0, -3), new Vector2Int(3, 0),
                    new Vector2Int(-3, 0), new Vector2Int(-1, -2), new Vector2Int(-1, 2), new Vector2Int(1, -2),
                    new Vector2Int(1, 2), new Vector2Int(-2, -1), new Vector2Int(-2, 1), new Vector2Int(2, -1),
                    new Vector2Int(2, 1),
                }
            }
        };
    }
}