﻿using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using NativeQuadTree;
using System.Linq;
using Unity.Collections;



[BurstCompile]
public struct BasicGeneticAlgorithmParallel : IJob, IGeneticAlgorithmParallel<BasicIndividualStruct>
{
  public BasicCrossOperatorParallel cross;
  public BasicMutationOperatorParallel mutation;
  public BasicFitnessFunctionParallel fitness;
  public BasicSelectionFunctionParallel selection;
  public BasicInitialization popInitialization;
  public NativeBasicPopulation pop;


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
    RunGA();
  }

  public void RunGA()
  {
    pop.SetPopulation(popInitialization.ModifyPopulation(pop.GetPopulation()));

    for (int i = 0; i < iterations; i++)
    {
      pop.SetPopulation(fitness.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(selection.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(cross.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(mutation.ModifyPopulation(pop.GetPopulation()));
    }

    pop.SetPopulation(fitness.ModifyPopulation(pop.GetPopulation()));
    SetWinner();
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 4);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
    _forward = (Vector2)resources[3];
  }

  public Vector2 GetResult()
  {
    return _winner[0];
  }

  private void SetWinner()
  {
    _winner[0] = new Vector2(0, 0);
    float maxFitness = 0.0f;
    foreach (var individual in pop.GetPopulation())
    {
      if (maxFitness < individual.fitness)
      {
        var v = UtilsGA.UtilsGA.RotateVector(_forward.normalized, individual.path[0].x);
        v *= individual.path[0].y;
        v = UtilsGA.UtilsGA.MoveToOrigin(v, _startPosition);
        _winner[0] = new Vector2(v.x, v.y);
        maxFitness = individual.fitness;
      }
    }
  }

  public void Dispose()
  {
    cross.Dispose();
    mutation.Dispose();
    fitness.Dispose();
    selection.Dispose();
    foreach (var ind in pop.GetPopulation())
    {
      ind.Dispose();
    }
  }
}


