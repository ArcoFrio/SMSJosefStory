<p align="center">A BepInEx mod for <b>Starmaker Story</b> that replaces every character in the game with Josef.</p>

<p align="center">Not most characters. Every character.</p>

---

## What it actually does

Starmaker Story has a large cast. This mod looks at that cast and asks one question: what if it didn't?

At runtime, `SMSJosefStory` reaches into the loaded Unity scene and swaps out character art wherever it finds it — dialogue busts, CG scenes, background NPCs, minigame fighters, the main menu lineup. It also patches the dialogue system so every speaker is named Josef, every name inside the text is rewritten to Josef, and the typewriter voice blips become a single custom `Hmmm.ogg`.

The game itself is never modified. No files are patched, nothing is repacked, and your install stays vanilla. The mod just gets to the scene first and rearranges things while the game isn't looking.

## How it works

The mod is a [BepInEx](https://github.com/BepInEx/BepInEx) plugin — a DLL dropped in `BepInEx/plugins/` that gets loaded into the game's Mono runtime at startup. From there it has two ways to change things.

**1. Direct scene manipulation.** [`Core.cs`](SMSJosefStory/Core.cs) subscribes to `SceneManager.sceneLoaded` and polls in `Update()`. When the scene named `CoreGameScene` appears, it caches the top-level hierarchy roots by name:

```csharp
bustManager = GameObject.Find("2_Bust_Manager").transform;
level       = GameObject.Find("5_Levels").transform;
mainCanvas  = GameObject.Find("9_MainCanvas").transform;
```

Then it runs the three sweeps: `JosefifyAllBusts()`, `JosefifyAllScenes()`, `JosefifyAllNPCs()`. The `GameStart` scene gets `JosefifyMainMenu()` instead.

This is all string-based hierarchy walking, which means it is completely at the mercy of the game's object names. A patch that renames `2_Bust_Manager` breaks this instantly and silently. That's the tradeoff for not touching game files.

**2. Harmony patches.** For things that regenerate constantly — text, audio — swapping the object once doesn't stick, so [HarmonyX](https://github.com/BepInEx/HarmonyX) hooks the methods themselves. There are exactly three, all at the bottom of `Core.cs`, all postfixes:

| Patched method | What the postfix does |
|---|---|
| `Actor.GetName` | Returns `"Josef"` whenever a dialogue is active |
| `NodeText.Get` | Regex-replaces ~54 character names in the line with `Josef` |
| `Typewriter.GetGibberish` | Returns the custom voice clip instead of the character's |

The name list in `Patch_NodeTextGet` is sorted longest-first on purpose. `Sam` sits at the very end, because matching it before `Samantha` turns her into "Josefantha" — which is funny once and then it's just a bug.

**Assets live inside the DLL.** There's no assets folder to install and no loose PNGs to lose. Every texture is an `EmbeddedResource` compiled into the assembly and pulled out by name:

```csharp
public static Texture2D LoadEmbeddedTexture(string resourcePath, bool pointFilter = true)
{
    string fullName = "SMSJosefStory." + resourcePath;
    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
    // ... read bytes, Texture2D.LoadImage, done
}
```

The audio is the exception. Unity can't decode OGG straight from a byte array, so `Hmmm.ogg` gets written to the temp folder and loaded back through `UnityWebRequestMultimedia.GetAudioClip` with a `file:///` URI. Slightly cursed, works fine.

---

## Character sprite replacement

This is the part worth reading if you're here to learn something. Swapping a sprite in this game is *almost* a one-liner, and the gap between "almost" and "actually" is where the interesting bits are.

### A bust is not one sprite

In `2_Bust_Manager`, each character is a GameObject whose visible art is split across children:

```
Anna_YellowSexy
└── MBase1  (or D1Base)        ← SpriteRenderer: the body
    ├── Blink                  ← SpriteRenderer: closed eyes
    ├── Mouth
    │   ├── 1  2  3  4         ← SpriteRenderers: lipsync frames
    └── Expressions
        └── Happy  Angry  Sad  Flirty
```

Replace only `MBase1` and you get Josef's body wearing someone else's eyes and mouth, which is a horror game. So `JosefifyAllBusts()` reads Josef's own bust once, caches his blink/mouth/expression sprites, and copies them onto every bust it converts. Different characters use different base-child names, hence the fallback:

```csharp
Transform mBase = child.Find("MBase1");
if (mBase == null) mBase = child.Find("D1Base");
if (mBase == null) continue;
```

### The mask will humble you

This is the one that costs people an afternoon. The game's bust shader uses a **second texture** on a `_MaskTex` property for the jiggle/deform effect. It's still bound to the *original* character's silhouette. Assign a new sprite and forget the mask, and your art gets deformed against the shape of whoever used to be standing there.

```csharp
sr.sprite = chosen;
sr.material.SetTexture("_MaskTex", sharedMask);   // not optional
```

Every Josef texture in this mod ships with a matching `...Mask.png` for exactly this reason.

### SpriteRenderer and UI Image do not behave the same

Genuinely the sharpest edge in the whole codebase, and it isn't obvious from reading either call site.

On a `SpriteRenderer`, touching `.material` makes Unity silently hand you a **private clone** of the material. You can call `SetTexture` and only that one renderer changes.

On a UI `Image`, `.material` is the **shared** material. Call `SetTexture` on it and you've just re-masked every UI element in the game that uses it. So the main menu path clones by hand first:

```csharp
charImg.material = new Material(charImg.material);   // clone, THEN mutate
charImg.material.SetTexture("_MaskTex", maskTex);
```

Same-looking line, wildly different blast radius.

### Variety, cheaply

Eight Josef textures, picked at random per bust, with a coin-flip horizontal mirror so a room full of Josefs doesn't look copy-pasted:

```csharp
if (UnityEngine.Random.value > 0.5f)
{
    Vector3 scale = mBase.localScale;
    scale.x = -Mathf.Abs(scale.x);
    mBase.localScale = scale;
}
```

Flipping the parent flips the sprite and its mask together, so they stay aligned. Four characters (Adrian, Alice, Anna, Charlotte) opt out via a `josefExceptions` dictionary and get purpose-drawn art instead, matched by substring so every outfit variant of a character is caught by one entry.

---

## Tutorial: build your own sprite replacement mod

Say you want to replace exactly one character's bust. Here's the whole thing.

### 1. Set the project up

Target **.NET Framework 4.8**, output type **Library**. Reference the DLLs in [`Libraries/`](Libraries/) — at minimum `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp.dll`, and the `UnityEngine.*Module` DLLs you touch. Point `OutputPath` at your game's `BepInEx/plugins/` folder so every build deploys itself.

### 2. Add your PNG as an embedded resource

Drop it in the project, then in the `.csproj`:

```xml
<EmbeddedResource Include="Textures\Bust\MyChar.png" />
<EmbeddedResource Include="Textures\Bust\MyCharMask.png" />
```

(In Visual Studio: right-click the file → Properties → Build Action → **Embedded Resource**.)

The runtime name is your root namespace plus the path with backslashes turned into dots. `Textures\Bust\MyChar.png` becomes `YourMod.Textures.Bust.MyChar.png`. Get this wrong and you get a null stream and a very quiet failure, so log it.

### 3. Find your target's hierarchy path

You cannot guess these. Use the scene dumps in [`Reference Only (DO NOT MODIFY)/`](SMSJosefStory/Reference%20Only%20%28DO%20NOT%20MODIFY%29/) — `InitialHierarchy_CoreGameScene_1.8.txt` is the entire object tree of the main scene, and it is the single most useful file in this repo. Ctrl-F for a character name and read off the path. [`Debugging.cs`](SMSJosefStory/Debugging.cs) has reflection-based printers for dumping conditions, triggers, and dialogue trees at runtime when the static dump isn't enough.

### 4. Write the plugin

```csharp
using BepInEx;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMod
{
    [BepInPlugin("you.starmakerstory.mymod", "My Mod", "1.0.0")]
    public class MyPlugin : BaseUnityPlugin
    {
        private bool applied;

        public void Awake() => SceneManager.sceneLoaded += (s, m) => applied = false;

        public void Update()
        {
            if (applied || SceneManager.GetActiveScene().name != "CoreGameScene") return;

            var bustManager = GameObject.Find("2_Bust_Manager");
            if (bustManager == null) return;   // scene not built yet, try next frame

            var target = bustManager.transform.Find("Anna_YellowSexy");
            var mBase  = target?.Find("MBase1") ?? target?.Find("D1Base");
            var sr     = mBase?.GetComponent<SpriteRenderer>();
            if (sr == null) { applied = true; return; }

            sr.sprite = LoadSprite("MyMod.Textures.Bust.MyChar.png");
            sr.material.SetTexture("_MaskTex", LoadTexture("MyMod.Textures.Bust.MyCharMask.png"));

            applied = true;
            Logger.LogInfo("Replaced Anna's bust");
        }

        private static Texture2D LoadTexture(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (stream == null) { Debug.LogError($"Missing resource: {name}"); return null; }
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(data);
                tex.filterMode = FilterMode.Point;
                return tex;
            }
        }

        private static Sprite LoadSprite(string name)
        {
            var tex = LoadTexture(name);
            return tex == null ? null
                : Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
```

The `applied` flag matters. `Update()` runs every frame, and re-decoding PNGs sixty times a second is a great way to make a slideshow.

### 5. Run it

Build, launch, and check `BepInEx/LogOutput.log`. If the sprite is missing, the resource name is wrong. If it's there but warped, you skipped the mask. If nothing happened at all, the scene hadn't finished loading — which is why the code above returns and retries instead of giving up.

### Gotchas worth internalizing

- Match your PNG's dimensions to the original, or the pivot math drifts and your character floats.
- Sprites are created with a centered pivot `(0.5f, 0.5f)`; the game's originals may not be. Watch for vertical offset.
- `FilterMode.Point` keeps pixel art crisp. Use bilinear for smooth art.
- Anything the game re-instantiates later (dialogue text, pooled UI) needs a Harmony patch, not a one-time assignment.

---

## Building

```bash
git clone https://github.com/ArcoFrio/SMSJosefStory.git
```

Open `SMSJosefStory.sln` in Visual Studio and build. Set the Debug `OutputPath` to your own `BepInEx/plugins/` folder first — the one committed in the `.csproj` is a local path and definitely not yours.

Requires a Starmaker Story install with BepInEx set up. Game assemblies are in `Libraries/` and are needed to compile against.

## Repo layout

| Path | What's in it |
|---|---|
| `SMSJosefStory/Core.cs` | The whole mod: sweeps, loaders, Harmony patches |
| `SMSJosefStory/Debugging.cs` | Runtime reflection dumpers for reverse-engineering the game |
| `SMSJosefStory/Textures/` | Embedded art — busts, masks, menu, scenes |
| `SMSJosefStory/Audio/` | `Hmmm.ogg` |
| `SMSJosefStory/Reference Only (DO NOT MODIFY)/` | Decompiled game classes + full scene hierarchy dumps |
| `Libraries/` | Game and Unity assemblies to compile against |

The `Reference Only` folder is exactly what the name says — it's there so you can read how the game works and so your IDE can resolve types. Nothing in it is compiled into the mod.
