Sync Dash
A hyper-casual endless runner with a unique real-time network synchronization twist.

Table of Contents
Game Concept

Core Mechanics

Real-Time State Synchronization

UI & Game Flow

Performance Optimization

Shaders & Visual Effects

How to Play

Setup Instructions

Gameplay Video

Screenshots

Compatibility

Game Concept
"Sync Dash" is a fast-paced hyper-casual endless runner designed to simulate a real-time multiplayer experience locally. Players control a glowing cube on the right side of the screen, navigating an ever-accelerating environment. The unique twist is the "ghost" player on the left side, which meticulously mirrors the main player's actions with simulated network latency, providing a compelling visual representation of real-time state synchronization.

Core Mechanics
Player Control: Guide a glowing cube automatically moving forward on the right half of the screen.

Tap to Jump: Tap or press the spacebar to make the player cube jump, avoiding incoming obstacles.

Collect Orbs: Gather glowing orbs scattered across the path to earn points.

Dynamic Speed: The game's speed continuously increases over time, progressively challenging player reflexes.

Score System: Points are awarded based on distance traveled and collected orbs. A separate score is tracked for the ghost player on the left side.

Real-Time State Synchronization
The core technical challenge and unique selling point of "Sync Dash" is its local simulation of real-time network syncing:

Action Mirroring: The ghost player on the left side precisely mimics all of the main player's actions (jumping, collecting orbs, colliding with obstacles).

Simulated Lag: A configurable delay is introduced to the ghost player's actions, making it feel like a genuine network latency, enhancing the "multiplayer opponent" illusion.

Smoothing Interpolation: To ensure a fluid and non-jittery experience, the ghost player's movement is smoothly interpolated between received action states.

Local Data Structures: Player actions are sent to the ghost player via a local queue system, replicating how data would be transmitted over a network.

UI & Game Flow
Main Menu: A clean and intuitive main menu provides "Start" and "Exit" options.

Game Over Screen: Upon collision with an obstacle, a game over screen appears, offering "Restart" to play again or "Main Menu" to return to the start.

Score Display: The current player score and ghost score are prominently displayed at the top of the screen during gameplay.

Performance Optimization
Object Pooling: Obstacles and collectibles are managed using an object pooling system to minimize instantiation/destruction overhead, ensuring smooth performance, especially on mobile devices.

Optimized Syncing: The action queuing and processing mechanism is designed for efficiency to prevent frame drops.

Shaders & Visual Effects
"Sync Dash" features several visually engaging effects:

Glowing Player: The player's cube is rendered with a custom glowing shader, enhancing its visual appeal.

Particle Bursts: Collecting an orb triggers a vibrant particle burst effect, providing immediate visual feedback.

Explosion Effect: Upon colliding with an obstacle, a significant explosion particle effect is displayed.

Camera Shake: A dynamic camera shake effect is triggered upon player collision with an obstacle, adding impact to the game over event.

Orb Rotation: Orbs rotate around their own axis, making them more visually appealing and noticeable.

How to Play
Start Game: From the Main Menu, click "Start".

Move Forward: Your cube will automatically move forward.

Jump: Tap anywhere on the screen or press the Spacebar to jump.

Avoid Obstacles: Jump over obstacles to continue your run.

Collect Orbs: Jump into glowing orbs to increase your score.

Game Over: Colliding with an obstacle ends the game.

Restart/Main Menu: On the Game Over screen, choose to "Restart" or return to the "Main Menu".

Gameplay Video- https://youtu.be/CJUu_48KUNY

This project is developed with Unity and is compatible with Unity 2021 or later versions.