# Crypto Mining Clicker — Unity web port

Open this folder with **Unity 6000.0.73f1 LTS**, open **Assets/Scenes/Main.unity**, then press Play.
The project retains the **Universal 2D 5.1.0** template, URP 2D Renderer and portrait 480 × 854 layout.

## Implemented from the web game

- Mining room, animated GPU fans, eight rack slots, trading computer and six navigation tabs.
- Nine GPU models, six fictional coins and six cooler tiers with the web game's prices, progression gates, heat, power, fees and multipliers.
- Synchronized click mining, automatic mining, overclock, fever and overheat/restart.
- GPU inventory with explicit slot selection/replacement; cooling inventory with transfer and stock-cooler restoration.
- Market prices, 5-second candlesticks, 45–90 second market events, separate chart/mining coin selection, quantity/all sales, scaling contracts.
- Rebirth requirements, persistent FORK network, five opening scenes with typewriter captions and seven tutorial pages using the original Korean copy.
- JSON autosave every three seconds, pause/quit saves and a backup of the preceding save.

## Rendering and saves

This is a native C#/Unity UI implementation, not an embedded browser. Layout, copy and gameplay follow index.html; CSS gradients, browser emoji and typography are represented with Unity graphics and system Korean fonts, so rendering is not pixel-identical. Target devices should be checked for font availability before distribution.

Saves live in Application.persistentDataPath/crypto_mining_unity6_web_v1.json. Browser localStorage and previous Unity saves are independent; this port does not overwrite or automatically import them. Opening/tutorial flags also use a separate namespace.

## Validation

Tools → Crypto Mining → Validate Gameplay runs 20 isolated checks without writing player saves, covering click/auto mining, unlocks, slot replacement, cooler inventory, sales, contracts, heat, FORK, rebirth and JSON round trips. Results are written to Logs/WebPortValidation.txt. All seven menus and the scrollable candle chart were also exercised in Play mode at 480 × 854.

Unity Version Control is pinned to 2.12.4 for compatibility with this editor. The existing InputSystem_Actions asset is preserved; the UI uses InputSystemUIInputModule's default UI actions.
