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

See [Sprite File Format](fileformat.md)

## More Examples

### Example #1: Creating a sprite from scratch
The following example will create a "sprite.spr" file with this sprite:  
<div style="text-align:center">
<img alt="Image of Example 1 sprite" src="https://jpiolho.github.io/GoldSrcSprite/images/example1.png" style="border:1px solid black"/>
</div>

```csharp
GoldSrcSprite sprite = new GoldSrcSprite();

// Set some basic settings
sprite.Type = GoldSrcSpriteType.ParallelUpright;
sprite.TextureFormat = GoldSrcSpriteTextureFormat.Normal;
sprite.Synchronization = GoldSrcSpriteSynchronization.Synchronized;

// Create the palette
sprite.Palette = new Color[256];
sprite.Palette[0] = Color.Red;
sprite.Palette[1] = Color.Green;
sprite.Palette[2] = Color.Blue;
sprite.Palette[3] = Color.White;

// Create the frame
var frame = new GoldSrcSpriteFrame(sprite,2,2);

// Frame basic settings
frame.OriginX = 1;
frame.OriginY = 1;

// Actual image data
new byte[]
{
0,1,
2,3
}.CopyTo(frame.Data, 0);

// Add the frame to the sprite
sprite.Frames.Add(frame);

// Recalculate bounding radius. Should call when changing frames sizes
sprite.RecalculateBoundingRadius();

// Save the sprite
sprite.SaveToFile("sprite.spr");
```

### Example #2: Creating a sprite from a bitmap
The following example will create a "sprite.spr" file from the following image. **Note:** The bitmap is in 256 color mode. This example will not work with any other modes.
<div style="text-align:center">
<img alt="Image of Example 2 sprite" src="https://jpiolho.github.io/GoldSrcSprite/images/example2.bmp" style="border:1px solid black"/>
</div>

```csharp
// Load external bmp
using (Bitmap bmp = (Bitmap)Bitmap.FromFile("github.bmp"))
{
	// Create sprite
	var sprite = new GoldSrcSprite();

	// Basic sprite information
	sprite.Type = GoldSrcSpriteType.ParallelUpright;
	sprite.TextureFormat = GoldSrcSpriteTextureFormat.Normal;
	sprite.Synchronization = GoldSrcSpriteSynchronization.Synchronized;


	// Copy the palette
	sprite.Palette = new Color[256];
	bmp.Palette.Entries.CopyTo(sprite.Palette, 0);

	// Create new frame
	var frame = new GoldSrcSpriteFrame(sprite, bmp.Width, bmp.Height);
	sprite.Frames.Add(frame);

	// Center the frame
	frame.OriginX = (int)Math.Round(bmp.Width / 2f);
	frame.OriginY = (int)Math.Round(bmp.Height / 2f);

	// Copy data over from bitmap to the sprite
	var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

	var ptr = data.Scan0;
	var idx = 0;
	for (var i = 0; i < bmp.Height; i++) {
		Marshal.Copy(ptr, frame.Data, idx, data.Stride);
		idx += bmp.Width;
		ptr += Math.Abs(data.Stride);
	}

	bmp.UnlockBits(data);


	// Recalculate bounding radius. Should call when changing frames sizes
	sprite.RecalculateBoundingRadius();

	// Save file
	sprite.SaveToFile("sprite.spr");
}
```
**In-engine result**
<div style="text-align:center">
<img alt="In-engine result of Example 2" src="https://jpiolho.github.io/GoldSrcSprite/images/example2_result.png" style="border:1px solid black"/>
</div>
