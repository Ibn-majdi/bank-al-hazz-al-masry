# Visual Design — Bank Al-Hazz Al-Masry (Unity 6.6)

Engine: Unity 6.6 (6000.6.10f1), Android target SDK 35, min SDK 24.
This file replaces the old VISUAL_DESIGN.md. The old file is not approved.
Generated images are NOT approved as reference.

## Identity
- Modern clean Egyptian, modern in every element, new and clean.
- NOT pharaonic in the base theme. Pharaonic and other themes are seasons/events only, they open and close.
- Colours: colourful and strong. Primary: Nile blue. No fantasy blue.
- Interface: light. Buttons: medium, not big and not small, rounded/circular. Cards: square.

## Scene (agreed 2026-11-25)
- Square 3D board like the real Monopoly board, like a table.
- 4 chairs around it, one per player.
- Bank: virtual character, NOT a player. Standing on the left of the table,
  next to a scoreboard where he writes the score, cashier drawer in front like a bank.
- Tapping the scoreboard shows each player's score/balance, how many cities each owns, and whose turn it is.
- Room: 4 players + virtual bank. Bank is not a fifth player seat.
- Player whose turn it is: spotlight / light on him.
- Rolling dice: camera zooms to the board full screen, shows piece/car and dice movement.
- Stopping on a city: city card appears with all its info and a 3D image of the city inside it, Buy or Pass if unowned.
- Buying: camera moves to the table scene and the player pays the bank.

## Board
- Square arena like the real Monopoly board from the Amazon link (B000J591JM).
- Governorates and cities around the edges, Chance and Community Chest cards in the middle.
- 8 colours, geographic: Delta/countryside green, Upper Egypt/South orange for the sun,
  Alexandria/coastal light sky blue for the sea, Sinai/Red Sea red, Cairo navy.
  Price is by the city's real value, not by colour.

## Cards and characters
- Cards, characters and visual identity close to Legends of Runeterra in structure
  and style only — NOT the same identity, NOT the same colours, no fantasy blue.
- City card: city and its landmark only, NO characters.
- Characters belong in character/avatar cards only.
- Animation and arena/movement: Monopoly Go.
- Lighting and colours: Kingshot.

## Build (Unity 6.6)
1. Unity Hub + Unity Editor 6000.6.10f1 + Android Build Support (SDK + NDK + OpenJDK).
2. Open this project in Unity Hub, open Assets/Scenes/MainScene.unity.
3. Build Android .aab for Google Play.
