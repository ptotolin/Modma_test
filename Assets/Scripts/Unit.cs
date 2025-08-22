using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Events
    public event Action<Unit> EventDestroyed;
    public event Action<Unit> EventInitialized;
    
    [Header("Unit Info")]
    [SerializeField] private string unitName = "Unit";
    [SerializeField] private string unitID;
    
    // Components cache
    private Dictionary<System.Type, IUnitComponent> components = new Dictionary<System.Type, IUnitComponent>();
    private List<IUnitComponent> allComponents = new List<IUnitComponent>();
    
    // Properties
    public string Name => unitName;
    public string ID => unitID;
    public Vector3 Position => transform.position;
    public bool IsAlive { get; private set; } = true;
    
    protected virtual void Awake()
    {
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(unitID)) {
            unitID = System.Guid.NewGuid().ToString();
        }
        
        InitializeComponents();
    }
    
    private void Start()
    {
        EventInitialized?.Invoke(this);
    }
    
    private void InitializeComponents()
    {
        // Find all unit components on this GameObject and cache them
        IUnitComponent[] unitComponents = GetComponents<IUnitComponent>();
        
        foreach (var component in unitComponents) {
            System.Type componentType = component.GetType();
            
            if (!components.ContainsKey(componentType)) {
                components[componentType] = component;
                allComponents.Add(component);
                
                // Initialize component
                component.Initialize(this);
            }
        }
    }
    
    // Generic method to get component
    public T GetUnitComponent<T>() where T : class, IUnitComponent
    {
        if (components.TryGetValue(typeof(T), out IUnitComponent component)) {
            return component as T;
        }
        return null;
    }
    
    // Check if unit has specific component
    public bool HasComponent<T>() where T : class, IUnitComponent
    {
        return components.ContainsKey(typeof(T));
    }
    
    // Get movement component (works with both physics and non-physics)
    public IMovement GetMovement()
    {
        // Try to find any component that implements IMovement
        foreach (var component in allComponents) {
            if (component is IMovement movement) {
                return movement;
            }
        }
        return null;
    }
    
    // Check if unit can move
    public bool CanMove()
    {
        return GetMovement() != null && IsAlive;
    }
    
    // Unit destruction
    public void DestroyUnit()
    {
        if (!IsAlive) return;
        
        IsAlive = false;
        
        EventDestroyed?.Invoke(this);
        
        // Destroy GameObject after a frame to allow cleanup
        Destroy(gameObject);
    }
}