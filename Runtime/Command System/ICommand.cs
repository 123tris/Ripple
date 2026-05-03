using System;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace Ripple.Experimental
{
    public interface ICommand
    {
        void Execute(GameObject context);
    }

    public interface ICondition
    {
        bool Evaluate(GameObject context);
    }

    public class CommandSO : ScriptableObject
    {
        [SerializeReference] private List<ICommand> _commands = new();

        public void Execute(GameObject context)
        {
            foreach (var command in _commands)
            {
                command.Execute(context);
            }
        }
    }

    [Serializable]
    public class BranchCommand : ICommand
    {
        [SerializeReference] private ICondition _condition;

        [SerializeReference] private ICommand _onTrue;
        [SerializeReference] private ICommand _onFalse;

        public void Execute(GameObject context)
        {
            if (_condition.Evaluate(context))
                _onTrue?.Execute(context);
            else
                _onFalse?.Execute(context);
        }
    }

    [Serializable]
    public class AddCommand : ICommand
    {
        [SerializeField] VariableInstancer _variableInstancer;
        [SerializeField] private VariableDeclaration variable;
        [SerializeField] private float valueToAdd;

        public void Execute(GameObject context)
        {
            object instanceValue = _variableInstancer.GetInstanceValue(variable);
            int castValue = (int)instanceValue;
            float value = castValue;
            _variableInstancer.SetInstanceValue(variable, value + valueToAdd);
        }
    }
}