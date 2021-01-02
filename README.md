# VU.Server

This project is a wrapper around the original [Venice Unleashed](https://veniceunleashed.net) server and provides additional functionality. This application utilizes .NET Core 3.1 and works on both Windows and Linux.

## Functionality

- [x] Automatic server restart on crash
- [x] Support for console input (through RCON)
- [ ] Server state information
  - [ ] FPS
  - [ ] Player Count
  - [ ] Game Mode
  - [ ] Map
- [ ] System information
  - [ ] Core frequency
  - [ ] Core load
  - [ ] Memory usage
  - [ ] Network usage

## Usage

As this application uses .NET Core 3.1, the runtime **must** be installed on your system. You can follow your platform-specific instructions [here](https://dotnet.microsoft.com/download/dotnet-core/3.1) on Microsoft's website.

1. Download a release of VU.Server, or build it yourself using the instructions below.
2. Extract the `vuserver-win64.zip` or `vuserver-linux64.zip` to a folder
3. Run `VU.Server.exe` (Windows) or `VU.Server` (Linux) with the flags you would when running the VU server directly with the addition of the `-vupath` argument, pointing to the installation directory of VU. This argument does not have to be provided if you extracted VU.Server in the VU directory.

Example: `./VU.Server.exe -vupath "C:\Venice Unleashed\" -serverInstancePath "C:\VU_Server1"`

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