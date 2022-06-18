![Arkanoid logo](/Images/icon.png)

# DOTS Arkanoid

It's a clone of the classic arcade game created with the experimental Unity DOTS technologies. The game is based on the original Arkanoid gameplay. And the visual style refers to the games of the 16-bit era.

The goal of this project is to get to know Unity DOTS better. The DOTS (Data-Oriented Technology Stack) is a set of technologies developed by Unity for building high-performance, multithreaded code. The most important part of DOTS is the ECS component system, which allows to write code in a date oriented way. The DOTS Arkanoid project uses physics and rendering built on ECS (DOTS Physics and Hybrid Renderer). Some technical details are described below.

![scr1](/Images/1s.png) ![scr2](/Images/2s.png)
![scr3](/Images/3s.png) ![scr4](/Images/4s.png)

## Game design details

* Original game mechanics and retro look inspired by [Arkanoid](https://en.wikipedia.org/wiki/Arkanoid) arcade game and SNES Arkanoid: Doh It Again 1997 by Taito. Local coop mode inspired by Block Block (World 910910) arcade game by Capcom.
* For input you can use touch screen, keyboard (A, D, Left, Right, Space), a mouse (only player 1) and gamepads (Xbox Controller/XInput)
* Catch, Enlarge, Laser, Disruption and other power-ups have implemented, hi score and balls speedup mechanics have implemented as well.
* 5 original stages are included, arcade version of field size 13x18 is used.
* The ball movement and bouncing physics is close to the original Arkanoid physics.
* Press Alt+Enter to switch between full screen and windowed mode (for desktop builds).

## Tech details

* Pure ECS in runtime, except Audio, UI and resources, where [hybrid components](https://docs.unity3d.com/Packages/com.unity.entities@0.50/manual/hybrid_component.html) are used.
* URP + [Hybrid render](https://docs.unity3d.com/Packages/com.unity.rendering.hybrid@latest/index.html) with shader for UV coord. animation. The Hybrid render uses Vulcan API only, so we can build the game on Android and Windows desktop platforms.
* [DOTS Physics](https://docs.unity3d.com/Packages/com.unity.physics@latest/index.html) for triggers (ICollisionEventsJob), queries (CastCollider) and masks filtering
* Burst compilation and jobs scheduling is used for most jobs, however, some sync points optimizations are possible.
* You can use "F1" cheat button to spawn multiple balls.

## Known issues

* The current DOTS packages are experimental and may contain bugs. If you want to build a project using IL2CPP you must set Project settings -> Player -> Configuration section -> IL2CPP Code Generation -> Faster (smaller) builds, [otherwise the project will be built with an error](https://forum.unity.com/threads/executionengineexception-attempting-to-call-method-unity-entities-fastequality-compareimpl-1.1296462/).
* If you get messages about material or shader errors, please re-import the assets.

## Links

* [Getting started with Unity DOTS](https://nikolayk.medium.com/getting-started-with-unity-dots-part-1-ecs-7f963777db8e)
* [Systems Interaction in Entity-Component-System (Events)](https://medium.com/@ben.rasooli/systems-interaction-in-entity-component-system-events-4a050153c8ac)
* [Unity DOTS forum](https://forum.unity.com/forums/data-oriented-technology-stack.147/)
* [Arkanoid StrategyWiki](https://strategywiki.org/wiki/Arkanoid/Gameplay)

## Third party resources

* "Press Start 2P" font, designed by CodeMan38 (OFL)
* Title logo "Wonder Boy/Sega" by http://arcade.photonstorm.com/
* Sprites from Arkanoid: Doh it Again 1997 SNES/Taito
* Sound effects from Arkanoid II NES/Taito
* Original Arkanoid gameplay by Taito Corporation

## Download

You can download DOTS Arkanoid packages for Android and Windows in [Release page](https://github.com/EugenyN/DOTS-Arkanoid/releases).


## License

Copyright (c) 2022 Eugeny Novikov. The source code is under the MIT license.
