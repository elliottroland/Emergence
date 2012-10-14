using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Emergence.Weapons;

namespace Emergence
{
    public struct Bullet{
    
        public Vector3 pos;
        public Vector3 dir;
        public int timeLeft;

        public void update() {

            pos = pos + dir * 50;
            timeLeft -= 10;
        
        }
    
    }

    public class Player : Agent {
        public PlayerIndex playerIndex;

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position, Vector2 direction)
            : base(c, position, direction) {
            this.playerIndex = playerIndex;
        }

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position)
            : this(c, playerIndex, position, new Vector2(0, (float)MathHelper.PiOver2)) { }

        public override void Update(GameTime gameTime) {

            equipped.Update(gameTime);

            //ping the input engine for move
            List<Actions> actions = core.inputEngine.getGameKeys();
            actions.AddRange(core.inputEngine.getGameButtons(playerIndex));

            Vector2 move = core.inputEngine.getMove() + core.inputEngine.getMove(playerIndex);
            Vector2 look = core.inputEngine.getLook() + core.inputEngine.getLook(playerIndex) * lookSensitivity;
            look = new Vector2(MathHelper.ToRadians(look.X), MathHelper.ToRadians(look.Y)) * rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            direction = direction + look;
            clampDirection();

            Vector3 jumpVelocity = Vector3.Zero;

            foreach (Actions a in actions)
                if (a == Actions.Jump) {
                    jumpVelocity.Y = jump;
                }
                else if (a == Actions.Fire) {
                    equipped.fire(this);
                }
                else if (a == Actions.Downgrade)
                    equipped = equipped.upgradeDown();

            Vector3 velocity = Vector3.Zero;
            Vector3 forward = getDirectionVector();
            if (core.clip)
                forward.Y = 0;
            forward.Normalize();
            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            right.Normalize();
            velocity = forward * move.Y + right * move.X;
            if (velocity != Vector3.Zero)
                velocity.Normalize();
            velocity = velocity * speed;   //this is when it actually becomes the velocity

            //the velocity given to the physics engine is but a request for movement.
            //the final position is decided by that engine, taking into account gravity and collision detection/response
            core.physicsEngine.applyMovement(gameTime, playerIndex, velocity + jumpVelocity);

            //update all bullet positions
            for (int i = bullets.Count - 1; i >= 0; --i) {
                Bullet curB = bullets[i];
                curB.update();
                bullets[i] = curB;
                if (bullets[i].timeLeft <= 0)
                    bullets.Remove(bullets[i]);

            }

            core.physicsEngine.updateCollisionCellsFor(this);
        }
    }
}
