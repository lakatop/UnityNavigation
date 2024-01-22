﻿using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using Unity.Collections;



[BurstCompile]
public struct BasicGeneticAlgorithmParallel : IJob, IGeneticAlgorithmParallel<BasicIndividualStruct>
{
  public MeanCrossOperatorParallel cross;
  public GreedyCircleMutationOperatorParallel mutation;
  public FitnessJerkCostParallel jerkFitness;
  public FitnessCollisionParallel collisionFitness;
  public FitnessEndDistanceParallel endDistanceFitness;
  public NegativeSelectionParallel selection;
  public KineticFriendlyInitialization popInitialization;
  public NativeBasicPopulation pop;
  public FitnessEvaluationLogger logger;


  public int iterations { get; set; }
  public int populationSize { get; set; }

  public NativeArray<Vector2> _winner;
  public float _timeDelta;
  public float _agentSpeed;
  public Vector2 _startPosition;
  public Vector2 _forward;

  public Unity.Mathematics.Random _rand;

  public void Execute()
  {
    popInitialization.ModifyPopulation(ref pop._population, 0);

    for (int i = 0; i < iterations; i++)
    {
      //jerkFitness.ModifyPopulation(ref pop._population, i);
      collisionFitness.ModifyPopulation(ref pop._population, i);
      //endDistanceFitness.ModifyPopulation(ref pop._population, i);

      logger.LogPopulationState(ref pop._population, i);
      selection.ModifyPopulation(ref pop._population, i);
      cross.ModifyPopulation(ref pop._population, i);
      mutation.ModifyPopulation(ref pop._population, i);
    }

    //jerkFitness.ModifyPopulation(ref pop._population, iterations);
    collisionFitness.ModifyPopulation(ref pop._population, iterations);
    //endDistanceFitness.ModifyPopulation(ref pop._population, iterations);
    logger.LogPopulationState(ref pop._population, iterations);
    SetWinner();
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 4);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
    _forward = (Vector2)resources[3];
    if (_forward.x == 0 && _forward.y == 0)
      _forward = new Vector2(1, 0);
  }

  public Vector2 GetResult()
  {
    return _winner[0];
  }

  private void SetWinner()
  {
    _winner[0] = new Vector2(0, 0);
    float maxFitness = float.MinValue;
    foreach (var individual in pop._population)
    {
      if (maxFitness < individual.fitness)
      {
        var v = UtilsGA.UtilsGA.RotateVector(_forward.normalized, individual.path[0].x);
        v *= individual.path[0].y;
        _winner[0] = new Vector2(v.x, v.y);
        maxFitness = individual.fitness;
      }
    }
  }

  public string GetConfiguration()
  {
    var builder = new System.Text.StringBuilder();
    builder.AppendLine(string.Format("CROSS,{0}", cross.GetComponentName()));
    builder.AppendLine(string.Format("MUTATION,{0}", mutation.GetComponentName()));
    builder.AppendLine(string.Format("FITNESSES,{0}, {1}, {2}", jerkFitness.GetComponentName(), endDistanceFitness.GetComponentName(), collisionFitness.GetComponentName()));
    builder.AppendLine(string.Format("SELECTION,{0}", selection.GetComponentName()));
    builder.AppendLine(string.Format("INITIALIZATION,{0}", popInitialization.GetComponentName()));

    return builder.ToString();
  }

  public void Dispose()
  {
    cross.Dispose();
    mutation.Dispose();
    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    selection.Dispose();
    logger.Dispose();
    _winner.Dispose();
    pop.Dispose();
  }
}


