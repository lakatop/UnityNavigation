using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Agent component represented in game
/// Implements IBaseAgent interface
/// </summary>
public abstract class BaseAgent : IBaseAgent
{

  // Class initialization ---------------------------------------------------------

  // Agent body
  /// <summary>
  /// GameObject representing Agent
  /// </summary>
  private GameObject _object = null;
  /// <summary>
  /// Path to material of agent
  /// </summary>
  private string _materialPath = "group1";

  public BaseAgent()
  {
    _object = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _object.GetComponent<CapsuleCollider>().radius = 0.5f;
    _object.AddComponent<NavMeshAgent>();
    _object.GetComponent<NavMeshAgent>().baseOffset = 1f;
    _object.GetComponent<MeshRenderer>().material = Resources.Load<Material>(_materialPath);
    _object.AddComponent<DirectionArrowGizmo>();

    //Create3DArrowIndicator(_object.transform);
  }

  // IBaseAgent interface ---------------------------------------------------------

  /// <inheritdoc cref="IBaseAgent.OnBeforeUpdate"/>
  public abstract void OnBeforeUpdate();
  /// <inheritdoc cref="IBaseAgent.OnAfterUpdate"/>
  public abstract void OnAfterUpdate(Vector2 newPos);
  /// <inheritdoc cref="IBaseAgent.SetDestination(Vector3)"/>
  public abstract void SetDestination(Vector2 des);
  /// <inheritdoc cref="IBaseAgent.id"/>
  public int id { get; set; }
  /// <inheritdoc cref="IBaseAgent.speed"/>
  public float speed { get; set; }
  /// <inheritdoc cref="IBaseAgent._position"/>
  public Vector2 position { get; protected set; }
  /// <inheritdoc cref="IBaseAgent._destination"/>
  public Vector2 destination { get; protected set; }
  /// <inheritdoc cref="IBaseAgent.updateInterval"/>
  public float updateInterval { get; set; }
  /// <inheritdoc cref="IBaseAgent.collisionAlg"/>
  public abstract IBaseCollisionAvoider collisionAlg { get; set; }
  /// <inheritdoc cref="IBaseAgent.pathPlanningAlg"/>
  public abstract IBasePathPlanner pathPlanningAlg { get; set; }
  /// <inheritdoc cref="IBaseAgent.SetPosition(Vector2)"/>
  public void SetPosition(Vector2 pos)
  {
    if (_object.transform.position.y > 1.59f)
    {
      _object.transform.position = new Vector3(pos.x, 1.58f, pos.y);
    }
    var step = speed * Time.deltaTime;
    position = Vector2.MoveTowards(position, pos, step);
    if (_object != null)
    {
      _object.transform.position = new Vector3(position.x, 1.58f, position.y);
      GetComponent<NavMeshAgent>().Warp(_object.transform.position);
    }
  }
  /// <inheritdoc cref="IBaseAgent.SetForward(Vector2)"/>
  public void SetForward(Vector2 forw)
  {
    float singleStep = speed * Time.deltaTime;
    var direction = Vector3.RotateTowards(_object.transform.forward, new Vector3(forw.x, 0, forw.y), singleStep, 0.0f);
    _object.transform.rotation = Quaternion.LookRotation(direction);
  }
  
  // Other methods ----------------------------------------------------------------

  /// <summary>
  /// Set Agents name
  /// </summary>
  /// <param name="name">Agents name.
  /// If empty, Agents name defaults to "Agent<id>"</id></param>
  public void SetName(string name = "")
  {
    if (name == "")
    {
      _object.name = "Agent" + id;
    }
    else
    {
      _object.name = name;
    }
  }

  /// <summary>
  /// Get component attached to agent's GameObject
  /// </summary>
  /// <typeparam name="T">Component type</typeparam>
  /// <returns>Component of type T attached to agent</returns>
  public T GetComponent<T>()
  {
    return _object.GetComponent<T>();
  }

  /// <summary>
  /// Add component to agent's GameObject
  /// </summary>
  /// <typeparam name="T">Component type</typeparam>
  /// <returns>Added component</returns>
  public T AddComponent<T>() where T : MonoBehaviour
  {
    return _object.AddComponent<T>();
  }

  public Vector3 GetPos()
  {
    return _object.transform.position;
  }
}
