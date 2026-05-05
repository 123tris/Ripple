using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple.Editor
{
    [CreateAssetMenu(menuName = "Ripple/Codegen Config", fileName = "RippleCodegenConfig")]
    public sealed class RippleCodegenConfig : ScriptableObject
    {
        [Serializable]
        public struct Row
        {
            public string TypeName;
            public string CSharpType;
            public string DisplayName;
            public bool IsNumeric;
            public bool IsDisplayable;
            public bool IsComparable;
        }

        public string OutputDirectory = "Assets/Ripple/Runtime/Generated";

        public List<Row> Rows = new List<Row>
        {
            new Row { TypeName = "Bool",       CSharpType = "bool",                  DisplayName = "Bool",       IsDisplayable = true },
            new Row { TypeName = "Int",        CSharpType = "int",                   DisplayName = "Int",        IsNumeric = true, IsDisplayable = true, IsComparable = true },
            new Row { TypeName = "Float",      CSharpType = "float",                 DisplayName = "Float",      IsNumeric = true, IsDisplayable = true, IsComparable = true },
            new Row { TypeName = "Double",     CSharpType = "double",                DisplayName = "Double",     IsNumeric = true, IsDisplayable = true, IsComparable = true },
            new Row { TypeName = "String",     CSharpType = "string",                DisplayName = "String",     IsDisplayable = true },
            new Row { TypeName = "Vector2",    CSharpType = "UnityEngine.Vector2",   DisplayName = "Vector2",    IsDisplayable = true },
            new Row { TypeName = "Vector3",    CSharpType = "UnityEngine.Vector3",   DisplayName = "Vector3",    IsDisplayable = true },
            new Row { TypeName = "GameObject", CSharpType = "UnityEngine.GameObject",DisplayName = "GameObject", IsDisplayable = true },
            new Row { TypeName = "Transform",  CSharpType = "UnityEngine.Transform", DisplayName = "Transform",  IsDisplayable = true },
            new Row { TypeName = "AudioClip",  CSharpType = "UnityEngine.AudioClip", DisplayName = "AudioClip" },
        };
    }
}
