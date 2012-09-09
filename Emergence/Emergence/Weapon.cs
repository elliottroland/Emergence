using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace GamesProject
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Weapon : Microsoft.Xna.Framework.GameComponent
    {

        int damage;
        int ammoUsed;
        int cooldown;
        int curCooldown;

        public Weapon(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if(curCooldown > 0)
                curCooldown--;
            base.Update(gameTime);
        }

        public virtual void fire(){
        
            //remove ammo used by weapon

            curCooldown = cooldown;

        }

        //Traverse the weapon by the left branch
        public abstract Weapon upgradeLeft();

        //Traverse the weapon by the right branch
        public abstract Weapon upgradeRight();

        //Revert to more basic weapon
        public abstract Weapon upgradeDown();

    }
}