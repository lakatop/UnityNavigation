﻿using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Concrete class for collision avoidance
/// Uses third party RVO2 library for all calculations
/// </summary>
public class ORCACollision : IBaseCollisionAvoider
{
  public IBaseAgent agent { get; private set; }
  private RVOAdapter _adapter { get; set; }
  private SimulationManager _simManager { get; set; }
  private Dictionary<int, int> _agentIdToOrcaIDMap { get; set; }

  private readonly float _timeStep = 0.01f;
  private float _lastUpdate = 0.0f;

  public ORCACollision() { }

  public ORCACollision(IBaseAgent agent)
  {
    this.agent = agent;
    _adapter = RVOAdapter.Instance;
    _simManager = SimulationManager.Instance;
    _simManager.RegisterCollisionListener(this);
    _agentIdToOrcaIDMap = new Dictionary<int, int>();
    OnStart();
  }

  public void OnStart()
  {
    _adapter.SetAgentDefaults(15.0f, 10, 5.0f, 5.0f, 0.5f, 8.0f, new RVO.Vector2(0.0f, 0.0f));
    _adapter.SetTimeStep(_timeStep);
  }

  public void Update()
  {
    _lastUpdate += Time.deltaTime;
    if (_lastUpdate < _timeStep)
      return;

    _lastUpdate = 0.0f;

    // Get all agents from simManager and update position (in ORCA simulation) to those which are
    // not of type ORCAAgent - this will ensure correct collision in the next calculation
    foreach (var agent in _simManager.GetAgents())
    {
      if (_agentIdToOrcaIDMap.TryGetValue(agent.id, out var orcaId))
      {
        _adapter.setAgentPosition(orcaId, agent.position);
      }
      else
      {
        _adapter.setAgentPosition(((ORCAAgent)agent)._orcaId, agent.position);
      }
    }

    _adapter.DoStep();
  }

  public void OnAgentAdded(IBaseAgent agent)
  {
    int id = _adapter.AddAgent(agent);
    if(agent is ORCAAgent)
    {
      ((ORCAAgent)agent)._orcaId = id;
    }
    else
    {
      // We need to keep track of agents other that ORCAAgent type
      // and update their postiion before every update step.
      // ORCAAgents are updating their position in ORCA simulation themselves.
      _agentIdToOrcaIDMap.Add(agent.id, id);
    }
  }

  public Vector2 GetAgentPosition(int id)
  {
    return _adapter.GetAgentPosition(id);
  }

  public void SetAgentPreferredVelocity(int id, Vector2 prefVelocity)
  {
    _adapter.SetAgentPrefVelocity(id, prefVelocity);
  }

  public Vector2 GetAgentPreferredVelocity(int id)
  {
    return _adapter.GetAgentPreferredVelocity(id);
  }

  public Vector2 GetAgentVelocity(int id)
  {
    return _adapter.GetAgentVelocity(id);
  }
}