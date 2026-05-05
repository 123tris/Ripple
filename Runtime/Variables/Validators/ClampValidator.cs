using System;
using UnityEngine;

namespace Ripple
{
    [Serializable]
    public sealed class ClampFloatValidator : IValidator<float>
    {
        [SerializeField] public float Min;
        [SerializeField] public float Max;

        public bool Validate(float candidate, out float corrected)
        {
            corrected = Mathf.Clamp(candidate, Min, Max);
            return true;
        }
    }

    [Serializable]
    public sealed class ClampIntValidator : IValidator<int>
    {
        [SerializeField] public int Min;
        [SerializeField] public int Max;

        public bool Validate(int candidate, out int corrected)
        {
            corrected = Mathf.Clamp(candidate, Min, Max);
            return true;
        }
    }

    [Serializable]
    public sealed class ClampDoubleValidator : IValidator<double>
    {
        [SerializeField] public double Min;
        [SerializeField] public double Max;

        public bool Validate(double candidate, out double corrected)
        {
            corrected = candidate < Min ? Min : candidate > Max ? Max : candidate;
            return true;
        }
    }
}
