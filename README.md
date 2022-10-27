![DefaultEcs](https://github.com/Doraku/DefaultEcs/blob/master/image/DefaultEcsLogo.png)
This project provides a set of analyzers to enhance the Visual Studio experience when working on a DefaultEcs project by adding diagnostics or by removing general C# diagnostics that do not apply.  
The main repo for DefaultEcs is [here](https://github.com/Doraku/DefaultEcs/), this repo is specific to the analyzer.

[Here](https://github.com/Doraku/DefaultEcs.Analyzer/blob/master/documentation/index.md) is the list of analyzers and suppressors defined in this project.

[![NuGet](https://buildstats.info/nuget/DefaultEcs.Analyzer)](https://www.nuget.org/packages/DefaultEcs.Analyzer)
[![Coverage Status](https://coveralls.io/repos/github/Doraku/DefaultEcs.Analyzer/badge.svg?branch=master)](https://coveralls.io/github/Doraku/DefaultEcs.Analyzer?branch=master)
![continuous integration status](https://github.com/doraku/defaultEcs.analyzer/workflows/continuous%20integration/badge.svg)
[![preview package](https://img.shields.io/badge/preview-package-blue?style=flat&logo=github)](https://github.com/Doraku/DefaultEcs.Analyzer/packages/502961)
[![Join the chat at https://gitter.im/Doraku/DefaultEcs](https://badges.gitter.im/Doraku/DefaultEcs.svg)](https://gitter.im/Doraku/DefaultEcs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

- [Release notes](./documentation/RELEASENOTES.md 'Release notes')
<a/>

- [Requirement](#Requirement)
- [Versioning](#Versioning)
- [Code generation](#Code)

<a name='Requirement'></a>
# Requirement

<a name='Versioning'></a>
# Versioning
This is the current strategy used to version DefaultEcs.Analyzer: v0.major.minor
- 0: DefaultEcs is still in heavy development and although a lot of care is given to not break the current api, it can still happen
- major: incremented when there is a breaking change (reset minor number)
- minor: incremented when there is a new feature or a bug fix

<a name='Code'></a>
# Code generation
Referencing DefaultEcs.Analyzer in your project gives you access to some attributes to automatically generate code for you:

## UpdateAttribute
This attribute should be used on a void method inside a type inheriting `AEntitySetSystem` or `AEntityMultiMapSystem`.
```csharp
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [Update]
        private static void Update(float time, ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }
    }
```

The containing type need to be declared as partial for the code generation to work. Your system component composition will be deduced from the parameters of the method. In the example above, it would be `world.GetEntities().With<Position>().With<LinearVelocity>().AsSet()`.

Parameters can be requested as `ref` or `in` depending on whether you want to change their value or not.  
If a parameter has the same type as the generic type of the system, it is the state of `ISystem.Update` method that is passed and not a component (`float` in the example above).

You can request the `Entity` as a parameter too, this is usefull of you want to use it to record action on a `EntityCommandRecorder`.
```csharp
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [Update]
        private static void Update(float time, in Entity entity, ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }
    }
```

If you need to define more exotic rules, all attributes (`WithAttribute`, `WithoutAttribute`, `DisabledAttribute`, ...) that you normally define on the parent type will also be used.
```csharp
    [Without(typeof(bool))]
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [Update]
        private static void Update(float time, ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }

        [WithPredicate]
        private bool Filter(in int _) => true;
    }
```

This would generate `world.GetEntities().With<Position>().With<LinearVelocity>().Without<bool>().With(Filter).AsSet()` for you.

If for some reason you need to define a constructor, you can use the factory overcharge of the base type with the produced `CreateEntityContainer` method.
```csharp
    public sealed partial class PlayerSystem : AEntitySetSystem<float>
    {
        public PlayerSystem(World world)
            : base(world, CreateEntityContainer, null, 0)
        { }

        [Update]
        private static void Update(...)
        {
            ...
        }
    }
```

## UseBufferAttribute
If your system need to use a buffer to process entities (no multithreading and need to do composition change operation), you can add this attribute with the `UpdateAttribute`.
```csharp
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [Update, UseBuffer]
        private static void Update(float time, ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }
    }
```

The correct constructors will be generated for you.

## ConstructorParameterAttribute
If you need some fields or properties to be set in the constructor of your system but still want it to be done for you, you can decorate them with the attribute.
```csharp
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [ConstructorParameter]
        private readonly int _myField;

        [Update]
        private static void Update(float time, in Entity entity, ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }
    }
```

The generated constructors for the above example will request an extra `int myField` parameter.

## AddedAttribute, ChangedAttribute
If you need reactive rules on the component composition, you can do so by decorating the parameter of your `Update` method.
```csharp
    public sealed partial class MovementSystem : AEntitySetSystem<float>
    {
        [Update]
        private static void Update(float time, [Added][Changed] ref Position position, in LinearVelocity linearVelocity)
        {
            position.Value += linearVelocity.Value * time;
        }
    }
```

This would generate `world.GetEntities().WhenAdded<Position>().WhenChanged<Position>().With<LinearVelocity>().AsSet()`.