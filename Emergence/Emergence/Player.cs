using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Emergence.Weapons;

namespace Emergence
{
    public class Player : Agent {
        public PlayerIndex playerIndex;
        public bool drawUpgradePath = false, showScoreboard = false;
        public float maxPathsWheelHeight = 475, currentPathsWheelHeight = 200;
        public float elapsedTime = 0;

        public float scoreboardDist = 0;
        public static float maxScoreBoardDist = 576.0f / 4;

        public float damageAlpha1 = 0, damageAlpha2 = 0;
        public float blinkTime = 2, curBlinkTime = 0;
        public int blinkCost = 50;

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position, Vector2 direction)
            : base(c, position, direction) {
            this.playerIndex = playerIndex;
        }

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position)
            : this(c, playerIndex, position, new Vector2(0, (float)MathHelper.PiOver2)) { }

        public void fakeUpdate(GameTime gameTime) {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = this.elapsedTime + (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.elapsedTime = 0;
            if (spawnTime > 0) {
                spawnTime -= elapsedTime;
                if (spawnTime <= 0) {
                    spawnTime = 0;
                    health = 100;
                    ammo = 200;
                    equipped = new Pistol();
                    core.spawnPlayer(this);
                }
                return;
            }
            else if (health < 0) {
                spawnTime = spawnDelay;
                return;
            }
            equipped.Update(gameTime);

            //ping the input engine for move
            List<Actions> actions = new List<Actions>();
            actions.AddRange(core.inputEngine.getGameKeys());
            actions.AddRange(core.inputEngine.getGameButtons(playerIndex));

            Vector2 move = core.inputEngine.getMove() + core.inputEngine.getMove(playerIndex);
            Vector2 look = core.inputEngine.getLook() + core.inputEngine.getLook(playerIndex) * lookSensitivity;
            look = new Vector2(MathHelper.ToRadians(look.X), MathHelper.ToRadians(look.Y)) * rotationSpeed * elapsedTime;

            direction = direction + look;
            clampDirection();

            Vector3 jumpVelocity = Vector3.Zero;
            
            foreach (Actions a in actions)
                if (a == Actions.Jump) {
                    jumpVelocity.Y = jump;
                }
                else if (a == Actions.Fire) {
                    equipped.fire(this, core.physicsEngine);
                }
                else if (a == Actions.Downgrade)
                    equipped = equipped.upgradeDown();
                else if (a == Actions.Scoreboard) {
                    showScoreboard = true;
                    if (scoreboardDist < maxScoreBoardDist)
                        scoreboardDist += 30;
                }
                else if (a == Actions.Reload) {
                    if (currentPathsWheelHeight < maxPathsWheelHeight)
                        currentPathsWheelHeight += 30;
                    drawUpgradePath = true;
                }
                else if(actions.Contains(Actions.Pause)){
                    core.currentState=GameState.MenuScreen;
                    core.menuEngine.currentMenu=core.menuEngine.pauseMenu;
                    }

            //Check for released buttons
            if (!actions.Contains(Actions.Reload)) {
                currentPathsWheelHeight = 0;
                drawUpgradePath = false;
            }
            if (!actions.Contains(Actions.Scoreboard)) {
                scoreboardDist = 0;
                showScoreboard = false;
            }

            //reduce damage alpha values
            if (takingDamage) {
                if (damageAlpha1 != 0)
                    damageAlpha2 = 1;
                else
                    damageAlpha1 = 1;
                takingDamage = false;
            }
            if (damageAlpha1 > 0) {
                damageAlpha1 -= 0.005f;
            }
            else
                damageAlpha1 = 0;


            if (damageAlpha2 > 0) {
                damageAlpha2 -= 0.005f;
            }
            else
                damageAlpha2 = 0;

            Vector3 velocity = Vector3.Zero;
            Vector3 forward = getDirectionVector();
            curBlinkTime = Math.Max(curBlinkTime - (float)gameTime.ElapsedGameTime.TotalSeconds, 0);

            if (curBlinkTime <= 0 && actions.Contains(Actions.Aim) && ammo >= blinkCost) {
                jumpVelocity = Vector3.Zero;
                curBlinkTime = blinkTime;
                ammo -= blinkCost;
                core.physicsEngine.applyBlink(elapsedTime, this, forward * 1000);
            }
            else {
                if (core.clip)
                    forward.Y = 0;
                forward.Normalize();
                Vector3 right = Vector3.Cross(forward, Vector3.Up);
                right.Normalize();
                velocity = forward * move.Y + right * move.X;
                if (velocity != Vector3.Zero)
                    velocity.Normalize();
                velocity = velocity * speed;   //this is when it actually becomes the velocity
                core.physicsEngine.applyMovement(elapsedTime, playerIndex, velocity + jumpVelocity);
            }

            //the velocity given to the physics engine is but a request for movement.
            //the final position is decided by that engine, taking into account gravity and collision detection/response

            core.physicsEngine.updateCollisionCellsFor(this);
        }
    }
}
