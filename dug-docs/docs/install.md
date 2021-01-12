---
id: install
title: Install
---

### Linux Deb (Debian, Ubuntu, Mint, Pop!_os)

1. Go to the [latest release](https://github.com/unfrl/dug/releases/latest) and download the .deb package.
    * It should look like `dug.<version>.linux-x64.deb`
2. On most distros double clicking the .deb package will allow you to install via a UI, alternatively it can be installed by running `sudo dpkg -i ./dug.<version>.linux-x64.deb`

### Linux RPM (RHEL, CentOS, Fedora)

1. Go to the [latest release](https://github.com/unfrl/dug/releases/latest) and download the .rpm package.
    * It should look like `dug.<version>.linux-x64.rpm`
2. On most distros double clicking the .deb package will allow you to install via a UI, alternatively it can be installed by running `rpm -i ./dug.<version>.linux-x64.deb`

### Arch

A friend put dug in the AUR! [here](https://aur.archlinux.org/packages/dug-git/)

### Docker

Not sure if this counts as an "Installation" but there is a docker image available [here](https://hub.docker.com/r/unfrl/dug)

### OSX
1. Go to the [latest release](https://github.com/unfrl/dug/releases/latest) and download the osx binary.
    * It should look like `dug-osx-x64`
2. You should be able to download that, make is executable, and run it from the terminal. Then you can put it somewhere and update your path so you can execute it from anywhere.

### Windows

#### Chocolatey (choco cli)
> Chocolatey manually reviews all packages, which takes a while, so this can lag behind the latest. [status info here](https://chocolatey.org/packages/dug)

1. Run: `choco install dug`

#### Executable
1. Go to the [latest release](https://github.com/unfrl/dug/releases/latest) and download the .exe binary.
    * It should look like `dug.exe`
2. You should be able to download that and run it from the terminal. Then you can put it somewhere and update your path so you can execute it from anywhere.