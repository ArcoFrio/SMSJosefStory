# Reference Classes for SMSDiagDebug Mod

This folder is for storing additional source code classes that you want to reference in the mod development.

## How to Add Reference Classes

1. **Create a new .cs file** in this folder with a descriptive name
2. **Add the class code** you want to reference
3. **Update the project file** to include the new reference if needed
4. **Reference the classes** in your main mod files

## Example Structure

```
ReferenceClasses/
├── README.md (this file)
├── DialogueManager.cs
├── SpriteRenderer.cs
├── RoomTalkComponent.cs
└── CGManager.cs
```

## Usage in Main Mod Files

You can reference these classes in your main mod files like this:

```csharp
// In Core.cs, Debugging.cs, or CGLogger.cs
using SMSDiagDebug.ReferenceClasses;

// Then use the classes as needed
```

## Notes

- These classes should be for reference only, not for direct compilation
- They help understand the game's class structure
- Use them to understand field names, method signatures, and class hierarchies
- The actual functionality will be implemented using reflection in the main mod files 