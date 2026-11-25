# Game Design Document — بنك الحظ المصري: محافظات
# Bank Al-Hazz Al-Masry: Governorates

**Version:** 1.0 — Core skeleton (2026-11-25)
**Status:** Final shape agreed with the owner. Minor details (specific avatar
names, specific store prices, final governorate list) are deferred — build the
core first, with placeholder governorate names that can be swapped later.
**Platform:** Google Play app — Unity + C# (NOT a web game)
**Repository:** `bank-al-hazz-al-masry` on GitHub

---

## 1. Overview

A professional mobile board game in the spirit of classic Monopoly, reskinned
for Egypt. Players travel around a board of Egyptian governorates and famous
places, buy them, charge rent, build houses and hotels, and bankrupt their
opponents. The game is realistic in style with cinematic animation, in Arabic
(RTL) and English, free with in-app purchases.

Reference for the feel (not the rules or art): Yu-Gi-Oh! Master Duel —
polished, animated, competitive, room-based online play.

## 2. Final shape (agreed, do not change without owner approval)

| Area | Decision |
|---|---|
| Name | بنك الحظ المصري — محافظات |
| Platform | Google Play app, Unity + C#, project on GitHub |
| Visual style | Realistic + cinematic animation |
| Language | Arabic (RTL) + English, same amounts in both languages |
| Solo mode | 2 players vs AI. Timed 15 / 30 / 45 / 60 min, player picks duration before start. Computer is banker automatically. |
| Online mode | Rooms inside the game, invite / code. Up to 4 players + 1 human banker = 5 in room. Classic open (1 hour+, players decide). |
| Board | Selectable 28 / 32 / 40 squares. Room creator chooses. |
| Rules | Hasbro classic (see §4). Start 1500 EGP, GO 200 EGP, jail fine 50 EGP. |
| Monetization | Free with in-app purchases: characters/pieces, board skins, in-game currency — all of them. |
| Tokens | Transport means only, cosmetic, no gameplay advantage. Starting free token: bicycle. Store: normal car, luxury car, electric scooter, tuk-tuk, private plane — each with its own movement animation. Bundle = piece shape + player avatar. |
| Avatars | Famous Egyptian (mostly) / Arab characters. |

## 3. Modes

### 3.1 Solo (vs AI)
- 2 players: human + 1 AI. Option to add 1–2 more AI to make a 3–4 seat table
  (restores competition for colour sets and trading).
- Timed only: 15 / 30 / 45 / 60 minutes, chosen before the match starts.
- Banker: computer (server) automatically.
- AI difficulty: Easy / Normal / Hard (see `AIManager.cs`).
- AI acts in 2–5 seconds, never stalls the clock.
- Random starting player.

### 3.2 Online rooms
- Host creates a room, sets: board size (28/32/40), match length, turn timer,
  starting cash, GO salary, auctions on/off.
- Invite by link or room code; players enter the code to join.
- Up to 4 playing seats + 1 optional non-playing human banker seat = 5 in room.
- Human banker is an auctioneer / announcer only: **no power to create money,
  lend, set prices, or override the server ledger**. The server is the banker
  by default and always holds the true state.
- Spectators can watch but not play.
- Disconnect: seat is held, player may rejoin with a catch-up summary; if
  they do not return, an AI of the same difficulty takes the seat.
- Server rolls the dice and validates trades/auctions (anti-tamper).

## 4. Rules (Hasbro classic — verified against the official Hasbro rulebook)

### 4.1 Money
- Each player starts with **1500 EGP**.
- Passing or landing on GO pays **200 EGP**, once per circuit. No double for
  landing exactly on GO. Sent directly to Jail: no 200 EGP.
- Bank never goes broke.

### 4.2 Board and properties
- Full board: 40 squares / 28 buyable: 22 streets in 8 colour groups,
  4 transports, 2 utilities.
- Smaller boards (28 / 32): remove **whole colour groups only**, keep the
  4 corners + transports + utilities, keep 2 Chance + 2 Community Chest
  spaces. There is no official 28- or 32-square board — these are derived
  from classic ratios and must be playtested. Starting cash scales to
  ≈26% of that board's total property value; GO ≈ starting cash ÷ 7.5.
- Street price ladder (EGP, same numerals as classic $):

| Group | Prices | Base rent | House cost |
|---|---|---|---|
| Brown | 60, 60 | 2, 4 | 50 |
| Light blue | 100, 100, 120 | 6, 6, 8 | 50 |
| Pink | 140, 140, 160 | 10, 10, 12 | 100 |
| Orange | 180, 180, 200 | 14, 14, 16 | 100 |
| Red | 220, 220, 240 | 18, 18, 20 | 150 |
| Yellow | 260, 260, 280 | 22, 22, 24 | 150 |
| Green | 300, 300, 320 | 26, 26, 28 | 200 |
| Dark blue | 350, 400 | 35, 50 | 200 |

- Full colour set with no buildings = **double base rent**. Rent tiers with
  buildings follow the deed (e.g. Boardwalk 50 / 200 / 600 / 1400 / 1700 /
  2000 for site / 1 / 2 / 3 / 4 houses / hotel).
- Transports: 200 EGP each; rent 25 / 50 / 100 / 200 for 1–4 owned.
- Utilities: 150 EGP each; rent 4× dice roll (1 owned) / 10× dice roll (both).
- Taxes: Income Tax flat 200 EGP; Luxury Tax 100 EGP. Free Parking: nothing
  (no jackpot by default).

### 4.3 Building
- Build only on streets, only when you own the **full colour set** and no
  property in that set is mortgaged.
- **Even building:** never more than one house ahead on any property in the
  group; max 4 houses per property.
- **Hotel:** only after 4 houses on every property in the set; pay one house
  cost, return the 4 houses; max 1 hotel per property.
- In 15-min Quick matches: hotel after **3 houses**.
- Bank supply is finite: **32 houses, 12 hotels**; if out, wait; if several
  players want more than the Bank has, buildings are auctioned.
- Selling buildings: back to the Bank at **half the price paid**, evenly.
- Direct hotel upgrade (skipping houses) is NOT allowed.

### 4.4 Jail
- In via: landing on Go to Jail, a Go to Jail card, or 3 doubles in a row.
  Move directly, do not pass GO, do not collect 200 EGP, turn ends.
- Landing on the Jail space normally = Just Visiting, nothing happens.
- Out: (a) roll doubles on any of your next 3 turns — move, no extra roll;
  (b) play a Get Out of Jail Free card; (c) pay **50 EGP** fine before rolling.
- Forced exit: fail to roll doubles by your 3rd turn → pay 50 EGP and move.
- In Jail you may still collect rent, trade, build, mortgage, bid.
- Quick Jail (15/30-min matches): must exit on your next turn.

### 4.5 Chance and Community Chest
- 16 cards per deck; 3 spaces each on the 40-board (2 each on smaller boards).
- Chance: movement cards (Advance to GO / nearest transport ×2 / nearest
  utility / Go Back 3 / Go to Jail), money cards, repairs ($25/house,
  $100/hotel), Get Out of Jail Free.
- Community Chest: mostly money in/out, Advance to Go, Go to Jail, repairs
  ($40/house, $115/hotel), Get Out of Jail Free. Use the 2021 Hasbro
  community-flavour rewrite as the localisation template (neighbourhood
  iftar / street mawlid / helping neighbours / school fundraiser), keeping
  the 16 mechanics exactly, rewritten in Egyptian/Arabic terms.
- One Get Out of Jail Free per deck; kept until used/traded, then returned
  to the bottom of the deck.

### 4.6 Trading, auctions, mortgages
- **Auction is mandatory:** if you land on an unowned property and decline
  to buy at the printed price, the Banker immediately auctions it. Any
  player may bid, including the decliner. 20–30 s timer, minimum increment
  10 EGP.
- **Trading:** any time; trade **money, properties, Get Out of Jail Free
  cards only** — never buildings (sell first), never player loans. Structured
  offers only (cash + deeds + jail cards, both sides); no unenforceable
  promises. Mortgaged deeds can be traded.
- **Mortgage:** only unimproved property; sell all buildings in the group
  first (half price). Mortgage value = **half the printed price**; no rent
  while mortgaged; lift = value **+ 10% interest**.

### 4.7 Doubles
- Roll two dice, move the total. Doubles: resolve the space, roll again.
- **Third successive doubles = directly to Jail**, no move.
- Doubles to exit Jail do not give an extra roll; doubles after paying the
  fine / using a jail card do.

### 4.8 Bankruptcy and winning
- Bankrupt if you owe more than you can raise after selling buildings
  (half price) and mortgaging. You are out, token removed.
- Owe another player: they get everything of value. Owe the Bank: the Bank
  auctions your properties.
- Classic / untimed: **last player not bankrupt wins**.
- Timed matches: bankruptcy still eliminates immediately; when the clock
  ends, finish the current round so every player has had the same number of
  turns, then the richest wins by **net-worth valuation**:
  cash + unmortgaged property at printed price + mortgaged property at half
  printed price + houses at purchase price + hotels at purchase price
  (including the houses turned in).

### 4.9 Timed-match rulesets

| Match | Ruleset | End |
|---|---|---|
| 15 min (solo 2p, Quick) | Deal 2 deeds, pay for them; hotel after 3 houses; Quick Jail; auctions ON; 35 s turn timer | Clock → finish round → richest by valuation. Bankruptcy ends it at once. |
| 30 min (solo / 4p Quick) | Same hybrid, or deal 3 deeds free; hotel after 3; Quick Jail; 45–60 s turns | Clock → finish round → richest. Or first bankruptcy ends it. |
| 45 min (4p Standard) | Classic building (4 houses), classic Jail, 60 s turns; optional 2 paid dealt deeds | Clock → finish round → richest; or last-player-standing if near. |
| 60 min (4p Classic) | Full classic; 60 s turns; no dealt deeds | Last-player-standing first; otherwise clock → richest. |

- Turn timer default **60 s** (35 s in 15-min Quick), paused during auctions
  and trade responses (separate 20–30 s timers).
- On timeout: auto-roll / decline purchase / pass bid / decline trade / end
  manage — **never auto-buy, auto-build or auto-accept a trade**.
- Show a live net-worth leaderboard in the last 5 minutes.

### 4.10 House rules (NOT defaults)
Do NOT enable by default: Free Parking jackpot, $400/double GO, player
loans, full-price house resale, skipping Chance/Community Chest/tax spaces,
direct hotel upgrades. These may be offered only as clearly-labelled
House Rules toggles. Do NOT copy Monopoly GO! (solo progression, no
bankruptcy win).

## 5. Governorates (placeholder — to be finalised later)

Placeholder mapping for the 40-square board (swap names later, keep the
price ladder in §4.2):

- Brown (cheapest): Fayoum — Wadi El Rayyan, Beni Suef
- Light blue: Minya, Assiut, Sohag — Abydos
- Pink: Mansoura, Tanta, Zagazig
- Orange: Port Said, Ismailia, Suez
- Red: Damietta — Ras El Bar, Alexandria — Qaitbay Citadel, Marsa Matrouh
- Yellow: Luxor — Karnak, Aswan — High Dam, Qena — Dendera
- Green: Hurghada, Sharm El Sheikh, Dahab
- Dark blue (most expensive): Cairo — Zamalek, Giza — Pyramids
- Transports: Ramses Station, Cairo Airport, Alexandria Port, Sidi Gaber Station
- Utilities: Electricity, Water

## 6. Tokens, avatars, store

- Tokens are **transport means only, cosmetic, no gameplay advantage**.
- Starting free token: **bicycle**.
- Store tokens (each with its own movement animation on the board):
  normal car, luxury car, electric scooter, tuk-tuk, private plane.
- Bundle = piece shape + player avatar.
- Avatars: famous Egyptian (mostly) / Arab characters. Specific names and
  store prices are deferred — do not hard-code them in v1.

## 7. Monetization

- Free with in-app purchases: characters/pieces, board skins, in-game
  currency — all of them.
- No pay-to-win: purchases are cosmetic (tokens, avatars, board skins) or
  in-game currency for cosmetics; they never change rent, dice, or rules.

## 8. Technical plan (Unity + C#)

- Engine: Unity 2022.3 LTS, C#. Android build (.aab) for Google Play.
- Architecture: `GameManager` owns the turn loop and timers; `Board` owns
  squares; `Bank` owns money supply, houses/hotels, auctions, mortgages;
  `NetworkRoom` owns lobby + server-authoritative state; AI via `AIManager`.
- Server authority online: server rolls dice, validates trades/auctions,
  holds true state. Client never trusts its own dice.
- Localization: `Localization.cs` — every UI string in Arabic + English,
  amounts identical in both languages, RTL Arabic layout.
- Art/audio: realistic board, governorate photos, transport token models,
  cinematic dice/board animations, music + SFX — to be added after the core.
- Google Play: developer account ($25 one-time), app signing, store listing,
  privacy policy, in-app purchase products — after the core is playable.

## 9. Milestones

1. [x] v1 skeleton: project + GDD + core C# scripts (this commit)
2. [ ] Unity scene wiring: board, UI, dice, tokens
3. [ ] Solo vs AI playable (2p, timed, classic rules)
4. [ ] Art + audio + cinematic animation
5. [ ] Online rooms + human banker seat
6. [ ] Store + in-app purchases
7. [ ] Playtesting (28/32-board balance) + Google Play release
