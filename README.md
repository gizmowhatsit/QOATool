# QOATool

A C# implementation of the **Quite OK Audio (QOA)** format.
This tool allows for encoding WAV files into QOA and decoding QOA files back into WAV.

## Credits & Acknowledgements

This project is built upon the work of the QOA community:

- **Format Specification**: [qoaformat.org](https://qoaformat.org/)  
  The QOA format is a fast, lossy audio compression format with a very small reference implementation.

- **Transpilation Source**: [github.com/pfusik/qoa-fu](https://github.com/pfusik/qoa-fu)  
  The core logic in `QOA.cs` is based on the transpiled source code from Piotr Fusik's `qoa-fu` repository.

## Usage

This project is a command-line interface (CLI) tool.

### Convert WAV to QOA

Encodes a 16-bit PCM WAV file into a QOA file.

```bash
QOATool wav2qoa <input.wav> <output.qoa> [options]
```

**Options:**

| Option         | Description                               |
| -------------- | ----------------------------------------- |
| `-m`, `--mono` | Downmix the audio to mono before encoding |

**Examples:**

```bash
# Standard conversion
QOATool wav2qoa music.wav music.qoa

# Convert to mono
QOATool wav2qoa stereo.wav mono.qoa -m
```

### Convert QOA to WAV

Decodes a QOA file back into a 16-bit PCM WAV file.

```bash
QOATool qoa2wav <input.qoa> <output.wav>
```

**Example:**

```bash
QOATool qoa2wav music.qoa music_decoded.wav
```

## Releases

Unsigned binary releases are available for your convenience, but you are encouraged to build this project from source.

## License

The `QOA.cs` file is based on code licensed under the MIT License. See the source files for specific license headers.

The additional code in this repository is licensed under the MIT License. See the included license file for details.
