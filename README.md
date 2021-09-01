# GoldSrcSprite
GoldsrcSprite is a simple .NET Standard library to handle GoldSrc engine sprite (.spr) files.

## Quickstart
**Nuget**:  
[![NuGet](https://img.shields.io/nuget/dt/GoldSrcSprite.svg)](https://www.nuget.org/packages/GoldSrcSprite/)  
`Install-Package GoldSrcSprite`

## Usage Example
```csharp
// Load sprite
var sprite = GoldSrcSprite.FromFile("sprite.spr");

// Get bitmap of the first frame
using(var bmp = sprite.Frames[0].GetBitmap()) {
	// Save as bmp
    bmp.Save("sprite.bmp");
}

// Save sprite
sprite.SaveToFile("sprite.spr");
```

## About .spr files
The format itself is flexible, however keep in mind that game engines might not load the most extreme cases.
* A sprite can only use 256 colors with it's own palette
* Can specify how the sprite should be oriented and rendered
* Each sprite can contain multiple frames
* Each frame share the same palette
* Each frame can be different sizes and have a different center origin point

## Further Reading
See the [Project page](https://jpiolho.github.io/GoldSrcSprite/) for more information and examples
