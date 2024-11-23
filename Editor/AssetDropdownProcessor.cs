using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class AssetDropdownProcessor : OdinAttributeProcessor<MonoBehaviour>
{
    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.MemberType != MemberTypes.Field) return;

        Type propertyBaseType = ((FieldInfo)member).FieldType;
        if (propertyBaseType.IsSubclassOfRawGeneric(typeof(GameEvent<>)) || propertyBaseType.IsSubclassOfRawGeneric(typeof(VariableSO<>)))
        {
            attributes.Add(new AssetDropdown());
        }
    }
}
