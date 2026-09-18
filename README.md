\# LavaRiseOnline



A 2D online multiplayer game for 4 players built in Unity, where you climb a tower while lava rises from below. Last player standing wins.



!\[Gameplay](path-to-your-gif.gif)



\## 🎮 How to Play



\- \*\*WASD\*\* — move

\- \*\*Space\*\* — jump

\- \*\*Right click\*\* — shoot

\- On death, you switch to spectator mode and can keep dropping \*\*bombs\*\* onto the play area to affect the remaining players



Objective: survive the rising lava and be the last player alive.



\## 🛠️ Tech Stack



\- \*\*Unity\*\* (2021.3 LTS)

\- \*\*Photon PUN2\*\* — multiplayer networking



\## 🌐 About the Project



The original idea started as a group project, but this version — including major multiplayer synchronization fixes — was developed entirely by me.



\### Technical Challenges Solved



\- Synchronizing each player's "alive/dead" state across all connected clients

\- Handling RPCs for networked events: shooting, projectile impacts, spectator bombs

\- Coordinating match logic through the MasterClient (player spawning, win condition checks)

\- Fixed synchronization bugs: lava wasn't correctly triggering the defeat screen, the last surviving player wasn't winning automatically, and reaching the goal was making every player win instead of only the one who reached it



\## 📦 Running It Locally



1\. Clone the repository

2\. Open it with Unity 2021.3 LTS or newer

3\. You'll need to configure your own Photon PUN2 App ID (via `Window > Photon Unity Networking > PUN Wizard`, or directly in `Assets/Resources/PhotonServerSettings`) to connect and test multiplayer from the editor



> Note: if you just want to \*\*play\*\* the game (not compile it), check for a playable build link on itch.io — no setup needed there.

