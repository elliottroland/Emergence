using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Emergence.Weapons;

namespace Emergence {
    abstract public class Agent : ICollidable {
        protected Vector3 position, oldPosition;
        public Vector2 direction; //theta, phi -- representing, in spherical coords the direction of a unit vector
        public float speed = 400f, rotationSpeed = 15f, lookSensitivity = 15f, jump = 8f, terminalVelocity = -1000f;
        public CoreEngine core;
        protected Vector3 size = new Vector3(32,88,32);
       
        public Weapon equipped;
        public int health = 100;
        public int ammo = 200;

        public int maxHealth = 150, maxAmmo = 300;
        public float spawnTime = 0, spawnDelay = 5;
        public bool takingDamage = false;
        public int kills = 0, deaths = 0;
        public float timeSinceDamageDealt = 0, timeLimitSinceDamageDealt = 2;

        public Agent damageSource;
        public String name;

        public Velocities agentVelocities;

        protected List<CollisionGridCell> collisionCells;

        public Agent(CoreEngine c, Vector3 position, Vector2 direction) {
            core = c;
            this.position = position;
            this.direction = direction;
            equipped = new Pistol();

            agentVelocities = new Velocities();
            collisionCells = new List<CollisionGridCell>();
        }

        public void setName(String s) {
            name = s;
        }

        public Vector3 getDirectionVector() {
            return new Vector3((float)(Math.Cos(direction.X) * Math.Sin(direction.Y)),
                                (float)(Math.Cos(direction.Y)),
                                (float)(Math.Sin(direction.X) * Math.Sin(direction.Y)));
        }

        public Vector2 getDirectionFromVector(Vector3 vec) {
            vec = Vector3.Normalize(vec);
            return new Vector2((float)Math.Atan2(vec.Z, vec.X), (float)Math.Acos(vec.Y));
        }

        public Vector3 getEyePosition() {
            return position + new Vector3(0, size.Y-8, 0);
        }

        public Vector3 getCenter() {
            return getBoundingBox().Min + size * 0.5f;
        }

        public void dealDamage(float damage, Agent source) {
            health -= (int)damage;
            takingDamage = true;
            damageSource = source;
            timeSinceDamageDealt = 0;
        }

        public BoundingBox getBoundingBoxFor(Vector3 pos) {
            return new BoundingBox(new Vector3(pos.X - size.X / 2, pos.Y, pos.Z - size.Z / 2),
                new Vector3(pos.X + size.X / 2, pos.Y + size.Y, pos.Z + size.Z / 2));
        }

        public BoundingBox getBoundingBox() {
            return getBoundingBoxFor(position);
        }

        BoundingBox ICollidable.getBoundingBox() {
            return getBoundingBox();
        }

        List<CollisionGridCell> ICollidable.getCollisionCells() {
            return collisionCells;
        }

        void ICollidable.setCollisionCells(List<CollisionGridCell> cells) {
            collisionCells = cells;
        }

        protected void clampDirection()   {
            direction.X = (float)(direction.X % (Math.PI * 2));
            direction.Y = (float)Math.Min(Math.PI - 0.0001f, Math.Max(0.0001f, direction.Y));
        }

        abstract public void Update(GameTime gameTime);

        public void setPosition(Vector3 newPos) {
            oldPosition = position;
            position = newPos;
        }

        public Vector3 getPosition() {
            return position;
        }

        public Vector3 getOldPosition() {
            if (oldPosition == null)
                return Vector3.Zero;
            return oldPosition;
        }
    }
}
