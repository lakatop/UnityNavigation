﻿using Unity.Collections;
using UnityEngine;

public class GeneticAlgorithmDirector
{
  public GeneticAlgorithmDirector() { }

  public void MakeBasicGA(BasicGeneticAlgorithmBuilder builder, BaseAgent agent)
  {
    // Set crossover
    builder.SetCrossover(new BasicCrossOperator());

    // Set fitness
    var fitness = new BasicFitnessFunction();
    fitness.SetResources(new System.Collections.Generic.List<object>
    {
      agent.position,
      agent.destination,
      0.5f,
      agent.id,
      SimulationManager.Instance.GetQuadTree()
    });
    builder.SetFitness(fitness);

    // Set mutation
    var mutation = new BasicMutationOperator();
    mutation.SetResources(new System.Collections.Generic.List<object>
    {
      agent.speed,
      Time.deltaTime
    });
    builder.SetMutation(mutation);

    // Set selection
    builder.SetSelection(new BasicSelectionFunction());

    // Set population size and iterations
    var _gaAlg = builder.GetResult();
    _gaAlg.populationSize = 30;
    _gaAlg.iterations = 10;
    _gaAlg.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position
    });
  }

  public IGeneticAlgorithmParallel<BasicIndividualStruct> MakeBasicGAParallel (BaseAgent agent)
  {
    var ga = new BasicGeneticAlgorithmParallel();
    int populationSize = 10;
    int iterations = 50;
    int pathSize = 10;
    float maxAcc = 1f;

    // Set crossover
    var offsprings = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    for (int i = 0; i < populationSize; i++)
    {
      var element = offsprings[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      offsprings[i] = element;
    }
    ga.cross = new MeanCrossOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      offsprings = offsprings,
      pathSize = pathSize,
      iterations = iterations,
    };

    // Set mutation
    ga.mutation = new BasicMutationOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      _agentSpeed = agent.speed,
      _updateInterval = SimulationManager.Instance._agentUpdateInterval,
      _rotationRange = 15, 
    };

    // Set fitnesses
    ga.collisionFitness = new FitnessCollisionParallel()
    {
      _startPosition = agent.position,
      _destination = agent.destination,
      _agentRadius = 0.5f,
      _agentIndex = agent.id,
      _quadTree = SimulationManager.Instance.GetQuadTree(),
      _forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.6f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance._agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.endDistanceFitness = new FitnessEndDistanceParallel()
    {
      _startPosition = agent.position,
      _destination = agent.destination,
      _forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.35f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance._agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.jerkFitness = new FitnessJerkCostParallel()
    {
      _startPosition = agent.position,
      _forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.05f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance._agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };

    // Set ranking
    ga.ranking = new WeightedSumRanking()
    {
      resultingFitnesses = new NativeArray<float>(populationSize, Allocator.TempJob)
    };

    // Set selection
    ga.selection = new NegativeSelectionParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
    };

    // Set initialization
    ga.popInitialization = new KineticFriendlyInitialization()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      populationSize = populationSize,
      agentSpeed = agent.speed,
      updateInterval = SimulationManager.Instance._agentUpdateInterval,
      pathSize = pathSize,
      startPosition = agent.position,
      forward = agent.GetForward(),
    };

    //ga.popInitialization = new DebugInitialization()
    //{
    //  startPosition = agent.position,
    //  forward = agent.GetForward(),
    //  previousVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude
    //};

    // Set logger
    var topIndividuals = new NativeArray<BasicIndividualStruct>(iterations + 1, Allocator.TempJob);
    for (int i = 0; i < topIndividuals.Length; i++)
    {
      var element = topIndividuals[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      topIndividuals[i] = element;
    }
    ga.logger = new FitnessEvaluationLogger()
    {
      _topIndividuals = topIndividuals,
    };

    ga.populationSize = populationSize;
    ga.iterations = iterations;

    // Initialize population
    var population = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    for (int i = 0; i < populationSize; i++)
    {
      var element = population[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      population[i] = element;
    } 
    ga.pop = new NativeBasicPopulation()
    {
      _population = population
    };

    // Set population drawer
    ga.popDrawer = new NativeBasicPopulationDrawer()
    {
      forward = agent.GetForward(),
      startPosition = agent.position,
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime))
  };

    ga._winner = new NativeArray<Vector2>(1, Allocator.TempJob);
    ga._rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime));
    ga.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position,
      agent.GetForward(),
      ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc,
      SimulationManager.Instance._agentUpdateInterval,
});

    return ga;
  }
}
