# بنك الحظ المصري — محافظات
# Bank Al-Hazz Al-Masry — Governorates

A professional mobile board game for Google Play, inspired by classic Monopoly
rules (Hasbro) and reskinned with an Egyptian spirit: governorates and famous
places across Egypt, Egyptian pounds (EGP), Egyptian Chance / Community Chest
cards, and cinematic realistic visuals.

> **Not a web game.** This is a Unity + C# project for Android (Google Play).
> The whole project lives in this repository.

## Final shape (agreed with the owner, 2026-11-25)

| Area | Decision |
|---|---|
| Name | بنك الحظ المصري — محافظات |
| Platform | Google Play app, Unity + C#, GitHub |
| Visual style | Realistic + cinematic animation |
| Language | Arabic (RTL) + English |
| Solo mode | 2 players vs AI, timed 15 / 30 / 45 / 60 min, player picks duration before start, computer is banker automatically |
| Online mode | Rooms inside the game, invite / code, up to 4 players + 1 human banker = 5 in room, classic open (1 hour+, players decide) |
| Board | Selectable 28 / 32 / 40 squares, room creator chooses |
| Rules | Hasbro classic: start 1500 EGP, GO 200 EGP, jail fine 50 EGP, houses 50/100/150/200 by colour group, hotel after 4 houses (3 in 15-min Quick), auctions ON, trading cash/deeds/jail cards only, mortgage half price +10% to lift |
| Monetization | Free with in-app purchases: characters/pieces, board skins, in-game currency |
| Tokens | Transport means only, cosmetic, no gameplay advantage. Starting free token: bicycle. Store: normal car, luxury car, electric scooter, tuk-tuk, private plane — each with its own movement animation. Bundle = piece shape + player avatar |
| Avatars | Famous Egyptian (mostly) / Arab characters |

See [GAME_DESIGN.md](GAME_DESIGN.md) for the full game design document.

## Project layout

```
Assets/
  Scenes/        MainScene.unity (board, UI, GameManager)
  Scripts/       Core C# gameplay code (see below)
  Art/           Board art, tokens, avatars (to be added)
  Audio/         Music and SFX (to be added)
  Prefabs/       Board squares, cards, dice, tokens
ProjectSettings/ Unity project settings
```

## Core scripts

| Script | Responsibility |
|---|---|
| `GameConfig.cs` | Shared constants: money, timers, board sizes, price ladder |
| `GameManager.cs` | Turn loop, match timer, win condition, net-worth valuation |
| `Board.cs` | 28/32/40-square data model, square types, governorate placeholders |
| `Player.cs` | Cash, properties, houses/hotels, jail state, net worth |
| `Dice.cs` | Two-dice rolls, doubles tracking, 3 doubles = jail |
| `Bank.cs` | Banker logic: GO salary, houses/hotels supply (32/12), auctions, mortgages |
| `Property.cs` | Price / rent / house ladder 60–400 EGP, rent tiers, mortgage |
| `ChanceDeck.cs` | 16 Chance cards |
| `CommunityChestDeck.cs` | 16 Community Chest cards |
| `Auction.cs` | Mandatory auction when a property is declined |
| `Trade.cs` | Structured offers: cash + deeds + jail cards |
| `AIManager.cs` | Easy / Normal / Hard AI opponents |
| `NetworkRoom.cs` | Room create/join by code/invite, host settings, banker seat |
| `Localization.cs` | Arabic + English UI strings (RTL Arabic) |

## Build (Unity)

- Unity 2022.3 LTS or newer (URP not required for v1).
- Open the project folder in Unity Hub, open `Assets/Scenes/MainScene.unity`.
- Android: File → Build Settings → Android, build an `.aab` for Google Play.
- No web build is produced by this project.

## Status (v1 skeleton)

- [x] Project skeleton + GDD + core C# gameplay scripts
- [ ] Unity scene wiring (board visuals, UI prefabs)
- [ ] Art: realistic board, governorate photos, transport tokens, avatars
- [ ] Audio: music + SFX
- [ ] Online multiplayer server (authoritative dice/trades)
- [ ] Google Play listing, signing, in-app purchases

## License

All rights reserved — Ibn Majdi. Monopoly rules referenced are Hasbro's
classic rules; this game is an original Egyptian reskin, not a copy of
Monopoly artwork, names, or branding.
