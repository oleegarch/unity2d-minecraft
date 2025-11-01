# 🌍 Unity 2D Minecraft — Procedural Sandbox Prototype

[🎮 **Play Demo on GitHub Pages**](https://oleegarch.github.io/unity2d-minecraft)

## ✨ Features

### 🧱 Procedural Infinite World
- Generates endlessly in all directions.  
- Supports **biome-based generation**, allowing unlimited expansion.  
- Current biomes:
  - ❄️ *Winter*  
  - 🌲 *Forest*  
  - 🏜️ *Desert*  
  - 🌋 *Red Desert*

### 🌳 World Elements
- Procedural **tree spawning**.  
- **Ore generation** underground (no caves yet).  
- **Gravity-aware blocks**: cutting the bottom of a tree or cactus destroys the entire structure above.  

### 🪓 Building & Mining
- **Block breaking animations** with item drops.  
- Dropped blocks can be **picked up** and stored in the inventory.

### 🎒 Inventory & Crafting
- **Player inventory** and **creative inventory**.  
- **Workbench system** with its own crafting grid.  
- Crafting recipes are defined via **ScriptableObjects**, making it easy to add new items without touching the UI.

### 🐄 Entities
- Early **entity system** — currently includes:
  - The **player**  
  - A simple **cow**  

### 💾 Save System
- Partial but working **world save/load** implementation using the file system.

### 🎨 Animations
- Basic **walking**, **block-breaking**, and environmental animations.

---

## 🛠️ Built With
- **Unity** (C#)
- **ScriptableObjects** for data-driven systems
- **Custom chunk renderer** for procedural tiles

---

## 🚀 Roadmap
- [ ] Expand biome diversity  
- [ ] Improve terrain smoothness  
- [ ] Add more creatures and AI  
- [ ] Introduce day/night cycle  
- [ ] Fully implement saving/loading  
- [ ] Polished UI for crafting and inventory  

---

## 💬 About the Project
This project was made purely for learning and fun — a sandbox experiment that grew into a small procedural world.  
It’s a personal attempt to understand how complex systems like **Minecraft’s world generation and crafting logic** work under the hood.

> “Started as a weekend experiment — turned into a month-long obsession.”