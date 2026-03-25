<<<<<<< HEAD
=======

>>>>>>> 160d64b (Add MIT license)
# RestorePointGUI

<p align="center">
  <a href="https://github.com/kevinz26/RestorePointGUI/releases/latest">
    <img src="https://img.shields.io/badge/Download-Installer-brightgreen?style=for-the-badge" alt="Download Installer">
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/github/v/release/kevinz26/RestorePointGUI?style=flat-square" alt="Latest Release">
  <img src="https://img.shields.io/github/downloads/kevinz26/RestorePointGUI/total?style=flat-square" alt="Downloads">
  <img src="https://img.shields.io/github/license/kevinz26/RestorePointGUI?style=flat-square" alt="License">
  <img src="https://img.shields.io/github/repo-size/kevinz26/RestorePointGUI?style=flat-square" alt="Repo Size">
  <img src="https://img.shields.io/github/last-commit/kevinz26/RestorePointGUI?style=flat-square" alt="Last Commit">
  <img src="https://img.shields.io/badge/platform-Windows%20x64-blue?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8-purple?style=flat-square" alt=".NET 8">
  <img src="https://img.shields.io/badge/WPF-Desktop-blueviolet?style=flat-square" alt="WPF Desktop">
</p>

<p align="center">
  A fast, lightweight WPF utility for creating and managing Windows system restore points.
</p>

---

## Install

### Recommended
Download the latest installer from the Releases page:

<p>
  <a href="https://github.com/kevinz26/RestorePointGUI/releases/latest">
    <img src="https://img.shields.io/badge/Download-Latest%20Release-brightgreen?style=for-the-badge" alt="Download Latest Release">
  </a>
</p>

### Portable
A ZIP version is also available on the Releases page for portable or manual use.

---

## Demo

<p align="center">
  <img src="demo.gif" width="800" alt="RestorePointGUI Demo">
</p>

---


## Features

- Create Windows system restore points
- Simple and clean WPF interface
- Lightweight and fast
- Installer and portable ZIP releases
- Windows x64 support
- .NET 8 based desktop app

---

## Requirements

- Windows 10 or newer
- Administrator privileges for restore point operations
- x64 system

---

## Build from source

```powershell
dotnet restore RestorePoint.sln
dotnet build RestorePoint.sln -c Release