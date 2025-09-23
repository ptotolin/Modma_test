using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour, IResettable, IHasMaterial
{
    // Events
    public event Action<Unit> EventInitialized;
    
    [Header("Unit Info")]
    [SerializeField] private string unitName = "Unit";
    [SerializeField] private string unitID;

    [Header("Material")] 
    [SerializeField] private GameObjectPhysicalMaterial physicalMaterial;
    
    // Components cache
    private Dictionary<System.Type, IUnitComponent> components = new Dictionary<System.Type, IUnitComponent>();
    private List<IUnitComponent> allComponents = new List<IUnitComponent>();
    
    // Presenter
    private UnitPresenter unitPresenter;
    
    // Properties
    public string Name => unitName;
    public string ID => unitID;
    public Vector3 Position => transform.position;
    public bool IsAlive { get; private set; } = true;
    public GameObjectPhysicalMaterial Material => physicalMaterial;
    
    protected virtual void Awake()
    {
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(unitID)) {
            unitID = System.Guid.NewGuid().ToString();
        }
        
        InitializeComponents();
    }
    
    protected virtual void Start()
    {
        unitPresenter = new UnitPresenter(this);
        EventInitialized?.Invoke(this);
    }

    private void OnDestroy()
    {
        unitPresenter.Dispose();
    }
    
    public virtual void Reset()
    {
        IResettable[] unitComponents = GetComponents<IUnitComponent>();
        foreach (var component in unitComponents) {
            component.Reset();
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
        foreach (var component in allComponents) {
            if (component is IMovement movement) {
                return movement;
            }
        }
        return null;
    }
    
    public bool CanMove()
    {
        return GetMovement() != null && IsAlive;
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
                
                component.Initialize(this);
            }
        }
    }
}