﻿using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct BasicInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float timeDelta;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float rotationRange = 120f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      for (int j = 0; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * timeDelta;
        individual.path[j] = new float2(rotation, size);
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.5f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y));
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}

/// <summary>
/// Initial rotation range is 60 degree cone (-30 - 30)
/// After that, only 5 degree rotations are allowed
/// </summary>
[BurstCompile]
public struct DebugInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var pathSize = 10;

    var individual = new BasicIndividualStruct();
    individual.Initialize(pathSize, Allocator.TempJob);
    individual.path.Add(new float2(0, 1));

    for (int j = 0; j < pathSize - 1; j++)
    {
      individual.path.Add(new float2(0, 1));
    }
    currentPopulation[0] = individual;


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y), new Color(0, 1, 0), 50, false);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}


[BurstCompile]
public struct GlobeInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float initRotationRange = 360 / populationSize;
    float rotationRange = 30;
    float initRotation = 0f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.path[0] = new float2(initRotation, agentSpeed * updateInterval);
      initRotation += initRotationRange;

      for (int j = 1; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * updateInterval;
        individual.path[j] = new float2(rotation, size);
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.2f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y));
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}

/// <summary>
/// Initial rotation range is 60 degree cone (-30 - 30)
/// After that, only 5 degree rotations are allowed
/// </summary>
[BurstCompile]
public struct KineticFriendlyInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float initRotationRange = 120 / populationSize;
    float rotationRange = 15;
    float initRotation = -60f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.path[0] = new float2(initRotation, agentSpeed * updateInterval);
      initRotation += initRotationRange;

      for (int j = 1; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * updateInterval;
        individual.path[j] = new float2(rotation, size);
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.2f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y), new Color(0,1,0), 50, false);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}
