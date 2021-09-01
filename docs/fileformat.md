# GoldSrc Sprite File Format

## Index
* [File Structure](#file-structure)
* [Credits](#credits)

## File Structure
A sprite file is composed of 3 sections.

* Header
* Palette
* Frames

All values are little-endian.

### Header

|Offset  |Size  |Type  |Description|Reference|
|:------:|:----:|:----:|-----------|---------|
|0       |4     |char  |File Identifier. Should always be 'IDSP'|
|4       |4     |int   |Sprite file version. Should always be 0x02|
|8       |4     |int   |Sprite type|
|12      |4     |int   |Texture format|       
|16      |4     |float |Bounding radius|
|20      |4     |int   |Maximum frame width|
|24      |4     |int   |Maximum frame height|
|28      |4     |int   |Number of frames|number_of_frames|
|32      |4     |float |Beam length|
|36      |4     |int   |Synchronization type|

### Palette
Starting offset: `40`

|Offset|Size|Type|Description|Reference|
|:----:|:--:|:--:|-----------|---------|
|0     |2   |short|Size of palette. Should always be 256 however|size_of_palette|
|4     |...|byte|Array of RGB values for the palette. Total size: `Palette.size_of_palette * 3`|

### Frames
Starting offset: `after palette` or `after last frame`  
Number of frames sections: `Header.number_of_frames`

|Offset|Size|Type|Description|Reference|
|:----:|:--:|:--:|-----------|---------|
|0     |4   |int |Frame group|
|4     |4   |int |Frame origin X|
|8     |4   |int |Frame origin Y|
|12    |4   |int |Frame width|frame_width|
|16    |4   |int |Frame height|frame_height|
|20    |... |byte|Array of image data. Each byte refers to a palette index. Total size: `Frame.frame_width * Frame.frame_height`|

## Credits
Source: <http://yuraj.ucoz.com/half-life-formats.pdf>