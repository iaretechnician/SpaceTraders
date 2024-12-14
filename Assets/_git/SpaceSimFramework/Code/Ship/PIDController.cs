using UnityEngine;

namespace SpaceSimFramework
{
/// <summary>
/// Implementation of proportional-integral-derivative controller. Just google it.
/// </summary>
public class PIDController
{
    public float pFactor, iFactor, dFactor;

    private Vector3 _integral;
    private Vector3 _lastError;

    public PIDController(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public Vector3 Update(Vector3 currentError, float timeFrame)
    {
        _integral += currentError * timeFrame;
        var deriv = (currentError - _lastError) / timeFrame;
        _lastError = currentError;
        return currentError * pFactor
            + _integral * iFactor
            + deriv * dFactor;
    }
}
}