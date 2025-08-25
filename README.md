# Unity 2D Top-Down Shooter

A modular 2D top-down shooter game built with Unity, demonstrating clean architecture patterns and efficient game development practices.

## 🎮 **Core Features**

### **Player Controls**
- **Virtual Joystick**: Touch-based movement control for mobile/desktop
- **Keyboard Support**: WASD/Arrow keys as alternative input
- **Smooth Movement**: Physics-based movement with configurable acceleration and drag

### **Combat System**
- **Modular Weapon System**: Interface-based weapon architecture (`IWeapon`)
- **Projectile Weapons**: Configurable damage, fire rate, range, and projectile speed
- **Smart Targeting**: Automatic targeting of nearest enemies(like in archero)
- **Range-Based Combat**: Weapons respect maximum range limitations

### **Enemy AI**
- **Proximity Attacks**: Enemies only attack when player enters their attack radius
- **Pathfinding**: Enemies move towards player using physics-based movement
- **Wave Spawning**: Enemies spawn from outside screen boundaries
- **Dynamic Targeting**: Enemies automatically target the player

### **Performance Optimizations**
- **Object Pooling**: Efficient memory management for projectiles and enemies
- **Automatic Pool Creation**: Pools are created on-demand without manual configuration
- **Resource Recycling**: Objects return to pool instead of being destroyed

### **UI System**
- **Health Bar**: Modular UI with View-Presenter pattern
- **Clean Architecture**: Separation of UI logic from game logic
- **Event-Driven Updates**: Health bars update automatically via events

## 🏗️ **Architecture Highlights**

### **Modular Component System**
- **Unit System**: Base class for all game entities (Player, Enemies)
- **Component-Based Design**: Health, Movement, Weapons as separate components
- **Interface-Driven**: `IMovement`, `IWeapon`, `IUnitComponent` for flexibility

### **Event-Driven Communication**
- **Health Events**: `EventHealthChanged`, `EventDeath` for reactive systems
- **Weapon Events**: `EventFire`, `EventAmmoChanged` for UI updates
- **Pool Events**: `EventObjectSpawned`, `EventObjectDespawned` for monitoring

### **Singleton Management**
- **EnemyManager**: Centralized enemy tracking and targeting
- **ObjectPool**: Global object pooling system
- **WorldBounds**: Screen boundary management

## 🎯 **Technical Implementation**

### **Movement System**
```csharp
// Physics-based movement with configurable parameters
public interface IMovement
{
    void Move(Vector2 direction);
    float MaxSpeed { get; }
    Vector2 Velocity { get; }
}
```

### **Weapon System**
```csharp
// Modular weapon interface
public interface IWeapon
{
    bool CanFire(Vector2 targetPos);
    void Fire(Vector2 firePoint, Vector2 targetPos, Unit owner);
    float Range { get; }
    float Damage { get; }
}
```

### **Object Pooling**
```csharp
// Automatic pool creation and management
ObjectPool.Instance.Spawn<Projectile>(prefab, position);
ObjectPool.Instance.Despawn(gameObject);
```

## 🚀 **Key Design Patterns**

- **MVP**: UI separation with View-Presenter pattern
- **Object Pool**: Memory-efficient object recycling
- **Component Pattern**: Modular entity design
- **Event System**: Loose coupling between systems
- **Interface Segregation**: Clean, testable code structure

## 🎮 **Gameplay**

The player controls a character in a top-down view, using a virtual joystick or keyboard to move. Enemies spawn from outside the screen and approach the player. The player automatically targets and shoots the nearest enemy. Enemies only attack when they get close enough to the player. The game features a health system with visual health bars.

This project demonstrates clean code architecture, performance optimization, and modular game design suitable for scalable game development.
