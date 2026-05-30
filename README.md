# VideoUtility [![Static Badge](https://img.shields.io/badge/download%20-%20program?style=flat&label=v1.0.0)](https://github.com/MoLayth/VideoUtility/releases/tag/v1.0.0)

A lightweight Windows desktop application designed to streamline video transcoding and processing. The application features customizable presets, asynchronous progress tracking, and Windows Shell integration, allowing you to execute hardware-accelerated video operations directly from the Windows Explorer right-click context menu.

## Application Modes
VideoUtility offers two ways to interact with your media, depending on the complexity of your task:

### 1. Basic UI Mode:  
A straightforward graphical interface for standard, everyday video conversions and simple tasks.  
<img src = "VideoUtility/Assets/UI%20Mode%20Screenshot.png"  alt="UI Mode Screenshot" width="400">
### 2. Advanced Command Mode  
A powerful text-based mode that grants you direct access to FFmpeg's extensive capabilities, allowing you to build complex processing pipelines and save them as presets.  
<img src="VideoUtility/Assets/Command%20Mode%20Screenshot.png" alt="Command Mode Screenshot" width="400">

---

## How to Use Advanced Command Mode
To make batch processing and presets easier, VideoUtility manages the input and output file paths behind the scenes. Because of this, we use a specialized syntax to map your arguments to the underlying FFmpeg engine.  
A standard FFmpeg command is typically structured like this:  
**ffmpeg [Input Options] -i [Input File] [Output Options] [Output File]**  
In VideoUtility, you only need to provide the instructions using our custom i-o-e format:  
**i(Input Options) o(Output Options) e(Extension)**
- **i(...):** Arguments applied before the input file (e.g., hardware acceleration flags).
- **o(...):** Arguments applied after the input file (e.g., codecs, filters, quality settings).
- **e(...):** The specific output file extension (e.g., mp3, gif)

***Important Note on Extensions: If your command does not require changing the file container, you can leave the extension bracket empty e(). VideoUtility will automatically inherit the default format from your selected input video.***

---

## Examples
**Example 1:** Extract Audio to MP3

If you want to extract audio, your normal FFmpeg command would look like this:  
`ffmpeg -i input.mp4 -vn -acodec libmp3lame -q:a 2 output.mp3`

In VideoUtility, you simply write:  
`i()o(-vn -acodec libmp3lame -q:a 2)e(mp3)`

**Example 2:** Creating a "Convert to GIF" Preset  
You can save any command as a custom preset. For example, to create a high-quality, scaled GIF preset, you would configure it like this:  
`i()o(-vf "fps=20,scale=1080:-1:flags=lanczos")e(gif)`

![Example GIF](VideoUtility/Assets/V.gif)
