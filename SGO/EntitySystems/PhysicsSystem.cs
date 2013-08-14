﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameObject;
using GameObject.System;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;
using ServerInterfaces.Map;
using ServerInterfaces.Tiles;

namespace SGO.EntitySystems
{
    internal class PhysicsSystem : EntitySystem
    {
        public PhysicsSystem(EntityManager em)
            : base(em)
        {
            EntityQuery = new EntityQuery();
            EntityQuery.AllSet.Add(typeof(PhysicsComponent));
            EntityQuery.AllSet.Add(typeof(TransformComponent));
            EntityQuery.Exclusionset.Add(typeof(SlaveMoverComponent));
        }

        public override void Update(float frametime)
        {
            var entities = EntityManager.GetEntities(EntityQuery);
            foreach(var entity in entities)
            {
                GasEffect(entity, frametime);
            }
        }

        private void GasEffect(Entity entity, float frameTime)
        {
            var transform = entity.GetComponent<TransformComponent>(ComponentFamily.Transform);
            var physics = entity.GetComponent<PhysicsComponent>(ComponentFamily.Physics);
            ITile t =
                IoCManager.Resolve<IMapManager>().GetTileFromWorldPosition(transform.Position);
            if (t == null)
                return;
            var gasVel = t.GasCell.GasVelocity;
            if (gasVel.Abs() > physics.Mass) // Stop tiny wobbles
            {
                transform.Position = new Vector2(transform.X + (gasVel.X * frameTime), transform.Y + (gasVel.Y * frameTime));
            }
        }
    }
}