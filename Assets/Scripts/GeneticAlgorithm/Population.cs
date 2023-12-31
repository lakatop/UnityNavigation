﻿using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public class BasicIndividual
{
  public BasicIndividual()
  {
    fitness = 0f;
    path = new List<float2>();
  }

  public float fitness { get; set; }
  public List<float2> path { get; set; }
}

public struct BasicIndividualStruct
{
  public void Initialize(int length, Allocator allocator)
  {
    fitness = 0f;
    path = new UnsafeList<float2>(length, allocator);
  }

  public void Dispose()
  {
    path.Dispose();
  }

  public float fitness;
  public UnsafeList<float2> path;
}

public class BasicPopulation : IPopulation<BasicIndividual>
{
  public BasicPopulation()
  {
    _population = new List<BasicIndividual>();
  }

  public BasicIndividual[] GetPopulation()
  {
    return _population.ToArray();
  }

  public void SetPopulation(BasicIndividual[] population)
  {
    _population = new List<BasicIndividual>(population);
  }

  private List<BasicIndividual> _population { get; set; }
}

public struct NativeBasicPopulation : IParallelPopulation<BasicIndividualStruct>
{
  public NativeArray<BasicIndividualStruct> GetPopulation()
  {
    return _population;
  }

  public void SetPopulation(NativeArray<BasicIndividualStruct> population)
  {
    _population = population;
  }

  public void SetIndividual(BasicIndividualStruct individual, int index)
  {
    _population[index] = individual;
  }

  public void Dispose()
  {
    foreach(var individual in _population)
    {
      individual.Dispose();
    }
  }

  public NativeArray<BasicIndividualStruct> _population;
}

public struct BasicIndividualSortDescending : IComparer<BasicIndividualStruct>
{
  public int Compare(BasicIndividualStruct x, BasicIndividualStruct y)
  {
    if (x.fitness < y.fitness)
    {
      return 1;
    }

    if(x.fitness > y.fitness)
    {
      return -1;
    }

    return 0;
  }
}