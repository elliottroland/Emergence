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

    public class Player {
        public Vector3 position;
        public Vector2 direction; //theta, phi -- representing, in spherical coords the direction of a unit vector
        public PlayerIndex playerIndex;
        public float speed = 200f, rotationSpeed = 15f, lookSensitivity = 15f, jump = 800f, terminalVelocity = -1000f;
        public CoreEngine core;
        private Vector3 size = new Vector3(32,88,32);
       
        public Weapon equipped;
        public int health;
        public int ammo;

        public List<Bullet> bullets = new List<Bullet>();

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position, Vector2 direction) {
            core = c;
            this.position = position;
            this.direction = direction;
            this.playerIndex = playerIndex;
            equipped = new Pistol();
            ammo = 200;
        }

        public Player(CoreEngine c, PlayerIndex playerIndex, Vector3 position) : this(c, playerIndex, position, new Vector2(0, (float)MathHelper.PiOver2)) { }

        public Vector3 getDirectionVector() {
            return new Vector3((float)(Math.Cos(direction.X) * Math.Sin(direction.Y)),
                                (float)(Math.Cos(direction.Y)),
                                (float)(Math.Sin(direction.X) * Math.Sin(direction.Y)));
        }

        public Vector3 getEyePosition() {
            return position + new Vector3(0, size.Y-8, 0);
        }

        public BoundingBox getBoundingBoxFor(Vector3 pos) {
            return new BoundingBox(new Vector3(pos.X - size.X / 2, pos.Y, pos.Z - size.Z / 2),
                new Vector3(pos.X + size.X / 2, pos.Y + size.Y, pos.Z + size.Z / 2));
        }

        public BoundingBox getBoundingBox() {
            return getBoundingBoxFor(position);
        }

        private void clampDirection()   {
            direction.X = (float)(direction.X % (Math.PI * 2));
            direction.Y = (float)Math.Min(Math.PI - 0.0001f, Math.Max(0.0001f, direction.Y));
        }

        public void Update(GameTime gameTime) {
            
            equipped.Update(gameTime);
            
            //ping the input engine for move
            List<Actions> actions = core.inputEngine.getGameKeys();
            actions.AddRange(core.inputEngine.getGameButtons(playerIndex));

            Vector2 move = core.inputEngine.getMove() + core.inputEngine.getMove(playerIndex);
            Vector2 look = core.inputEngine.getLook() + core.inputEngine.getLook(playerIndex)*lookSensitivity;
            look = new Vector2(MathHelper.ToRadians(look.X), MathHelper.ToRadians(look.Y)) * rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            direction = direction + look;
            clampDirection();

            Vector3 jumpVelocity = Vector3.Zero;

            foreach (Actions a in actions)
                if (a == Actions.Jump)
                {
                    jumpVelocity.Y = jump;
                }
                else if (a == Actions.Fire){
                    equipped.fire(this);
                }
                else if (a == Actions.Downgrade)
                    equipped = equipped.upgradeDown();

            Vector3 velocity = Vector3.Zero;
            Vector3 forward = getDirectionVector();
            forward.Y = 0;
            forward.Normalize();
            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            right.Normalize();
            velocity = forward * move.Y + right * move.X;
            if (velocity != Vector3.Zero)
                velocity.Normalize();
            velocity = velocity * speed;   //this is when it actually becomes the velocity

            position = position + core.physicsEngine.applyCollision(gameTime, playerIndex, velocity + jumpVelocity);

            //update all bullet positions
            for (int i = bullets.Count-1; i >= 0; --i)
            {
                Bullet curB = bullets[i];
                curB.update();
                bullets[i] = curB;
                if (bullets[i].timeLeft <= 0)
                    bullets.Remove(bullets[i]);

            }

        }
    }
}
