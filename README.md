# VU.Server

This project is a wrapper around the original [Venice Unleashed](https://veniceunleashed.net) server and provides additional functionality. This application utilizes .NET Core 3.1 and currently only works on Windows.

## Functionality

- [x] Automatic server restart on crash
- [x] Support for console input (through RCON)
- [ ] Server state information
  - [ ] FPS
  - [x] Player Count
  - [x] Game Mode
  - [x] Map
- [ ] System information
  - [ ] Core frequency
  - [ ] Core load
  - [ ] Memory usage
  - [ ] Network usage
- [ ] Process Affinity
- [ ] Logging of system and server information to CSV files for analysis
  - [ ] CPU Core Frequency
  - [ ] CPU Core Load
  - [ ] Memory Usage

## Usage

As this application uses .NET Core 3.1, the runtime **must** be installed on your system. You can follow your platform-specific instructions [here](https://dotnet.microsoft.com/download/dotnet-core/3.1) on Microsoft's website.

1. Download a release of VU.Server, or build it yourself using the instructions below.
2. Extract the `vuserver-win64.zip`.
3. Run `VU.Server.exe` with the following flags.

| Argument                  | Default                                  | Description                                                                              |
|---------------------------|------------------------------------------|------------------------------------------------------------------------------------------|
| `--path`                  | (Required)                               | Path to Venice Unleashed
| `--instance`              | (Required)                               | Path to VU server instance
| `--gamepath`              |                                          | Path to Battlefield 3 game files
| `--gameport`              | 25200                                    | Set VU server game port
| `--harmonyport`           | 7948                                     | Set VU server harmony port
| `--remoteport`            | 47200                                    | Set VU server remote port
| `--frequency`             | Default30 [ Default30, High60, High120 ] | Set VU server frequency
| `--unlisted`              | false                                    | Prevent VU server from being visible on the server list
| `--noupdate`              | false                                    | Prevent automatic updates from restarting the server
| `--highresterrain`        | false                                    | Enables high resolution terrain. Useful for extending maps beyond their original play area
| `--disableterraininterop` | false                                    | Disables interpolation between different terrain LODs
| `--skipchecksum`          | false                                    | Disables level checksum validation on client connection
| `--perftrace`             | false                                    | Prevent automatic updates from restarting the server
| `--env`                   | prod [ prod, dev ]                       | Specifies the Zeus environment to connect to. Defaults to prod
| `--tracedc`               | false                                    | Traces DataContainer usage in VEXT and prints any dangling DCs during level destruction
| `--trace`                 | false                                    | Enables verbose logging

Example: `./VU.Server.exe --path "C:\Venice Unleashed\" --instance "C:\VU_Server1"`

## Building

To build the application, you need the .NET Core 3.1 SDK. You can get it [here](https://dotnet.microsoft.com/download/dotnet-core/3.1) on Microsoft's website.

1. Clone the repository recursively using `git clone --recusive https://github.com/Imposter/VU.Server`
2. Run `dotnet restore` when in the repository root directory
3. Run `dotnet build` to build the application

## Disclaimer

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
OR OTHER DEALINGS IN THE SOFTWARE.

#
Copyright (C) 2020 Imposter and other contributors. All Rights Reserved.
